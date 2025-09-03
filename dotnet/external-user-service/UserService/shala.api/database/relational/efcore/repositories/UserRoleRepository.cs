using Microsoft.EntityFrameworkCore;
using shala.api.database.interfaces;
using shala.api.database.mappers;
using shala.api.domain.types;

namespace shala.api.database.relational.efcore;

public class UserRoleRepository : IUserRoleRepository
{
    #region Construction

    public IConfiguration Configuration { get; }
    private DatabaseContext Context { get; set; }
    private readonly ILogger<UserRoleRepository> _logger;

    public UserRoleRepository(IConfiguration configuration, DatabaseContext context, ILogger<UserRoleRepository> logger)
    {
        Configuration = configuration;
        Context = context;
        _logger = logger;
    }

    #endregion

    public async Task<bool> AddRoleToUserAsync(Guid userId, Guid roleId)
    {
        var user = await Context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        var model = new UserRoleDbModel
        {
            UserId = userId,
            RoleId = roleId,
            TenantId = user.TenantId,
        };
        var record = await Context.UserRoles.AddAsync(model);
        var recordsAdded = await Context.SaveChangesAsync();
        return recordsAdded > 0;
    }

    public async Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId)
    {
        var record = await Context.UserRoles.FirstOrDefaultAsync(
            ur => ur.UserId == userId && ur.RoleId == roleId);
        if (record == null)
        {
            throw new Exception("User role not found");
        }
        Context.UserRoles.Remove(record);
        var recordsDeleted = await Context.SaveChangesAsync();
        return recordsDeleted > 0;
    }

    public async Task<IEnumerable<UserRole>> GetRolesForUserAsync(Guid userId)
    {
        var records = await Context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        return records.Select(r => new UserRole
        {
            UserId = r.UserId,
            RoleId = r.RoleId,
            TenantId = r.TenantId,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
        });
    }

    public async Task<bool> HasUserThisRoleAsync(Guid userId, Guid roleId)
    {
        var record = await Context.UserRoles.FirstOrDefaultAsync(
            ur => ur.UserId == userId && ur.RoleId == roleId);
        return record != null;
    }

    public async Task<List<UserRole>> GetUsersInRoleAsync(Guid roleId)
    {
        var records = await Context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync();

        return records.Select(r => new UserRole
        {
            UserId = r.UserId,
            RoleId = r.RoleId,
            TenantId = r.TenantId,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
        }).ToList();
    }
}

