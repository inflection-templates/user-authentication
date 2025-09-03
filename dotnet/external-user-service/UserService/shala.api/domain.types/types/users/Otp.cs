namespace shala.api.domain.types;

public class Otp
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } = Guid.Empty;
    public bool Used { get; set; } = false;
    public string OtpCode { get; set; } = null!;
    public string Purpose { get; set; } = "login";
    public DateTime? ValidFrom { get; set; } = null!;
    public DateTime? ValidTill { get; set; } = null!;
    public DateTime? CreatedAt { get; set; } = null!;
    public DateTime? UpdatedAt { get; set; } = null!;
    public DateTime? DeletedAt { get; set; } = null!;
}
