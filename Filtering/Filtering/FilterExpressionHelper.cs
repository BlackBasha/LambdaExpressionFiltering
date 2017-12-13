using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Filtering.Repositories;

namespace Filtering
{
    public  class FilterExpressionHelper<TO,T> 
    {
        /// <summary>
        /// this calss is used for rebuild the name of the parameter of the expression
        /// </summary>
        internal class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter;

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return base.VisitParameter(_parameter);
            }

            internal ParameterReplacer(ParameterExpression parameter)
            {
                _parameter = parameter;
            }
        }

        /// <summary>
        /// generate the Where part of the query
        /// </summary>
        /// <param name="filterExpressionList"> all the filter operations to create the full where filter with and operand</param>
        /// <returns></returns>
        public Expression<Func<TO, bool>> GetFilterPredicateForWhere(IList<FilterExpression> filterExpressionList)
        {

            if (filterExpressionList.Count == 0)
            {
                return null;
            }
            BinaryExpression filterValue = Expression.MakeBinary(ExpressionType.Equal, Expression.Constant(1), Expression.Constant(1));
            var argument = Expression.Parameter(typeof(TO), "y");


            foreach (var member in filterExpressionList)
            {


                // get the type of the property
                System.Reflection.PropertyInfo p = typeof(Position).GetProperty(member.ColumnName);

                Type typeOfProperty = p.PropertyType;
                bool nullable = Nullable.GetUnderlyingType(typeOfProperty) != null;
                object conversionResult=null;

                // cast the value
                if (typeOfProperty.IsEnum)
                {
                    conversionResult = Enum.Parse(typeOfProperty, member.Value);
                }
                else
                {
                    if (typeOfProperty.IsGenericType && typeOfProperty.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        if (member.Value == null)
                        {
                            conversionResult= Activator.CreateInstance(typeOfProperty);
                        }

                        typeOfProperty = Nullable.GetUnderlyingType(typeOfProperty);
                    }
                    else
                        conversionResult = Convert.ChangeType(member.Value, typeOfProperty);
                }


                // create the lambda 
                var typeofObject = typeof(TO);

                var parameter = Expression.Parameter(typeofObject);
                if (typeOfProperty != null)
                {
                    var body = Expression.Convert(Expression.Property(parameter, member.ColumnName ),typeOfProperty);
                    var delegateType = typeof(Func<,>).MakeGenericType(typeofObject, typeOfProperty);
                    dynamic lambda = Expression.Lambda(delegateType, body, parameter);


                    //create the expressin
                    var parameterB = lambda.Parameters[0];
                    var leftB = lambda.Body;
                    var rightB = Expression.Constant(conversionResult);


                    var correctValue = Expression.Convert(rightB, typeOfProperty);

                    if (member.FilterOperation == FilterOperations.Contains)
                    {
                        var expr = GetExpression(member.ColumnName, member.Value);
                        Expression<Func<TO, bool>> predicateContains = new ParameterReplacer(argument).Visit(expr);

                        filterValue = Expression.AndAlso(filterValue, predicateContains.Body);
                    }

                    else
                    {
                        var predicate = Expression.MakeBinary(member.FilterOperation.ToExpressionType(), leftB, correctValue);



                        predicate = (BinaryExpression)new ParameterReplacer(argument).Visit(predicate);

                        filterValue = Expression.And(filterValue, predicate);
                    }
                }
            }
            return Expression.Lambda<Func<TO, bool>>(filterValue, argument);

        }

        /// <summary>
        /// Generate the Sort part of the query
        /// </summary>
        /// <param name="orders"> the orders list</param>
        /// <param name="dataList"> the data to be orderd</param>
        /// <returns></returns>
        public IQueryable<T> GetFilterPredicateForSort(List<Order> orders,IQueryable<T> dataList)
        {

            if (orders.Count == 0)
            {
                return null;
            }
            foreach (var member in orders)
            {

                if (member.Dir == OrderDirection.Asc)
                {
                    dataList = dataList.OrderBy(member.ColumnName);
                }
                else
                {
                    dataList = dataList.OrderByDescending(member.ColumnName);
                }



            }
            return dataList;

        }

        private  Expression<Func<TO, bool>> GetExpression(string propertyName, string propertyValue)
        {
            var parameterExp = Expression.Parameter(typeof(TO), "type");
            var propertyExp = Expression.Property(parameterExp, propertyName);
            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var someValue = Expression.Constant(propertyValue, typeof(string));
            var containsMethodExp = Expression.Call(propertyExp, method, someValue);

            return Expression.Lambda<Func<TO, bool>>(containsMethodExp, parameterExp);
        }

        /// <summary>
        /// Create the select part of the query
        /// </summary>
        /// <returns></returns>
        public Expression<Func<TO, T>> BuildSelector()
        {
            try
            {
                Type type = typeof(TO);
                Type typeDto = typeof(T);
                var ctor = Expression.New(typeDto);
                ParameterExpression parameter = Expression.Parameter(type, "p");
                var propertiesDto = typeDto.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var memberAssignments = propertiesDto.Select(p =>
                {
                    PropertyInfo propertyInfo = type.GetProperty(p.Name, BindingFlags.Public | BindingFlags.Instance);
                    MemberExpression memberExpression = Expression.Property(parameter, propertyInfo);
                    return Expression.Bind(p, memberExpression);
                });
                var memberInit = Expression.MemberInit(ctor, memberAssignments);
                return Expression.Lambda<Func<TO, T>>(memberInit, parameter);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in recreating the return type you should check the matched data types",ex);
            }
        }

    }
}
