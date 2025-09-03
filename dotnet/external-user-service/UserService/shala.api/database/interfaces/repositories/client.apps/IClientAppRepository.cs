using shala.api.database.interfaces.models;
using shala.api.domain.types;

namespace shala.api.database.interfaces;

public interface IClientAppRepository
  : ICRUDRepository<
    ClientApp,
    ClientAppCreateModel,
    ClientAppUpdateModel,
    ClientAppSearchFilters,
    IDbModel>
{
    Task<ClientApp?> GetByApiKeyAsync(string apiKey);
    Task<ClientApp?> GetByCodeAsync(string code);

}
