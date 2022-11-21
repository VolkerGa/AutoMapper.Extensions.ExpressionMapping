using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping.Impl;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace Automapper.Extensions.ExpressionMapping.EFCore
{
    public class AsyncSourceInjectedQuery<TSource, TDestination> : SourceSourceInjectedQuery<TSource, TDestination>, IAsyncEnumerable<TDestination>
    {
        public AsyncSourceInjectedQuery(IQueryable<TSource> dataSource, IQueryable<TDestination> destQuery,
            IMapper mapper, IEnumerable<ExpressionVisitor> beforeVisitors, IEnumerable<ExpressionVisitor> afterVisitors,
            Action<Exception> exceptionHandler, IDictionary<string, object> parameters,
            IEnumerable<IEnumerable<MemberInfo>> membersToExpand,
            SourceInjectedQueryInspector inspector) : base(dataSource, destQuery, mapper, beforeVisitors, afterVisitors, exceptionHandler, parameters,
                    membersToExpand, inspector, new AsyncSourceInjectedQueryProvider<TSource, TDestination>(mapper, dataSource, destQuery, beforeVisitors, afterVisitors, exceptionHandler, parameters, membersToExpand)
                    {
                        Inspector = inspector ?? new SourceInjectedQueryInspector()
                    })
        {
        }

        protected internal AsyncSourceInjectedQuery(IQueryProvider provider, Expression expression, Action<IEnumerable<object>> enumerationHandler, Action<Exception> exceptionHandler) : base(provider, expression, enumerationHandler, exceptionHandler)
        {
        }

        public IAsyncEnumerator<TDestination> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            try
            {
                var results = ((IAsyncQueryProvider)Provider).ExecuteAsync<IAsyncEnumerable<TDestination>>(Expression);
                return results.GetAsyncEnumerator();
            }
            catch (Exception x)
            {
                ExceptionHandler(x);
                throw;
            }
        }
    }
}
