
using System.ComponentModel.DataAnnotations;

namespace shala.api.domain.types;

public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string? FirstName { get; set; } = null!;
    public string? LastName { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string? CountryCode { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public Guid? TenantId { get; set; } = Guid.Empty;
}

public class UserCreateModel
{
    public string? FirstName { get; set; } = null!;
    public string? LastName { get; set; } = null!;
    public string? UserName { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string? CountryCode { get; set; } = null!;
    [Required]
    public string? PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
    public Guid? TenantId { get; set; } = Guid.Empty;
}

public class UserUpdateModel
{
    public int id { get; set; }
    public string? FirstName { get; set; } = null!;
    public string? LastName { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string? CountryCode { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public Guid? TenantId { get; set; } = Guid.Empty;
}

public class UserSearchFilters : BaseSearchFilters
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? CountryCode { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? TenantId { get; set; }
}
