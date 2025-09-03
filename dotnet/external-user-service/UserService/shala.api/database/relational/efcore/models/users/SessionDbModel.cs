using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using shala.api.database.interfaces.models;

namespace shala.api.database.relational.efcore;

public class SessionDbModel : IDbModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid? Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public bool IsActive { get; set; } = true;
    public Guid? SessionRoleId { get; set; } = null!;

    public string? AuthenticationMethod { get; set; } = null!;
    public string? OAuthProvider { get; set; } = null!;

    public bool MfaEnabled { get; set; } = true;
    public string? MfaType { get; set; } = null!;
    public bool MfaAuthenticated { get; set; } = false;


    public string? UserAgent { get; set; } = null!;
    public string? IpAddress { get; set; } = null!;
    public Guid? ClientAppId { get; set; } = null!;


    [Required]
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public DateTime? ValidTill { get; set; } = null!;
    public DateTime? LoggedOutAt { get; set; } = null;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }

}
