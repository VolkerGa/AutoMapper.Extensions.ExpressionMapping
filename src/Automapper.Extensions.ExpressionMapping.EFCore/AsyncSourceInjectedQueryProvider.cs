using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping.Impl;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace Automapper.Extensions.ExpressionMapping.EFCore
{
    public class AsyncSourceInjectedQueryProvider<TSource, TDestination> : SourceInjectedQueryProvider<TSource, TDestination>, IAsyncQueryProvider
    {
        public AsyncSourceInjectedQueryProvider(IMapper mapper, IQueryable<TSource> dataSource,
            IQueryable<TDestination> destQuery, IEnumerable<ExpressionVisitor> beforeVisitors,
            IEnumerable<ExpressionVisitor> afterVisitors, Action<Exception> exceptionHandler,
            IDictionary<string, object> parameters,
            IEnumerable<IEnumerable<MemberInfo>> membersToExpand) : base(mapper, dataSource, destQuery,
                beforeVisitors, afterVisitors, exceptionHandler, parameters, membersToExpand)
        {
        }

        public override IQueryable CreateQuery(Expression expression)
            => new AsyncSourceInjectedQuery<TSource, TDestination>(this, expression, EnumerationHandler, ExceptionHandler);

        public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new AsyncSourceInjectedQuery<TSource, TElement>(this, expression, EnumerationHandler, ExceptionHandler);


        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult);
            Type destResultType;
            if (resultType.IsGenericType)
            {
                var typeDefinition = resultType.IsGenericTypeDefinition ? resultType : resultType.GetGenericTypeDefinition();
                destResultType = typeDefinition == typeof(Task<>) ? resultType.GenericTypeArguments[0] : resultType;
            }
            else
            {
                destResultType = resultType;
            }
            return ExecuteCore<TResult>(expression, destResultType, (Expression e, Type resultType) =>
            {
                var executeAsyncMethod = typeof(IAsyncQueryProvider).GetMethod("ExecuteAsync")?.MakeGenericMethod(resultType);
                return executeAsyncMethod?.Invoke((IAsyncQueryProvider)QueryProvider,
                    new object[] { e, default(CancellationToken) });
            });
        }
    }
}
