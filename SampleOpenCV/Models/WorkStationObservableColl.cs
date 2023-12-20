using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SharpTech.Sql;

namespace SampleOpenCV
{
    public class WorkStationObservableColl : ObservableCollection<WorkStationData>
    {
        public bool Different(WorkStationObservableColl compSrc)
        {
            int i = 0;

            // If they're different sizes, return immediately
            if (compSrc.Count != this.Count)
                return true;

            // If they're the same size, loop through and compare
            foreach (WorkStationData c in compSrc)
            {
                if (c != null)
                {
                    WorkStationData c2 = this.ElementAt(i);
                    if (c2 != c)
                        return true;
                }
                i++;
            }

            return false;
        }
        public void PopulateFromSqlCommand(ref Microsoft.Data.SqlClient.SqlCommand sqlCommand)
        {
            sqlCommand.CommandTimeout = 10; // I don't want this command to take longer than 10 seconds.
            sqlCommand.CommandText = @" SELECT [MachinePK]          /* 00 */   
                                              ,[Name]               /* 01 */
                                              ,[Description]        /* 02 */
                                              ,[IsActive]           /* 03 */
                                              ,[Threshold]          /* 04 */
                                              ,[LogMode]            /* 05 */
                                              ,[LogValues]          /* 06 */
                                              ,[IsRunning]          /* 07 */
                                              ,[IPAddress]          /* 08 */
                                              ,[LastPing]           /* 09 */
                                              ,[IsIdle]             /* 10 */
                                              ,[LastStopTime]       /* 11 */
                                              ,[ErrorConnecting]    /* 12 */
                                              ,[AplzID]             /* 13 */
                                              ,[LastMdeMeldungTime] /* 14 */
                                              ,[ProgramName]        /* 15 */
                                              ,[Timestamp]          /* 16 */
                                              ,[CameraStreamURL]    /* 17 */
                                              ,[CameraFK]           /* 18 */
                                              ,[Model]              /* 19 */
                                              ,[Serial]             /* 20 */
                                                FROM [Machine]      
                                                /* WHERE [CameraStreamURL] IS NOT NULL */
                                                ORDER BY [Name] Asc";

            Microsoft.Data.SqlClient.SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            if (sqlDataReader.HasRows)
            {
                int i = 0;

                while (sqlDataReader.Read())
                {
                    WorkStationData wkstn = new WorkStationData();

                    if (wkstn != null)
                    {
                        wkstn.LoadData(ref sqlDataReader);
                        Add(wkstn);
                    }
                }
            }
            sqlDataReader.Close();
        }
    }

}
