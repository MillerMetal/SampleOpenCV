using System.Data;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SharpTech.Sql
{
    public class DatabaseDefinition
    {
        public string ConnectionString { get; set; }
        public int CommandTimeoutSeconds { get; set; } = 300;

        public DatabaseDefinition()
        {

        }

        public DatabaseDefinition(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public QueryResults GetResults(string queryString, params SqlParameter[] sqlParameters)
        {
            QueryResults queryResults = new QueryResults();

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeoutSeconds;

                        if (sqlParameters != null && sqlParameters.Length > 0)
                            sqlCommand.Parameters.AddRange(sqlParameters);

                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            if (sqlDataReader != null)
                            {
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

        public async Task<QueryResults> GetResultsAsync(string queryString, params SqlParameter[] sqlParameters)
        {
            QueryResults queryResults = new QueryResults();

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    await sqlConnection.OpenAsync();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeoutSeconds;

                        if (sqlParameters != null && sqlParameters.Length > 0)
                            sqlCommand.Parameters.AddRange(sqlParameters);

                        using (SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                        {
                            if (sqlDataReader != null)
                            {
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

        public QueryResults GetFirstResult(string queryString, params SqlParameter[] sqlParameters)
        {
            QueryResults queryResults = new QueryResults();

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeoutSeconds;

                        if (sqlParameters != null && sqlParameters.Length > 0)
                            sqlCommand.Parameters.AddRange(sqlParameters);

                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            if (sqlDataReader != null)
                            {
                                if (sqlDataReader.Read())
                                {
                                    object[] row = new object[sqlDataReader.FieldCount];

                                    sqlDataReader.GetValues(row);

                                    //queryResults.Row = row;

                                    queryResults.Rows = new List<object[]>();

                                    queryResults.Rows.Add(row);
                                }
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

        public async Task<QueryResults> GetFirstResultAsync(string queryString, params SqlParameter[] sqlParameters)
        {
            QueryResults queryResults = new QueryResults();

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    await sqlConnection.OpenAsync();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeoutSeconds;

                        if (sqlParameters != null && sqlParameters.Length > 0)
                            sqlCommand.Parameters.AddRange(sqlParameters);

                        using (SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                        {
                            if (sqlDataReader != null)
                            {
                                if (sqlDataReader.Read())
                                {
                                    object[] row = new object[sqlDataReader.FieldCount];

                                    sqlDataReader.GetValues(row);

                                    //queryResults.Row = row;

                                    queryResults.Rows = new List<object[]>();

                                    queryResults.Rows.Add(row);
                                }
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

        public QueryResults Execute(string queryString, params SqlParameter[] sqlParameters)
        {
            QueryResults queryResults = new QueryResults();

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeoutSeconds;

                        if (sqlParameters != null && sqlParameters.Length > 0)
                            sqlCommand.Parameters.AddRange(sqlParameters);

                        queryResults.RowsAffected = sqlCommand.ExecuteNonQuery();
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

        public async Task<QueryResults> ExecuteAsync(string queryString, params SqlParameter[] sqlParameters)
        {
            QueryResults queryResults = new QueryResults();

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    await sqlConnection.OpenAsync();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeoutSeconds;

                        queryResults.RowsAffected = await sqlCommand.ExecuteNonQueryAsync();
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
                        sqlCommand.CommandTimeout = CommandTimeoutSeconds;

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

        

        // Overload of original Insert method
        public int Insert(string queryString, params SqlParameter[] sqlParameters)
        {
            int returnInt;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = CommandTimeoutSeconds;

                        // This sanitizes things so that injection attacks are thwarted.
                        if (sqlParameters!=null && sqlParameters.Length>0)
                        {
                            // This is a pameter that cannot be executed. 
                            sqlCommand.Parameters.AddRange(sqlParameters);
                        }

                        //returnInt = (int)sqlCommand.ExecuteScalar();

                        object obj = sqlCommand.ExecuteScalar();

                        if (obj == null)
                            returnInt = -1;
                        else
                            returnInt = (int)obj;
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
}
