using shala.api.domain.types;

namespace shala.api.database.interfaces;

public interface ICRUDRepository<
    T,
    TCreateModel,
    TUpdateModel,
    TSearchFilters,
    TDbModel>
{
    Task<T?> CreateAsync(TCreateModel item);

    Task<SearchResults<T>> SearchAsync(TSearchFilters filters);

    Task<bool> ExistsAsync(Guid id);

    Task<T?> GetByIdAsync(Guid id);

    Task<T?> UpdateAsync(Guid id, TUpdateModel item);

    Task<bool> DeleteAsync(Guid id);

}
