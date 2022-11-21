using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping.Impl;

namespace Automapper.Extensions.ExpressionMapping.EFCore
{
    public class AsyncQueryDataSourceInjection<TSource> : QueryDataSourceInjection<TSource>
    {
        public AsyncQueryDataSourceInjection(IQueryable<TSource> dataSource, IMapper mapper) : base(dataSource, mapper)
        {
        }

        protected override ISourceInjectedQueryable<TDestination> CreateQueryable<TDestination>() =>
                        new AsyncSourceInjectedQuery<TSource, TDestination>(DataSource,
                new TDestination[0].AsQueryable(),
                Mapper,
                BeforeMappingVisitors,
                AfterMappingVisitors,
                ExceptionHandler,
                Parameters,
                MembersToExpand,
                Inspector);

    }
}
