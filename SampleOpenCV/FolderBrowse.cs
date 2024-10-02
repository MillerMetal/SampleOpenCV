using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Emgu.CV;
using MathNet.Spatial.Euclidean;
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Drawing.Configuration;
using Microsoft.VisualBasic;
using MathNet.Spatial.Units;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Collections;

namespace SampleOpenCV
{
    public partial class MainWindow : Window
    {
        public static ICollection<string> GetFilesIncludingSubfolders(string path, string searchPattern)
        {
            List<string> paths = new List<string>();
            var directories = Directory.GetDirectories(path);

            foreach (var directory in directories)
            {
                paths.AddRange(GetFilesIncludingSubfolders(directory, searchPattern));
            }
            paths.AddRange(Directory.GetFiles(path, searchPattern).ToList());
            return paths;
        }


        private void FolderBrowseDetectEdges()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if ( (result == System.Windows.Forms.DialogResult.OK) && (!string.IsNullOrWhiteSpace(fbd.SelectedPath)))
                {
                    int i;
                    int nIndOriginal;
                    int nIndFlash;
                    string[] files ;

                    List<string> paths = new List<string>();
                    if (System.Windows.Forms.MessageBox.Show("Do you want to clear the previous results first?", "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        paths.AddRange(GetFilesIncludingSubfolders(fbd.SelectedPath, "goofy*.*"));
                        files = paths.ToArray();
                        for (i=0; i<files.Length; i++)
                        {
                            if (files[i].IndexOf("goofy00-")<0)
                            {
                                if (File.Exists(files[i]))
                                {
                                    try
                                    {
                                        Title = "MainWindow - Multi Folder Deleting " + files[i];
                                        File.Delete(files[i]);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                paths.Remove(files[i]);
                            }
                        }
                        Title = "MainWindow - Multi Folder Deleting Complete";


                        if (System.Windows.Forms.MessageBox.Show("The selected file folders have been emptied of processing results.  Do you want to continue and process all the folders?", "", MessageBoxButtons.YesNo)== System.Windows.Forms.DialogResult.No)
                        {
                            return;
                        }
                    }
                    else
                    {
                        paths.AddRange(GetFilesIncludingSubfolders(fbd.SelectedPath, "goofy00-*.*"));
                    }

                    files = paths.ToArray();


                    string szPathOriginal;
                    string szPathFlash;
                    string tstr;
                    string szOutputPath;

                    // The folders should be arranged in pairs...
                    if (files.Length > 0) {
                        //if (files.Length % 2 == 0) {
                        //    System.Windows.Forms.MessageBox.Show("Press OK to start processing " + files.Length / 2 + " image pairs");
                        //for (i = 0; i <)
                        //}
                        i = 0;
                        while ( (i+1) < files.Length)
                        {
                            szPathOriginal = szPathFlash = "";          // start with empty strings

                            tstr = files[i].ToLower();                  // try to find original or flash
                            nIndOriginal = tstr.IndexOf("-original");
                            nIndFlash = tstr.IndexOf("-flash");
                            if (nIndFlash >= 0)
                                szPathFlash = tstr;
                            if (nIndOriginal >= 0)
                                szPathOriginal = tstr;

                            tstr = files[i+1].ToLower();                // next find should be the opposite of the first
                            if (nIndFlash >= 0)
                            {
                                nIndOriginal = tstr.IndexOf("-original");
                                if (nIndOriginal >= 0)
                                    szPathOriginal = tstr;
                            }
                            else
                            if (nIndOriginal >= 0)
                            {
                                nIndFlash = tstr.IndexOf("-flash");
                                if (nIndFlash >= 0)
                                    szPathFlash = tstr;
                            }

                            // Skip folders that have already been calculated
                            szOutputPath = System.IO.Path.GetDirectoryName(szPathOriginal) + "\\" + "goofy00x01.png";
                            if (File.Exists(szOutputPath)==false)
                            {
                                if (szPathFlash.Length > 0 && szPathOriginal.Length > 0)
                                {
                                    if (szPathFlash.Substring(0, nIndFlash) == szPathOriginal.Substring(0, nIndOriginal))
                                    {
                                        // OK!  Ready to detect edges!
                                        szLastFileName = szPathOriginal;
                                        szLastFileNameFlash = szPathFlash;
                                        DetectEdges(false);
                                    }
                                    else
                                    {
                                        // The paths were not the same
                                        System.Windows.Forms.MessageBox.Show("One of the files selected could not be found.  Process stopping.");
                                    }
                                }
                                else
                                {
                                    // We didn't find one of the files...  notify user
                                    System.Windows.Forms.MessageBox.Show("The files selected could not be found.  Process stopping.");
                                }
                            }



                            i += 2;
                        }

                        Title = "MainWindow - Multi Folder Undistort Complete";
                    }
                }
            }
        }
    }
}