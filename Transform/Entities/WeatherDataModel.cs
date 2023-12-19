namespace Transform.Entities;

public class WeatherDataModel
{
    public Guid Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Created { get; set; }
    public DateTime Observed { get; set; }
    public string ParameterId { get; set; }
    public string StationId { get; set; }
    public double Value { get; set; }
}
