
namespace shala.api.domain.types;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Code { get; set; } = null!;
    public string? CountryCode { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;

}

public class TenantCreateModel
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Code { get; set; } = null!;
    public string? CountryCode { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;

}

public class TenantUpdateModel
{
    public string? Name { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public string? Code { get; set; } = null!;
    public string? CountryCode { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public string? Email { get; set; } = null!;
}

public class TenantSearchFilters : BaseSearchFilters
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
