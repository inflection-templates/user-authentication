using System.ComponentModel;

namespace shala.api.domain.types;

public enum DefaultRoles
{
    [Description("System Administrator")]
    SystemAdmin = 0,

    [Description("System User")]
    SystemUser,

    [Description("User")]
    User,

}
