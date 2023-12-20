using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Net.WebSockets;
using System.Security.Policy;
using Emgu.CV.CvEnum;
using System.ComponentModel.DataAnnotations;
using Emgu.CV.Dnn;
using System.Xml.Linq;
using mfi_erp_api;
using System.Windows.Media.Media3D;
using System.Data;

namespace SampleOpenCV
{
    public class WorkStationData : INotifyPropertyChanged, IEquatable<WorkStationData>
    {
        public event PropertyChangedEventHandler PropertyChanged;


        public bool Equals(WorkStationData y)
        {
            if (y == null)
                return false;
            return (   //this.PropertyChanged==y.PropertyChanged &&
                   this.CameraFK == y.CameraFK
                && this.MachinePK == y.MachinePK
                && this.Name == y.Name
                && this.Description == y.Description
                //&& this.IsActive==y.IsActive
                && this.Threshold == y.Threshold
                && this.LogMode == y.LogMode
                && this.LogValues == y.LogValues
                //&& this.IsRunning==y.IsRunning
                && this.IPAddress == y.IPAddress
                && this.LastPing == y.LastPing
                && this.IsIdle == y.IsIdle
                && this.LastStopTime == y.LastStopTime
                // && this.ErrorConnecting == y.ErrorConnecting
                && this.AplzID == y.AplzID
                && this.LastMdeMeldungTime == y.LastMdeMeldungTime
                && this.ProgramName == y.ProgramName
                //&& this.Timestamp==y.Timestamp
                //&& this.CameraStreamURL==y.CameraStreamURL
                && this.CameraFK == y.CameraFK
                && this.Model == y.Model
                && this.Serial == y.Serial
                );
        }

        public override bool Equals(object? obj) => Equals(obj as WorkStationData);
        public int GetHashCode(WorkStationData obj) => (Name, Description).GetHashCode();


        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public int MachinePK
        {
            get { return m_MachinePK; }
            set { m_MachinePK = value; OnPropertyChanged("MachinePK"); }
        }
        int m_MachinePK;
        public string? Name
        {
            get => m_Name;
            set { m_Name = value; OnPropertyChanged("Name"); }
        }
        string? m_Name;
        public string? Description
        {
            get => m_Description;
            set { m_Description = value; OnPropertyChanged("Description"); }
        }
        string? m_Description;
        public bool IsActive
        {
            get { return m_IsActive; }
            set { m_IsActive = value; OnPropertyChanged("IsActive"); }
        }
        bool m_IsActive;
        public int Threshold
        {
            get { return m_Threshold; }
            set { m_Threshold = value; OnPropertyChanged("Threshold"); }
        }
        int m_Threshold;
        public bool LogMode
        {
            get { return m_LogMode; }
            set { m_LogMode = value; OnPropertyChanged("LogMode"); }
        }
        bool m_LogMode;
        public bool LogValues
        {
            get { return m_LogValues; }
            set { m_LogValues = value; OnPropertyChanged("LogValues"); }
        }
        bool m_LogValues;
        public bool IsRunning
        {
            get { return m_IsRunning; }
            set { m_IsRunning = value; OnPropertyChanged("IsRunning"); }
        }
        bool m_IsRunning;
        public string? IPAddress
        {
            get => m_IPAddress;
            set { m_IPAddress = value; OnPropertyChanged("IPAddress"); }
        }
        string? m_IPAddress;
        public DateTime? LastPing
        {
            get => m_LastPing;
            set { m_LastPing = value; OnPropertyChanged("LastPing"); }
        }
        DateTime? m_LastPing;
        public bool IsIdle
        {
            get { return m_IsIdle; }
            set { m_IsIdle = value; OnPropertyChanged("IsIdle"); }
        }
        bool m_IsIdle;
        public DateTime? LastStopTime
        {
            get => m_LastStopTime;
            set { m_LastStopTime = value; OnPropertyChanged("LastStopTime"); }
        }
        DateTime? m_LastStopTime;
        public bool ErrorConnecting
        {
            get { return m_ErrorConnecting; }
            set { m_ErrorConnecting = value; OnPropertyChanged("ErrorConnecting"); }
        }
        bool m_ErrorConnecting;
        public int AplzID
        {
            get { return m_AplzID; }
            set { m_AplzID = value; OnPropertyChanged("AplzID"); }
        }
        int m_AplzID;
        public DateTime? LastMdeMeldungTime
        {
            get => m_LastMdeMeldungTime;
            set { m_LastMdeMeldungTime = value; OnPropertyChanged("LastMdeMeldungTime"); }
        }
        DateTime? m_LastMdeMeldungTime;
        public string? ProgramName
        {
            get => m_ProgramName;
            set { m_ProgramName = value; OnPropertyChanged("ProgramName"); }
        }
        string? m_ProgramName;
        public TimeStamp Timestamp
        {
            get => m_Timestamp;
            set { m_Timestamp = value; OnPropertyChanged("Timestamp"); }
        }
        TimeStamp m_Timestamp;
        public string? CameraStreamURL
        {
            get => m_CameraStreamURL;
            set { m_CameraStreamURL = value; OnPropertyChanged("CameraStreamURL"); }
        }
        string? m_CameraStreamURL;
        public int CameraFK
        {
            get { return m_CameraFK; }
            set { m_CameraFK = value; OnPropertyChanged("CameraFK"); }
        }
        int m_CameraFK;
        public string? Model
        {
            get => m_Model;
            set { m_Model = value; OnPropertyChanged("Model"); }
        }
        string? m_Model;

        public string? Serial
        {
            get => m_Serial;
            set { m_Serial = value; OnPropertyChanged("Serial"); }
        }
        string? m_Serial;
        public bool LoadData(ref Microsoft.Data.SqlClient.SqlDataReader reader)
        {
            MachinePK           = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
            Name                = reader.IsDBNull(1) ?"" : reader.GetString(1);
            Description         = reader.IsDBNull(2) ?"" : reader.GetString(2);
            IsActive            = reader.IsDBNull(3) ? false : reader.GetBoolean(3);
            Threshold           = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
            LogMode             = reader.IsDBNull(5) ? false : reader.GetBoolean(5);
            LogValues           = reader.IsDBNull(6) ? false : reader.GetBoolean(6);
            IsRunning           = reader.IsDBNull(7) ? false : reader.GetBoolean(7);
            IPAddress           = reader.IsDBNull(8) ?"" : reader.GetString(8);
            LastPing            = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9);
            IsIdle              = reader.IsDBNull(10) ? false : reader.GetBoolean(10);
            LastStopTime        = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11);
            ErrorConnecting     = reader.IsDBNull(12) ? false : reader.GetBoolean(12);
            AplzID              = reader.IsDBNull(13) ? 0 : reader.GetInt32(13);
            LastMdeMeldungTime  = reader.IsDBNull(14) ? (DateTime?)null : reader.GetDateTime(14);
            ProgramName         = reader.IsDBNull(15) ?"" : reader.GetString(15);
            //Timestamp = reader.IsDBNull(16) ? (TimeStamp)null : reader.GetValue(16).TryCast<TimeStamp>;
            CameraStreamURL     = reader.IsDBNull(17) ?"" : reader.GetString(17);
            CameraFK            = reader.IsDBNull(18) ? 0 : reader.GetInt32(18);
            Model               = reader.IsDBNull(19) ?"" : reader.GetString(19);
            Serial              = reader.IsDBNull(20) ?"" : reader.GetString(20);

            // Flag all of the properties
            OnPropertyChanged(string.Empty);

            return true;
        }



        public bool SaveData(ref Microsoft.Data.SqlClient.SqlCommand sqlCommand)
        {
            if (sqlCommand != null)
            {
                // We will null out the CameraStreamURL in the 
                // Machine table.  The correct URL exists in the
                // Camera table.
                sqlCommand.Parameters.Clear();
                /*sqlCommand.CommandText = @"Update Machine Set
                                           ,Name                 = @Name              
                                           ,Description          = @Description       
                                           ,IsActive             = @IsActive          
                                           ,Threshold            = @Threshold         
                                           ,LogMode              = @LogMode           
                                           ,LogValues            = @LogValues         
                                           ,IsRunning            = @IsRunning         
                                           ,IPAddress            = @IPAddress         
                                           ,LastPing             = @LastPing          
                                           ,IsIdle               = @IsIdle            
                                           ,LastStopTime         = @LastStopTime      
                                           ,ErrorConnecting      = @ErrorConnecting   
                                           ,AplzID               = @AplzID            
                                           ,LastMdeMeldungTime   = @LastMdeMeldungTime
                                           ,ProgramName          = @ProgramName       
                                           ,CameraStreamURL      = @CameraStreamURL   
                                           ,CameraFK             = @CameraFK          
                                           ,Model                = @Model             
                                           ,Serial               = @Serial            
                                        Where MachinePK = @MachinePK";*/
                sqlCommand.CommandText =  @"Update Machine Set
                                            CameraFK        = @CameraFK          
                                            Where MachinePK = @MachinePK";

                sqlCommand.Parameters.Add(new SqlParameter("@MachinePK", SqlDbType.Int)).Value = this.MachinePK;
//                sqlCommand.Parameters.Add(new SqlParameter("@Description", SqlDbType.Text)).Value = this.Description         ;
  //              sqlCommand.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit)).Value = this.IsActive            ;
    //            sqlCommand.Parameters.Add(new SqlParameter("@Threshold", SqlDbType.Int)).Value = this.Threshold           ;
      //          sqlCommand.Parameters.Add(new SqlParameter("@LogMode", SqlDbType.Bit)).Value = this.LogMode             ;
        //        sqlCommand.Parameters.Add(new SqlParameter("@LogValues", SqlDbType.Bit)).Value = this.LogValues           ;
          //      sqlCommand.Parameters.Add(new SqlParameter("@IsRunning", SqlDbType.Bit)).Value = this.IsRunning           ;
            //    sqlCommand.Parameters.Add(new SqlParameter("@IPAddress", SqlDbType.Text)).Value = this.IPAddress           ;
              //  sqlCommand.Parameters.Add(new SqlParameter("@LastPing", SqlDbType.DateTime2)).Value = this.LastPing            ;
                //sqlCommand.Parameters.Add(new SqlParameter("@IsIdle", SqlDbType.Bit)).Value = this.IsIdle              ;
//                sqlCommand.Parameters.Add(new SqlParameter("@LastStopTime", SqlDbType.DateTime2)).Value = this.LastStopTime        ;
  //              sqlCommand.Parameters.Add(new SqlParameter("@ErrorConnecting", SqlDbType.Bit)).Value = this.ErrorConnecting     ;
    //            sqlCommand.Parameters.Add(new SqlParameter("@AplzID", SqlDbType.Int)).Value = this.AplzID              ;
      //          sqlCommand.Parameters.Add(new SqlParameter("@LastMdeMeldungTime", SqlDbType.DateTime2)).Value = this.LastMdeMeldungTime  ;
        //        sqlCommand.Parameters.Add(new SqlParameter("@ProgramName", SqlDbType.Text)).Value = this.ProgramName         ;
          //      sqlCommand.Parameters.Add(new SqlParameter("@CameraStreamURL", SqlDbType.Text)).Value = this.CameraStreamURL;
                sqlCommand.Parameters.Add(new SqlParameter("@CameraFK", SqlDbType.Int)).Value = this.CameraFK       ;
//                sqlCommand.Parameters.Add(new SqlParameter("@Model", SqlDbType.Text)).Value = this.Model          ;
  //              sqlCommand.Parameters.Add(new SqlParameter("@Serial", SqlDbType.Text)).Value = this.Serial         ;

                sqlCommand.ExecuteNonQuery();
            }
            return true;
        }


    }
}