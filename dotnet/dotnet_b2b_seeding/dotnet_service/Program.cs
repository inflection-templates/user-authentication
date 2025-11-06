using DotnetService.Database;
using DotnetService.Config;
using DotnetService.Startup.Configurations;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;

namespace DotnetService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Setup database
            DatabaseConnector.Setup();
            
            // Build web application
            var builder = WebApplication.CreateBuilder(args);
            
            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            // Register DbContext
            var connectionString = Config.Config.GetConnectionString();
            if (Config.Config.DbDialect == "postgres")
            {
                builder.Services.AddDbContext<DatabaseContext>(options =>
                    options.UseNpgsql(connectionString));
            }
            else if (Config.Config.DbDialect == "mysql")
            {
                builder.Services.AddDbContext<DatabaseContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            }
            else
            {
                builder.Services.AddDbContext<DatabaseContext>(options =>
                    options.UseSqlServer(connectionString));
            }
            
            var app = builder.Build();
            
            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            
            // Use seeder extension method (runs seeding after app is built)
            app.UseSeeder();
            
            await app.RunAsync();
        }
    }
}

