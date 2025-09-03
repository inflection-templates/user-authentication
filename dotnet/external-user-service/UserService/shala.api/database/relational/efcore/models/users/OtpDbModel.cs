using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using shala.api.database.interfaces.models;

namespace shala.api.database.relational.efcore;

public class OtpDbModel : IDbModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid? Id { get; set; }

    [Required]
    public Guid UserId { get; set; } = Guid.Empty;

    [Required]
    public bool Used { get; set; } = false;

    [Required]
    public string OtpCode { get; set; } = string.Empty;

    public string Purpose { get; set; } = "login";

    public DateTime? ValidFrom { get; set; } = null!;

    public DateTime? ValidTill { get; set; } = null!;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }

}
