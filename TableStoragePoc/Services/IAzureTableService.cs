using System.Text.Json;
using Azure.Data.Tables;
using TableStoragePoc.Extensions;
using TableStoragePoc.Models;

namespace TableStoragePoc.Services;

public interface IAzureTableService
{
    Task<IEnumerable<TimeValueEntry>> GetAllValues(TableClient tableClient);
    IEnumerable<TimeValueEntry> GetValuesByTimeseries(TableClient tableClient, int id);
}

public class AzureTableService : IAzureTableService
{
    public async Task<IEnumerable<TimeValueEntry>> GetAllValues(TableClient tableClient)
    {
        var entities = tableClient.Query<TimeValueEntry>();
        
        if (!entities.Any())
        {
            await SeedInitialValues(tableClient);
            entities = tableClient.Query<TimeValueEntry>();
        }
        
        return entities;
    }

    public IEnumerable<TimeValueEntry> GetValuesByTimeseries(TableClient tableClient, int id)
    {
        var timeSeriesIds = new[] { "2066", "3171", "23", "3016", "2964", "2724", "43", "1689" };
        
        
        var entities = from entity in tableClient.Query<TimeValueEntry>()
            where timeSeriesIds.Contains(entity.PartitionKey)
            select entity;

        var entitiesById = from entity in tableClient.Query<TimeValueEntry>()
            where entity.PartitionKey == id.ToString()
            select entity;
        
        if (entitiesById.Any())
        {
            return entitiesById;
        }
        
        return entities;
    }

    private async Task SeedInitialValues(TableClient tableClient)
    {
        var seed = await File.ReadAllTextAsync("DataFiles/seed.json");
        var values = JsonSerializer.Deserialize<TimeValue[]>(seed);

        var logEntries = values.Select(le => new TimeValueEntry
        {
            PartitionKey = le.TS_ID.ToString(),
            Value = double.Parse(le.TE_VALUE),
            FromTime = DateTimeExtensions.ParseExact(le.TE_FROMTIME, "yyyy-MM-dd HH:mm:ss"),
            ToTime = DateTimeExtensions.ParseExact(le.TE_TOTIME, "yyyy-MM-dd HH:mm:ss"),
            QualityFlag = le.QA_NAME,
            QualityControlLevel = le.QC_NAME,
            RowKey = le.AQTVL_GUID
        });

        foreach (var row in logEntries)
        {
            await tableClient.AddEntityAsync(row);
        }
    }
}