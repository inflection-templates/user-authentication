namespace shala.api.domain.types;

public class LoginResponse
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string UserName { get; set; } = null!;
    public Guid SessionId { get; set; } = Guid.Empty;
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime? Expiration { get; set; } = null!;
    public string? FirstName { get; set; } = null!;
    public string? LastName { get; set; } = null!;
    public string? FullName { get; set; } = null!;
    public string? RoleName { get; set; } = null!;
    public DateTime? ValidTill { get; set; } = DateTime.MinValue;
}

public class MfaChallengeResponse
{
    public Guid UserId { get; set; } = Guid.Empty;
    public Guid SessionId { get; set; } = Guid.Empty;
    public bool MfaEnabled { get; set; } = true;
    public string? MfaType { get; set; } = null!;
}
