namespace shala.api.database.relational.efcore;

public class EFCoreDatabaseInitializer : IDatabaseInitializer
{
    public EFCoreDatabaseInitializer()
    {
    }

    public void Init(WebApplicationBuilder builder)
    {
        IDatabaseInjector injector = new EFCoreInjector();
        injector.Register(builder);
    }

}
