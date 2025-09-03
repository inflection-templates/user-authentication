using shala.api.common;
using shala.api.database.interfaces;
using shala.api.database.mappers;
using shala.api.domain.types;

namespace shala.api.database.relational.efcore;

public class SessionRepository : ISessionRepository
{
    #region Construction

    public IConfiguration Configuration { get; }
    private DatabaseContext Context { get; set; }
    private readonly ILogger<SessionRepository> _logger;

    public SessionRepository(
        IConfiguration configuration,
        DatabaseContext context,
        ILogger<SessionRepository> logger)
    {
        Configuration = configuration;
        Context = context;
        _logger = logger;
    }

    #endregion

    public async Task<Session?> CreateAsync(Guid userId)
    {
        var sessionExpiryDuration = Configuration.GetValue<int>("LoginSession:SessionTimeoutInDays");
        var model = new SessionDbModel
        {
            UserId = userId,
            ValidTill = DateTime.Now.AddDays(sessionExpiryDuration),
        };
        var record = await Context.Sessions.AddAsync(model);
        var recordsAdded = await Context.SaveChangesAsync();
        if (recordsAdded == 0)
        {
            throw new Exception("Session not created");
        }

        var dto = ModelMapper.Map<SessionDbModel, Session>(record.Entity);
        return dto;
    }

    public async Task<Session?> CreateAsync(SessionCreateModel model)
    {
        var sessionExpiryDuration = Configuration.GetValue<int>("LoginSession:SessionTimeoutInDays");
        var session = new SessionDbModel
        {
            UserId = model.UserId,
            IsActive = true,
            SessionRoleId = model.SessionRoleId,
            ValidTill = DateTime.Now.AddDays(sessionExpiryDuration),

            AuthenticationMethod = model.AuthenticationMethod,
            OAuthProvider = model.OAuthProvider,

            MfaEnabled = model.MfaEnabled,
            MfaType = model.MfaType,
            MfaAuthenticated = model.MfaAuthenticated,

            UserAgent = model.UserAgent,
            IpAddress = model.IpAddress,
            ClientAppId = model.ClientAppId,

            StartedAt = DateTime.UtcNow,
            LoggedOutAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        var record = await Context.Sessions.AddAsync(session);
        var recordsAdded = await Context.SaveChangesAsync();
        if (recordsAdded == 0)
        {
            throw new Exception("Session not created");
        }

        var dto = ModelMapper.Map<SessionDbModel, Session>(record.Entity);
        return dto;
    }

    public async Task<Session?> GetByIdAsync(Guid sessionId)
    {
        var record = await Context.Sessions.FindAsync(sessionId);
        if (record == null)
        {
            return null;
        }
        var dto = ModelMapper.Map<SessionDbModel, Session>(record);
        return dto;
    }

    public async Task<bool> DeleteAsync(Guid sessionId)
    {
        var record = await Context.Sessions.FindAsync(sessionId);
        if (record == null)
        {
            return false;
        }
        Context.Sessions.Remove(record);
        var recordsDeleted = await Context.SaveChangesAsync();
        return recordsDeleted > 0;
    }

    public async Task<Session?> UpdateMfaAuthenticatedAsync(Guid sessionId, bool mfaAuthenticated)
    {
        var record = await Context.Sessions.FindAsync(sessionId);
        if (record == null)
        {
            return null;
        }
        record.MfaAuthenticated = mfaAuthenticated;
        Context.Sessions.Update(record);
        var recordsUpdated = await Context.SaveChangesAsync();
        if (recordsUpdated == 0)
        {
            throw new Exception("Session not updated");
        }
        var dto = ModelMapper.Map<SessionDbModel, Session>(record);
        return dto;
    }

    public async Task<bool> LogoutAsync(Guid sessionId)
    {
        var record = await Context.Sessions.FindAsync(sessionId);
        if (record == null)
        {
            return false;
        }
        record.IsActive = false;
        record.UpdatedAt = DateTime.UtcNow;
        Context.Sessions.Update(record);
        var recordsUpdated = await Context.SaveChangesAsync();
        return recordsUpdated > 0;
    }
}
