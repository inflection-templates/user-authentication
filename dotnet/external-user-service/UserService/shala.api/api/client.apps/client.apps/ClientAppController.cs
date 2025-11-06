using System.Text;
using FluentValidation;
using shala.api.common;
using shala.api.domain.types;
using shala.api.services;
using shala.api.startup;

namespace shala.api;

public class ClientAppController
{
    private readonly IClientAppService _clientAppService;
    private readonly IApiKeyService _apiKeyService;
    private readonly IValidator<ClientAppCreateModel> _createValidator;
    private readonly IValidator<ClientAppUpdateModel> _updateValidator;
    private readonly IValidator<ClientAppSearchFilters> _searchValidator;
    private readonly ILogger<ClientAppController> _logger;

    public ClientAppController(IClientAppService ClientAppService,
                             IApiKeyService apiKeyService,
                             IValidator<ClientAppCreateModel> createValidator,
                             IValidator<ClientAppUpdateModel> updateValidator,
                             IValidator<ClientAppSearchFilters> searchValidator,
                             ILogger<ClientAppController> logger)
    {
        _clientAppService = ClientAppService;
        _apiKeyService = apiKeyService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _searchValidator = searchValidator;
        _logger = logger;
    }

    public async Task<IResult> GetById(HttpContext context, Guid id)
    {
        try
        {

            if (id == Guid.Empty)
            {
                return ResponseHandler.BadRequest("Invalid client app id");
            }
            var ClientApp = await _clientAppService.GetByIdAsync(id);
            return ClientApp != null ?
                ResponseHandler.Ok("ClientApp retrieved successfully", ClientApp) :
                ResponseHandler.NotFound("ClientApp not found");
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
                return ResponseHandler.BadRequest("Invalid client app code");
            }
            var ClientApp = await _clientAppService.GetByCodeAsync(code);
            return ClientApp != null ?
                ResponseHandler.Ok("ClientApp retrieved successfully", ClientApp) :
                ResponseHandler.NotFound("ClientApp not found");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Create(HttpContext context, ClientAppCreateModel model)
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

            var record = await _clientAppService.CreateAsync(model);
            if (record == null)
            {
                return ResponseHandler.InternalServerError("ClientApp not created");
            }
            return ResponseHandler.Created("ClientApp created successfully", model, $"/ClientApps/{record.Id}");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }

    }

    public async Task<IResult> Update(HttpContext context, Guid id, ClientAppUpdateModel model)
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
            var updated = await _clientAppService.UpdateAsync(id, model);
            return ResponseHandler.Ok("ClientApp updated", updated);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Search(HttpContext context, ClientAppSearchFilters filters)
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
            var users = await _clientAppService.SearchAsync(filters);
            return ResponseHandler.Ok("ClientApps retrieved successfully", users);
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
            var clientApp = await _clientAppService.GetByIdAsync(id);
            if (clientApp == null)
            {
                return ResponseHandler.NotFound("Client app for the API key is not found");
            }

            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null)
            {
                return ResponseHandler.Unauthorized("Unauthorized");
            }
            if (clientApp.OwnerUserId != currentUser.UserId)
            {
                return ResponseHandler.Unauthorized("Unauthorized");
            }

            var deletedKeys = await _apiKeyService.DeleteByClientAppIdAsync(id);
            var deleted = await _clientAppService.DeleteAsync(id);
            return deleted ?
                ResponseHandler.Ok("ClientApp deleted") :
                ResponseHandler.InternalServerError("Error deleting user");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

}
