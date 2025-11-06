using shala.api.database.interfaces.models;
using shala.api.domain.types;

namespace shala.api.database.interfaces;

public interface IRoleRepository
  : ICRUDRepository<
    Role,
    RoleCreateModel,
    RoleUpdateModel,
    RoleSearchFilters,
    IDbModel>
{
    Task<Role?> GetByCodeAsync(string code);
    Task<Role?> GetByNameAsync(string name);
}
