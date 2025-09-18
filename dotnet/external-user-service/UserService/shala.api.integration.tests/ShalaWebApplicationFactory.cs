using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace shala.api.integration.tests;

internal class ShalaWebApplicationFactory : WebApplicationFactory<Program>
{

    public IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureTestServices(services =>
        {
            // NOTE: This is another way in case you need some customization of the 'Test Services'
            // This way you can override database context initialization
            //configureTestServices(services);
        });
    }

    // private void configureTestServices(IServiceCollection services)
    // {
    //     // Remove the app's DatabaseContext registration
    //     var descriptor = services.SingleOrDefault(
    //         d => d.ServiceType == typeof(DbContextOptions<DatabaseContext>));
    //     if (descriptor != null)
    //     {
    //         services.Remove(descriptor);
    //     }

    //     //Read configurations from appsettings.test.json
    //     var configuration = new ConfigurationBuilder()
    //         .SetBasePath(Directory.GetCurrentDirectory())
    //         .AddJsonFile("appsettings.test.json")
    //         .Build();

    //     // // Read the database configs from appsettings.test.json
    //     var host = configuration.GetValue<string>("Database:Host");
    //     var port = configuration.GetValue<int>("Database:Port");
    //     var databaseName = configuration.GetValue<string>("Database:Name");
    //     var user = configuration.GetValue<string>("Database:User");
    //     var password = configuration.GetValue<string>("Database:Password");

    //     ServiceProvider = services.BuildServiceProvider();

    //     var connectionStringBuilder = new MySqlConnectionStringBuilder
    //     {
    //         Server = host,
    //         Port = (uint)port,
    //         UserID = user,
    //         Password = password,
    //         Database = databaseName
    //     };
    //     var connectionString = connectionStringBuilder.ConnectionString;

    //     services.AddDbContext<DatabaseContext>(options =>
    //     {
    //         options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    //     });

    //     // Build the service provider for seeding and services like OpenTelemetry
    //     var sp = services.BuildServiceProvider();

    //     using (var scope = sp.CreateScope())
    //     {
    //         var scopedServices = scope.ServiceProvider;
    //         var db = scopedServices.GetRequiredService<DatabaseContext>();

    //         db.Database.EnsureDeleted();

    //         //Wait for the database to be deleted
    //         System.Threading.Thread.Sleep(2000);

    //         // Ensure the test database schema is created
    //         db.Database.EnsureCreated();
    //     }
    // }

}
