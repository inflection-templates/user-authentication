using Microsoft.EntityFrameworkCore;
using shala.api.database.interfaces;
using shala.api.database.mappers;
using shala.api.domain.types;

namespace shala.api.database.relational.efcore;

public class ClientAppRepository : IClientAppRepository
{
    #region Construction

    public IConfiguration Configuration { get; }
    private DatabaseContext Context { get; set; }
    private readonly ILogger<ClientAppRepository> _logger;

    public ClientAppRepository(IConfiguration configuration, DatabaseContext context, ILogger<ClientAppRepository> logger)
    {
        Configuration = configuration;
        Context = context;
        _logger = logger;
    }

    #endregion

    public async Task<ClientApp?> CreateAsync(ClientAppCreateModel model)
    {
        var clientApp = ModelMapper.Map<ClientAppCreateModel, ClientAppDbModel>(model);
        var record = await Context.ClientApps.AddAsync(clientApp);
        var recordsAdded = await Context.SaveChangesAsync();
        if (recordsAdded == 0)
        {
            return null;
        }
        var dto = ModelMapper.Map<ClientAppDbModel, ClientApp>(record.Entity);
        return dto;
    }

    public async Task<ClientApp?> GetByIdAsync(Guid id)
    {
        var record = await Context.ClientApps.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<ClientAppDbModel, ClientApp>(record);
        return dto;
    }
    public async Task<ClientApp?> UpdateAsync(Guid id, ClientAppUpdateModel model)
    {
        var record = await Context.ClientApps.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            throw new Exception("Client App not found");
        }
        if (model.Name != null)
        {
            record.Name = model.Name;
        }
        if (model.Code != null)
        {
            record.Code = model.Code;
        }
        if (model.LogoUrl != null)
        {
            record.LogoUrl = model.LogoUrl;
        }
        if (model.WebsiteUrl != null)
        {
            record.WebsiteUrl = model.WebsiteUrl;
        }
        if (model.PrivacyPolicyUrl != null)
        {
            record.PrivacyPolicyUrl = model.PrivacyPolicyUrl;
        }
        if (model.TermsOfServiceUrl != null)
        {
            record.TermsOfServiceUrl = model.TermsOfServiceUrl;
        }
        if (model.Description != null)
        {
            record.Description = model.Description;
        }
        Context.ClientApps.Update(record);
        var recordsUpdated = await Context.SaveChangesAsync();
        if (recordsUpdated == 0)
        {
            throw new Exception("ClientApp not updated");
        }
        var dto = ModelMapper.Map<ClientAppDbModel, ClientApp>(record);
        return dto;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var record = await Context.ClientApps.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            throw new Exception("ClientApp not found");
        }
        Context.ClientApps.Remove(record);
        var recordsDeleted = await Context.SaveChangesAsync();
        if (recordsDeleted == 0)
        {
            throw new Exception("ClientApp not deleted");
        }
        return true;
    }

    public async Task<SearchResults<ClientApp>> SearchAsync(ClientAppSearchFilters filters)
    {
        var query = Context.ClientApps.AsQueryable();
        if (filters.Name != null)
        {
            query.Where(u => EF.Functions.Like(u.Name, $"%{filters.Name}%"));
        }
        if (filters.Code != null)
        {
            query.Where(u => EF.Functions.Like(u.Code, $"%{filters.Code}%"));
        }

        var orderBy = filters.OrderBy ?? "Name";
        query = filters.Order == SortOrder.Ascending ?
            query.OrderBy(e => EF.Property<object>(e, orderBy)) :
            query.OrderByDescending(e => EF.Property<object>(e, orderBy));

        var totalRecordsCount = query.Count();
        var offset = filters.PageIndex * filters.ItemsPerPage;
        var records = await query.Skip(offset).Take(filters.ItemsPerPage).ToListAsync();
        var dtos = ModelMapper.Map<IEnumerable<ClientAppDbModel>, IEnumerable<ClientApp>>(records);
        var results = new SearchResults<ClientApp>
        {
            Items = dtos,
            ItemsPerPage = filters.ItemsPerPage,
            PageIndex = filters.PageIndex,
            RetrievedCount = records.Count,
            TotalCount = totalRecordsCount,
            TotalPages = (int)Math.Ceiling((double)totalRecordsCount / filters.ItemsPerPage)
        };
        return results;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var exists = await Context.ClientApps.AnyAsync(u => u.Id == id);
        return exists;
    }

    public async Task<ClientApp?> GetByApiKeyAsync(string apiKey)
    {
        var record = await Context.ApiKeys.FirstOrDefaultAsync(u => u.Key == apiKey);
        if (record == null)
        {
            return null;
        }
        var clientApp = await Context.ClientApps.FirstOrDefaultAsync(u => u.Id == record.ClientAppId);
        if (clientApp == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<ClientAppDbModel, ClientApp>(clientApp);
        return dto;
    }

    public async Task<ClientApp?> GetByCodeAsync(string code)
    {
        var record = await Context.ClientApps.FirstOrDefaultAsync(u => u.Code == code);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<ClientAppDbModel, ClientApp>(record);
        return dto;
    }

}
