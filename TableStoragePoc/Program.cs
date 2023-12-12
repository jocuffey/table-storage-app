using Azure.Data.Tables;
using TableStoragePoc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddTransient<IAzureTableService, AzureTableService>();

var app = builder.Build();

var azureTableService = app.Services.GetRequiredService<IAzureTableService>();

var tableClient = new TableClient(app.Configuration["AZ_STORAGE_ACCOUNT"], 
    app.Configuration["TABLE_NAME"]);
    
await tableClient.CreateIfNotExistsAsync();

app.MapGet("/", async () =>
{
    var entities = await azureTableService.GetAllValues(tableClient);
    return entities;
});

app.MapGet("/{id}", (int id) =>
{
    var entities = azureTableService.GetValuesByTimeseries(tableClient, id);
    return entities;
});

app.Run();