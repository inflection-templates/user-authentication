using Microsoft.EntityFrameworkCore;
using shala.api.common;
using shala.api.database.interfaces;
using shala.api.database.mappers;
using shala.api.domain.types;

namespace shala.api.database.relational.efcore;

public class UserRepository : IUserRepository
{
    #region Construction

    public IConfiguration Configuration { get; }
    private DatabaseContext Context { get; set; }
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IConfiguration configuration, DatabaseContext context, ILogger<UserRepository> logger)
    {
        Configuration = configuration;
        Context = context;
        _logger = logger;
    }

    #endregion

    public async Task<User?> CreateAsync(UserCreateModel model)
    {
        try
        {
            var user = new UserDbModel
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                CountryCode = model.CountryCode,
                PhoneNumber = model.PhoneNumber,
                TenantId = model.TenantId,
                UserName = model.UserName ?? Helper.GenerateRandomString(8),
            };
            var record = await Context.Users.AddAsync(user);
            var recordsAdded = await Context.SaveChangesAsync();
            if (recordsAdded == 0)
            {
                throw new Exception("User not created");
            }

            return new User
            {
                Id = record.Entity.Id ?? Guid.Empty,
                UserName = record.Entity.UserName,
                FirstName = record.Entity.FirstName,
                LastName = record.Entity.LastName,
                Email = record.Entity.Email,
                CountryCode = record.Entity.CountryCode,
                PhoneNumber = record.Entity.PhoneNumber,
                TenantId = record.Entity.TenantId
            };
        }
        catch (Exception ex)
        {
           _logger.LogError($"User not created: {ex.Message}");
        }
        return null;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var record = await Context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            return null;
        }

        return new User
        {
            Id = record.Id ?? Guid.Empty,
            UserName = record.UserName,
            FirstName = record.FirstName,
            LastName = record.LastName,
            Email = record.Email,
            CountryCode = record.CountryCode,
            PhoneNumber = record.PhoneNumber,
            TenantId = record.TenantId
        };
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var record = await Context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (record == null)
        {
            return null;
        }
        return new User
        {
            Id = record.Id ?? Guid.Empty,
            UserName = record.UserName,
            FirstName = record.FirstName,
            LastName = record.LastName,
            Email = record.Email,
            CountryCode = record.CountryCode,
            PhoneNumber = record.PhoneNumber,
            TenantId = record.TenantId
        };
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        var record = await Context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        if (record == null)
        {
            return null;
        }

        return new User
        {
            Id = record.Id ?? Guid.Empty,
            UserName = record.UserName,
            FirstName = record.FirstName,
            LastName = record.LastName,
            Email = record.Email,
            CountryCode = record.CountryCode,
            PhoneNumber = record.PhoneNumber,
            TenantId = record.TenantId
        };
    }

    public async Task<User?> UpdateAsync(Guid id, UserUpdateModel model)
    {
        var record = await Context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            throw new Exception("User not found");
        }
        if (model.FirstName != null)
        {
            record.FirstName = model.FirstName;
        }
        if (model.LastName != null)
        {
            record.LastName = model.LastName;
        }
        if (model.Email != null)
        {
            record.Email = model.Email;
        }
        if (model.CountryCode != null)
        {
            record.CountryCode = model.CountryCode;
        }
        if (model.PhoneNumber != null)
        {
            record.PhoneNumber = model.PhoneNumber;
        }
        if (model.TenantId != Guid.Empty)
        {
            record.TenantId = model.TenantId;
        }
        Context.Users.Update(record);
        var recordsUpdated = await Context.SaveChangesAsync();
        if (recordsUpdated == 0)
        {
            throw new Exception("User not updated");
        }
        return new User
        {
            Id = record.Id ?? Guid.Empty,
            UserName = record.UserName,
            FirstName = record.FirstName,
            LastName = record.LastName,
            Email = record.Email,
            CountryCode = record.CountryCode,
            PhoneNumber = record.PhoneNumber,
            TenantId = record.TenantId
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var record = await Context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (record == null)
        {
            throw new Exception("User not found");
        }
        Context.Users.Remove(record);
        var recordsDeleted = await Context.SaveChangesAsync();
        if (recordsDeleted == 0)
        {
            throw new Exception("User not deleted");
        }
        return true;
    }

    public async Task<SearchResults<User>> SearchAsync(UserSearchFilters filters)
    {
        var query = Context.Users.AsQueryable();
        if (filters.FirstName != null)
        {
            query.Where(u => EF.Functions.Like(u.FirstName, $"%{filters.FirstName}%"));
        }
        if (filters.LastName != null)
        {
            query.Where(u => EF.Functions.Like(u.LastName, $"%{filters.LastName}%"));
        }
        if (filters.Email != null)
        {
            query.Where(u => EF.Functions.Like(u.Email, $"%{filters.Email}%"));
        }
        if (filters.CountryCode != null)
        {
            query.Where(u => u.CountryCode == filters.CountryCode);
        }
        if (filters.PhoneNumber != null)
        {
            query.Where(u => EF.Functions.Like(u.PhoneNumber, $"%{filters.PhoneNumber}%"));
        }
        if (filters.TenantId != null)
        {
            query = query.Where(u => u.TenantId == filters.TenantId);
        }

        var orderBy = filters.OrderBy ?? "CreatedAt";
        query = filters.Order == SortOrder.Ascending ?
            query.OrderBy(e => EF.Property<object>(e, orderBy)) :
            query.OrderByDescending(e => EF.Property<object>(e, orderBy));

        var totalRecordsCount = await query.CountAsync();
        var offset = filters.PageIndex * filters.ItemsPerPage;
        var records = await query.Skip(offset).Take(filters.ItemsPerPage).ToListAsync();
        var dtos = records.Select(r => {
            return new User
            {
                Id = r.Id ?? Guid.Empty,
                UserName = r.UserName,
                FirstName = r.FirstName,
                LastName = r.LastName,
                Email = r.Email,
                CountryCode = r.CountryCode,
                PhoneNumber = r.PhoneNumber,
                TenantId = r.TenantId
            };
        }).ToList();

        var results = new SearchResults<User>
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
        var exists = await Context.Users.AnyAsync(u => u.Id == id);
        return exists;
    }

    public async Task<User?> GetByPhoneAsync(string countryCode, string phoneNumber)
    {
        var record = await Context.Users.FirstOrDefaultAsync(
            u => u.CountryCode == countryCode &&
                 u.PhoneNumber == phoneNumber);
        if (record == null)
        {
            return null;
        }
        return new User
        {
            Id = record.Id ?? Guid.Empty,
            UserName = record.UserName,
            FirstName = record.FirstName,
            LastName = record.LastName,
            Email = record.Email,
            CountryCode = record.CountryCode,
            PhoneNumber = record.PhoneNumber,
            TenantId = record.TenantId
        };
    }

}
