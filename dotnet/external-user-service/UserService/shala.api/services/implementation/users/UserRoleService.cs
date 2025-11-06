using shala.api.domain.types;
using shala.api.database.interfaces;

namespace shala.api.services;

public class UserRoleService : IUserRoleService
{

    #region Constructor

    private readonly IUserRoleRepository _userRoleRepository;
    private readonly ILogger<UserRoleService> _logger;

    public UserRoleService(IUserRoleRepository userRoleRepository, ILogger<UserRoleService> logger)
    {
        _userRoleRepository = userRoleRepository;
        _logger = logger;
    }

    public async Task<bool> AddRoleToUserAsync(Guid userId, Guid roleId)
    {
        return await _userRoleRepository.AddRoleToUserAsync(userId, roleId);
    }

    public async Task<IEnumerable<UserRole>> GetRolesForUserAsync(Guid userId)
    {
        return await _userRoleRepository.GetRolesForUserAsync(userId);
    }

    public async Task<bool> HasUserThisRoleAsync(Guid userId, Guid roleId)
    {
        return await _userRoleRepository.HasUserThisRoleAsync(userId, roleId);
    }

    public async Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId)
    {
        return await _userRoleRepository.RemoveRoleFromUserAsync(userId, roleId);
    }

    public async Task<List<UserRole>> GetUsersInRoleAsync(Guid roleId)
    {
        return await _userRoleRepository.GetUsersInRoleAsync(roleId);
    }
}

#endregion
