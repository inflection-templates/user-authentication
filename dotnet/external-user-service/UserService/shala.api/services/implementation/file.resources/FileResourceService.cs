using shala.api.common;
using shala.api.database.interfaces;
using shala.api.domain.types;

namespace shala.api.services;

public class FileResourceService : IFileResourceService
{

    #region Constructor

    private readonly IFileResourceRepository _fileResourceRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<FileResourceService> _logger;

    public FileResourceService(
        IFileResourceRepository fileResourceRepository,
        IUserRoleRepository userRoleRepository,
        IRoleRepository roleRepository,
        ILogger<FileResourceService> logger)
    {
        _fileResourceRepository = fileResourceRepository;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    #endregion

    public async Task<bool> CanAccessAsync(FileResource resource, Guid userId)
    {
        return await hasAccess(resource, userId);
    }

    public async Task<bool> CanDeleteAsync(FileResource resource, Guid userId)
    {
        return await hasAccess(resource, userId);
    }

    public async Task<bool> CanDownloadAsync(FileResource resource, Guid userId)
    {
        return await hasAccess(resource, userId);
    }

    public async Task<FileResource?> CreateAsync(FileResourceCreateModel model)
    {
        return await _fileResourceRepository.CreateAsync(model);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _fileResourceRepository.DeleteAsync(id);
    }

    public async Task<FileResource?> GetByIdAsync(Guid id)
    {
        return await _fileResourceRepository.GetByIdAsync(id);
    }

    public async Task<SearchResults<FileResource>> SearchAsync(FileResourceSearchFilters filters)
    {
        return await _fileResourceRepository.SearchAsync(filters);
    }

    private async Task<bool> hasAccess(FileResource resource, Guid userId)
    {
        var isOwner = resource.OwnerUserId == userId;
        if (isOwner)
        {
            return true;
        }
        var systemAdminRole = await _roleRepository.GetByNameAsync("SystemAdmin");
        if (systemAdminRole == null)
        {
            return false;
        }
        var userRoles = await _userRoleRepository.GetRolesForUserAsync(userId);
        if (userRoles != null && userRoles.Any())
        {
            var isSystemAdmin = userRoles.Any(r => r.RoleId == systemAdminRole.Id);
            if (isSystemAdmin)
            {
                return true;
            }
        }
        return false;
    }

}
