using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DotnetService.Database.Models
{
    /// <summary>
    /// Tenant model.
    /// Similar to the Python tenant.py implementation.
    /// </summary>
    [Table("tenants")]
    public class Tenant : BaseModel
    {
        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;
        
        [MaxLength(255)]
        [Column("domain")]
        public string? Domain { get; set; }
        
        [Column("description", TypeName = "text")]
        public string? Description { get; set; }
        
        [MaxLength(100)]
        [Column("industry")]
        public string? Industry { get; set; }
        
        [MaxLength(255)]
        [Column("websiteUrl")]
        public string? WebsiteUrl { get; set; }
        
        [MaxLength(255)]
        [Column("logoUrl")]
        public string? LogoUrl { get; set; }
        
        [MaxLength(100)]
        [Column("contactEmail")]
        public string? ContactEmail { get; set; }
        
        [MaxLength(20)]
        [Column("contactPhone")]
        public string? ContactPhone { get; set; }
        
        [MaxLength(100)]
        [Column("address")]
        public string? Address { get; set; }
        
        [MaxLength(50)]
        [Column("city")]
        public string? City { get; set; }
        
        [MaxLength(50)]
        [Column("state")]
        public string? State { get; set; }
        
        [MaxLength(20)]
        [Column("postalCode")]
        public string? PostalCode { get; set; }
        
        [MaxLength(50)]
        [Column("country")]
        public string? Country { get; set; }
        
        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "active";
        
        [Column("isActive")]
        public bool IsActive { get; set; } = true;
        
        [Column("isVerified")]
        public bool IsVerified { get; set; } = false;
        
        [Column("settings", TypeName = "json")]
        public string? Settings { get; set; }
        
        [Column("billingConfig", TypeName = "json")]
        public string? BillingConfig { get; set; }
        
        [Column("verifiedAt")]
        public DateTime? VerifiedAt { get; set; }
        
        [MaxLength(255)]
        [Column("verifiedBy")]
        public string? VerifiedBy { get; set; }
        
        [Column("deletedAt")]
        public DateTime? DeletedAt { get; set; }
        
        // Relationships
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}

