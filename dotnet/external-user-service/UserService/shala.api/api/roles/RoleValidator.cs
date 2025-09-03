using System.Data;
using FluentValidation;
using shala.api.domain.types;

namespace shala.api;

public class RoleCreateModelValidator : AbstractValidator<RoleCreateModel>
{
    public RoleCreateModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Role name is required");
        RuleFor(x => x.Name).MinimumLength(1).WithMessage("Role name must be at least 1 characters long");
        RuleFor(x => x.Name).MaximumLength(50).WithMessage("Role name must be at most 50 characters long");

        RuleFor(x => x.Description).NotEmpty().WithMessage("Role description is required");
        RuleFor(x => x.Description).MinimumLength(4).WithMessage("Role description must be at least 4 characters long");
        RuleFor(x => x.Description).MaximumLength(500).WithMessage("Role description must be at most 500 characters long");

        RuleFor(x => x.Code).NotEmpty().WithMessage("Role code is required");
        RuleFor(x => x.Code).MinimumLength(1).WithMessage("Role code must be at least 1 characters long");
        RuleFor(x => x.Code).MaximumLength(50).WithMessage("Role code must be at most 50 characters long");
    }

}
public class RoleUpdateModelValidator : AbstractValidator<RoleUpdateModel>
{
    public RoleUpdateModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Role name is required");
        RuleFor(x => x.Name).MinimumLength(1).WithMessage("Role name must be at least 1 characters long");
        RuleFor(x => x.Name).MaximumLength(50).WithMessage("Role name must be at most 50 characters long");

        RuleFor(x => x.Description).NotEmpty().WithMessage("Role description is required");
        RuleFor(x => x.Description).MinimumLength(1).WithMessage("Role description must be at least 1 characters long");
        RuleFor(x => x.Description).MaximumLength(500).WithMessage("Role description must be at most 500 characters long");

    }
}

public class RoleSearchFiltersValidator : AbstractValidator<RoleSearchFilters>
{
    public RoleSearchFiltersValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(-1).WithMessage("Page index should be equal to or greater than 0");
        RuleFor(x => x.ItemsPerPage).GreaterThan(0).WithMessage("Items per page should be greater than 0");
        RuleFor(x => x.ItemsPerPage).LessThanOrEqualTo(5000).WithMessage("Items per page should be less than or equal to 5000");
        RuleFor(x => x.Sort).Matches(@"^(asc|desc)$").WithMessage("Invalid sort order");
        RuleFor(x => x.OrderBy).Matches(@"^(Name|Code|CreatedAt)$").WithMessage("Invalid sort by field");

        RuleFor(x => x.Name).MaximumLength(50).WithMessage("Role name must be at most 50 characters long");
        RuleFor(x => x.Code).MaximumLength(50).WithMessage("Role code must be at most 50 characters long");
    }
}
