using Microsoft.EntityFrameworkCore;

namespace shala.api.database.relational.efcore;

public class DatabaseContext: DbContext
{

    #region Constructors

    private readonly IWebHostEnvironment _environment;

    public DatabaseContext(IWebHostEnvironment environment)
    {
        _environment = environment;
        this.Database.EnsureCreated();
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.LazyLoadingEnabled = false;
    }

    public DatabaseContext(IWebHostEnvironment environment, DbContextOptions<DatabaseContext> options): base(options)
    {
        _environment = environment;
        this.Database.EnsureCreated();
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.LazyLoadingEnabled = false;
    }

    #endregion

    #region DbSets

    public DbSet<UserDbModel> Users { get; set; } = null!;
    public DbSet<RoleDbModel> Roles { get; set; } = null!;
    public DbSet<UserRoleDbModel> UserRoles { get; set; } = null!;
    public DbSet<OtpDbModel> Otps { get; set; } = null!;
    public DbSet<UserAuthProfileDbModel> UserAuthProfiles { get; set; } = null!;
    public DbSet<ClientAppDbModel> ClientApps { get; set; } = null!;
    public DbSet<ApiKeyDbModel> ApiKeys { get; set; } = null!;
    public DbSet<TenantDbModel> Tenants { get; set; } = null!;
    public DbSet<TenantSettingsDbModel> TenantSettings { get; set; } = null!;
    public DbSet<SessionDbModel> Sessions { get; set; } = null!;
    public DbSet<FileResourceDbModel> FileResources { get; set; } = null!;

    #endregion

    #region Protected Methods

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserDbModel>().ToTable("users");
        modelBuilder.Entity<RoleDbModel>().ToTable("roles");
        modelBuilder.Entity<UserRoleDbModel>().ToTable("user_roles");
        modelBuilder.Entity<OtpDbModel>().ToTable("otps");
        modelBuilder.Entity<UserAuthProfileDbModel>().ToTable("user_auth_profiles");
        modelBuilder.Entity<ClientAppDbModel>().ToTable("client_apps");
        modelBuilder.Entity<ApiKeyDbModel>().ToTable("api_keys");
        modelBuilder.Entity<TenantDbModel>().ToTable("tenants");
        modelBuilder.Entity<TenantSettingsDbModel>().ToTable("tenant_settings");
        modelBuilder.Entity<SessionDbModel>().ToTable("sessions");
        modelBuilder.Entity<FileResourceDbModel>().ToTable("file_resources");
    }

    #endregion

}
