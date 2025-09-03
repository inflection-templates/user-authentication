using shala.api.domain.types;

namespace shala.api.modules.storage;


public class FileStorageService : IFileStorageService
{

    #region Constructor

    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;
    private readonly IFileStorageProviderService _fileStorageProviderService;

    public FileStorageService(
        IConfiguration configuration,
        ILogger<FileStorageService> logger,
        IFileStorageProviderService fileStorageProviderService)
    {
        _configuration = configuration;
        _logger = logger;
        _fileStorageProviderService = fileStorageProviderService;
    }

    #endregion

    public async Task<bool> DeleteAsync(string storageKey)
    {
        return await _fileStorageProviderService.DeleteAsync(storageKey);
    }

    public async Task<Stream?> DownloadAsync(string storageKey)
    {
        return await _fileStorageProviderService.DownloadAsync(storageKey);
    }

    public async Task<string?> UploadAsync(IFormFile file)
    {
        return await _fileStorageProviderService.UploadAsync(file);
    }
}
