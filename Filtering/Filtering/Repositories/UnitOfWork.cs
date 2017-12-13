using System;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using DALForInsurance.Interfaces;

namespace Filtering.Repositories
{
    public class UnitOfWork : IDisposable
    {
        private readonly DBforTestEntities _context;

        public UnitOfWork()
        {
            _context = new DBforTestEntities();
        }

        //Private Fields
        public IEFRepository<T> Repository<T>() where T : class
        {
            return new GenericRepository<T>(_context);
        }

        public TRepository GenericRepository<TRepository>() where TRepository : class
        {
           
            return (TRepository)Activator.CreateInstance(typeof(TRepository), _context);
        }

        public void Dispose()
        {
            _context.Database.Connection.Close();
            _context?.Dispose();
            GC.SuppressFinalize(this);
        }

        public int Save()
        {
            try
            {
                return _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                var fail = GenerateException(dbEx);
               
                throw fail;
            }
        }



        /// <summary>
        /// Saves The Entity State To DB Asynchronously
        /// </summary>
        public async Task SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbEntityValidationException dbEx)
            {
                var fail = GenerateException(dbEx);
                // Debug.WriteLine(fail.Message, fail);
                throw fail;
            }
        }
        public DBforTestEntities Context => _context;


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
    }
}
