using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using BC = BCrypt.Net.BCrypt;
using shala.api.common;
using shala.api.domain.types;
using shala.api.services;
using shala.api.modules.communication;
using shala.api.startup;

namespace shala.api;

public class UserAuthController
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly IUserAuthService _userAuthService;
    private readonly IUserAuthProfileService _userAuthProfileService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<UserPasswordLoginModel> _passwordLoginValidator;
    private readonly IValidator<UserOtpLoginModel> _otpLoginValidator;
    private readonly IValidator<UserSendOtpModel> _otpCreateRequestValidator;
    private readonly IValidator<UserResetPasswordSendLinkModel> _resetPasswordSendLinkValidator;
    private readonly IValidator<UserResetPasswordModel> _resetPasswordValidator;
    private readonly IValidator<UserChangePasswordModel> _changePasswordValidator;
    private readonly IValidator<UserRefreshTokenModel> _refreshTokenValidator;
    private readonly ILogger<UserAuthController> _logger;

    public UserAuthController(
                        IConfiguration Configuration,
                        IUserService UserService,
                        IUserAuthService UserAuthService,
                        IUserAuthProfileService UserAuthProfileService,
                        IEmailService emailService,
                        ISmsService smsService,
                        IJwtTokenService jwtTokenService,
                        IValidator<UserPasswordLoginModel> passwordLoginValidator,
                        IValidator<UserOtpLoginModel> otpLoginValidator,
                        IValidator<UserSendOtpModel> otpCreateRequestValidator,
                        IValidator<UserResetPasswordSendLinkModel> resetPasswordSendLinkValidator,
                        IValidator<UserResetPasswordModel> resetPasswordValidator,
                        IValidator<UserChangePasswordModel> changePasswordValidator,
                        IValidator<UserRefreshTokenModel> refreshTokenValidator,
                        ILogger<UserAuthController> logger)
    {
        _configuration = Configuration;
        _userService = UserService;
        _userAuthService = UserAuthService;
        _userAuthProfileService = UserAuthProfileService;
        _emailService = emailService;
        _smsService = smsService;
        _jwtTokenService = jwtTokenService;
        _passwordLoginValidator = passwordLoginValidator;
        _otpLoginValidator = otpLoginValidator;
        _otpCreateRequestValidator = otpCreateRequestValidator;
        _resetPasswordSendLinkValidator = resetPasswordSendLinkValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _changePasswordValidator = changePasswordValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _logger = logger;
    }

    public async Task<IResult> LoginWithPassword(HttpContext context, UserPasswordLoginModel model)
    {
        try {
            var validationResult = await _passwordLoginValidator.ValidateAsync(model);
            if (!validationResult.IsValid) {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }

            User? existingUser = null;
            if (!string.IsNullOrEmpty(model.Email)) {
                existingUser = await _userService.GetByEmailAsync(model.Email);
            }
            else if (!string.IsNullOrEmpty(model.UserName)) {
                existingUser = await _userService.GetByUsernameAsync(model.UserName);
            }
            else if (!string.IsNullOrEmpty(model.CountryCode) && !string.IsNullOrEmpty(model.PhoneNumber)) {
                existingUser = await _userService.GetByPhoneAsync(model.CountryCode, model.PhoneNumber);
            }
            else {
                return ResponseHandler.BadRequest("Invalid login credentials");
            }
            if (existingUser == null) {
                return ResponseHandler.NotFound("User not found");
            }

            var authenticated = await validatePassword(existingUser.Id, model.Password);
            if (!authenticated) {
                return ResponseHandler.Unauthorized("Invalid password");
            }

            var mfaEnabled = await _userAuthProfileService.GetMfaEnabledAsync(existingUser.Id);
            var preferredMfaType = await _userAuthProfileService.GetPreferredMfaTypeAsync(existingUser.Id);

            var sessionModel = new SessionCreateModel
            {
                UserId = existingUser.Id,
                AuthenticationMethod = "Password",
                MfaEnabled = mfaEnabled,
                MfaType = preferredMfaType,
                UserAgent = context.Request.Headers["User-Agent"],
                IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "",
                StartedAt = DateTime.UtcNow,
            };
            var session = await _userAuthService.CreateSessionAsync(existingUser.Id);
            if (session == null) {
                return ResponseHandler.InternalServerError("Session cannot be created.");
            }

            if (mfaEnabled) {
                return ResponseHandler.Ok("MFA Challenge requested", new {
                    UserId = existingUser.Id,
                    SessionId = session.Id,
                    MfaEnabled = true,
                    MfaType = preferredMfaType,
                });
            }

            var token = _jwtTokenService.GenerateToken(existingUser, session.Id, model.Role);
            var refreshToken = _jwtTokenService.GenerateToken(existingUser, session.Id, model.Role); // Using same service for refresh token

            var fullName = (existingUser.FirstName ?? "") + " " + (existingUser.LastName ?? "");
            fullName = fullName.Trim();
            var res = new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddDays(5), // 5 days validity
                UserId = existingUser.Id,
                UserName = existingUser.UserName,
                SessionId = session.Id,
                FirstName = existingUser.FirstName,
                LastName = existingUser.LastName,
                FullName = fullName,
                RoleName = model.Role,
                ValidTill = session.ValidTill,
            };
            return ResponseHandler.Ok($"User {existingUser.UserName} logged in successfully!", res);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> SendOtp(HttpContext context, UserSendOtpModel model)
    {
        try{
            var validationResult = await _otpCreateRequestValidator.ValidateAsync(model);
            if (!validationResult.IsValid) {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }

            User? existingUser = null;
            if (!string.IsNullOrEmpty(model.Email)) {
                existingUser = await _userService.GetByEmailAsync(model.Email);
            }
            else if (!string.IsNullOrEmpty(model.UserName)) {
                existingUser = await _userService.GetByUsernameAsync(model.UserName);
            }
            else if (!string.IsNullOrEmpty(model.CountryCode) && !string.IsNullOrEmpty(model.PhoneNumber)) {
                existingUser = await _userService.GetByPhoneAsync(model.CountryCode, model.PhoneNumber);
            }
            else {
                return ResponseHandler.BadRequest("Invalid login credentials");
            }
            if (existingUser == null) {
                return ResponseHandler.NotFound("User not found");
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var savedOtp = await _userAuthService.CreateOtpAsync(existingUser.Id, otp, model.Purpose);
            if (savedOtp == null) {
                return ResponseHandler.InternalServerError("OTP cannot be saved.");
            }

            if (!string.IsNullOrEmpty(existingUser.Email) &&
                (model.PreferredChannel == OtpChannelPreference.Email || model.PreferredChannel == OtpChannelPreference.SMSAndEmail)) {
                var sentEmail = await _emailService.SendEmailOtpAsync(existingUser, otp, model.Purpose);
                if (!sentEmail) {
                    return ResponseHandler.InternalServerError("OTP cannot be sent.");
                }
            }
            else if (!string.IsNullOrEmpty(existingUser.PhoneNumber) && !string.IsNullOrEmpty(existingUser.CountryCode) &&
                    (model.PreferredChannel == OtpChannelPreference.SMS || model.PreferredChannel == OtpChannelPreference.SMSAndEmail)) {
                var sentSms = await this._smsService.SendOtpAsync(existingUser, existingUser.CountryCode, existingUser.PhoneNumber, otp);
                if (!sentSms) {
                    return ResponseHandler.InternalServerError("OTP cannot be sent.");
                }
            }

            return ResponseHandler.Ok("OTP generated successfully", new {
                Email = existingUser.Email
            });
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> LoginWithOtp(HttpContext context, UserOtpLoginModel model)
    {
        try{
            var validationResult = await _otpLoginValidator.ValidateAsync(model);
            if (!validationResult.IsValid) {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }

            User? existingUser = null;
            if (!string.IsNullOrEmpty(model.Email)) {
                existingUser = await _userService.GetByEmailAsync(model.Email);
            }
            else if (!string.IsNullOrEmpty(model.UserName)) {
                existingUser = await _userService.GetByUsernameAsync(model.UserName);
            }
            else if (!string.IsNullOrEmpty(model.CountryCode) && !string.IsNullOrEmpty(model.PhoneNumber)) {
                existingUser = await _userService.GetByPhoneAsync(model.CountryCode, model.PhoneNumber);
            }
            else {
                return ResponseHandler.BadRequest("Invalid login credentials");
            }
            if (existingUser == null) {
                return ResponseHandler.NotFound("User not found");
            }

            if (string.IsNullOrEmpty(model.Otp)) {
                return ResponseHandler.BadRequest("Invalid OTP");
            }
            var otpRecord = await _userAuthService.GetOtpAsync(existingUser.Id, model.Otp);
            if (otpRecord == null) {
                return ResponseHandler.Unauthorized("Invalid OTP");
            }
            if (otpRecord.ValidTill < DateTime.UtcNow) {
                return ResponseHandler.Unauthorized("OTP expired");
            }
            if (otpRecord.Used == true) {
                return ResponseHandler.Unauthorized("OTP already used");
            }

            var session = await _userAuthService.CreateSessionAsync(existingUser.Id);
            if (session == null) {
                return ResponseHandler.InternalServerError("Session cannot be created.");
            }

            var token = _jwtTokenService.GenerateToken(existingUser, session.Id, model.Role);
            var refreshToken = _jwtTokenService.GenerateToken(existingUser, session.Id, model.Role); // Using same service for refresh token

            var fullName = (existingUser.FirstName ?? "") + " " + (existingUser.LastName ?? "");
            fullName = fullName.Trim();
            var res = new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddDays(5), // 5 days validity
                UserId = existingUser.Id,
                UserName = existingUser.UserName,
                SessionId = session.Id,
                FirstName = existingUser.FirstName,
                LastName = existingUser.LastName,
                FullName = fullName,
                RoleName = model.Role,
                ValidTill = session.ValidTill,
            };
            return ResponseHandler.Ok($"User {existingUser.UserName} logged in successfully!", res);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> ResetPasswordSendLink(HttpContext context, UserResetPasswordSendLinkModel model)
    {
        try{
            var validationResult = await _resetPasswordSendLinkValidator.ValidateAsync(model);
            if (!validationResult.IsValid) {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }

            User? existingUser = await _userService.GetByEmailAsync(model.Email);
            if (existingUser == null) {
                return ResponseHandler.NotFound("User not found");
            }

            var resetToken = UserAuthenticationHandler.GenerateResetToken(_configuration, existingUser.Id, existingUser.Email);
            if (string.IsNullOrEmpty(resetToken)) {
                return ResponseHandler.InternalServerError("Reset token cannot be created.");
            }

            if (!string.IsNullOrEmpty(existingUser.Email)) {
                var frontendUrl = _configuration.GetValue<string>("FrontendUrl");
                var resetLink = $"{frontendUrl}/reset-password?token={resetToken}";
                var sentEmail = await _emailService.SendPasswordResetLinkAsync(existingUser, resetLink);
                if (!sentEmail) {
                    return ResponseHandler.InternalServerError("Password reset link cannot be sent.");
                }
            }
            else if (!string.IsNullOrEmpty(existingUser.PhoneNumber) && !string.IsNullOrEmpty(existingUser.CountryCode)) {
                var sentSms = await this._smsService.SendPasswordResetLinkAsync(existingUser, existingUser.CountryCode, existingUser.PhoneNumber, resetToken);
                if (!sentSms) {
                    return ResponseHandler.InternalServerError("Password reset link cannot be sent.");
                }
            }

            return ResponseHandler.Ok("Password reset link sent successfully", new {
                Email = existingUser.Email
            });
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> ResetPassword(HttpContext context, UserResetPasswordModel model)
    {
        try{
            var validationResult = await _resetPasswordValidator.ValidateAsync(model);
            if (!validationResult.IsValid) {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }
            //Existing user check
            User? existingUser = await _userService.GetByEmailAsync(model.Email);
            if (existingUser == null) {
                return ResponseHandler.NotFound("User not found");
            }

            var (email, userId) = UserAuthenticationHandler.ValidateResetToken(this._configuration, model.ResetToken);
            if (string.IsNullOrEmpty(email) || userId == Guid.Empty) {
                return ResponseHandler.NotFound("Invalid or expired reset token");
            }

            var hashedPassword = BC.HashPassword(model.NewPassword);
            if (string.IsNullOrEmpty(hashedPassword)) {
                return ResponseHandler.InternalServerError("Password cannot be hashed.");
            }
            var updated = await _userAuthProfileService.UpdateHashedPasswordAsync(existingUser.Id, hashedPassword);
            if (!updated) {
                return ResponseHandler.InternalServerError("Password cannot be updated.");
            }

            return ResponseHandler.Ok("Password reset successfully", new {
                Email = existingUser.Email,
            });
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> ChangePassword(HttpContext context, UserChangePasswordModel model)
    {
        try {
            var validationResult = await _changePasswordValidator.ValidateAsync(model);
            if (!validationResult.IsValid) {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }

            User? existingUser = null;
            if (!string.IsNullOrEmpty(model.Email)) {
                existingUser = await _userService.GetByEmailAsync(model.Email);
                if (existingUser == null) {
                    return ResponseHandler.NotFound("User not found");
                }
            }
            else if (!string.IsNullOrEmpty(model.UserName)) {
                existingUser = await _userService.GetByUsernameAsync(model.UserName);
                if (existingUser == null) {
                    return ResponseHandler.NotFound("User not found");
                }
            }
            else if (!string.IsNullOrEmpty(model.CountryCode) && !string.IsNullOrEmpty(model.PhoneNumber)) {
                existingUser = await _userService.GetByPhoneAsync(model.CountryCode, model.PhoneNumber);
                if (existingUser == null) {
                    return ResponseHandler.NotFound("User not found");
                }
            }
            else {
                return ResponseHandler.BadRequest("Invalid login credentials");
            }

            var authenticated = await validatePassword(existingUser.Id, model.OldPassword);

            var hashedPassword = BC.HashPassword(model.NewPassword);
            var updated = await _userAuthProfileService.UpdateHashedPasswordAsync(existingUser.Id, hashedPassword);
            if (!updated) {
                return ResponseHandler.InternalServerError("Password cannot be updated.");
            }

            return ResponseHandler.Ok("Password changed successfully", new {
                Email = existingUser.Email,
            });
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> RefreshToken(HttpContext context, UserRefreshTokenModel model)
    {
        try {
            var validationResult = await _refreshTokenValidator.ValidateAsync(model);
            if (!validationResult.IsValid) {
                var errors = validationResult.Errors.ToDictionary(
                        e => e.PropertyName,
                        e => new[] { e.ErrorMessage }
                    );
                return ResponseHandler.ValidationError("Validation error", errors);
            }

            var refreshToken = model.RefreshToken;
            var verificationResult = UserAuthenticationHandler.ValidateRefreshToken(this._configuration, model.RefreshToken);
            if (verificationResult.UserId == Guid.Empty) {
                return ResponseHandler.Unauthorized("Invalid or expired refresh token");
            }
            var userId = verificationResult.UserId;

            var existingUser = await _userService.GetByIdAsync(userId);
            if (existingUser == null) {
                return ResponseHandler.NotFound("User not found");
            }

            var session = await _userAuthService.CreateSessionAsync(existingUser.Id);
            if (session == null) {
                return ResponseHandler.InternalServerError("Session cannot be created.");
            }

            var token = _jwtTokenService.GenerateToken(existingUser, session.Id, verificationResult.Role);

            var fullName = (existingUser.FirstName ?? "") + " " + (existingUser.LastName ?? "");
            fullName = fullName.Trim();
            var res = new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddDays(5), // 5 days validity
                UserId = existingUser.Id,
                UserName = existingUser.UserName,
                SessionId = session.Id,
                FirstName = existingUser.FirstName,
                LastName = existingUser.LastName,
                FullName = fullName,
                RoleName = verificationResult.Role,
                ValidTill = session.ValidTill,
            };
            return ResponseHandler.Ok($"Access token for user {existingUser.UserName} refreshed successfully!", res);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Logout(HttpContext context)
    {
        try {
            var verificationResult = UserAuthenticationHandler.GetCurrentUser(context);
            if (verificationResult.UserId == Guid.Empty) {
                return ResponseHandler.Unauthorized("Invalid user");
            }
            if (verificationResult.SessionId == Guid.Empty) {
                return ResponseHandler.Unauthorized("Invalid session");
            }

            var session = await _userAuthService.GetSessionAsync(verificationResult.SessionId);
            if (session == null) {
                return ResponseHandler.NotFound("Current session not found");
            }

            var loggedOut = await _userAuthService.LogoutSessionAsync(verificationResult.SessionId);
            if (!loggedOut) {
                return ResponseHandler.InternalServerError("Session cannot be logged out.");
            }

            return ResponseHandler.Ok("User logged out successfully");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    private async Task<bool> validatePassword(Guid userId, string password)
    {
        var passwordHash = await _userAuthProfileService.GetHashedPasswordAsync(userId);
        if (string.IsNullOrEmpty(passwordHash)) {
            return false;
        }
        return BC.Verify(password, passwordHash);
    }

}
