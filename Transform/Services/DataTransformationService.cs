using System.Text.Json.Nodes;
using Transform.Entities;
using Transform.IRepository;

namespace Transform.Services
{
    public class DataTransformationService : BackgroundService
    {
        private readonly RawDMIDataStorageService _rawDMIDataStorageService;
        private readonly IServiceProvider _serviceProvider;

        public DataTransformationService(RawDMIDataStorageService rawDMIDataStorageService, IServiceProvider serviceProvider)
        {
            _rawDMIDataStorageService = rawDMIDataStorageService ?? throw new ArgumentNullException(nameof(rawDMIDataStorageService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_rawDMIDataStorageService.Items.TryDequeue(out var items) != true) 
                return;

            using var scope = _serviceProvider.CreateAsyncScope();
            IWeatherRepository repo = scope.ServiceProvider.GetRequiredService<IWeatherRepository>();

            JsonNode? node = JsonNode.Parse(items);
            var date = node["timeStamp"];
            var type = node["type"];

            var elements = node["features"].AsArray();
            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];

                var geometry = element["geometry"].AsObject();
                var coordinates = geometry["coordinates"].AsArray();

                var properties = element["properties"].AsObject();
                var value = properties["value"].AsObject();

                var id = element["id"];
                var created = properties["created"];
                var latitude = coordinates[1]["$numberDouble"];
                var longitude = coordinates[0]["$numberDouble"];
                var observed = properties["observed"];
                var parameterId = properties["parameterId"];
                var stationId = properties["stationId"];
                var valueValue = value["$numberDouble"];

                WeatherDataModel weatherDataModel = new WeatherDataModel()
                {
                    Id = Guid.Parse(id.GetValue<string>()),
                    Created = DateTime.Parse(created.GetValue<string>()),
                    Latitude = Double.Parse(latitude.GetValue<string>()),
                    Longitude = Double.Parse(longitude.GetValue<string>()),
                    Observed = DateTime.Parse(observed.GetValue<string>()),
                    ParameterId = parameterId.GetValue<string>(),
                    StationId = stationId.GetValue<string>(),
                    Value = Double.Parse(valueValue.GetValue<string>())
                };

                repo.Add(weatherDataModel);
            }

            await repo.Save();
        }
    }
}