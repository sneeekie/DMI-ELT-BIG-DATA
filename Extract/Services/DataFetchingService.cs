 
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
        PeriodicTimer timer = new(TimeSpan.FromMinutes(10));
        while(await timer.WaitForNextTickAsync(stoppingToken)) 
        {
            _logger.LogInformation($"Running {nameof(ExecuteAsync)}");

            HttpClient client = _httpClientFactory.CreateClient();
            HttpResponseMessage data = await client.GetAsync($"https://dmigw.govcloud.dk/v2/metObs/collections/observation/items?api-key={_apiKey}", stoppingToken);

            string content = await data.Content.ReadAsStringAsync(stoppingToken);

            MongoClientSettings settings = MongoClientSettings.FromConnectionString(_mongoDbConnection);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            MongoClient mongoClient = new(settings);
            
            IMongoDatabase db = mongoClient.GetDatabase("weatherdata");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("raw");
            BsonDocument document = BsonSerializer.Deserialize<BsonDocument>(content);
            await collection.InsertOneAsync(document, cancellationToken: stoppingToken);
        }
    }
}