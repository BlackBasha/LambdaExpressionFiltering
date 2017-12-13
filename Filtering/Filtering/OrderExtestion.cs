using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Filtering
{
    public static class OrderExtention
    {
        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IEnumerable<TSource> query, string propertyName)
        {
            try
            {
                var entityType = typeof(TSource);

                //Create x=>x.PropName
                var propertyInfo = entityType.GetProperty(propertyName);
                ParameterExpression arg = Expression.Parameter(entityType, "x");
                MemberExpression property = Expression.Property(arg, propertyName);
                var selector = Expression.Lambda(property, new ParameterExpression[] { arg });

                //Get System.Linq.Queryable.OrderBy() method.
                var enumarableType = typeof(System.Linq.Queryable);
                var method = enumarableType.GetMethods()
                     .Where(m => m.Name == "OrderBy" && m.IsGenericMethodDefinition)
                     .Where(m =>
                     {
                         var parameters = m.GetParameters().ToList();
                     //Put more restriction here to ensure selecting the right overload                
                     return parameters.Count == 2;//overload that has 2 parameters
                 }).Single();
                //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
                var genericMethod = method
                     .MakeGenericMethod(entityType, propertyInfo?.PropertyType);

                /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
                  Note that we pass the selector as Expression to the method and we don't compile it.
                  By doing so EF can extract "order by" columns and generate SQL for it.*/
                var newQuery = (IOrderedQueryable<TSource>)genericMethod
                     .Invoke(genericMethod, new object[] { query, selector });
                return newQuery;

            }
            catch (Exception ex)
            {

                throw new Exception("Error in order by",ex);
            }
          
        }

        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IEnumerable<TSource> query, string propertyName)
        {
            var entityType = typeof(TSource);

            //Create x=>x.PropName
            var propertyInfo = entityType.GetProperty(propertyName);
            var arg = Expression.Parameter(entityType, "x");
            var property = Expression.Property(arg, propertyName);
            var selector = Expression.Lambda(property, arg);

            //Get System.Linq.Queryable.OrderBy() method.
            var enumarableType = typeof(Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "OrderByDescending" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     //Put more restriction here to ensure selecting the right overload                
                     return parameters.Count == 2;//overload that has 2 parameters
                 }).Single();
            //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
            MethodInfo genericMethod = method
                 .MakeGenericMethod(entityType, propertyInfo?.PropertyType);

            /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
              Note that we pass the selector as Expression to the method and we don't compile it.
              By doing so EF can extract "order by" columns and generate SQL for it.*/
            var newQuery = (IOrderedQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }


    }
}
