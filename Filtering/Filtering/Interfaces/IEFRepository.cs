using Filtering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Filtering.Repositories;

namespace DALForInsurance.Interfaces
{
    public interface IEFRepository<T> : IRepository<T> where T : class
    {


        /// <summary>
        /// Gets All Entities with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
        /// </summary>
        //[Obsolete("GetAllNoTracking artık Kullanılmıyor, onun yerine, Find fonksiyonunu, noTracking parametresi true olarak kullanın.")]
        IQueryable<T> GetAllNoTracking { get; }

        /// <summary>
        /// This Function will use Expression to filter results,
        /// </summary>
        /// <param name="where">where clause (Exp: x=> x.Id == Id && x.Name.StartsWith("A"))</param>
        /// <param name="noTracking">Set It to true if you want to get data with NoTracking Enabled</param>
        /// <param name="includeSoftDelete">If set to true, All Items on Db Included Softdeleted Items will be get</param>
        /// <returns></returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> where, bool noTracking = false, bool includeSoftDelete = false);

        /// <summary>
        /// This Function will use Expression to filter results,
        /// </summary>
        /// <param name="where">where clause (Exp: x=> x.Id == Id && x.Name.StartsWith("A"))</param>
        /// <param name="select">Select Expression (Exp: x=> new {Id = x.Id, Name = x.Name})</param>
        /// <param name="noTracking">Set It to true if you want to get data with NoTracking Enabled</param>
        /// <param name="includeSoftDelete">If set to true, All Items on Db Included Softdeleted Items will be get</param>
        /// <returns></returns>
        IEnumerable<TOut> Find<TOut>(Expression<Func<T, bool>> where, Expression<Func<T, TOut>> select, bool noTracking = false, bool includeSoftDelete = false);

        /// <summary>
        /// This Function will use Expression to filter results,
        /// </summary>
        /// <param name="where">where clause (Exp: x=> x.Id == Id && x.Name.StartsWith("A"))</param>
        /// <param name="page">page number, used to get page number x in paged results, if not determined all data will be returned</param>
        /// <param name="pageSize">pagesize, number of items to retrieve</param>
        /// <param name="noTracking">Set It to true if you want to get data with NoTracking Enabled</param>
        /// <param name="includeSoftDelete">If set to true, All Items on Db Included Softdeleted Items will be get</param>
        /// <returns></returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> where, int page, int pageSize, bool noTracking = false, bool includeSoftDelete = false);

        /// <summary>
        /// This Function will use Expression to filter results,
        /// </summary>
        /// <param name="where">where clause (Exp: x=> x.Id == Id && x.Name.StartsWith("A"))</param>
        /// <param name="page">page number, used to get page number x in paged results, if not determined all data will be returned</param>
        /// <param name="pageSize">pagesize, number of items to retrieve</param>
        /// <param name="total">Total Number of Records</param>
        /// <param name="noTracking">Set It to true if you want to get data with NoTracking Enabled</param>
        /// <param name="includeSoftDelete">If set to true, All Items on Db Included Softdeleted Items will be get</param>
        /// <returns></returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> where, int page, int pageSize, out int total, bool noTracking = false, bool includeSoftDelete = false);

        /// <summary>
        /// This Function will use Expression to filter results,
        /// </summary>
        /// <param name="where">where clause (Exp: x=> x.Id == Id && x.Name.StartsWith("A"))</param>
        /// <param name="select">Select Expression (Exp: x=> new {Id = x.Id, Name = x.Name})</param>
        /// <param name="page">page number, used to get page number x in paged results, if not determined all data will be returned</param>
        /// <param name="pageSize">pagesize, number of items to retrieve</param>
        /// <param name="noTracking">Set It to true if you want to get data with NoTracking Enabled</param>
        /// <param name="includeSoftDelete">If set to true, All Items on Db Included Softdeleted Items will be get</param>
        /// <returns></returns>
        IEnumerable<TOut> Find<TOut>(Expression<Func<T, bool>> where, Expression<Func<T, TOut>> select, int page, int pageSize, bool noTracking = false, bool includeSoftDelete = false);

        /// <summary>
        /// This Function will use Expression to filter results,
        /// </summary>
        /// <param name="where">where clause (Exp: x=> x.Id == Id && x.Name.StartsWith("A"))</param>
        /// <param name="select">Select Expression (Exp: x=> new {Id = x.Id, Name = x.Name})</param>
        /// <param name="page">page number, used to get page number x in paged results, if not determined all data will be returned</param>
        /// <param name="pageSize">pagesize, number of items to retrieve</param>
        /// <param name="total">Total Number of Records</param>
        /// <param name="noTracking">Set It to true if you want to get data with NoTracking Enabled</param>
        /// <param name="includeSoftDelete">If set to true, All Items on Db Included Softdeleted Items will be get</param>
        /// <returns></returns>
        IEnumerable<TOut> Find<TOut>(Expression<Func<T, bool>> where, Expression<Func<T, TOut>> select, int page, int pageSize, out int total, bool noTracking = false, bool includeSoftDelete = false);


        //TODO: Add FindIncluding Functions
        /// <summary>
        /// Gets All Entities Including Eager Loaded Relations
        /// </summary>
        /// <param name="includedProperties">Included Relations</param>
        /// <returns>Entities with eager loaded selected relations</returns>
        IQueryable<T> GetAllIncluding(params Expression<Func<T, object>>[] includedProperties);

        IEnumerable<TOut> FindWithOrder<TOut>(Expression<Func<T, bool>> @where, Expression<Func<T, TOut>> @select, int page, int pageSize, out int total, IList<Order> orders, bool noTracking = false, bool includeSoftDelete = false);

        IEnumerable<TOut> Find<TOut>(FilterBundle<TOut> bundle, bool noTracking = false, bool includeSoftDelete = false);

        /// <summary>
        /// Get all Entities Including Eager loaded Relations also nested relations.
        /// </summary>
        /// <param name="includedProperties">Comma seperated table (relation) names to include in result set.</param>
        /// <returns>Entities with Eager loaded selected relations</returns>
        IQueryable<T> GetAllIncluding(string includedProperties);

        /// <summary>
        /// This Function Returns All Items Included SoftDeleted Items if related entity is implementing ISoftDeleteEnabled
        /// </summary>
        IQueryable<T> GetAllIncludeSoftDeleted { get; }

        void InsertRange(List<T> entities);

        void StateDeleted(T entity);

        /// <summary>
        /// Bulk inserts objects to table
        /// </summary>
        /// <param name="rows">List of type [your type]</param>
        /// <param name="tableName">TableName to insert data in</param>
        void BulkInsert(List<T> rows, string tableName);

        ///// <summary>
        ///// Bulk insert for Nested Objects
        ///// You should create 
        ///// </summary>
        ///// <param name="rows"></param>
        ///// <param name="tableName"></param>
        //void BulkInsert(List<object> rows, string tableName);

        void HardDelete(T entity);
        void DeleteRange(List<T> entities);
    }
}
