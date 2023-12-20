 
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Extract.Services;

public class DataFetchingService(ILogger<DataFetchingService> logger, IConfiguration configuration, IHttpClientFactory clientFactory) 
    : BackgroundService
{
    private readonly ILogger<DataFetchingService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IHttpClientFactory _httpClientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));

    private readonly string _apiKey = configuration["ApiKeys:DMI"] ?? throw new ArgumentNullException(nameof(_apiKey));

    private readonly string _mongoDbConnection = configuration["ConnectionStrings:MongoDB"] ?? throw new ArgumentNullException(nameof(_mongoDbConnection));
    
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial run
        _logger.LogInformation($"[Initial] Running {nameof(ExecuteAsync)}");
        string? data = await FetchData(stoppingToken);
        if(data is not null)
            await SaveData(data, stoppingToken);

        PeriodicTimer timer = new(TimeSpan.FromMinutes(10));
        while(await timer.WaitForNextTickAsync(stoppingToken)) 
        {
            _logger.LogInformation($"[Timer] Running {nameof(ExecuteAsync)}");

            data = await FetchData(stoppingToken);
            if(data is not null)
                await SaveData(data, stoppingToken);
        }
    }

    private async Task<string?> FetchData(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogDebug("Fetching information from DMI");
            HttpClient client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.GetAsync($"https://dmigw.govcloud.dk/v2/metObs/collections/observation/items?api-key={_apiKey}", stoppingToken);
            if(response.IsSuccessStatusCode == false)
            {
                _logger.LogError("Failed fetching data from DMI");
                return null;
            }

            string content = await response.Content.ReadAsStringAsync(stoppingToken);
            _logger.LogDebug("Finished loading data from DMI");
            return content;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        return null;
    }

    private async Task SaveData(string data, CancellationToken stoppingToken)
    {
        MongoClientSettings settings = MongoClientSettings.FromConnectionString(_mongoDbConnection);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);

        try 
        {
            MongoClient mongoClient = new(settings);
            
            IMongoDatabase db = mongoClient.GetDatabase("weatherdata");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("raw");

            BsonDocument document = BsonSerializer.Deserialize<BsonDocument>(data);
            await collection.InsertOneAsync(document, cancellationToken: stoppingToken);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}