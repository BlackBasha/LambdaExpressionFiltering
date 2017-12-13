using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Filtering.Repositories;

namespace Filtering
{
  
   /// <summary>
   /// this class is used to build the main filter expression like x=>x.Id=5
   /// </summary>
    public class FilterExpression
    {
        /// <summary>
        /// type of filter operation
        /// </summary>
        public FilterOperations FilterOperation { get; set; }
        /// <summary>
        /// the value of the filter
        /// </summary>
        public dynamic Value { get; set; }
        /// <summary>
        /// the name of the column to filter
        /// </summary>
        public string ColumnName { get; set; }
    }


    /// <summary>
    /// This class is used as the filter bundle that contains all the parts that could used in the filtering
    /// </summary>
    public class FilterBundle<T>
    {
        public FilterBundle()
        {
           
            Orders = new List<Order>();
            Page = 1;
            PageSize = 20;
        }
        public List<FilterExpression> FilterExpression { get; set; }
        public List<Order> Orders { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
