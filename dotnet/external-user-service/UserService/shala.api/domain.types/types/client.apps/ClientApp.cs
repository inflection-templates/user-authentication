
using System.ComponentModel.DataAnnotations;

namespace shala.api.domain.types;

public class ClientApp
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? RedirectUri { get; set; }
    public string? LogoUrl { get; set; } = null!;
    public string? WebsiteUrl { get; set; } = null!;
    public string? PrivacyPolicyUrl { get; set; } = null!;
    public string? TermsOfServiceUrl { get; set; } = null!;
    public bool Verified { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

}

public class ClientAppCreateModel
{
    [Required]
    public Guid OwnerUserId { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string Code { get; set; } = null!;
    [Required]
    public string Description { get; set; } = null!;
    public string? RedirectUri { get; set; }
    public string? LogoUrl { get; set; } = null!;
    public string? WebsiteUrl { get; set; } = null!;
    public string? PrivacyPolicyUrl { get; set; } = null!;
    public string? TermsOfServiceUrl { get; set; } = null!;
    public bool Verified { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ClientAppUpdateModel
{
    public string? Name { get; set; } = null!;
    public string? Code { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public string? RedirectUri { get; set; }
    public string? LogoUrl { get; set; } = null!;
    public string? WebsiteUrl { get; set; } = null!;
    public string? PrivacyPolicyUrl { get; set; } = null!;
    public string? TermsOfServiceUrl { get; set; } = null!;
    public bool Verified { get; set; } = false;
}

public class ClientAppSearchFilters : BaseSearchFilters
{
    public Guid? OwnerUserId { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
}
