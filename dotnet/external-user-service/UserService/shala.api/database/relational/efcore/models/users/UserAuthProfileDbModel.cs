using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using shala.api.database.interfaces.models;

namespace shala.api.database.relational.efcore;

public class UserAuthProfileDbModel : IDbModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid? Id { get; set; }

    [Required]
    public Guid UserId { get; set; } = Guid.Empty;

    // Password related fields

    public string? PasswordHash { get; set; } = string.Empty;

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

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }

}
