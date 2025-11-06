
namespace shala.api.domain.types;

public class UserVerificationResult
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid SessionId { get; set; } = Guid.Empty;
    public Guid? TenantId { get; set; } = Guid.Empty;
}
