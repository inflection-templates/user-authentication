

namespace shala.api.domain.types;

public class UserPasswordLoginModel
{
    public string? UserName { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string? CountryCode { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Role { get; set; } = null!;
}

public class UserOtpLoginModel
{
    public string? UserName { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string? CountryCode { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public string Otp { get; set; } = null!;
    public string? Role { get; set; } = null!;
}

public class UserSendOtpModel
{
    public string? UserName { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string? CountryCode { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public string? Role { get; set; } = null!;
    public string Purpose { get; set; } = "Login";
    public OtpChannelPreference PreferredChannel { get; set; } = OtpChannelPreference.SMS;
}

public class UserLoginResult
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime Expires { get; set; }
    public User User { get; set; } = null!;
    public string Role { get; set; } = null!;
    public Guid RoleId { get; set; } = Guid.Empty;
}

public class UserResetPasswordSendLinkModel
{
    public string Email { get; set; } = null!;
}

public class UserResetPasswordModel
{
    public string Email { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ResetToken { get; set; } = null!;
}

public class UserChangePasswordModel
{
    public string? UserName { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string? CountryCode { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public string OldPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

public class UserRefreshTokenModel
{
    public string RefreshToken { get; set; } = null!;
}

public class UserTotpValidationModel
{
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public string TotpCode { get; set; } = null!;
}
