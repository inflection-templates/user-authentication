using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetService.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetService.Database.Seeds
{
    /// <summary>
    /// Role seeder that creates roles.
    /// Similar to the Python role_seeder.py implementation.
    /// </summary>
    public class RoleSeeder : BaseSeeder
    {
        public static readonly List<string> RoleOptions = new List<string> { "Tenant", "Organization", "User" };
        
        public RoleSeeder(DatabaseContext context, int batchSize = 1000, bool verbose = true)
            : base(context, batchSize, verbose)
        {
        }
        
        public override async Task<int> SeedAsync(int count = 3)
        {
            Console.WriteLine($"Seeding {count} roles...");
            
            // Get all existing users
            var users = await Context.Users.ToListAsync();
            
            if (users.Count == 0)
            {
                Console.WriteLine("[ERROR] No users found in database. Please seed users first.");
                return 0;
            }
            
            var roles = new List<Role>();
            
            // Create exactly one role of each type, assigned to different users
            for (int i = 0; i < RoleOptions.Count; i++)
            {
                var roleName = RoleOptions[i];
                var user = users[Math.Min(i, users.Count - 1)];
                
                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleName = roleName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                roles.Add(role);
            }
            
            await BatchInsertAsync(roles);
            Console.WriteLine($"Successfully seeded {CreatedCount} roles");
            return CreatedCount;
        }
    }
}

