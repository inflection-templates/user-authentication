using System.ComponentModel;

namespace shala.api.domain.types;

public enum PermissionScope
{

    [Description("System")]
    System = 0,

    [Description("Tenant")]
    Tenant,

    [Description("User")]
    User,

    [Description("Restricted")]
    Restricted,

    [Description("Public")]
    Public
}
