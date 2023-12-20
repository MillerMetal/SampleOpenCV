using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Data.SqlClient;
using SharpTech.Sql;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace SampleOpenCV.UserControls
{
    /// <summary>
    /// Interaction logic for CameraDataUserControl.xaml
    /// </summary>
    public partial class CameraDataUserControl : UserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool bInCameraChange = false;
        private bool bInWorkStationChange = false;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<CameraData> CameraDataColl2
        {
            get { return Static.CameraDataColl; }
        }

        public ObservableCollection<WorkStationData> WorkStationColl2
        {
            get { return Static.WorkStationColl; }
        }

        // Did the selection change in the CameraIPComboBox?


        public CameraDataUserControl()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void AddCameraClick(object sender, RoutedEventArgs e)
        {
            CameraData curCam = this.CameraIPComboBox.SelectedItem as CameraData;

            // Add a new camera to the database
            try
            {
                using (DBConnection dbConnection = new())
                {
                    // Use the connection to grab some simple data
                    Microsoft.Data.SqlClient.SqlCommand sqlCommand = dbConnection.GetSqlCommand();
                    if (sqlCommand != null)
                    {
                        Static.CameraDataColl.AddNewCam(/*ref curCam, */ref sqlCommand);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
                return ;
            }
        }

        private void DeleteCameraClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult rval;

            rval = MessageBox.Show ( "Are you sure you want to delete the current camera?\r\nThis operation cannot be undone, and the calibration data will need to be recalculated.\r\nIf you are not sure, please press the export button and save\r\nthe calibration values of the current camera before proceeding."
                                    ,"Delete Currently Selected Camera"
                                    ,MessageBoxButton.YesNo);
            if (rval == MessageBoxResult.Yes
                && this.CameraIPComboBox!=null
                && this.CameraIPComboBox.SelectedItem!=null
                && Static.CameraDataColl!=null)

            {
                // OK.  They do want to delete this camera.
                // Delete the camera and select the next to the
                // last camera in the list.
                CameraData? curCam = this.CameraIPComboBox.SelectedItem as CameraData;
                int nSelIndex = this.CameraIPComboBox.SelectedIndex;

                if (curCam != null)
                {
                    // Add a new camera to the database
                    try
                    {
                        using (DBConnection dbConnection = new())
                        {
                            // Use the connection to grab some simple data
                            Microsoft.Data.SqlClient.SqlCommand? sqlCommand = dbConnection.GetSqlCommand();
                            if (sqlCommand != null)
                            {
                                if (Static.CameraDataColl.DeleteCurCam(ref curCam, ref sqlCommand))
                                {
                                    int nCount = Static.CameraDataColl.Count;
                                    // If we succeeded, be sure to select another index
                                    if (nSelIndex < nCount)
                                        this.CameraIPComboBox.SelectedIndex = nSelIndex;
                                    else
                                    if (nCount > 0)
                                        this.CameraIPComboBox.SelectedIndex = nCount - 1;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception: " + ex.Message);
                        return;
                    }
                }
            }
        }

        public void UpdateCheckerCalibrations(string CameraMatrix, string DistortionMatrix)
        {
            CameraData? curCam = this.CameraIPComboBox.SelectedItem as CameraData;
            if (curCam != null)
            {
                curCam.CameraMatrix = CameraMatrix;
                curCam.DistortionMatrix = DistortionMatrix;
            }
        }

        public void UpdateCornerCalibration(string HomographyMatrix)
        {
            CameraData? curCam = this.CameraIPComboBox.SelectedItem as CameraData;
            if (curCam != null)
                curCam.HomographyMatrix = HomographyMatrix;
        }

        public event EventHandler CameraSelectionChanged;
        protected virtual void OnCameraSelectionChanged(EventArgs e)
        {
            CameraSelectionChanged?.Invoke(this, e);
        }

        private void CameraIPComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The selection changed!
            if (bInWorkStationChange==false)
            {
                bool rval = false;
                CameraData? curCam = this.CameraIPComboBox.SelectedItem as CameraData;
                if (curCam != null)
                {
                    // Change the workstation to match the camera
                    // that we just selected, if we can.  If we
                    // can't, make sure the workstation is in an
                    // unselected state by changing it's index
                    // to -1.

                    IEnumerable<WorkStationData> wsd = Static.WorkStationColl.Where(i => i.CameraFK == curCam.CameraPK);
                    int nCnt = wsd.Count();
                    if (nCnt > 0 && wsd != null)
                    {
                        WorkStationData? ws = wsd.ElementAt(0);
                        if (ws != null)
                        {
                            bInCameraChange = true;
                            this.WorkCenterComboBox.SelectedItem = ws;
                            rval = true;
                        }
                    }
                }

                if (!rval)
                    this.WorkCenterComboBox.SelectedIndex = -1;
            }
            bInCameraChange = false;
            OnCameraSelectionChanged(e);
        }

        private void WorkCenterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool rval = false;

            if (bInCameraChange == false)
            {
                bInWorkStationChange= true;

                if (this.WorkCenterComboBox.SelectedIndex >= 0)
                {
                    // If we're changing the workstation, and a camera
                    // is selected, then update the workstation's 
                    // CameraFK to match the currently selected
                    // camera's CameraPK.  Warn the user that the
                    // database is about to be changed
                    MessageBoxResult mbrval;
                    WorkStationData? ws = this.WorkCenterComboBox.SelectedItem as WorkStationData;
                    CameraData? cd = this.CameraIPComboBox.SelectedItem as CameraData;

                    if (ws != null && cd != null)
                    {
                        // What is the original workstation associated with the current camera?
                        IEnumerable<WorkStationData> wsd = Static.WorkStationColl.Where(i => i.CameraFK == cd.CameraPK);
                        WorkStationData? oldws = null;
                        int nCnt = wsd.Count();

                        mbrval = MessageBoxResult.Yes;

                        if (wsd != null && nCnt > 0)
                        {
                            // Get ready to update the old entry's database.
                            oldws = wsd.ElementAt(0);
                            oldws.CameraFK = -1;

                            mbrval = MessageBox.Show("You are about to change the camera associated with the Work Station " +
                                                        oldws.Name + " to " + ws.Name + "\r\n\r\nAre you sure that you want to do this?"
                                                    , "Change Camera WorkStation"
                                                    , MessageBoxButton.YesNo);
                        }

                        if (mbrval == MessageBoxResult.Yes)
                        {

                            // Internally assign the collection's new CameraFK
                            ws.CameraFK = cd.CameraPK;


                            /*
                            // Update the CameraFK in the database entry
                            // associated with this workstation.
                            using (DBConnection dbConnection = new())
                            {
                                // Use the connection to grab some simple data
                                Microsoft.Data.SqlClient.SqlCommand? sqlCommand = dbConnection.GetSqlCommand();
                                if (sqlCommand != null)
                                {
                                    if (oldws != null)
                                    {
                                        sqlCommand.Parameters.Clear();
                                        sqlCommand.CommandText = @"Update Machine
                                                            Set CameraStreamURL=NULL, CameraFK=NULL
                                                            Where Machine.MachinePK = @MachPK";
                                        sqlCommand.Parameters.Add(new SqlParameter("@MachPK", SqlDbType.Int)).Value = oldws.MachinePK;
                                        sqlCommand.ExecuteNonQuery();
                                    }


                                    // We will null out the CameraStreamURL in the 
                                    // Machine table.  The correct URL exists in the
                                    // Camera table.
                                    sqlCommand.Parameters.Clear();
                                    sqlCommand.CommandText = @"Update Machine
                                                            Set CameraStreamURL=null, CameraFK=@CamPK
                                                            Where MachinePK = @MachPK";
                                    sqlCommand.Parameters.Add(new SqlParameter("@CamPK", SqlDbType.Int)).Value = cd.CameraPK;
                                    sqlCommand.Parameters.Add(new SqlParameter("@MachPK", SqlDbType.Int)).Value = ws.MachinePK;
                                    sqlCommand.ExecuteNonQuery();
                                }
                            }*/

                            rval = true;
                        }
                        
                    }
                }
            }
            bInWorkStationChange = false;
        }

        public void SetArucoPath_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();

            dlg.Title = "Pick Aruco Calibration Images Path Folder";
            dlg.IsFolderPicker= true;
            dlg.InitialDirectory = Properties.Settings.Default.ArucoPath;

            dlg.AddToMostRecentlyUsedList = true;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory= Properties.Settings.Default.ArucoPath;
            dlg.EnsureFileExists= true;
            dlg.EnsurePathExists= true;
            dlg.EnsureReadOnly= true;
            dlg.EnsureValidNames= true;
            dlg.Multiselect= true;
            dlg.ShowPlacesList= true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Properties.Settings.Default.ArucoPath = dlg.FileName;
            }
        }

        public void SetCheckerPath_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();

            dlg.Title = "Pick Checker Calibration Images Path Folder";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = Properties.Settings.Default.CheckerPath;

            dlg.AddToMostRecentlyUsedList = true;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = Properties.Settings.Default.CheckerPath;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = true;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = true;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Properties.Settings.Default.CheckerPath = dlg.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void SaveToDatabaseClick(object sender, RoutedEventArgs e)
        {
            // Update the CameraFK in the database entry
            // associated with this workstation.
            using (DBConnection dbConnection = new())
            {
                // Use the connection to grab some simple data
                Microsoft.Data.SqlClient.SqlCommand? sqlCommand = dbConnection.GetSqlCommand();

                foreach (CameraData c in Static.CameraDataColl)
                {
                    c.SaveData(ref sqlCommand);
                }

                foreach (WorkStationData w in Static.WorkStationColl)
                {
                }

            }
        }
    }
}
