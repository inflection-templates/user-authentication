using shala.api.domain.types;

namespace shala.api.services;

public interface IClientAppService
{
    Task<ClientApp?> GetByIdAsync(Guid id);
    Task<ClientApp?> GetByCodeAsync(string code);
    Task<ClientApp?> GetByApiKeyAsync(string apiKey);
    Task<ClientApp?> CreateAsync(ClientAppCreateModel model);
    Task<ClientApp?> UpdateAsync(Guid id, ClientAppUpdateModel model);
    Task<bool> DeleteAsync(Guid id);
    Task<SearchResults<ClientApp>> SearchAsync(ClientAppSearchFilters filters);
}
