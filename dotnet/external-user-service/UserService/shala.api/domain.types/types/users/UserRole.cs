
namespace shala.api.domain.types;

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid? TenantId { get; set; }
    public User? User { get; set; } = null!;
    public Role? Role { get; set; } = null!;
    public Tenant? Tenant { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
