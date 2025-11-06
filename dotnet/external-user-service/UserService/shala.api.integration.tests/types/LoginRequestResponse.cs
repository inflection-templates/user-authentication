using shala.api.domain.types;

public class LoginRequestResponse
{
    public string Status { get; set; } = "success";
    public string Message { get; set; } = "Request successfully processed";
    public LoginResponse? Data { get; set; } = null;
    public string? Url { get; set; } = null;
}
