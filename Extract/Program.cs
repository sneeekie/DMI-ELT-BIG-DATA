using Extract.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddHostedService<DataFetchingService>();

var app = builder.Build();

app.Run();