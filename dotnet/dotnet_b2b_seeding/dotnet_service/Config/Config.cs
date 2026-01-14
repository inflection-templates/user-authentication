using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace DotnetService.Config
{
    /// <summary>
    /// Configuration manager for loading and accessing environment variables.
    /// Similar to the Python config.py implementation.
    /// </summary>
    public static class Config
    {
        private static IConfiguration? _configuration;
        
        static Config()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            
            _configuration = builder.Build();
        }
        
        // Helper method to get config value (checks both flat and nested keys)
        private static string? GetConfigValue(string key, params string[] nestedKeys)
        {
            // First check environment variables (flat keys)
            var envValue = _configuration?[key];
            if (!string.IsNullOrEmpty(envValue))
                return envValue;
            
            // Then check nested keys from appsettings.json
            foreach (var nestedKey in nestedKeys)
            {
                var nestedValue = _configuration?[nestedKey];
                if (!string.IsNullOrEmpty(nestedValue))
                    return nestedValue;
            }
            
            return null;
        }
        
        // Service Configuration
        public static string ServiceName => GetConfigValue("SERVICE_NAME", "Service:SERVICE_NAME") ?? "service-skeleton";
        public static string NodeEnv => GetConfigValue("NODE_ENV", "Service:NODE_ENV") ?? "development";
        public static int Port => int.TryParse(GetConfigValue("PORT", "Service:PORT"), out var port) ? port : 3000;
        public static string BaseUrl => GetConfigValue("BASE_URL", "Service:BASE_URL") ?? "http://localhost:3000";
        public static string ServiceVersion => GetConfigValue("SERVICE_VERSION", "Service:SERVICE_VERSION") ?? "1.0.0";
        
        // Database Configuration
        public static string DbDialect => GetConfigValue("DB_DIALECT", "Database:DB_DIALECT") ?? "postgres";
        public static string DbHost => GetConfigValue("DB_HOST", "Database:DB_HOST") ?? "localhost";
        public static int DbPort => int.TryParse(GetConfigValue("DB_PORT", "Database:DB_PORT"), out var dbPort) ? dbPort : 5432;
        public static string DbName => GetConfigValue("DB_NAME", "Database:DB_NAME") ?? "service_skeleton";
        public static string DbUserName => GetConfigValue("DB_USER_NAME", "Database:DB_USER_NAME") ?? "postgres";
        public static string DbUserPassword => GetConfigValue("DB_USER_PASSWORD", "Database:DB_USER_PASSWORD") ?? "password";
        public static string BusinessMode => GetConfigValue("BUSINESS_MODE", "Service:BUSINESS_MODE") ?? "b2c";
        
        // Seeding Configuration (only from environment variables, like Python/Node.js)
        // Defaults to true - seeds automatically unless explicitly set to false
        public static bool AutoSeedOnStartup
        {
            get
            {
                var value = _configuration?["AUTO_SEED_ON_STARTUP"];
                if (string.IsNullOrEmpty(value))
                    return true; // Default to true - auto seed
                return bool.TryParse(value, out var autoSeed) && autoSeed;
            }
        }
        public static int SeedUserCount => int.TryParse(_configuration?["SEED_USER_COUNT"], out var count) ? count : 2;
        
        public static string GetConnectionString()
        {
            if (DbDialect == "postgres")
            {
                return $"Host={DbHost};Port={DbPort};Database={DbName};Username={DbUserName};Password={DbUserPassword}";
            }
            else if (DbDialect == "mysql")
            {
                return $"Server={DbHost};Port={DbPort};Database={DbName};User={DbUserName};Password={DbUserPassword};CharSet=utf8mb4";
            }
            else
            {
                return $"Server={DbHost};Database={DbName};User Id={DbUserName};Password={DbUserPassword};TrustServerCertificate=true";
            }
        }
        
        public static bool IsDevelopment() => NodeEnv == "development";
        public static bool IsProduction() => NodeEnv == "production";
        public static bool IsTest() => NodeEnv == "test";
    }
}

