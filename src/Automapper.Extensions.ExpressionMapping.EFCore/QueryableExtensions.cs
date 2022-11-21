using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping.Impl;

namespace Automapper.Extensions.ExpressionMapping.EFCore
{
    public static class QueryableExtensions
    {
        public static IQueryDataSourceInjection<TSource> UseAsAsyncDataSource<TSource>(this IQueryable<TSource> dataSource, IConfigurationProvider config)
            => dataSource.UseAsAsyncDataSource(config.CreateMapper());

        public static IQueryDataSourceInjection<TSource> UseAsAsyncDataSource<TSource>(this IQueryable<TSource> dataSource, IMapper mapper)
            => new AsyncQueryDataSourceInjection<TSource>(dataSource, mapper);

    }
}
