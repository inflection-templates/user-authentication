namespace shala.api.domain.types;

public class UserAuthProfile
{
    public Guid? Id { get; set; }
    public Guid UserId { get; set; } = Guid.Empty;

    // Password related fields
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime? PasswordLastRotatedAt { get; set; } = null!;

    // MFA related fields
    public bool MfaEnabled { get; set; } = false;
    public string MfaType { get; set; } = "TOTP"; // TOTP, SMS, EMAIL
    public string TotpSecret { get; set; } = string.Empty;
    public DateTime? TotpSecretLastRotatedAt { get; set; } = null!;
    public string? RecoveryCode { get; set; } = null!;

    // OAuth related fields
    public bool HasSignedUpWithOAuth { get; set; } = false;
    public string? OAuthProvider { get; set; } = null!;

    // Email and phone verification related fields
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;

    // Session related fields
    public DateTime? LastLoginAt { get; set; } = null!;
    public string? LastLoginMetadata { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
