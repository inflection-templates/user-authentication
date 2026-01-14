using FluentValidation;
using BC = BCrypt.Net.BCrypt;
using shala.api.common;
using shala.api.domain.types;
using shala.api.services;
using shala.api.startup;
using QRCoder;
using OtpNet;
using System.IdentityModel.Tokens.Jwt;

namespace shala.api;

public class MfaAuthController
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly IUserAuthService _userAuthService;
    private readonly IUserAuthProfileService _userAuthProfileService;
    private readonly IRoleService _roleService;
    private readonly IValidator<UserTotpValidationModel> _totpValidator;
    private readonly ILogger<MfaAuthController> _logger;

    public MfaAuthController(
                        IConfiguration Configuration,
                        IUserService UserService,
                        IUserAuthService UserAuthService,
                        IUserAuthProfileService UserAuthProfileService,
                        IRoleService roleService,
                        IValidator<UserTotpValidationModel> totpValidator,
                        ILogger<MfaAuthController> logger)
    {
        _configuration = Configuration;
        _userService = UserService;
        _userAuthService = UserAuthService;
        _userAuthProfileService = UserAuthProfileService;
        _roleService = roleService;
        _totpValidator = totpValidator;
        _logger = logger;
    }

    public async Task<IResult> EnableMfa(HttpContext context)
    {
        try
        {
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null || currentUser.UserId == Guid.Empty)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }
            var userId = currentUser.UserId;
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return ResponseHandler.NotFound("User not found");
            }
            var mfaEnabled = await _userAuthProfileService.GetMfaEnabledAsync(userId);
            if (mfaEnabled)
            {
                return ResponseHandler.BadRequest("Mfa already enabled");
            }
            var mfaEnabledUpdated = await _userAuthProfileService.SetMfaEnabledAsync(userId, true);
            if (!mfaEnabledUpdated)
            {
                return ResponseHandler.BadRequest("Mfa not enabled");
            }
            return ResponseHandler.Ok("Mfa enabled successfully");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> GenerateTotpQRCode(HttpContext context)
    {
        try
        {
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null || currentUser.UserId == Guid.Empty)
            {
                return ResponseHandler.Unauthorized("Unauthorized access");
            }
            var userId = currentUser.UserId;
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return ResponseHandler.NotFound("User not found");
            }
            var totpSecret = await _userAuthProfileService.GetTotpSecretAsync(userId);
            if (totpSecret == null)
            {
                totpSecret = Helper.GenerateTotpSecret();
                var secretUpdated = await _userAuthProfileService.UpdateTotpSecretAsync(userId, totpSecret);
                if (!secretUpdated)
                {
                    return ResponseHandler.BadRequest("Totp secret not updated");
                }
            }
            var uri = GenerateQrCodeUri(user.UserName, totpSecret);
            var qrCodeBase64Png = GenerateQRCodeBuffer(uri);
            return Results.File(new MemoryStream(qrCodeBase64Png), "image/png");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> ValidateTotp(HttpContext context, UserTotpValidationModel model)
    {
        try
        {
            var validationResult = await _totpValidator.ValidateAsync(model);
            if (!validationResult.IsValid) {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }

            var userId = model.UserId;
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return ResponseHandler.NotFound("User not found");
            }
            var totpSecret = await _userAuthProfileService.GetTotpSecretAsync(userId);
            if (string.IsNullOrEmpty(totpSecret))
            {
                return ResponseHandler.BadRequest("Totp secret not found");
            }
            var isValid = VerifyTOTP(totpSecret, model.TotpCode);
            if (!isValid)
            {
                return ResponseHandler.BadRequest("Invalid otp");
            }

            var session = await _userAuthService.GetSessionAsync(model.SessionId);
            if (session == null)
            {
                return ResponseHandler.BadRequest("Session not found");
            }

            var roleName = DefaultRoles.User.ToString();
            var roleId = session.SessionRoleId;
            if (roleId != null && roleId != Guid.Empty)
            {
                var role = await _roleService.GetByIdAsync((Guid)roleId);
                if (role != null)
                {
                    roleName = role.Name;
                }
            }

            var token = UserAuthenticationHandler.GenerateAuthToken(this._configuration, user, session.Id, roleName);
            var refreshToken = UserAuthenticationHandler.GenerateRefreshToken(this._configuration, user, roleName);

            var fullName = (user.FirstName ?? "") + " " + (user.LastName ?? "");
            fullName = fullName.Trim();
            var res = new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshToken),
                Expiration = token.ValidTo,
                UserId = user.Id,
                UserName = user.UserName,
                SessionId = session.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = fullName,
                RoleName = roleName,
                ValidTill = session.ValidTill,
            };
            return ResponseHandler.Ok($"User {user.UserName} logged in successfully!", res);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error validating totp: {ex.Message}");
            _logger.LogError(ex.StackTrace);
            return ResponseHandler.ControllerException(ex);
        }
    }

    private string GenerateQrCodeUri(string username, string secret)
    {
        var platformInfo = PlatformInfoHandler.GetPlatformInfo();
        var issuer = platformInfo?.Platform ?? "Shala";
        issuer = issuer.Replace(" ", string.Empty);
        return $"otpauth://totp/{username}?secret={secret}&issuer={issuer}";
    }

    string GenerateQrCode(string uri)
    {
        // using var qrGenerator = new QRCodeGenerator();
        // using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        // using var qrCode = new PngByteQRCode(qrCodeData);
        // var qrCodeImage = qrCode.GetGraphic(20);
        // OR
        var qrCodeImage = PngByteQRCodeHelper.GetQRCode(uri, QRCodeGenerator.ECCLevel.Q, 20);

        return Convert.ToBase64String(qrCodeImage);
    }

    byte[] GenerateQRCodeBuffer(string uri)
    {
        var qrCodeImage = PngByteQRCodeHelper.GetQRCode(uri, QRCodeGenerator.ECCLevel.Q, 20);
        return qrCodeImage;
    }

    bool VerifyTOTP(string secret, string totpCode)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        return totp.VerifyTotp(totpCode, out _);
    }

}
