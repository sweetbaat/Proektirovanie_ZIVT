using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApp
{
   public class Penalty
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public double Amount { get; set; }
    
    }
}
