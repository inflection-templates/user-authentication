using shala.api.domain.types;

namespace shala.api.database.interfaces;

public interface IFileResourceRepository
{
    Task<FileResource?> CreateAsync(FileResourceCreateModel model);
    Task<FileResource?> GetByIdAsync(Guid path);
    Task<SearchResults<FileResource>> SearchAsync(FileResourceSearchFilters filters);
    Task<bool> DeleteAsync(Guid path);
}
