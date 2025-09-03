using Humanizer;
using MongoDB.Driver;
using Serilog;

namespace shala.api.database.nosql.mongodb;

public class MongoDbInitializer : IDatabaseInitializer
{
    public MongoDbInitializer()
    {
    }

    public void Init(WebApplicationBuilder builder)
    {
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

        //MongoDB connection string for srv connections like Atlas
        // connectionString = connectionString ?? $"mongodb+srv://{user}:{password}@{host}:{port}/{databaseName}";

        //MongoDB connection string for normal connection
        connectionString = connectionString ?? $"mongodb://{user}:{password}@{host}:{port}/{databaseName}";

        IDatabaseInjector injector = new MongoDbInjector();
        injector.Register(builder);

        var client = new MongoClient(connectionString);
        var db = (MongoDatabaseBase)client.GetDatabase(databaseName);

        CreateCollections(db);

    }

    private static void CreateCollections(MongoDatabaseBase db)
    {
        // CreateCollection<UserDbModel>(db);
    }

    private static void CreateCollection<T>(MongoDatabaseBase db)
    {
        var collectionName = GetCollectionName<T>();
        try {
            db.CreateCollection(collectionName);
        }
        catch (MongoCommandException mongoExp)
        {
            Log.Error(mongoExp.Message);
        }
        catch (Exception exp)
        {
            Log.Error(exp.Message);
        }
    }

    private static string GetCollectionName<T>()
    {
        var typeName = typeof(T).Name;

        typeName = typeName.Replace("DbModel", "");

        var temp = "";
        for (var i = 0; i < typeName.Length; i++)
        {
            char x = typeName[i];
            if (char.IsUpper(x) && i != 0)
            {
                temp += " ";
            }
            temp += x;
        }
        temp = temp.ToLower();

        var pluralized = "";
        var tokens = temp.Split(' ');
        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            if (i == tokens.Length - 1)
            {
                pluralized += token.Pluralize();
            }
            else
            {
                pluralized += token + '_';
            }
        }
        return pluralized;
    }

}

