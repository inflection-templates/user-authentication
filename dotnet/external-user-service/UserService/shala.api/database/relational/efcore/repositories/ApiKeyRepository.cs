using Microsoft.EntityFrameworkCore;
using shala.api.database.interfaces;
using shala.api.database.mappers;
using shala.api.domain.types;

namespace shala.api.database.relational.efcore;

public class ApiKeyRepository : IApiKeyRepository
{
    #region Construction

    public IConfiguration Configuration { get; }
    private DatabaseContext Context { get; set; }
    private readonly ILogger<ApiKeyRepository> _logger;

    public ApiKeyRepository(IConfiguration configuration, DatabaseContext context, ILogger<ApiKeyRepository> logger)
    {
        Configuration = configuration;
        Context = context;
        _logger = logger;
    }

    #endregion

    public async Task<List<ApiKey>> GetByClientAppIdAsync(Guid clientAppId)
    {
        var records = await Context.ApiKeys.Where(u => u.ClientAppId == clientAppId).ToListAsync();
        var dtos = ModelMapper.Map<List<ApiKeyDbModel>, List<ApiKey>>(records);
        return dtos;
    }

    public async Task<ApiKey?> GetByKeyAsync(string key)
    {
        var record = await Context.ApiKeys.FirstOrDefaultAsync(u => u.Key == key);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<ApiKeyDbModel, ApiKey>(record);
        return dto;
    }

    public async Task<string?> GetSecretHashAsync(string key)
    {
        var record = await Context.ApiKeys.FirstOrDefaultAsync(u => u.Key == key);
        if (record == null)
        {
            return null;
        }
        return record.SecretHash;
    }

    public async Task<ApiKey?> CreateAsync(ApiKeyCreateModel item)
    {
        var apiKey = ModelMapper.Map<ApiKeyCreateModel, ApiKeyDbModel>(item);
        var record = await Context.ApiKeys.AddAsync(apiKey);
        var recordsAdded = await Context.SaveChangesAsync();
        if (recordsAdded == 0)
        {
            throw new Exception("Api Key not created");
        }
        var dto = ModelMapper.Map<ApiKeyDbModel, ApiKey>(record.Entity);
        return dto;
    }

    public async Task<SearchResults<ApiKey>> SearchAsync(ApiKeySearchFilters filters)
    {
        var query = Context.ApiKeys.AsQueryable();
        if (filters.ClientAppId != Guid.Empty)
        {
            query = query.Where(u => u.ClientAppId == filters.ClientAppId);
        }
        if (filters.IsActive != null)
        {
            query = query.Where(u => u.IsActive == filters.IsActive);
        }
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            query.Where(u => EF.Functions.Like(u.Name, $"%{filters.Name}%"));
        }

        var orderBy = filters.OrderBy ?? "Name";
        query = filters.Order == SortOrder.Ascending ?
            query.OrderBy(e => EF.Property<object>(e, orderBy)) :
            query.OrderByDescending(e => EF.Property<object>(e, orderBy));

        var totalRecordsCount = query.Count();
        var offset = filters.PageIndex * filters.ItemsPerPage;
        var records = await query.Skip(offset).Take(filters.ItemsPerPage).ToListAsync();
        var dtos = ModelMapper.Map<IEnumerable<ApiKeyDbModel>, IEnumerable<ApiKey>>(records);
        var results = new SearchResults<ApiKey>
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
        var exists = await Context.ApiKeys.AnyAsync(u => u.Id == id);
        return exists;
    }

    public async Task<ApiKey?> GetByIdAsync(Guid id)
    {
        var record = await Context.ApiKeys.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<ApiKeyDbModel, ApiKey>(record);
        return dto;
    }

    public async Task<ApiKey?> UpdateAsync(Guid id, ApiKeyUpdateModel item)
    {
        var record = await Context.ApiKeys.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            return null;
        }
        if (item.Name != null)
        {
            record.Name = item.Name;
        }
        if (item.Description != null)
        {
            record.Description = item.Description;
        }
        if (item.ValidTill != null)
        {
            // Default to 90 days if not provided
            var DAYS = 90;
            record.ValidTill = item.ValidTill ?? DateTime.Now.AddDays(DAYS);
        }

        Context.ApiKeys.Update(record);
        var recordsUpdated = await Context.SaveChangesAsync();
        if (recordsUpdated == 0)
        {
            throw new Exception("Api Key not updated");
        }
        var dto = ModelMapper.Map<ApiKeyDbModel, ApiKey>(record);
        return dto;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var record = await Context.ApiKeys.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            throw new Exception("Api Key not found");
        }
        Context.ApiKeys.Remove(record);
        var recordsDeleted = await Context.SaveChangesAsync();
        return recordsDeleted > 0;
    }

    public async Task<bool> DeleteByClientAppIdAsync(Guid clientAppId)
    {
        Context.ApiKeys.RemoveRange(
            Context.ApiKeys.Where(u => u.ClientAppId == clientAppId)
        );
        var recordsDeleted = await Context.SaveChangesAsync();
        return recordsDeleted == 0;
    }

}
