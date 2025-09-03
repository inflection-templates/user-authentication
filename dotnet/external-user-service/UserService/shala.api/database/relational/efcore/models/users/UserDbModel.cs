using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using shala.api.database.interfaces.models;

namespace shala.api.database.relational.efcore;

public class UserDbModel : IDbModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid? Id { get; set; }

    [Required]
    public string UserName { get; set; } = null!;

    public string? FirstName { get; set; } = null!;

    public string? LastName { get; set; } = null!;

    public string? Email { get; set; } = null!;

    public string? CountryCode { get; set; } = null!;

    public string? PhoneNumber { get; set; } = null!;

    public Guid? TenantId { get; set; } = Guid.Empty;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }

}
