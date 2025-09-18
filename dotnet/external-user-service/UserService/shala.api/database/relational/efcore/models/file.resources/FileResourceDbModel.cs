using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using shala.api.database.interfaces.models;

namespace shala.api.database.relational.efcore;

public class FileResourceDbModel : IDbModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid? Id { get; set; }

    [Required]
    public Guid OwnerUserId { get; set; }

    public Guid? TenantId { get; set; } = null!;

    [Required]
    public string FileName { get; set; } = null!;

    [Required]
    public string StorageKey { get; set; } = null!;

    [Required]
    public bool IsPublic { get; set; } = false;

    [Required]
    public long FileSize { get; set; } = 0;

    public string? MimeType { get; set; } = null!;

    public string? FileExtension { get; set; } = null!;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }

}
