using FluentValidation;
using shala.api.domain.types;

namespace shala.api;

public class UserTotpValidationModelValidator : AbstractValidator<UserTotpValidationModel>
{
    public UserTotpValidationModelValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
        RuleFor(x => x.UserId).NotEqual(Guid.Empty).WithMessage("UserId is required");

        RuleFor(x => x.TotpCode).NotEmpty().WithMessage("Totp code is required");
        RuleFor(x => x.TotpCode).MinimumLength(6).WithMessage("Totp code must be at least 6 characters long");
        RuleFor(x => x.TotpCode).MaximumLength(6).WithMessage("Totp code must be at most 6 characters long");
        RuleFor(x => x.TotpCode).Matches(@"^\d{6}$").WithMessage("Invalid Totp code");

        RuleFor(x => x.SessionId).NotEmpty().WithMessage("SessionId is required");
        RuleFor(x => x.SessionId).NotEqual(Guid.Empty).WithMessage("SessionId is required");
    }

}
