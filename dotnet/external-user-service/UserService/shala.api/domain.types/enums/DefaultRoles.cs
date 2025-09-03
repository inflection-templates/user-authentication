using System.ComponentModel;

namespace shala.api.domain.types;

public enum DefaultRoles
{
    [Description("System Administrator")]
    SystemAdmin = 0,

    [Description("Tenant Administrator")]
    TenantAdmin,

    [Description("User")]
    User,

    [Description("Organization Admin")]
    OrganizationAdmin,

    [Description("Team Admin")]
    TeamAdmin,

    [Description("Moderator")]
    Moderator,

    [Description("Developer")]
    Developer,

}
