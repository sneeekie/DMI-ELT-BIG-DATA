using Extract.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddHostedService<DataFetchingService>();
builder.Services.AddSingleton<RawDMIDataStorageService>();

var app = builder.Build();

app.Run();