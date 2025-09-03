using shala.api.database.interfaces.models;
using shala.api.domain.types;

namespace shala.api.database.interfaces;

public interface ITenantRepository
  : ICRUDRepository<
    Tenant,
    TenantCreateModel,
    TenantUpdateModel,
    TenantSearchFilters,
    IDbModel>
{
    Task<Tenant?> GetByCodeAsync(string code);
    Task<Tenant?> GetByEmailAsync(string email);
}
