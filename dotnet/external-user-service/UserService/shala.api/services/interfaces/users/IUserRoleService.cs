using Microsoft.AspNetCore.Identity;
using shala.api.domain.types;

namespace shala.api.services;

public interface IUserRoleService
{
    Task<bool> AddRoleToUserAsync(Guid userId, Guid roleId);

    Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId);

    Task<IEnumerable<UserRole>> GetRolesForUserAsync(Guid userId);

    Task<bool> HasUserThisRoleAsync(Guid userId, Guid roleId);

    Task<List<UserRole>> GetUsersInRoleAsync(Guid roleId);

}
