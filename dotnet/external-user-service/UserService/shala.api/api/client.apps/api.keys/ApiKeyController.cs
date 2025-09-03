using FluentValidation;
using shala.api.common;
using shala.api.domain.types;
using shala.api.services;

namespace shala.api;

public class ApiKeyController
{
    private readonly IClientAppService _clientAppService;
    private readonly IApiKeyService _apiKeyService;
    private readonly IValidator<ApiKeyCreateModel> _createValidator;
    private readonly IValidator<ApiKeyCreateRequestModel> _createRequestValidator;
    private readonly IValidator<ApiKeySearchFilters> _searchValidator;
    private readonly ILogger<ApiKeyController> _logger;

    public ApiKeyController(IApiKeyService apiKeyService,
                            IClientAppService clientAppService,
                            IValidator<ApiKeyCreateModel> createValidator,
                            IValidator<ApiKeyCreateRequestModel> createRequestValidator,
                            IValidator<ApiKeySearchFilters> searchValidator,
                            ILogger<ApiKeyController> logger)
    {
        _clientAppService = clientAppService;
        _apiKeyService = apiKeyService;
        _createValidator = createValidator;
        _createRequestValidator = createRequestValidator;
        _searchValidator = searchValidator;
        _logger = logger;
    }

    public async Task<IResult> GenerateApiKey(HttpContext context, ApiKeyCreateRequestModel model)
    {
        try
        {
            var validationResult = await _createRequestValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }

            var clientAppId = model.ClientAppId;
            var clientApp = await _clientAppService.GetByIdAsync(clientAppId);
            if (clientApp == null)
            {
                return ResponseHandler.NotFound("Client app not found");
            }
            var apiKey = Helper.GenerateKey(32);
            var apiSecret = Helper.GenerateKey(32);
            var apiSecretHash = BCrypt.Net.BCrypt.HashPassword(apiSecret);
            if (model.ValidTill == null)
            {
                // Default to 1 year
                model.ValidTill = DateTime.UtcNow.AddYears(1);
            }
            var apiKeyCreateModel = new ApiKeyCreateModel
            {
                ClientAppId = clientApp.Id,
                Name = model.Name,
                Description = model.Description,
                ValidTill = model.ValidTill,
                Key = apiKey,
                SecretHash = apiSecretHash
            };
            validationResult = await _createValidator.ValidateAsync(apiKeyCreateModel);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }
            var record = await _apiKeyService.CreateAsync(apiKeyCreateModel);
            var msg = "API Key generated successfully. Please store this key and secret somewhere safe. The secret will not be available again.";
            return apiKey != null ?
                ResponseHandler.Created(msg, record) :
                ResponseHandler.InternalServerError("Error generating API Key");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> DeleteApiKey(HttpContext context, Guid id)
    {
        try
        {
            var apiKey = await _apiKeyService.GetByIdAsync(id);
            if (apiKey == null)
            {
                return ResponseHandler.NotFound("API key not found");
            }
            var deleted = await _apiKeyService.DeleteAsync(id);
            return deleted ?
                ResponseHandler.Ok("API key deleted") :
                ResponseHandler.InternalServerError("Error deleting API key");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> SearchApiKeys(HttpContext context, ApiKeySearchFilters filters)
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
            var apiKeys = await _apiKeyService.SearchAsync(filters);
            return ResponseHandler.Ok("API keys retrieved successfully", apiKeys);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> DeleteByClientAppId(HttpContext context, Guid id)
    {
        try
        {
            var deleted = await _apiKeyService.DeleteByClientAppIdAsync(id);
            return deleted ?
                ResponseHandler.Ok("API keys deleted") :
                ResponseHandler.InternalServerError("Error deleting API keys");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> GetByClientAppIdAsync(HttpContext context, Guid id)
    {
        try
        {
            var apiKeys = await _apiKeyService.GetByClientAppIdAsync(id);
            return ResponseHandler.Ok("API keys retrieved successfully", apiKeys);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

}
