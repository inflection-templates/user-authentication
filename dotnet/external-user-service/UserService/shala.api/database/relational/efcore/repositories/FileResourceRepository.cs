using Microsoft.EntityFrameworkCore;
using shala.api.database.interfaces;
using shala.api.database.mappers;
using shala.api.domain.types;

namespace shala.api.database.relational.efcore;

public class FileResourceRepository : IFileResourceRepository
{
    #region Construction

    public IConfiguration Configuration { get; }
    private DatabaseContext Context { get; set; }
    private readonly ILogger<FileResourceRepository> _logger;

    public FileResourceRepository(IConfiguration configuration, DatabaseContext context, ILogger<FileResourceRepository> logger)
    {
        Configuration = configuration;
        Context = context;
        _logger = logger;
    }

    #endregion

    public async Task<FileResource?> CreateAsync(FileResourceCreateModel model)
    {
        var fileResource = ModelMapper.Map<FileResourceCreateModel, FileResourceDbModel>(model);
        var record = await Context.FileResources.AddAsync(fileResource);
        var recordsAdded = await Context.SaveChangesAsync();
        if (recordsAdded == 0)
        {
            throw new Exception("File Resource not created");
        }
        var dto = ModelMapper.Map<FileResourceDbModel, FileResource>(record.Entity);
        return dto;
    }

    public async Task<FileResource?> GetByIdAsync(Guid id)
    {
        var record = await Context.FileResources.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<FileResourceDbModel, FileResource>(record);
        return dto;
    }

    public async Task<SearchResults<FileResource>> SearchAsync(FileResourceSearchFilters filters)
    {
        var query = Context.FileResources.AsQueryable();
        if (filters.OwnerUserId != Guid.Empty && filters.OwnerUserId != null)
        {
            query = query.Where(u => u.OwnerUserId == filters.OwnerUserId);
        }
        if (filters.TenantId != Guid.Empty && filters.TenantId != null)
        {
            query = query.Where(u => u.TenantId == filters.TenantId);
        }
        if (!string.IsNullOrWhiteSpace(filters.FileName))
        {
            query.Where(u => EF.Functions.Like(u.FileName, $"%{filters.FileName}%"));
        }

        var orderBy = filters.OrderBy ?? "CreatedAt";
        query = filters.Order == SortOrder.Ascending ?
            query.OrderBy(e => EF.Property<object>(e, orderBy)) :
            query.OrderByDescending(e => EF.Property<object>(e, orderBy));

        var totalRecordsCount = await query.CountAsync();
        var offset = filters.PageIndex * filters.ItemsPerPage;
        var records = await query.Skip(offset).Take(filters.ItemsPerPage).ToListAsync();
        var dtos = records.Select(ModelMapper.Map<FileResourceDbModel, FileResource>).ToList();
        var results = new SearchResults<FileResource>
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

    public async Task<bool> DeleteAsync(Guid id)
    {
        var record = await Context.FileResources.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            throw new Exception("File Resource not found");
        }
        Context.FileResources.Remove(record);
        var recordsDeleted = await Context.SaveChangesAsync();
        return recordsDeleted > 0;
    }
}
