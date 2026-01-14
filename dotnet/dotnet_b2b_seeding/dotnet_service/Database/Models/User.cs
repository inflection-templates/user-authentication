using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetService.Database.Models
{
    /// <summary>
    /// User model.
    /// Similar to the Python user.py implementation.
    /// </summary>
    [Table("users")]
    public class User : BaseModel
    {
        [Required]
        [MaxLength(50)]
        [Column("username")]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        [Column("password")]
        public string Password { get; set; } = string.Empty;
        
        [MaxLength(50)]
        [Column("firstName")]
        public string? FirstName { get; set; }
        
        [MaxLength(50)]
        [Column("lastName")]
        public string? LastName { get; set; }
        
        [MaxLength(20)]
        [Column("phoneNumber")]
        public string? PhoneNumber { get; set; }
        
        [MaxLength(5)]
        [Column("countryCode")]
        public string? CountryCode { get; set; }
        
        [Column("isEmailVerified")]
        public bool IsEmailVerified { get; set; } = false;
        
        [Column("isPhoneVerified")]
        public bool IsPhoneVerified { get; set; } = false;
        
        [Column("isActive")]
        public bool IsActive { get; set; } = true;
        
        [Column("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }
        
        [Column("tenantId")]
        public Guid? TenantId { get; set; }
        
        [MaxLength(50)]
        [Column("role")]
        public string Role { get; set; } = "Normal User";
        
        [Column("organizationId")]
        public Guid? OrganizationId { get; set; }
        
        [Column("isOrganizationAdmin")]
        public bool IsOrganizationAdmin { get; set; } = false;
        
        [Column("invitedAt")]
        public DateTime? InvitedAt { get; set; }
        
        [Column("joinedAt")]
        public DateTime? JoinedAt { get; set; }
        
        [MaxLength(255)]
        [Column("invitedBy")]
        public string? InvitedBy { get; set; }
        
        [Column("deletedAt")]
        public DateTime? DeletedAt { get; set; }
        
        // Relationships
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
        
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}

