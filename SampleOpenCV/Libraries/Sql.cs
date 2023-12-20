using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.SqlClient;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;

using APlusInnovations.Common;
using System.Reflection;

namespace APlusInnovations.Sql
{
    public static class SqlTools
    {
        //public static List<InformationSchemaColumnsRow> GetInformationSchemaColumns(string databaseConnectionString, string tableName)
        //{
        //    List<InformationSchemaColumnsRow> returnValue = new List<InformationSchemaColumnsRow>();

        //    SqlConnection sqlConnection = null;

        //    SqlDataReader sqlDataReader = null;

        //    //string queryString = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS ORDER BY TABLE_NAME, ORDINAL_POSITION";

        //    string queryString = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS";

        //    if (tableName.HasValue())
        //        queryString += " WHERE TABLE_NAME = '" + tableName + "'";

        //    queryString += " ORDER BY TABLE_NAME, ORDINAL_POSITION";

        //    try
        //    {
        //        sqlConnection = new SqlConnection(databaseConnectionString);

        //        sqlConnection.Open();

        //        SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection);

        //        sqlDataReader = sqlCommand.ExecuteReader();

        //        if (sqlDataReader.RecordsAffected == -1)
        //        {
        //            System.Data.DataTable dataTable = sqlDataReader.GetSchemaTable();

        //            int dataTableColumnNameIndex = dataTable.Columns.IndexOf("ColumnName");

        //            int tableCatalogIndex = -1;
        //            int tableNameIndex = -1;
        //            int columnNameIndex = -1;
        //            int ordinalPositionIndex = -1;
        //            int columnDefaultIndex = -1;
        //            int isNullableIndex = -1;
        //            int dataTypeIndex = -1;
        //            int characterMaximumLengthIndex = -1;
        //            int characterOctetLengthIndex = -1;
        //            int numericPrecisionIndex = -1;
        //            int numericPrecisionRadixIndex = -1;
        //            int numericScaleIndex = -1;
        //            int dateTimePrecisionIndex = -1;

        //            for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
        //            {
        //                System.Data.DataRow dataRow = dataTable.Rows[rowIndex];

        //                string dataTableColumnName = dataRow[dataTableColumnNameIndex].ToString();

        //                if (dataTableColumnName.ToUpper() == "TABLE_CATALOG")
        //                    tableCatalogIndex = rowIndex;

        //                if (dataTableColumnName.ToUpper() == "TABLE_NAME")
        //                    tableNameIndex = rowIndex;

        //                if (dataTableColumnName.ToUpper() == "COLUMN_NAME")
        //                    columnNameIndex = rowIndex;

        //                if (dataTableColumnName.ToUpper() == "ORDINAL_POSITION")
        //                    ordinalPositionIndex = rowIndex;

        //                if (dataTableColumnName.ToUpper() == "COLUMN_DEFAULT")
        //                    columnDefaultIndex = rowIndex;

        //                if (dataTableColumnName.ToUpper() == "IS_NULLABLE")
        //                    isNullableIndex = rowIndex;

        //                if (dataTableColumnName.ToUpper() == "DATA_TYPE")
        //                    dataTypeIndex = rowIndex;

        //                if (dataTableColumnName.ToUpper() == "CHARACTER_MAXIMUM_LENGTH")
        //                    characterMaximumLengthIndex = rowIndex;

        //                if (dataTableColumnName.ToUpper() == "CHARACTER_OCTET_LENGTH")
        //                    characterOctetLengthIndex = rowIndex;

        //                if (dataTableColumnName.ToUpper() == "NUMERIC_PRECISION")
        //                    numericPrecisionIndex = rowIndex;

        //                if (dataTableColumnName.ToUpper() == "NUMERIC_PRECISION_RADIX")
        //                    numericPrecisionRadixIndex = rowIndex;

        //                if (dataTableColumnName.ToUpper() == "NUMERIC_SCALE")
        //                    numericScaleIndex = rowIndex;

        //                if (dataTableColumnName.ToUpper() == "DATETIME_PRECISION")
        //                    dateTimePrecisionIndex = rowIndex;
        //            }

        //            //MessageBox.Show("tableNameIndex = " + tableNameIndex);

        //            while (sqlDataReader.Read())
        //            {
        //                object[] rowArray = new object[sqlDataReader.FieldCount];

        //                sqlDataReader.GetValues(rowArray);

        //                InformationSchemaColumnsRow informationSchemaColumnsRow = new InformationSchemaColumnsRow();

        //                if (tableCatalogIndex != -1)
        //                    informationSchemaColumnsRow.TableCatalog = VariableTools.TryCast<string>(rowArray[tableCatalogIndex], "");

        //                if (tableNameIndex != -1)
        //                    informationSchemaColumnsRow.TableName = VariableTools.TryCast<string>(rowArray[tableNameIndex], "");

        //                if (columnNameIndex != -1)
        //                    informationSchemaColumnsRow.ColumnName = VariableTools.TryCast<string>(rowArray[columnNameIndex], "");

        //                if (ordinalPositionIndex != -1)
        //                    informationSchemaColumnsRow.OrdinalPosition = VariableTools.TryCast<int>(rowArray[ordinalPositionIndex], 0);

        //                if (columnDefaultIndex != -1)
        //                    informationSchemaColumnsRow.ColumnDefault = VariableTools.TryCast<string>(rowArray[columnDefaultIndex], "");

        //                if (isNullableIndex != -1)
        //                    informationSchemaColumnsRow.IsNullable = VariableTools.TryCast<string>(rowArray[isNullableIndex], "");

        //                if (dataTypeIndex != -1)
        //                    informationSchemaColumnsRow.DataType = VariableTools.TryCast<string>(rowArray[dataTypeIndex], "");

        //                if (characterMaximumLengthIndex != -1)
        //                    informationSchemaColumnsRow.CharacterMaximumLength = VariableTools.TryCast<int>(rowArray[characterMaximumLengthIndex], 0);

        //                if (characterOctetLengthIndex != -1)
        //                    informationSchemaColumnsRow.CharacterOctetLength = VariableTools.TryCast<int>(rowArray[characterOctetLengthIndex], 0);

        //                if (numericPrecisionIndex != -1)
        //                    informationSchemaColumnsRow.NumericPrecision = VariableTools.TryCast<byte>(rowArray[numericPrecisionIndex], (byte)0);

        //                if (numericPrecisionRadixIndex != -1)
        //                    informationSchemaColumnsRow.NumericPrecisionRadix = (Int16)VariableTools.TryCast<Int16>(rowArray[numericPrecisionRadixIndex], 0);

        //                if (numericScaleIndex != -1)
        //                    informationSchemaColumnsRow.NumericScale = VariableTools.TryCast<int>(rowArray[numericScaleIndex], 0);

        //                if (dateTimePrecisionIndex != -1)
        //                    informationSchemaColumnsRow.DateTimePrecision = (Int16)VariableTools.TryCast<Int16>(rowArray[dateTimePrecisionIndex], 0);


        //                returnValue.Add(informationSchemaColumnsRow);
        //            }
        //        }
        //        else
        //        {

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Execution error:" + Environment.NewLine + Environment.NewLine + ex.Message);
        //    }
        //    finally
        //    {
        //        if (sqlDataReader != null)
        //            sqlDataReader.Close();

        //        if (sqlConnection != null)
        //            sqlConnection.Close();
        //    }

        //    return returnValue;
        //}

        public static List<InformationSchemaColumnsRow> GetInformationSchemaColumns(string databaseConnectionString, string tableName)
        {
            List<string> primaryKeyList = new List<string>();

            string queryString = @"
SELECT column_name
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE OBJECTPROPERTY(OBJECT_ID(constraint_name), 'IsPrimaryKey') = 1
AND table_name = '" + tableName + "'";

            List<object[]> results = ReturnResults(databaseConnectionString, queryString);

            if (results.Count > 0)
            {
                foreach (object[] row in results)
                {
                    string primaryKeyColumn = row[0].TryCast<string>(null);

                    if (primaryKeyColumn.HasValue())
                        primaryKeyList.Add(primaryKeyColumn);
                }
            }




            List<InformationSchemaColumnsRow> informationSchemaColumnsRowList = new List<InformationSchemaColumnsRow>();

            //SqlConnection sqlConnection = null;

            //SqlDataReader sqlDataReader = null;
            
            //string queryString = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS ORDER BY TABLE_NAME, ORDINAL_POSITION";

            queryString = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS";

            if (tableName.HasValue())
                queryString += " WHERE TABLE_NAME = '" + tableName + "'";

            queryString += " ORDER BY TABLE_NAME, ORDINAL_POSITION";

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(databaseConnectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                        {
                            sqlCommand.CommandTimeout = 120;

                            using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                            {
                                if (sqlDataReader.RecordsAffected == -1)
                                {
                                    System.Data.DataTable dataTable = sqlDataReader.GetSchemaTable();

                                    int dataTableColumnNameIndex = dataTable.Columns.IndexOf("ColumnName");

                                    int tableCatalogIndex = -1;
                                    int tableNameIndex = -1;
                                    int columnNameIndex = -1;
                                    int ordinalPositionIndex = -1;
                                    int columnDefaultIndex = -1;
                                    int isNullableIndex = -1;
                                    int dataTypeIndex = -1;
                                    int characterMaximumLengthIndex = -1;
                                    int characterOctetLengthIndex = -1;
                                    int numericPrecisionIndex = -1;
                                    int numericPrecisionRadixIndex = -1;
                                    int numericScaleIndex = -1;
                                    int dateTimePrecisionIndex = -1;

                                    for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
                                    {
                                        System.Data.DataRow dataRow = dataTable.Rows[rowIndex];

                                        string dataTableColumnName = dataRow[dataTableColumnNameIndex].ToString();

                                        if (dataTableColumnName.ToUpper() == "TABLE_CATALOG")
                                            tableCatalogIndex = rowIndex;

                                        if (dataTableColumnName.ToUpper() == "TABLE_NAME")
                                            tableNameIndex = rowIndex;

                                        if (dataTableColumnName.ToUpper() == "COLUMN_NAME")
                                            columnNameIndex = rowIndex;

                                        if (dataTableColumnName.ToUpper() == "ORDINAL_POSITION")
                                            ordinalPositionIndex = rowIndex;

                                        if (dataTableColumnName.ToUpper() == "COLUMN_DEFAULT")
                                            columnDefaultIndex = rowIndex;

                                        if (dataTableColumnName.ToUpper() == "IS_NULLABLE")
                                            isNullableIndex = rowIndex;

                                        if (dataTableColumnName.ToUpper() == "DATA_TYPE")
                                            dataTypeIndex = rowIndex;

                                        if (dataTableColumnName.ToUpper() == "CHARACTER_MAXIMUM_LENGTH")
                                            characterMaximumLengthIndex = rowIndex;

                                        if (dataTableColumnName.ToUpper() == "CHARACTER_OCTET_LENGTH")
                                            characterOctetLengthIndex = rowIndex;

                                        if (dataTableColumnName.ToUpper() == "NUMERIC_PRECISION")
                                            numericPrecisionIndex = rowIndex;

                                        if (dataTableColumnName.ToUpper() == "NUMERIC_PRECISION_RADIX")
                                            numericPrecisionRadixIndex = rowIndex;

                                        if (dataTableColumnName.ToUpper() == "NUMERIC_SCALE")
                                            numericScaleIndex = rowIndex;

                                        if (dataTableColumnName.ToUpper() == "DATETIME_PRECISION")
                                            dateTimePrecisionIndex = rowIndex;
                                    }

                                    //MessageBox.Show("tableNameIndex = " + tableNameIndex);

                                    while (sqlDataReader.Read())
                                    {
                                        object[] rowArray = new object[sqlDataReader.FieldCount];

                                        sqlDataReader.GetValues(rowArray);

                                        InformationSchemaColumnsRow informationSchemaColumnsRow = new InformationSchemaColumnsRow();

                                        if (tableCatalogIndex != -1)
                                            informationSchemaColumnsRow.TableCatalog = VariableTools.TryCast<string>(rowArray[tableCatalogIndex], "");

                                        if (tableNameIndex != -1)
                                            informationSchemaColumnsRow.TableName = VariableTools.TryCast<string>(rowArray[tableNameIndex], "");

                                        if (columnNameIndex != -1)
                                            informationSchemaColumnsRow.ColumnName = VariableTools.TryCast<string>(rowArray[columnNameIndex], "");

                                        if (ordinalPositionIndex != -1)
                                            informationSchemaColumnsRow.OrdinalPosition = VariableTools.TryCast<int>(rowArray[ordinalPositionIndex], 0);

                                        if (columnDefaultIndex != -1)
                                            informationSchemaColumnsRow.ColumnDefault = VariableTools.TryCast<string>(rowArray[columnDefaultIndex], "");

                                        if (isNullableIndex != -1)
                                            informationSchemaColumnsRow.IsNullable = VariableTools.TryCast<string>(rowArray[isNullableIndex], "");

                                        if (dataTypeIndex != -1)
                                            informationSchemaColumnsRow.DataType = VariableTools.TryCast<string>(rowArray[dataTypeIndex], "");

                                        if (characterMaximumLengthIndex != -1)
                                            informationSchemaColumnsRow.CharacterMaximumLength = VariableTools.TryCast<int>(rowArray[characterMaximumLengthIndex], 0);

                                        if (characterOctetLengthIndex != -1)
                                            informationSchemaColumnsRow.CharacterOctetLength = VariableTools.TryCast<int>(rowArray[characterOctetLengthIndex], 0);

                                        if (numericPrecisionIndex != -1)
                                            informationSchemaColumnsRow.NumericPrecision = VariableTools.TryCast<byte>(rowArray[numericPrecisionIndex], (byte)0);

                                        if (numericPrecisionRadixIndex != -1)
                                            informationSchemaColumnsRow.NumericPrecisionRadix = (Int16)VariableTools.TryCast<Int16>(rowArray[numericPrecisionRadixIndex], 0);

                                        if (numericScaleIndex != -1)
                                            informationSchemaColumnsRow.NumericScale = VariableTools.TryCast<int>(rowArray[numericScaleIndex], 0);

                                        if (dateTimePrecisionIndex != -1)
                                            informationSchemaColumnsRow.DateTimePrecision = (Int16)VariableTools.TryCast<Int16>(rowArray[dateTimePrecisionIndex], 0);


                                        Type type = SqlTools.GetCSharpType(informationSchemaColumnsRow.DataType);
                                        string typeString = type.Name;


                                        bool primaryKey = false;

                                        if (primaryKeyList.HasValue())
                                            primaryKey = primaryKeyList.Contains(informationSchemaColumnsRow.ColumnName) ? true : false;

                                        bool timestamp = informationSchemaColumnsRow.DataType.EqualsIgnoreCaseOrdinal("timestamp");

                                        if (!type.IsNullable())
                                            typeString = "Nullable<" + typeString + ">";

                                        string defaultValueString = "null";

                                        if (informationSchemaColumnsRow.ColumnDefault.HasValue())
                                        {
                                            string defaultValue = informationSchemaColumnsRow.ColumnDefault.Trim('(', ')');

                                            if (defaultValue.HasValue())
                                            {
                                                Debug.WriteLine("defaultValue = " + defaultValue);
                                                Debug.WriteLine("type = " + type.ToString());

                                                if (type == typeof(int))
                                                {
                                                    int? temp = defaultValue.TryCastNullable<int>();

                                                    if (temp != null)
                                                        defaultValueString = temp.ToString();
                                                }
                                                else if (type == typeof(bool))
                                                {
                                                    bool? temp = defaultValue.TryCastNullable<bool>();

                                                    Debug.WriteLine("temp = " + temp);

                                                    if (temp != null)
                                                        defaultValueString = temp.ToString().ToLower();
                                                }
                                                else if (type == typeof(double))
                                                {
                                                    double? temp = defaultValue.TryCastNullable<double>();

                                                    Debug.WriteLine("temp = " + temp);

                                                    if (temp != null)
                                                        defaultValueString = temp.ToString().ToLower() + "D";
                                                }
                                            }
                                        }

                                        informationSchemaColumnsRow.IsPrimaryKey = primaryKey;
                                        informationSchemaColumnsRow.IsTimestamp = timestamp;

                                        informationSchemaColumnsRow.CSharpType = type;
                                        informationSchemaColumnsRow.CSharpTypeString = typeString;
                                        informationSchemaColumnsRow.DefaultValueString = defaultValueString;




                                        informationSchemaColumnsRowList.Add(informationSchemaColumnsRow);
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex.Message);
                        informationSchemaColumnsRowList = new List<InformationSchemaColumnsRow>();
                    }
                    finally
                    {
                        sqlConnection.Close();
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Execution error:" + Environment.NewLine + Environment.NewLine + ex.Message);
            }

            return informationSchemaColumnsRowList;
        }

        public static List<string> GetPrimaryKeyColumns(string databaseConnectionString, string tableName)
        {
            string queryString = @"
SELECT column_name
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE OBJECTPROPERTY(OBJECT_ID(constraint_name), 'IsPrimaryKey') = 1
AND table_name = '" + tableName + "'";

            List<object[]> results = ReturnResults(databaseConnectionString, queryString);

            if (results.Count > 0)
            {
                List<string> primaryKeyList = new List<string>();

                foreach (object[] row in results)
                {
                    string primaryKeyColumn = row[0].TryCast<string>(null);

                    if (primaryKeyColumn.HasValue())
                        primaryKeyList.Add(primaryKeyColumn);
                }

                return primaryKeyList;
            }

            return null;
        }

        public static string ReturnString(string DatabaseConnectionString, string DatabaseQueryString)
        {
            SqlConnection DatabaseConnection = new SqlConnection(DatabaseConnectionString);

            StringBuilder stringBuilder = new StringBuilder();

            try
            {
                DatabaseConnection.Open();

                SqlCommand ThisCommand = new SqlCommand(DatabaseQueryString, DatabaseConnection);

                SqlDataReader ThisReader = null;

                ThisReader = ThisCommand.ExecuteReader();

                int row = 0;

                while (ThisReader.Read())
                {
                    object[] ColumnArray = new object[ThisReader.FieldCount];

                    if (row > 0)
                        stringBuilder.Append(Environment.NewLine);

                    ThisReader.GetValues(ColumnArray);

                    for (int i = 0; i < ColumnArray.Length; i++)
                    {
                        if (i > 0)
                            stringBuilder.Append(", ");

                        stringBuilder.Append(ColumnArray[i].ToString());
                    }

                    row++;
                }
            }
            catch (Exception ex)
            {
                stringBuilder = new StringBuilder(ex.Message);
            }
            finally
            {
                DatabaseConnection.Close();
            }

            return stringBuilder.ToString();
        }

        //public static int RunStoredProcedure(string DatabaseConnectionString, string DatabaseQueryString)
        //{
        //    int returnValue = -1;

        //    SqlConnection DatabaseConnection = new SqlConnection(DatabaseConnectionString);

        //    try
        //    {
        //        DatabaseConnection.Open();

        //        SqlCommand ThisCommand = new SqlCommand(DatabaseQueryString, DatabaseConnection);

        //        SqlParameter retValue = ThisCommand.Parameters.Add("return", System.Data.SqlDbType.Int);
        //        retValue.Direction = System.Data.ParameterDirection.ReturnValue;

        //        ThisCommand.ExecuteNonQuery();

        //        returnValue = (int)retValue.Value;
        //    }
        //    catch (Exception ex)
        //    {
        //        returnValue = -2;
        //    }
        //    finally
        //    {
        //        DatabaseConnection.Close();
        //    }

        //    return returnValue;
        //}

        public static int ReturnRowsAffected(string DatabaseConnectionString, string DatabaseQueryString)
        {
            int returnValue = 0;

            SqlConnection DatabaseConnection = new SqlConnection(DatabaseConnectionString);

            try
            {
                DatabaseConnection.Open();

                SqlCommand ThisCommand = new SqlCommand(DatabaseQueryString, DatabaseConnection);

                returnValue = ThisCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("SqlCommand Error: " + ex.Message);
                returnValue = -2;
            }
            finally
            {
                DatabaseConnection.Close();
            }

            return returnValue;
        }

        public static int ReturnFirstKey(string DatabaseConnectionString, string DatabaseQueryString)
        {
            SqlConnection DatabaseConnection = new SqlConnection(DatabaseConnectionString);
            int ReturnValue = -1;

            try
            {
                DatabaseConnection.Open();

                SqlCommand ThisCommand = new SqlCommand(DatabaseQueryString, DatabaseConnection);

                SqlDataReader ThisReader = null;

                ThisReader = ThisCommand.ExecuteReader();

                ThisReader.Read();

                ReturnValue = (int)ThisReader.GetValue(0);

            }
            catch (Exception ex)
            {
                ReturnValue = -1;
            }
            finally
            {
                DatabaseConnection.Close();
            }

            return ReturnValue;
        }


        public static object ReturnFirstValue(string DatabaseConnectionString, string DatabaseQueryString)
        {
            SqlConnection DatabaseConnection = new SqlConnection(DatabaseConnectionString);
            object ReturnValue = null;

            try
            {
                DatabaseConnection.Open();

                SqlCommand ThisCommand = new SqlCommand(DatabaseQueryString, DatabaseConnection);

                SqlDataReader ThisReader = null;

                ThisReader = ThisCommand.ExecuteReader();

                ThisReader.Read();

                ReturnValue = ThisReader.GetValue(0);

            }
            catch (Exception ex)
            {
                ReturnValue = -1;
            }
            finally
            {
                DatabaseConnection.Close();
            }

            return ReturnValue;
        }

        //public static object[] ReturnFirstRow(string DatabaseConnectionString, string DatabaseQueryString)
        //{
        //    SqlConnection DatabaseConnection = new SqlConnection(DatabaseConnectionString);

        //    int FieldCount = -1;
        //    object[] ReturnValue = new object[0];

        //    try
        //    {
        //        DatabaseConnection.Open();

        //        SqlCommand ThisCommand = new SqlCommand(DatabaseQueryString, DatabaseConnection);

        //        SqlDataReader ThisReader = null;

        //        ThisReader = ThisCommand.ExecuteReader();

        //        ThisReader.Read();

        //        ReturnValue = new object[ThisReader.FieldCount];

        //        FieldCount = ThisReader.GetValues(ReturnValue);

        //    }
        //    catch (Exception ex)
        //    {
        //        FieldCount = -1;
        //        ReturnValue = new object[0];
        //    }
        //    finally
        //    {
        //        DatabaseConnection.Close();
        //    }

        //    return ReturnValue;
        //}

        public static object[] ReturnFirstRow(string DatabaseConnectionString, string DatabaseQueryString)
        {
            object[] ReturnValue = new object[0];

            using (SqlConnection DatabaseConnection = new SqlConnection(DatabaseConnectionString))
            {
                int FieldCount = -1;

                try
                {
                    DatabaseConnection.Open();

                    using (SqlCommand ThisCommand = new SqlCommand(DatabaseQueryString, DatabaseConnection))
                    {
                        using (SqlDataReader reader = ThisCommand.ExecuteReader())
                        {
                            reader.Read();

                            ReturnValue = new object[reader.FieldCount];

                            FieldCount = reader.GetValues(ReturnValue);
                        }
                    }

                }
                catch (Exception ex)
                {
                    FieldCount = -1;
                    ReturnValue = new object[0];
                }
                finally
                {
                    DatabaseConnection.Close();
                }
            }

            return ReturnValue;
        }

        //public static List<object[]> ReturnResults(string DatabaseConnectionString, string DatabaseQueryString)
        //{
        //    SqlConnection DatabaseConnection = new SqlConnection(DatabaseConnectionString);

        //    int FieldCount = -1;

        //    List<object[]> ReturnValue = new List<object[]>();

        //    try
        //    {
        //        DatabaseConnection.Open();

        //        SqlCommand ThisCommand = new SqlCommand(DatabaseQueryString, DatabaseConnection);

        //        SqlDataReader ThisReader = null;

        //        ThisReader = ThisCommand.ExecuteReader();

        //        while (ThisReader.Read())
        //        {
        //            object[] ColumnArray = new object[ThisReader.FieldCount];

        //            ThisReader.GetValues(ColumnArray);

        //            ReturnValue.Add(ColumnArray);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FieldCount = -1;
        //        ReturnValue = new List<object[]>();
        //    }
        //    finally
        //    {
        //        DatabaseConnection.Close();
        //    }

        //    return ReturnValue;
        //}

        public static List<object[]> ReturnResults(string DatabaseConnectionString, string DatabaseQueryString)
        {
            List<object[]> ReturnValue = new List<object[]>();

            using (SqlConnection DatabaseConnection = new SqlConnection(DatabaseConnectionString))
            {
                try
                {
                    DatabaseConnection.Open();

                    using (SqlCommand ThisCommand = new SqlCommand(DatabaseQueryString, DatabaseConnection))
                    {
                        ThisCommand.CommandTimeout = 120;

                        using (SqlDataReader reader = ThisCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                object[] ColumnArray = new object[reader.FieldCount];

                                reader.GetValues(ColumnArray);

                                ReturnValue.Add(ColumnArray);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    ReturnValue = new List<object[]>();
                }
                finally
                {
                    DatabaseConnection.Close();
                }

            }

            return ReturnValue;
        }

        public static QueryResults GetQueryResults(string DatabaseConnectionString, string DatabaseQueryString)
        {
            QueryResults queryResults = new QueryResults();

            SqlConnection sqlConnection = new SqlConnection(DatabaseConnectionString);

            SqlDataReader sqlDataReader = null;

            //List<object[]> resultList = new List<object[]>();

            //int rowCount = 0;

            int recordsAffected = 0;

            bool noResults = false;

            try
            {
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand(DatabaseQueryString, sqlConnection);

                sqlDataReader = sqlCommand.ExecuteReader();

                if (sqlDataReader.RecordsAffected == -1)
                {
                    System.Data.DataTable dataTable = sqlDataReader.GetSchemaTable();

                    int columnNameIndex = dataTable.Columns.IndexOf("ColumnName");

                    for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
                    {
                        System.Data.DataRow dataRow = dataTable.Rows[rowIndex];

                        string columnName = dataRow[columnNameIndex].ToString();

                        queryResults.ColumnNames.Add(columnName);
                    }


                    while (sqlDataReader.Read())
                    {
                        object[] row = new object[sqlDataReader.FieldCount];

                        sqlDataReader.GetValues(row);

                        queryResults.Rows.Add(row);

                        //rowCount++;
                    }

                }
                else
                {
                    noResults = true;

                    recordsAffected = sqlDataReader.RecordsAffected;
                }
            }
            catch (Exception ex)
            {
                //rowCount = 0;
                MessageBox.Show("Execution error:" + Environment.NewLine + Environment.NewLine + ex.Message);
            }
            finally
            {
                if (sqlDataReader != null)
                    sqlDataReader.Close();
            }


            return queryResults;
        }

        public class QueryResults
        {
            public List<string> ColumnNames;
            public List<object[]> Rows;

            public QueryResults()
            {
                ColumnNames = new List<string>();
                Rows = new List<object[]>();
            }

            public object GetField(int rowIndex, string columnName)
            {
                if (rowIndex >= 0 && rowIndex < Rows.Count)
                {
                    for (int i = 0; i < ColumnNames.Count; i++)
                    {
                        if (ColumnNames[i] == columnName)
                        {
                            return Rows[rowIndex][i];
                        }
                    }
                }

                return null;
            }

            public object GetField(int rowIndex, int columnIndex)
            {
                if (rowIndex >= 0 && rowIndex < Rows.Count)
                {
                    if (columnIndex >= 0 && columnIndex < Rows[rowIndex].Length)
                    {
                        return Rows[rowIndex][columnIndex];
                    }
                }

                return null;
            }
        }


        public static int Insert(string DatabaseConnectionString, string DatabaseQueryString)
        {
            SqlConnection DatabaseConnection = new SqlConnection(DatabaseConnectionString);

            int returnInt;

            try
            {
                DatabaseConnection.Open();

                SqlCommand ThisCommand = new SqlCommand(DatabaseQueryString, DatabaseConnection);

                returnInt = (int)ThisCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                returnInt = -1;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                DatabaseConnection.Close();
            }

            return returnInt;
        }

        public static int Delete(string DatabaseConnectionString, string DeleteFromTableName, string WhereColumnName, string WhereValue)
        {
            if (DeleteFromTableName.Length == 0 || WhereColumnName.Length == 0 || WhereValue.Length == 0)
                return 0;

            string completeWhereClause = "WHERE [" + WhereColumnName + "]='" + WhereValue + "';";

            return Delete(DatabaseConnectionString, DeleteFromTableName, completeWhereClause);
        }

        public static int Delete(string DatabaseConnectionString, string DeleteFromTableName, string CompleteWhereClause)
        {
            if (DeleteFromTableName.Length == 0 || !CompleteWhereClause.Substring(0, 5).ToUpper().Equals("WHERE"))
                return 0;

            string databaseQueryString = "DELETE FROM [" + DeleteFromTableName + "] " + CompleteWhereClause + ";";

            SqlConnection DatabaseConnection = new SqlConnection(DatabaseConnectionString);

            int returnInt = 0;

            try
            {
                DatabaseConnection.Open();

                SqlCommand ThisCommand = new SqlCommand(databaseQueryString, DatabaseConnection);

                returnInt = ThisCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                returnInt = -1;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                DatabaseConnection.Close();
            }

            return returnInt;
        }

        public static string ReplaceValueCharacters(string ValueString)
        {
            if (ValueString != null)
                return ValueString.Replace("'", "''").Replace(';', '-');

            return ValueString;
        }

        public static Type GetCSharpType(string sqlDataType)
        {
            if (sqlTypeDictionary == null)
            {
                sqlTypeDictionary = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
                sqlTypeDictionary.Add("varbinary", typeof(byte[]));
                sqlTypeDictionary.Add("binary", typeof(byte[]));
                sqlTypeDictionary.Add("image", typeof(byte[]));

                sqlTypeDictionary.Add("varchar", typeof(string));
                sqlTypeDictionary.Add("char", typeof(string));
                sqlTypeDictionary.Add("nvarchar", typeof(string));
                sqlTypeDictionary.Add("nchar", typeof(string));
                sqlTypeDictionary.Add("text", typeof(string));
                sqlTypeDictionary.Add("ntext", typeof(string));

                sqlTypeDictionary.Add("bit", typeof(bool));
                sqlTypeDictionary.Add("tinyint", typeof(Byte));
                sqlTypeDictionary.Add("smallint", typeof(Int16));
                sqlTypeDictionary.Add("int", typeof(int));
                sqlTypeDictionary.Add("bigint", typeof(Int64));

                sqlTypeDictionary.Add("numeric", typeof(decimal));
                sqlTypeDictionary.Add("decimal", typeof(decimal));
                sqlTypeDictionary.Add("float", typeof(double));

                sqlTypeDictionary.Add("datetime", typeof(DateTime));

                sqlTypeDictionary.Add("timestamp", typeof(TimeStamp));







            }

            if (sqlTypeDictionary.ContainsKey(sqlDataType))
                return sqlTypeDictionary[sqlDataType];

            //Debug.WriteLine("GetCSharpType(" + sqlDataType + ")" + " - Matching type not found!");

            return null;
        }

        public static Dictionary<string, Type> sqlTypeDictionary;
    }

    public class InformationSchemaColumnsRow
    {
        public string TableCatalog { get; set; }
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public Int32 OrdinalPosition { get; set; }
        public string ColumnDefault { get; set; }
        public string IsNullable { get; set; }
        public string DataType { get; set; }
        public Int32 CharacterMaximumLength { get; set; }
        public Int32 CharacterOctetLength { get; set; }
        public byte NumericPrecision { get; set; }
        public Int16 NumericPrecisionRadix { get; set; }
        public Int32 NumericScale { get; set; }
        public Int16 DateTimePrecision { get; set; }

        public bool IsPrimaryKey { get; set; }
        public bool IsTimestamp { get; set; }
        public Type CSharpType { get; set; }
        public string CSharpTypeString { get; set; }
        public string DefaultValueString { get; set; }

        public InformationSchemaColumnsRow()
        {

        }
    }

    public class SQLCommandBuilder
    {
        public List<string[]> stringArrayList;

        public SQLCommandBuilder()
        {
            stringArrayList = new List<string[]>();
        }

        public void AddNullable(string ColumnName, int? Value)
        {
            if (Value == null)
                AddNull(ColumnName);
            else
                Add(ColumnName, Value.ToString());
        }

        public void AddNullable(string ColumnName, decimal? Value)
        {
            if (Value == null)
                AddNull(ColumnName);
            else
                Add(ColumnName, Value.ToString());
        }

        public void AddNullable(string ColumnName, bool? Value)
        {
            if (Value == null)
                AddNull(ColumnName);
            else
                Add(ColumnName, (Value == true) ? "1" : "0");
        }

        public void AddNullable(string ColumnName, DateTime? Value)
        {
            if (Value == null)
                AddNull(ColumnName);
            else
                AddDateTime(ColumnName, (DateTime)Value);
        }

        public void AddNullable(string ColumnName, string Value)
        {
            if (Value == null)
                AddNull(ColumnName);
            else
                Add(ColumnName, Value);
        }

        public void Add(string ColumnName, string Value)
        {
            string[] stringArray = { ColumnName, Value };

            stringArrayList.Add(stringArray);
        }

        public void AddNull(string ColumnName)
        {
            AddWithoutQuotes(ColumnName, "NULL");
        }

        public void AddObject(string ColumnName, object obj)
        {
            if (obj == null || obj == DBNull.Value)
                AddNull(ColumnName);
            else
                Add(ColumnName, obj.ToString());
        }

        public void AddQueryResultsField(SqlTools.QueryResults queryResults, int rowIndex, string columnName)
        {
            if (rowIndex >= 0 && rowIndex < queryResults.Rows.Count)
            {
                Add(columnName, queryResults.GetField(rowIndex, columnName).ToString());
            }
        }

        public void AddQueryResultsField(SqlTools.QueryResults queryResults, int rowIndex, int columnIndex)
        {
            if (rowIndex >= 0 && rowIndex < queryResults.Rows.Count)
            {
                if (columnIndex >= 0 && columnIndex < queryResults.Rows[rowIndex].Length)
                {
                    Add(queryResults.ColumnNames[columnIndex], queryResults.GetField(rowIndex, columnIndex).ToString());
                }
            }
        }

        public void AddGetDate(string ColumnName)
        {
            AddWithoutQuotes(ColumnName, "GETDATE()");
        }

        public void AddDateTime(string ColumnName, DateTime dateTime)
        {
            Add(ColumnName, dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public void AddWithNewLines(string ColumnName, string Value)
        {
            string[] stringArray = { ColumnName, Value, "NewLines" };

            stringArrayList.Add(stringArray);
        }

        public void AddWithoutQuotes(string ColumnName, string Value)
        {
            string[] stringArray = { ColumnName, Value, "NoQuotes" };

            stringArrayList.Add(stringArray);
        }

        public string GetInsertCommand(string InsertIntoTable, string OutputColumnName)
        {
            StringBuilder sb = new StringBuilder();

            if (OutputColumnName != null)
            {
                sb.Append("DECLARE @IdentityOutput TABLE (ID INT) ");
                //sb.Append(" DECLARE @IdentityValue INT ");
            }

            sb.Append("INSERT INTO [");
            sb.Append(InsertIntoTable);
            sb.Append("] ");

            if (stringArrayList.Count > 0)
            {

                for (int i = 0; i < stringArrayList.Count; i++)
                {
                    string[] stringArray = stringArrayList[i];

                    if (i == 0)
                        sb.Append("(");
                    else
                        sb.Append(",");

                    sb.Append("[");
                    sb.Append(stringArray[0]);
                    sb.Append("]");

                    if (i == stringArrayList.Count - 1)
                        sb.Append(")");
                }

                if (OutputColumnName != null)
                {
                    sb.Append(" OUTPUT INSERTED.");
                    sb.Append(OutputColumnName);
                    sb.Append(" INTO @IdentityOutput");
                }

                sb.Append(" VALUES ");

                for (int i = 0; i < stringArrayList.Count; i++)
                {
                    string[] stringArray = stringArrayList[i];

                    if (i == 0)
                        sb.Append("(");
                    else
                        sb.Append(",");

                    bool useQuotes = true;
                    bool replaceNewLines = false;

                    if (stringArray.Length > 2)
                    {
                        if (stringArray[2] == "NoQuotes")
                            useQuotes = false;

                        if (stringArray[2] == "NewLines")
                            replaceNewLines = true;
                    }

                    if (useQuotes)
                        sb.Append("'");

                    string replacedString = ReplaceValueCharacters(stringArray[1]);

                    if (replaceNewLines)
                        replacedString = ReplaceNewLineCharacters(replacedString);

                    sb.Append(replacedString);

                    if (useQuotes)
                        sb.Append("'");

                    if (i == stringArrayList.Count - 1)
                        sb.Append(")");
                }
            }
            else
            {
                if (OutputColumnName != null)
                {
                    sb.Append("OUTPUT INSERTED.");
                    sb.Append(OutputColumnName);
                    sb.Append(" INTO @IdentityOutput ");
                }

                sb.Append("DEFAULT VALUES");
            }

            if (OutputColumnName != null)
            {
                //sb.Append(" SELECT @IdentityValue = (SELECT ID FROM @IdentityOutput)");
                sb.Append(" SELECT ID FROM @IdentityOutput");
            }

            sb.Append(";");

            return sb.ToString();
        }

        public string GetUpdateCommand(string TableToUpdate, string WhereColumnName, string WhereValue)
        {
            string completeWhereClause = "WHERE [" + WhereColumnName + "]='" + WhereValue + "'";

            return GetUpdateCommand(TableToUpdate, completeWhereClause);
        }

        public string GetUpdateCommand(string TableToUpdate, string CompleteWhereClause)
        {
            StringBuilder sb = new StringBuilder();

            if (stringArrayList.Count > 0 && CompleteWhereClause.Substring(0, 5).ToUpper().Equals("WHERE"))
            {
                sb.Append("UPDATE ");
                //sb.Append(TableToUpdate);
                sb.Append("[" + TableToUpdate + "]");
                sb.Append(" SET ");

                for (int i = 0; i < stringArrayList.Count; i++)
                {
                    string[] stringArray = stringArrayList[i];

                    sb.Append("[");
                    sb.Append(stringArray[0]);
                    sb.Append("]=");

                    bool useQuotes = true;
                    bool replaceNewLines = false;

                    if (stringArray.Length > 2)
                    {
                        if (stringArray[2] == "NoQuotes")
                            useQuotes = false;

                        if (stringArray[2] == "NewLines")
                            replaceNewLines = true;
                    }

                    if (useQuotes)
                        sb.Append("'");

                    string replacedString = ReplaceValueCharacters(stringArray[1]);

                    if (replaceNewLines)
                        replacedString = ReplaceNewLineCharacters(replacedString);

                    sb.Append(replacedString);

                    if (useQuotes)
                        sb.Append("'");


                    if (i != stringArrayList.Count - 1)
                        sb.Append(",");
                }

                sb.Append(" ");
                sb.Append(CompleteWhereClause);
                sb.Append(";");
            }

            return sb.ToString();
        }

        private string ReplaceValueCharacters(string ValueString)
        {
            if (ValueString != null)
                return ValueString.Replace("'", "''");
            //return ValueString.Replace("'", "''").Replace(';', '-');

            return ValueString;
        }

        private string ReplaceNewLineCharacters(string ValueString)
        {
            if (ValueString != null)
                return ValueString.Replace(Environment.NewLine, "' + CHAR(13) + CHAR(10) + '");

            return ValueString;
        }
    }

    public class QueryResults
    {
        public bool Error { get; set; }
        public Exception Exception { get; set; }

        public List<string> Headers { get; set; }

        public List<object[]> Rows { get; set; }

        public int RowsAffected { get; set; }
    }

    public class SqlDatabaseHelper
    {
        public string ConnectionString;
        public int CommandTimeout;
        public int? UserID;

        public SqlDatabaseHelper()
        {
            CommandTimeout = 120;

            UserID = null;
        }

        public SqlDatabaseHelper(string connectionString) : this()
        {
            this.ConnectionString = connectionString;
        }

        public ObjectArrayList GetResults(string queryString)
        {
            ObjectArrayList objectArrayList = null;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeout;

                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            if (sqlDataReader != null)
                            {
                                objectArrayList = new ObjectArrayList();

                                while (sqlDataReader.Read())
                                {
                                    object[] row = new object[sqlDataReader.FieldCount];

                                    sqlDataReader.GetValues(row);

                                    objectArrayList.Add(row);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return objectArrayList;
        }

        public QueryResults GetQueryResults(string queryString, bool includeHeaders = false)
        {
            QueryResults queryResults = new QueryResults();

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeout;

                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            if (sqlDataReader != null)
                            {
                                if (includeHeaders)
                                {
                                    queryResults.Headers = new List<string>();

                                    for (int i = 0; i < sqlDataReader.FieldCount; i++)
                                    {
                                        queryResults.Headers.Add(sqlDataReader.GetName(i));
                                    }
                                }

                                List<object[]> rows = new List<object[]>();

                                while (sqlDataReader.Read())
                                {
                                    object[] row = new object[sqlDataReader.FieldCount];

                                    sqlDataReader.GetValues(row);

                                    rows.Add(row);
                                }

                                queryResults.Rows = rows;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                queryResults.Error = true;
                queryResults.Exception = ex;
            }

            return queryResults;
        }

        public object[] GetFirstRow(string queryString)
        {
            object[] row = null;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeout;

                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            if (sqlDataReader != null)
                            {
                                if (sqlDataReader.Read())
                                {
                                    row = new object[sqlDataReader.FieldCount];

                                    sqlDataReader.GetValues(row);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return row;
        }

        public object GetFirstValue(string queryString)
        {
            object value = null;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeout;

                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            if (sqlDataReader != null)
                            {
                                if (sqlDataReader.Read())
                                    value = sqlDataReader.GetValue(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return value;
        }


        //public int GetRowsAffected(string queryString)
        //{
        //    int rowsAffected = 0;

        //    try
        //    {
        //        using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
        //        {
        //            sqlConnection.Open();

        //            using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
        //            {
        //                sqlCommand.CommandTimeout = CommandTimeout;

        //                rowsAffected = sqlCommand.ExecuteNonQuery();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //        rowsAffected = -2;
        //    }

        //    return rowsAffected;
        //}

        public int GetRowsAffected(string queryString)
        {
            return GetRowsAffected(queryString, CommandTimeout);
        }

        public int GetRowsAffected(string queryString, int commandTimeout)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = commandTimeout;

                        rowsAffected = sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                rowsAffected = -2;
            }

            return rowsAffected;
        }

        public string ExecuteWithErrorMessage(string queryString)
        {
            int rowsAffected = 0;
            string errorMessage = null;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeout;

                        rowsAffected = sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                errorMessage = ex.Message;
            }

            return errorMessage;
        }
        
        public int Insert(string queryString)
        {
            int returnInt;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeout;

                        //returnInt = (int)sqlCommand.ExecuteScalar();

                        object obj = sqlCommand.ExecuteScalar();

                        if (obj == null)
                        {
                            returnInt = -1;
                        }
                        else
                        {
                            //if (obj is int)
                            //{
                            //    //Debug.WriteLine(obj.ToString());

                            //    returnInt = (int)obj;
                            //}
                            //else
                            //{
                            //    returnInt = -1;
                            //}
                            returnInt = (int)obj;
                        }

                        
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                returnInt = -1;
            }

            return returnInt;
        }

        public SharpTech.Core.ResultWrapper<int> InsertWithErrors(string queryString)
        {
            int returnInt = -1;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeout;

                        object obj = sqlCommand.ExecuteScalar();

                        if (obj != null)
                            returnInt = (int)obj;
                    }
                }
            }
            catch (Exception ex)
            {
                returnInt = -1;
                return new SharpTech.Core.ResultWrapper<int>(exception: ex);
            }

            return new SharpTech.Core.ResultWrapper<int>(value: returnInt);
        }

        public int Delete(string tableName, string whereColumnName, string whereValue)
        {
            if (tableName.IsNullOrWhiteSpace() || whereColumnName.IsNullOrWhiteSpace() || whereValue.IsNullOrWhiteSpace())
                return 0;

            string completeWhereClause = "WHERE [" + whereColumnName + "]='" + whereValue + "';";

            return Delete(tableName, completeWhereClause);
        }

        public int Delete(string deleteFromTableName, string completeWhereClause)
        {
            if (deleteFromTableName.IsNullOrWhiteSpace() || completeWhereClause.IsNullOrWhiteSpace())
                return 0;

            if (!completeWhereClause.Substring(0, 5).EqualsIgnoreCaseOrdinal("WHERE"))
                return 0;

            string databaseQueryString = "DELETE FROM [" + deleteFromTableName + "] " + completeWhereClause + ";";

            int returnInt;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(databaseQueryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeout;

                        returnInt = (int)sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                returnInt = -1;
            }

            return returnInt;
        }

    }


    public class SqlCommandHelperColumn
    {
        public string ColumnName;
        public string Value;
        public bool AddQuotes;
        public bool ReplaceNewLineCharacters;

        public SqlCommandHelperColumn()
        {
            AddQuotes = true;
            ReplaceNewLineCharacters = false;
        }

        public SqlCommandHelperColumn(string columnName, string value) : this()
        {
            this.ColumnName = columnName;
            this.Value = value;
        }
    }

    public class SqlCommandHelper
    {
        public List<SqlCommandHelperColumn> SqlCommandHelperColumns;

        public SqlCommandHelper()
        {
            SqlCommandHelperColumns = new List<SqlCommandHelperColumn>();
        }

        public void Add(DatabaseField databaseField)
        {
            if (databaseField.RawValue == null)
                AddNull(databaseField.columnInfo.ColumnName);
            else
                Add(databaseField.columnInfo.ColumnName, databaseField.RawValue.ToSqlString());
        }

        public void AddNull(string columnName)
        {
            SqlCommandHelperColumns.Add(new SqlCommandHelperColumn(columnName, "NULL") { AddQuotes = false });
        }

        public void Add(string columnName, string value)
        {
            SqlCommandHelperColumns.Add(new SqlCommandHelperColumn(columnName, value));
        }

        public string GetInsertCommand(string tableToUpdate, string outputColumnName)
        {
            StringBuilder sb = new StringBuilder();

            if (outputColumnName != null)
            {
                sb.Append("DECLARE @IdentityOutput TABLE (ID INT)");
                sb.Append(Environment.NewLine);
            }

            sb.Append("INSERT INTO [");
            sb.Append(tableToUpdate);
            sb.Append("]");

            if (SqlCommandHelperColumns.Count > 0)
            {

                sb.Append(Environment.NewLine);
                sb.Append("(");

                for (int i = 0; i < SqlCommandHelperColumns.Count; i++)
                {
                    SqlCommandHelperColumn sqlCommandHelperColumn = SqlCommandHelperColumns[i];

                    sb.Append(Environment.NewLine);

                    if (i > 0)
                        sb.Append(",");

                    sb.Append("[");
                    sb.Append(sqlCommandHelperColumn.ColumnName);
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

                for (int i = 0; i < SqlCommandHelperColumns.Count; i++)
                {
                    SqlCommandHelperColumn sqlCommandHelperColumn = SqlCommandHelperColumns[i];

                    sb.Append(Environment.NewLine);

                    if (i > 0)
                        sb.Append(",");

                    if (sqlCommandHelperColumn.AddQuotes)
                        sb.Append("'");

                    string value = ReplaceValueCharacters(sqlCommandHelperColumn.Value);

                    if (sqlCommandHelperColumn.ReplaceNewLineCharacters)
                        value = ReplaceNewLineCharacters(value);

                    sb.Append(value);

                    if (sqlCommandHelperColumn.AddQuotes)
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

        public string GetUpdateCommand(string tableToUpdate, string whereColumnName, string whereValue)
        {
            string completeWhereClause = "WHERE [" + whereColumnName + "]='" + whereValue + "'";

            return GetUpdateCommand(tableToUpdate, completeWhereClause);
        }

        public string GetUpdateCommand(string tableToUpdate, string completeWhereClause)
        {
            StringBuilder sb = new StringBuilder();

            if (SqlCommandHelperColumns.Count > 0 && completeWhereClause.Substring(0, 5).ToUpper().Equals("WHERE"))
            {
                sb.Append("UPDATE ");
                sb.Append("[");
                sb.Append(tableToUpdate);
                sb.Append("]");
                sb.Append(" SET ");

                for (int i = 0; i < SqlCommandHelperColumns.Count; i++)
                {
                    SqlCommandHelperColumn sqlCommandHelperColumn = SqlCommandHelperColumns[i];

                    sb.Append(Environment.NewLine);

                    if (i > 0)
                        sb.Append(",");

                    sb.Append("[");
                    sb.Append(sqlCommandHelperColumn.ColumnName);
                    sb.Append("] = ");

                    if (sqlCommandHelperColumn.AddQuotes)
                        sb.Append("'");

                    string value = ReplaceValueCharacters(sqlCommandHelperColumn.Value);

                    if (sqlCommandHelperColumn.ReplaceNewLineCharacters)
                        value = ReplaceNewLineCharacters(value);

                    sb.Append(value);

                    if (sqlCommandHelperColumn.AddQuotes)
                        sb.Append("'");
                }

                sb.Append(Environment.NewLine);
                sb.Append(completeWhereClause);
                sb.Append(";");
            }

            return sb.ToString();
        }

        private string ReplaceValueCharacters(string ValueString)
        {
            if (ValueString != null)
                return ValueString.Replace("'", "''");
            //return ValueString.Replace("'", "''").Replace(';', '-');

            return ValueString;
        }

        private string ReplaceNewLineCharacters(string ValueString)
        {
            if (ValueString != null)
                return ValueString.Replace(Environment.NewLine, "' + CHAR(13) + CHAR(10) + '");

            return ValueString;
        }
    }


    public abstract class DatabaseRow : ObservableObject
    {
        public string TableName;
        public List<DatabaseField> DatabaseFieldList;
        public DatabaseField<int?> PrimaryKeyField { get; set; }
        public DatabaseField<TimeStamp?> TimestampField;
        public DatabaseField<int?> LastUserIDField;

        public SqlDatabaseHelper sqlDatabaseHelper;

        public bool HasPrimaryKey
        {
            get
            {
                if (PrimaryKeyField == null)
                    return false;

                if (PrimaryKeyField.Value == null)
                    return false;

                return true;
            }
        }

        public bool HasValueChanged
        {
            get
            {
                foreach (DatabaseField databaseField in DatabaseFieldList)
                {
                    if (databaseField.HasValueChanged)
                        return true;
                }

                return false;
            }
        }

        public DatabaseRow()
        {
        }

        public bool Load(int? primaryKeyValue)
        {
            if (primaryKeyValue == null)
                return false;

            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT TOP 1 ");

            for (int i = 0; i < DatabaseFieldList.Count; i++)
            {
                DatabaseField databaseField = DatabaseFieldList[i];

                if (i > 0)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(",");
                }

                sb.Append("[");
                sb.Append(databaseField.columnInfo.ColumnName);
                sb.Append("]");
            }

            sb.Append(Environment.NewLine);
            sb.Append("FROM [");
            sb.Append(TableName);
            sb.Append("]");
            sb.Append(Environment.NewLine);
            sb.Append("WHERE [");
            sb.Append(PrimaryKeyField.columnInfo.ColumnName);
            sb.Append("] = '");
            sb.Append(primaryKeyValue);
            sb.Append("'");

            //Debug.WriteLine(sb.ToString());

            object[] row = sqlDatabaseHelper.GetFirstRow(sb.ToString());

            if (row.IsNullOrEmpty())
                return false;

            if (row.Length != DatabaseFieldList.Count)
                return false;

            for (int i = 0; i < DatabaseFieldList.Count && i < row.Length; i++)
            {
                DatabaseField databaseField = DatabaseFieldList[i];

                databaseField.SetValue(row[i], true);
            }

            return true;
        }

        public void Refresh()
        {
            Load(PrimaryKeyField.Value);
        }

        public bool Save()
        {
            return Save(false);
        }

        public bool Save(bool forceOverwrite)
        {
            int? primaryKeyValue = PrimaryKeyField.RawValue.TryCastNullable<int>();

            bool insert = primaryKeyValue == null ? true : false;

            if (!forceOverwrite && !insert && TimestampField != null)
            {
                // check for timestamp change

                bool updated = HasTimestampChanged(sqlDatabaseHelper);

                if (updated)
                    return false;
            }

            if (LastUserIDField != null && sqlDatabaseHelper.UserID != null)
                LastUserIDField.Value = sqlDatabaseHelper.UserID;

            SqlCommandHelper sqlCommandHelper = new SqlCommandHelper();

            foreach (DatabaseField databaseField in DatabaseFieldList)
            {
                if (databaseField.columnInfo.IsPrimaryKey || databaseField.columnInfo.IsTimestamp)
                    continue;

                if (forceOverwrite || databaseField.HasValueChanged)
                {
                    sqlCommandHelper.Add(databaseField);
                }
            }

            //Debug.WriteLine(sqlCommandHelper.GetInsertCommand(TableName, PrimaryKeyField.columnInfo.ColumnName));
            //return true;

            if (insert)
            {
                PrimaryKeyField.Value = sqlDatabaseHelper.Insert(sqlCommandHelper.GetInsertCommand(TableName, PrimaryKeyField.columnInfo.ColumnName));
            }
            else
            {
                int results = sqlDatabaseHelper.GetRowsAffected(sqlCommandHelper.GetUpdateCommand(TableName, PrimaryKeyField.columnInfo.ColumnName, primaryKeyValue.ToSqlString()));
            }

            return true;
        }

        public string GetTestSqlString()
        {
            int? primaryKeyValue = PrimaryKeyField.RawValue.TryCastNullable<int>();

            bool insert = primaryKeyValue == null ? true : false;

            SqlCommandHelper sqlCommandHelper = new SqlCommandHelper();

            foreach (DatabaseField databaseField in DatabaseFieldList)
            {
                if (databaseField.columnInfo.IsPrimaryKey || databaseField.columnInfo.IsTimestamp)
                    continue;

                if (databaseField.HasValueChanged)
                {
                    sqlCommandHelper.Add(databaseField);
                }
            }

            if (insert)
            {
                return sqlCommandHelper.GetInsertCommand(TableName, PrimaryKeyField.columnInfo.ColumnName);
            }
            else
            {
                return sqlCommandHelper.GetUpdateCommand(TableName, PrimaryKeyField.columnInfo.ColumnName, primaryKeyValue.ToSqlString());
            }
        }

        public bool HasTimestampChanged(SqlDatabaseHelper databaseHelper)
        {
            if (PrimaryKeyField == null || TimestampField == null)
                return false;

            int? primaryKeyValue = PrimaryKeyField.RawValue.TryCastNullable<int>();

            if (primaryKeyValue == null)
                return false;

            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT TOP 1 [");
            sb.Append(TimestampField.columnInfo.ColumnName);
            sb.Append("]");
            sb.Append(Environment.NewLine);
            sb.Append("FROM [");
            sb.Append(TableName);
            sb.Append("]");
            sb.Append(Environment.NewLine);
            sb.Append("WHERE [");
            sb.Append(PrimaryKeyField.columnInfo.ColumnName);
            sb.Append("] = '");
            sb.Append(primaryKeyValue);
            sb.Append("'");

            TimeStamp timestamp = databaseHelper.GetFirstValue(sb.ToString()).TryCast<TimeStamp>(null);

            if (timestamp == null)
                return false;

            if (timestamp > TimestampField.Value)
                return true;

            return false;
        }
    }

    public class DatabaseColumnInformation
    {
        public string ColumnName;
        public int? MaximumCharacterLength;
        public int? NumericPrecision;
        public int? NumericScale;
        public bool IsNullable;
        public bool IsPrimaryKey;
        public bool IsTimestamp;
        public object DefaultValue;

        public DatabaseColumnInformation(string columnName, int? maximumCharacterLength, int? numericPrecision, int? numericScale, bool isNullable, bool isPrimaryKey, bool isTimestamp, object defaultValue)
        {
            this.ColumnName = columnName;
            this.MaximumCharacterLength = maximumCharacterLength;
            this.NumericPrecision = numericPrecision;
            this.NumericScale = numericScale;
            this.IsNullable = isNullable;
            this.IsPrimaryKey = isPrimaryKey;
            this.IsTimestamp = isTimestamp;
            this.DefaultValue = defaultValue;
        }
    }
    public abstract class DatabaseField : INotifyPropertyChanged
    {
        public DatabaseColumnInformation columnInfo;

        public abstract object RawValue { get; }

        public abstract void SetValue(object obj);

        public abstract void SetValue(object obj, bool resetChanges);

        public abstract void ResetChanges();

        public abstract bool HasValueChanged { get; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //public abstract string GetSqlString();
    }

    public class DatabaseField<T> : DatabaseField
    {
        private T _Value;
        //public T Value { get; set; }
        public T Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                OnPropertyChanged("Value");
            }
        }

        public T OriginalValue { get; set; }

        public override object RawValue { get { return Value; } }

        public override bool HasValueChanged
        {
            get
            {
                return !EqualityComparer<T>.Default.Equals(Value, OriginalValue);
            }
        }

        public DatabaseField(DatabaseColumnInformation databaseColumnInformation)
        {
            this.columnInfo = databaseColumnInformation;

            if (columnInfo.DefaultValue != null)
                SetValue(columnInfo.DefaultValue, true);
        }

        public static implicit operator T(DatabaseField<T> databaseColumn)
        {
            return databaseColumn.Value;
        }

        public override void SetValue(object obj)
        {
            SetValue(obj, false);
        }

        public override void SetValue(object obj, bool resetChanges)
        {
            T value;

            if (!obj.TryCastIfPossible<T>(out value))
                value = default(T);

            Value = value;

            if (resetChanges)
                ResetChanges();
        }

        public override void ResetChanges()
        {
            OriginalValue = Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        //public override string GetSqlString()
        //{
        //    if (Value == null)
        //        return null;

        //    if (typeof(T) == typeof(DateTime))
        //        return ((DateTime)Convert.ChangeType(Value, typeof(DateTime))).ToSqlString();

        //    return Value.ToString();
        //}
    }

    //public abstract class DatabaseColumn
    //{
    //    public string ColumnName;
    //    public int? MaximumCharacterLength;
    //    public int? NumericPrecision;
    //    public int? NumericScale;
    //    public bool IsNullable;
    //    public bool PrimaryKey;

    //    public abstract void SetValue(object obj);
    //}

    //public class DatabaseColumn<T> : DatabaseColumn
    //{
    //    //public string ColumnName;
    //    //public int? MaximumCharacterLength;
    //    //public int? NumericPrecision;
    //    //public int? NumericScale;
    //    //public bool IsNullable;
    //    //public bool PrimaryKey;

    //    //public bool ValueChanged;

    //    //private T value;
    //    //public T Value
    //    //{
    //    //    get { return value; }
    //    //    set
    //    //    {
    //    //        if (!EqualityComparer<T>.Default.Equals(value, this.value))
    //    //        {
    //    //            this.value = value;
    //    //            ValueChanged = true;
    //    //        }
    //    //    }
    //    //}

    //    public T Value;
    //    public T OriginalValue;

    //    public DatabaseColumn(string columnName, int? maximumCharacterLength, int? numericPrecision, int? numericScale, bool isNullable, bool primaryKey)
    //    {
    //        this.ColumnName = columnName;
    //        this.MaximumCharacterLength = maximumCharacterLength;
    //        this.NumericPrecision = numericPrecision;
    //        this.NumericScale = numericScale;
    //        this.IsNullable = isNullable;
    //        this.PrimaryKey = primaryKey;

    //    }

    //    public static implicit operator T(DatabaseColumn<T> databaseColumn)
    //    {
    //        return databaseColumn.Value;
    //    }

    //    public override void SetValue(object obj)
    //    {
    //        //Value = obj.TryCast<T>();
    //    }
    //}














    //public class DataTablePlus
    //{
    //    internal readonly string _TableName;
    //    public string TableName
    //    {
    //        get { return _TableName; }
    //    }
        
    //    internal readonly DataRowPlusCollection _Rows;
    //    public DataRowPlusCollection Rows
    //    {
    //        get { return _Rows; }
    //    }

    //    internal readonly DataColumnPlusCollection _Columns;
    //    public DataColumnPlusCollection Columns
    //    {
    //        get { return _Columns; }
    //    }

    //    internal readonly RecordManager recordManager;

    //    public DataTablePlus()
    //    {
    //        this._Columns = new Sql.DataColumnPlusCollection(this);
    //        this._Rows = new Sql.DataRowPlusCollection(this);
    //        recordManager = new RecordManager(this);
    //    }
    //}

    //public class RecordManager
    //{
    //    private readonly DataTablePlus dataTablePlus;

    //    public RecordManager(DataTablePlus dataTablePlus)
    //    {
    //        if (dataTablePlus == null)
    //            throw new Exception("Table is null");

    //        this.dataTablePlus = dataTablePlus;
    //    }
    //}

    //public class DataRowPlusCollection
    //{
    //    private readonly DataTablePlus dataTablePlus;

    //    public DataRowPlusCollection(DataTablePlus dataTablePlus)
    //    {
    //        this.dataTablePlus = dataTablePlus;
    //    }
    //}

    //public class DataRowPlus
    //{
    //    private readonly DataTablePlus _Table;
    //    public DataTablePlus Table
    //    {
    //        get { return _Table; }
    //    }
        
    //    private readonly DataColumnPlusCollection _Columns;
    //    public DataColumnPlusCollection Columns
    //    {
    //        get { return _Columns; }
    //    }

    //    internal int oldRecord = -1;
    //    internal int newRecord = -1;
    //    internal int tempRecord;
    //    internal long _rowID = -1;

    //    public DataRowPlus(DataTablePlus dataTablePlus)
    //    {
    //        this._Table = dataTablePlus;
    //    }

    //    public object this[int columnIndex]
    //    {
    //        get
    //        {
    //            DataColumnPlus column = _Columns[columnIndex];
    //            int record = GetDefaultRecord();
    //            _Table.recordManager.VerifyRecord(record, this);
    //            //VerifyValueFromStorage(column, DataRowVersion.Default, column[record]);
    //            return column[record];
    //        }
    //        set
    //        {
    //            DataColumnPlus column = _Columns[columnIndex];
    //            this[column] = value;
    //        }
    //    }

    //    public object this[string columnName]
    //    {
    //        get
    //        {
    //            DataColumnPlus column = GetDataColumn(columnName);
    //            int record = GetDefaultRecord();
    //            _Table.recordManager.VerifyRecord(record, this);
    //            //VerifyValueFromStorage(column, DataRowVersion.Default, column[record]);
    //            return column[record];
    //        }
    //        set
    //        {
    //            DataColumnPlus column = GetDataColumn(columnName);
    //            this[column] = value;
    //        }
    //    }
        
    //    internal int GetDefaultRecord()
    //    {
    //        if (tempRecord != -1)
    //            return tempRecord;

    //        if (newRecord != -1)
    //            return newRecord;

    //        // If row has oldRecord - this is deleted row.
    //        if (oldRecord == -1)
    //            throw new Exception("RowRemovedFromTheTable");
    //        else
    //            throw new Exception("DeletedRowInaccessible");
    //    }

    //    internal DataColumnPlus GetDataColumn(string columnName)
    //    {
    //        DataColumnPlus column = _Columns[columnName];

    //        if (column != null)
    //            return column;

    //        throw new Exception("Column " + columnName + " is not in table " + _Table.TableName);
    //    }

    //    //[Conditional("DEBUG")]
    //    //private void VerifyValueFromStorage(DataColumn column, DataRowVersion version, object valueFromStorage)
    //    //{
    //    //    // Dev11 900390: ignore deleted rows by adding "newRecord != -1" condition - we do not evaluate computed rows if they are deleted
    //    //    if (column.DataExpression != null && !inChangingEvent && tempRecord == -1 && newRecord != -1)
    //    //    {
    //    //        // for unchanged rows, check current if original is asked for.
    //    //        // this is because by design, there is only single storage for an unchanged row.
    //    //        if (version == DataRowVersion.Original && oldRecord == newRecord)
    //    //        {
    //    //            version = DataRowVersion.Current;
    //    //        }
    //    //        // There are various known issues detected by this assert for non-default versions, 
    //    //        // for example DevDiv2 bug 73753
    //    //        // Since changes consitutute breaking change (either way customer will get another result), 
    //    //        // we decided not to fix them in Dev 11
    //    //        Debug.Assert(valueFromStorage.Equals(column.DataExpression.Evaluate(this, version)),
    //    //            "Value from storage does lazily computed expression value");
    //    //    }
    //    //}
    //}

    //public class DataColumnPlusCollection
    //{
    //    private readonly DataTablePlus dataTablePlus;

    //    private readonly Dictionary<string, DataColumnPlus> columnFromName;

    //    public DataColumnPlusCollection(DataTablePlus dataTablePlus)
    //    {
    //        this.dataTablePlus = dataTablePlus;

    //        columnFromName = new Dictionary<string, Sql.DataColumnPlus>();
    //    }

    //    public DataColumnPlus this[int index]
    //    {
    //        get
    //        {
    //            try
    //            { // Perf: use the readonly _list field directly and let ArrayList check the range
    //                return (DataColumnPlus)_list[index];
    //            }
    //            catch (ArgumentOutOfRangeException)
    //            {
    //                throw ExceptionBuilder.ColumnOutOfRange(index);
    //            }
    //        }
    //    }

    //    public DataColumnPlus this[string name]
    //    {
    //        get
    //        {
    //            if (null == name)
    //            {
    //                throw ExceptionBuilder.ArgumentNull("name");
    //            }
    //            DataColumnPlus column;
    //            if ((!columnFromName.TryGetValue(name, out column)) || (column == null))
    //            {
    //                // Case-Insensitive compares
    //                int index = IndexOfCaseInsensitive(name);
    //                if (0 <= index)
    //                {
    //                    column = (DataColumnPlus)_list[index];
    //                }
    //                else if (-2 == index)
    //                {
    //                    throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
    //                }
    //            }
    //            return column;
    //        }
    //    }
    //}

    //public class DataColumnPlus
    //{


    //    public string ColumnName { get; set; }
    //    public int? MaximumCharacterLength { get; set; }
    //    public int? NumericPrecision { get; set; }
    //    public int? NumericScale { get; set; }
    //    public bool IsNullable { get; set; }
    //    public bool IsPrimaryKey { get; set; } = false;
    //    public bool IsTimestamp { get; set; } = false;
    //    public object DefaultValue { get; set; }
    //    public Type DataType { get; set; } = typeof(string);
    //    public int OrdinalPosition { get; set; } = 0;
    //    public bool IsReadOnly { get; set; } = false;

    //    public DataColumnPlus()
    //    {

    //    }

    //    public DataColumnPlus(string columnName, int? maximumCharacterLength, int? numericPrecision, int? numericScale, bool isNullable, bool isPrimaryKey, bool isTimestamp, object defaultValue, Type dataType, int ordinalPosition, bool isReadOnly)
    //    {
    //        this.ColumnName = columnName;
    //        this.MaximumCharacterLength = maximumCharacterLength;
    //        this.NumericPrecision = numericPrecision;
    //        this.NumericScale = numericScale;
    //        this.IsNullable = isNullable;
    //        this.IsPrimaryKey = isPrimaryKey;
    //        this.IsTimestamp = isTimestamp;
    //        this.DefaultValue = defaultValue;
    //        this.DataType = dataType;
    //        this.OrdinalPosition = ordinalPosition;
    //        this.IsReadOnly = isReadOnly;
    //    }
    //}



    public static class SqlExtensionMethods
    {
        public static string ToSqlString(this DateTime input)
        {
            return input.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string ToSqlString(this object input)
        {
            if (input == null)
                return null;

            if (input is DateTime)
                return ((DateTime)input).ToString("yyyy-MM-dd HH:mm:ss");

            if (input is byte[])
                return VariableTools.ByteArrayToHexSQL((byte[])input);

            //if (typeof(T) == typeof(DateTime))
            //    return ((DateTime)Convert.ChangeType(Value, typeof(DateTime))).ToSqlString();

            return input.ToString();
        }
    }
}
