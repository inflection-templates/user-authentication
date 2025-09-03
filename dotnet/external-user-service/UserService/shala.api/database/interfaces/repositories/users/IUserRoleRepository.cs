using shala.api.domain.types;

namespace shala.api.database.interfaces;

public interface IUserRoleRepository
{
    Task<bool> AddRoleToUserAsync(Guid userId, Guid roleId);

    Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId);

    Task<IEnumerable<UserRole>> GetRolesForUserAsync(Guid userId);

    Task<bool> HasUserThisRoleAsync(Guid userId, Guid roleId);

    Task<List<UserRole>> GetUsersInRoleAsync(Guid roleId);
}
