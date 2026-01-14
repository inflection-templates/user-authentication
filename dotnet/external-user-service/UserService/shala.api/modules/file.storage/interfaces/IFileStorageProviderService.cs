using shala.api.domain.types;

namespace shala.api.modules.storage;


public interface IFileStorageProviderService
{
    Task<bool> DeleteAsync(string storageKey);
    Task<Stream?> DownloadAsync(string storageKey);
    Task<string?> UploadAsync(IFormFile file);
}
