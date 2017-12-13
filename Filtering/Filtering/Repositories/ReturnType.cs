using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Filtering.Repositories
{
    /// <summary>
    /// this class is a strong type of the object that would be returned forn the selection it could use whatever properties of the main class that we select form
    /// and we should define the same properties names and datatypes of the main class.
    /// </summary>
   public  class ReturnType
    {

        public string Name { get; set; }
        public int Id { get; set; }
        public DateTime? InsertDate { get; set; }
        public string Email { get; set; }
        public int? Salary { get; set; }
    }
}
