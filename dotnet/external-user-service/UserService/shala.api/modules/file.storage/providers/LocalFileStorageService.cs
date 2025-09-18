using shala.api.common;
using shala.api.modules.storage;

public class LocalFileStorageService : IFileStorageProviderService
{
    private string _storagePath = string.Empty;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly IConfiguration _configuration;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _configuration = configuration;
        _storagePath = _configuration.GetSection("FileStorage:Local:StoragePath")?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(_storagePath))
        {
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
        }
        else
        {
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), _storagePath);
        }
        Directory.CreateDirectory(_storagePath); // Ensure the directory exists
    }

    public async Task<string?> UploadAsync(IFormFile file)
    {
        try
        {
            var key = getStorageKey(file.FileName);
            var filePath = Path.Combine(_storagePath, key);
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath) && !string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return null;
        }
    }

    public async Task<Stream?> DownloadAsync(string storageKey)
    {
        try
        {
            var filePath = Path.Combine(_storagePath, storageKey);
            if (!File.Exists(filePath))
            {
                return null;
            }

            var memoryStream = new MemoryStream();
            using var fileStream = new FileStream(filePath, FileMode.Open);
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string storageKey)
    {
        try
        {
            var filePath = Path.Combine(_storagePath, storageKey);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await Task.CompletedTask;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return false;
        }
    }

    private string getStorageKey(string filename)
    {
        var baseStorageKeyPath = _configuration.GetValue<string>("FileStorage:Local:BaseStorageKeyPath") ?? string.Empty;
        var randomString = Helper.GenerateRandomString(6);
        var filePath = DateTime.Now.ToString("yyyy-MM-dd") + "/" + randomString + "_" + filename;
        var key = string.IsNullOrEmpty(baseStorageKeyPath) ? filePath : baseStorageKeyPath + "/" + filePath;
        return key;
    }

}
