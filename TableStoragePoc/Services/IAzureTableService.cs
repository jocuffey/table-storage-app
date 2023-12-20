using System.Diagnostics;
using System.Text.Json;
using Azure.Data.Tables;
using TableStoragePoc.Extensions;
using TableStoragePoc.Models;

namespace TableStoragePoc.Services;

public interface IAzureTableService
{
    Task<IEnumerable<TimeValueEntry>> GetAllValues(TableClient tableClient);
    IEnumerable<TimeValueEntry> GetValuesByTimeseries(TableClient tableClient, int id);
    Task<TimeValueEntry> AddEntity(TableClient tableClient, TimeValue value);
    Task<long> BatchInsert(TableClient tableClient);
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
    
    public async Task<TimeValueEntry> AddEntity(TableClient tableClient, TimeValue value)
    {
        var entry = new TimeValueEntry
        {
            PartitionKey = value.TS_ID.ToString(),
            Value = double.Parse(value.TE_VALUE),
            FromTime = DateTimeExtensions.ParseExact(value.TE_FROMTIME, "yyyy-MM-dd HH:mm:ss"),
            ToTime = DateTimeExtensions.ParseExact(value.TE_TOTIME, "yyyy-MM-dd HH:mm:ss"),
            QualityFlag = value.QA_NAME,
            QualityControlLevel = value.QC_NAME,
            RowKey = value.AQTVL_GUID
        };
        
        await tableClient.AddEntityAsync(entry);
        return entry;
    } 
    
    public async Task<long> BatchInsert(TableClient tableClient)
    {
        var timeSeriesIds = new[] { "2066", "3171", "23", "3016", "2964", "2724", "43", "1689" };

        int numberOfConcurrentInserts = 10000;
        var tasks = new List<Task>();

        for (int i = 0; i < numberOfConcurrentInserts; i++)
        {
            string partitionKey = timeSeriesIds[new Random().Next(0, timeSeriesIds.Length)];

            var task = Task.Run(() => tableClient.AddEntityAsync(new TimeValueEntry
            {
                PartitionKey = partitionKey,
                RowKey = Guid.NewGuid().ToString(),
                Value = i,
                FromTime = DateTime.UtcNow,
                ToTime = DateTime.UtcNow,
                QualityFlag = "A",
                QualityControlLevel = "1"
            }));
            tasks.Add(task);
        }
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
    
    private async Task SeedInitialValues(TableClient tableClient)
    {
        var seed = await File.ReadAllTextAsync("DataFiles/seed.json");
        var values = JsonSerializer.Deserialize<TimeValue[]>(seed);
        var tasks = new List<Task>();

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
            var task = Task.Run(() => tableClient.AddEntityAsync(row));
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }
}