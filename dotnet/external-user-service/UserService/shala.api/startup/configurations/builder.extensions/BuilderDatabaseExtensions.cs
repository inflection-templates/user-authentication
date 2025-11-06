/////////////////////////////////////////////////////////////////////////////////
// PLEASE NOTE THAT - These imports will change based on your database settings
using shala.api.database.relational.efcore;
using shala.api.database.nosql.mongodb;
/////////////////////////////////////////////////////////////////////////////////

namespace shala.api.startup.configurations;

public static class BuilderDatabaseExtensions
{

    public static WebApplicationBuilder SetupDatabases(this WebApplicationBuilder builder)
    {
        var databaseType = builder.Configuration.GetValue<string>("Database:Type");
        if (string.IsNullOrEmpty(databaseType))
        {
            throw new ArgumentNullException("Database type is missing in appsettings.json");
        }
        if (databaseType == "Relational")
        {
            builder.SetupRelationalDatabase();
        }
        else if (databaseType == "NoSQL")
        {

            builder.SetupNoSQLDatabase();
        }
        else
        {
            throw new ArgumentException("Invalid database type in appsettings.json");
        }

        return builder;
    }

}
