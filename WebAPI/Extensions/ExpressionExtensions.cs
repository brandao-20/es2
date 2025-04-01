using System.Linq.Expressions;

namespace WebAPI.Extensions
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(T), "p");
            var firstBody = Expression.Invoke(first, parameter);
            var secondBody = Expression.Invoke(second, parameter);
            var body = Expression.AndAlso(firstBody, secondBody);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
}
