namespace Transform.Entities;

public class WeatherDataModel
{
    public Guid Id { get; set; } // id
    public double Latitude { get; set; } // geometry.coordinates.1	
    public double Longitude { get; set; } // geometry.coordinates.0
    public DateTime Created { get; set; } // created
    public DateTime Observed { get; set; } // observed
    public string ParameterId { get; set; } // parameterId
    public string StationId { get; set; } //stationId
    public double Value { get; set; } // value
}
