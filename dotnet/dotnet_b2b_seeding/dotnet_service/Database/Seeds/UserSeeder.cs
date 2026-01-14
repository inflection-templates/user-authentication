using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotnetService.Database.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using BCrypt.Net;

namespace DotnetService.Database.Seeds
{
    /// <summary>
    /// User seeder that creates users, optionally loading from system.admin.seed.json.
    /// Similar to the Python user_seeder.py implementation.
    /// </summary>
    public class UserSeeder : BaseSeeder
    {
        public UserSeeder(DatabaseContext context, int batchSize = 1000, bool verbose = true)
            : base(context, batchSize, verbose)
        {
        }
        
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        
        private Dictionary<string, object>? LoadSeedData()
        {
            var seedPath = Path.Combine(Directory.GetCurrentDirectory(), "seed.data", "system.admin.seed.json");
            
            try
            {
                if (File.Exists(seedPath))
                {
                    var json = File.ReadAllText(seedPath);
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    
                    if (data != null)
                    {
                        var result = new Dictionary<string, object>
                        {
                            { "username", data.GetValueOrDefault("UserName", "admin") ?? "admin" },
                            { "firstName", data.GetValueOrDefault("FirstName", "System") ?? "System" },
                            { "lastName", "Admin" },
                            { "email", data.GetValueOrDefault("Email", "sys.admin@example.com") ?? "sys.admin@example.com" },
                            { "password", data.GetValueOrDefault("Password", "ChangeMe123!") ?? "ChangeMe123!" },
                            { "phoneCode", data.GetValueOrDefault("PhoneCode", "+91") ?? "+91" }
                        };
                        
                        if (data.TryGetValue("PhoneNumber", out var phoneNumber) && phoneNumber != null)
                        {
                            result["phoneNumber"] = phoneNumber;
                        }
                        
                        return result;
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
            Console.WriteLine($"Seeding {count} users...");
            
            // Get all tenants
            var tenants = await Context.Tenants.ToListAsync();
            
            if (tenants.Count > 0)
            {
                Console.WriteLine($"[INFO] Assigning users to {tenants.Count} tenant(s)");
            }
            else
            {
                Console.WriteLine("[WARN] No tenants found. Users will be created without tenantId.");
            }
            
            // Load seed data
            var seedData = LoadSeedData();
            
            var defaultPassword = HashPassword("password123");
            
            // Default values
            var usernames = new List<string> { "john.smith", "sarah.brown", "mike.johnson", "jane.williams", "david.davis" };
            var firstNames = new List<string> { "John", "Jane", "Mike", "Sarah", "David" };
            var lastNames = new List<string> { "Smith", "Johnson", "Williams", "Brown", "Davis" };
            var emailDomain = "example.com";
            
            // Override with seed data if available
            if (seedData != null)
            {
                if (seedData.ContainsKey("username"))
                {
                    usernames = new List<string> { seedData["username"].ToString()! };
                }
                if (seedData.ContainsKey("firstName"))
                {
                    firstNames = new List<string> { seedData["firstName"].ToString()! };
                }
                if (seedData.ContainsKey("email") && seedData["email"].ToString()!.Contains("@"))
                {
                    emailDomain = seedData["email"].ToString()!.Split('@')[1];
                }
            }
            
            var users = new List<User>();
            var random = new Random();
            
            for (int i = 0; i < count; i++)
            {
                User user;
                
                // Use seed data for first user if available
                if (i == 0 && seedData != null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = seedData.GetValueOrDefault("username", $"user{i}").ToString()!,
                        Email = seedData.GetValueOrDefault("email", $"user{i}@{emailDomain}").ToString()!,
                        Password = HashPassword(seedData.GetValueOrDefault("password", "ChangeMe123!").ToString()!),
                        FirstName = (seedData.GetValueOrDefault("firstName", GetRandomChoice(firstNames) ?? "System") ?? "System").ToString(),
                        LastName = seedData.GetValueOrDefault("lastName", "Admin").ToString(),
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    if (seedData.ContainsKey("phoneNumber") && seedData["phoneNumber"] != null)
                    {
                        user.PhoneNumber = seedData["phoneNumber"].ToString();
                    }
                    if (seedData.ContainsKey("phoneCode") && seedData["phoneCode"] != null)
                    {
                        user.CountryCode = seedData["phoneCode"].ToString();
                    }
                }
                else
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = i < usernames.Count ? GetRandomChoice(usernames)! : $"user{i}",
                        Email = $"user{i}@{emailDomain}",
                        Password = defaultPassword,
                        FirstName = GetRandomChoice(firstNames),
                        LastName = GetRandomChoice(lastNames),
                        IsActive = GetRandomBoolean(0.9),
                        IsEmailVerified = GetRandomBoolean(0.8),
                        IsPhoneVerified = GetRandomBoolean(0.5),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                }
                
                // Assign tenant if available
                if (tenants.Count > 0)
                {
                    var tenant = GetRandomChoice(tenants);
                    if (tenant != null)
                    {
                        user.TenantId = tenant.Id;
                    }
                }
                
                users.Add(user);
            }
            
            await BatchInsertAsync(users);
            Console.WriteLine($"Successfully seeded {CreatedCount} users");
            return CreatedCount;
        }
    }
}

