## TimeValueLog in Azure Table Storage

---

### Introduction

This project is a simple application that demonstrates how to use Azure Table Storage to store time value data.
The application uses the Azure Table Storage API to query and insert data into a table.
This is a starting point to begin exploring the structure of data entities and how to query them.

### Prerequisites

- Azure Subscription
- Azure Storage Account

### Setup

1. Create an Azure Storage Account
2. Add storage account connection string to appsettings.json
3. Add table name to appsettings.json
4. Run the application. If the table does not exist, it will be created
5. If empty, table will be seeded with timevalue data from seed.json

### Notes

- It is recommended to use Azure Storage Explorer to view table data

**Querying table data**

- We can use LINQ syntax to write queries against the table

```csharp 
// Query an entity by partition key
var entitiesById = from entity in tableClient.Query<MyEntity>()
            where entity.PartitionKey == "key"
            select entity;
```

```csharp
// Filter on date range
var entitiesByDate = from entity in tableClient.Query<MyEntity>()
            where entity.FromDate >= new DateTime(2023, 12, 10)
            && entity.ToDate <= new DateTime(2023, 12, 11)
            select entity;
```


