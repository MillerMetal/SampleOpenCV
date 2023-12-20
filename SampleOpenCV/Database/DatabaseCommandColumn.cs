using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTech.Sql
{
    public class DatabaseCommandColumn
    {
        public string ColumnName;
        public string Value;
        public bool AddQuotes;
        public bool ReplaceNewLineCharacters;

        public DatabaseCommandColumn()
        {
            AddQuotes = true;
            ReplaceNewLineCharacters = false;
        }

        public DatabaseCommandColumn(string columnName, string value) : this()
        {
            this.ColumnName = columnName;
            this.Value = value;
        }
    }
}
