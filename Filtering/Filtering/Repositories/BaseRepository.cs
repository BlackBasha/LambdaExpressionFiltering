using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using DALForInsurance.Interfaces;

namespace Filtering.Repositories
{
    public class BaseRepository<T, TC> : IEFRepository<T> where T : class where TC : DbContext
    {
        private readonly TC _context;
        private IDbSet<T> _dbSet;

        public BaseRepository(TC context)
        {
            _context = context;
        }

        protected virtual IDbSet<T> DbSet => _dbSet ?? (_dbSet = _context.Set<T>());

        /// <summary>
        /// Insert An Entity Into Db using EntityFramework
        /// </summary>
        /// <param name="entity">Entity to Insert</param>
        public virtual void Insert(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                this.DbSet.Add(entity);
            }
            catch (DbEntityValidationException dbEx)
            {
                var fail = GenerateException(dbEx);
                throw fail;
            }
        }

        public void InsertRange(List<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                if (!entities.Any())
                    return;

                ((this.DbSet) as DbSet<T>)?.AddRange(entities);
            }
            catch (DbEntityValidationException dbEx)
            {
                var fail = GenerateException(dbEx);
                throw fail;
            }
        }


        public virtual void Delete(params object[] id)
        {
            var entityToDelete = DbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void DeleteRange(List<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                if (!entities.Any())
                    return;

                ((this.DbSet) as DbSet<T>)?.RemoveRange(entities);
            }
            catch (DbEntityValidationException dbEx)
            {
                var fail = GenerateException(dbEx);
                throw fail;
            }
        }

        public virtual void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            
            this.DbSet.Remove(entity);
        }

        public virtual void StateDeleted(T entity)
        {
            _context.Entry(entity).State = EntityState.Deleted;
        }

      

        public void HardDelete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            this.DbSet.Remove(entity);
        }

        /// <summary>
        /// Update entity using entityframework
        /// </summary>        
        /// <param name="entity">Entity</param>
        public virtual void Update(T entity, object[] KeyIDs)
        {
            var fromEntity = _context.Entry(entity);
            var existingEntity = _context.Set<T>().Find(KeyIDs);
            if (existingEntity != null) // If entity found to update
            {
                var toUpdateEntity = _context.Entry(existingEntity);

                foreach (var property in typeof(T).GetProperties())
                {
                    if (
                        !property.PropertyType.FullName.Contains("System.Collection")
                        && property.PropertyType.FullName.Contains("System.")
                        && fromEntity.Property(property.Name) != null
                        && fromEntity.Property(property.Name).CurrentValue != null
                        && fromEntity.Property(property.Name).CurrentValue.ToString() != "01.01.0001 00:00:00")
                    {
                        toUpdateEntity.Property(property.Name).IsModified = true;
                        toUpdateEntity.Property(property.Name).CurrentValue = fromEntity.Property(property.Name).CurrentValue;
                    }
                }
            }
        }

        /// <summary>
        /// Update entity using entityframework state
        /// </summary>        
        /// <param name="entity">Entity</param>
        public virtual void EntityStateUpdate(T entity, object[] KeyIDs)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (_context.Entry(entity).State == EntityState.Detached)
            {
                DbSet.Attach(entity);
            }

            _context.Entry(entity).State = EntityState.Modified;
        }


        /// <summary>
        /// Gets All Entities
        /// </summary>
        public virtual IQueryable<T> GetAll
        {
            get
            {
                if (typeof(ISoftDeleteEnabled).IsAssignableFrom(typeof(T)))
                {
                    var exp2 = GetExpression();
                    return DbSet.Where<T>(exp2);
                }

                return DbSet;
            }
        }


        /// <summary>
        /// Gets All Entities with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
        /// </summary>
        public virtual IQueryable<T> GetAllNoTracking
        {
            get
            {
                if (typeof(ISoftDeleteEnabled).IsAssignableFrom(typeof(T)))
                {
                    var exp2 = GetExpression();
                    return DbSet.Where<T>(exp2).AsNoTracking();
                }

                return DbSet.AsNoTracking();
            }
        }


        private IQueryable<T> PrivateFind(Expression<Func<T, bool>> @where, bool noTracking = false,
            bool includeSoftDelete = false)
        {
            var result = noTracking ? DbSet.AsNoTracking().Where(@where) : DbSet.Where(@where);

            // If includeSoftDelete is true then do not filter result
            if (!includeSoftDelete && typeof(ISoftDeleteEnabled).IsAssignableFrom(typeof(T)))
            {
                // if includeSoftDelete is false filter IsDeleted records
                var exp2 = GetExpression();
                result = result.Where<T>(exp2);
            }

            return result;
        }

        /// <summary>
        /// This Function will use Expression to filter results,
        /// </summary>
        /// <param name="where">where clause (Exp: x=> x.Id == Id && x.Name.StartsWith("A"))</param>
        /// <param name="noTracking"></param>
        /// <param name="includeSoftDelete"></param>
        /// <returns></returns>
        public IEnumerable<T> Find(Expression<Func<T, bool>> @where, bool noTracking = false, bool includeSoftDelete = false)
        {
            return PrivateFind(where, noTracking, includeSoftDelete);
        }

        /// <summary>
        /// This Function will use Expression to filter results,
        /// </summary>
        /// <param name="where">where clause (Exp: x=> x.Id == Id && x.Name.StartsWith("A"))</param>
        /// <param name="select">The Select Expression</param>
        /// <param name="noTracking"></param>
        /// <param name="includeSoftDelete"></param>
        /// <returns></returns>
        public IEnumerable<TOut> Find<TOut>(Expression<Func<T, bool>> @where, Expression<Func<T, TOut>> @select, bool noTracking = false, bool includeSoftDelete = false)
        {
            return PrivateFind(@where, noTracking, includeSoftDelete).Select(@select);
        }

        /// <summary>
        /// This Function will use Expression to filter results,
        /// </summary>
        /// <param name="where">where clause (Exp: x=> x.Id == Id && x.Name.StartsWith("A"))</param>        
        /// <param name="page">(optional) page number, used to get page number x in paged results, if not determined all data will be returned</param>
        /// <param name="pageSize">(optional) pagesize, number of items to retrieve</param>
        /// <param name="noTracking"></param>
        /// <param name="includeSoftDelete"></param>
        /// <returns></returns>
        public IEnumerable<T> Find(Expression<Func<T, bool>> @where, int page, int pageSize, bool noTracking = false, bool includeSoftDelete = false)
        {
            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page), $"{nameof(page)} should be larger than zero");

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"{nameof(pageSize)} should be larger than zero");

            return PrivateFind(@where, noTracking, includeSoftDelete).OrderBy(o => 1).Skip((page - 1) * pageSize).Take(pageSize);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> @where, int page, int pageSize, out int total, bool noTracking = false, bool includeSoftDelete = false)
        {
            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page), $"{nameof(page)} should be larger than zero");

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"{nameof(pageSize)} should be larger than zero");

            var data = PrivateFind(@where, noTracking, includeSoftDelete);
            total = data.Count();

            return data.OrderBy(o => 1).Skip((page - 1) * pageSize).Take(pageSize);
        }


        public IEnumerable<T> FindWithOrder(Expression<Func<T, bool>> @where, int page, int pageSize, out int total,IList<Order> orders, bool noTracking = false, bool includeSoftDelete = false)
        {
            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page), $"{nameof(page)} should be larger than zero");

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"{nameof(pageSize)} should be larger than zero");

            var data = PrivateFind(@where, noTracking, includeSoftDelete);
            total = data.Count();

            if (orders?.Count>0)
            {
                foreach (var orderItem in orders)
                {
                   data = data.OrderBy(orderItem.ColumnName,orderItem.Dir);
                }
                return data.Skip((page - 1) * pageSize).Take(pageSize);
            }

            return data.OrderBy(o => 1).Skip((page - 1) * pageSize).Take(pageSize);
        }


        public IEnumerable<TOut> Find<TOut>(Expression<Func<T, bool>> @where, Expression<Func<T, TOut>> @select, int page, int pageSize, bool noTracking = false, bool includeSoftDelete = false)
        {
            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page), $"{nameof(page)} should be larger than zero");

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"{nameof(pageSize)} should be larger than zero");

            return PrivateFind(@where, noTracking, includeSoftDelete).OrderBy(o => 1).Skip((page - 1) * pageSize).Take(pageSize).Select(select);
        }

        public IEnumerable<TOut> FindWithOrder<TOut>(Expression<Func<T, bool>> @where, Expression<Func<T, TOut>> @select, int page, int pageSize, IList<Order> orders, bool noTracking = false, bool includeSoftDelete = false)
        {
            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page), $"{nameof(page)} should be larger than zero");

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"{nameof(pageSize)} should be larger than zero");

            var data = PrivateFind(@where, noTracking, includeSoftDelete);

            if (orders?.Count > 0)
            {
                foreach (var orderItem in orders)
                {
                    data = data.OrderBy(orderItem.ColumnName, orderItem.Dir);
                }
                return data.Skip((page - 1) * pageSize).Take(pageSize).Select(select);
            }
            return data.OrderBy(o => 1).Skip((page - 1) * pageSize).Take(pageSize).Select(select);
        }

        public IEnumerable<TOut> Find<TOut>(FilterBundle<TOut> bundle, bool noTracking = false, bool includeSoftDelete = false)
        {
            if (bundle.Page <= 0)
                throw new ArgumentOutOfRangeException(nameof(bundle.Page), $"{nameof(bundle.Page)} should be larger than zero");

            if (bundle.PageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bundle.PageSize), $"{nameof(bundle.PageSize)} should be larger than zero");



            // Where  Expression

            FilterExpressionHelper<T, TOut> filterExpressionHelper = new FilterExpressionHelper<T, TOut>();

            var whereFilter = filterExpressionHelper.GetFilterPredicateForWhere(bundle.FilterExpression);



            //select 


            var select = filterExpressionHelper.BuildSelector();


            var data = PrivateFind(whereFilter, noTracking, includeSoftDelete);

            //order data 

            return filterExpressionHelper.GetFilterPredicateForSort(bundle.Orders, data.Select(select)).Skip((bundle.Page - 1) * bundle.PageSize).Take(bundle.PageSize);

        }

        public IEnumerable<TOut> Find<TOut>(Expression<Func<T, bool>> @where, Expression<Func<T, TOut>> @select, int page, int pageSize, out int total, bool noTracking = false, bool includeSoftDelete = false)
        {
            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page), $"{nameof(page)} should be larger than zero");

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"{nameof(pageSize)} should be larger than zero");

            var data = PrivateFind(@where, noTracking, includeSoftDelete);

            total = data.Count();

            return data.OrderBy(o => 1).Skip((page - 1) * pageSize).Take(pageSize).Select(select);
        }

        public IEnumerable<TOut> FindWithOrder<TOut>(Expression<Func<T, bool>> @where, Expression<Func<T, TOut>> @select, int page, int pageSize, out int total, IList<Order> orders, bool noTracking = false, bool includeSoftDelete = false)
        {
            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page), $"{nameof(page)} should be larger than zero");

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"{nameof(pageSize)} should be larger than zero");

            var data = PrivateFind(@where, noTracking, includeSoftDelete);

            total = data.Count();

           


            if (orders?.Count > 0)
            {
                foreach (var orderItem in orders)
                {
                    data = data.OrderBy(orderItem.ColumnName, orderItem.Dir);
                }
                return data.Skip((page - 1) * pageSize).Take(pageSize).Select(select);
            }

            return data.OrderBy(o => 1).Skip((page - 1) * pageSize).Take(pageSize).Select(select);
        }

        private static Expression<Func<T, bool>> GetExpression()
        {
            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            MemberExpression member = Expression.Property(param, "IsDeleted");
            Expression exp = Expression.Not(member);
            var exp2 = Expression.Lambda<Func<T, bool>>(exp, param);
            return exp2;
        }

        /// <summary>
        /// Gets All Entities Including Eager Loaded Relations
        /// </summary>
        /// <param name="includedProperties">Included Relations</param>
        /// <returns>Entities with eager loaded selected relations</returns>
        public virtual IQueryable<T> GetAllIncluding(params Expression<Func<T, object>>[] includedProperties)
        {
            var entities = DbSet.AsQueryable();

            if (typeof(ISoftDeleteEnabled).IsAssignableFrom(typeof(T)))
            {
                var exp2 = GetExpression();
                entities = DbSet.Where<T>(exp2).AsNoTracking();
            }

            foreach (var includedPropery in includedProperties)
            {
                entities = entities.Include(includedPropery);
            }

            return entities;
        }

        /// <summary>
        /// Get all Entities Including Eager loaded Relations also nested relations.
        /// </summary>
        /// <param name="includedProperties">Comma seperated table (relation) names to include in result set.</param>
        /// <returns>Entities with Eager loaded selected relations</returns>
        public virtual IQueryable<T> GetAllIncluding(string includedProperties)
        {
            var entities = DbSet.AsQueryable();
            var relations = includedProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (typeof(ISoftDeleteEnabled).IsAssignableFrom(typeof(T)))
            {
                var exp2 = GetExpression();
                entities = DbSet.Where<T>(exp2).AsNoTracking();
            }

            foreach (var property in relations)
            {
                entities = entities.Include(property);
            }

            return entities;
        }

        public IQueryable<T> GetAllIncludeSoftDeleted => DbSet;

        /// <summary>
        /// Get entity by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        public virtual T GetById(params object[] id)
        {
            var item = this.DbSet.Find(id);

            if (item != null)
                if (typeof(ISoftDeleteEnabled).IsAssignableFrom(typeof(T)))
                    item = (bool)typeof(T).GetProperty("IsDeleted").GetValue(item) == false ? item : null;

            return item;
        }

        /// <summary>
        /// Generate a meaningful exception
        /// </summary>
        /// <param name="dbEx">Db Exception</param>
        /// <returns></returns>
        private static Exception GenerateException(DbEntityValidationException dbEx)
        {
            var msg = string.Empty;

            foreach (var validationErrors in dbEx.EntityValidationErrors)
                foreach (var validationError in validationErrors.ValidationErrors)
                    msg += Environment.NewLine +
                           $"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}";

            var fail = new Exception(msg, dbEx);
            return fail;
        }

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this._disposed = true;
        }

        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> ex)
        {
            var item = this.DbSet.FirstOrDefault(ex);

            if (item != null)
                if (typeof(ISoftDeleteEnabled).IsAssignableFrom(typeof(T)))
                    item = (bool)typeof(T).GetProperty("IsDeleted").GetValue(item) == false ? item : null;

            return item;
        }

        

        public void BulkInsert(List<T> rows, string tableName)
        {
            throw new NotImplementedException();
        }
    }
}
