using Azure;
using Azure.Data.Tables;

namespace TableStoragePoc.Models;

public record TimeValueEntry : ITableEntity
{
    public ETag ETag { get; set; } = default!;
    public double Value { get; set; }
    public DateTime FromTime { get; set; }
    public DateTime ToTime { get; set; }
    public string? QualityFlag { get; set; }
    public string? QualityControlLevel { get; set; }
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset? Timestamp { get; set; } = default!;
}