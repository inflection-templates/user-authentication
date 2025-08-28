using Microsoft.EntityFrameworkCore;
using AspNetJwtAuth.Models;

namespace AspNetJwtAuth.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure Permission entity
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure UserRole relationships
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
                entity.Property(e => e.AssignedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure RolePermission relationships
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
                entity.Property(e => e.AssignedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "admin", Description = "Administrator role with full access", CreatedAt = DateTime.UtcNow },
                new Role { Id = 2, Name = "moderator", Description = "Moderator role with limited admin access", CreatedAt = DateTime.UtcNow },
                new Role { Id = 3, Name = "user", Description = "Standard user role", CreatedAt = DateTime.UtcNow }
            );

            // Seed Permissions
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Name = "read:posts", Description = "Read posts", Category = "posts", CreatedAt = DateTime.UtcNow },
                new Permission { Id = 2, Name = "write:posts", Description = "Create and edit posts", Category = "posts", CreatedAt = DateTime.UtcNow },
                new Permission { Id = 3, Name = "delete:posts", Description = "Delete posts", Category = "posts", CreatedAt = DateTime.UtcNow },
                new Permission { Id = 4, Name = "manage:users", Description = "Manage user accounts", Category = "users", CreatedAt = DateTime.UtcNow },
                new Permission { Id = 5, Name = "view:analytics", Description = "View analytics and reports", Category = "analytics", CreatedAt = DateTime.UtcNow }
            );

            // Seed Role-Permission mappings
            modelBuilder.Entity<RolePermission>().HasData(
                // Admin has all permissions
                new RolePermission { Id = 1, RoleId = 1, PermissionId = 1, AssignedAt = DateTime.UtcNow },
                new RolePermission { Id = 2, RoleId = 1, PermissionId = 2, AssignedAt = DateTime.UtcNow },
                new RolePermission { Id = 3, RoleId = 1, PermissionId = 3, AssignedAt = DateTime.UtcNow },
                new RolePermission { Id = 4, RoleId = 1, PermissionId = 4, AssignedAt = DateTime.UtcNow },
                new RolePermission { Id = 5, RoleId = 1, PermissionId = 5, AssignedAt = DateTime.UtcNow },

                // Moderator has limited permissions
                new RolePermission { Id = 6, RoleId = 2, PermissionId = 1, AssignedAt = DateTime.UtcNow },
                new RolePermission { Id = 7, RoleId = 2, PermissionId = 2, AssignedAt = DateTime.UtcNow },
                new RolePermission { Id = 8, RoleId = 2, PermissionId = 3, AssignedAt = DateTime.UtcNow },

                // User has basic permissions
                new RolePermission { Id = 9, RoleId = 3, PermissionId = 1, AssignedAt = DateTime.UtcNow },
                new RolePermission { Id = 10, RoleId = 3, PermissionId = 2, AssignedAt = DateTime.UtcNow }
            );

            // Seed default users with hashed passwords
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
            
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    Username = "admin", 
                    Email = "admin@example.com", 
                    PasswordHash = passwordHash,
                    FirstName = "Admin",
                    LastName = "User",
                    CreatedAt = DateTime.UtcNow 
                },
                new User 
                { 
                    Id = 2, 
                    Username = "moderator", 
                    Email = "moderator@example.com", 
                    PasswordHash = passwordHash,
                    FirstName = "Moderator",
                    LastName = "User",
                    CreatedAt = DateTime.UtcNow 
                },
                new User 
                { 
                    Id = 3, 
                    Username = "user123", 
                    Email = "user@example.com", 
                    PasswordHash = passwordHash,
                    FirstName = "Regular",
                    LastName = "User",
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // Seed User-Role mappings
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, UserId = 1, RoleId = 1, AssignedAt = DateTime.UtcNow }, // admin -> admin role
                new UserRole { Id = 2, UserId = 2, RoleId = 2, AssignedAt = DateTime.UtcNow }, // moderator -> moderator role
                new UserRole { Id = 3, UserId = 3, RoleId = 3, AssignedAt = DateTime.UtcNow }  // user123 -> user role
            );
        }
    }
}
