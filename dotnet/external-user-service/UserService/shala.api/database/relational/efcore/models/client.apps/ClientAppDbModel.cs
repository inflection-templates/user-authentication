using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using shala.api.database.interfaces.models;

namespace shala.api.database.relational.efcore;

public class ClientAppDbModel : IDbModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid? Id { get; set; }

    [Required]
    public Guid OwnerUserId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Code { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? RedirectUri { get; set; }

    public string? LogoUrl { get; set; } = null!;

    public string? WebsiteUrl { get; set; } = null!;

    public string? PrivacyPolicyUrl { get; set; } = null!;

    public string? TermsOfServiceUrl { get; set; } = null!;

    public bool Verified { get; set; } = false;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }

}
