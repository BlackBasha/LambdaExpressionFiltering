using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Filtering.Repositories;
using System.Diagnostics;

namespace Filtering.Test
{
    [TestClass]
    public class SomeFilteringWorks
    {
        [TestMethod]
        public void GetPositionTest()
        {
            var repo = new PositionRepository();

            var bundle = new FilterBundle<ReturnType>
            {
                FilterExpression = new List<FilterExpression>
                {
                    new FilterExpression
                    {
                        FilterOperation = FilterOperations.NotEqual,
                        Value = null,
                        ColumnName = "Email"
                    },
                    new FilterExpression
                    {
                        FilterOperation = FilterOperations.Contains,
                        Value = "Tam",
                        ColumnName = "Name"
                    }
                }
            };
            bundle.Orders.Add(new Order {ColumnName = "Name", Dir = OrderDirection.Desc});
            bundle.Orders.Add(new Order {ColumnName = "Salary", Dir = OrderDirection.Asc});
            var data = repo.GetPostions(bundle);
            Assert.IsNotNull(data);
        }


    }

}
