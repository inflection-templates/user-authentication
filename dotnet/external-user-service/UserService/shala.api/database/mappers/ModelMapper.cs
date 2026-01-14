using AutoMapper;

namespace shala.api.database.mappers;

public static class ModelMapper
{

    #region Publics

    public static TDestination Map<TSource, TDestination>(TSource item)
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<TSource, TDestination>());
        var mapper = new Mapper(mapperConfig);
        return mapper.Map<TDestination>(item);
    }

    #endregion

}
