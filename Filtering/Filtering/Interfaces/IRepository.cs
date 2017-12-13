
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DALForInsurance.Interfaces
{
    public interface IRepository<T> : IDisposable where T : class
    {
        /// <summary>
        /// Gets All Entities
        /// </summary>
        IQueryable<T> GetAll { get; }

        /// <summary>
        /// Get entity by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        T GetById(params object[] id);

        /// <summary>
        /// Get entity by expression
        /// </summary>
        /// <param name="ex">Expression</param>
        /// <returns>Entity</returns>
        T FirstOrDefault(Expression<Func<T,bool>> ex);

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Insert(T entity);

        /// <summary>
        /// Update entity
        /// </summary>        
        /// <param name="entity">Entity</param>
        void Update(T entity, object[] KeyIDs);

        /// <summary>
        /// Update entity via entityState
        /// </summary>        
        /// <param name="entity">Entity</param>
        void EntityStateUpdate(T entity, object[] KeyIDs);

        /// <summary>
        /// Delete entity
        /// </summary>        
        /// <param name="id">Identifier</param>
        void Delete(params object[] id);

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Delete(T entity);

    }
}