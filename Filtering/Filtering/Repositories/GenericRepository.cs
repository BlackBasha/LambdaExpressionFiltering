

namespace Filtering.Repositories
{
    public class GenericRepository<T> : BaseRepository<T, DBforTestEntities> where T : class
    {
        public GenericRepository(DBforTestEntities db)
            : base(db)
        {
        }

    }

}