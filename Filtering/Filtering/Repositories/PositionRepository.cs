using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Filtering.Repositories
{
    /// <summary>
    /// the implementation of the position repository 
    /// </summary>
    public class PositionRepository 
    {
        private readonly UnitOfWork _unitOfwork;
        private readonly DALForInsurance.Interfaces.IEFRepository<Position> _newGenericRepository;


        public PositionRepository()
        {

            _unitOfwork = new UnitOfWork();
            _newGenericRepository = _unitOfwork.Repository<Position>();

        }

        public List<ReturnType> GetPostions(FilterBundle<ReturnType> filterBundle)
        {
           return _newGenericRepository.Find(filterBundle).ToList();
        }
    }
}
