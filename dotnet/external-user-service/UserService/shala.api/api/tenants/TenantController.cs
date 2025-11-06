using FluentValidation;
using shala.api.common;
using shala.api.domain.types;
using shala.api.services;
using shala.api.startup;

namespace shala.api;

public class TenantController
{
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IUserRoleService _userRoleService;
    private readonly IValidator<TenantCreateModel> _createValidator;
    private readonly IValidator<TenantUpdateModel> _updateValidator;
    private readonly IValidator<TenantSearchFilters> _searchValidator;
    private readonly ILogger<TenantController> _logger;

    public TenantController(
                        ITenantService TenantService,
                        IUserService userService,
                        IRoleService roleService,
                        IUserRoleService userRoleService,
                        IValidator<TenantCreateModel> createValidator,
                        IValidator<TenantUpdateModel> updateValidator,
                        IValidator<TenantSearchFilters> searchValidator,
                        ILogger<TenantController> logger)
    {
        _tenantService = TenantService;
        _userService = userService;
        _roleService = roleService;
        _userRoleService = userRoleService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _searchValidator = searchValidator;
        _logger = logger;
    }

    public async Task<IResult> Create(HttpContext context, TenantCreateModel model)
    {
        try
        {
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null)
            {
                return ResponseHandler.Unauthorized("User not authenticated");
            }
            var isSystemAdmin = currentUser.Role == DefaultRoles.SystemAdmin.ToString();
            if (!isSystemAdmin)
            {
                return ResponseHandler.Forbidden("Only system admin can create tenant");
            }
            var validationResult = await _createValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }

            var tenantCode = model.Code;
            if (tenantCode != null)
            {
                var existingTenantWithTenantCode = await _tenantService.GetByCodeAsync(tenantCode);
                if (existingTenantWithTenantCode != null)
                {
                    return ResponseHandler.Conflict("Tenant already exists");
                }
            }
            else
            {
                tenantCode = await Helper.GetUniqueTenantCode(this._tenantService, model.Name);
                model.Code = tenantCode;
            }
            var email = model.Email;
            if (email == null)
            {
                return ResponseHandler.BadRequest("Email is required");
            }

            var existingTenantWithEmail = await _tenantService.GetByEmailAsync(email);
            if (existingTenantWithEmail != null)
            {
                return ResponseHandler.Conflict("Tenant with this email already exists");
            }

            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
            {
                user = await this._userService.CreateAsync(new UserCreateModel
                {
                    Email = email,
                    UserName = email,
                    Password = model.Password,
                    CountryCode = model.CountryCode ?? null,
                    PhoneNumber = model.PhoneNumber ?? null,
                });
            }
            if (user == null)
            {
                return ResponseHandler.InternalServerError("Tenant admin user cannot be created");
            }

            var role = await _roleService.GetByNameAsync("TenantAdmin");
            if (role == null)
            {
                return ResponseHandler.InternalServerError("Role not found");
            }

            var record = await _tenantService.CreateAsync(model);
            if (record == null)
            {
                return ResponseHandler.InternalServerError("Tenant not created");
            }

            var roleAdded = await _userRoleService.AddRoleToUserAsync(user.Id, role.Id);
            if (!roleAdded)
            {
                return ResponseHandler.InternalServerError("User role not created");
            }

            return ResponseHandler.Created("Tenant created successfully", record, $"/tenants/{record.Id}");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }

    }

    public async Task<IResult> GetById(HttpContext context, Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return ResponseHandler.BadRequest("Invalid tenant id");
            }
            var record = await _tenantService.GetByIdAsync(id);
            return record != null ?
                ResponseHandler.Ok("Tenant retrieved successfully", record, $"/tenants/{record.Id}") :
                ResponseHandler.NotFound("Tenant not found");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> GetByCode(HttpContext context, string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return ResponseHandler.BadRequest("Invalid tenant code");
            }
            var record = await _tenantService.GetByCodeAsync(code);
            return record != null ?
                ResponseHandler.Ok("Tenant retrieved successfully", record, $"/tenants/{record.Id}") :
                ResponseHandler.NotFound("Tenant not found");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Update(HttpContext context, Guid id, TenantUpdateModel model)
    {
        try
        {
            var hasPermissions = this.hasPermissions(context, id);
            if (!hasPermissions)
            {
                return ResponseHandler.Forbidden("You do not have permission to update this tenant");
            }
            var validationResult = await _updateValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }
            var record = await _tenantService.UpdateAsync(id, model);
            if (record == null)
            {
                return ResponseHandler.NotFound("record not found");
            }
            return ResponseHandler.Ok("Tenant updated successfully", record, $"/tenants/{record.Id}");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    private bool hasPermissions(HttpContext context, Guid id)
    {
        var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
        if (currentUser == null)
        {
            return false;
        }
        var currentUserTenantId = currentUser.TenantId;
        var isSystemAdmin = currentUser.Role == DefaultRoles.SystemAdmin.ToString();
        if (isSystemAdmin)
        {
            return true;
        }
        if (currentUserTenantId != id)
        {
            return false;
        }
        var isTenantAdmin = currentUser.Role == DefaultRoles.TenantAdmin.ToString();
        if (!isTenantAdmin)
        {
            return false;
        }
        return true;
    }

    public async Task<IResult> Search(HttpContext context, TenantSearchFilters filters)
    {
        try
        {
            var validationResult = await _searchValidator.ValidateAsync(filters);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }
            var tenants = await _tenantService.SearchAsync(filters);
            return ResponseHandler.Ok("Tenants retrieved successfully", tenants);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Delete(HttpContext context, Guid id)
    {
        try
        {
            var hasPermissions = this.hasPermissions(context, id);
            if (!hasPermissions)
            {
                return ResponseHandler.Forbidden("You do not have permission to delete this tenant");
            }
            if (id == Guid.Empty)
            {
                return ResponseHandler.BadRequest("Invalid tenant id");
            }
            var deleted = await _tenantService.DeleteAsync(id);
            return deleted ?
                ResponseHandler.Ok("Tenant deleted successfully") :
                ResponseHandler.InternalServerError("Error deleting tenant");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

}
