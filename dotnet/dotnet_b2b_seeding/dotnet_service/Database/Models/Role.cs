using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetService.Database.Models
{
    /// <summary>
    /// Role model.
    /// Similar to the Python role.py implementation.
    /// </summary>
    [Table("roles")]
    public class Role : BaseModel
    {
        [Required]
        [Column("userId")]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(50)]
        [Column("roleName")]
        public string RoleName { get; set; } = string.Empty;
        
        // Relationships
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

