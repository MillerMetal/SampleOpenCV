using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Net.WebSockets;
using System.Security.Policy;

using System.ComponentModel.DataAnnotations;
using System.Transactions;
using System.Xml.Linq;
using System.Diagnostics.Eventing.Reader;
using static Azure.Core.HttpHeader;
using System.Data;

namespace SampleOpenCV
{
    



    public class CompareCamera : IEqualityComparer<CameraData>
    {
        public CompareCamera()
        {
        }
        public bool Equals(CameraData x, CameraData y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return false;
        }
        public int GetHashCode(CameraData obj)
        {
            return (int)obj.GetHashCode();
        }
    }



    public class CameraData : INotifyPropertyChanged, IEquatable<CameraData>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool Equals(CameraData y)
        {
            if (y == null)
                return false;

            return Static.NormalizeWhiteSpace(CameraMatrix) == Static.NormalizeWhiteSpace(y.CameraMatrix)
                && CameraPK == y.CameraPK
                && Static.NormalizeWhiteSpace(DistortionMatrix) == Static.NormalizeWhiteSpace(y.DistortionMatrix)
                && Static.NormalizeWhiteSpace(HomographyMatrix) == Static.NormalizeWhiteSpace(y.HomographyMatrix)
                && ResultScale == y.ResultScale
                && ResultWidth == y.ResultWidth
                && ResultLength == y.ResultLength
                && BorderTop == y.BorderTop
                && BorderBottom == y.BorderBottom
                && BorderLeft == y.BorderLeft
                && BorderRight == y.BorderRight
                && URL == y.URL
                && DiffOfGaussRadius1 == y.DiffOfGaussRadius1
                && DiffOfGaussSigma1 == y.DiffOfGaussSigma1
                && DiffOfGaussRadius2 == y.DiffOfGaussRadius2
                && DiffOfGaussSigma2 == y.DiffOfGaussSigma2
                && FillToleranceHigh == y.FillToleranceHigh
                && FillToleranceLow == y.FillToleranceLow
                && ImageIndex == y.ImageIndex
                && NumIterations == y.NumIterations
                && SegmentLength == y.SegmentLength
                && ColorRadius == y.ColorRadius
                && SpatialRadius == y.SpatialRadius
                && CameraProcessingURL == y.CameraProcessingURL;
        }


        public override bool Equals(object? obj) => Equals(obj as CameraData);
        public int GetHashCode(WorkStationData obj) => (CameraPK, Static.NormalizeWhiteSpace(CameraMatrix)
            , Static.NormalizeWhiteSpace(DistortionMatrix), Static.NormalizeWhiteSpace(HomographyMatrix),ResultLength,ResultScale,ResultWidth
            ,BorderBottom,BorderLeft,BorderRight,BorderTop,URL,DiffOfGaussRadius1,DiffOfGaussRadius2,DiffOfGaussSigma1,DiffOfGaussSigma2
            ,FillToleranceHigh,FillToleranceLow,ImageIndex,NumIterations,ColorRadius,SpatialRadius,CameraProcessingURL).GetHashCode();

        public int ROILeft
        {
            get { return m_nROILeft; }
            set { m_nROILeft = value; OnPropertyChanged("ROILeft"); }
        }
        int m_nROILeft;
        public int ROITop
        {
            get { return m_nROITop; }
            set { m_nROITop = value; OnPropertyChanged("ROITop"); }
        }
        int m_nROITop;
        public int ROIRight
        {
            get { return m_nROIRight; }
            set { m_nROIRight = value; OnPropertyChanged("ROIRight"); }
        }
        int m_nROIRight;
        public int ROIBottom
        {
            get { return m_nROIBottom; }
            set { m_nROIBottom = value; OnPropertyChanged("ROIBottom"); }
        }
        int m_nROIBottom;




        public int CameraPK
        {
            get { return m_CameraPK; }
            private set { m_CameraPK = value; OnPropertyChanged("CameraPK"); }
        }
        int m_CameraPK;

        public string? CameraMatrix
        {
            get => m_CameraMatrix;
            set { m_CameraMatrix = value; OnPropertyChanged("CameraMatrix"); }
        }
        string? m_CameraMatrix;

        public string? DistortionMatrix
        {
            get => m_DistortionMatrix; 
            set { m_DistortionMatrix = value; OnPropertyChanged("m_DistortionMatrix"); }
        }
        string? m_DistortionMatrix;

        public string? HomographyMatrix
        {
            get => m_HomographyMatrix;
            set { m_HomographyMatrix = value; OnPropertyChanged("HomographyMatrix"); }
        }
        string? m_HomographyMatrix;

        public double ResultScale
        {
            get { return m_ResultScale; }
            set { m_ResultScale = value; OnPropertyChanged("ResultScale"); }
        }
        double m_ResultScale;

        public double ResultWidth
        {
            get { return m_ResultWidth; }
            set { m_ResultWidth = value; OnPropertyChanged("ResultWidth"); }
        }
        double m_ResultWidth;
        public double ResultLength
        {
            get { return m_ResultLength; }
            set { m_ResultLength = value; OnPropertyChanged("ResultLength"); }
        }
        double m_ResultLength;

        public double BorderTop
        {
            get { return m_BorderTop; }
            set { m_BorderTop = value; OnPropertyChanged("BorderTop"); }
        }
        double m_BorderTop;

        public double BorderLeft
        {
            get { return m_BorderLeft; }
            set { m_BorderLeft = value; OnPropertyChanged("BorderLeft"); }
        }
        double m_BorderLeft;

        public double BorderRight
        {
            get { return m_BorderRight; }
            set { m_BorderRight = value; OnPropertyChanged("BorderRight"); }
        }
        double m_BorderRight;

        public double BorderBottom
        {
            get { return m_BorderBottom; }
            set { m_BorderBottom = value; OnPropertyChanged("BorderBottom"); }
        }
        double m_BorderBottom;
        public string? URL
        {
            get => m_URL;
            set { m_URL = value; OnPropertyChanged("URL"); }
        }
        string? m_URL;





        public double DiffOfGaussRadius1
        {
            get { return m_DiffOfGaussRadius1; }
            set { m_DiffOfGaussRadius1 = value; OnPropertyChanged("DiffOfGaussRadius1"); }
        }
        double m_DiffOfGaussRadius1;
        public double DiffOfGaussSigma1
        {
            get { return m_DiffOfGaussSigma1; }
            set { m_DiffOfGaussSigma1 = value; OnPropertyChanged("DiffOfGaussSigma1"); }
        }
        double m_DiffOfGaussSigma1;
        public double DiffOfGaussRadius2
        {
            get { return m_DiffOfGaussRadius2; }
            set { m_DiffOfGaussRadius2 = value; OnPropertyChanged("DiffOfGaussRadius2"); }
        }
        double m_DiffOfGaussRadius2;

        public double DiffOfGaussSigma2
        {
            get { return m_DiffOfGaussSigma2; }
            set { m_DiffOfGaussSigma2 = value; OnPropertyChanged("DiffOfGaussSigma2"); }
        }
        double m_DiffOfGaussSigma2;
        public int FillToleranceLow
        {
            get { return m_FillToleranceLow; }
            set { m_FillToleranceLow = value; OnPropertyChanged("FillToleranceLow"); }
        }
        int m_FillToleranceLow;
        public int FillToleranceHigh
        {
            get { return m_FillToleranceHigh; }
            set { m_FillToleranceHigh = value; OnPropertyChanged("FillToleranceHigh"); }
        }
        int m_FillToleranceHigh;
        public int ImageIndex
        {
            get { return m_ImageIndex; }
            set { m_ImageIndex = value; OnPropertyChanged("ImageIndex"); }
        }
        int m_ImageIndex;


        public int NumIterations
        {
            get { return m_NumIterations; }
            set { m_NumIterations = value; OnPropertyChanged("NumIterations"); }
        }
        int m_NumIterations;
        public int SegmentLength
        {
            get { return m_SegmentLength; }
            set { m_SegmentLength = value; OnPropertyChanged("SegmentLength"); }
        }
        int m_SegmentLength;

        public int ColorRadius
        {
            get { return m_ColorRadius; }
            set { m_ColorRadius = value; OnPropertyChanged("ColorRadius"); }
        }
        int m_ColorRadius;
        public int SpatialRadius
        {
            get { return m_SpatialRadius; }
            set { m_SpatialRadius = value; OnPropertyChanged("SpatialRadius"); }
        }
        int m_SpatialRadius;
        public string? CameraProcessingURL
        {
            get => m_CameraProcessingURL;
            set { m_CameraProcessingURL = value; OnPropertyChanged("CameraProcessingURL"); }
        }
        string? m_CameraProcessingURL;



        public void SetCameraPK (int nNewVal)
        {
            CameraPK = nNewVal;
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public CameraData()
        {
            ROIBottom = 0;
            ROILeft = 0;
            ROIRight = 0;
            ROITop = 0;

            CameraPK = -1;
            CameraMatrix = DistortionMatrix = HomographyMatrix = "";
            ResultScale = 32;
            ResultWidth = 83;
            ResultLength = 132;
            BorderTop = 5;
            BorderLeft = 5;
            BorderRight = 5;
            BorderBottom = 5;
            URL = "rtsp://user:admin@192.168.1.2_CHANGEME";
            DiffOfGaussRadius1 = 1;
            DiffOfGaussSigma1 = 1;
            DiffOfGaussRadius2 = 2;
            DiffOfGaussSigma2 = 1;
            ImageIndex = 1;
            FillToleranceLow = 10;
            FillToleranceHigh = 10;
            NumIterations = 20;
            SegmentLength = 75;
            ColorRadius = 30;
            SpatialRadius = 10;
        }


        // This method inserts a row into the Camera table in the MachineInterface database
        // using the current object.  If it has a CameraPK associated with it already,
        // that association will be lost to the current object, but not the Database.
        // On exit, the new 
        public bool Insert(ref Microsoft.Data.SqlClient.SqlCommand cmd)
        {
            if (cmd == null)
            {
                return false;
            }

            //cmd.CommandText = 

            return true;
        }

        public bool LoadData(ref Microsoft.Data.SqlClient.SqlDataReader reader)
        {
            CameraPK                = reader.IsDBNull(0) ? 0 :  reader.GetInt32(0);
            CameraMatrix            = reader.IsDBNull(1) ? "" : reader.GetString(1);
            DistortionMatrix        = reader.IsDBNull(2) ? "" : reader.GetString(2);
            HomographyMatrix        = reader.IsDBNull(3) ? "" : reader.GetString(3);
            ResultScale             = reader.IsDBNull(4) ? 0 :  reader.GetDouble(4);
            ResultWidth             = reader.IsDBNull(5) ? 0 :  reader.GetDouble(5);
            ResultLength            = reader.IsDBNull(6) ? 0 :  reader.GetDouble(6);
            BorderTop               = reader.IsDBNull(7) ? 0 :  reader.GetDouble(7);
            BorderLeft              = reader.IsDBNull(8) ? 0 :  reader.GetDouble(8);
            BorderRight             = reader.IsDBNull(9) ? 0 :  reader.GetDouble(9);
            BorderBottom            = reader.IsDBNull(10) ? 0 : reader.GetDouble(10);
            URL                     = reader.IsDBNull(11) ? "" :reader.GetString(11);
            DiffOfGaussRadius1      = reader.IsDBNull(12) ? 0 : reader.GetDouble(12);
            DiffOfGaussSigma1       = reader.IsDBNull(13) ? 0 : reader.GetDouble(13);
            DiffOfGaussRadius2      = reader.IsDBNull(14) ? 0 : reader.GetDouble(14);
            DiffOfGaussSigma2       = reader.IsDBNull(15) ? 0 : reader.GetDouble(15);
            ImageIndex              = reader.IsDBNull(16) ? 0 : reader.GetInt32(16);
            FillToleranceLow        = reader.IsDBNull(17) ? 0 : reader.GetInt32(17);
            FillToleranceHigh       = reader.IsDBNull(18) ? 0 : reader.GetInt32(18);
            NumIterations           = reader.IsDBNull(19) ? 0 : reader.GetInt32(19);
            SegmentLength           = reader.IsDBNull(20) ? 0 : reader.GetInt32(20);
            ColorRadius             = reader.IsDBNull(21) ? 0 : reader.GetInt32(21);
            SpatialRadius           = reader.IsDBNull(22) ? 0 : reader.GetInt32(22);
            CameraProcessingURL     = reader.IsDBNull(23) ? "" :reader.GetString(23);
            ROILeft                 = reader.IsDBNull(24) ? 0 : reader.GetInt32(24);
            ROITop                  = reader.IsDBNull(25) ? 0 : reader.GetInt32(25);
            ROIRight                = reader.IsDBNull(26) ? 0 : reader.GetInt32(26);
            ROIBottom               = reader.IsDBNull(27) ? 0 : reader.GetInt32(27);

            // Flag all of the properties
            OnPropertyChanged(string.Empty);

            return true;
        }

        public bool SaveData(ref Microsoft.Data.SqlClient.SqlCommand sqlCommand)
        {
            int rval;
            if (sqlCommand != null)
            {
                // We will null out the CameraStreamURL in the 
                // Machine table.  The correct URL exists in the
                // Camera table.
                sqlCommand.Parameters.Clear();
                sqlCommand.CommandText = @"UPDATE [MachineInterface].[dbo].Camera SET
                                            CameraMatrix            = @CameraMatrix       
                                           ,DistortionMatrix        = @DistortionMatrix   
                                           ,HomographyMatrix        = @HomographyMatrix   
                                           ,ResultScale             = @ResultScale        
                                           ,ResultWidth             = @ResultWidth        
                                           ,ResultLength            = @ResultLength       
                                           ,BorderTop               = @BorderTop          
                                           ,BorderLeft              = @BorderLeft         
                                           ,BorderRight             = @BorderRight        
                                           ,BorderBottom            = @BorderBottom       
                                           ,URL                     = @URL                
                                           ,DiffOfGaussRadius1      = @DiffOfGaussRadius1 
                                           ,DiffOfGaussSigma1       = @DiffOfGaussSigma1  
                                           ,DiffOfGaussRadius2      = @DiffOfGaussRadius2 
                                           ,DiffOfGaussSigma2       = @DiffOfGaussSigma2  
                                           ,ImageIndex              = @ImageIndex         
                                           ,FillToleranceLow        = @FillToleranceLow   
                                           ,FillToleranceHigh       = @FillToleranceHigh  
                                           ,MSSNumIterations        = @MSSNumIterations      
                                           ,MSSSegmentLength        = @MSSSegmentLength      
                                           ,MSSColorRadius          = @MSSColorRadius        
                                           ,MSSSpatialRadius        = @MSSSpatialRadius      
                                           ,CameraProcessingURL     = @CameraProcessingURL
                                           ,ROILeft                 = @ROILeft
                                           ,ROITop                  = @ROITop
                                           ,ROIRight                = @ROIRight
                                           ,ROIBottom               = @ROIBottom
                                        Where CameraPK = @CameraPK";

                sqlCommand.Parameters.Add(new SqlParameter("@CameraPK", SqlDbType.Int)).Value = this.CameraPK       ;
                sqlCommand.Parameters.Add(new SqlParameter("@CameraMatrix", SqlDbType.Text)).Value = this.CameraMatrix       ;
                sqlCommand.Parameters.Add(new SqlParameter("@DistortionMatrix", SqlDbType.Text)).Value = this.DistortionMatrix   ;
                sqlCommand.Parameters.Add(new SqlParameter("@HomographyMatrix", SqlDbType.Text)).Value = this.HomographyMatrix   ;
                sqlCommand.Parameters.Add(new SqlParameter("@ResultScale", SqlDbType.Float)).Value = this.ResultScale        ;
                sqlCommand.Parameters.Add(new SqlParameter("@ResultWidth", SqlDbType.Float)).Value = this.ResultWidth        ;
                sqlCommand.Parameters.Add(new SqlParameter("@ResultLength", SqlDbType.Float)).Value = this.ResultLength       ;
                sqlCommand.Parameters.Add(new SqlParameter("@BorderTop", SqlDbType.Float)).Value = this.BorderTop          ;
                sqlCommand.Parameters.Add(new SqlParameter("@BorderLeft", SqlDbType.Float)).Value = this.BorderLeft         ;
                sqlCommand.Parameters.Add(new SqlParameter("@BorderRight", SqlDbType.Float)).Value = this.BorderRight        ;
                sqlCommand.Parameters.Add(new SqlParameter("@BorderBottom", SqlDbType.Float)).Value = this.BorderBottom       ;
                sqlCommand.Parameters.Add(new SqlParameter("@URL", SqlDbType.Text)).Value = this.URL                ;
                sqlCommand.Parameters.Add(new SqlParameter("@DiffOfGaussRadius1", SqlDbType.Float)).Value = this.DiffOfGaussRadius1 ;
                sqlCommand.Parameters.Add(new SqlParameter("@DiffOfGaussSigma1", SqlDbType.Float)).Value = this.DiffOfGaussSigma1  ;
                sqlCommand.Parameters.Add(new SqlParameter("@DiffOfGaussRadius2", SqlDbType.Float)).Value = this.DiffOfGaussRadius2 ;
                sqlCommand.Parameters.Add(new SqlParameter("@DiffOfGaussSigma2", SqlDbType.Float)).Value = this.DiffOfGaussSigma2  ;
                sqlCommand.Parameters.Add(new SqlParameter("@ImageIndex", SqlDbType.Int)).Value = this.ImageIndex         ;
                sqlCommand.Parameters.Add(new SqlParameter("@FillToleranceLow", SqlDbType.Int)).Value = this.FillToleranceLow   ;
                sqlCommand.Parameters.Add(new SqlParameter("@FillToleranceHigh", SqlDbType.Int)).Value = this.FillToleranceHigh  ;
                sqlCommand.Parameters.Add(new SqlParameter("@MSSNumIterations", SqlDbType.Int)).Value = this.NumIterations      ;
                sqlCommand.Parameters.Add(new SqlParameter("@MSSSegmentLength", SqlDbType.Int)).Value = this.SegmentLength      ;
                sqlCommand.Parameters.Add(new SqlParameter("@MSSColorRadius", SqlDbType.Int)).Value = this.ColorRadius        ;
                sqlCommand.Parameters.Add(new SqlParameter("@MSSSpatialRadius", SqlDbType.Int)).Value = this.SpatialRadius      ;
                sqlCommand.Parameters.Add(new SqlParameter("@CameraProcessingURL", SqlDbType.Text)).Value = this.CameraProcessingURL;
                sqlCommand.Parameters.Add(new SqlParameter("@ROILeft", SqlDbType.Int)).Value = this.ROILeft;
                sqlCommand.Parameters.Add(new SqlParameter("@ROITop", SqlDbType.Int)).Value = this.ROITop;
                sqlCommand.Parameters.Add(new SqlParameter("@ROIRight", SqlDbType.Int)).Value = this.ROIRight;
                sqlCommand.Parameters.Add(new SqlParameter("@ROIBottom", SqlDbType.Int)).Value = this.ROIBottom;

                rval = sqlCommand.ExecuteNonQuery();               
            }

            return true;
        }
    }
}
