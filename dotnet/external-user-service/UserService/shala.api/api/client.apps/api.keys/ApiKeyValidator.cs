using FluentValidation;
using shala.api.domain.types;

namespace shala.api;

public class ApiKeyCreateModelValidator : AbstractValidator<ApiKeyCreateModel>
{
    public ApiKeyCreateModelValidator()
    {
        RuleFor(x => x.ClientAppId).NotEmpty().WithMessage("Client app id is required");
        RuleFor(x => x.ClientAppId).NotEqual(Guid.Empty).WithMessage("Invalid client app id");
        RuleFor(x => x.Key).NotEmpty().WithMessage("Api key is required");
        RuleFor(x => x.Key).MinimumLength(16).WithMessage("Api key should be greater than 16 characters");
        RuleFor(x => x.SecretHash).NotEmpty().WithMessage("Api key secret is required");
        RuleFor(x => x.SecretHash).MinimumLength(16).WithMessage("Api key secret should be greater than 16 characters");
        RuleFor(x => x.ValidTill).GreaterThan(DateTime.Now.AddDays(7)).WithMessage("Valid till should be atleast 7 days from now");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Api key name is required");
        RuleFor(x => x.Name).Length(1, 50).WithMessage("Api key name should be less than 50 characters");
        RuleFor(x => x.Description).Length(0, 500).WithMessage("Api key description should be less than 500 characters");
    }
}

public class ApiKeyCreateRequestModelValidator : AbstractValidator<ApiKeyCreateRequestModel>
{
    public ApiKeyCreateRequestModelValidator()
    {
        RuleFor(x => x.ClientAppId).NotEmpty().WithMessage("Client app id is required");
        RuleFor(x => x.ClientAppId).NotEqual(Guid.Empty).WithMessage("Invalid client app id");
        RuleFor(x => x.ValidTill).GreaterThan(DateTime.Now.AddDays(7)).WithMessage("Valid till should be atleast 7 days from now");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Api key name is required");
        RuleFor(x => x.Name).Length(1, 50).WithMessage("Api key name should be less than 50 characters");
        RuleFor(x => x.Description).Length(0, 500).WithMessage("Api key description should be less than 500 characters");
    }
}

public class ApiKeySearchFiltersValidator : AbstractValidator<ApiKeySearchFilters>
{
    public ApiKeySearchFiltersValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(-1).WithMessage("Page index should be equal to or greater than 0");
        RuleFor(x => x.ItemsPerPage).GreaterThan(0).WithMessage("Items per page should be greater than 0");
        RuleFor(x => x.ItemsPerPage).LessThanOrEqualTo(5000).WithMessage("Items per page should be less than or equal to 5000");
        RuleFor(x => x.Sort).Matches(@"^(asc|desc)$").WithMessage("Invalid sort order");
        RuleFor(x => x.OrderBy).Matches(@"^(Name|CreatedAt)$").WithMessage("Invalid sort by field");

        RuleFor(x => x.Name).Length(0, 50).WithMessage("Api key name should be less than 50 characters");
    }
}
