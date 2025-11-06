using Microsoft.EntityFrameworkCore;
using DotnetService.Database.Models;
using DotnetService.Config;
using Pomelo.EntityFrameworkCore.MySql;

namespace DotnetService.Database
{
    /// <summary>
    /// Entity Framework Core database context.
    /// Similar to the Python database_connector.py implementation.
    /// </summary>
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ClientApp> ClientApps { get; set; }
        
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
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
            }
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.OrganizationId);
                
                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.Users)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configure Tenant entity
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
            });
            
            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Roles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure ClientApp entity
            modelBuilder.Entity<ClientApp>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.OwnerUserId);
                entity.HasIndex(e => e.OrganizationId);
            });
        }
    }
}

