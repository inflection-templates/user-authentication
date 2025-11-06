namespace shala.api.database.mappers;

public interface IModelMapper
{
    TDestination Map<TSource, TDestination>(TSource item);
}
