namespace shala.api.database.nosql.mongodb;

public static class DatabaseConfigurations
{
    public static WebApplicationBuilder SetupNoSQLDatabase(this WebApplicationBuilder builder)
    {
        var flavor = builder.Configuration.GetValue<string>("Database:Flavor");
        if (string.IsNullOrEmpty(flavor) || flavor != "MongoDB")
        {
            throw new ArgumentNullException("Database flavor is missing in appsettings.json");
        }

        var host = builder.Configuration.GetValue<string>("Database:Host");
        var port = builder.Configuration.GetValue<int>("Database:Port");
        var databaseName = builder.Configuration.GetValue<string>("Database:Name");
        var user = builder.Configuration.GetValue<string>("Database:User");
        var password = builder.Configuration.GetValue<string>("Database:Password");

        var connectionString = builder.Configuration.GetValue<string>("Database:ConnectionString");

        if (string.IsNullOrEmpty(connectionString) ||
            (string.IsNullOrEmpty(host) && string.IsNullOrEmpty(databaseName)))
        {
            throw new ArgumentNullException("Database configuration is missing in appsettings.json");
        }
        if (flavor == "MongoDB")
        {
            IDatabaseInitializer initializer = new MongoDbInitializer();
            initializer.Init(builder);
            return builder;
        }
        // else if (flavor == "Cassandra")
        // {
        //     //Cassandra connection string
        //     connectionString = connectionString ?? $"ContactPoint={host};Port={port};UserName={user};Password={password};";
        // }
        // else if (flavor == "CosmosDB")
        // {
        //     //CosmosDB connection string
        //     connectionString = connectionString ?? $"AccountEndpoint={host};AccountKey={password};";
        // }
        // else if (flavor == "DynamoDB")
        // {
        //     //DynamoDB connection string
        //     connectionString = connectionString ?? $"ServiceURL={host};";
        // }
        // else if (flavor == "Redis")
        // {
        //     //Redis connection string
        //     connectionString = connectionString ?? $"{host}:{port},password={password}";
        // }

        return builder;
    }

}
