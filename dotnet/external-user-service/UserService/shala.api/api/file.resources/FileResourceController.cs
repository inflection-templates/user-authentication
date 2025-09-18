using FluentValidation;
using shala.api.common;
using shala.api.domain.types;
using shala.api.services;
using shala.api.startup;
using shala.api.modules.storage;

namespace shala.api;

public class FileResourceController
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileResourceController> _logger;
    private readonly IUserService _userService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileResourceService _fileResourceService;
    private readonly IUserAuthService _userAuthService;
    private readonly IRoleService _roleService;
    private readonly IValidator<FileResourceSearchFilters> _searchValidator;
    private readonly IValidator<FileResourceCreateModel> _createValidator;

    public FileResourceController(
                        IConfiguration configuration,
                        ILogger<FileResourceController> logger,
                        IUserService UserService,
                        IRoleService roleService,
                        IFileResourceService fileResourceService,
                        IFileStorageService fileStorageService,
                        IUserAuthService userAuthService,
                        IValidator<FileResourceCreateModel> createValidator,
                        IValidator<FileResourceSearchFilters> searchValidator)
    {
        _configuration = configuration;
        _logger = logger;
        _userService = UserService;
        _roleService = roleService;
        _userAuthService = userAuthService;
        _fileResourceService = fileResourceService;
        _fileStorageService = fileStorageService;
        _createValidator = createValidator;
        _searchValidator = searchValidator;
    }

    public async Task<IResult> Upload(HttpContext context, IFormFile file, bool isPublic)
    {
        try
        {
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }
            var userId = currentUser.UserId;
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return ResponseHandler.NotFound("User not found");
            }

            if (file == null || file.Length == 0)
            {
                return ResponseHandler.BadRequest("Invalid file");
            }

            var storageKey = await _fileStorageService.UploadAsync(file);
            if (string.IsNullOrEmpty(storageKey))
            {
                return ResponseHandler.BadRequest("Failed to upload file");
            }

            var metadata = new FileResourceCreateModel {
                OwnerUserId   = userId,
                TenantId      = user.TenantId,
                StorageKey    = storageKey,
                FileName      = file.FileName,
                FileSize      = file.Length,
                FileExtension = Path.GetExtension(file.FileName),
                MimeType      = file.ContentType,
                IsPublic      = isPublic,
            };
            var validationResult = await _createValidator.ValidateAsync(metadata);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }

            var resource = await _fileResourceService.CreateAsync(metadata);
            if (resource == null)
            {
                return ResponseHandler.BadRequest("Failed to create file resource");
            }
            return ResponseHandler.Created("File uploaded successfully", resource);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> UploadMany(HttpContext context, IEnumerable<IFormFile> files, bool isPublic)
    {
        try
        {
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }
            var userId = currentUser.UserId;
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return ResponseHandler.NotFound("User not found");
            }

            if (files == null || !files.Any())
            {
                return ResponseHandler.BadRequest("Invalid files");
            }

            var resources = new List<FileResource>();
            var failedUplaods = new List<string>();
            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                {
                    continue;
                }

                var storageKey = await _fileStorageService.UploadAsync(file);
                if (string.IsNullOrEmpty(storageKey))
                {
                    failedUplaods.Add(file.FileName);
                    continue;
                }

                var metadata = new FileResourceCreateModel {
                    OwnerUserId = userId,
                    TenantId    = user.TenantId,
                    StorageKey  = storageKey,
                    FileName    = file.FileName,
                    FileSize    = file.Length,
                    MimeType    = file.ContentType,
                    IsPublic    = isPublic,
                };

                var validationResult = await _createValidator.ValidateAsync(metadata);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.ToDictionary(
                            e => e.PropertyName,
                            e => new[] { e.ErrorMessage }
                        );
                    _logger.LogError($"Validation error: {metadata.FileName}", errors);
                }

                var resource = await _fileResourceService.CreateAsync(metadata);
                if (resource != null)
                {
                    resources.Add(resource);
                }
            }

            if (resources.Count == 0)
            {
                return ResponseHandler.InternalServerError("Failed to upload files");
            }

            var message = failedUplaods.Any() ? "Failed to upload some files" : "Files uploaded successfully";
            var res = new { Resources = resources, FailedUploads = failedUplaods };
            return ResponseHandler.Created(message, failedUplaods.Any() ? res : resources);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Download(HttpContext context, Guid fileId)
    {
        try
        {
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }
            var userId = currentUser.UserId;
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return ResponseHandler.NotFound("User not found");
            }

            var resource = await _fileResourceService.GetByIdAsync(fileId);
            if (resource == null)
            {
                return ResponseHandler.NotFound("File not found");
            }

            var canDownload = await _fileResourceService.CanDownloadAsync(resource, userId);
            if (!canDownload)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }

            var file = await _fileStorageService.DownloadAsync(resource.StorageKey);
            if (file == null)
            {
                return ResponseHandler.NotFound("File not found");
            }

            return Results.File(file, resource.MimeType, resource.FileName);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> DownloadPublic(HttpContext context, Guid fileId)
    {
        try
        {
            var resource = await _fileResourceService.GetByIdAsync(fileId);
            if (resource == null)
            {
                return ResponseHandler.NotFound("File not found");
            }

            if (!resource.IsPublic)
            {
                return ResponseHandler.Unauthorized("Unauthorized access: The file is not publicly accessible");
            }

            var file = await _fileStorageService.DownloadAsync(resource.StorageKey);
            if (file == null)
            {
                return ResponseHandler.NotFound("File not found");
            }

            return ResponseHandler.Ok("File downloaded successfully", file);
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
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }

            var resource = await _fileResourceService.GetByIdAsync(id);
            if (resource == null)
            {
                return ResponseHandler.NotFound("File not found");
            }

            var canDownload = await _fileResourceService.CanAccessAsync(resource, currentUser.UserId);
            if (!canDownload)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }

            return ResponseHandler.Ok("File resource retrieved successfully", resource);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Search(HttpContext context, FileResourceSearchFilters filters)
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

            var resources = await _fileResourceService.SearchAsync(filters);
            return ResponseHandler.Ok("File resources retrieved successfully", resources);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Delete(HttpContext context, Guid fileId)
    {
        try
        {
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }

            var resource = await _fileResourceService.GetByIdAsync(fileId);
            if (resource == null)
            {
                return ResponseHandler.NotFound("File not found");
            }

            var canDelete = await _fileResourceService.CanDeleteAsync(resource, currentUser.UserId);
            if (!canDelete)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }

            var deletedFromStorage = await _fileStorageService.DeleteAsync(resource.StorageKey);
            if (!deletedFromStorage)
            {
                return ResponseHandler.InternalServerError("Error deleting file from storage");
            }

            var deleted = await _fileResourceService.DeleteAsync(resource.Id);
            return deleted ?
                ResponseHandler.Ok("File deleted") :
                ResponseHandler.InternalServerError("Error deleting delete");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

}
