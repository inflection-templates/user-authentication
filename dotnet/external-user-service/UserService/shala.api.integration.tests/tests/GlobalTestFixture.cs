// Global setup and teardown
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace shala.api.integration.tests;

public class GlobalTestFixture : IDisposable
{
    public GlobalTestFixture()
    {
        // Example global setup (e.g., test database initialization)
        Console.WriteLine("Global setup executed before all tests.");
        dropExistingDatabase();
    }

    public void Dispose()
    {
        // Example global teardown (e.g., clean up resources)
        Console.WriteLine("Global teardown executed after all tests.");
    }

    private void dropExistingDatabase()
    {
                // Read configurations from appsettings.test.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json")
            .Build();

        // Read the database configs from appsettings.test.json
        var host = configuration.GetValue<string>("Database:Host");
        var port = configuration.GetValue<int>("Database:Port");
        var databaseName = configuration.GetValue<string>("Database:Name");
        var user = configuration.GetValue<string>("Database:User");
        var password = configuration.GetValue<string>("Database:Password");

        var connectionStringBuilder = new MySqlConnectionStringBuilder
        {
            Server = host,
            Port = (uint) port,
            UserID = user,
            Password = password,
            Database = databaseName
        };
        var connectionString = connectionStringBuilder.ConnectionString;

        // Delete the existing database through mysql conenctor
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE IF EXISTS {databaseName};";
        command.ExecuteNonQuery();
        connection.Close();

    }

}
