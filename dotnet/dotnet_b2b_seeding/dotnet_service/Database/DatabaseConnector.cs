using Microsoft.EntityFrameworkCore;
using DotnetService.Config;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql;

namespace DotnetService.Database
{
    /// <summary>
    /// Database connector for initializing and managing database connections.
    /// Similar to the Python database_connector.py implementation.
    /// </summary>
    public static class DatabaseConnector
    {
        private static DbContextOptions<DatabaseContext>? _options;
        
        private static DbContextOptions<DatabaseContext> GetOptions()
        {
            if (_options == null)
            {
                var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
                var connectionString = Config.Config.GetConnectionString();
                
                if (Config.Config.DbDialect == "postgres")
                {
                    optionsBuilder.UseNpgsql(connectionString);
                }
                else if (Config.Config.DbDialect == "mysql")
                {
                    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                }
                else
                {
                    optionsBuilder.UseSqlServer(connectionString);
                }
                
                _options = optionsBuilder.Options;
            }
            
            return _options;
        }
        
        public static DatabaseContext GetContext()
        {
            // Create a new context instance each time to avoid disposal issues
            return new DatabaseContext(GetOptions());
        }
        
        public static bool CreateDatabase()
        {
            try
            {
                using var context = GetContext();
                var created = context.Database.EnsureCreated();
                
                if (created)
                {
                    Console.WriteLine($"[OK] Database {Config.Config.DbName} created successfully");
                }
                else
                {
                    Console.WriteLine($"[INFO] Database {Config.Config.DbName} already exists");
                }
                
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[WARN] Could not create database (may already exist): {e.Message}");
                return true; // Continue even if creation fails
            }
        }
        
        public static bool Initialize()
        {
            try
            {
                Console.WriteLine($"[INFO] Environment: {Config.Config.NodeEnv}");
                Console.WriteLine($"[INFO] Database Name: {Config.Config.DbName}");
                Console.WriteLine($"[INFO] Database Username: {Config.Config.DbUserName}");
                Console.WriteLine($"[INFO] Database Host: {Config.Config.DbHost}");
                
                // Create database if it doesn't exist
                CreateDatabase();
                
                // Ensure tables are created
                using var context = GetContext();
                Console.WriteLine("[INFO] Creating database tables...");
                context.Database.EnsureCreated();
                Console.WriteLine("[OK] Database tables created successfully");
                
                Console.WriteLine("🔄 Database connection has been established successfully.");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"❌ Unable to connect to the database: {e.Message}");
                return false;
            }
        }
        
        public static void Setup()
        {
            Console.WriteLine("🛢️ Setting up the database...");
            Initialize();
        }
        
        public static void Close()
        {
            // No static context to dispose - each context is created and disposed individually
            Console.WriteLine("🔄 Database connection has been closed successfully.");
        }
    }
}

