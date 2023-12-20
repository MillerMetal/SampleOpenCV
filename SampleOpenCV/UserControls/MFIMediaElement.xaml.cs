using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing;
using Unosquare.FFME.Common;
using System.Drawing.Drawing2D;
//using APlusInnovations.Controls;

namespace SampleOpenCV.UserControls
{
    /// <summary>
    /// Interaction logic for MFIMediaElement.xaml
    /// </summary>
    public partial class MFIMediaElement : UserControl
    {

        private bool m_bLoadSucceeded = false;

        public string CameraStreamURL
        {
            get { return (string)GetValue(CameraStreamURLProperty); }
            set { SetValue(CameraStreamURLProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CameraStreamURL.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CameraStreamURLProperty =
            DependencyProperty.Register("CameraStreamURL", typeof(string), typeof(MFIMediaElement), new PropertyMetadata("rtsp://mysample.com/11"));


        public MFIMediaElement()
        {
            InitializeComponent();

            this.Media.MediaFailed += OnMediaFailed;
            this.Media.MediaInitializing += OnMediaInitializing;
            this.Media.MediaOpened += OnMediaOpened;
            this.Media.MediaChanged += OnMediaChanged;
            this.Media.MediaStateChanged += OnMediaStateChanged;
            //this.Media.MediaClosed += OnMediaClosed;

            //this.Media.PreviewMouseDoubleClick += OnMyMediaDoubleClick2;

            //BindMediaRenderingEvents();
        }

        private void BindMediaRenderingEvents()
        {
            Bitmap overlayBitmap = null;
            Graphics overlayGraphics = null;
            var overlayTextFont = new Font("Courier New", 144, System.Drawing.FontStyle.Bold);
            var overlayTextFontBrush = System.Drawing.Brushes.WhiteSmoke;
            var overlayTextOffset = new PointF(12, 8);
            var overlayBackBuffer = IntPtr.Zero;

            this.Media.RenderingVideo += (s, e) =>
            {
                //if (this.TextBlk.Visibility != Visibility.Hidden)
                  //  this.TextBlk.Visibility = Visibility.Hidden;

                if (overlayBackBuffer != e.Bitmap.Scan0)
                {
                    overlayGraphics?.Dispose();
                    overlayBitmap?.Dispose();

                    overlayBitmap = e.Bitmap.CreateDrawingBitmap();

                    overlayBackBuffer = e.Bitmap.Scan0;
                    overlayGraphics = Graphics.FromImage(overlayBitmap);
                    overlayGraphics.InterpolationMode = InterpolationMode.Default;
                }

                overlayGraphics?.DrawString(
                    $"Clock: {e.Clock.TotalSeconds:00.00}\r\n" +
                    $"PN   : {e.PictureNumber}",
                    overlayTextFont,
                    overlayTextFontBrush,
                    overlayTextOffset);
            };
        }

        private void OnMyMediaDoubleClick(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            //MainWindowViewModel mainWindowViewModel = GetMainWindowViewModel();

            //if (mainWindowViewModel != null)
           // {
            //    mainWindowViewModel.EditNest(WorkOrderNestPK);
           // }
            MessageBox.Show("Hello World 1!");
        }

        private void OnMyMediaDoubleClick2(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show("Hello World 2!");
        }

        private void OnMediaFailed(object sender, MediaFailedEventArgs e)
        {
            //var errorCategory = e.ErrorException is TimeoutException
              //  ? "Timeout"
                //: "General";
            m_bLoadSucceeded = false;
            if (this.TextBlk.Visibility== Visibility.Hidden)
            {
                this.TextBlk.Visibility = Visibility.Visible;
                this.TextBlk.Text = "No Media Stream";
            }
            /*
            MessageBox.Show(
                Application.Current.MainWindow,
                $"Media Failed ({errorCategory}): {e.ErrorException.GetType()}\r\n{e.ErrorException.Message}",
                $"{nameof(Media)} Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK);*/
        }

        private void OnMediaChanged(object sender, MediaOpenedEventArgs e)
        {
            string tstr = this.TextBlk.Text;
            this.TextBlk.Text = tstr;
        }

        private void OnMediaStateChanged(object sender, MediaStateChangedEventArgs e)
        {
            string tstr = this.TextBlk.Text;
            this.TextBlk.Text = tstr;
        }



        private void OnMediaOpened(object sender, MediaOpenedEventArgs e)
        {
            if (m_bLoadSucceeded)
                this.TextBlk.Visibility = Visibility.Hidden;
            else
            {
                this.TextBlk.Visibility = Visibility.Visible;
                this.TextBlk.Text = "Media Load Failed";
            }
        }

        private void OnMediaClosed(object sender, EventArgs e)
        {
            this.TextBlk.Visibility = Visibility.Visible;
            this.TextBlk.Text = "Media Closed";
        }

        private void OnMediaInitializing(object sender, MediaInitializingEventArgs e)
        {
            //this.TextBlk.Visibility = Visibility.Visible;
            //this.TextBlk.Text = "Media Initializing";


            // An example of injecting input options for http/https streams
            // A simple website to get live stream examples: https://pwn.sh/tools/getstream.html
            if (e.MediaSource.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                e.MediaSource.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                e.Configuration.PrivateOptions["user_agent"] = $"{typeof(ContainerConfiguration).Namespace}/{typeof(ContainerConfiguration).Assembly.GetName().Version}";
                e.Configuration.PrivateOptions["headers"] = "Referer:https://www.unosquare.com";
            }

            // Example of forcing tcp transport on rtsp feeds
            // RTSP is similar to HTTP but it only provides metadata about the underlying stream
            // Most RTSP compatible streams expose RTP data over both UDP and TCP.
            // TCP provides reliable communication while UDP does not
            if (e.MediaSource.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase))
            {
                e.Configuration.PrivateOptions["rtsp_transport"] = "tcp";
                e.Configuration.GlobalOptions.FlagNoBuffer = true;

                // You can change the open/read timeout before the packet reading
                // operation fails. Reaching a tiemout limit will fire the MediaFailed event
                // with a TiemoutException
                e.Configuration.ReadTimeout = TimeSpan.FromSeconds(10);
            }

            //this.TextBlk.Visibility = Visibility.Hidden;

        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string tstr = CameraStreamURL;

            m_bLoadSucceeded = true;
            if (tstr != null)
            {
                try
                {
                    this.TextBlk.Visibility = Visibility.Visible;
                    this.TextBlk.Text = "Opening Media Stream";
                    await this.Media.Open(new Uri(tstr));
                    this.Media.Volume = 0;
                    this.Media.IsMuted = true;
                }
                catch(Exception ex)
                {
                    this.TextBlk.Visibility = Visibility.Visible;
                    this.TextBlk.Text = "Media Failed";
                }
            }
            else
            {
                this.TextBlk.Visibility = Visibility.Visible;
                this.TextBlk.Text = "No Video Stream";
            }
        }

    }
}
