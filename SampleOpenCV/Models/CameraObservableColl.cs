using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SharpTech.Sql;

namespace SampleOpenCV
{
    public class CameraObservableColl : ObservableCollection<CameraData>
    {
        public bool Different(CameraObservableColl compSrc)
        {
            int i = 0;

            // If they're different sizes, return immediately
            if (compSrc.Count != this.Count)
                return true;

            // If they're the same size, loop through and compare
            foreach (CameraData c in compSrc)
            {
                if (c != null)
                {
                    CameraData c2 = this.ElementAt(i);
                    if (c2 != c) 
                        return true;
                }
                i++;
            }

            return false ;
        }
        public void PopulateFromDB(ref Microsoft.Data.SqlClient.SqlCommand sqlCommand)
        {
            sqlCommand.CommandTimeout = 10; // I don't want this command to take longer than 10 seconds.
            sqlCommand.CommandText = @" SELECT [CameraPK]                   /* 00 */
                                                      ,[CameraMatrix]                   /* 01 */       
                                                      ,[DistortionMatrix]               /* 02 */
                                                      ,[HomographyMatrix]               /* 03 */
                                                      ,[ResultScale]                    /* 04 */
                                                      ,[ResultWidth]                    /* 05 */
                                                      ,[ResultLength]                   /* 06 */
                                                      ,[BorderTop]                      /* 07 */
                                                      ,[BorderLeft]                     /* 08 */
                                                      ,[BorderRight]                    /* 09 */
                                                      ,[BorderBottom]                   /* 10 */
                                                      ,[URL]                            /* 11 */
                                                      ,[DiffOfGaussRadius1]             /* 12 */
                                                      ,[DiffOfGaussSigma1]              /* 13 */
                                                      ,[DiffOfGaussRadius2]             /* 14 */
                                                      ,[DiffOfGaussSigma2]              /* 15 */
                                                      ,[FillToleranceLow]               /* 16 */
                                                      ,[FillToleranceHigh]              /* 17 */
                                                      ,[ImageIndex]                     /* 18 */
                                                      ,[MSSNumIterations]               /* 19 */
                                                      ,[MSSSegmentLength]               /* 20 */
                                                      ,[MSSColorRadius]                 /* 21 */
                                                      ,[MSSSpatialRadius]               /* 22 */
                                                      ,[CameraProcessingURL]            /* 23 */
                                                      ,[ROILeft]                        /* 24 */
                                                      ,[ROITop]                         /* 25 */
                                                      ,[ROIRight]                       /* 26 */
                                                      ,[ROIBottom]                      /* 27 */
                                                        FROM [Camera]
                                                        WHERE [URL] IS NOT NULL
                                                        ORDER BY [URL] Asc";

            Microsoft.Data.SqlClient.SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            if (sqlDataReader.HasRows)
            {
                int i = 0;

                while (sqlDataReader.Read())
                {
                    CameraData cam = new CameraData();

                    if (cam != null)
                    {
                        cam.LoadData(ref sqlDataReader);
                        Add(cam);
                        //if (cam.URL != null)
                        //{
                        //    CameraIPStrings.Add(cam.URL, cam.CameraPK);
                        //    CameraURLStrings.Add(cam.CameraPK, cam.URL);
                        //}
                    }
                }
            }
            sqlDataReader.Close();

        }

        public bool DeleteCurCam ( ref CameraData curCamData
                                 , ref Microsoft.Data.SqlClient.SqlCommand sqlCommand)
        {
            bool rval = false;

            //CameraData cam = 
            if (curCamData!= null)
            {
                sqlCommand.CommandTimeout = 10; // I don't want this command to take longer than 10 seconds.
                sqlCommand.CommandText = @"Delete from [Camera]
                                           Where CameraPK=@CameraPK";
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.Add(new SqlParameter("@CameraPK", SqlDbType.Int)).Value = curCamData.CameraPK;
                rval = (sqlCommand.ExecuteNonQuery() == 1);
                if (rval)
                {
                    // Only remove it from the list if we were able to remove it from the database.
                    rval = Remove(curCamData);
                }
            }

            return rval;
        }

        public bool AddNewCam( /*ref CameraData curCamData
                             , */ref Microsoft.Data.SqlClient.SqlCommand sqlCommand)
        {
            CameraData cam = new CameraData();
            bool rval = false;

            if (cam != null)
            {
                /*
                if (curCamData != null)
                {
                    // If the camera data was null, the initial
                    // camera data is used.  Otherwise, we use 
                    cam.BorderBottom        = curCamData.BorderBottom;
                    cam.BorderLeft          = curCamData.BorderLeft;
                    cam.BorderRight         = curCamData.BorderRight;   
                    cam.BorderTop           = curCamData.BorderTop;
                    cam.CameraMatrix        = curCamData.CameraMatrix;
                    cam.CameraProcessingURL = curCamData.CameraProcessingURL;
                    cam.ColorRadius         = curCamData.ColorRadius;
                    cam.DiffOfGaussRadius1  = curCamData.DiffOfGaussRadius1;
                    cam.DiffOfGaussRadius2  = curCamData.DiffOfGaussRadius2;
                    cam.DiffOfGaussSigma1   = curCamData.DiffOfGaussSigma1;
                    cam.DiffOfGaussSigma2   = curCamData.DiffOfGaussSigma2;
                    cam.DistortionMatrix    = curCamData.DistortionMatrix;
                    cam.FillToleranceHigh   = curCamData.FillToleranceHigh;
                    cam.FillToleranceLow    = curCamData.FillToleranceLow;  
                    cam.HomographyMatrix    = curCamData.HomographyMatrix;
                    cam.ImageIndex          = curCamData.ImageIndex;
                    cam.NumIterations       = curCamData.NumIterations;
                    cam.ResultLength        = curCamData.ResultLength;
                    cam.ResultScale         = curCamData.ResultScale;
                    cam.ResultWidth         = curCamData.ResultWidth;
                    cam.SegmentLength       = curCamData.SegmentLength;
                    cam.SpatialRadius       = curCamData.SpatialRadius;
                    // Leave cam.URL blank/null
                    //cam.URL                 = "rtsp://user:a_dmin_1@usernameANDORchangemyip";
                }*/

                if (sqlCommand != null)
                {
                    sqlCommand.CommandTimeout = 10; // I don't want this command to take longer than 10 seconds.
                    sqlCommand.CommandText = @"
                            insert into [Camera]
                            (BorderBottom       
                            ,BorderLeft         
                            ,BorderRight        
                            ,BorderTop          
                            ,CameraMatrix       
                            ,CameraProcessingURL
                            ,MSSColorRadius        
                            ,DiffOfGaussRadius1 
                            ,DiffOfGaussRadius2 
                            ,DiffOfGaussSigma1  
                            ,DiffOfGaussSigma2  
                            ,DistortionMatrix   
                            ,FillToleranceHigh  
                            ,FillToleranceLow   
                            ,HomographyMatrix   
                            ,ImageIndex         
                            ,MSSNumIterations      
                            ,ResultLength       
                            ,ResultScale        
                            ,ResultWidth        
                            ,MSSSegmentLength      
                            ,MSSSpatialRadius      
                            ,URL
                            ,ROILeft
                            ,ROITop
                            ,ROIRight
                            ,ROIBottom
                                )
                                VALUES
                            (@BorderBottom       
                            ,@BorderLeft         
                            ,@BorderRight        
                            ,@BorderTop          
                            ,@CameraMatrix       
                            ,@CameraProcessingURL
                            ,@ColorRadius        
                            ,@DiffOfGaussRadius1 
                            ,@DiffOfGaussRadius2 
                            ,@DiffOfGaussSigma1  
                            ,@DiffOfGaussSigma2  
                            ,@DistortionMatrix   
                            ,@FillToleranceHigh  
                            ,@FillToleranceLow   
                            ,@HomographyMatrix   
                            ,@ImageIndex         
                            ,@NumIterations      
                            ,@ResultLength       
                            ,@ResultScale        
                            ,@ResultWidth        
                            ,@SegmentLength      
                            ,@SpatialRadius      
                            ,@URL
                            ,@ROILeft
                            ,@ROITop
                            ,@ROIRight
                            ,@ROIBottom
                            )
                            Set @CameraPK = SCOPE_IDENTITY()";
                    sqlCommand.Parameters.Add("@BorderBottom        ", SqlDbType.Real ).Value = cam.BorderBottom;
                    sqlCommand.Parameters.Add("@BorderLeft          ", SqlDbType.Real).Value = cam.BorderLeft          ;
                    sqlCommand.Parameters.Add("@BorderRight         ", SqlDbType.Real).Value = cam.BorderRight         ;
                    sqlCommand.Parameters.Add("@BorderTop           ", SqlDbType.Real).Value = cam.BorderTop           ;
                    sqlCommand.Parameters.Add("@CameraMatrix        ", SqlDbType.NVarChar, 4000).Value = cam.CameraMatrix?? (object)DBNull.Value;
                    sqlCommand.Parameters.Add("@CameraProcessingURL ", SqlDbType.NVarChar, 4000).Value = cam.CameraProcessingURL?? (object)DBNull.Value ;
                    sqlCommand.Parameters.Add("@ColorRadius         ", SqlDbType.Int ).Value = cam.ColorRadius         ;
                    sqlCommand.Parameters.Add("@DiffOfGaussRadius1  ", SqlDbType.Real ).Value = cam.DiffOfGaussRadius1  ;
                    sqlCommand.Parameters.Add("@DiffOfGaussRadius2  ", SqlDbType.Real ).Value = cam.DiffOfGaussRadius2  ;
                    sqlCommand.Parameters.Add("@DiffOfGaussSigma1   ", SqlDbType.Real ).Value = cam.DiffOfGaussSigma1   ;
                    sqlCommand.Parameters.Add("@DiffOfGaussSigma2   ", SqlDbType.Real ).Value = cam.DiffOfGaussSigma2   ;
                    sqlCommand.Parameters.Add("@DistortionMatrix    ", SqlDbType.NVarChar, 4000).Value = cam.DistortionMatrix ?? (object)DBNull.Value;
                    sqlCommand.Parameters.Add("@FillToleranceHigh   ", SqlDbType.Int ).Value = cam.FillToleranceHigh   ;
                    sqlCommand.Parameters.Add("@FillToleranceLow    ", SqlDbType.Int ).Value = cam.FillToleranceLow    ;
                    sqlCommand.Parameters.Add("@HomographyMatrix    ", SqlDbType.NVarChar, 4000).Value = cam.HomographyMatrix ?? (object)DBNull.Value   ;
                    sqlCommand.Parameters.Add("@ImageIndex          ", SqlDbType.Int ).Value = cam.ImageIndex          ;
                    sqlCommand.Parameters.Add("@NumIterations       ", SqlDbType.Int ).Value = cam.NumIterations       ;
                    sqlCommand.Parameters.Add("@ResultLength        ", SqlDbType.Real ).Value = cam.ResultLength        ;
                    sqlCommand.Parameters.Add("@ResultScale         ", SqlDbType.Real ).Value = cam.ResultScale         ;
                    sqlCommand.Parameters.Add("@ResultWidth         ", SqlDbType.Real ).Value = cam.ResultWidth         ;
                    sqlCommand.Parameters.Add("@SegmentLength       ", SqlDbType.Int ).Value = cam.SegmentLength       ;
                    sqlCommand.Parameters.Add("@SpatialRadius       ", SqlDbType.Int ).Value = cam.SpatialRadius       ;
                    sqlCommand.Parameters.Add("@URL                 ", SqlDbType.NVarChar, 4000).Value = cam.URL ?? (object)DBNull.Value;
                    sqlCommand.Parameters.Add("@ROILeft             ", SqlDbType.Int).Value = cam.ROILeft;
                    sqlCommand.Parameters.Add("@ROITop              ", SqlDbType.Int).Value = cam.ROITop;
                    sqlCommand.Parameters.Add("@ROIRight            ", SqlDbType.Int).Value = cam.ROIRight;
                    sqlCommand.Parameters.Add("@ROIBottom           ", SqlDbType.Int).Value = cam.ROIBottom;
                    SqlParameter sqlParameter = sqlCommand.Parameters.Add("@CameraPK", SqlDbType.Int);
                    sqlParameter.Direction = ParameterDirection.Output ;
                    sqlCommand.ExecuteNonQuery();

                    int nRetID = (int)sqlParameter.Value;
                    cam.SetCameraPK(nRetID);

                    rval = true;
                    Add(cam);
                }
            }

            return rval ;
        }

    }

}
