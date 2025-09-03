
using shala.api.modules;
using shala.api.services;

namespace shala.api.startup.configurations;

public static class BuilderInjectionExtensions
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        ServiceInjector.Register(builder.Services);
        return builder;
    }

    public static WebApplicationBuilder RegisterControllers(this WebApplicationBuilder builder)
    {
        ControllerInjector.Register(builder);
        return builder;
    }

    public static WebApplicationBuilder RegisterValidators(this WebApplicationBuilder builder)
    {
        ValidatorInjector.Register(builder);
        return builder;
    }

    public static WebApplicationBuilder RegisterModules(this WebApplicationBuilder builder)
    {
        ModuleInjector.Register(builder.Services);
        return builder;
    }
}
