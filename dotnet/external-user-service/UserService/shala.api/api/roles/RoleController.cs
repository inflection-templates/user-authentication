using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using BC = BCrypt.Net.BCrypt;
using shala.api.common;
using shala.api.domain.types;
using shala.api.services;
using shala.api.startup;

namespace shala.api;

public class RoleController
{
    private readonly IConfiguration _configuration;
    private readonly IRoleService _roleService;
    private readonly IValidator<RoleCreateModel> _createValidator;
    private readonly IValidator<RoleUpdateModel> _updateValidator;
    private readonly IValidator<RoleSearchFilters> _searchValidator;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
                        IConfiguration Configuration,
                        IRoleService roleService,
                        IValidator<RoleCreateModel> createValidator,
                        IValidator<RoleUpdateModel> updateValidator,
                        IValidator<RoleSearchFilters> searchValidator,
                        ILogger<RoleController> logger)
    {
        _configuration = Configuration;
        _roleService = roleService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _searchValidator = searchValidator;
        _logger = logger;
    }

    public async Task<IResult> Create(HttpContext context, RoleCreateModel model)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(model);
            if (!validationResult.IsValid) {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }
            var record = await _roleService.CreateAsync(model);
            if (record == null)
            {
                return ResponseHandler.InternalServerError("Error creating role");
            }
            return ResponseHandler.Created("Role created successfully", record, $"/roles/{record.Id}");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Search(HttpContext context, RoleSearchFilters filters)
    {
        try
        {
            var validationResult = await _searchValidator.ValidateAsync(filters);
            if (!validationResult.IsValid) {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }
            var roles = await _roleService.SearchAsync(filters);
            return ResponseHandler.Ok("Roles retrieved successfully", roles);
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
                return ResponseHandler.BadRequest("Invalid role id");
            }
            var record = await _roleService.GetByIdAsync(id);
            return record != null ?
                ResponseHandler.Ok("Role retrieved successfully", record, $"/roles/{record.Id}") :
                ResponseHandler.NotFound("Role not found");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> GetByName(HttpContext context, string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return ResponseHandler.BadRequest("Invalid role code");
            }
            var record = await _roleService.GetByNameAsync(name);
            return record != null ?
                ResponseHandler.Ok("Role retrieved successfully", record, $"/roles/{record.Id}") :
                ResponseHandler.NotFound("Role not found");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Update(HttpContext context, Guid id, RoleUpdateModel model)
    {
        try
        {
            var validationResult = await _updateValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }
            var record = await _roleService.UpdateAsync(id, model);
            if (record == null)
            {
                return ResponseHandler.InternalServerError("Error updating role");
            }
            return ResponseHandler.Ok("Role updated successfully", record, $"/roles/{record.Id}");
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
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }
            var currentUserTenantId = currentUser.TenantId;

            if (id == Guid.Empty)
            {
                return ResponseHandler.BadRequest("Invalid role id");
            }
            var record = await _roleService.GetByIdAsync(id);
            if (record == null)
            {
                return ResponseHandler.NotFound("Role not found");
            }
            if (record.IsDefaultRole)
            {
                return ResponseHandler.BadRequest("Default role cannot be deleted");
            }
            if (currentUserTenantId == Guid.Empty || record.TenantId != currentUserTenantId)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }
            var deleted = await _roleService.DeleteAsync(id);
            return deleted ?
                ResponseHandler.Ok("Role deleted") :
                ResponseHandler.InternalServerError("Error deleting role");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

}
