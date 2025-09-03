using FluentValidation;
using shala.api.domain.types;

namespace shala.api;

public class UserPasswordLoginModelValidator : AbstractValidator<UserPasswordLoginModel>
{
    public UserPasswordLoginModelValidator()
    {
        RuleFor(x => x)
            .Must(HaveAtLeastOneContactInfo)
            .WithMessage("At least one of UserName, Email, or Phone must be provided.");

        RuleFor(x => x.UserName).MinimumLength(4).WithMessage("UserName must be at least 4 characters long");
        RuleFor(x => x.UserName).MaximumLength(50).WithMessage("UserName must be at most 50 characters long");

        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address");
        RuleFor(x => x.Email).MinimumLength(4).WithMessage("Email must be at least 4 characters long");
        RuleFor(x => x.Email).MaximumLength(50).WithMessage("Email must be at most 50 characters long");

        RuleFor(x => x.PhoneNumber).Matches(@"^\d{8,15}$").WithMessage("Phone number should be between 8 and 15 digits");
        RuleFor(x => x.CountryCode).MinimumLength(1).WithMessage("Country code must be at least 1 character long");
        RuleFor(x => x.CountryCode).MaximumLength(5).WithMessage("Country code must be at most 5 characters long");

        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        RuleFor(x => x.Password).MinimumLength(8).WithMessage("Password must be at least 8 characters long");
        RuleFor(x => x.Password).MaximumLength(50).WithMessage("Password must be at most 50 characters long");
    }

    private bool HaveAtLeastOneContactInfo(UserPasswordLoginModel model)
    {
        return !string.IsNullOrEmpty(model.UserName) ||
               !string.IsNullOrEmpty(model.Email) ||
               !string.IsNullOrEmpty(model.PhoneNumber);
    }
}

public class UserOtpLoginModelValidator : AbstractValidator<UserOtpLoginModel>
{
    public UserOtpLoginModelValidator()
    {
        RuleFor(x => x)
            .Must(HaveAtLeastOneContactInfo)
            .WithMessage("At least one of UserName, Email, or Phone must be provided.");

        RuleFor(x => x.UserName).MinimumLength(4).WithMessage("UserName must be at least 4 characters long");
        RuleFor(x => x.UserName).MaximumLength(50).WithMessage("UserName must be at most 50 characters long");

        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address");
        RuleFor(x => x.Email).MinimumLength(4).WithMessage("Email must be at least 4 characters long");
        RuleFor(x => x.Email).MaximumLength(50).WithMessage("Email must be at most 50 characters long");

        RuleFor(x => x.PhoneNumber).Matches(@"^\d{8,15}$").WithMessage("Phone number should be between 8 and 15 digits");
        RuleFor(x => x.CountryCode).MinimumLength(1).WithMessage("Country code must be at least 1 character long");
        RuleFor(x => x.CountryCode).MaximumLength(5).WithMessage("Country code must be at most 5 characters long");

        RuleFor(x => x.Otp).NotEmpty().WithMessage("Otp is required");
        RuleFor(x => x.Otp).MinimumLength(6).WithMessage("Otp must be at least 6 characters long");
        RuleFor(x => x.Otp).MaximumLength(6).WithMessage("Otp must be at most 6 characters long");
        RuleFor(x => x.Otp).Matches(@"^\d{6}$").WithMessage("Otp must be 6 digits long");
    }

    private bool HaveAtLeastOneContactInfo(UserOtpLoginModel model)
    {
        return !string.IsNullOrEmpty(model.UserName) ||
               !string.IsNullOrEmpty(model.Email) ||
               !string.IsNullOrEmpty(model.PhoneNumber);
    }

}

public class UserSendOtpModelValidator : AbstractValidator<UserSendOtpModel>
{
    public UserSendOtpModelValidator()
    {
        RuleFor(x => x)
            .Must(HaveAtLeastOneContactInfo)
            .WithMessage("At least one of UserName, Email, or Phone must be provided.");

        RuleFor(x => x.UserName).MinimumLength(4).WithMessage("UserName must be at least 4 characters long");
        RuleFor(x => x.UserName).MaximumLength(50).WithMessage("UserName must be at most 50 characters long");

        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address");
        RuleFor(x => x.Email).MinimumLength(4).WithMessage("Email must be at least 4 characters long");
        RuleFor(x => x.Email).MaximumLength(50).WithMessage("Email must be at most 50 characters long");

        RuleFor(x => x.PhoneNumber).Matches(@"^\d{8,15}$").WithMessage("Phone number should be between 8 and 15 digits");
        RuleFor(x => x.CountryCode).MinimumLength(1).WithMessage("Country code must be at least 1 character long");
        RuleFor(x => x.CountryCode).MaximumLength(5).WithMessage("Country code must be at most 5 characters long");

        RuleFor(x => x.Purpose).NotEmpty().WithMessage("Purpose is required");
    }

    private bool HaveAtLeastOneContactInfo(UserSendOtpModel model)
    {
        return !string.IsNullOrEmpty(model.UserName) ||
               !string.IsNullOrEmpty(model.Email) ||
               !string.IsNullOrEmpty(model.PhoneNumber);
    }
}

public class UserResetPasswordSendLinkModelValidator : AbstractValidator<UserResetPasswordSendLinkModel>
{
    public UserResetPasswordSendLinkModelValidator()
    {
        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address");
        RuleFor(x => x.Email).MinimumLength(4).WithMessage("Email must be at least 4 characters long");
        RuleFor(x => x.Email).MaximumLength(50).WithMessage("Email must be at most 50 characters long");
    }
}

public class UserResetPasswordModelValidator : AbstractValidator<UserResetPasswordModel>
{
    public UserResetPasswordModelValidator()
    {
        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address");
        RuleFor(x => x.Email).MinimumLength(4).WithMessage("Email must be at least 4 characters long");
        RuleFor(x => x.Email).MaximumLength(50).WithMessage("Email must be at most 50 characters long");

        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New password is required");
        RuleFor(x => x.NewPassword).MinimumLength(8).WithMessage("New password must be at least 8 characters long");
        RuleFor(x => x.NewPassword).MaximumLength(50).WithMessage("New password must be at most 50 characters long");

        RuleFor(x => x.ResetToken).NotEmpty().WithMessage("Reset token is required");
        RuleFor(x => x.ResetToken).MinimumLength(8).WithMessage("Reset token must be at least 8 characters long");
        RuleFor(x => x.ResetToken).MaximumLength(50).WithMessage("Reset token must be at most 50 characters long");
    }
}

public class UserChangePasswordModelValidator : AbstractValidator<UserChangePasswordModel>
{
    public UserChangePasswordModelValidator()
    {
        RuleFor(x => x)
            .Must(HaveAtLeastOneContactInfo)
            .WithMessage("At least one of UserName, Email, or Phone must be provided.");

        RuleFor(x => x.UserName).MinimumLength(4).WithMessage("UserName must be at least 4 characters long");
        RuleFor(x => x.UserName).MaximumLength(50).WithMessage("UserName must be at most 50 characters long");

        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address");
        RuleFor(x => x.Email).MinimumLength(4).WithMessage("Email must be at least 4 characters long");
        RuleFor(x => x.Email).MaximumLength(50).WithMessage("Email must be at most 50 characters long");

        RuleFor(x => x.PhoneNumber).Matches(@"^\d{8,15}$").WithMessage("Phone number should be between 8 and 15 digits");
        RuleFor(x => x.CountryCode).MinimumLength(1).WithMessage("Country code must be at least 1 character long");
        RuleFor(x => x.CountryCode).MaximumLength(5).WithMessage("Country code must be at most 5 characters long");

        RuleFor(x => x.OldPassword).NotEmpty().WithMessage("Old password is required");
        RuleFor(x => x.OldPassword).MinimumLength(8).WithMessage("Old password must be at least 8 characters long");
        RuleFor(x => x.OldPassword).MaximumLength(50).WithMessage("Old password must be at most 50 characters long");

        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New password is required");
        RuleFor(x => x.NewPassword).MinimumLength(8).WithMessage("New password must be at least 8 characters long");
        RuleFor(x => x.NewPassword).MaximumLength(50).WithMessage("New password must be at most 50 characters long");
    }

    private bool HaveAtLeastOneContactInfo(UserChangePasswordModel model)
    {
        return !string.IsNullOrEmpty(model.UserName) ||
               !string.IsNullOrEmpty(model.Email) ||
               !string.IsNullOrEmpty(model.PhoneNumber);
    }
}

public class UserRefreshTokenModelValidator : AbstractValidator<UserRefreshTokenModel>
{
    public UserRefreshTokenModelValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required");
        RuleFor(x => x.RefreshToken).MinimumLength(8).WithMessage("Refresh token must be at least 8 characters long");
        RuleFor(x => x.RefreshToken).MaximumLength(512).WithMessage("Refresh token must be at most 50 characters long");
    }
}
