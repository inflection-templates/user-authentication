using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace shala.api.database.relational.efcore;

public static class DatabaseConfigurations
{

    public static WebApplicationBuilder SetupRelationalDatabase(this WebApplicationBuilder builder)
    {

        var flavor = builder.Configuration.GetValue<string>("Database:Flavor");
        if (string.IsNullOrEmpty(flavor))
        {
            throw new ArgumentNullException("Database flavor is missing in appsettings.json");
        }

        var host = builder.Configuration.GetValue<string>("Database:Host");
        var port = builder.Configuration.GetValue<int>("Database:Port");
        var databaseName = builder.Configuration.GetValue<string>("Database:Name");
        var user = builder.Configuration.GetValue<string>("Database:User");
        var password = builder.Configuration.GetValue<string>("Database:Password");
        var connectionString = builder.Configuration.GetValue<string>("Database:ConnectionString");

        if (string.IsNullOrEmpty(connectionString) &&
            string.IsNullOrEmpty(host) &&
            string.IsNullOrEmpty(databaseName))
        {
            throw new ArgumentNullException("Database configuration is missing in appsettings.json");
        }

        if (flavor == "Postgres")
        {
            //Postgres connection string
            connectionString = connectionString ?? $"Host={host};Port={port};Database={databaseName};";
            if (!string.IsNullOrEmpty(user))
            {
                connectionString += $"UserName={user};";
            }
            if (!string.IsNullOrEmpty(password))
            {
                connectionString += $"Password={password};";
            }
        }
        else if (flavor == "MySQL")
        {
            if (!string.IsNullOrEmpty(host))
            {
                var connectionStringBuilder = new MySqlConnectionStringBuilder
                {
                    Server = host,
                    Port = (uint) port,
                    UserID = user,
                    Password = password,
                    Database = databaseName
                };
                connectionString = connectionStringBuilder.ConnectionString;
            }
        }
        else if (flavor == "SQLServer")
        {
            //SQL Server connection string
            connectionString = connectionString ?? $"Server={host},{port};Database={databaseName};";
            if (!string.IsNullOrEmpty(user))
            {
                connectionString += $"User Id={user};";
            }
            if (!string.IsNullOrEmpty(password))
            {
                connectionString += $"Password={password};";
            }
        }
        else if (flavor == "SQLite")
        {
            //SQLite connection string
            connectionString = $"Data Source={databaseName}.db";
        }
        else if (flavor == "InMemory")
        {
            //In-memory database connection string
            connectionString = "DataSource=:memory:";
        }
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException("Database connection string is missing in appsettings.json");
        }

        builder.Services.AddDbContext<DatabaseContext>(options =>
        {
            if (flavor == "Postgres")
            {
                options.UseNpgsql(connectionString);
            }
            else if (flavor == "MySQL")
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
            else if (flavor == "SQLServer")
            {
                options.UseSqlServer(connectionString);
            }
            else if (flavor == "SQLite")
            {
                options.UseSqlite(connectionString);
            }
            else if (flavor == "InMemory")
            {
                if (string.IsNullOrEmpty(databaseName))
                {
                    databaseName = "InMemoryDb";
                }
                options.UseInMemoryDatabase(databaseName);
            }
        });

        IDatabaseInitializer databaseInjector = new EFCoreDatabaseInitializer();
        databaseInjector.Init(builder);

        return builder;
    }

}
