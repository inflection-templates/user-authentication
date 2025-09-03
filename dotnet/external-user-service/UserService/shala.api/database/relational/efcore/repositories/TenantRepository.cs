using Microsoft.EntityFrameworkCore;
using shala.api.database.interfaces;
using shala.api.database.mappers;
using shala.api.domain.types;

namespace shala.api.database.relational.efcore;

public class TenantRepository : ITenantRepository
{
    #region Construction

    public IConfiguration Configuration { get; }
    private DatabaseContext Context { get; set; }
    private readonly ILogger<TenantRepository> _logger;

    public TenantRepository(IConfiguration configuration, DatabaseContext context, ILogger<TenantRepository> logger)
    {
        Configuration = configuration;
        Context = context;
        _logger = logger;
    }

    #endregion

    public async Task<Tenant?> GetByCodeAsync(string code)
    {
        var record = await Context.Tenants.FirstOrDefaultAsync(u => u.Code == code);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<TenantDbModel, Tenant>(record);
        return dto;
    }

    public async Task<Tenant?> GetByEmailAsync(string email)
    {
        var record = await Context.Tenants.FirstOrDefaultAsync(u => u.Email == email);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<TenantDbModel, Tenant>(record);
        return dto;
    }

    public async Task<Tenant?> CreateAsync(TenantCreateModel item)
    {
        var tenant = ModelMapper.Map<TenantCreateModel, TenantDbModel>(item);
        var record = await Context.Tenants.AddAsync(tenant);
        var recordsAdded = await Context.SaveChangesAsync();
        if (recordsAdded == 0)
        {
            throw new Exception("Tenant not created");
        }
        var dto = ModelMapper.Map<TenantDbModel, Tenant>(record.Entity);
        return dto;
    }

    public async Task<SearchResults<Tenant>> SearchAsync(TenantSearchFilters filters)
    {
        var query = Context.Tenants.AsQueryable();
        if (filters.Name != null)
        {
            query = query.Where(u => EF.Functions.Like(u.Name, $"%{filters.Name}%"));
        }
        if (filters.Code != null)
        {
            query = query.Where(u => u.Code != null && u.Code.Contains(filters.Code));
        }
        if (filters.Email != null)
        {
            query = query.Where(u => EF.Functions.Like(u.Email, $"%{filters.Email}%"));
        }
        if (filters.PhoneNumber != null)
        {
            query = query.Where(u => EF.Functions.Like(u.PhoneNumber, $"%{filters.PhoneNumber}%"));
        }

        var orderBy = filters.OrderBy ?? "Name";
        query = filters.Order == SortOrder.Ascending ?
            query.OrderBy(e => EF.Property<object>(e, orderBy)) :
            query.OrderByDescending(e => EF.Property<object>(e, orderBy));

        var totalRecords = await query.CountAsync();
        var offset = filters.PageIndex * filters.ItemsPerPage;
        var records = await query.Skip(offset).Take(filters.ItemsPerPage).ToListAsync();
        var dtos = records.Select(u => ModelMapper.Map<TenantDbModel, Tenant>(u)).ToList();
        var results = new SearchResults<Tenant>
        {
            Items = dtos,
            ItemsPerPage = filters.ItemsPerPage,
            PageIndex = filters.PageIndex,
            RetrievedCount = records.Count,
            TotalCount = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / filters.ItemsPerPage)
        };
        return results;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await Context.Tenants.AnyAsync(u => u.Id == id);
    }

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        var record = await Context.Tenants.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<TenantDbModel, Tenant>(record);
        return dto;
    }

    public async Task<Tenant?> UpdateAsync(Guid id, TenantUpdateModel item)
    {
        var record = await Context.Tenants.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            throw new Exception("Tenant not found");
        }
        if (item.Name != null)
        {
            record.Name = item.Name;
        }
        if (item.Code != null)
        {
            record.Code = item.Code;
        }
        if (item.Email != null)
        {
            record.Email = item.Email;
        }
        if (item.PhoneNumber != null)
        {
            record.PhoneNumber = item.PhoneNumber;
        }

        Context.Tenants.Update(record);
        var recordsUpdated = await Context.SaveChangesAsync();
        if (recordsUpdated == 0)
        {
            throw new Exception("Tenant not updated");
        }
        var dto = ModelMapper.Map<TenantDbModel, Tenant>(record);
        return dto;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var record = await Context.Tenants.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            throw new Exception("Tenant not found");
        }
        Context.Tenants.Remove(record);
        var recordsDeleted = await Context.SaveChangesAsync();
        if (recordsDeleted == 0)
        {
            throw new Exception("Tenant not deleted");
        }
        return true;
    }

}
