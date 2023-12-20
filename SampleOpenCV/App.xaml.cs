using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unosquare.FFME;

namespace SampleOpenCV
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            string szCurDir = System.IO.Path.GetDirectoryName(
                 System.Reflection.Assembly.GetExecutingAssembly().Location);

            Library.FFmpegDirectory = szCurDir + "\\FFMPeg\\";
            Library.FFmpegLoadModeFlags = FFmpegLoadMode.FullFeatures;
            Library.EnableWpfMultiThreadedVideo = false; // !System.Diagnostics.Debugger.IsAttached; // test with true and false
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Task.Run(async () =>
            {
                try
                {
                    // Pre-load FFmpeg
                    await Library.LoadFFmpegAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("We goofed" + Environment.NewLine + ex.Message);
                }
            });
        }
    }
}
