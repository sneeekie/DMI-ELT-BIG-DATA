using System.Text.Json.Nodes;
using MongoDB.Bson;
using Transform.Entities;
using Transform.IRepository;

namespace Transform.Services
{
    public class DataTransformationService : BackgroundService
    {
        private readonly ILogger<DataTransformationService> _logger;
        private readonly RawDMIDataStorageService _rawDMIDataStorageService;
        private readonly IServiceProvider _serviceProvider;

        public DataTransformationService(ILogger<DataTransformationService> logger, RawDMIDataStorageService rawDMIDataStorageService, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rawDMIDataStorageService = rawDMIDataStorageService ?? throw new ArgumentNullException(nameof(rawDMIDataStorageService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            PeriodicTimer timer = new(TimeSpan.FromSeconds(10));
            while(await timer.WaitForNextTickAsync(stoppingToken)) 
            {
                _logger.LogInformation($"[Timer] Running {nameof(ExecuteAsync)}");
                await TransformAndSaveAsync(stoppingToken);
            }
        }

        private async Task TransformAndSaveAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (_rawDMIDataStorageService.Items.TryDequeue(out BsonDocument items) != true) 
                    return;

                using var scope = _serviceProvider.CreateAsyncScope();
                IWeatherRepository repo = scope.ServiceProvider.GetRequiredService<IWeatherRepository>();

                var elements = items.GetValue("features").AsBsonArray;
                for (int i = 0; i < elements.Count; i++)
                {
                    var element = elements[i];

                    if(element["geometry"].IsBsonNull)
                        continue;
                    
                    var geometry = element["geometry"];
                    var coordinates = geometry["coordinates"].AsBsonArray;

                    var properties = element["properties"];
                    var value = properties["value"];

                    var id = element["id"];
                    var created = properties["created"];
                    var latitude = coordinates[1];
                    var longitude = coordinates[0];
                    var observed = properties["observed"];
                    var parameterId = properties["parameterId"];
                    var stationId = properties["stationId"];

                    WeatherDataModel weatherDataModel = new()
                    {
                        Created = DateTime.Parse(created.ToString()),
                        Latitude = Double.Parse(latitude.ToString()),
                        Longitude = Double.Parse(longitude.ToString()),
                        Observed = DateTime.Parse(observed.ToString()),
                        ParameterId = parameterId.ToString(),
                        StationId = stationId.ToString(),
                        Value = Double.Parse(value.ToString()),
                        DmiID = Guid.Parse(id.ToString())
                    };

                    repo.Add(weatherDataModel);
                }

                await repo.Save(stoppingToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}