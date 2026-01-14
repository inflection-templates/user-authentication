using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetService.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetService.Database.Seeds
{
    /// <summary>
    /// Main database seeder that orchestrates seeding of all tables.
    /// Similar to the Python database_seeder.py implementation.
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly DatabaseContext _context;
        
        public DatabaseSeeder(DatabaseContext context)
        {
            _context = context;
        }
        
        public async Task<bool> CheckDatabaseConnectionAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                Console.WriteLine("[OK] Database connection successful");
                return true;
            }
            catch (Exception error)
            {
                Console.WriteLine($"[ERROR] Database connection failed: {error.Message}");
                return false;
            }
        }
        
        public async Task<int> SeedAllAsync(Dictionary<string, int> counts)
        {
            Console.WriteLine("[INFO] Using B2B mode");
            Console.WriteLine($"[INFO] Will create roles: {string.Join(", ", RoleSeeder.RoleOptions)}");
            
            int totalCreated = 0;
            
            // Seed tenants
            var tenantCount = counts.GetValueOrDefault("tenants", 1);
            Console.WriteLine("\n[SEED] Seeding tenants...");
            var tenantSeeder = new TenantSeeder(_context, batchSize: 100);
            var createdTenants = await tenantSeeder.SeedAsync(tenantCount);
            totalCreated += createdTenants;
            
            // Seed users
            var userCount = counts.GetValueOrDefault("users", 2);
            Console.WriteLine("\n[SEED] Seeding users...");
            var userSeeder = new UserSeeder(_context, batchSize: 100);
            var createdUsers = await userSeeder.SeedAsync(userCount);
            totalCreated += createdUsers;
            
            // Seed roles
            var roleCount = counts.GetValueOrDefault("roles", 3);
            Console.WriteLine("\n[SEED] Seeding roles...");
            var roleSeeder = new RoleSeeder(_context, batchSize: 100);
            var createdRoles = await roleSeeder.SeedAsync(roleCount);
            totalCreated += createdRoles;
            
            // Seed client apps
            var clientAppCount = counts.GetValueOrDefault("clientApps", 2);
            Console.WriteLine("\n[SEED] Seeding client apps...");
            var clientAppSeeder = new ClientAppSeeder(_context, batchSize: 100);
            var createdClientApps = await clientAppSeeder.SeedAsync(clientAppCount);
            totalCreated += createdClientApps;
            
            Console.WriteLine("\n[OK] Seeding completed!");
            return totalCreated;
        }
    }
}

