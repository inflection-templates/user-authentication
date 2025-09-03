using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using shala.api.common;
using shala.api.modules.storage;

public class AmazonS3Service : IFileStorageProviderService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public AmazonS3Service(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _configuration = configuration;
        _bucketName = _configuration.GetValue<string>("FileStorage:AmazonS3:BucketName") ?? string.Empty;
        if (string.IsNullOrEmpty(_bucketName))
        {
            _bucketName = "Storage";
        }
        var awsAccessKey = _configuration.GetValue<string>("FileStorage:AmazonS3:AccessKey");
        var awsSecretKey = _configuration.GetValue<string>("FileStorage:AmazonS3:SecretKey");
        var awsRegion = _configuration.GetValue<string>("FileStorage:AmazonS3:Region");
        var region = RegionEndpoint.GetBySystemName(awsRegion);
        var awsCredentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
        _s3Client = new AmazonS3Client(awsCredentials, region);
    }

    public async Task<bool> DeleteAsync(string storageKey)
    {
        try
        {
            var resp = await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = storageKey
            });
            return resp.HttpStatusCode == HttpStatusCode.OK;
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
            var response = await _s3Client.GetObjectAsync(_bucketName, storageKey);
            return response.ResponseStream;
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
            using var newMemoryStream = new MemoryStream();
            await file.CopyToAsync(newMemoryStream);

            var request = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = key,
                BucketName = _bucketName,
                ContentType = file.ContentType
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(request);

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
        var baseStorageKeyPath = _configuration.GetValue<string>("FileStorage:AmazonS3:BaseStorageKeyPath") ?? string.Empty;
        var randomString = Helper.GenerateRandomString(6);
        return baseStorageKeyPath + "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/" + randomString + "_" + filename;
    }

}
