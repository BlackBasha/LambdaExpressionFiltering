using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Filtering
{
   
    public class FilterDefinitionForPropertyValue<O,T>: IFilter<O>
    {
        public FilterDefinitionForPropertyValue()
        {

        }
        public Expression<Func<O, T>> FieldSelector { get; set; }

        //public Expression<Func<O, bool>> GetFilterPredicateFor(IList<FilterExpression<O>> filterExpressionList)
        //{

        //    T typedValue;
        //    Type type = typeof(T);
        //    bool nullable = Nullable.GetUnderlyingType(type) != null;
        //    object conversionResult;
        //    BinaryExpression filterValue= Expression.MakeBinary( ExpressionType.Equal,Expression.Constant(1),Expression.Constant(1));
        //    ;
        //    Expression<Func<O, bool>> filterExValue;
        //    FilterExpression<O> firstElement = new FilterExpression<O>();
           

        //    if (filterExpressionList.Count == 0)
        //    {
        //        return null;
        //    }

        //    if (filterExpressionList.Count == 1)
        //    {
        //        firstElement = filterExpressionList.First();
        //        return GetFilterPredicateFor(firstElement.FilterOperation, firstElement.Value);
        //        //return filterValue;
        //    }

        //    //firstElement = filterExpressionList.First();
        //    //filterValue = GetFilterPredicateBinary(firstElement.FilterOperation, firstElement.Value);
        //    foreach (var item in filterExpressionList.Skip(1))
        //    {
        //        if (nullable)
        //        {
        //            type = Nullable.GetUnderlyingType(type);

        //            if (item.Value == "NULL")
        //            {
        //                conversionResult = null;
        //            }
        //            else
        //            {
        //                if (type.IsEnum)
        //                {
        //                    conversionResult = (T)Enum.Parse(type, item.Value);
        //                }
        //                else
        //                {
        //                    conversionResult = Convert.ChangeType(item.Value, type);
        //                }
        //            }

        //        }
        //        else
        //        {
        //            if (type.IsEnum)
        //            {
        //                conversionResult = (T)Enum.Parse(type, item.Value);
        //            }
        //            else
        //            {
        //                conversionResult = Convert.ChangeType(item.Value, type);
        //            }
        //        }


        //        typedValue = (T)conversionResult;

        //        FilterExpressionHelper<O> test = new FilterExpressionHelper<O>();

        //        var parameter = Expression.Parameter(typeof(O));
        //        var memberExpression = Expression.Property(parameter, item.ColumnName);
        //        var lambdaExpression = (Expression<Func<O, object>>)Expression.Lambda(memberExpression, parameter);

        //        var predicate = test.GetFilterPredicateBinary(lambdaExpression, item.FilterOperation, item.ColumnType);

        //        filterValue = Expression.AndAlso(filterValue, predicate);
        //    }

        //    return Expression.Lambda<Func<O, bool>>(filterValue);

        //}

       

        public Expression<Func<O, bool>> GetFilterPredicateFor(FilterOperations operation, object value)
        {
     
            T typedValue;
            Type type = typeof(T);
            bool nullable = Nullable.GetUnderlyingType(type) != null;
            object conversionResult;

            if (nullable)
            {
                type = Nullable.GetUnderlyingType(type);
                
                if (value == "NULL")
                {
                    conversionResult = null;
                }
                else
                {
                    if (type.IsEnum)
                    {
                        conversionResult = (T)Enum.Parse(type, value.ToString());
                    }
                    else
                    {
                        conversionResult = Convert.ChangeType(value, type);
                    }
                }

            }
            else
            {
                if (type.IsEnum)
                {
                    conversionResult = (T)Enum.Parse(type, value.ToString());
                }
                else
                {
                    conversionResult = Convert.ChangeType(value, type);
                }
            }


            typedValue = (T)conversionResult;


            var predicate = GetFilterPredicate(FieldSelector, operation, typedValue);
            return predicate;
        
        }

        public Expression<Func<O, bool>> GetFilterPredicate(Expression<Func<O, T>> selector, FilterOperations operand, T value)
        {
            //var getExpressionBody = selector.Body as MemberExpression;
            //if (getExpressionBody == null)
            //{
            //    throw new Exception("getExpressionBody is not MemberExpression: " + selector.Body);
            //}

            var parameter = selector.Parameters[0];
            var left = selector.Body;
            var right = Expression.Constant(value);
            var correctValue = Expression.Convert(right, typeof(T));
            var binaryExpression = Expression.MakeBinary(operand.ToExpressionType(), left, correctValue);
            return Expression.Lambda<Func<O, bool>>(binaryExpression, parameter);
        }

       

        
    }
}
