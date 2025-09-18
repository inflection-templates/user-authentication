using FluentValidation;
using shala.api.domain.types;

namespace shala.api;

public class TenantCreateModelValidator : AbstractValidator<TenantCreateModel>
{
    public TenantCreateModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Name).MinimumLength(2).WithMessage("Name must be at least 2 characters long");
        RuleFor(x => x.Name).MaximumLength(50).WithMessage("Name must be at most 50 characters long");

        RuleFor(x => x.Description).MinimumLength(2).WithMessage("Description must be at least 2 characters long");
        RuleFor(x => x.Description).MaximumLength(512).WithMessage("Description must be at most 512 characters long");

        RuleFor(x => x.Code).MinimumLength(2).WithMessage("Code must be at least 2 characters long");
        RuleFor(x => x.Code).MaximumLength(16).WithMessage("Code must be at most 16 characters long");

        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address");

        RuleFor(x => x.PhoneNumber).Matches(@"^\d{8,15}$").WithMessage("Phone number should be between 8 and 15 digits");
        RuleFor(x => x.CountryCode).MinimumLength(1).WithMessage("Country code must be at least 1 character long");
        RuleFor(x => x.CountryCode).MaximumLength(5).WithMessage("Country code must be at most 5 characters long");

        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        RuleFor(x => x.Password).MinimumLength(8).WithMessage("Password must be at least 8 characters long");
        RuleFor(x => x.Password).MaximumLength(50).WithMessage("Password must be at most 50 characters long");

    }

}

public class TenantUpdateModelValidator : AbstractValidator<TenantUpdateModel>
{
    public TenantUpdateModelValidator()
    {
        RuleFor(x => x.Name).MinimumLength(2).WithMessage("Name must be at least 2 characters long");
        RuleFor(x => x.Name).MaximumLength(50).WithMessage("Name must be at most 50 characters long");
        RuleFor(x => x.Description).MinimumLength(2).WithMessage("Description must be at least 2 characters long");
        RuleFor(x => x.Description).MaximumLength(512).WithMessage("Description must be at most 512 characters long");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address");
        RuleFor(x => x.PhoneNumber).Matches(@"^\d{8,15}$").WithMessage("Phone number should be between 8 and 15 digits");
        RuleFor(x => x.CountryCode).MinimumLength(1).WithMessage("Country code must be at least 1 character long");
        RuleFor(x => x.CountryCode).MaximumLength(5).WithMessage("Country code must be at most 5 characters long");
    }
}

public class TenantSearchFiltersValidator : AbstractValidator<TenantSearchFilters>
{
    public TenantSearchFiltersValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(-1).WithMessage("Page index should be equal to or greater than 0");
        RuleFor(x => x.ItemsPerPage).GreaterThan(0).WithMessage("Items per page should be greater than 0");
        RuleFor(x => x.ItemsPerPage).LessThanOrEqualTo(5000).WithMessage("Items per page should be less than or equal to 5000");
        RuleFor(x => x.Sort).Matches(@"^(asc|desc)$").WithMessage("Invalid sort order");
        RuleFor(x => x.OrderBy).Matches(@"^(Name|Code|CreatedAt)$").WithMessage("Invalid sort by field");

        RuleFor(x => x.Name).MinimumLength(2).WithMessage("Name must be at least 2 characters long");
        RuleFor(x => x.Name).MaximumLength(50).WithMessage("Name must be at most 50 characters long");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address");
        RuleFor(x => x.PhoneNumber).Matches(@"^\d{8,15}$").WithMessage("Phone number should be between 8 and 15 digits");
        RuleFor(x => x.Code).MinimumLength(2).WithMessage("Code must be at least 2 characters long");
        RuleFor(x => x.Code).MaximumLength(16).WithMessage("Code must be at most 16 characters long");
    }
}
