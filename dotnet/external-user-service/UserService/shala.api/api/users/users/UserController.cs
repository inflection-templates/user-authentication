using FluentValidation;
using shala.api.common;
using shala.api.domain.types;
using shala.api.eventmessaging;
using shala.api.services;
using shala.api.startup;

namespace shala.api;

public class UserController
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;
    private readonly IUserRoleService _userRoleService;
    private readonly IUserAuthService _userAuthService;
    private readonly IUserAuthProfileService _userAuthProfileService;
    private readonly IRoleService _roleService;
    private readonly IValidator<UserCreateModel> _createValidator;
    private readonly IValidator<UserUpdateModel> _updateValidator;

    public UserController(
                        IConfiguration configuration,
                        ILogger<UserController> logger,
                        IUserService UserService,
                        IRoleService roleService,
                        IUserRoleService userRoleService,
                        IUserAuthService userAuthService,
                        IUserAuthProfileService userAuthProfileService,
                        IValidator<UserCreateModel> createValidator,
                        IValidator<UserUpdateModel> updateValidator)
    {
        _configuration          = configuration;
        _logger                 = logger;
        _userService            = UserService;
        _roleService            = roleService;
        _userAuthService        = userAuthService;
        _userAuthProfileService = userAuthProfileService;
        _userRoleService        = userRoleService;
        _createValidator        = createValidator;
        _updateValidator        = updateValidator;
    }

    public async Task<IResult> GetById(HttpContext context, Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return ResponseHandler.BadRequest("Invalid user id");
            }
            var record = await _userService.GetByIdAsync(id);
            return record != null ?
                ResponseHandler.Ok("User retrieved successfully", record, $"/users/{record.Id}") :
                ResponseHandler.NotFound("User not found");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Create(HttpContext context, UserCreateModel model)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }
            //Existing user check
            var existingUser = await _userService.GetByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return ResponseHandler.Conflict("User already exists");
            }

            var userName = model.UserName;
            if (userName != null)
            {
                var existingUserWithUserName = await _userService.GetByUsernameAsync(userName);
                if (existingUserWithUserName != null)
                {
                    return ResponseHandler.Conflict("User already exists");
                }
            }
            else
            {
                userName = await Helper.GetUniqueUsername(
                  this._userService, model.FirstName, model.LastName);
                model.UserName = userName;
            }
            var record = await _userService.CreateAsync(model);
            if (record == null)
            {
                return ResponseHandler.InternalServerError("User not created");
            }
            string? hashedPassword = null;
            if (!string.IsNullOrEmpty(model.Password))
            {
                hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }
            var authProfile = await _userAuthProfileService.CreateUserAuthProfileAsync(record.Id, hashedPassword);
            if (authProfile == null)
            {
                return ResponseHandler.InternalServerError("User auth profile not created");
            }

            //Publish user signed up event to message broker
            var publisher = context.RequestServices.GetRequiredService<Publisher>();
            if (publisher != null)
            {
                var eventPublished = new UserSignedUp(record.Id.ToString(), record.Email, record.CountryCode, record.PhoneNumber);
                await publisher.Publish<UserSignedUp>(eventPublished);
            }

            return ResponseHandler.Created("User created successfully", record, $"/users/{record.Id}");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }

    }

    public async Task<IResult> Update(HttpContext context, Guid id, UserUpdateModel model)
    {
        try
        {
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null || id != currentUser.UserId)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
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
            var record = await _userService.UpdateAsync(id, model);
            if (record == null)
            {
                return ResponseHandler.NotFound("User not found");
            }
            return ResponseHandler.Ok("User updated successfully", record, $"/users/{record.Id}");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Search(HttpContext context, UserSearchFilters filters)
    {
        try
        {
            var users = await _userService.SearchAsync(filters);
            return ResponseHandler.Ok("Users retrieved successfully", users);
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
            if (currentUser == null || id != currentUser.UserId)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }
            if (id == Guid.Empty)
            {
                return ResponseHandler.BadRequest("Invalid user id");
            }
            var deleted = await _userService.DeleteAsync(id);
            return deleted ?
                ResponseHandler.Ok("User deleted") :
                ResponseHandler.InternalServerError("Error deleting user");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }



}
