using MongoDB.Bson;
using MongoDB.Driver;

namespace Transform.Services;

public class DataFetchingService(ILogger<DataFetchingService> logger, IConfiguration configuration, RawDMIDataStorageService rawDmiDataStorageService) 
    : BackgroundService
{
    private readonly ILogger<DataFetchingService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    
    private readonly string _mongoDbConnection = configuration["ConnectionStrings:MongoDB"] ?? throw new ArgumentNullException(nameof(_mongoDbConnection));

    private readonly RawDMIDataStorageService _rawDmiDataStorageService = rawDmiDataStorageService ?? throw new ArgumentNullException(nameof(rawDmiDataStorageService));
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //PeriodicTimer timer = new(TimeSpan.FromMinutes(10));
        //while(await timer.WaitForNextTickAsync(stoppingToken)) 
        {
            _logger.LogInformation($"Running {nameof(ExecuteAsync)}");
            
            MongoClientSettings settings = MongoClientSettings.FromConnectionString(_mongoDbConnection);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            MongoClient mongoClient = new(settings);
            
            IMongoDatabase db = mongoClient.GetDatabase("weatherdata");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("raw");

            await (await collection.FindAsync(_ => true, cancellationToken: stoppingToken)).ForEachAsync(e => {
                var data = e.ToJson();
                
                _rawDmiDataStorageService.Items.Enqueue(data);
            }, stoppingToken);
        }
    }
}