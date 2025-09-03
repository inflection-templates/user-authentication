using shala.api.services;
using shala.api.startup;
using Serilog;

namespace shala.api.common;

public static class UserAuthorizationHandler
{
    public static async Task<bool> Authorize(HttpContext context)
    {
        var roleService = context.RequestServices.GetRequiredService<IRoleService>();
        var items = UserAuthenticationHandler.GetCurrentUser(context);
        await Task.Run(() => {
            Log.Information("Authorizing users at multiple authorization levels...");
        });
        return true;
    }
}
