using Transform;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EfDbContext>();

var app = builder.Build();


await using var scope = app.Services.CreateAsyncScope();

var db = scope.ServiceProvider.GetRequiredService<EfDbContext>();
bool result = await db.Database.EnsureCreatedAsync();
if(result != true)
    throw new ArgumentException("Unable to verify that the database is created.");

app.Run();