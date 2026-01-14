using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotnetService.Database.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DotnetService.Database.Seeds
{
    /// <summary>
    /// Client app seeder that creates client applications, optionally loading from internal.clients.seed.json.
    /// Similar to the Python client_app_seeder.py implementation.
    /// </summary>
    public class ClientAppSeeder : BaseSeeder
    {
        public ClientAppSeeder(DatabaseContext context, int batchSize = 1000, bool verbose = true)
            : base(context, batchSize, verbose)
        {
        }
        
        private List<Dictionary<string, object>>? LoadSeedData()
        {
            var seedPath = Path.Combine(Directory.GetCurrentDirectory(), "seed.data", "internal.clients.seed.json");
            
            try
            {
                if (File.Exists(seedPath))
                {
                    var json = File.ReadAllText(seedPath);
                    var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
                    
                    if (data != null)
                    {
                        var appsFromFile = new List<Dictionary<string, object>>();
                        foreach (var x in data)
                        {
                            var clientCode = x.GetValueOrDefault("ClientCode")?.ToString() 
                                ?? x.GetValueOrDefault("ClientName", "client-app")?.ToString() 
                                ?? "client-app";
                            
                            var app = new Dictionary<string, object>
                            {
                                { "name", x.GetValueOrDefault("ClientName", "Client App")?.ToString() ?? "Client App" },
                                { "code", clientCode.Trim().ToLower() },
                                { "description", x.GetValueOrDefault("Description", "Client application")?.ToString() ?? "Client application" },
                                { "isVerified", true },
                                { "isActive", true },
                                { "status", "active" }
                            };
                            
                            if (x.TryGetValue("ApiKey", out var apiKey) && apiKey != null)
                            {
                                app["apiKey"] = apiKey;
                            }
                            
                            appsFromFile.Add(app);
                        }
                        return appsFromFile;
                    }
                }
            }
            catch (Exception e)
            {
                if (Verbose)
                {
                    Console.WriteLine($"[WARN] Could not load seed data: {e.Message}");
                }
            }
            
            return null;
        }
        
        public override async Task<int> SeedAsync(int count = 2)
        {
            Console.WriteLine($"Seeding {count} client app(s)...");
            
            // Get users for owner
            var users = await Context.Users.ToListAsync();
            
            if (users.Count == 0)
            {
                Console.WriteLine("[ERROR] No users found. Seed users before client apps.");
                return 0;
            }
            
            var owner = users[0];
            
            // Load seed data
            var appsFromFile = LoadSeedData();
            
            // Default client apps
            var defaults = appsFromFile ?? new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "name", "Default" },
                    { "code", "default" },
                    { "description", "Primary web client application" },
                    { "isVerified", true },
                    { "isActive", true },
                    { "status", "active" }
                }
            };
            
            // Ensure ownerUserId is set on each app
            var normalized = new List<ClientApp>();
            foreach (var app in defaults)
            {
                var clientApp = new ClientApp
                {
                    Id = Guid.NewGuid(),
                    Name = app.GetValueOrDefault("name", "Default").ToString()!,
                    Code = app.GetValueOrDefault("code", "default").ToString()!.ToLower(),
                    Description = app.GetValueOrDefault("description", "Client application").ToString()!,
                    ApiKey = app.GetValueOrDefault("apiKey")?.ToString(),
                    OwnerUserId = owner.Id,
                    IsVerified = (bool)(app.GetValueOrDefault("isVerified", true) ?? true),
                    IsActive = (bool)(app.GetValueOrDefault("isActive", true) ?? true),
                    Status = app.GetValueOrDefault("status", "active").ToString()!,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                normalized.Add(clientApp);
            }
            
            // Limit to requested count
            var toCreate = normalized.Take(Math.Max(1, Math.Min(count, normalized.Count))).ToList();
            
            await BatchInsertAsync(toCreate);
            Console.WriteLine($"Successfully seeded {CreatedCount} client app(s)");
            return CreatedCount;
        }
    }
}

