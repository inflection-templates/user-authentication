using shala.api.common;
using shala.api.domain.types;
using shala.api.services;
using shala.api.startup;

namespace shala.api;

public class UserRoleController
{
    private readonly IUserService _userService;
    private readonly IUserRoleService _userRoleService;
    private readonly IRoleService _roleService;
    private readonly ILogger<UserRoleController> _logger;

    public UserRoleController(IUserService UserService,
                        IRoleService roleService,
                        IUserRoleService userRoleService,
                        ILogger<UserRoleController> logger)
    {
        _userService = UserService;
        _roleService = roleService;
        _userRoleService = userRoleService;
        _logger = logger;
    }

    public async Task<IResult> AddTenantRoleForUser(HttpContext context, Guid userId, Guid roleId)
    {
        try
        {
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null)
            {
                return ResponseHandler.Unauthorized("User not authenticated");
            }

            var role = await _roleService.GetByIdAsync(roleId);
            if (role == null)
            {
                return ResponseHandler.BadRequest("Role by given name not found");
            }
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return ResponseHandler.BadRequest("User by given id not found");
            }

            if (role.Name == DefaultRoles.SystemAdmin.ToString())
            {
                return ResponseHandler.BadRequest("Cannot assign system admin role to user");
            }
            if (role.Name == DefaultRoles.TenantAdmin.ToString())
            {
                var isSystemAdmin = currentUser.Role == DefaultRoles.SystemAdmin.ToString();
                var isTenantAdmin = currentUser.Role == DefaultRoles.TenantAdmin.ToString() &&
                                    currentUser.TenantId == user.TenantId;
                if (!isSystemAdmin && !isTenantAdmin)
                {
                    return ResponseHandler.Forbidden("Only system admin or tenant admin of same tenant can assign this role");
                }
            }
            var added = await _userRoleService.AddRoleToUserAsync(userId, role.Id);
            return added ?
                ResponseHandler.Ok("User role assigned successfully", new { Added = added }) :
                ResponseHandler.InternalServerError("Error adding user role");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> RemoveTenantRoleForUser(HttpContext context, Guid userId, Guid roleId)
    {
        try
        {
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null)
            {
                return ResponseHandler.Unauthorized("User not authenticated");
            }

            var role = await _roleService.GetByIdAsync(roleId);
            if (role == null)
            {
                return ResponseHandler.BadRequest("Role by given name not found");
            }
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return ResponseHandler.BadRequest("User by given id not found");
            }

            if (role.Name == DefaultRoles.SystemAdmin.ToString())
            {
                return ResponseHandler.BadRequest("Cannot remove system admin role assignment");
            }
            if (role.Name == DefaultRoles.TenantAdmin.ToString())
            {
                var isSystemAdmin = currentUser.Role == DefaultRoles.SystemAdmin.ToString();
                var isTenantAdmin = currentUser.Role == DefaultRoles.TenantAdmin.ToString() &&
                                    currentUser.TenantId == user.TenantId;
                if (!isSystemAdmin && !isTenantAdmin)
                {
                    return ResponseHandler.Forbidden("Only system admin or tenant admin of same tenant can remove this role");
                }
            }
            var removed = await _userRoleService.RemoveRoleFromUserAsync(userId, roleId);
            return removed ?
                ResponseHandler.Ok("User role removed successfully") :
                ResponseHandler.InternalServerError("Error removing user role");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> GetRolesForUser(HttpContext context, Guid userId)
    {
        try
        {
            var roles = await _userRoleService.GetRolesForUserAsync(userId);
            return ResponseHandler.Ok("User roles retrieved successfully", roles);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> HasUserThisRole(HttpContext context, Guid userId, Guid roleId)
    {
        try
        {
            var hasRole = await _userRoleService.HasUserThisRoleAsync(userId, roleId);
            return ResponseHandler.Ok("User role association retrieved successfully", new { HasRole = hasRole });
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

}
