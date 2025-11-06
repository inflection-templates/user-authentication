
namespace shala.api.domain.types;

public class TenantSettings
{
    public Guid? Id { get; set; }
    public Guid? TenantId { get; set; }
    public string Settings { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TenantSettingsCreateModel
{
    public Guid? TenantId { get; set; }
    public string Settings { get; set; } = string.Empty;
}

public class TenantSettingsUpdateModel
{
    public string? Settings { get; set; } = string.Empty;
}

public class TenantSettingsSearchFilters : BaseSearchFilters
{
    public Guid? TenantId { get; set; }
}
