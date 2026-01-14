using Hangfire;
using Hangfire.MemoryStorage;

namespace shala.api.startup.configurations;

public static class BuilderSchedulerExtensions
{
    public static WebApplicationBuilder AddScheduler(this WebApplicationBuilder builder)
    {
        builder.Services.AddHangfire(config =>
            config.UseMemoryStorage()
        );
        //Or
        // builder.AddSQLServerStorage();
        //Or
        // builder.AddHangfireMongoDbStorage();
        //Or
        // builder.AddHangfirePostgresStorage();
        //Or
        // builder.AddHangfireRedisStorage();

        builder.Services.AddHangfireServer();

        return builder;
    }

    // private static WebApplicationBuilder AddSQLServerStorage(this WebApplicationBuilder builder)
    // {
    //     var connectionString = builder.Configuration.GetValue<string>("Hangfire:Storage:ConnectionString");
    //     builder.Services.AddHangfire(config =>
    //         config
    //         .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    //         .UseSimpleAssemblyNameTypeSerializer()
    //         .UseRecommendedSerializerSettings()
    //         .UseSqlServerStorage(connectionString)
    //     );
    //     return builder;
    // }

    //Install Hangfire.Mongo nuget package
    //```dotnet add package Hangfire.Mongo```
    // using Hangfire.Mongo;
    // using Hangfire.Mongo.Migration.Strategies;
    // using Hangfire.Mongo.Migration.Strategies.Backup;
    // using MongoDB.Driver;
    // private static WebApplicationBuilder AddHangfireMongoDbStorage(this WebApplicationBuilder builder)
    // {

    //     var host = builder.Configuration.GetValue<string>("Hangfire:Storage:Host");
    //     var port = builder.Configuration.GetValue<int>("Hangfire:Storage:Port");
    //     var databaseName = builder.Configuration.GetValue<string>("Hangfire:Storage:Name");
    //     var user = builder.Configuration.GetValue<string>("Hangfire:Storage:User");
    //     var password = builder.Configuration.GetValue<string>("Hangfire:Storage:Password");

    //     // var connectionString = $"mongodb+srv://{user}:{password}@{host}:{port}/jobs";
    //     var connectionString = $"mongodb://{user}:{password}@{host}:{port}/jobs";
    //     var mongoUrlBuilder = new MongoUrlBuilder(connectionString);
    //     var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());

    //     // Add Hangfire services. Hangfire.AspNetCore nuget required
    //     builder.Services.AddHangfire(configuration => configuration
    //         .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    //         .UseSimpleAssemblyNameTypeSerializer()
    //         .UseRecommendedSerializerSettings()
    //         .UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, new MongoStorageOptions
    //         {
    //             MigrationOptions = new MongoMigrationOptions
    //             {
    //                 MigrationStrategy = new MigrateMongoMigrationStrategy(),
    //                 BackupStrategy = new CollectionMongoBackupStrategy()
    //             },
    //             Prefix = "hangfire.mongo",
    //             CheckConnection = true
    //         })
    //     );
    //     // Add the processing server as IHostedService
    //     builder.Services.AddHangfireServer(serverOptions =>
    //     {
    //         serverOptions.ServerName = "Hangfire.Mongo server 1";
    //     });

    //     return builder;
    // }

    //Install Hangfire.PostgreSql nuget package
    //```dotnet add package Hangfire.PostgreSql```
    // private static WebApplicationBuilder AddHangfirePostgresStorage(this WebApplicationBuilder builder)
    // {
    //     var connectionString = builder.Configuration.GetValue<string>("Hangfire:Storage:ConnectionString");
    //     builder.Services.AddHangfire(config =>
    //         config.UsePostgreSqlStorage(c =>
    //             c.UseNpgsqlConnection(connectionString))
    //     );
    //     return builder;
    // }

    // https://github.com/marcoCasamento/Hangfire.Redis.StackExchange for Redis storage
    // ```dotnet add package Hangfire.Redis.StackExchange```
    // private static WebApplicationBuilder AddHangfireRedisStorage(this WebApplicationBuilder builder)
    // {
    //     var connectionString = builder.Configuration.GetValue<string>("Hangfire:Storage:ConnectionString");
    //     builder.Services.AddHangfire(config =>
    //         config.UseRedisStorage(connectionString)
    //     );
    //     return builder;
    // }

}
