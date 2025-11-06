using Google.Cloud.Storage.V1;
using shala.api.common;
using shala.api.modules.storage;

public class GoogleCloudStorageService : IFileStorageProviderService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly IConfiguration _configuration;
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

    public GoogleCloudStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _configuration = configuration;
        _bucketName = _configuration.GetValue<string>("FileStorage:GoogleCloudStorage:BucketName") ?? string.Empty;
        if (string.IsNullOrEmpty(_bucketName))
        {
            _bucketName = "Storage";
        }
        var jsonKeyFilePath = _configuration.GetValue<string>("FileStorage:GoogleCloudStorage:JsonKeyFilePath") ?? string.Empty;
        var projectId = _configuration.GetValue<string>("FileStorage:GoogleCloudStorage:ProjectId") ?? string.Empty;

        //Ensure GOOGLE_APPLICATION_CREDENTIALS is set in the environment variables
        //export GOOGLE_APPLICATION_CREDENTIALS="/<env-storage-path>/service-account.json"
        _storageClient = StorageClient.Create();
    }

    public async Task<bool> DeleteAsync(string storageKey)
    {
        try
        {
            await _storageClient.DeleteObjectAsync(_bucketName, storageKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return false;
        }
    }

    public async Task<Stream?> DownloadAsync(string storageKey)
    {
        try
        {
            var memoryStream = new MemoryStream();
            var response = await _storageClient.DownloadObjectAsync(_bucketName, storageKey, memoryStream);
            if (response == null)
            {
                _logger.LogError($"Error downloading file");
                return null;
            }
            _logger.LogInformation($"File downloaded successfully: {response.Name}");
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return null;
        }
    }


    public async Task<string?> UploadAsync(IFormFile file)
    {
        try
        {
            var key = getStorageKey(file.FileName);

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var response = await _storageClient.UploadObjectAsync(_bucketName, key, file.ContentType, memoryStream);
            if (response == null)
            {
                _logger.LogError($"Error uploading file");
                return null;
            }
            _logger.LogInformation($"File uploaded successfully: {response.Name}");
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return null;
        }
    }

    private string getStorageKey(string filename)
    {
        var baseStorageKeyPath = _configuration.GetValue<string>("FileStorage:GoogleCloudStorage:BaseStorageKeyPath") ?? string.Empty;
        var randomString = Helper.GenerateRandomString(6);
        return baseStorageKeyPath + "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/" + randomString + "_" + filename;
    }

}
