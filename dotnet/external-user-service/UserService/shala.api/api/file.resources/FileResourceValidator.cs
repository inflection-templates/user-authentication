using FluentValidation;
using shala.api.domain.types;

public class FileResourceCreateModelValidator : AbstractValidator<FileResourceCreateModel>
{
    public FileResourceCreateModelValidator()
    {
        RuleFor(x => x.OwnerUserId).NotEmpty().WithMessage("Owner user id is required");
        RuleFor(x => x.FileName).NotEmpty().WithMessage("File name is required");
        RuleFor(x => x.FileName).MinimumLength(2).WithMessage("File name must be at least 2 characters long");
        RuleFor(x => x.FileName).MaximumLength(256).WithMessage("File name must be at most 256 characters long");
        RuleFor(x => x.StorageKey).NotEmpty().WithMessage("Storage key is required");
        RuleFor(x => x.StorageKey).MinimumLength(2).WithMessage("Storage key must be at least 2 characters long");
        RuleFor(x => x.FileSize).GreaterThan(0).WithMessage("File size should be greater than 0");
        RuleFor(x => x.MimeType).NotEmpty().WithMessage("Mime type is required");
        RuleFor(x => x.MimeType).MinimumLength(2).WithMessage("Mime type must be at least 2 characters long");
        RuleFor(x => x.MimeType).MaximumLength(256).WithMessage("Mime type must be at most 256 characters long");
    }
}

public class FileResourceSearchFiltersValidator : AbstractValidator<FileResourceSearchFilters>
{
    public FileResourceSearchFiltersValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(-1).WithMessage("Page index should be equal to or greater than 0");
        RuleFor(x => x.ItemsPerPage).GreaterThan(0).WithMessage("Items per page should be greater than 0");
        RuleFor(x => x.OrderBy).Matches(@"^(OwnerUserId|FileName|FileSize|CreatedAt)$").WithMessage("Invalid sort by field");
    }
}
