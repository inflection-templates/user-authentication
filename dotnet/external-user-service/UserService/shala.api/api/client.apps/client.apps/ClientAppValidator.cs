using System.Data;
using FluentValidation;
using shala.api.domain.types;

namespace shala.api;

public class ClientAppCreateModelValidator : AbstractValidator<ClientAppCreateModel>
{
    public ClientAppCreateModelValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Client app code is required");
        RuleFor(x => x.Code).Length(1, 50).WithMessage("Client app code should be less than 50 characters");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Client app name is required");
        RuleFor(x => x.Name).Length(1, 50).WithMessage("Client app name should be less than 50 characters");
        RuleFor(x => x.Description).Length(0, 500).WithMessage("Client app description should be less than 500 characters");
        RuleFor(x => x.LogoUrl).Matches(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$").WithMessage("Invalid logo url");
        RuleFor(x => x.OwnerUserId).NotEmpty().WithMessage("Owner user id is required");
        RuleFor(x => x.RedirectUri).Matches(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$").WithMessage("Invalid redirect uri");
        RuleFor(x => x.TermsOfServiceUrl).Matches(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$").WithMessage("Invalid terms of service url");
        RuleFor(x => x.WebsiteUrl).Matches(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$").WithMessage("Invalid website url");
        RuleFor(x => x.PrivacyPolicyUrl).Matches(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$").WithMessage("Invalid privacy policy url");
    }
}

public class ClientAppUpdateModelValidator : AbstractValidator<ClientAppUpdateModel>
{
    public ClientAppUpdateModelValidator()
    {
        RuleFor(x => x.Name).Length(1, 50).WithMessage("Client app name should be less than 50 characters");
        RuleFor(x => x.Description).Length(0, 500).WithMessage("Client app description should be less than 500 characters");
        RuleFor(x => x.LogoUrl).Matches(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$").WithMessage("Invalid logo url");
        RuleFor(x => x.RedirectUri).Matches(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$").WithMessage("Invalid redirect uri");
        RuleFor(x => x.TermsOfServiceUrl).Matches(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$").WithMessage("Invalid terms of service url");
        RuleFor(x => x.WebsiteUrl).Matches(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$").WithMessage("Invalid website url");
        RuleFor(x => x.PrivacyPolicyUrl).Matches(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$").WithMessage("Invalid privacy policy url");
    }
}

public class ClientAppSearchFiltersValidator : AbstractValidator<ClientAppSearchFilters>
{
    public ClientAppSearchFiltersValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(-1).WithMessage("Page index should be equal to or greater than 0");
        RuleFor(x => x.ItemsPerPage).GreaterThan(0).WithMessage("Items per page should be greater than 0");
        RuleFor(x => x.ItemsPerPage).LessThanOrEqualTo(5000).WithMessage("Items per page should be less than or equal to 5000");
        RuleFor(x => x.Sort).Matches(@"^(asc|desc)$").WithMessage("Invalid sort order");
        RuleFor(x => x.OrderBy).Matches(@"^(Name|Code|CreatedAt)$").WithMessage("Invalid sort by field");

        RuleFor(x => x.Name).Length(0, 50).WithMessage("Client app name should be less than 50 characters");
        RuleFor(x => x.Code).Length(0, 50).WithMessage("Client app code should be less than 50 characters");
    }
}
