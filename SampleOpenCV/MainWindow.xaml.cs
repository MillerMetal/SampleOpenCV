using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Shape;
using Emgu.CV.CvEnum;
using Emgu.CV.Aruco;
using Emgu.CV.Util;
using Emgu.CV.XImgproc;
using Emgu.CV.Cuda;

using SharpTech.Sql;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Configuration;
using PointConverter = System.Drawing.PointConverter;
using Microsoft.VisualBasic;
using System.Windows.Controls;
using System.IO;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;
using Point = System.Drawing.Point;
using Emgu.CV.Features2D;
using System.Windows.Media.Media3D;
using System.Net;
using System.Reflection;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Security.RightsManagement;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Reflection.PortableExecutable;
using System.Data.Common;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Security.Policy;
using SampleOpenCV.UserControls;
using System.Windows.Input;

namespace SampleOpenCV
{
    public static class MatExtension
    {
        public static dynamic GetValue(this Mat mat, int row, int col)
        {
            var value = CreateElement(mat.Depth);
            Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 1);
            return value[0];
        }

        public static void SetValue(this Mat mat, int row, int col, dynamic value)
        {
            var target = CreateElement(mat.Depth, value);
            Marshal.Copy(target, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 1);
        }
        private static dynamic CreateElement(DepthType depthType, dynamic value)
        {
            var element = CreateElement(depthType);
            element[0] = value;
            return element;
        }

        private static dynamic CreateElement(DepthType depthType)
        {
            if (depthType == DepthType.Cv8S)
            {
                return new sbyte[1];
            }
            if (depthType == DepthType.Cv8U)
            {
                return new byte[1];
            }
            if (depthType == DepthType.Cv16S)
            {
                return new short[1];
            }
            if (depthType == DepthType.Cv16U)
            {
                return new ushort[1];
            }
            if (depthType == DepthType.Cv32S)
            {
                return new int[1];
            }
            if (depthType == DepthType.Cv32F)
            {
                return new float[1];
            }
            if (depthType == DepthType.Cv64F)
            {
                return new double[1];
            }
            return new float[1];
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        Matrix<double> cameraMatrix = new Matrix<double>(3, 3);
        Matrix<double> distortionMatrix = new Matrix<double>(5, 1);
        Matrix<double> cameraMatrix2 = new Matrix<double>(3, 3);
        Matrix<double> distortionMatrix2 = new Matrix<double>(5, 1);
        Matrix<double> homographyMatrix = new Matrix<double>(3, 3);
        Matrix<double> invHomographyMatrix = new Matrix<double>(3, 3);
        long nNumCheckersSnapped=0;
        long nNumArucosSnapped=0;

        public MainWindow()
        {
            InitializeComponent();

            DBConnection.SetDefaultConnectionString("Data Source=192.168.75.39;Initial Catalog=MachineInterface;User id=MIETrakOverview;Password=MsFSrf9dBarS;Encrypt=True;TrustServerCertificate=True");
            DBConnection.SetDefaultCommandTimeoutSeconds(10);

            Title = "MainWindow - No File Opened";

            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool m_IsCaptureInProgress;
        private readonly object ScreenshotSyncLock = new object();

        bool isLoaded;
        long nLoadedCnt = 0;
        Dictionary<string, string> WorkstationConnectionStrings = new Dictionary<string, string>();
        Dictionary<string, string> WorkstationNameStrings = new Dictionary<string, string>();
        Dictionary<string, int> CameraIPStrings = new Dictionary<string, int>();
        Dictionary<int, string> CameraURLStrings = new Dictionary<int, string>();
        string strSelectedCameraURL = "";
        string strSelectedWorkstation = "";

        private bool IsCaptureInProgress
        {
            get { lock (ScreenshotSyncLock) return m_IsCaptureInProgress; }
            set { lock (ScreenshotSyncLock) m_IsCaptureInProgress = value; }
        }

        private async void CaptureFrame(object sender, RoutedEventArgs e, int nCaptureType=0)  // 0=checker, 1=aruco, 2=measurment test
        {
            // Don't run the capture operation as it is in progress
            // GDI requires exclusive access to files when writing
            // so we do this one at a time
            if (IsCaptureInProgress)
                return;

            // Immediately set the progress to true.
            IsCaptureInProgress = true;

            // Send the capture to the background so we don't have frames skipping
            // on the UI. This prvents frame jittering.
            var captureTask = Task.Run(async () =>
            {
                string prefix;
                string prefixtype;
                string prefixtime;
                string prefixipaddr;

                prefixtime = prefix = prefixipaddr =
                prefixtype = string.Empty;

                if (nCaptureType == 0)
                {
                    prefix = Properties.Settings.Default.CheckerPath;
                    prefixtype = "Checker";
                }
                else
                if (nCaptureType == 1)
                {
                    // Store with a aruco prefix in the name
                    prefix = Properties.Settings.Default.ArucoPath;
                    prefixtype = "Aruco";
                }
                else
                if (nCaptureType == 2)
                {
                    prefix = Properties.Settings.Default.MeasurePath;
                    prefixtype = "Snapshot";
                }
                else
                {
                    // Store with a measure prefix in the name
                    prefix = Properties.Settings.Default.ArucoPath;
                    prefixtype = "Measure";
                }

                if (strSelectedCameraURL.Contains('@') == false)
                {
                    if (strSelectedCameraURL.ToLower().Contains("snapshot"))
                    {

                    }
                    else
                    {
                        MessageBox.Show("The URL for the currently selected camera must contain a '@'.  Snapshot not taken");
                        return;
                    }
                }
                int nPrefixIpAddrIndex = strSelectedCameraURL.LastIndexOf('@');
                prefixipaddr = strSelectedCameraURL.Substring(nPrefixIpAddrIndex + 1);
                prefixipaddr = prefixipaddr.Replace(".", "-");
                prefixipaddr = "-IPAddr-" + prefixipaddr + "-";


                if (prefix==null || Directory.Exists(prefix) == false)
                {
                    MessageBoxResult rval = 
                    MessageBox.Show("The path " + prefix + " does not exist.  Do you want to select a path?  If yes, please enter a path and press the Snap Checker Button again", "Image Capture Path Does Not Exist", MessageBoxButton.YesNo);
                    if (rval == MessageBoxResult.Yes)
                    {
                        if (nCaptureType == 0)
                            ctlCameraDataCtl.SetCheckerPath_Click(sender, e);
                        else
                        if (nCaptureType == 1)
                            ctlCameraDataCtl.SetArucoPath_Click(sender, e);
                        else
                        if (nCaptureType == 2)
                            ctlCameraDataCtl.SetMeasurePath_Click(sender, e);
                        else
                            MessageBox.Show("Encountered an unexpected capture type request of " + nCaptureType.ToString());
                        return;
                    }
                    return;
                }

                if (prefix!=null && prefix.Length>0 && prefix.ElementAt(prefix.Length- 1) != '\\')
                {
                    prefix += "\\";
                }

                try
                {
                    var bmp = MediaView.Media.CaptureBitmapAsync().GetAwaiter().GetResult();

                    // Save the bitmap to the next snapshot available.  Use
                    // the folder that was stored in the settings folder.
                    // We will use the ip address of the camera along with the
                    // current time, so that we are sure we don't overwrite.  We
                    // will also store the type ofg snapshot if 
                    if (nCaptureType == 0)
                    {
                        nNumArucosSnapped++;
                    }
                    else
                    {
                        nNumCheckersSnapped++;
                    }

                    DateTime dateTime = DateTime.Now;
                    prefixtime = dateTime.ToString("yyyy-MM-dd-HH-mm-ss");

                    bmp?.Save(prefix + prefixtype + prefixipaddr + prefixtime + ".png");

                    // prevent further processing if we did not get a bitmap.
                    //bmp?.Save(App.GetCaptureFilePath("Screenshot", "png"), ImageFormat.Png);
                    //ViewModel.NotificationMessage = "Captured screenshot.";
                    //bmp?.Save("c:\\Users\\G.Norkus\\Desktop\\Goofy.jpg");
                }
                catch (Exception ex)
                {
                    var messageTask = Dispatcher.InvokeAsync(() =>
                    {
                        MessageBox.Show(
                            $"Capturing Video Frame Failed: {ex.GetType()}\r\n{ex.Message}",
                            $"{nameof(MediaElement)} Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK);
                    });
                }
                finally
                {
                    // unlock for further captures.
                    IsCaptureInProgress = false;
                }
            });

            // When done, do the updates of the text boxes
            await captureTask;
            NumArucoImagesSnapped.Text = nNumArucosSnapped.ToString();
            NumCheckerImagesSnapped.Text = nNumCheckersSnapped.ToString();
        }

        async private void childCameraSelectionChanged(object sender, EventArgs e)
        {
            await MediaView.Media.Close();
            MediaView.Visibility = Visibility.Collapsed;
            gdiImage.Visibility = Visibility.Visible;
            MediaView.TextBlk.Text = "";
            MediaView.TextBlk.Visibility = Visibility.Hidden;

            nNumArucosSnapped=0;
            NumArucoImagesSnapped.Text = nNumArucosSnapped.ToString();
            nNumCheckersSnapped=0;
            NumCheckerImagesSnapped.Text = nNumCheckersSnapped.ToString();
        }

    private async void OnLoad(object sender, RoutedEventArgs rea)
        {
            try
            {
                //Interlocked.Increment(ref nLoadedCnt);
                //Debug.WriteLine("Main Window Loaded " + rea.ToString());
                //await RefreshButtonWork_Async(sender, rea);
                //Interlocked.Decrement(ref nLoadedCnt);
                await LoadFreshWorkCenterList();
                MediaView.Visibility = Visibility.Collapsed;
                gdiImage.Visibility = Visibility.Visible;

                this.ctlCameraDataCtl.CameraSelectionChanged += childCameraSelectionChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        public string[] WriteToCsv(byte[] arr11x11, string szvarname, string szname, bool bFlipX=false, bool bFlipY=false)
        {
            int i, j, cnt;
            int a;
            float f;
            string[] tstr = new string[11];
            string[] tstr2 = new string[11];    

            cnt = 0;
            for (i=0; i<11; i++)
            {
                tstr[i] = "{";
                cnt = i * 11;
                if (bFlipX)
                    cnt += 10;

                for (j=0; j<11; j++)
                {
                    a = (int) arr11x11[cnt * 2];
                    a = a - 127;
                    f = (float)a / 127.0F;
                    if (bFlipX)
                        cnt--;
                    else
                        cnt++;
                    tstr[i] += f.ToString("##0.000000") + "F";
                    if (j!=10)
                        tstr[i] += ",";
                }
                tstr[i] += "},";
            }

            if (bFlipY == false)
            {
                for (i = 0; i < 11; i++)
                    tstr2[i] = tstr[i];
            }
            else
            {
                for (i = 0; i < 11; i++)
                    tstr2[10 - i] = tstr[i];
            }

            tstr2[0] = "float[,] " + szvarname + " = { \r\n" + tstr2[0];
            tstr2[10] += "};\r\n\r\n";

            return tstr2;

            //File.WriteAllLines(szname, tstr2);
        }
        public void WriteToCs(byte[] arr11x11, string szname)
        {
        }

        public byte[] FileToByteArray(string fileName)
        {
            byte[] buff = null;
            FileStream fs = new FileStream(fileName,
                                           FileMode.Open,
                                           FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(fileName).Length;
            buff = br.ReadBytes((int)numBytes);
            return buff;
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            //Properties.Settings.Default.Save();
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /*************************************************/
        public static BitmapSource ToBitmapSource(Emgu.CV.Mat image)
        {
            using (System.Drawing.Bitmap source = Emgu.CV.BitmapExtension.ToBitmap(image))
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }

        }

        /*************************************************/


        //private System.Drawing.Image ConvertImage(ImageSource imagesource)
       // {
        //    throw new NotImplementedException();
       // }

        public static BitmapSource ToBitmapSource<TColor, TDepth>(Emgu.CV.Image<TColor, TDepth> image)
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
        {
            using (System.Drawing.Bitmap source = image.AsBitmap())
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }

        // UndistortBigImage(cvimgGrayBefore, cameraMatrix, distortionMatrix);
        public Image<TColor, TDepth> UndistortBigImage<TColor, TDepth>(Image<TColor, TDepth> cvimgBefore, Rectangle? optROI = null )
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
        {
            System.Drawing.Size orgImageSize = new System.Drawing.Size(cvimgBefore.Size.Width, cvimgBefore.Size.Height);

            System.Drawing.Size newImageSize = new System.Drawing.Size(cvimgBefore.Size.Width * 3 / 2, cvimgBefore.Size.Height * 3 / 2);
            Emgu.CV.Image<TColor, TDepth> cvimgBigColor = new Image<TColor, TDepth>(newImageSize.Width, newImageSize.Height);

            cvimgBigColor.ROI = new System.Drawing.Rectangle(0, 0, orgImageSize.Width, orgImageSize.Height);
            cvimgBefore.CopyTo(cvimgBigColor);
            cvimgBigColor.ROI = new System.Drawing.Rectangle(0, 0, newImageSize.Width, newImageSize.Height);

            System.Drawing.Rectangle validROI = new Rectangle();

            Emgu.CV.Mat newCameraMat = CvInvoke.GetOptimalNewCameraMatrix(cameraMatrix.Mat
                                                           , distortionMatrix.Mat
                                                           , orgImageSize
                                                           , 1
                                                           , newImageSize
                                                           , ref validROI);

            Emgu.CV.Image<TColor, TDepth> cvimgUndistorted = new Emgu.CV.Image<TColor, TDepth>(newImageSize.Width, newImageSize.Height);

            // Undistort the image using the stored camera parameters
            CvInvoke.Undistort(cvimgBigColor, cvimgUndistorted, cameraMatrix, distortionMatrix, newCameraMat);

            // Search the four sides for where the black stops
            int nLeft, nRight, nTop, nBottom, nTempY, nTempX;
            Object pixelZeroColor;

            if (typeof(TColor) == typeof(Bgr))
            {
                pixelZeroColor = new Bgr(0, 0, 0);
            }
            else
            if (typeof(TColor) == typeof(Bgra))
            {
                pixelZeroColor = new Bgra(0, 0, 0, 0);
            }
            else
            {
                pixelZeroColor = new Gray(0);
            }

            nTempY = cvimgUndistorted.Height / 2;
            nTempX = cvimgUndistorted.Width / 2;

            for (nLeft = 0; nLeft < cvimgUndistorted.Width; nLeft++)
            {
                if (cvimgUndistorted[nTempY, nLeft].Equals(pixelZeroColor) == false)
                    break;
            }

            for (nRight = cvimgUndistorted.Width - 1; nRight >= 0; nRight--)
            {
                if (cvimgUndistorted[nTempY, nRight].Equals(pixelZeroColor) == false)
                    break;
            }

            for (nTop = 0; nTop < cvimgUndistorted.Height; nTop++)
            {
                if (cvimgUndistorted[nTop, nTempX].Equals(pixelZeroColor) == false)
                    break;
            }

            for (nBottom = cvimgUndistorted.Height - 1; nBottom >= 0; nBottom--)
            {
                if (cvimgUndistorted[nBottom, nTempX].Equals(pixelZeroColor) == false)
                    break;
            }

            nTempY = 0;

            if (optROI != null)
                cvimgUndistorted.ROI = (Rectangle)optROI;
            else
                cvimgUndistorted.ROI = new Rectangle(nLeft, nTop, nRight - nLeft, nBottom - nTop);

            return cvimgUndistorted;
        }


        private void DetectCorners()
        {
            double dScaleFactor = 32.0;

            #region DetectCorners
            OpenFileDialog openPic = new OpenFileDialog();

            // First, open the image captured that has the plates (or paper pieces)
            // and the ArUco corner targets.
            openPic.Multiselect = false;
            openPic.Title = "Open Aruco Image";

            if (cameraMatrix != null)
            {
                if (Properties.Settings.Default.CameraMatrix.Length > 0)
                {
                    /*
                    XmlReader rdr = XmlReader.Create(new StringReader(Properties.Settings.Default.CameraMatrix));
                    if (rdr != null)
                    {
                        object o = (new XmlSerializer(typeof(Matrix<double>))).Deserialize(rdr);
                        if (o != null)
                            cameraMatrix = (Matrix<double>)o;
                    }
                    */

                    Emgu.CV.Mat tmat = new Mat();
                    FileStorage fsr = new FileStorage(Properties.Settings.Default.CameraMatrix, FileStorage.Mode.Read | FileStorage.Mode.Memory);
                    FileNode fn = fsr.GetNode("Camera");
                    fn.ReadMat(tmat);
                    tmat.ConvertTo(cameraMatrix, DepthType.Cv64F);
                }
            }
            if (distortionMatrix != null)
            {
                if (Properties.Settings.Default.DistortionMatrix.Length > 0)
                {
                    /*
                    XmlReader rdr = XmlReader.Create(new StringReader(Properties.Settings.Default.DistortionMatrix));
                    if (rdr != null)
                    {
                        object o = (new XmlSerializer(typeof(Matrix<double>))).Deserialize(rdr);
                        if (o != null)
                            distortionMatrix = (Matrix<double>)o;
                    }
                    */

                    Emgu.CV.Mat tmat = new Mat();
                    FileStorage fsr = new FileStorage(Properties.Settings.Default.DistortionMatrix, FileStorage.Mode.Read | FileStorage.Mode.Memory);
                    FileNode fn = fsr.GetNode("Distortion");
                    fn.ReadMat(tmat);
                    tmat.ConvertTo(distortionMatrix, DepthType.Cv64F);
                }
            }

            if (openPic.ShowDialog() == true)
            {
                string szFileNameOnly = System.IO.Path.GetFileName(openPic.FileName);
                Title = "MainWindow - Calibration Image Loaded - " + szFileNameOnly;

                List<Emgu.CV.Image<Bgr, Byte>> cvimgsColor = new List<Image<Bgr, byte>>();

                // Open the target image
                cvimgsColor.Add(new Image<Bgr, Byte>(openPic.FileName));

                // Return Value allocation
                VectorOfVectorOfPointF myMarkerCorners = new VectorOfVectorOfPointF();
                VectorOfVectorOfPointF myRejects = new VectorOfVectorOfPointF();
                VectorOfInt myMarkerIds = new VectorOfInt();

                // Get the default parameters for the conversion
                //DetectorParameters myDetectorParams = new DetectorParameters();
                //myDetectorParams = DetectorParameters.GetDefault();
                DetectorParameters myDetectorParams = DetectorParameters.GetDefault();

                // Create the dictionary for receiving values
                Dictionary myDict = new Dictionary(Dictionary.PredefinedDictionaryName.DictArucoOriginal);

                // Some image result holders
                Emgu.CV.Image<Bgr, Byte> cvimgColorBefore = new Image<Bgr, Byte>(openPic.FileName);
                Emgu.CV.Image<Bgr, Byte> cvimgColor = new Image<Bgr, Byte>(openPic.FileName);


                Emgu.CV.Image<Gray, byte> cvimgGrayBefore = cvimgColor.Convert<Gray, byte>();
                Emgu.CV.Image<Gray, byte> cvimgGray = cvimgColor.Convert<Gray, byte>();

                // Using the camera calibration data from the checkerboard patterns, and the 
                // distortion array calculated from those images, undistort the images so it's
                // a TRUE camera projection with no induced curves.  We use our own undistort
                // so that we can get the maximum amount of pixels.
                cvimgGray = UndistortBigImage<Gray, Byte>(cvimgGrayBefore);
                cvimgColor = UndistortBigImage<Bgr, Byte>(cvimgColorBefore);

                // Store a gray result image
                /*************************************************/
                gdiImage.Source = ToBitmapSource(cvimgGray);
                //gdiImage.InvalidateVisual();
                gdiGreyImage.Source = ToBitmapSource(cvimgColor);
                //gdiGreyImage.UpdateLayout();

                cvimgGrayBefore = cvimgColor.Convert<Gray, byte>();

                Emgu.CV.Mat imgInputAruco = cvimgGrayBefore.Mat;
                Emgu.CV.Mat imgInputAruco2 = cvimgGrayBefore.Mat;

                Emgu.CV.Aruco.ArucoInvoke.DetectMarkers(imgInputAruco, myDict, myMarkerCorners, myMarkerIds, myDetectorParams, myRejects);

                if (myMarkerCorners.Size == 4)
                {
                    // We're looking for the corners with the following IDs, in the following order:
                    ArucoInvoke.DrawDetectedMarkers(imgInputAruco, myMarkerCorners, myMarkerIds, new MCvScalar(255, 0, 255));
                    //Emgu.CV.Aruco.
                    imgInputAruco.Save(@"c:\users\g.norkus\desktop\camera\temp.png");

                    //ArucoInvoke.DrawDetectedMarkers(imgInputAruco2, myRejects, myMarkerIds, new MCvScalar(0, 255, 0));
                    //imgInputAruco2.Save(@"c:\users\g.norkus\desktop\camera\temp2.png");

                    // Draw the bounding corners...
                    Dictionary<int, int> map = new Dictionary<int, int>();
                    map.Add(430, 3); // these are the corner indeces 0=tr 1=br 2=bl 3=tl
                    map.Add(219, 0);
                    map.Add(338, 1);
                    map.Add(908, 2);

                    Dictionary<int, PointF> mappts = new Dictionary<int, PointF>();

                    // The dimensions must not include the white border, which is 1/2"
                    CameraData? cd = this.ctlCameraDataCtl.CameraIPComboBox.SelectedItem as CameraData;

                    if (cd != null)
                    {
                        Properties.Settings.Default.ROILeft = cd.ROILeft = cvimgGray.ROI.Left;
                        Properties.Settings.Default.ROITop = cd.ROITop = cvimgGray.ROI.Top;
                        Properties.Settings.Default.ROIRight = cd.ROIRight = cvimgGray.ROI.Right;
                        Properties.Settings.Default.ROIBottom = cd.ROIBottom = cvimgGray.ROI.Bottom;
                    }
                    else
                    {
                        MessageBox.Show("The calibration could not be applied because a camera has not been selected");
                        return;
                    }

                    //                    float dLength = 123.5F;      // 134.25" - 1"
                    //                  float dLength2 = 123.5F;        // 131" - 1"
                    //                float dWidth = 64.5F;      //  84-15/16" - 1"

                    float dLength = (float)cd.ResultLength;      // 134.25" - 1"
                    float dLength2 = (float)cd.ResultLength;        // 131" - 1"
                    float dWidth = (float)cd.ResultWidth;      //  84-15/16" - 1"

                    //mappts.Add(430, new PointF(0.0F, 0.0F));
                    //mappts.Add(219, new PointF(dLength * dScaleFactor, 0.0F));
                    //mappts.Add(338, new PointF(dLength2 * dScaleFactor, dWidth * dScaleFactor));
                    //mappts.Add(908, new PointF(0.0F, dWidth * dScaleFactor));

                    mappts.Add(430, new PointF((float)(cd.BorderLeft*dScaleFactor), (float)(cd.BorderTop*dScaleFactor)));
                    mappts.Add(219, new PointF((float)( (dLength+cd.BorderLeft) * dScaleFactor), (float)(cd.BorderTop * dScaleFactor)));
                    mappts.Add(338, new PointF((float)( (dLength2+cd.BorderLeft) * dScaleFactor), (float)((dWidth+cd.BorderTop) * dScaleFactor)));
                    mappts.Add(908, new PointF((float)(cd.BorderLeft * dScaleFactor), (float)((dWidth + cd.BorderTop) * dScaleFactor)));



                    LineSegment2DF line;
                    line = new LineSegment2DF(myMarkerCorners[2][map[myMarkerIds[2]]], myMarkerCorners[1][map[myMarkerIds[1]]]);
                    cvimgColor.Draw(line, new Bgr(0, 0.0, 255.0), 1);
                    line = new LineSegment2DF(myMarkerCorners[3][map[myMarkerIds[3]]], myMarkerCorners[0][map[myMarkerIds[0]]]);
                    cvimgColor.Draw(line, new Bgr(0, 255.0, 0.0), 1);
                    line = new LineSegment2DF(myMarkerCorners[1][map[myMarkerIds[1]]], myMarkerCorners[3][map[myMarkerIds[3]]]);
                    cvimgColor.Draw(line, new Bgr(255.0, 0.0, 0.0), 1);
                    line = new LineSegment2DF(myMarkerCorners[2][map[myMarkerIds[2]]], myMarkerCorners[0][map[myMarkerIds[0]]]);
                    cvimgColor.Draw(line, new Bgr(0, 255.0, 255.0), 1);

                    cvimgColor.Save(@"c:\users\g.norkus\desktop\camera\temp3.png");

                    PointF[] srcs = new PointF[4];
                    srcs[0] = myMarkerCorners[0][map[myMarkerIds[0]]];
                    srcs[1] = myMarkerCorners[1][map[myMarkerIds[1]]];
                    srcs[2] = myMarkerCorners[2][map[myMarkerIds[2]]];
                    srcs[3] = myMarkerCorners[3][map[myMarkerIds[3]]];

                    PointF[] dsts = new PointF[4];
                    dsts[0] = mappts[myMarkerIds[0]];
                    dsts[1] = mappts[myMarkerIds[1]];
                    dsts[2] = mappts[myMarkerIds[2]];
                    dsts[3] = mappts[myMarkerIds[3]];

                    Emgu.CV.Mat homog = CvInvoke.FindHomography(srcs, dsts);
                    homographyMatrix = new Matrix<double>(homog.Rows, homog.Cols);
                    homog.CopyTo(homographyMatrix);
                    invHomographyMatrix = new Matrix<double>(3, 3);
                    CvInvoke.Invert(homographyMatrix, invHomographyMatrix, DecompMethod.LU);
                    //CameraData? cd = this.ctlCameraDataCtl.CameraIPComboBox.SelectedItem as CameraData;

                    if (homographyMatrix != null)
                    {
                        // Store the intrinsics in the user settings
                        FileStorage fs = new FileStorage(".xml", FileStorage.Mode.Write | FileStorage.Mode.Memory);
                        fs.Write(homographyMatrix.Mat, "Homography");
                        Properties.Settings.Default.HomographyMatrix = fs.ReleaseAndGetString();

                        //Properties.Settings.Default.HomographyMatrix = sb.ToString();
                        cd.HomographyMatrix = Properties.Settings.Default.HomographyMatrix;
                        cd.CameraMatrix = Properties.Settings.Default.CameraMatrix;
                        cd.DistortionMatrix = Properties.Settings.Default.DistortionMatrix;
                    }

                    Properties.Settings.Default.ResultScale = dScaleFactor;
                    Properties.Settings.Default.ResultLength = dLength;
                    Properties.Settings.Default.ResultWidth = dWidth;
                    Properties.Settings.Default.Save();

                    Emgu.CV.Image<Gray, Byte> cvimgDewarped = 
                        cvimgGray.WarpPerspective<double>( homographyMatrix
                                                         , (int)((dLength+cd.BorderLeft+cd.BorderRight) * dScaleFactor)
                                                         , (int)((dWidth+cd.BorderTop+cd.BorderBottom) * dScaleFactor)
                                                         , Inter.Cubic, Warp.Default, BorderType.Default, new Gray(0));

                    /*************************************************/
                    gdiGreyImage.Source = ToBitmapSource(cvimgDewarped);
                    cvimgDewarped.Save(@"c:\users\g.norkus\desktop\camera\temp4.png");
                }
                else
                if (myMarkerCorners.Size > 4)
                {
                    // We're looking for the corners with the following IDs, in the following order:
                    ArucoInvoke.DrawDetectedMarkers(imgInputAruco, myMarkerCorners, myMarkerIds, new MCvScalar(255, 0, 255));

                    /*************************************************/
                    gdiGreyImage.Source = ToBitmapSource(imgInputAruco);
                }
                else
                {
                    myMarkerIds = new VectorOfInt(myRejects.Size);
                    //Emgu.CV.Aruco.ArucoInvoke.DetectMarkers(imgInputAruco, myDict, myMarkerCorners, myMarkerIds, myDetectorParams, myRejects);
                    ArucoInvoke.DrawDetectedMarkers(imgInputAruco, myRejects, myMarkerIds, new MCvScalar(255, 0, 255));
                    imgInputAruco.Save(@"c:\users\g.norkus\desktop\camera\tempfail.png");
                }
                #endregion
            }
        }

        private List<MCvPoint3D32f> CreateObjectPoints(System.Drawing.Size sz, float w = 1.0f, float h = 1.0f)
        {
            float x, y;

            var chessboard = new List<MCvPoint3D32f>();

            for (y = 0; y < sz.Height; y++)
            {
                for (x = 0; x < sz.Width; x++)
                {
                    chessboard.Add(new MCvPoint3D32f(x * w, y * h, 0));
                }
            }

            return chessboard;
        }

        /*
        private Int64 SumData(in Mat img)
        {
            Int64 rval = 0;
            long i;
            long nSize = 0;

            if (img.IsContinuous) 
            {
                nSize = img.Size.Width * img.Size.Height * img.ElementSize;
            }

            if (nSize > 0) 
            {
                unsafe
                {
                    byte* imgPtr = (byte *)img.DataPointer.ToPointer();

                    for (i = 0; i < nSize; i++)
                    {
                        rval += *imgPtr;
                        imgPtr++;
                    }
                }
            }

            return rval;
        }*/

        private void LoadCalibrationImages()
        {
            int i,j;
            List<Emgu.CV.Image<Bgr, Byte>> cvimgsColor = new List<Image<Bgr, byte>>();
            System.Drawing.Size patternSize = new System.Drawing.Size(9, 7);
            Mat[] rotationVectors;
            Mat[] translationVectors;
            Boolean bShow = false;
            Int64 sum;
            string outstr;

            MCvPoint3D32f[][] _cornersObjectList;
            PointF[][] _cornersPointsList;
            VectorOfPointF[] _cornersPointsVec;
            bool bFound;
            double error = 0.0;
            float sqW = 1.0f;
            float sqH = 0.997f;

            OpenFileDialog openPic = new OpenFileDialog();

            openPic.Multiselect = true;
            openPic.Title = "Open Calibration Images - Select Multiple Frames";

            if (openPic.ShowDialog() == true)
            {


                // Open all of the calibration images
                i = 0;
                foreach (String filename in openPic.FileNames)
                {
                    Title = "MainWindow - Loading Image " + i.ToString() + " of " + openPic.FileNames.Count();
                    cvimgsColor.Add(new Image<Bgr, Byte>(filename));
                    i++;
                }

                #region Initialize Variable Arrays
                _cornersPointsVec = new VectorOfPointF[cvimgsColor.Count];
                _cornersObjectList = new MCvPoint3D32f[cvimgsColor.Count][];
                _cornersPointsList = new PointF[cvimgsColor.Count][];
                #endregion

                outstr = "";

                for (i = 0; i < cvimgsColor.Count; i++)
                {
                    Title = "MainWindow - PreProcessing Image " + i.ToString() + " of " + cvimgsColor.Count;
                    Debug.WriteLine(Title);

                    #region First, convert the bitmap to gray

                    //sum = SumData(cvimgsColor[i].Mat);

                    Emgu.CV.Image<Gray, byte> cvimgGray = cvimgsColor[i].Convert<Gray, byte>();
                    //sum = SumData(cvimgGray.Mat);

                    //Emgu.CV.Image<Gray, byte> cvimgAdaptThresh = cvimgGray.ThresholdAdaptive( new Gray(255)
                    //                                                                      , Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC
                    //                                                                    , Emgu.CV.CvEnum.ThresholdType.Binary
                    //                                                                  , 31
                    //                                                                , new Gray(-20));
                    //gdiGreyImage.Source = ToBitmapSource<Gray, Byte>(cvimgAdaptThresh);
                    Emgu.CV.Image<Gray, byte> cvimgAdaptThresh = cvimgGray.ThresholdBinary(new Gray(220), new Gray(255));
                    //sum = SumData(cvimgAdaptThresh.Mat);

                    Emgu.CV.Image<Gray, byte> cvimgErode = cvimgAdaptThresh.Erode(1);
                    //sum = SumData(cvimgErode.Mat);

                    /*************************************************/
                    gdiGreyImage.Source = ToBitmapSource<Gray, Byte>(cvimgErode);
                    #endregion


                    #region Next, Find the chess board corners
                    _cornersPointsVec[i] = new VectorOfPointF();
                    bFound = CvInvoke.FindChessboardCorners(cvimgGray, patternSize, _cornersPointsVec[i]);
                    if (bFound)
                    {
                        //for (j=0; j < _cornersPointsVec[i].Size; j++)
                        //{
                        //    outstr += _cornersPointsVec[i][j].X.ToString("F7") + "," + _cornersPointsVec[i][j].Y.ToString("F7") + " ";
                       // }
                        //outstr += "\n";
                    }
                    else
                    {
                        MessageBox.Show("Couldn't find corners for " + openPic.FileNames[i]);
                    }
                    #endregion

                    #region Draw and display the corners on a color copy of the image
                    if (bFound)
                    {
                        CvInvoke.CornerSubPix(cvimgGray, _cornersPointsVec[i], new System.Drawing.Size(11, 11), new System.Drawing.Size(-1, -1), new MCvTermCriteria(30, 0.1));
                        //for (j = 0; j < _cornersPointsVec[i].Size; j++)
                        //{
                        //    outstr += _cornersPointsVec[i][j].X.ToString("F7") + "," + _cornersPointsVec[i][j].Y.ToString("F7") + " ";
                        //}
                        //outstr += "\n";

                        Emgu.CV.Image<Bgr, Byte> cvimgColorCopy = cvimgsColor[i].Copy();

                        _cornersObjectList[i] = CreateObjectPoints(patternSize, sqW, sqH).ToArray();
                        _cornersPointsList[i] = _cornersPointsVec[i].ToArray();

                        // IMPORTANT NOTE!!!  DrawChessboardCorners requires that the cornersPointsVec
                        // be a PointF (float), not an int or a double.
                        CvInvoke.DrawChessboardCorners(cvimgColorCopy, patternSize, _cornersPointsVec[i], bFound);

                        /*************************************************/
                        gdiGreyImage.Source = ToBitmapSource<Bgr, Byte>(cvimgColorCopy);
                    }
                    else
                    {
                    }
                    #endregion

                    /*************************************************/
                    gdiImage.Source = ToBitmapSource<Bgr, Byte>(cvimgsColor[i]);

                    if (bShow)
                    {
                        if (MessageBox.Show("Image " + i.ToString() + "  Press OK to continue.") != MessageBoxResult.OK)
                            bShow = false;
                    }



              
                }

                #region Calibrate the camera and store the intrinsics                    

                error = CvInvoke.CalibrateCamera(_cornersObjectList
                                         , _cornersPointsList
                                         , cvimgsColor[0].Size
                                         , cameraMatrix
                                         , distortionMatrix
                                         , CalibType.Default
                                         , new MCvTermCriteria(250, 0.001)
                                         , out rotationVectors
                                         , out translationVectors);

                #endregion



                Rectangle rc = new Rectangle();
                Mat rval;

                rval = CvInvoke.GetOptimalNewCameraMatrix(cameraMatrix, distortionMatrix, cvimgsColor[0].Size, 1, cvimgsColor[0].Size, ref rc);
                bool bShowMsg = true;

                if (cameraMatrix != null)
                {
                    // Store the intrinsics in the user settings
                    /*
                    StringBuilder sb = new StringBuilder();
                    (new XmlSerializer(typeof(Matrix<double>))).Serialize(new StringWriter(sb), cameraMatrix);
                    Properties.Settings.Default.CameraMatrix = sb.ToString();
                    */

                    FileStorage fs = new FileStorage(".xml", FileStorage.Mode.Write | FileStorage.Mode.Memory);
                    fs.Write(cameraMatrix.Mat, "Camera");
                    Properties.Settings.Default.CameraMatrix = fs.ReleaseAndGetString();
                }

                if (distortionMatrix != null)
                {
                    // Store the intrinsics in the user settings
                    /*
                    StringBuilder sb = new StringBuilder();
                    (new XmlSerializer(typeof(Matrix<double>))).Serialize(new StringWriter(sb), distortionMatrix);
                    Properties.Settings.Default.DistortionMatrix = sb.ToString();
                    */
                    FileStorage fs = new FileStorage(".xml", FileStorage.Mode.Write | FileStorage.Mode.Memory);
                    fs.Write(distortionMatrix.Mat, "Distortion");
                    Properties.Settings.Default.DistortionMatrix = fs.ReleaseAndGetString();
                }

                Properties.Settings.Default.Save();

                // Now, set the values in the currently selected camera

                ctlCameraDataCtl.UpdateCheckerCalibrations( Properties.Settings.Default.CameraMatrix
                                                          , Properties.Settings.Default.DistortionMatrix);


                for (i = 0; i < cvimgsColor.Count; i++)
                {
                    Title = "MainWindow - Processing Image " + i.ToString() + " of " + cvimgsColor.Count;

                    Emgu.CV.Image<Bgr, Byte> cvimgColorCopy = cvimgsColor[i].Copy();
                    Emgu.CV.Image<Bgr, Byte> cvimgColorResult = cvimgsColor[i].Copy();

                    CvInvoke.Undistort(cvimgColorCopy, cvimgColorResult, cameraMatrix, distortionMatrix);
                    cvimgColorResult.Save(@"c:\users\g.norkus\desktop\camera\templist" + i.ToString() + ".png");

                    /*************************************************/
                    gdiImage.Source = ToBitmapSource<Bgr, Byte>(cvimgColorResult);

                    CvInvoke.DrawChessboardCorners(cvimgColorCopy, patternSize, _cornersPointsVec[i], true);
                    /*************************************************/
                    gdiGreyImage.Source = ToBitmapSource<Bgr, Byte>(cvimgColorCopy);


                    if (bShowMsg)
                    {
                        MessageBoxResult res;

                        if (bShowMsg)
                        {
                            res = MessageBox.Show("Displaying Undistorted Image "
                                                    + i.ToString()
                                                    + "  Press OK to continue, or Cancel (Esc) to stop showing this message."
                                                , "Progress"
                                                , MessageBoxButton.OKCancel
                                                , MessageBoxImage.Warning
                                                 );

                            if (res == MessageBoxResult.Cancel)
                                bShowMsg = false;
                        }
                    }
                }
            }

            Title = "MainWindow - Finished Loading Checker Calibration Images";
        }



        private void SaveCornerDetectImage()
        {
            #region SaveCorners
            SaveFileDialog savePic = new SaveFileDialog();

            savePic.Title = "Save Detected Corner Image";
            savePic.Filter = "Png File|*.png";
            float dScaleFactor = 32.0F;


            if (savePic.ShowDialog() == true)
            {
                var ext = Path.GetExtension(savePic.FileName);
                var name = Path.GetFileNameWithoutExtension(savePic.FileName);
                var dir = Path.GetDirectoryName(savePic.FileName);

                System.Windows.Media.ImageSource src = gdiImage.Source;
                BitmapSource bmpSource = src as BitmapSource;
                System.Windows.Media.Imaging.PngBitmapEncoder pngBitmapEncoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                System.IO.FileStream stream = new System.IO.FileStream(dir + "\\" + name + ext, FileMode.Create);
                pngBitmapEncoder.Interlace = PngInterlaceOption.On;
                pngBitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmpSource));
                pngBitmapEncoder.Save(stream);
                stream.Flush();
                stream.Close();

                src = gdiGreyImage.Source;
                bmpSource = src as BitmapSource;
                pngBitmapEncoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                stream = new System.IO.FileStream(dir + "\\" + name + "2" + ext, FileMode.Create);
                pngBitmapEncoder.Interlace = PngInterlaceOption.On;
                pngBitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmpSource));
                pngBitmapEncoder.Save(stream);
                stream.Flush();
                stream.Close();

            }

            #endregion
        }


        // helper function:
        // finds a cosine of angle between vectors
        // from pt0->pt1 and from pt0->pt2
        static double angle(System.Drawing.Point pt1, System.Drawing.Point pt2, System.Drawing.Point pt0)
        {
            double dx1 = pt1.X - pt0.X;
            double dy1 = pt1.Y - pt0.Y;
            double dx2 = pt2.X - pt0.X;
            double dy2 = pt2.Y - pt0.Y;
            return (dx1 * dx2 + dy1 * dy2) / Math.Sqrt((dx1 * dx1 + dy1 * dy1) * (dx2 * dx2 + dy2 * dy2) + 1e-10);
        }

        int thresh = 50;
        int N = 11;


        // This uses the existing homography matrix to reverse the translation of line segments back
        // into the view.  This will later be used for 3D transformation if we have correct pose estimates.
        //private LineSegment2D[] TransformHomographyLines(LineSegment2D[] inputLines, bool bReverse )
        //{
        //Mat tMat = homographyMatrix.Mat();

        //CvInvoke.Invert(homographyMatrix, tMat, DecompMethod.Normal);
        //LineSegment2D[] outputLines = new LineSegment2D[inputLines.Length]; 
        //}
        private string MatrixToString<TDepth>(Matrix<TDepth> inputArr, string szLabel)
            where TDepth : new()
        {
            TDepth nval;
            long x, y;
            string rval = "";

            rval += szLabel + "\n[";

            for (x=0; x<inputArr.Cols; x++)
            {
                if (x > 0) { rval += "\n"; }

                for (y=0; y<inputArr.Rows; y++)
                {
                    if (y>0) 
                    { 
                        rval += ", "; 
                    }

                    rval += inputArr.Data[x, y].ToString();
                }
            }

            rval += "\n]";
            return rval;
        }

        private void CheckMatricesLoaded()
        {
            string tstr;

            if (homographyMatrix != null)
            {
                /*
                XmlReader rdr = XmlReader.Create(new StringReader(Properties.Settings.Default.HomographyMatrix));
                if (rdr != null)
                {
                    object o = (new XmlSerializer(typeof(Matrix<double>))).Deserialize(rdr);
                    if (o != null)
                    {
                        homographyMatrix = (Matrix<double>)o;

                        invHomographyMatrix = new Matrix<double>(3, 3);
                        CvInvoke.Invert(homographyMatrix, invHomographyMatrix, DecompMethod.LU);
                    }
                }*/

                Emgu.CV.Mat tmat = new Mat();
                FileStorage fsr = new FileStorage(Properties.Settings.Default.HomographyMatrix, FileStorage.Mode.Read | FileStorage.Mode.Memory);
                FileNode fn = fsr.GetNode("Homography");
                fn.ReadMat(tmat);
                tmat.ConvertTo(homographyMatrix, DepthType.Cv64F);
                invHomographyMatrix = new Matrix<double>(3, 3);
                CvInvoke.Invert(homographyMatrix, invHomographyMatrix, DecompMethod.LU);

                /*
                FileStorage fs = new FileStorage(".xml", FileStorage.Mode.WriteBase64 | FileStorage.Mode.Memory);
                fs.Write(homographyMatrix.Mat, "Homography");
                Properties.Settings.Default.HomographyMatrix = fs.ReleaseAndGetString();
                */
            }
            else
            {
                //tstr = MatrixToString<double>(homographyMatrix, "Homography Matrix");
            }

            if (cameraMatrix != null)
            {
                /*
                XmlReader rdr = XmlReader.Create(new StringReader(Properties.Settings.Default.CameraMatrix));
                if (rdr != null)
                {
                    object o = (new XmlSerializer(typeof(Matrix<double>))).Deserialize(rdr);
                    if (o != null)
                        cameraMatrix = (Matrix<double>)o;
                }*/

                Emgu.CV.Mat tmat = new Mat();
                FileStorage fsr = new FileStorage(Properties.Settings.Default.CameraMatrix, FileStorage.Mode.Read | FileStorage.Mode.Memory);
                FileNode fn = fsr.GetNode("Camera");
                fn.ReadMat(tmat);
                tmat.ConvertTo(cameraMatrix, DepthType.Cv64F);

                /*
                FileStorage fs = new FileStorage(".xml", FileStorage.Mode.WriteBase64 | FileStorage.Mode.Memory);
                fs.Write(cameraMatrix.Mat, "Camera");
                Properties.Settings.Default.CameraMatrix = fs.ReleaseAndGetString();
                */
            }
            else
            {
                //tstr = MatrixToString<double>(cameraMatrix, "Camera Matrix");
            }

            if (distortionMatrix != null)
            {
                /*
                XmlReader rdr = XmlReader.Create(new StringReader(Properties.Settings.Default.DistortionMatrix));
                if (rdr != null)
                {
                    object o = (new XmlSerializer(typeof(Matrix<double>))).Deserialize(rdr);
                    if (o != null)
                        distortionMatrix = (Matrix<double>)o;
                }
                */

                
                Emgu.CV.Mat tmat = new Mat();
                FileStorage fsr = new FileStorage(Properties.Settings.Default.DistortionMatrix, FileStorage.Mode.Read | FileStorage.Mode.Memory);
                FileNode fn = fsr.GetNode("Distortion");
                fn.ReadMat(tmat);
                tmat.ConvertTo(distortionMatrix, DepthType.Cv64F);
                

                /*
                FileStorage fs = new FileStorage(".xml", FileStorage.Mode.WriteBase64 | FileStorage.Mode.Memory);
                fs.Write(distortionMatrix.Mat, "Distortion");
                Properties.Settings.Default.DistortionMatrix= fs.ReleaseAndGetString();
                */
            }
            else
            {
                //tstr = MatrixToString<double>(distortionMatrix, "Distortion Matrix");
            }
        }

        private Image<Bgr,Byte> KMeansAttempt(Image<Bgr, Byte> byteSrc, int clusterCount, int attempts)
        {
            Image<Bgr, float> src = byteSrc.Convert<Bgr, float>();
            Matrix<float> samples = new Matrix<float>(src.Rows * src.Cols, 1, 3);
            Matrix<int> finalClusters = new Matrix<int>(src.Rows * src.Cols, 1);

            for (int y = 0; y<src.Rows; y++)
            {
                for (int x = 0; x<src.Cols; x++)
                {
                    samples.Data[y + x * src.Rows, 0] = (float) src[y, x].Blue;
                    samples.Data[y + x * src.Rows, 1] = (float) src[y, x].Green;
                    samples.Data[y + x * src.Rows, 2] = (float) src[y, x].Red;
                }
            }

            MCvTermCriteria term = new MCvTermCriteria(10000, 0.0001);
            term.Type = TermCritType.Iter | TermCritType.Eps;

            Matrix<Single> centers = new Matrix<Single>(clusterCount, samples.Cols, 3);
            //CvInvoke.cvKMeans2(samples, clusterCount, finalClusters, term, attempts, IntPtr.Zero, KMeansInitType.PPCenters, centers, IntPtr.Zero);
            CvInvoke.Kmeans(samples, clusterCount, finalClusters, term, attempts, KMeansInitType.PPCenters, centers);

            Image<Bgr, Byte> new_image = new Image<Bgr, Byte>(src.Size);

            for (int y = 0; y < src.Rows; y++)
            {
                for (int x = 0; x < src.Cols; x++)
                {
                    int cluster_idx = finalClusters[y + x * src.Rows, 0];
                    MCvScalar sca1 = CvInvoke.cvGet2D(centers, cluster_idx, 0);
                    Bgr color = new Bgr(sca1.V0, sca1.V1, sca1.V2);

                    PointF p = new PointF(x, y);
                    new_image.Draw(new CircleF(p, 1.0f), color, 1);
                }
            }
            return new_image;
        }

        PointF[] GetDefaultBorders()
        {
            // The default borders are in the order of TopLeft, TopRight, BottomRight, BottomLeft
            // This is so a single polyline command can be used to draw the border
            CameraData? cd = this.ctlCameraDataCtl.CameraIPComboBox.SelectedItem as CameraData;
            float dBorderTop = (float)Properties.Settings.Default.BorderTop;
            float dBorderBottom = (float)Properties.Settings.Default.BorderBottom;
            float dBorderRight = (float)Properties.Settings.Default.BorderRight;
            float dBorderLeft = (float)Properties.Settings.Default.BorderLeft;
            float dScaleFactor = (float)Properties.Settings.Default.ResultScale;
            float dLength = (float)Properties.Settings.Default.ResultLength;
            float dWidth = (float)Properties.Settings.Default.ResultWidth;

            if (cd != null)
            {
                dBorderTop = (float)cd.BorderTop;
                dBorderBottom = (float)cd.BorderBottom;
                dBorderRight = (float)cd.BorderRight;
                dBorderLeft = (float)cd.BorderLeft;
                dScaleFactor = (float)cd.ResultScale;
                dLength = (float)cd.ResultLength;
                dWidth = (float)cd.ResultWidth;
            }


            //TopLeft
//            PointF[] defaultBorders = new PointF[] { new PointF { X = dBorderLeft * dScaleFactor, Y = dBorderTop * dScaleFactor }
  //                                                 , new PointF { X = (dLength-dBorderRight) * dScaleFactor, Y = dBorderTop * dScaleFactor }
    //                                               , new PointF { X = (dLength-dBorderRight) * dScaleFactor, Y = (dWidth-dBorderBottom) * dScaleFactor }
      //                                             , new PointF { X = dBorderLeft * dScaleFactor, Y = (dWidth-dBorderBottom) * dScaleFactor } };
            PointF[] defaultBorders = new PointF[] { new PointF { X = 0.0F
                                                                , Y = 0.0F }
                                                   , new PointF { X = (dLength+dBorderLeft+dBorderRight) * dScaleFactor
                                                                , Y = 0.0F}
                                                   , new PointF { X = (dLength+dBorderLeft+dBorderRight) * dScaleFactor
                                                                , Y = (dWidth+dBorderTop+dBorderBottom) * dScaleFactor }
                                                   , new PointF { X = 0.0F
                                                                , Y = (dWidth+dBorderTop+dBorderBottom) * dScaleFactor } };

            return defaultBorders;
        }


        VectorOfVectorOfPointF vecRackPolys = new VectorOfVectorOfPointF();
        VectorOfVectorOfPoint vecMaskPolys = new VectorOfVectorOfPoint();
        Mat matFillPoints ;

        void GenerateDetectMaskPolys(System.Drawing.Size sz)
        {
            PointF[] dstBorders = GetDefaultBorders();   // TopL, TopR, BotR, BotL
            float dScaleFactor = (float)Properties.Settings.Default.ResultScale;
            float x, y, x2, y2;
            float xinc, yinc;
            ushort nPolyCnt;
            VectorOfPoint tpoly = new VectorOfPoint(4);
            VectorOfPointF srcPoly = new VectorOfPointF(4);
            Point[] destPoly = new Point[4];
            PointF[] spoly = new PointF[4];
            string tstr,mstr;
            MemoryStream ms;
            VectorOfPointF vpt;

            //tstr = MatrixToString<double>(cameraMatrix, "Camera Matrix");
            //tstr = MatrixToString<double>(homographyMatrix, "Homography Matrix");
            //tstr = MatrixToString<double>(distortionMatrix, "Distortion Matrix");


            vecRackPolys.Clear();
            vecMaskPolys.Clear();

            xinc = 3.0F;  // increment of 2.5"
            yinc = 1.5F;  // increment of 2.5"

            tstr = "";
            mstr = "";

            nPolyCnt = 1;

            // Figure out how many neighbors connected points
            // have.  We use the flood fill method to join points.  
            // Of course, this is not optimized for video purposes.
            int tw = (int)Math.Floor(dstBorders[1].X - dstBorders[0].X);
            int txinc = (int)Math.Floor(dScaleFactor * xinc);
            int w = tw / txinc + (((tw % txinc)>0) ? 1 : 0);

            int th = (int)Math.Floor(dstBorders[3].Y - dstBorders[0].Y);
            int tyinc = (int)Math.Floor(dScaleFactor * yinc);
            int h = th / tyinc + (((th % tyinc) > 0) ? 1 : 0);

            matFillPoints = new Mat(h,w,DepthType.Cv32F,1);

            // Loop around to get the polygon borders.
            for (y = dstBorders[0].Y; y < dstBorders[3].Y; y += dScaleFactor * yinc)
            {
                for (x = dstBorders[0].X; x < dstBorders[1].X; x += dScaleFactor * xinc)
                {
                    x2 = x + dScaleFactor * xinc;
                    if (x2 > dstBorders[1].X)
                        x2 = dstBorders[1].X;

                    y2 = y + dScaleFactor * yinc;
                    if (y2 > dstBorders[3].Y)
                        y2 = dstBorders[3].Y;

                    spoly[0].X = x; spoly[1].X = x2-1; spoly[2].X = x2-1; spoly[3].X = x;
                    spoly[0].Y = y; spoly[1].Y = y;    spoly[2].Y = y2-1; spoly[3].Y = y2-1;

                    vpt = new VectorOfPointF(spoly);

                    //tstr = vpt.ToString();
                    
                    vecRackPolys.Push(vpt);

                    tstr += "[" + spoly[0].X + ", " + spoly[0].Y + ";  ";
                    tstr += spoly[1].X + ", " + spoly[1].Y + ";  ";
                    tstr += spoly[2].X + ", " + spoly[2].Y + ";  ";
                    tstr += spoly[3].X + ", " + spoly[3].Y + "]\n";

                    // Transform and convert to ints
                    PointF[] dpoly = CvInvoke.PerspectiveTransform(spoly, invHomographyMatrix);
                    for (int i = 0; i < 4; i++)
                        destPoly[i] = Point.Round(dpoly[i]);

                    mstr += "[" + destPoly[0].X + ", " + destPoly[0].Y + ";  ";
                    mstr += destPoly[1].X + ", " + destPoly[1].Y + ";  ";
                    mstr += destPoly[2].X + ", " + destPoly[2].Y + ";  ";
                    mstr += destPoly[3].X + ", " + destPoly[3].Y + "]\n";

                    vecMaskPolys.Push(new VectorOfPoint(destPoly));
                }
            }
        }

        // This method draws a series of parallelograms that represent hit test regions on the 
        // metal rack.  Usually, the metal rack or some noise will cause a hit in one of the
        // parallelograms.  If a parallelogram is hit, the subsequent filling mask will not
        // have the hit drawn.  This will leave us with just a few large areas that will likely
        // be close to the correct measure of the piece of metal being measured.  Some tolerance
        // of rotation can be expected, but not extreme angles.  Pieces of metal below a 
        // certain size threshold will be ignored.
        Emgu.CV.Image<Gray,ushort> DrawRectDetectMask<TColor, TDepth>(ref Emgu.CV.Image<TColor, TDepth> img)
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
        {
            Emgu.CV.Image<Gray, ushort> rval = new Image<Gray, ushort>(img.Size);

            PointF[] dstBorders = GetDefaultBorders();   // TopL, TopR, BotR, BotL
            float dScaleFactor = (float)Properties.Settings.Default.ResultScale;
            ushort nPolyCnt;

            nPolyCnt = 1;

            for (int i = 0; i < vecMaskPolys.Size; i++)
                rval.FillConvexPoly(vecMaskPolys[i].ToArray(), new Gray(nPolyCnt++));

            return rval;
        }

        Emgu.CV.Image<Bgr, Byte> DrawRectDetectMaskPolys<TColor, TDepth>(ref Emgu.CV.Image<TColor, TDepth> img)
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
        {
            Emgu.CV.Image<Bgr, Byte> rval = new Image<Bgr, Byte>(img.Size);

            PointF[] dstBorders = GetDefaultBorders();   // TopL, TopR, BotR, BotL
            float dScaleFactor = (float)Properties.Settings.Default.ResultScale;

            for (int i = 0; i < vecMaskPolys.Size; i++)
                rval.DrawPolyline(vecMaskPolys[i].ToArray(),true,new Bgr(255,255,255));

            return rval;
        }


        private void EraseDrawingBorders<TColor,TDepth>(ref Emgu.CV.Image<TColor,TDepth> img
                                                        , float divby=0
                                                        , float fAddLeft=0.0f
                                                        , float fAddTop=0.0f
                                                        , float fSubRight=0.0f          
                                                        , float fSubBottom=0.0f
                                                        , double dEraseColor=0.0
                                                        )
            
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
            // EraseDrawingBorders - the last 4 parameters are specified in the desired
            // resolution of the image (i.e. 32nds of inch, etc).

        {
            PointF[] dstBorders = GetDefaultBorders();
            dstBorders[0].X += fAddLeft;
            dstBorders[0].Y += fAddTop;
            dstBorders[1].X -= fSubRight;
            dstBorders[1].Y += fAddTop;
            dstBorders[2].X -= fSubRight;
            dstBorders[2].Y -= fSubBottom;
            dstBorders[3].X += fAddLeft;
            dstBorders[3].Y -= fSubBottom;

            PointF[] srcBorders = CvInvoke.PerspectiveTransform(dstBorders, invHomographyMatrix);
            if (divby > 0)
            {
                for (int i = 0; i < srcBorders.Length; i++)
                {
                    srcBorders[i].X /= divby;
                    srcBorders[i].Y /= divby;
                }
            }

            // Use the points just calculated to make polygons...
            System.Drawing.Point[][] pts = new System.Drawing.Point[][]
            {
                    new Point[]
                    {
                          new Point{ X=0, Y=0 }
                        , Point.Round(srcBorders[0])
                        , Point.Round(srcBorders[1])
                        , new Point { X=img.Width, Y=0 }
                    },
                    new Point[]
                    {
                          new Point { X=img.Width, Y=0 }
                        , Point.Round(srcBorders[1])
                        , Point.Round(srcBorders[2])
                        , new Point { X=img.Width, Y=img.Height }
                    },
                    new Point[]
                    {
                          new Point { X=img.Width, Y=img.Height }
                        , Point.Round(srcBorders[2])
                        , Point.Round(srcBorders[3])
                        , new Point { X=0, Y=img.Height }
                    },
                    new Point[]
                    {
                          new Point { X=0, Y=img.Height }
                        , Point.Round(srcBorders[3])
                        , Point.Round(srcBorders[0])
                        , new Point{ X=0, Y=0 }
                    }
            };

            Object pixelBlackColor;
            if (typeof(TColor) == typeof(Bgr))
            {
                pixelBlackColor = new Bgr(dEraseColor, dEraseColor, dEraseColor);
            }
            else
            if (typeof(TColor) == typeof(Bgra))
            {
                pixelBlackColor = new Bgra(dEraseColor, dEraseColor, dEraseColor, dEraseColor);
            }
            else
            {
                pixelBlackColor = new Gray(dEraseColor);
            }

            img.FillConvexPoly(pts[0], (TColor)pixelBlackColor);
            img.FillConvexPoly(pts[1], (TColor)pixelBlackColor);
            img.FillConvexPoly(pts[2], (TColor)pixelBlackColor);
            img.FillConvexPoly(pts[3], (TColor)pixelBlackColor);
        }

        private void DisplayEmptyPolys<TColor, TDepth>(ref Emgu.CV.Image<TColor,TDepth>img, ref float[] histIgnoreVals, bool bUseRealRack=false)
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
        {
            int nFillVal = vecMaskPolys.Size;
            Object pixelWhiteColor;

            if (bUseRealRack)
            {
                for (int i = 0; i < vecRackPolys.Size; i++)
                {
                    if (histIgnoreVals[i + 1] == 0)
                    {
                        VectorOfPointF vpf = vecRackPolys[i];
                        //VectorOfPoint vp = new VectorOfPoint();

                        PointF[] pfa = vecRackPolys[i].ToArray();
                        Point[] pa = new Point[pfa.Length];

                        for (int j = 0; j < vpf.Size; j++)
                            pa[j] = Point.Round(pfa[j]);

                        if (typeof(TColor) == typeof(Bgr))
                        {
                            pixelWhiteColor = new Bgr(255, 255, 255);
                        }
                        else
                        if (typeof(TColor) == typeof(Bgra))
                        {
                            pixelWhiteColor = new Bgra(255, 255, 255, 255);
                        }
                        else
                        {
                            if (typeof(TDepth) == typeof(byte))
                                pixelWhiteColor = new Gray(255);
                            else
                                pixelWhiteColor = new Gray(nFillVal);
                        }
                        img.FillConvexPoly(pa, (TColor)pixelWhiteColor);
                    }
                }

            }
            else
            {
                for (int i = 0; i < vecMaskPolys.Size; i++)
                {
                    if (histIgnoreVals[i + 1] == 0)
                    {
                        if (typeof(TColor) == typeof(Bgr))
                        {
                            pixelWhiteColor = new Bgr(255, 255, 255);
                        }
                        else
                        if (typeof(TColor) == typeof(Bgra))
                        {
                            pixelWhiteColor = new Bgra(255, 255, 255, 255);
                        }
                        else
                        {
                            if (typeof(TDepth) == typeof(byte))
                                pixelWhiteColor = new Gray(255);
                            else
                                pixelWhiteColor = new Gray(nFillVal);
                        }
                        img.FillConvexPoly(vecMaskPolys[i].ToArray(), (TColor)pixelWhiteColor);
                    }
                }
            }
        }

        private void CalcEmptyPolys( ref Emgu.CV.Image<Gray, ushort> imgSrc
                                   , ref Emgu.CV.Image<Gray, ushort> imgResult
                                   , ref float[] histIgnoreValsResult)
        {
            int x, y;
            int i;
            Mat mask;
            Rectangle rc = new Rectangle();
            int n;
            int ix, iy;
            List<System.Drawing.Point> vop = new List<System.Drawing.Point>();
            List<int> voi = new List<int>();
            List<System.Drawing.Point> vopr = new List<System.Drawing.Point>();
            List<int> voir = new List<int>();
            System.Drawing.Point pt = new Point();

            Emgu.CV.Image<Gray, ushort> timg = DrawRectDetectMask<Gray, ushort>(ref imgSrc);
            timg.Save("c:\\users\\g.norkus\\Desktop\\cvimgtimgcs.png");
            imgResult = imgSrc.And(imgResult);

            DenseHistogram Histo = new DenseHistogram(65536, new RangeF(0, 65536));
            Histo.Calculate<ushort>(new Image<Gray, ushort>[] { imgResult }, true, null);
            histIgnoreValsResult = Histo.GetBinValues();

            // Initialize the fill values to a single value
            matFillPoints.SetTo(new Gray(0F).MCvScalar);
            mask = new Mat(matFillPoints.Size + new System.Drawing.Size(2,2), DepthType.Cv8U, 1);

            // Now that we have the bin values, we can populate the testemptymatrix
            // We take advantage of the fact that the fill values are the same size (+1)
            // as the matFillPoints
            i = 1;
            for (y=0; y<matFillPoints.Rows; y++)
            {
                for (x=0; x<matFillPoints.Cols; x++)
                {
                    matFillPoints.SetValue(y, x, (histIgnoreValsResult[i] == 0) ? 32767F : 0F);
                    i++;
                }    
            }

            // Now, loop through the values and do a floodfill whenever we see a 65535

            i = 1;
            for (pt.Y = 0; pt.Y < matFillPoints.Rows; pt.Y++)
            {
                for (pt.X = 0; pt.X< matFillPoints.Cols; pt.X++)
                {
                    if (matFillPoints.GetValue(pt.Y, pt.X) > 0)
                    {
                        voi.Add(CvInvoke.FloodFill(matFillPoints, null, pt, new Gray(i).MCvScalar, out rc
                            , new Gray(0F).MCvScalar, new Gray(0F).MCvScalar, Connectivity.EightConnected, FloodFillType.FixedRange));
                        vop.Add(pt);
                        i++;
                    }
                }
            }

            for (i=0; i<voi.Count; i++)
            {
                if (voi[i] <= 2)
                {
                    voir.Add(CvInvoke.FloodFill(matFillPoints, null, vop[i], new Gray(0).MCvScalar, out rc
                                     , new Gray(0F).MCvScalar, new Gray(0F).MCvScalar, Connectivity.EightConnected, FloodFillType.FixedRange));
                    vopr.Add(vop[i]);
                }
            }

            i = 1;
            for (iy = 0; iy < matFillPoints.Rows; iy++)
            {
                for (ix = 0; ix < matFillPoints.Cols; ix++)
                {
                    n = (int)matFillPoints.GetValue(iy, ix);
                    if ( (n == 0) && (histIgnoreValsResult[i] == 0) )
                        histIgnoreValsResult[i] = 1;
                    i++;
                }
            }
        }

        string szLastFileName = "";
        string szLastFileNameFlash = "";

        
        private void LoadCheckerImages(object sender, RoutedEventArgs e)
        {
            LoadCalibrationImages();
        }

        private void LoadArucoImages(object sender, RoutedEventArgs e)
        {
            DetectCorners();
        }

        private void testButton3_Click(object sender, RoutedEventArgs e)
        {
            SaveCornerDetectImage();
        }

        private void UndistortImageClick(object sender, RoutedEventArgs e)
        {
            DetectEdges(true);
        }

        private void UndistortImageFoldersClick(object sender, RoutedEventArgs e)
        {
            FolderBrowseDetectEdges();
        }


        private void RedoClick(object sender, RoutedEventArgs e)
        {
            DetectEdges(false);
        }

        private async void SnapshotChecker_Click(object sender, RoutedEventArgs e)
        {
            if (gdiImage.Visibility!=Visibility.Collapsed) 
            {
                // This means we didn't have a video going yet...
                MediaView.Visibility = Visibility.Visible;
                gdiImage.Visibility = Visibility.Collapsed;

                // Start the video, and inform the user the
                // video was started and that they need to press
                // the snap button again to take a picture...

                bool bSuccess = await RefreshButtonWork_Async(sender, e);

                if (bSuccess)
                {
                    MessageBox.Show("The stream was started.  Please press Snap again to take a picture for calibration.");
                    return;
                }
                else
                {
                    MessageBox.Show("The stream could not be started.  Please check cable connections.");
                    return;
                }
            }

             CaptureFrame(sender, e, 3);
            //NumArucoImagesSnapped.Text = nNumArucosSnapped.ToString();
            //NumCheckerImagesSnapped.Text = nNumCheckersSnapped.ToString();


            //using (WebClient client = new WebClient())
            //{
            //    client.Credentials = new NetworkCredential("admin", "Cam3ra_Admin");
            //    client.DownloadFile("http://192.168.74.191/cgi-bin/snapshot.cgi", "c:\\users\\G.Norkus\\Downloads\\MyFile6.jpg");
            // }
        }

        private async void SnapshotAruco_Click(object sender, RoutedEventArgs e)
        {
            // Did the user change the CameraIP?
            if (strSelectedCameraURL != (ctlCameraDataCtl.CameraIPComboBox.SelectedItem as CameraData).URL)
            {
                // Stop the stream
                await MediaView.Media.Close();
                MediaView.Visibility = Visibility.Collapsed;
                gdiImage.Visibility = Visibility.Visible;
            }

            if (gdiImage.Visibility != Visibility.Collapsed)
            {
                // This means we didn't have a video going yet...
                MediaView.Visibility = Visibility.Visible;
                gdiImage.Visibility = Visibility.Collapsed;

                // Start the video, and inform the user the
                // video was started and that they need to press
                // the snap button again to take a picture...

                bool bSuccess = await RefreshButtonWork_Async(sender, e);

                if (bSuccess)
                {
                    MessageBox.Show("The stream was started.  Please press Snap again to take a picture for calibration.");
                    return;
                }
                else
                {
                    MessageBox.Show("The stream could not be started.  Please check cable connections.");
                    MediaView.Visibility = Visibility.Collapsed;
                    gdiImage.Visibility = Visibility.Visible;
                    return;
                }
            }

            CaptureFrame(sender, e, 1);

            //using (WebClient client = new WebClient())
            //{
            //    client.Credentials = new NetworkCredential("admin", "Cam3ra_Admin");
            //    client.DownloadFile("http://192.168.74.191/cgi-bin/snapshot.cgi", "c:\\users\\G.Norkus\\Downloads\\MyFile6.jpg");
            // }
        }

        private async void SnapshotMeasure_Click(object sender, RoutedEventArgs e)
        {
            string prefix;
            string prefixtype;
            string prefixtime;
            string prefixipaddr;
            string strSaveNoFlash;
            string strSaveFlash;
            bool bUseLastFile = false;

            prefixtime = prefix = prefixipaddr = prefixtype = string.Empty;
            prefix = Properties.Settings.Default.MeasurePath;
            prefixtype = "Measure";
            if (prefix != null && prefix.Length > 0 && prefix.ElementAt(prefix.Length - 1) != '\\')
            {
                prefix += "\\";
            }
            if (prefix == null || Directory.Exists(prefix) == false)
            {
                MessageBoxResult rval =
                MessageBox.Show("The path " + prefix + " does not exist.  Do you want to select a path?  If yes, please enter a path and press the Snap Checker Button again", "Image Capture Path Does Not Exist", MessageBoxButton.YesNo);
                if (rval == MessageBoxResult.Yes)
                    ctlCameraDataCtl.SetMeasurePath_Click(sender, e);
                return;
            }


            // Did the user change the CameraIP?
            if (strSelectedCameraURL != (ctlCameraDataCtl.CameraIPComboBox.SelectedItem as CameraData).URL)
            {
                // Stop the stream
                await MediaView.Media.Close();
                MediaView.Visibility = Visibility.Collapsed;
                gdiImage.Visibility = Visibility.Visible;
            }

            // Start the video, and inform the user the
            // video was started and that they need to press
            // the snap button again to take a picture...

            //bool bSuccess = await RefreshButtonWork_Async(sender, e);
            bool bSuccess = true;

            strSelectedCameraURL = string.Empty;
            if (ctlCameraDataCtl.CameraIPComboBox.SelectedIndex >= 0)
                strSelectedCameraURL = (ctlCameraDataCtl.CameraIPComboBox.SelectedItem as CameraData).URL;

            if (strSelectedCameraURL.ToLower().Contains("snapshot") == false)
                return;

            string curCameraURL = strSelectedCameraURL.ToLower();
            string curFlashCameraURL = curCameraURL.Replace("snapshot", "flashsnapshot");

            bool bCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (bCtrl == false)
            {
                using (WebClient client = new WebClient())
                {
                    DateTime dateTime = DateTime.Now;

                    prefixtime = dateTime.ToString("yyyy-MM-dd-HH-mm-ss");
                    strSaveFlash = prefix + prefixtype + "-00-Flash-" + prefixtime + ".png";
                    client.DownloadFile(curFlashCameraURL, strSaveFlash);

                    prefixtime = dateTime.ToString("yyyy-MM-dd-HH-mm-ss");
                    strSaveNoFlash = prefix + prefixtype + "-00-NoFlash-" + prefixtime + ".png";
                    client.DownloadFile(curCameraURL, strSaveNoFlash);

                    Emgu.CV.Image<Bgr, Byte> cvimgFlash = new Image<Bgr, Byte>(strSaveFlash);
                    Emgu.CV.Image<Bgr, Byte> cvimgNoFlash = new Image<Bgr, Byte>(strSaveNoFlash);
                    Emgu.CV.Image<Bgr, Byte> cvimgDiff = cvimgFlash.Sub(cvimgNoFlash);

                    prefixtime = dateTime.ToString("yyyy-MM-dd-HH-mm-ss");
                    szLastFileName = prefix + prefixtype + "-01-Diff-" + prefixtime + ".png";
                    szLastFileNameFlash = "";
                    cvimgDiff.Save(szLastFileName);

                    DetectEdges(false);

                    bSuccess = true;
                }
            }
            else
            {
                DetectEdges(true);
                bSuccess = true;
            }

            if (bSuccess)
            {
                //MessageBox.Show("The stream was started.  Please press Snap again to take a picture for calibration.");
                return;
            }
            else
            {
                MessageBox.Show("The stream could not be started.  Please check cable connections.");
                MediaView.Visibility = Visibility.Collapsed;
                gdiImage.Visibility = Visibility.Visible;
                return;
            }
        }

        static async Task ProcessRepositoriesAsyncPost(HttpClient client, System.Drawing.Image img )
        {
            string test;
            string cammatstr = Properties.Settings.Default.CameraMatrix;
            string distmatstr = Properties.Settings.Default.DistortionMatrix;
            string homogmatstr = Properties.Settings.Default.HomographyMatrix;

            byte[] img_byte_arr = ImagePacket.ImageToBytes(img);
            img.Dispose();
            ImagePacket packet = new ImagePacket(img_byte_arr);

            var url = "https://localhost:7179/PostPictureData";

            var jsonObj = new JsonObject
            {
                ["testPictureData_model"] = "string",
                ["pictureBytes"] = packet.image,
                ["cameramatrix"] = cammatstr,
                ["distortionmatrix"] = distmatstr,
                ["homographymatrix"] = homogmatstr,
                ["resultwidth"] = Properties.Settings.Default.ResultWidth.ToString(),
                ["resultlength"] = Properties.Settings.Default.ResultLength.ToString(),
                ["resultscale"] = Properties.Settings.Default.ResultScale.ToString(),
                ["borderbottom"] = Properties.Settings.Default.BorderBottom.ToString(),
                ["bordertop"] = Properties.Settings.Default.BorderTop.ToString(),
                ["borderright"] = Properties.Settings.Default.BorderRight.ToString(),
                ["borderleft"] = Properties.Settings.Default.BorderLeft.ToString(),
                //["cameramatrix"] = "Hello",
                //["distortionmatrix"] = "Hello",
                //["homographymatrix"] = "Hello",
            };

            System.Net.Http.HttpContent content = new StringContent(jsonObj.ToString(), UTF8Encoding.UTF8, "application/json");
            HttpResponseMessage messge;
            messge = await client.PostAsync(url, content);
            string description = string.Empty;
            if (messge.IsSuccessStatusCode)
            {
                MemoryStream ms;

                MessageBox.Show("Yeah!");
                string resultStr = await messge.Content.ReadAsStringAsync();
                JsonObject resultObj = JsonNode.Parse(resultStr).AsObject();

                string Test = resultObj["test"].ToString();
                string strBmp = resultObj["pictureBytes"].ToString();

                byte[] img_byte_arr_res = ImagePacket.DecodeBytes(strBmp);
                System.Drawing.Image img_res = ImagePacket.BytesToImage(img_byte_arr, out ms);
                var imageBitmap = new System.Drawing.Bitmap(img_res);

                imageBitmap.Save("c:\\users\\G.Norkus\\Desktop\\Goofy15.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        public System.Drawing.Image ConvertBitmapSourceToImage(BitmapSource input)
        {
            MemoryStream transportStream = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(input));
            enc.Save(transportStream);
            transportStream.Seek(0, SeekOrigin.Begin);
            return System.Drawing.Image.FromStream(transportStream);
        }
        private async void TestAPI_Click(object sender, RoutedEventArgs e)
        {
            using HttpClient client = new();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

            System.Windows.Media.Imaging.BitmapSource bmp = (BitmapSource)gdiImage.Source;
            System.Drawing.Image img = ConvertBitmapSourceToImage(bmp);
            await ProcessRepositoriesAsyncPost(client, img);
        }

        private async Task<bool> LoadFreshWorkCenterList()
        {
            WorkstationConnectionStrings.Clear();
            WorkstationNameStrings.Clear();
            CameraIPStrings.Clear();
            CameraURLStrings.Clear();

            if (ctlCameraDataCtl.CameraIPComboBox.SelectedIndex >= 0)
                strSelectedCameraURL = (ctlCameraDataCtl.CameraIPComboBox.SelectedItem as CameraData).URL;
            else
                strSelectedCameraURL = string.Empty;

            if (ctlCameraDataCtl.WorkCenterComboBox.SelectedIndex >= 0)
                strSelectedWorkstation = (ctlCameraDataCtl.WorkCenterComboBox.SelectedItem as WorkStationData).Name;
            else
                strSelectedWorkstation = string.Empty;

            //ctlCameraDataCtl.WorkCenterComboBox.Items.Clear();   // BAD, WILL CAUSE ERROR
            //ctlCameraDataCtl.CameraIPComboBox.Items.Clear();     // BAD, WILL CAUSE ERROR

            try
            {
                using (DBConnection dbConnection = new())
                {
                    // Use the connection to grab some simple data
                    Microsoft.Data.SqlClient.SqlCommand sqlCommand = dbConnection.GetSqlCommand();
                    if (sqlCommand != null)
                    {
                        // First, load temporary collections
                        CameraObservableColl tempCameraDataColl = new ();
                        WorkStationObservableColl tempWorkStationColl = new ();

                        tempCameraDataColl.PopulateFromDB(ref sqlCommand);
                        tempWorkStationColl.PopulateFromSqlCommand(ref sqlCommand);

                        // 'Except' finds the differences of two collections
                        IEnumerable<CameraData> cameras = tempCameraDataColl.Except(Static.CameraDataColl);
                        IEnumerable<WorkStationData> workstations = tempWorkStationColl.Except(Static.WorkStationColl);

                        // Has someone changed database entries before we have had a chance?
                        // If so, we will need to repopulate the data collections and
                        // warn the user.
                        if (cameras.Count()>0 || workstations.Count()>0)
                        {
                            Static.CameraDataColl.Clear();    // SHOULD call this instead
                            Static.CameraDataColl.PopulateFromDB(ref sqlCommand);

                            Static.WorkStationColl.Clear();    // SHOULD call this instead
                            Static.WorkStationColl.PopulateFromSqlCommand(ref sqlCommand);
                        }

                        // Now that we have the temporary collections, compare them to the
                        // existing collections.  If there is a difference, notify the 
                        // user and abort whatever operation was trying to be executed
                        // by returning a false.

                        //Static.CameraDataColl.Clear();    // SHOULD call this instead
                        //Static.CameraDataColl.PopulateFromDB(ref sqlCommand);

                        //Static.WorkStationColl.Clear();    // SHOULD call this instead
                        //Static.WorkStationColl.PopulateFromSqlCommand(ref sqlCommand);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
                return false;
            }

            /*
            if (Properties.Settings.Default.WorkStation != null &&
                Properties.Settings.Default.WorkStation.Trim().Length > 0)
            {
                ctlCameraDataCtl.WorkCenterComboBox.SelectedValue = strSelectedWorkstation = Properties.Settings.Default.WorkStation;

                // Did we get a CameraFK from this Workstation?
                if (CameraIPStrings[strSelectedWorkstation] != null && CameraIPStrings[strSelectedWorkstation] > 0)
                {
                    ctlCameraDataCtl.CameraIPComboBox.SelectedValue = WorkstationConnectionStrings[strSelectedWorkstation];
                }
            }
            else
            if (ctlCameraDataCtl.WorkCenterComboBox.Items.Count > 0)
            {
                ctlCameraDataCtl.WorkCenterComboBox.SelectedIndex = 0;
                strSelectedWorkstation = ctlCameraDataCtl.WorkCenterComboBox.SelectedItem.ToString();
            }*/

            return true;
        }

        private async Task<bool> RefreshButtonWork_Async(object sender, RoutedEventArgs e)
        {
            string tstr;
            string toutsr;
            toutsr = tstr = "";

            Debug.WriteLine("Entering RefreshButton_Click nLoadedCnt=" + nLoadedCnt.ToString());
            if (nLoadedCnt > 1)
                return false;

            bool bSuccess = true;

            /*
            if (isLoaded == false)
            {
                isLoaded = true;

                bSuccess = await LoadFreshWorkCenterList();
                if (bSuccess == false)
                {
                    isLoaded = false;
                    return false;
                }
            }
            */

            MediaView.Visibility = Visibility.Visible;
            gdiImage.Visibility = Visibility.Collapsed;

            MediaView.TextBlk.Text = "Attempting to start camera stream...";
            MediaView.TextBlk.Visibility = Visibility.Visible;
            if (ctlCameraDataCtl.CameraIPComboBox.SelectedIndex >= 0)
                strSelectedCameraURL = (ctlCameraDataCtl.CameraIPComboBox.SelectedItem as CameraData).URL;
            else
                strSelectedCameraURL = string.Empty;


            //bSuccess = WorkstationConnectionStrings.TryGetValue(strSelectedWorkstation, out strSelectedCameraURL);
            long nCountDown = 11;

            if (bSuccess)
            {
                nCountDown--;
                while (nCountDown > 0)
                {
                    Debug.WriteLine("nCountDown Loop = " + nCountDown.ToString());
                    await MediaView.Media.Close();

                    //MediaView.Media.LoopingBehavior= MediaView.Media.LoopingBehavior.
                    MediaView.Media.LoadedBehavior = Unosquare.FFME.Common.MediaPlaybackState.Play;
                    MediaView.Media.UnloadedBehavior = Unosquare.FFME.Common.MediaPlaybackState.Manual;

                    if (nCountDown < 10)
                        MediaView.TextBlk.Text = "Refreshing " + (11 - nCountDown).ToString();
                    else
                        MediaView.TextBlk.Text = "Refreshing";
                    MediaView.TextBlk.Visibility = Visibility.Visible;
                    nCountDown--;
                    Debug.WriteLine("nCountDown--");
                    //MediaView.CameraStreamURL = "";
                    //MediaView.CameraStreamURL = strSelectedCameraURL;
                    //MediaView.Media.Source = new System.Uri(strSelectedCameraURL);

                    bool bDidOpen = false;

                    bDidOpen = await MediaView.Media.Open(new Uri(strSelectedCameraURL));
                    if (bDidOpen)
                    {
                        Debug.WriteLine("Did open...nCountDown = " + nCountDown.ToString());
                        MediaView.Media.Position = new TimeSpan(0);
                        await MediaView.Media.Play();
                        MediaView.TextBlk.Visibility = Visibility.Hidden;
                        nCountDown = -1;
                        return true;
                    }
                }
                if (nCountDown == 0)
                {
                    MediaView.TextBlk.Visibility = Visibility.Visible;
                    MediaView.TextBlk.Text = "Could not start stream.  A network cable to the camera may be unplugged, or the camera may have lost power.";
                }
            }
            return false;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshButtonWork_Async(sender, e);
        }

        private void ChildWindowUserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Take focus away from current item to allow all the controls to 
            // properly update.
            Properties.Settings.Default.Save();
            base.OnClosing(e); 
        }

    }


    public class ImagePacket
    {
        public string hash { get; set; } = string.Empty;
        public int len { get; set; } = 0;
        public string image { get; set; } = string.Empty;
        public ImagePacket() { }
        public ImagePacket(byte[] img_sources)
        {
            hash = StringHash(img_sources);
            len = img_sources.Length;
            image = EncodeBytes(img_sources);
        }
        public byte[] GetRawData()
        {
            byte[] data = DecodeBytes(image);

            if (data.Length != len) throw new Exception("Error data len");
            if (!StringHash(data).Equals(hash)) throw new Exception("Error hash");

            return data;
        }


        /// <summary>
        /// Get original image
        /// </summary>
        /// <returns></returns>
        //static Image TakeScreen()
        //{
        //Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        //Graphics g = Graphics.FromImage(bitmap);
        //g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
        //return bitmap;
        //}
        /// <summary>
        /// Conver Image to byte array
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ImageToBytes(System.Drawing.Image value)
        {
            ImageConverter converter = new ImageConverter();
            byte[] arr = (byte[])converter.ConvertTo(value, typeof(byte[]));
            return arr;
        }
        /// <summary>
        /// Conver byte array to Image
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static System.Drawing.Image BytesToImage(byte[] value, out MemoryStream ms)
        {
            using (ms = new MemoryStream(value))
            {
                System.Drawing.Image rv = System.Drawing.Image.FromStream(ms);
                return rv;
            }
        }
        /// <summary>
        /// Convert bytes to base64
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EncodeBytes(byte[] value) => Convert.ToBase64String(value);
        /// <summary>
        /// convert base64 to bytes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] DecodeBytes(string value) => Convert.FromBase64String(value);
        /// <summary>
        /// get MD5 hash from byte array
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string StringHash(byte[] value)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(value);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}
