using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using shala.api.common;
using shala.api.modules.storage;

public class AzureBlobStorageService : IFileStorageProviderService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _containerName;
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _configuration = configuration;
        _containerName = _configuration.GetValue<string>("FileStorage:AzureBlobStorage:ContainerName") ?? string.Empty;
        if (string.IsNullOrEmpty(_containerName))
        {
            _containerName = "Storage";
        }
        var connectionString = _configuration.GetValue<string>("FileStorage:AzureBlobStorage:ConnectionString");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<bool> DeleteAsync(string storageKey)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(storageKey);
            var response = await blobClient.DeleteIfExistsAsync();
            if (response.Value)
            {
                return response.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return false;
        }
        return false;
    }

    public async Task<Stream?> DownloadAsync(string storageKey)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(storageKey);
            var downloadResponse = await blobClient.DownloadAsync();
            return downloadResponse.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
        }
        return null;
    }

    public async Task<string?> UploadAsync(IFormFile file)
    {
        try
        {
            var key = getStorageKey(file.FileName);
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            var blobClient = containerClient.GetBlobClient(key);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, true);

            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
        }
        return null;
    }

    private string getStorageKey(string filename)
    {
        var baseStorageKeyPath = _configuration.GetValue<string>("FileStorage:AzureBlobStorage:BaseStorageKeyPath") ?? string.Empty;
        var randomString = Helper.GenerateRandomString(6);
        return baseStorageKeyPath + "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/" + randomString + "_" + filename;
    }

}
