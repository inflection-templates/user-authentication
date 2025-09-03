
namespace shala.api.domain.types;

public class ApiKey
{
    public Guid Id { get; set; }

    public string Key { get; set; } = null!;

    public Guid ClientAppId { get; set; } = Guid.Empty;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ValidTill { get; set; } = null!;

}


public class ApiKeyCreateModel
{
    public Guid? ClientAppId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime? ValidTill { get; set; } = null!;
    public string? Key { get; set; } = null!;
    public string? SecretHash { get; set; } = null!;
}

public class ApiKeyUpdateModel
{
    public string? Name { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public DateTime? ValidTill { get; set; } = null!;
}

public class ApiKeyCreateRequestModel
{
    public Guid ClientAppId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? ValidTill { get; set; } = null!;
}

public class ApiKeyCreateVerificationModel
{
    public string ClientAppCode { get; set; } = string.Empty;
    // public string Password { get; set; } = string.Empty;
    public string ApiKeyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? ValidTill { get; set; } = null!;
}

public class ApiKeySearchFilters : BaseSearchFilters
{
    public Guid? ClientAppId { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public class ApiKeySecret
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}
