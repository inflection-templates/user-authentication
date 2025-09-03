using shala.api.domain.types;

namespace shala.api.services;

public interface IRoleService
{
    Task<Role?> CreateAsync(RoleCreateModel model);
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role?> GetByNameAsync(string name);
    Task<Role?> UpdateAsync(Guid id, RoleUpdateModel model);
    Task<bool> DeleteAsync(Guid id);
    Task<SearchResults<Role>> SearchAsync(RoleSearchFilters filters);
}
