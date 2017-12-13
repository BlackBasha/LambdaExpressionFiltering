using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filtering.Repositories
{
    interface IPositionRepositoy
    {
        //get the positions from database
        IQueryable GetPostions();
    }
}
