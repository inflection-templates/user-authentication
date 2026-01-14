using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetService.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetService.Database.Seeds
{
    /// <summary>
    /// Tenant seeder that creates tenants.
    /// Similar to the Python tenant_seeder.py implementation.
    /// </summary>
    public class TenantSeeder : BaseSeeder
    {
        public TenantSeeder(DatabaseContext context, int batchSize = 1000, bool verbose = true)
            : base(context, batchSize, verbose)
        {
        }
        
        public override async Task<int> SeedAsync(int count = 1)
        {
            Console.WriteLine($"Seeding {count} tenant(s)...");
            
            // Check if tenants already exist
            var existingCount = await Context.Tenants.CountAsync();
            
            if (existingCount > 0)
            {
                Console.WriteLine($"[INFO] {existingCount} tenant(s) already exist, skipping seed");
                return 0;
            }
            
            var tenants = new List<Tenant>();
            
            // Create default tenant
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Default",
                Code = "default",
                IsActive = true,
                Status = "active",
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            tenants.Add(tenant);
            
            await BatchInsertAsync(tenants);
            Console.WriteLine($"Successfully seeded {CreatedCount} tenant(s)");
            return CreatedCount;
        }
    }
}

