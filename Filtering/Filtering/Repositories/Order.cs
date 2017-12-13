using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Filtering.Repositories
{
    /// <summary>
    /// class for holding the column name and the type of ordering that should be used
    /// </summary>
    public class Order
    {
        /// <summary>
        /// the column of the table that would br requested in the order
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// the direction of the ordering 
        /// </summary>
        public OrderDirection Dir { get; set; }
       
    }

    /// <summary>
    /// extention class for the OrderBy that is used for the collections 
    /// </summary>
    public static class OrderByExtention
    {
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> entities, string propertyName, OrderDirection direction)
        {
            var orderBy = entities as IList<T> ?? entities.ToList();
            if (!orderBy.Any() || string.IsNullOrEmpty(propertyName))
                return orderBy;

            var propertyInfo = orderBy.First().GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            var asc = direction == OrderDirection.Asc;
            return asc ? orderBy.OrderBy(e => propertyInfo?.GetValue(e, null)) : orderBy.OrderByDescending(e => propertyInfo?.GetValue(e, null));
        }


        public static IQueryable<T> OrderBy<T>(this IQueryable<T> entities, string propertyName, OrderDirection direction)
        {
            if (!entities.Any() || string.IsNullOrEmpty(propertyName))
                return entities;

            var propertyInfo = entities.First().GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var asc = direction == OrderDirection.Asc;
            return asc ? entities.OrderBy(e => propertyInfo.GetValue(e, null)) : entities.OrderByDescending(e => propertyInfo.GetValue(e, null));
        }
    }

    /// <summary>
    /// directions of the ordering 
    /// </summary>
    public enum OrderDirection
    {
        Asc,
        Desc
    }


   
}