using System.Linq.Expressions;

namespace Afimilk.JobScheduler.BL
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Conditionally adds a filter to a queryable collection.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">The source queryable collection.</param>
        /// <param name="condition">The condition to determine whether the filter should be applied.</param>
        /// <param name="predicate">The filter predicate to apply if the condition is true.</param>
        /// <returns>The queryable collection with the filter applied if the condition is true.</returns>
        public static IQueryable<TSource> WhereIf<TSource>(
            this IQueryable<TSource> source,
            bool condition,
            Expression<Func<TSource, bool>> predicate)
        {
            if (condition)
            {
                return source.Where(predicate);
            }

            return source;
        }
    }
}