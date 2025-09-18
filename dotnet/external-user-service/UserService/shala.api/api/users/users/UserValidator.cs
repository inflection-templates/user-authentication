using System.Data;
using FluentValidation;
using shala.api.domain.types;

namespace shala.api;

public class UserCreateModelValidator : AbstractValidator<UserCreateModel>
{
    public UserCreateModelValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName is required");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        RuleFor(x => x.Password).MinimumLength(8).WithMessage("Password must be at least 8 characters");
        RuleFor(x => x.CountryCode).Length(0, 5).WithMessage("Country code should be less than 5 characters");
        RuleFor(x => x.CountryCode).Matches(@"^\+?\d{1,5}$").WithMessage("Country code should be between 1 and 5 digits");
        RuleFor(x => x.PhoneNumber).Length(6, 15).WithMessage("Phone number should be between 6 and 15 characters");
    }
}

public class UserUpdateModelValidator : AbstractValidator<UserUpdateModel>
{
    public UserUpdateModelValidator()
    {
        RuleFor(x => x.Email).EmailAddress().WithMessage("Email should be a valid email address");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address");
        RuleFor(x => x.CountryCode).Length(0, 5).WithMessage("Country code should be less than 5 characters");
        RuleFor(x => x.CountryCode).Matches(@"^\+?\d{1,5}$").WithMessage("Country code should be between 1 and 5 digits");
        RuleFor(x => x.PhoneNumber).Length(6, 15).WithMessage("Phone number should be between 6 and 15 characters");
    }
}

public class UserSearchFiltersValidator : AbstractValidator<UserSearchFilters>
{
    public UserSearchFiltersValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(-1).WithMessage("Page index should be equal to or greater than 0");
        RuleFor(x => x.ItemsPerPage).GreaterThan(0).WithMessage("Items per page should be greater than 0");
        RuleFor(x => x.ItemsPerPage).LessThanOrEqualTo(5000).WithMessage("Items per page should be less than or equal to 5000");
        RuleFor(x => x.Sort).Matches(@"^(asc|desc)$").WithMessage("Invalid sort order");
        RuleFor(x => x.OrderBy).Matches(@"^(FirstName|LastName|Email|PhoneNumber|CreatedAt)$").WithMessage("Invalid sort by field");

        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address");
        RuleFor(x => x.FirstName).Length(0, 50).WithMessage("First name should be less than 50 characters");
        RuleFor(x => x.LastName).Length(0, 50).WithMessage("Last name should be less than 50 characters");
        RuleFor(x => x.PhoneNumber).Length(0, 15).WithMessage("Phone number should be less than 15 characters");
        RuleFor(x => x.PhoneNumber).Matches(@"^\d{8,15}$").WithMessage("Phone number should be between 8 and 15 digits");
        RuleFor(x => x.CountryCode).Length(0, 5).WithMessage("Country code should be less than 5 characters");
        // Adding rule that optional + may be there in front of the country code
        RuleFor(x => x.CountryCode).Matches(@"^\+?\d{1,5}$").WithMessage("Country code should be between 1 and 5 digits");
    }
}
