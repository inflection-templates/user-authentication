namespace shala.api;

public static class Router
{

    public static WebApplication RegisterRoutes(this WebApplication app)
    {
        RoleRoutes.Map(app);
        UserRoutes.Map(app);
        UserAuthRoutes.Map(app);
        UserRoleRoutes.Map(app);
        TenantRoutes.Map(app);
        ClientAppRoutes.Map(app);
        ApiKeyRoutes.Map(app);
        FileResourceRoutes.Map(app);
        OAuthRoutes.Map(app);
        MfaAuthRoutes.Map(app);
        WellKnownRoutes.Map(app);

        return app;
    }

}

/////////////////////////////////////////////////////////////////////////////////////////
