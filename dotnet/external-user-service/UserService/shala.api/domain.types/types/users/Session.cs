
namespace shala.api.domain.types;

public class Session
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; } = Guid.Empty;
    public bool IsActive { get; set; } = true;
    public Guid? SessionRoleId { get; set; } = Guid.Empty;

    public string? AuthenticationMethod { get; set; } = null!;
    public string? OAuthProvider { get; set; } = null!;
    public bool MfaEnabled { get; set; } = false;
    public string? MfaType { get; set; } = null!;
    public bool MfaAuthenticated { get; set; } = false;

    public string? UserAgent { get; set; } = null!;
    public string? IpAddress { get; set; } = null!;
    public Guid? ClientAppId { get; set; } = Guid.Empty;

    public DateTime? StartedAt { get; set; } = null!;
    public DateTime? ValidTill { get; set; } = null!;
    public DateTime? LoggedOutAt { get; set; } = null!;

    public DateTime? CreatedAt { get; set; } = null!;
    public DateTime? UpdatedAt { get; set; } = null!;

}

public class SessionCreateModel
{
    public Guid UserId { get; set; } = Guid.Empty;
    public bool IsActive { get; set; } = true;
    public Guid? SessionRoleId { get; set; } = Guid.Empty;

    public string? AuthenticationMethod { get; set; } = null!;
    public string? OAuthProvider { get; set; } = null!;
    
    public bool MfaEnabled { get; set; } = false;
    public string? MfaType { get; set; } = null!;
    public bool MfaAuthenticated { get; set; } = false;

    public string? UserAgent { get; set; } = null!;
    public string? IpAddress { get; set; } = null!;
    public Guid? ClientAppId { get; set; } = Guid.Empty;

    public DateTime? StartedAt { get; set; } = null!;
    public DateTime? ValidTill { get; set; } = null!;
}
