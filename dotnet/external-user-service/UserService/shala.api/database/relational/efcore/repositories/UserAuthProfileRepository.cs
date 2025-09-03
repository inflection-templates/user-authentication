using Microsoft.EntityFrameworkCore;
using shala.api.common;
using shala.api.database.interfaces;
using shala.api.database.mappers;
using shala.api.domain.types;

namespace shala.api.database.relational.efcore;

public class UserAuthProfileRepository : IUserAuthProfileRepository
{
    #region Construction

    public IConfiguration Configuration { get; }
    private DatabaseContext Context { get; set; }
    private readonly ILogger<UserAuthProfileRepository> _logger;

    public UserAuthProfileRepository(
        IConfiguration configuration,
        DatabaseContext context,
        ILogger<UserAuthProfileRepository> logger)
    {
        Configuration = configuration;
        Context = context;
        _logger = logger;
    }

    #endregion

    public async Task<UserAuthProfile?> CreateUserAuthProfileAsync(Guid userId, string? passwordHash)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            var userAuthProfile = new UserAuthProfileDbModel
            {
                UserId = userId,
                TotpSecret = Helper.GenerateTotpSecret(),
                TotpSecretLastRotatedAt = DateTime.UtcNow,
                PasswordHash = passwordHash ?? string.Empty,
                PasswordLastRotatedAt = DateTime.UtcNow,
            };
            var recordAdded = await Context.UserAuthProfiles.AddAsync(userAuthProfile);
            var recordsAdded = await Context.SaveChangesAsync();
            if (recordsAdded == 0)
            {
                throw new Exception("User Auth Profile not created");
            }
            var dto = ModelMapper.Map<UserAuthProfileDbModel, UserAuthProfile>(recordAdded.Entity);
            return dto;
        }
        else
        {
            return ModelMapper.Map<UserAuthProfileDbModel, UserAuthProfile>(record);
        }
    }

    public async Task<UserAuthProfile?> GetUserAuthProfileAsync(Guid userId)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<UserAuthProfileDbModel, UserAuthProfile>(record);
        return dto;
    }

    public async Task<bool> UpdateHashedPasswordAsync(Guid userId, string hashedPassword)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User auth profile not found");
        }
        else
        {
            record.PasswordHash = hashedPassword;
            record.PasswordLastRotatedAt = DateTime.UtcNow;
            Context.UserAuthProfiles.Update(record);
            var recordsUpdated = await Context.SaveChangesAsync();
            if (recordsUpdated == 0)
            {
                throw new Exception("User password not updated");
            }
            return true;
        }
    }

    public async Task<string?> GetHashedPasswordAsync(Guid userId)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User not found");
        }
        return record.PasswordHash;
    }

    public async Task<bool> UpdateTotpSecretAsync(Guid userId, string secret)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User auth profile not found");
        }
        else
        {
            record.TotpSecret = secret;
            record.TotpSecretLastRotatedAt = DateTime.UtcNow;
            Context.UserAuthProfiles.Update(record);
            var recordsUpdated = await Context.SaveChangesAsync();
            if (recordsUpdated == 0)
            {
                throw new Exception("User Totp secret not updated");
            }
            return true;
        }
    }

    public async Task<string?> GetTotpSecretAsync(Guid userId)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User not found");
        }
        return record.TotpSecret;
    }

    public async Task<bool> GetMfaEnabledAsync(Guid userId)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User not found");
        }
        return record.MfaEnabled;
    }

    public async Task<bool> SetMfaEnabledAsync(Guid userId, bool enabled)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User auth profile not found");
        }
        else
        {
            record.MfaEnabled = enabled;
            Context.UserAuthProfiles.Update(record);
            var recordsUpdated = await Context.SaveChangesAsync();
            if (recordsUpdated == 0)
            {
                throw new Exception("User MFA not updated");
            }
            return true;
        }
    }

    public async Task<string?> GetPreferredMfaTypeAsync(Guid userId)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User auth profile not found");
        }
        else
        {
            return record.MfaType;
        }
    }

    public async Task<bool> SetPreferredMfaTypeAsync(Guid userId, string mfaType)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User auth profile not found");
        }
        else
        {
            record.MfaType = mfaType;
            Context.UserAuthProfiles.Update(record);
            var recordsUpdated = await Context.SaveChangesAsync();
            if (recordsUpdated == 0)
            {
                throw new Exception("User preferred MFA type not updated");
            }
            return true;
        }
    }

    public async Task<bool> GetSignedUpWithOAuthAsync(Guid userId)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User not found");
        }
        return record.HasSignedUpWithOAuth;
    }

    public async Task<bool> SetSignedUpWithOAuthAsync(Guid userId, bool signedUpWithOAuth, string? provider)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User auth profile not found");
        }

        record.HasSignedUpWithOAuth = signedUpWithOAuth;
        record.OAuthProvider = provider;
        Context.UserAuthProfiles.Update(record);
        var recordsUpdated = await Context.SaveChangesAsync();
        if (recordsUpdated == 0)
        {
            throw new Exception("User OAuth not updated");
        }
        return true;

    }

    public async Task<bool> GetEmailVerifiedAsync(Guid userId)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User not found");
        }
        return record.IsEmailVerified;
    }

    public async Task<bool> SetEmailVerifiedAsync(Guid userId, bool verified)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User auth profile not found");
        }
        else
        {
            record.IsEmailVerified = verified;
            Context.UserAuthProfiles.Update(record);
            var recordsUpdated = await Context.SaveChangesAsync();
            if (recordsUpdated == 0)
            {
                throw new Exception("User email not verified");
            }
            return true;
        }
    }

    public async Task<bool> GetPhoneVerifiedAsync(Guid userId)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User not found");
        }
        return record.IsPhoneVerified;
    }

    public async Task<bool> SetPhoneVerifiedAsync(Guid userId, bool verified)
    {
        var record = await Context.UserAuthProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        if (record == null)
        {
            throw new Exception("User auth profile not found");
        }
        else
        {
            record.IsPhoneVerified = verified;
            Context.UserAuthProfiles.Update(record);
            var recordsUpdated = await Context.SaveChangesAsync();
            if (recordsUpdated == 0)
            {
                throw new Exception("User phone not verified");
            }
            return true;
        }
    }

}
