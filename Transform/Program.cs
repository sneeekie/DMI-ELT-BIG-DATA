using Transform;
using Transform.IRepository;
using Transform.Repository;
using Transform.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EfDbContext>();
builder.Services.AddSingleton<RawDMIDataStorageService>();
builder.Services.AddHostedService<DataTransformationService>();
builder.Services.AddTransient<IWeatherRepository, WeatherRepository>();

var app = builder.Build();


await using var scope = app.Services.CreateAsyncScope();

var db = scope.ServiceProvider.GetRequiredService<EfDbContext>();
bool result = await db.Database.EnsureCreatedAsync();
if (result != true)
    throw new ArgumentException("Unable to verify that the database is created.");

app.Run();