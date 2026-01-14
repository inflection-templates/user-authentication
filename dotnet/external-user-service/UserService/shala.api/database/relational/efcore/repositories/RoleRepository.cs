using Microsoft.EntityFrameworkCore;
using shala.api.database.interfaces;
using shala.api.database.mappers;
using shala.api.domain.types;

namespace shala.api.database.relational.efcore;

public class RoleRepository : IRoleRepository
{
    #region Construction

    public IConfiguration Configuration { get; }
    private DatabaseContext Context { get; set; }
    private readonly ILogger<RoleRepository> _logger;

    public RoleRepository(IConfiguration configuration, DatabaseContext context, ILogger<RoleRepository> logger)
    {
        Configuration = configuration;
        Context = context;
        _logger = logger;
    }

    #endregion

    public async Task<Role?> GetByCodeAsync(string code)
    {
        var record = await Context.Roles.FirstOrDefaultAsync(u => u.Code == code);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<RoleDbModel, Role>(record);
        return dto;
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        var record = await Context.Roles.FirstOrDefaultAsync(u => u.Name == name);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<RoleDbModel, Role>(record);
        return dto;
    }

    public async Task<Role?> CreateAsync(RoleCreateModel item)
    {
        var model = new RoleDbModel
        {
            Code = item.Code ?? item.Name,
            Name = item.Name,
            Description = item.Description,
            TenantId = item.TenantId,
            IsDefaultRole = item.IsDefaultRole
        };

        var record = await Context.Roles.AddAsync(model);
        var recordsAdded = await Context.SaveChangesAsync();
        if (recordsAdded == 0)
        {
            throw new Exception("Role not created");
        }
        return new Role
        {
            Id = record.Entity.Id ?? Guid.Empty,
            Code = record.Entity.Code ?? record.Entity.Name,
            Name = record.Entity.Name,
            Description = record.Entity.Description,
            TenantId = record.Entity.TenantId,
            IsDefaultRole = record.Entity.IsDefaultRole
        };
    }

    public async Task<SearchResults<Role>> SearchAsync(RoleSearchFilters filters)
    {
        var query = Context.Roles.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            query = query.Where(r => r.Name.Contains(filters.Name));
        }
        if (!string.IsNullOrWhiteSpace(filters.Code))
        {
            query = query.Where(r => r.Code != null && r.Code.Contains(filters.Code));
        }
        if (filters.TenantId != null)
        {
            query = query.Where(r => r.TenantId == filters.TenantId);
        }

        var orderBy = filters.OrderBy ?? "Name";
        query = filters.Order == SortOrder.Ascending ?
            query.OrderBy(e => EF.Property<object>(e, orderBy)) :
            query.OrderByDescending(e => EF.Property<object>(e, orderBy));

        var totalRecordsCount = await query.CountAsync();
        var offset = filters.PageIndex * filters.ItemsPerPage;
        var records = await query.Skip(offset).Take(filters.ItemsPerPage).ToListAsync();
        var dtos = records.Select(r => {
            var dto = ModelMapper.Map<RoleDbModel, Role>(r);
            return dto;
        });

        var results = new SearchResults<Role>
        {
            Items = dtos,
            ItemsPerPage = filters.ItemsPerPage,
            PageIndex = filters.PageIndex,
            RetrievedCount = dtos.Count(),
            TotalCount = totalRecordsCount,
            TotalPages = (int)Math.Ceiling((double)totalRecordsCount / filters.ItemsPerPage)
        };
        return results;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await Context.Roles.AnyAsync(r => r.Id == id);
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        var record = await Context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<RoleDbModel, Role>(record);
        return dto;
    }

    public async Task<Role?> UpdateAsync(Guid id, RoleUpdateModel item)
    {
        var record = await Context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (record == null)
        {
            throw new Exception("Role not found");
        }
        if (item.Name != null)
        {
            record.Name = item.Name;
        }
        if (item.Description != null)
        {
            record.Description = item.Description;
        }
        if (item.IsDefaultRole != null)
        {
            record.IsDefaultRole = item.IsDefaultRole.Value;
        }
        if (item.TenantId != null && item.TenantId.HasValue)
        {
            record.TenantId = item.TenantId;
        }
        Context.Roles.Update(record);
        var recordsUpdated = await Context.SaveChangesAsync();
        if (recordsUpdated == 0)
        {
            throw new Exception("Role not updated");
        }
        var dto = ModelMapper.Map<RoleDbModel, Role>(record);
        return dto;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var record = await Context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (record == null)
        {
            throw new Exception("Role not found");
        }
        Context.Roles.Remove(record);
        var recordsDeleted = await Context.SaveChangesAsync();
        return recordsDeleted > 0;
    }

}
