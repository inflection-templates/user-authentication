namespace shala.api.startup.configurations;

public static class AppSeederExtensions
{
    public static WebApplication UseSeeder(this WebApplication app)
    {
        //Get service provider from app
        var serviceProvider = app.Services;

        var isDevEnvironment = app.Environment.IsDevelopment();
        var isTestEnvironment = app.Environment.IsEnvironment("Test");

        Seeder.SeedAsync(serviceProvider, isDevEnvironment, isTestEnvironment).Wait();
        return app;
    }
}
