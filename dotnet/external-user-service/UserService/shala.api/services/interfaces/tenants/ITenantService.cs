using shala.api.domain.types;

namespace shala.api.services;

public interface ITenantService
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetByCodeAsync(string code);
    Task<Tenant?> CreateAsync(TenantCreateModel model);
    Task<Tenant?> UpdateAsync(Guid id, TenantUpdateModel model);
    Task<bool> DeleteAsync(Guid id);
    Task<SearchResults<Tenant>> SearchAsync(TenantSearchFilters filters);
    Task<Tenant?> GetByEmailAsync(string email);
}
