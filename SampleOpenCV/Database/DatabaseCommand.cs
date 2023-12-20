using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTech.Sql
{
    public class DatabaseCommand
    {
        public List<DatabaseCommandColumn> Columns;

        public DatabaseCommand()
        {
            Columns = new List<DatabaseCommandColumn>();
        }

        public void Add(string columnName, string value)
        {
            Columns.Add(new DatabaseCommandColumn(columnName, value));
        }

        public void Add(string columnName, bool value)
        {
            Columns.Add(new DatabaseCommandColumn(columnName, value == true ? "1" : "0"));
        }

        public void AddNullable(string columnName, int? value)
        {
            if (value == null)
                AddNull(columnName);
            else
                Add(columnName, value.ToString());
        }

        public void AddNullable(string columnName, decimal? value)
        {
            if (value == null)
                AddNull(columnName);
            else
                Add(columnName, value.ToString());
        }

        public void AddNullable(string columnName, double? value)
        {
            if (value == null)
                AddNull(columnName);
            else
                Add(columnName, value.ToString());
        }

        public void AddNullable(string columnName, bool? value)
        {
            if (value == null)
                AddNull(columnName);
            else
                Add(columnName, value.ToString());
        }

        public void AddNullable(string columnName, DateTime? value)
        {
            if (value == null)
                AddNull(columnName);
            else
                AddDateTime(columnName, (DateTime)value);
        }

        public void AddNullable(string columnName, string value)
        {
            if (value == null)
                AddNull(columnName);
            else
                Add(columnName, value);
        }

        public void Add(string columnName, DateTime dateTime)
        {
            Add(columnName, dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public void AddDateTime(string columnName, DateTime dateTime)
        {
            Add(columnName, dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public void AddNull(string columnName)
        {
            Columns.Add(new DatabaseCommandColumn(columnName, "NULL") { AddQuotes = false });
        }

        public void AddWithoutQuotes(string columnName, string value)
        {
            Columns.Add(new DatabaseCommandColumn(columnName, value) { AddQuotes = false });
        }

        public string GetInsertCommand(string tableToUpdate, string outputColumnName)
        {
            StringBuilder sb = new StringBuilder();

            if (outputColumnName != null)
            {
                sb.Append("DECLARE @IdentityOutput TABLE (ID INT)");
                sb.Append(Environment.NewLine);
            }

            //sb.Append("INSERT INTO [");
            //sb.Append(tableToUpdate);
            //sb.Append("]");
            sb.Append("INSERT INTO ");
            sb.Append(tableToUpdate);

            if (Columns.Count > 0)
            {

                sb.Append(Environment.NewLine);
                sb.Append("(");

                for (int i = 0; i < Columns.Count; i++)
                {
                    DatabaseCommandColumn databaseCommandColumn = Columns[i];

                    sb.Append(Environment.NewLine);

                    if (i > 0)
                        sb.Append(",");

                    sb.Append("[");
                    sb.Append(databaseCommandColumn.ColumnName);
                    sb.Append("]");
                }

                sb.Append(")");
                if (outputColumnName != null)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(" OUTPUT INSERTED.[");
                    sb.Append(outputColumnName);
                    sb.Append("] INTO @IdentityOutput");
                }

                sb.Append(Environment.NewLine);
                sb.Append("VALUES");
                sb.Append(Environment.NewLine);
                sb.Append("(");

                for (int i = 0; i < Columns.Count; i++)
                {
                    DatabaseCommandColumn databaseCommandColumn = Columns[i];

                    sb.Append(Environment.NewLine);

                    if (i > 0)
                        sb.Append(",");

                    if (databaseCommandColumn.AddQuotes)
                        sb.Append("'");

                    string value = ReplaceValueCharacters(databaseCommandColumn.Value);

                    if (databaseCommandColumn.ReplaceNewLineCharacters)
                        value = ReplaceNewLineCharacters(value);

                    sb.Append(value);

                    if (databaseCommandColumn.AddQuotes)
                        sb.Append("'");
                }

                sb.Append(")");
            }
            else
            {
                if (outputColumnName != null)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("OUTPUT INSERTED.[");
                    sb.Append(outputColumnName);
                    sb.Append("] INTO @IdentityOutput ");
                }

                sb.Append("DEFAULT VALUES");
            }

            if (outputColumnName != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append("SELECT ID FROM @IdentityOutput");
            }

            sb.Append(";");

            return sb.ToString();
        }

        public string GetUpdateCommand(string tableToUpdate, string completeWhereClause)
        {
            StringBuilder sb = new StringBuilder();

            if (completeWhereClause.Substring(0, 5).ToUpper().Equals("WHERE") == false)
                throw new Exception("GetUpdateCommand - The where clause does not start with \"WHERE\": " + completeWhereClause);

            if (Columns.Count > 0 && completeWhereClause.Substring(0, 5).ToUpper().Equals("WHERE"))
            {
                sb.Append("UPDATE ");
                //sb.Append("[");
                //sb.Append(tableToUpdate);
                //sb.Append("]");
                sb.Append(tableToUpdate);
                sb.Append(" SET ");

                for (int i = 0; i < Columns.Count; i++)
                {
                    DatabaseCommandColumn databaseCommandColumn = Columns[i];

                    sb.Append(Environment.NewLine);

                    if (i > 0)
                        sb.Append(",");

                    sb.Append("[");
                    sb.Append(databaseCommandColumn.ColumnName);
                    sb.Append("] = ");

                    if (databaseCommandColumn.AddQuotes)
                        sb.Append("'");

                    string value = ReplaceValueCharacters(databaseCommandColumn.Value);

                    if (databaseCommandColumn.ReplaceNewLineCharacters)
                        value = ReplaceNewLineCharacters(value);

                    sb.Append(value);

                    if (databaseCommandColumn.AddQuotes)
                        sb.Append("'");
                }

                sb.Append(Environment.NewLine);
                sb.Append(completeWhereClause);
                sb.Append(";");
            }

            return sb.ToString();
        }

        public string GetUpdateCommand(string tableToUpdate, string whereColumnName, string whereValue)
        {
            string completeWhereClause = "WHERE [" + whereColumnName + "]='" + whereValue + "'";

            return GetUpdateCommand(tableToUpdate, completeWhereClause);
        }

        private string ReplaceValueCharacters(string valueString)
        {
            if (valueString != null)
                return valueString.Replace("'", "''");
            //return ValueString.Replace("'", "''").Replace(';', '-');

            return valueString;
        }

        private string ReplaceNewLineCharacters(string valueString)
        {
            if (valueString != null)
                return valueString.Replace(Environment.NewLine, "' + CHAR(13) + CHAR(10) + '");

            return valueString;
        }
    }
}
