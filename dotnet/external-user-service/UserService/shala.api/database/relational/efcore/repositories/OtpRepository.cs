using Microsoft.EntityFrameworkCore;
using shala.api.database.interfaces;
using shala.api.database.mappers;
using shala.api.domain.types;

namespace shala.api.database.relational.efcore;

public class OtpRepository : IOtpRepository
{
    #region Construction

    public IConfiguration Configuration { get; }
    private DatabaseContext Context { get; set; }
    private readonly ILogger<OtpRepository> _logger;

    public OtpRepository(IConfiguration configuration, DatabaseContext context, ILogger<OtpRepository> logger)
    {
        Configuration = configuration;
        Context = context;
        _logger = logger;
    }

    #endregion

    public async Task<Otp?> CreateAsync(Guid userId, string otp, string purpose)
    {
        var model = new OtpDbModel
        {
            UserId = userId,
            OtpCode = otp,
            Purpose = purpose,
            ValidFrom = DateTime.Now,
            ValidTill = DateTime.Now.AddMinutes(5),
        };

        var record = await Context.Otps.AddAsync(model);
        var recordsAdded = await Context.SaveChangesAsync();
        if (recordsAdded == 0)
        {
            throw new Exception("Otp not created");
        }
        return new Otp
        {
            Id = record.Entity.Id ?? Guid.Empty,
            UserId = record.Entity.UserId,
            OtpCode = record.Entity.OtpCode,
            Purpose = record.Entity.Purpose,
            ValidFrom = record.Entity.ValidFrom,
            ValidTill = record.Entity.ValidTill,
        };
    }

    public async Task<Otp?> GetByOtpAsync(Guid userId, string otp)
    {
        var record = await Context.Otps.FirstOrDefaultAsync(
            o => o.UserId == userId && o.OtpCode == otp);
        if (record == null)
        {
            throw new Exception("Otp not found");
        }
        return new Otp
        {
            Id = record.Id ?? Guid.Empty,
            UserId = record.UserId,
            OtpCode = record.OtpCode,
            Purpose = record.Purpose,
            ValidFrom = record.ValidFrom,
            ValidTill = record.ValidTill,
        };
    }
}
