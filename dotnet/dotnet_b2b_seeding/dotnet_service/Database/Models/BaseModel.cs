using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetService.Database.Models
{
    /// <summary>
    /// Base model class with common fields.
    /// Similar to the Python base.py implementation.
    /// </summary>
    public abstract class BaseModel
    {
        [Key]
        [Column("id", Order = 0)]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

