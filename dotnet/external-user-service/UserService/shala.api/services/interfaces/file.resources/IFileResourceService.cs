using shala.api.domain.types;

namespace shala.api.services;

public interface IFileResourceService
{
    Task<FileResource?> CreateAsync(FileResourceCreateModel model);
    Task<FileResource?> GetByIdAsync(Guid id);
    Task<SearchResults<FileResource>> SearchAsync(FileResourceSearchFilters filters);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> CanDownloadAsync(FileResource resource, Guid userId);
    Task<bool> CanAccessAsync(FileResource resource, Guid userId);
    Task<bool> CanDeleteAsync(FileResource resource, Guid userId);
}
