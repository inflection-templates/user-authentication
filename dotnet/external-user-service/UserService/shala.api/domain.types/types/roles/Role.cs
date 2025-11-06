
namespace shala.api.domain.types;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid? TenantId { get; set; } = null!;
    public bool IsDefaultRole { get; set; } = false;
}

public class RoleCreateModel
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid? TenantId { get; set; } = null!;
    public bool IsDefaultRole { get; set; } = false;
}

public class RoleUpdateModel
{
    public string? Name { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public bool? IsDefaultRole { get; set; } = null!;
    public Guid? TenantId { get; set; } = null!;
}

public class RoleSearchFilters : BaseSearchFilters
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public Guid? TenantId { get; set; }
    public bool IsDefaultRole { get; set; } = false;
}
