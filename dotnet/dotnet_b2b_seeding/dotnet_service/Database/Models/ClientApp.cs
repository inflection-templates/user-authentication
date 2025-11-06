using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetService.Database.Models
{
    /// <summary>
    /// ClientApp model.
    /// Similar to the Python client_app.py implementation.
    /// </summary>
    [Table("client_apps")]
    public class ClientApp : BaseModel
    {
        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;
        
        [MaxLength(100)]
        [Column("apiKey")]
        public string? ApiKey { get; set; }
        
        [Required]
        [Column("description", TypeName = "text")]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Column("ownerUserId")]
        public Guid OwnerUserId { get; set; }
        
        [Column("organizationId")]
        public Guid? OrganizationId { get; set; }
        
        [MaxLength(500)]
        [Column("redirectUri")]
        public string? RedirectUri { get; set; }
        
        [MaxLength(500)]
        [Column("logoUrl")]
        public string? LogoUrl { get; set; }
        
        [MaxLength(500)]
        [Column("websiteUrl")]
        public string? WebsiteUrl { get; set; }
        
        [MaxLength(500)]
        [Column("privacyPolicyUrl")]
        public string? PrivacyPolicyUrl { get; set; }
        
        [MaxLength(500)]
        [Column("termsOfServiceUrl")]
        public string? TermsOfServiceUrl { get; set; }
        
        [Column("isVerified")]
        public bool IsVerified { get; set; } = false;
        
        [Column("isActive")]
        public bool IsActive { get; set; } = true;
        
        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "active";
        
        [Column("deletedAt")]
        public DateTime? DeletedAt { get; set; }
    }
}

