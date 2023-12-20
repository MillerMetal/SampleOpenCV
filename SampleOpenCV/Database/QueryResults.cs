using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpTech.Sql
{
    public class QueryResults
    {
        public bool Error { get; set; }
        public Exception Exception { get; set; }

        public List<object[]> Rows { get; set; }

        //public object[] Row { get; set; }

        public int RowsAffected { get; set; }
    }
}
