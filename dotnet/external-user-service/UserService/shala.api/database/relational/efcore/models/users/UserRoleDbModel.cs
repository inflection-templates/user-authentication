using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using shala.api.database.interfaces.models;

namespace shala.api.database.relational.efcore;

public class UserRoleDbModel : IDbModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid? Id { get; set; }

    [Required]
    public Guid UserId { get; set; } = Guid.Empty;

    public Guid? TenantId { get; set; }

    [Required]
    public Guid RoleId { get; set; } = Guid.Empty;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }

}
