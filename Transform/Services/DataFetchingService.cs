using System.Net;
using System.Text.Json.Nodes;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Transform.Services;

public class DataFetchingService(ILogger<DataFetchingService> logger, IConfiguration configuration, RawDMIDataStorageService rawDmiDataStorageService) 
    : BackgroundService
{
    private readonly ILogger<DataFetchingService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    
    private readonly string _mongoDbConnection = configuration["ConnectionStrings:MongoDB"] ?? throw new ArgumentNullException(nameof(_mongoDbConnection));

    private readonly RawDMIDataStorageService _rawDmiDataStorageService = rawDmiDataStorageService ?? throw new ArgumentNullException(nameof(rawDmiDataStorageService));
    
    private DateTime _lastTimeStamp = DateTime.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Initial {nameof(ExecuteAsync)}");
        await SaveDataToQueueAsync(stoppingToken);

        PeriodicTimer timer = new(TimeSpan.FromSeconds(10));
        while(await timer.WaitForNextTickAsync(stoppingToken)) 
        {
            _logger.LogInformation($"[Timer] Running {nameof(ExecuteAsync)}");
            await SaveDataToQueueAsync(stoppingToken);
        }
    }

    private async Task SaveDataToQueueAsync(CancellationToken stoppingToken)
    {
        try
        {
            MongoClientSettings settings = MongoClientSettings.FromConnectionString(_mongoDbConnection);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            MongoClient mongoClient = new(settings);
            
            IMongoDatabase db = mongoClient.GetDatabase("weatherdata");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("raw");

            await (await collection.FindAsync(_ => true, cancellationToken: stoppingToken)).ForEachAsync(document => {

                try
                {
                    string? time = document["timeStamp"].AsString;
                    if(time is null)
                        return;
                    
                    if(DateTime.TryParse(time, out DateTime parsedTime) == false)
                        return;

                    if(parsedTime <= _lastTimeStamp)
                        return;

                    _lastTimeStamp = parsedTime;

                    _rawDmiDataStorageService.Items.Enqueue(document);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

            }, stoppingToken);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}