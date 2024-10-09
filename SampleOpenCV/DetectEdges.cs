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
using System.Numerics;
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
using System.Collections.Immutable;
using System.Windows.Forms;
using System.Diagnostics.Eventing.Reader;
using static MS.WindowsAPICodePack.Internal.CoreNativeMethods;

namespace SampleOpenCV
{
    class IndexedLineSegmentLengthComparer : IComparer<IndexedLineSegment>
    {
        public int Compare(IndexedLineSegment x, IndexedLineSegment y)
        {
            if (x.linesegment.Length < y.linesegment.Length) return -1;
            if (x.linesegment.Length > y.linesegment.Length) return 1;
            return 0;
        }
    }
    public class IndexedLineSegment {
        public int ind {  get; set; }
        public LineSegment2DF linesegment {  get; set; }

        public override int GetHashCode()
        {
            return ind;
        }

        public override bool Equals(object obj)
        {
            IndexedPoint other = obj as IndexedPoint;
            return ind == other.ind;
        }
        public IndexedLineSegment()
        {
            linesegment = new LineSegment2DF(); 
            ind = 0;
        }

        public IndexedLineSegment(LineSegment2DF newlinesegment, int newind)
        {
            linesegment = newlinesegment; ind = newind;
        }

        static public IndexedLineSegment[] ToArray(List<LineSegment2DF> listArrIndex)
        {
            IndexedLineSegment[] ptArr = null;
            int i;

            ptArr = new IndexedLineSegment[listArrIndex.Count];
            for (i = 0; i < listArrIndex.Count; i++)
                ptArr[i].linesegment = listArrIndex[i];

            return ptArr;
        }
    }



    public class LinkedLineSegment2D 
    {
        public Emgu.CV.Structure.LineSegment2D lineOriginal ;
        List<LinkedLineSegment2D> listLineSegment2D = null;
        List<PointF> listPointF = null;

        public LinkedLineSegment2D (System.Drawing.Point p1, System.Drawing.Point p2)
        {
            lineOriginal.P1 = p1;
            lineOriginal.P2 = p2;

            listLineSegment2D = new List<LinkedLineSegment2D>();
            listPointF = new List<PointF>();
        }

        public LinkedLineSegment2D (Emgu.CV.Structure.LineSegment2D line)
        {
            lineOriginal = line;
            listLineSegment2D = new List<LinkedLineSegment2D>();
            listPointF = new List<PointF>();
        }

        public PointF? Intersect90(ref LinkedLineSegment2D lineSegProposed, double dEpsilon=1.0)
        {
            double dExtAngle = lineOriginal.GetExteriorAngleDegree(lineSegProposed.lineOriginal);
            if ((dExtAngle > (270.0 - dEpsilon) && dExtAngle < (270.0 + dEpsilon)) ||
                (dExtAngle > (90.0 - dEpsilon) && dExtAngle < (90.0 + dEpsilon)) ||
                (dExtAngle > (-90.0 - dEpsilon) && dExtAngle < (-90.0 + dEpsilon))
                )
            {
                PointF pointIntersection = Intersect(lineSegProposed) ?? new PointF(-1000000,-1000000);

                if (pointIntersection.X!=1000000)
                {
                    listPointF.Add(pointIntersection);
                    lineSegProposed.listPointF.Add(pointIntersection);
                    listLineSegment2D.Add(lineSegProposed);
                    lineSegProposed.listLineSegment2D.Add(this);
                    return pointIntersection;
                }
            }

            return null;
        }

        public PointF? Intersect(LinkedLineSegment2D lineSegProposed)
        {
            if (lineSegProposed==null) return null;
            
            float A1 = lineOriginal.P2.Y - lineOriginal.P1.Y;
            float B1 = lineOriginal.P1.X - lineOriginal.P2.X;
            float C1 = A1 * lineOriginal.P1.X + B1 * lineOriginal.P1.Y;

            //Line2
            float A2 = lineSegProposed.lineOriginal.P2.Y - lineSegProposed.lineOriginal.P1.Y;
            float B2 = lineSegProposed.lineOriginal.P1.X - lineSegProposed.lineOriginal.P2.X;
            float C2 = A2 * lineSegProposed.lineOriginal.P1.X + B2 * lineSegProposed.lineOriginal.P1.Y;

            float det = A1 * B2 - A2 * B1;
            if (det == 0)
            {
                return null;
            }
            else
            {
                float x = (B2 * C1 - B1 * C2) / det;
                float y = (A1 * C2 - A2 * C1) / det;
                return new PointF(x, y);
            }
        }
    }


    public partial class MainWindow : Window
    {
        // Function for circle-generation
        // using Bresenham's algorithm


        static PointF? GetIntersection(Emgu.CV.Structure.LineSegment2D line1, Emgu.CV.Structure.LineSegment2D line2)
        {
            float A1 = line1.P2.Y - line1.P1.Y;
            float B1 = line1.P1.X - line1.P2.X;
            float C1 = A1 * line1.P1.X + B1 * line1.P1.Y;

            //Line2
            float A2 = line2.P2.Y - line2.P1.Y;
            float B2 = line2.P1.X - line2.P2.X;
            float C2 = A2 * line2.P1.X + B2 * line2.P1.Y;

            float det = A1 * B2 - A2 * B1;
            if (det == 0)
            {
                return (PointF?)null;
            }
            else
            {
                float x = (B2 * C1 - B1 * C2) / det;
                float y = (A1 * C2 - A2 * C1) / det;
                return new PointF(x, y);
            }
        }

        static bool PtfInBounds(PointF pt, int rows, int cols)
        {
            return (pt.X >= 0.0 && pt.Y >= 0.0 && pt.X < cols && pt.Y < rows);
        }

        void drawCircle(ref byte[,,] myimageData, int xc, int yc, int x, int y, int i, int h)
        //, byte d)
        {
            myimageData[yc + x - h, xc + y - h, 0] = 31; // 1
            myimageData[yc - y, xc - x, 0] = 63; // 7
            myimageData[yc - y, xc + x - h, 0] = 95; // 6
            myimageData[yc + x - h, xc - y, 0] = 127; // 4
            myimageData[yc - x, xc - y, 0] = 159; // 5
            myimageData[yc + y - h, xc + x - h, 0] = 191; // 2
            myimageData[yc + y - h, xc - x, 0] = 223; // 3
            myimageData[yc - x, xc + y - h, 0] = 255; // 8

            /*
            myimageData[yc + x + h, xc + y + h, 0] = 31; // 2
            myimageData[yc + y + h, xc + x + h, 0] = 63; // 1
            myimageData[yc - y, xc + x + h, 0] = 95; // 4
            myimageData[yc - x, xc + y + h, 0] = 127; // 3
            myimageData[yc - x, xc - y, 0] = 159; // 6
            myimageData[yc - y, xc - x, 0] = 191; // 5
            myimageData[yc + y + h, xc - x, 0] = 223; // 8
            myimageData[yc + x + h, xc - y, 0] = 255; // 7
            */

            Debug.WriteLine(
                            (x - h).ToString() + "," + (y - h).ToString() + "," + i.ToString() + "," + // 1
                            (-y).ToString() + "," + (-x).ToString() + "," + i.ToString() + "," + // 2
                            (-y).ToString() + "," + (x - h).ToString() + "," + i.ToString() + "," + // 3
                            (x - h).ToString() + "," + (-y).ToString() + "," + i.ToString() + "," + // 4
                            (-x).ToString() + "," + (-y).ToString() + "," + i.ToString() + "," + // 5
                            (y - h).ToString() + "," + (x - h).ToString() + "," + i.ToString() + "," + // 6
                            (y - h).ToString() + "," + (-x).ToString() + "," + i.ToString() + "," + // 7
                            (-x).ToString() + "," + (y - h).ToString() + "," + i.ToString() + "," // 8
                            );

        }
        void circleBresFast(ref byte[,,] myimageData, int yc, int xc, int d)
        {
            int r = (d - 1) / 2;                   // 5:  r = (5-1)/2 = 2     6:  r=(6-1)/2 = 2;
            int h = ((d - 1) & 1) == 1 ? 1 : 0;     // is it an odd or even radius?  odd=1, even=0
            int x = 0;
            int y = -r;
            int F_M = 1 - r;
            int d_e = 3;
            int d_ne = -(r << 1) + 5;
            int i = 1;

            Debug.WriteLine("y,x,2,y,x,4,y,x,8,y,x,3,y,x,6,y,x,1,y,x,5,y,x,7");
            drawCircle(ref myimageData, xc, yc, x, y, i++, h);

            while (x < -y)
            {
                if (F_M <= 0)
                {
                    F_M += d_e;
                }
                else
                {
                    F_M += d_ne;
                    d_ne += 2;
                    y++;
                }
                d_e += 2;
                d_ne += 2;
                x++;
                drawCircle(ref myimageData, xc, yc, x, y, i++, h);
            }
        }

        void SetOctantArrayPoints(ref List<System.Drawing.Point> offsets
                                  , int x
                                  , int y
                                  , int nOctantIndex
                                  , int h
                                  , int nInsertIndex
                                  , bool bTestIndex
                                    )
        {
            // Initialize default offsets
            int xp = 0;
            int yp = 0;
            bool bDoSet = true;

            // Set the correct octant value according the sequence specified by
            // nOctantIndex.
            switch (nOctantIndex)
            {
                case 0: yp = x - h; xp = y - h; break;
                case 1: yp = -y; xp = -x; break;
                case 2: yp = -y; xp = x - h; break;
                case 3: yp = x - h; xp = -y; break;
                case 4: yp = -x; xp = -y; break;
                case 5: yp = y - h; xp = x - h; break;
                case 6: yp = y - h; xp = -x; break;
                case 7: yp = -x; xp = y - h; break;
                default:
                    break;
            }

            // Even indeces add normally,
            if (bTestIndex)
            {
                if ((xp == offsets[nInsertIndex - 1].X)
                    && (yp == offsets[nInsertIndex - 1].Y))
                    bDoSet = false;
            }

            if ((nOctantIndex & 1) == 0)
            {
                if (bDoSet)
                    offsets.Add(new System.Drawing.Point(xp, yp));
            }
            else
            // Odd indeces add in reverse order
            {
                if (bDoSet)
                    offsets.Insert(nInsertIndex, new System.Drawing.Point(xp, yp));
            }
        }

        int CountFilledGrayPoints(ref Emgu.CV.Image<Gray,Byte> img, int x, int y, int x2, int y2, bool bIgnoreLast=false)
        {
            int count = 0;
            Byte[,,] imgdata = img.Data;
            int imgw = img.Width;
            int imgh = img.Height;
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            int nLoopLen = bIgnoreLast ? longest - 1 : longest;
            for (int i = 0; i <= nLoopLen; i++)
            {
                if (x >= 0
                  && x < imgw
                  && y >= 0
                  && y <= imgh
                  && imgdata[y, x, 0] != 0)
                    count++;

                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
            return count;
        }


        List<System.Drawing.PointF> GenerateLinearOffsets(int x, int y, int x2, int y2, bool bIgnoreLast = false)
        {
            List<System.Drawing.PointF> offsets = new List<System.Drawing.PointF>();

            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            int nLoopLen = bIgnoreLast ? longest - 1 : longest;
            for (int i = 0; i <= nLoopLen; i++)
            {
                offsets.Add(new System.Drawing.PointF(x, y));
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
            return offsets;
        }
        List<System.Drawing.PointF> GenerateLinearOffsetsF(PointF pt1, PointF pt2, bool bIgnoreLast = false)
        {
            return GenerateLinearOffsets((int)pt1.X, (int)pt1.Y, (int)pt2.X, (int)pt2.Y, bIgnoreLast);
        }
        List<System.Drawing.PointF> GenerateLinearOffsets2D(Point2D pt1, Point2D pt2, bool bIgnoreLast = false)
        {
            return GenerateLinearOffsets((int)pt1.X, (int)pt1.Y, (int)pt2.X, (int)pt2.Y, bIgnoreLast);
        }

        // DistanceToFilledPixel -
        // Takes source and result images as parameters.  The source image is copied
        // to the result image, and lines are drawn orthoganal to the direction of 
        // the horizontal list of coordinates provided.  The rvalLines list returned
        // is drawn in the order of the pixel values listed...
        bool DistanceToFilledPixel(ref Emgu.CV.Image<Gray, Byte> imgThresholdAdaptive
                                  , ref Emgu.CV.Image<Bgra, Byte> imgResult
                                  , ref List<System.Drawing.PointF> listHorz
                                  , ref List<System.Drawing.PointF> listUp
                                  , ref List<System.Drawing.PointF> listDown
                                  , ref List<IndexedLineSegment> rvalLines
                                  , ref IndexedLineSegment rvalSegment
                                  //, ref System.Drawing.PointF rval1
                                  //, ref System.Drawing.PointF rval2
                                  , MCvScalar fillcolor)
        {
            bool bval = true;
            bool bfoundup = false;
            bool bfounddown = false;
            int i, j, x, y, xp, yp;
            byte[,,] threshData = imgThresholdAdaptive.Data;
            int w = imgThresholdAdaptive.Width;
            int h = imgThresholdAdaptive.Height;
            int nNumLines = 0;
            PointF rval1 = new PointF();
            PointF rval2 = new PointF();

            System.Drawing.PointF testpt = new PointF();
            rvalLines.Clear();

            for (i = 0; i < listHorz.Count; i++)
            {
                xp = (int)listHorz[i].X;
                yp = (int)listHorz[i].Y;

                bfoundup = false;
                bfounddown = false;

                // look for intersection in the up direction
                for (j = 0; j < listUp.Count && !bfoundup; j++)
                {
                    x = (int)listUp[j].X + xp;
                    y = (int)listUp[j].Y + yp;
                    if (y >= 0 && y < imgThresholdAdaptive.Height &&
                        x >= 0 && x < imgThresholdAdaptive.Width)
                    {
                        if (threshData[y, x, 0] != 0 && j > 0)
                        {
                            bfoundup = true;
                            x = (int)listUp[j - 1].X + xp;
                            y = (int)listUp[j - 1].Y + yp;
                            rval1 = new System.Drawing.PointF(x, y);
                        }
                    }
                }
                // look for intersection in the up direction
                for (j = 0; j < listDown.Count && !bfounddown; j++)
                {
                    x = (int)listDown[j].X + xp;
                    y = (int)listDown[j].Y + yp;
                    if (y >= 0 && y < h && x >= 0 && x < w)
                    {
                        if (threshData[y, x, 0] != 0 && j > 0)
                        {
                            bfounddown = true;
                            x = (int)listDown[j - 1].X + xp;
                            y = (int)listDown[j - 1].Y + yp;
                            rval2 = new System.Drawing.PointF(x, y);
                        }
                    }
                }

                if (bfounddown && bfoundup)
                {
                    //CvInvoke.Line(imgResult, System.Drawing.Point.Round(rval1), System.Drawing.Point.Round(rval2), fillcolor);
                    rvalLines.Add(new IndexedLineSegment(new LineSegment2DF(rval1, rval2), nNumLines));
                }
            }

            if (rvalLines.Count > 0) 
            {
                // If we found lines, lets sort them by length.
                IndexedLineSegment[] linesSortedByLength = rvalLines.ToArray();
                Array.Sort(linesSortedByLength, new IndexedLineSegmentLengthComparer());

                // Now, pick off the median value from that list
                rvalSegment = linesSortedByLength[linesSortedByLength.Length/2];

                System.Drawing.PointF MyP1 = rvalSegment.linesegment.P1;
                System.Drawing.PointF MyP2 = rvalSegment.linesegment.P2;
                System.Numerics.Vector2 MyDir = rvalSegment.linesegment.Direction.ToVector2();

                // Draw arrow heads and lines in both directions
                CvInvoke.ArrowedLine(imgResult
                                     , System.Drawing.Point.Round(MyP1)
                                     , System.Drawing.Point.Round(MyP2)
                                     , fillcolor
                                     , 3
                                     , LineType.EightConnected
                                     , 0
                                     , 30.0 / rvalSegment.linesegment.Length);
                CvInvoke.ArrowedLine(imgResult
                                     , System.Drawing.Point.Round(MyP2)
                                     , System.Drawing.Point.Round(MyP1)
                                     , fillcolor
                                     , 3
                                     , LineType.EightConnected
                                     , 0
                                     , 30.0 / rvalSegment.linesegment.Length);

                CvInvoke.DrawMarker(imgResult, System.Drawing.Point.Round(MyP1), new MCvScalar(255,128,128,255), MarkerTypes.Cross, 40, 1);
                CvInvoke.DrawMarker(imgResult, System.Drawing.Point.Round(MyP2), new MCvScalar(255, 128, 128, 255), MarkerTypes.Cross, 40, 1);
            }

            // We have to find BOTH in order to have a successful return
            return (bfoundup && bfounddown);
        }

        bool TestCirclePoint(ref Emgu.CV.Image<Gray, Byte> img
                             , ref System.Drawing.Point[] arrOffsets
                             , ref byte[] arrValues
                             , System.Drawing.Point testpt
                               )
        {
            // Let's see if we can get the individual pixel values from the image
            int x;
            int y;
            int j;
            int nLargestContiguous = 0;
            System.Drawing.Size imsz = img.Size;
            byte[,,] imgData = img.Data;
            int nNumOffsets = arrOffsets.Length;
            int nNumToLoop = (2 * nNumOffsets);
            int nNumForLargeFailure = (nNumOffsets * 9) / 10;
            int nNumFilled;
            int nNumForSuccess = ((nNumOffsets >> 1) * 3 / 4);

            // Is this a valid point in the edge trace?
            nNumFilled = 0;
            // obtain the pixel values in the circle centered
            // at x,y in the threshold  image.
            for (j = 0; j < nNumOffsets; j++)
            {
                System.Drawing.Point mypt = arrOffsets[j];
                mypt.X += testpt.X;
                mypt.Y += testpt.Y;

                // Get right up to the boundaries
                if ((mypt.X < 0) || (mypt.Y < 0) || (mypt.X >= imsz.Width) || (mypt.Y >= imsz.Height))
                    arrValues[j] = 0;
                else
                {
                    arrValues[j] = imgData[mypt.Y, mypt.X, 0];
                    if (arrValues[j] != 0)
                        nNumFilled++;
                }
            }

            // now, test the array to see if there are nNumOffsets/2
            // contiguously set points.  We will need to loop around
            // 360 + 90 degrees to get an accurate result.  This 
            // is 5/4*nNumOffsets.
            int nNumContiguous = 0;
            byte nFillValue = 0;
            nLargestContiguous = 0;

            //int nNonZero = CvInvoke.CountNonZero(img);

            for (j = 0;
                    (j < nNumToLoop); //&& (nNumContiguous < nNumForSuccess) ;
                    j++)
            {
                // If we have a white pixel, then increment the 
                // contiguous count.  Otherwise, reset it to zero.
                if (arrValues[j % nNumOffsets] != 0)
                {
                    nNumContiguous++;
                    if (nLargestContiguous < nNumContiguous)
                    {
                        nLargestContiguous = nNumContiguous;
                        // Check to see if we really found a valid edge
                        // by drawing a line from the nLargestContiguous index to
                        // nNumForSuccess index in the arrOffsets.  If all of the
                        // pixels are nonblack, we can fill the cannyData with
                        // white.  If none can be found, it will end up black.
                        if (nLargestContiguous >= nNumForSuccess)
                        {
                        }
                    }
                }
                else
                    nNumContiguous = 0;
            }

            // If we failed, erase the test center point from the canny image.
            if ((nLargestContiguous < nNumForSuccess)
                // || (nFillValue == 0)
                || (nNumFilled > nNumForLargeFailure)
                )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    List<System.Drawing.Point> GenerateCircularOffsets(int d)
    {
        List<System.Drawing.Point> offsets = new List<System.Drawing.Point>();

        int r = (d - 1) / 2;                   // 5:  r = (5-1)/2 = 2     6:  r=(6-1)/2 = 2;
        int h = ((d - 1) & 1) == 1 ? 1 : 0;     // is it an odd or even radius?  odd=1, even=0
        int x;
        int y;
        int F_M;
        int d_e;
        int d_ne;
        int i;
        bool bTestIndex;

        int nInsertIndex = 0;

        // We generate the list in octants.  We then remove repeat occurances
        for (i = 0; i < 8; i++)
        {
            // If we are needing to reverse the order of points, set the 
            // nInsertIndex to the current size of the array at the beginning
            // of this iteration.  Odd Octant Indeces are reverse order.
            // Even Octant Indeces or normal order.
            nInsertIndex = offsets.Count;
            x = 0;
            y = -r;
            F_M = 1 - r;
            d_e = 3;
            d_ne = -(r << 1) + 5;

            // Compare the first point we're adding to see if it
            // is a duplicate to the last point.  This will only
            // be tested for odd Octant Indeces
            bTestIndex = nInsertIndex > 0 && ((i & 1) == 0);

            SetOctantArrayPoints(ref offsets, x, y, i, h, nInsertIndex, bTestIndex);

            while (x < -y)
            {
                if (F_M <= 0)
                {
                    F_M += d_e;
                }
                else
                {
                    F_M += d_ne;
                    d_ne += 2;
                    y++;
                }
                d_e += 2;
                d_ne += 2;
                x++;

                bTestIndex = false;

                if (x < -y || (i & 1) == 0)
                    bTestIndex = false;
                else
                if (x >= -y && (i & 1) == 1)
                    bTestIndex = true;

                SetOctantArrayPoints(ref offsets, x, y, i, h, nInsertIndex, bTestIndex);
            }
        }

        return offsets;
    }

    void circleMidpointFast(ref byte[,,] myimageData, int yc, int xc, int d)
    {
        int r = (d - 1) / 2;
        int h = ((d - 1) & 1) == 1 ? 1 : 0;     // is it an odd or even radius?  odd=1, even=0
        int x = r, y = 0;
        int t1 = r / 16;
        int t2;
        int i = 1;

        Debug.WriteLine("y,x,2,y,x,4,y,x,8,y,x,3,y,x,6,y,x,1,y,x,5,y,x,7");

        while (x >= y)
        {
            // draw all eight pixels
            drawCircle(ref myimageData, xc, yc, x, y, i, h);
            i++;

            y++;
            t1 = t1 + y;
            t2 = t1 - x;
            if (t2 >= 0)
            {
                t1 = t2;
                x--;
            }
        }
    }





    private void DetectEdges(bool bDoFileOpen, bool bShowDebug=false)
    {
        double dLength = Properties.Settings.Default.ResultLength;
        double dWidth = Properties.Settings.Default.ResultWidth;
        double dScaleFactor = Properties.Settings.Default.ResultScale;
        float dScaleFactorF = (float)dScaleFactor;
        string szOrgImage;
        string szFlashImage="";
        float dMedianWidthF;
        float dMedianHeightF;
        string szOutputPath = ""; 
        int i;
        int j;
        Emgu.CV.Image<Bgr, Byte> cvimgColorFlash = null;
        Emgu.CV.Image<Bgr, Byte> cvimgColorFlashSub1 = null;
        Emgu.CV.Image<Bgr, Byte> cvimgColorMul500 = null;
        Emgu.CV.Image<Bgr, Byte> cvimgColorMul375 = null;
        Emgu.CV.Image<Bgr, Byte> cvimgColorMul250 = null;
        Emgu.CV.Image<Bgr, Byte> cvimgColorMul125 = null;
        Emgu.CV.Image<Gray, Byte> cvimgThresholdAdaptive = null;


        System.Drawing.Rectangle rc2;
        
        bShowDebug = false;


        if (dLength * dWidth * dScaleFactor <= 0)
        {
            System.Windows.MessageBox.Show("Please press the Load ARuCo Images button and select a calibration image before attempting this action.");
            return;
        }


        CheckMatricesLoaded();

        if (bDoFileOpen)
        {
            Microsoft.Win32.OpenFileDialog openPic = new Microsoft.Win32.OpenFileDialog();
            openPic.Multiselect = false;
            openPic.Title = "Open Image";
            if (openPic.ShowDialog() == false)
            {
                return;
            }

            szOrgImage = openPic.FileName;
            szOutputPath = System.IO.Path.GetDirectoryName(szOrgImage) + "\\";
            szLastFileName = szOrgImage;


            openPic.Title = "Open Image With Flash";
            if (openPic.ShowDialog() == true)
            {
                szFlashImage = openPic.FileName;
            }
            else
            {
                if (System.Windows.MessageBox.Show("Do you want to try a single image processing instead of the normal dual processing?", "One Image Only?", MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.No)
                {
                    return;
                }
            }


            Title = "MainWindow - Undistort File - " + System.IO.Path.GetFileName(szLastFileName); ;
        }
        else
        {
            if (szLastFileNameFlash.Length == 0)
            {
                szOrgImage = szLastFileName;
                szOutputPath = System.IO.Path.GetDirectoryName(szOrgImage) + "\\";
                if (szOrgImage.Length == 0)
                {
                    System.Windows.MessageBox.Show("Please select a file using UndistortImage before trying to Redo");
                    return;
                }
                Title = "MainWindow - Undistort Recalculating - " + System.IO.Path.GetFileName(szLastFileName);
            }
            else
            {
                szOrgImage = szLastFileName;
                szFlashImage = szLastFileNameFlash;
                szOutputPath = System.IO.Path.GetDirectoryName(szOrgImage) + "\\";

                Title = "MainWindow - Multi Folder Undistort " + szLastFileName; 
            }
        }

        Emgu.CV.Image<Bgr, Byte> cvimgColorSrc = new Image<Bgr, Byte>(szOrgImage);

        cvimgColorSrc.Save(szOutputPath + "goofy00-original.png");
        //cvimgColorSrc = cvimgColorSrc.SmoothMedian(3);


        /**************************************************************************'
         * 
         * Undistort image to take out lens curvature
         *
         **************************************************************************/


        CameraData? cd = this.ctlCameraDataCtl.CameraIPComboBox.SelectedItem as CameraData;
            if (cd == null)
            {
                System.Windows.MessageBox.Show("A camera has not yet been selected.  Please select one and open the bitmaps again.");
                return;
            }

            System.Drawing.Rectangle rc = new System.Drawing.Rectangle(cd.ROILeft,cd.ROITop,cd.ROIRight-cd.ROILeft,cd.ROIBottom-cd.ROITop);

            Emgu.CV.Image<Bgr, Byte> cvimgTempUndistorted = UndistortBigImage<Bgr, Byte>(cvimgColorSrc, rc);
            Emgu.CV.Image<Bgr, Byte> cvimgColor = 
            cvimgColor = cvimgTempUndistorted.WarpPerspective<double>(homographyMatrix
                                                            , (int)((dLength + cd.BorderLeft + cd.BorderRight) * dScaleFactor)
                                                            , (int)((dWidth + cd.BorderTop + cd.BorderBottom) * dScaleFactor)
                                                            , Inter.Cubic
                                                            , Warp.Default
                                                            , BorderType.Default
                                                            , new Bgr(0, 0, 0));
            // if (bShowDebug)
                cvimgColor.Save(szOutputPath + "goofy00x01.png");

            Emgu.CV.Image<Gray, Byte> cvimgBWSrc = new Emgu.CV.Image<Gray, Byte>(cvimgColorSrc.Size);
            Emgu.CV.Image<Bgr, Byte> cvimgColorDiff = new Emgu.CV.Image<Bgr, Byte>(cvimgColorSrc.Size);
            Emgu.CV.Image<Gray, Byte> cvimgBWSrcDiff = new Emgu.CV.Image<Gray, Byte>(cvimgColorSrc.Size);

            if (szFlashImage.Length != 0)
            {
                cvimgColorFlash = new Image<Bgr, Byte>(szFlashImage);
                cvimgColorFlash.Save(szOutputPath + "goofy00-flash.png");
                cvimgColorFlash = UndistortBigImage<Bgr, Byte>(cvimgColorFlash, rc);
                cvimgColorFlash = cvimgColorFlash
                                        .WarpPerspective<double>(homographyMatrix
                                                , (int)((dLength + cd.BorderLeft + cd.BorderRight) * dScaleFactor)
                                                , (int)((dWidth + cd.BorderTop + cd.BorderBottom) * dScaleFactor)
                                                , Inter.Cubic
                                                , Warp.Default
                                                , BorderType.Default
                                                , new Bgr(0, 0, 0));
                // if (bShowDebug) 
                    cvimgColorFlash.Save(szOutputPath + "goofy05hh.png");
            }

            cvimgColorFlashSub1 = cvimgColorFlash.Sub(cvimgColor);


            /*******************************************************************
            * 
            * MULTIPLY SUB PICTURE
            * 
            *******************************************************************/
            cvimgColorMul500 = cvimgColor.Mul(0.5);

            // if (bShowDebug)
            {
                cvimgColorMul500.Save(szOutputPath + "goofy01x01-f500 SUB.png");
                //cvimgColorMul375 = cvimgColor.Mul(0.375);
                //cvimgColorMul375.Save(szOutputPath + "goofy01x02-f375 SUB.png");
                cvimgColorMul250 = cvimgColor.Mul(0.250);
                cvimgColorMul250.Save(szOutputPath + "goofy01x03-f250 SUB.png");
                cvimgColorMul125 = cvimgColor.Mul(0.125);
                cvimgColorMul125.Save(szOutputPath + "goofy01x04-f125 SUB.png");
            }

            cvimgColorDiff = cvimgColorFlashSub1.Sub(cvimgColorMul500);
            cvimgBWSrcDiff = cvimgColorDiff.Convert<Gray, Byte>();
            // if (bShowDebug)
                cvimgBWSrcDiff.Save(szOutputPath + "goofy01x05-d.png");

            cvimgThresholdAdaptive = cvimgColor.Convert<Gray,Byte>().Not().ThresholdAdaptive ( new Gray(255)
                                                                                             , AdaptiveThresholdType.MeanC
                                                                                             , Emgu.CV.CvEnum.ThresholdType.BinaryInv
                                                                                             , 11
                                                                                             , new Gray(2));

            Emgu.CV.Image<Gray, byte>[] cvimgContoursChannels = null;
            // if (bShowDebug) 
            cvimgContoursChannels = cvimgThresholdAdaptive.Convert<Bgra,Byte>().Split();

            Emgu.CV.Image<Gray, byte>[] cvimgColorChannels = cvimgColor.Split();
            // if (bShowDebug)
                cvimgThresholdAdaptive.SmoothMedian(11).Save(szOutputPath + "goofy01-e-MedianAdaptive.png");

            cvimgColorChannels[0] = cvimgColorChannels[0].Not().ThresholdAdaptive( new Gray(255)
                                                                                 , AdaptiveThresholdType.GaussianC
                                                                                 , Emgu.CV.CvEnum.ThresholdType.BinaryInv
                                                                                 , 11
                                                                                 , new Gray(3));
            // if (bShowDebug)
                cvimgColorChannels[0].SmoothMedian(5).Save(szOutputPath + "goofy01-f-B-MedianAdaptive.png");



            cvimgColorChannels[1] = cvimgColorChannels[1].Not().ThresholdAdaptive( new Gray(255)
                                                                                 , AdaptiveThresholdType.GaussianC
                                                                                 , Emgu.CV.CvEnum.ThresholdType.BinaryInv
                                                                                 , 11
                                                                                 , new Gray(3));
            // if (bShowDebug)
                cvimgColorChannels[1].SmoothMedian(5).Save(szOutputPath + "goofy01-f-G-MedianAdaptive.png");



            cvimgColorChannels[2] = cvimgColorChannels[2].Not().ThresholdAdaptive( new Gray(255)
                                                                                 , AdaptiveThresholdType.GaussianC
                                                                                 , Emgu.CV.CvEnum.ThresholdType.BinaryInv
                                                                                 , 11
                                                                                 , new Gray(3));
            // if (bShowDebug)
                cvimgColorChannels[2].SmoothMedian(5).Save(szOutputPath + "goofy01-f-R-MedianAdaptive.png");



            cvimgThresholdAdaptive = cvimgColorChannels[2].Or(cvimgColorChannels[1]).Or(cvimgColorChannels[0]);
            // if (bShowDebug)
                cvimgThresholdAdaptive.SmoothMedian(5).Save(szOutputPath + "goofy01-f-MergedBGR-MedianAdaptive.png");

            // if (bShowDebug)
            {
                Emgu.CV.Image<Bgr, Byte> cvimgColorAdaptive = new Emgu.CV.Image<Bgr, byte>(cvimgColorChannels);

                cvimgColorAdaptive.SmoothMedian(5).Save(szOutputPath + "goofy01-f-BGR-MedianAdaptive.png");

                cvimgThresholdAdaptive = cvimgColor.Convert<Gray, Byte>().Not().ThresholdAdaptive(new Gray(255)
                                                                                                 , AdaptiveThresholdType.GaussianC
                                                                                                 , Emgu.CV.CvEnum.ThresholdType.BinaryInv
                                                                                                 , 11
                                                                                                 , new Gray(3));
                cvimgThresholdAdaptive = cvimgThresholdAdaptive.SmoothMedian(5);
                cvimgThresholdAdaptive.Save(szOutputPath + "goofy01-f-MedianAdaptive.png");
            }

            // if (bShowDebug)
            {


                Emgu.CV.XImgproc.XImgprocInvoke.Thinning(cvimgThresholdAdaptive, cvimgThresholdAdaptive, Emgu.CV.XImgproc.ThinningTypes.GuoHall);
                cvimgThresholdAdaptive.Save(szOutputPath + "goofy01-f-Thinned.png");
            }



            cvimgThresholdAdaptive = cvimgBWSrcDiff.Not().ThresholdAdaptive(new Gray(255), AdaptiveThresholdType.MeanC, Emgu.CV.CvEnum.ThresholdType.BinaryInv, 11, new Gray(1));
            // if (bShowDebug)
            {
                cvimgThresholdAdaptive.Save(szOutputPath + "goofy01-d-Adaptive.png");
            }

            Emgu.CV.Image<Gray,Byte> cvimgThresholdAdaptiveDilate = cvimgThresholdAdaptive.Dilate(16);//.Save(szOutputPath + "goofy01-d-Adaptive Dilate 16.png");
            // if (bShowDebug) 
            {
                cvimgThresholdAdaptiveDilate.Save(szOutputPath + "goofy01-d-AdaptiveDilated.png");
            }

            Emgu.CV.Image<Gray, Byte> cvimgThresholdAdaptiveMedian = new Emgu.CV.Image<Gray,Byte>(cvimgThresholdAdaptive.Size);
            CvInvoke.MedianBlur(cvimgThresholdAdaptive, cvimgThresholdAdaptiveMedian, 5);
            // if (bShowDebug)
            {
                cvimgContoursChannels[0].SetZero();
                cvimgContoursChannels[1].SetZero();
                cvimgContoursChannels[2] = cvimgContoursChannels[3] = cvimgThresholdAdaptiveMedian;

                (new Emgu.CV.Image<Bgra, Byte>(cvimgContoursChannels)).Save(szOutputPath + "goofy01-e-Median-Red.png");
            }

            // if (bShowDebug) 
            { 
                Emgu.CV.Image<Gray, Byte> cvimgDistance = new Emgu.CV.Image<Gray, Byte>(cvimgThresholdAdaptive.Size);
                CvInvoke.DistanceTransform(cvimgThresholdAdaptiveMedian, cvimgDistance, null, DistType.L1, 3);
                cvimgDistance.Save(szOutputPath + "goofy01-d-AdaptiveDistance.png");
            }


            /*
            Emgu.CV.Image<Gray, byte> cvimgCanny2 = cvimgThresholdAdaptive.Canny(100.0, 200.0);
            cvimgCanny2.Save(szOutputPath + "goofy01x13 - Canny.png");

            VectorOfVectorOfPoint vpts = new VectorOfVectorOfPoint();
            using (VectorOfVectorOfPoint brightlencontours = new VectorOfVectorOfPoint())
            {

                Emgu.CV.Image<Gray, byte> cvimgCannyBrightLen = new Emgu.CV.Image<Gray, byte>(cvimgCanny2.Size);
                CvInvoke.FindContours(cvimgCanny2, brightlencontours, null, RetrType.Ccomp, ChainApproxMethod.ChainApproxTc89L1);
                for (j=0; j<256; j++)
                {
                    vpts.Clear();
                    for (i = 0; i < brightlencontours.Size; i++)
                    {
                        if (j < 255)
                        {
                            if (brightlencontours[i].Size == j)
                                vpts.Push(brightlencontours[i]);
                        }
                        else
                        {
                            if (brightlencontours[i].Size >= 255)
                                vpts.Push(brightlencontours[i]);
                        }
                    }
                    CvInvoke.DrawContours(cvimgCannyBrightLen, vpts, -1, new MCvScalar(j, j, j, 255), 1);
                }
                cvimgCannyBrightLen.Save(szOutputPath + "goofy01x13 - Canny Bright.png");
            }
            */
            //Emgu.CV.XImgproc.XImgprocInvoke.Thinning()

            Emgu.CV.Image<Gray, Byte> cvimgThresholdBWDiff = cvimgBWSrcDiff.ThresholdBinaryInv(new Gray(10), new Gray(255));
            // if (bShowDebug)
                cvimgThresholdBWDiff.Save(szOutputPath + "goofy01x06-ThreshDiff.png");

            int nNumRetries = 0;

            TryAnotherErode:
            Emgu.CV.Image<Gray, Byte> cvimgThresholdBWDiffEroded = cvimgThresholdBWDiff.Erode(16);

            // if (bShowDebug)
                cvimgThresholdBWDiffEroded.Save(szOutputPath + "goofy01x07-ThreshDiff Eroded by 16 - " + nNumRetries.ToString() + ".png");


            int nLargestContourInd = -1;
            int nLargestContourSize = 0;
            Emgu.CV.Image<Bgra, byte> cvimgContours = new Image<Bgra, Byte>(cvimgThresholdBWDiffEroded.Width, cvimgThresholdBWDiffEroded.Height);

            // if (bShowDebug)
                cvimgContoursChannels = cvimgContours.Split();

            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                int ew, eh;

                ew = cvimgThresholdBWDiffEroded.Width;
                eh = cvimgThresholdBWDiffEroded.Height;

                CvInvoke.FindContours(cvimgThresholdBWDiffEroded, contours, null, RetrType.Ccomp, ChainApproxMethod.ChainApproxTc89L1);

                // Find the largest object...  It will likely be the perimeter
                for (i = 0; i < contours.Size; i++)
                {
                    if ((contours[i] != null) && (Emgu.CV.CvInvoke.ArcLength(contours[i],true) > nLargestContourSize))
                    {
                        nLargestContourSize = (int)Emgu.CV.CvInvoke.ArcLength(contours[i],true);
                        nLargestContourInd = i;
                    }
                }

                // Draw the objects larger than a base size
                VectorOfVectorOfPoint rectContours = new VectorOfVectorOfPoint();
                VectorOfVectorOfPoint objectContours = new VectorOfVectorOfPoint();
                Emgu.CV.Util.VectorOfPoint approx = new VectorOfPoint();

                cvimgContours.SetZero();

                nLargestContourSize = 11700;
                for (i = 0; i < contours.Size; i++)
                {
                    double dArcLength = Emgu.CV.CvInvoke.ArcLength(contours[i], true);

                    // Do any of the points touch the extreme edge of the image?
                    VectorOfPoint singleContour = contours[i];

                    for (j = 0; j < singleContour.Size; j++)
                    {
                        if (singleContour[j].X == 0 ||
                            singleContour[j].Y == 0 ||
                            singleContour[j].X >= ew ||
                            singleContour[j].Y >= eh)
                        {
                            break;
                        }
                    }

                    if ((j==singleContour.Size) && (contours[i] != null) && (dArcLength > 2000) && (dArcLength < nLargestContourSize))
                    {
                        double epsilon = 0.04 * Emgu.CV.CvInvoke.ArcLength(contours[i], true);
                        Emgu.CV.CvInvoke.ApproxPolyDP(contours[i], approx, epsilon, true);

                        double dArea = Emgu.CV.CvInvoke.ContourArea(contours[i]);
                        if (approx.Size == 4 && dArea > 40000 && dArea < 7500000)
                        {
                            rectContours.Push(approx);
                            objectContours.Push(contours[i]);

                            // if (bShowDebug)
                                CvInvoke.DrawContours(cvimgContours, contours, i, new MCvScalar(255,255,255,255), 1);
                        }
                    }
                }


                if (rectContours.Size == 0)
                {
                    if (nNumRetries > 5)
                        return;
                    nNumRetries++;
                    cvimgThresholdBWDiff = cvimgThresholdBWDiffEroded;
                    goto TryAnotherErode;
                }



                // Save the outlines of the contours that are smaller than the biggest
                // if (bShowDebug) 
                    cvimgContours.Save(szOutputPath + "goofy01x12 - White Outlines.png");




                Emgu.CV.Image<Gray, Byte> cvimgHoughLinesGray = new Emgu.CV.Image<Gray, byte>(cvimgContours.Size);

                Emgu.CV.Image<Gray, Byte> cvimgThickWhiteOutline = cvimgContours.Convert<Gray, byte>().Dilate(5);
                // if (bShowDebug)
                {
                    cvimgContoursChannels[0] = cvimgThickWhiteOutline;
                    cvimgContoursChannels[1] = cvimgThickWhiteOutline;
                    cvimgContoursChannels[2] = cvimgThickWhiteOutline;
                    cvimgContoursChannels[3] = cvimgThickWhiteOutline;

                    cvimgContours = new Emgu.CV.Image<Bgra, Byte>(cvimgContoursChannels);
                    cvimgContours.Save(szOutputPath + "goofy01x12 - White Outlines Thick.png");
                }


                double rho = 0.5;
                double theta = Math.PI / 1440.0;
                Int32 threshold = 15;
                double min_line_length = 100;
                double max_line_gap = 10;
                Emgu.CV.Structure.LineSegment2D[][] lines;

                /*
                lines = cvimgThresholdBWDiff.HoughLinesBinary(rho, theta, threshold, min_line_length, max_line_gap);
                foreach (Emgu.CV.Structure.LineSegment2D line in lines[0])
                    cvimgHoughLinesGray.Draw(line, new Gray(255), 1);
                cvimgHoughLinesGray.Save(szOutputPath + "goofy01x12 - HoughLines Gray.png");
                cvimgHoughLinesGray.SetZero();
                */


                lines = cvimgThickWhiteOutline.HoughLinesBinary ( rho, theta, threshold, min_line_length, max_line_gap);
                foreach (Emgu.CV.Structure.LineSegment2D line in lines[0])
                    cvimgHoughLinesGray.Draw(line, new Gray(255), 1);
                cvimgHoughLinesGray = cvimgHoughLinesGray.Dilate(2);

                // if (bShowDebug)
                {
                    cvimgContoursChannels[0].SetZero();
                    cvimgContoursChannels[1] = cvimgHoughLinesGray;
                    cvimgContoursChannels[2].SetZero();
                    cvimgContoursChannels[3] = cvimgHoughLinesGray;

                    cvimgContours = new Emgu.CV.Image<Bgra, Byte>(cvimgContoursChannels);
                    cvimgContours.Save(szOutputPath + "goofy01x12 - HoughLines.png");
                }

                rho = 0.5;
                theta = Math.PI / 1440.0;
                threshold = 15;
                min_line_length = 500;
                max_line_gap = 2;




                cvimgContours.SetZero();





                // Now, go through each contour, and measure the centerpoint angles...
                // These are all rectangles, so we should have 4 corners.
                List<double> angles = new List<double>();
                VectorOfVectorOfPointF listCorners = new VectorOfVectorOfPointF();
                List<double> listAngles = new List<double>();
                List<List<double>> listCornerAngles = new List<List<double>>();
                //List<PointF> vecCorner = new List<PointF>();
                List<System.Drawing.Point> vecCorner = new List<System.Drawing.Point>();
                List<System.Drawing.Point> pttemparr = new List<System.Drawing.Point>();
                List<System.Drawing.Point> ptarr = new List<System.Drawing.Point>();
                List<List<System.Drawing.Point>> objCorners = new List<List<System.Drawing.Point>>();

                int nAngleIndDelta = 90;
                int nCornerInd = 0;
                bool bCornerStarted = false;



                // Try the Haris corner detector!
                
                Emgu.CV.Image<Gray, Byte> cvimgHarrisSource = null;
                Emgu.CV.Image<Gray, Byte> cvimgHarrisBlack = new Emgu.CV.Image<Gray, Byte>(cvimgContours.Size);
                Emgu.CV.Image<Gray, float> cvimgHarrisCorner = null;                
                Emgu.CV.Image<Gray, Byte>[] cvimgHarrisThreshold = null;
                Emgu.CV.Image<Bgr, Byte> cvimgHarrisThresholdBgr = new Emgu.CV.Image<Bgr, Byte>(cvimgContours.Size);
                Emgu.CV.Image<Bgra, Byte> cvimgHarrisThresholdBgra = null;
                Emgu.CV.Image<Gray, float> cvimgHarrisThresholdGray = new Emgu.CV.Image<Gray, float>(cvimgContours.Size);
                Emgu.CV.Image<Gray, Byte> cvimgHarrisThresholdMask = new Emgu.CV.Image<Gray, Byte>(cvimgContours.Size);

                cvimgHarrisThresholdBgra = new Emgu.CV.Image<Bgra, Byte>(cvimgContours.Size);
                cvimgHarrisThreshold = cvimgHarrisThresholdBgra.Split();
                cvimgHarrisThreshold[3].SetValue(255);

                // if (bShowDebug)
                {
                    cvimgHarrisThresholdBgra = new Emgu.CV.Image<Bgra, Byte>(cvimgHarrisThreshold);
                    cvimgHarrisThresholdBgra.Save(szOutputPath + "goofy00x00-Black.png");
                }

                /*
                cvimgHarrisThreshold[0] = new Image<Gray, Byte>(cvimgContours.Size);
                cvimgHarrisThreshold[1] = new Image<Gray, Byte>(cvimgContours.Size);
                cvimgHarrisThreshold[2] = new Image<Gray, Byte>(cvimgContours.Size);
                */

                for (i=0; i<objectContours.Size; i++)
                {
                    VectorOfPoint obj = objectContours[i];
                    double dArcLength = Emgu.CV.CvInvoke.ArcLength(obj, true);
                    List<System.Drawing.PointF> polyPts = new List<System.Drawing.PointF>();
                    int nObjSize = obj.Size;

                    nAngleIndDelta = (int)(dArcLength*0.04);


                    
                    // Draw just the object we are trying to measure so that we don't
                    // get other corners accidentally
                    cvimgHarrisSource = new Emgu.CV.Image<Gray, Byte>(cvimgContours.Size);
                    cvimgHarrisCorner = new Emgu.CV.Image<Gray, float>(cvimgContours.Size);
                    int ncnt2 = CvInvoke.CountNonZero(cvimgHarrisSource);

                    CvInvoke.FillPoly(cvimgHarrisSource, obj, new MCvScalar(255));
                    // if (bShowDebug)
                    {
                        cvimgHarrisThreshold[0] = cvimgHarrisSource.Copy();
                        cvimgHarrisThreshold[1].SetZero();
                        cvimgHarrisThreshold[2].SetZero();
                        cvimgHarrisThreshold[3] = cvimgHarrisSource.Copy();
                        cvimgHarrisThresholdBgra = new Emgu.CV.Image<Bgra, Byte>(cvimgHarrisThreshold);
                        cvimgHarrisThresholdBgra.Save(szOutputPath + "goofy01x08-Harris Source-" + i.ToString("D3") + ".png");
                    }

                    // if (bShowDebug)
                    {
                        int ncnt1 = CvInvoke.CountNonZero(cvimgHarrisSource);
                    }

                    CvInvoke.CornerHarris(cvimgHarrisSource, cvimgHarrisCorner, 7, 15, 0.04);

                    // if (bShowDebug)
                    {
                        int ncnt4 = CvInvoke.CountNonZero(cvimgHarrisSource);
                    }

                    // if (bShowDebug)
                    {
                        double[] dMinValue = new double[1];
                        double[] dMaxValue = new double[1];
                        System.Drawing.Point[] ptMinLoc;//= new System.Drawing.Point[3];
                        System.Drawing.Point[] ptMaxLoc;// = new System.Drawing.Point[3];

                        cvimgHarrisCorner.MinMax(out dMinValue, out dMaxValue, out ptMinLoc, out ptMaxLoc);
                        float dThresh = 0.1f * (float)dMaxValue[0];
                    }


                    CvInvoke.Threshold(cvimgHarrisCorner, cvimgHarrisThresholdGray, 0.0001, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                    cvimgHarrisThresholdMask = cvimgHarrisThresholdGray.Convert<Gray, Byte>();
                    // if (bShowDebug)
                    {
                        cvimgHarrisThreshold = cvimgHarrisThresholdBgra.Split();
                        cvimgHarrisThreshold[0].SetZero();
                        cvimgHarrisThreshold[1].SetZero();
                        cvimgHarrisThreshold[3] = cvimgHarrisThreshold[2] = cvimgHarrisThresholdMask;
                        cvimgHarrisThresholdBgra = (new Emgu.CV.Image<Bgra, Byte>(cvimgHarrisThreshold));
                        cvimgHarrisThresholdBgra.Save(szOutputPath + "goofy01x09-Harris Threshold Thin Gray-" + i.ToString("D3") + ".png");

                        //cvimgHarrisThresholdMask = cvimgHarrisThresholdMask.Dilate(5);
                        cvimgHarrisThreshold[0].SetZero();
                        cvimgHarrisThreshold[1].SetZero();
                        cvimgHarrisThreshold[3] = cvimgHarrisThreshold[2] = cvimgHarrisThresholdMask;
                        //                    cvimgHarrisThreshold[2].Copy(cvimgHarrisThresholdGray);
                        cvimgHarrisThreshold[2].Save(szOutputPath + "goofy01x09-Harris Threshold Gray Copy-" + i.ToString("D3") + ".png");
                        cvimgHarrisThresholdGray.Save(szOutputPath + "goofy01x09-Harris Threshold Gray-" + i.ToString("D3") + ".png");

                        cvimgHarrisThresholdBgra = (new Emgu.CV.Image<Bgra, Byte>(cvimgHarrisThreshold));
                        //cvimgHarrisCorner.Save(szOutputPath + "goofy01x09-Harris Corner\" + i.ToString("D3") + \".png");
                        cvimgHarrisThresholdBgra.Save(szOutputPath + "goofy01x10-Harris Threshold-" + i.ToString("D3") + ".png");
                    }



                    //Emgu.CV.Image<Bgr,Byte>
                    //CvInvoke.Merge(cvimgHarrisThreshold.Save(szOutputPath + "goofy01x10-Harris Threshold-" + i.ToString("D3") + ".png");


                    ptarr = new List<System.Drawing.Point>();
                    pttemparr = new List<System.Drawing.Point>();
                    angles.Clear();
                    listCornerAngles.Clear();
                    listCorners.Clear();

                    // Generate the absolute list of points.  We need to do this because
                    // the object doesn't have all of the points in between vertices.
                    for (j = 0; j < nObjSize; j++)
                    {
                        System.Drawing.PointF ptCenter = obj[j];
                        System.Drawing.PointF ptOne = obj[(j + 1) % nObjSize];
                        polyPts.AddRange(GenerateLinearOffsetsF(ptCenter, ptOne, true));
                    }

                    int nPolyPtsSize = polyPts.Count;
                    int nHalfPolyPtsSize = nPolyPtsSize / 2;

                    // Generate the angles for all of the points...
                    if (true)
                    {
                        System.Drawing.Point pt;
                        System.Drawing.Point ptOne;
                        System.Drawing.Point ptTwo;
                        System.Drawing.Point ptOneOffs = new System.Drawing.Point();
                        System.Drawing.Point ptTwoOffs = new System.Drawing.Point();
                        System.Drawing.Point ptCenterOffs = new System.Drawing.Point();

                        System.Drawing.Point ptOneSmallest = new System.Drawing.Point();
                        System.Drawing.Point ptTwoSmallest = new System.Drawing.Point();
                        System.Drawing.Point ptOneLargest = new System.Drawing.Point();
                        System.Drawing.Point ptTwoLargest = new System.Drawing.Point();
                        System.Drawing.Point ptOneNextLargest = new System.Drawing.Point();
                        System.Drawing.Point ptTwoNextLargest = new System.Drawing.Point();
                        System.Drawing.PointF ptfOne;
                        System.Drawing.PointF ptfTwo;
                        System.Drawing.PointF ptfCenter = new PointF();
                        System.Numerics.Vector2 vecSegment;
                        System.Drawing.Point ptCenter;   
                        bCornerStarted = false;
                        float dSmallestLength = 100000000;
                        float dLargestLength = 0;
                        float dNextLargestLength = 0;
                        float dLargestAngle = 0;
                        float dNextLargestAngle = 0;
                        float dCurLength = 0;
                        float dCurAngle = 360;
                        int nSmallestInd = -1;
                        int nLargestInd = -1;
                        int nNextLargestInd = -1;
                        int nSearchLargestStart = -1;
                        int nSearchLargestEnd = -1;
                        int ntempcnt;
                        int nNumAboveOne;
                        int nNumBelowOne;
                        int nNumAboveTwo;
                        int nNumBelowTwo;

                        LineSegment2DF vecOne = new LineSegment2DF();
                        LineSegment2DF vecTwo = new LineSegment2DF();
                        HashSet<IndexedPoint> uniquePoints = new HashSet<IndexedPoint>();


                        // Do a rough guesstimate of the smallest dimension..
                        for (j=0; j < nHalfPolyPtsSize; j++)
                        {
                            ptfOne = polyPts[j];
                            ptfTwo = polyPts[j + nHalfPolyPtsSize];
                            vecSegment.X = ptfTwo.X - ptfOne.X;
                            vecSegment.Y = ptfTwo.Y - ptfOne.Y;

                            dCurLength = vecSegment.Length();
                            if (dCurLength < dSmallestLength)
                            {
                                dSmallestLength = dCurLength;
                                nSmallestInd = j;
                                ptfCenter = new PointF((ptfOne.X + ptfTwo.X) / 2
                                                        , (ptfOne.Y + ptfTwo.Y) / 2);
                                vecOne = new LineSegment2DF(ptfOne, ptfTwo);
                            }
                        }


                        // Once we know the smallest dimension, get ready to do a line draw of it
                        // so the user can visualize it...  This line will also serve as the 
                        // way of knowing if we're on a vertical axis, or on a horizontal
                        if (nSmallestInd >= 0)
                        {
                            ptOneSmallest = System.Drawing.Point.Round(polyPts[nSmallestInd]);
                            ptTwoSmallest = System.Drawing.Point.Round(polyPts[(nSmallestInd + nHalfPolyPtsSize + nPolyPtsSize) % nPolyPtsSize]);
                        }

                        // Look for the harris corner detection points that
                        // intersect with the outline.  It may be that the first
                        // point is intersecting.
                        j = 0;
                        ptarr  = new List<System.Drawing.Point>();
                        pttemparr.Clear();

                        // Find the candidate points of the outline.  This will
                        // find false positives, but we will remove them by checking
                        // the angle with respect to their neighbors.  The resulting
                        // list of points traverses the outline of the object in a
                        // clockwise fashion, and are ordered as such, allowing us
                        // to confidently calculate angles of neighbors.  Any neighbor
                        // with an angle greater than 100 will be removed.
                        pt = System.Drawing.Point.Round(polyPts[(j + nPolyPtsSize) % nPolyPtsSize]);
                        while (cvimgHarrisThresholdMask.Data[pt.Y, pt.X, 0] != 0)
                        {
                            j--;
                            pt = System.Drawing.Point.Round(polyPts[(j + nPolyPtsSize) % nPolyPtsSize]);
                        }
                        vecCorner.Clear();
                        for (; j < nPolyPtsSize; j++)
                        {
                            pt = System.Drawing.Point.Round(polyPts[(j+nPolyPtsSize)%nPolyPtsSize]);

                            if (cvimgHarrisThresholdMask.Data[pt.Y, pt.X, 0] != 0)
                            {
                                bCornerStarted = true;
                                vecCorner.Add(pt);
                            }
                            else
                            if (bCornerStarted)
                            {
                                int k = vecCorner.Count / 2;

                                // This means we just passed the end of the list
                                bCornerStarted = false;
                                pt = vecCorner[k];

                                /*
                                CvInvoke.DrawMarker(cvimgContours
                                    , pt
                                    , new MCvScalar(255, 255, 0, 255)
                                    , MarkerTypes.Cross
                                    , 20);
                                */

                                // Only draw it if it ends up in the hough lines...
                                if (cvimgHoughLinesGray.Data[pt.Y, pt.X, 0]!=0)
                                    pttemparr.Add(pt);

                                vecCorner.Clear();
                            }
                        }
                        if (pttemparr.Count==0 && vecCorner.Count > 0)
                        {

                        }

                        if (pttemparr.Count>3)
                        {
                            // Calculate the distances of points with relation to each other
                            // if they match the following criteria
                            // 
                            List<System.Drawing.Point> possibleLineOneP1 = new List<System.Drawing.Point>();
                            List<System.Drawing.Point> possibleLineOneP2 = new List<System.Drawing.Point>();
                            List<System.Drawing.Point> possibleLineTwoP1 = new List<System.Drawing.Point>();
                            List<System.Drawing.Point> possibleLineTwoP2 = new List<System.Drawing.Point>();
                            Point2D p2d = new MathNet.Spatial.Euclidean.Point2D();

                            ntempcnt = pttemparr.Count;
                            int k, m;
                            int nLineOneCnt=0;
                            int nLineTwoCnt=0;
                            int nLineOneLongest = -1;
                            int nLineTwoLongest = -1;
                            double dLineOneMaxLen = 0;
                            double dLineTwoMaxLen = 0;
                            IndexedPoint ipt;
                            double angleTest, angleEdge;

                            // CIRCLE TEST
                            // Which of the points lie on an edge?  If they are on an edge,
                            // instead of a corner, don't preserve them.
                            List<System.Drawing.Point> offsets = GenerateCircularOffsets(49);
                            List<System.Drawing.Point> offsetsLinear;
                            System.Drawing.Point[] arrOffsets = offsets.ToArray();
                            int ncnt3 = CvInvoke.CountNonZero(cvimgHarrisSource);

                            System.Drawing.Size imsz = cvimgHarrisSource.Size;
                            byte[] arrValues = new byte[offsets.Count];

                            for (j = 0; j < ntempcnt; j++)
                            {
                                for (k=j+1; k<ntempcnt+j; k++)
                                {
                                    // To be added to our line segment list it must
                                    // satisfy the following:
                                    // 1.  The endpoints must form a line with one of the 
                                    //     shortest dimensions to within 0.5 degrees
                                    // 2.  The lengths of those endpoints must be the longest
                                    //     length of the set obtained.
                                    m = (k)%ntempcnt;
                                    System.Drawing.Point ptj = pttemparr[j];
                                    System.Drawing.Point ptm = pttemparr[m];
                                    PointF pfj = ptj;
                                    PointF pfm = ptm;
                                    PointF pfo = ptOneSmallest;
                                    PointF pft = ptTwoSmallest;
                                    System.Windows.Vector pvj = new System.Windows.Vector(pfj.X, pfj.Y);
                                    System.Windows.Vector pvm = new System.Windows.Vector(pfm.X, pfm.Y);
                                    System.Windows.Vector pvo = new System.Windows.Vector(pfo.X, pfo.Y);
                                    System.Windows.Vector pvt = new System.Windows.Vector(pft.X, pft.Y);
                                    System.Windows.Vector pvto = pvt - pvo;
                                    System.Windows.Vector pvmj = pvm - pvj;
                                    System.Windows.Vector pvoj = pvo - pvj;
                                    System.Windows.Vector pvmo = pvm - pvo;
                                    System.Windows.Vector pvmt = pvm - pvt;
                                    System.Windows.Vector pvtj = pvt - pvj;
                                    System.Windows.Vector phorz = new System.Windows.Vector(100, 0);

                                    bool rval;

                                    rval = TestCirclePoint(ref cvimgHarrisSource
                                                          , ref arrOffsets
                                                          , ref arrValues
                                                          , ptm);
                                    if (rval == false)
                                    {
                                        // if (bShowDebug)
                                            CvInvoke.DrawMarker(cvimgContours
                                                , ptm
                                                , new MCvScalar(0, 255, 0, 255)
                                                , MarkerTypes.Diamond
                                                , 10);

                                       uniquePoints.Add(new IndexedPoint(ptm, m));
                                    }
                                    else
                                    {
                                        // if (bShowDebug)
                                            CvInvoke.DrawMarker(cvimgContours
                                                , ptm
                                                , new MCvScalar(0, 0, 255, 255)
                                                , MarkerTypes.Square
                                                , 10);
                                    }

                                    // Edge one
                                    if (rval == false && pvoj.Length > 100 && pvmj.Length>100 && pvmo.Length>100)
                                    {
                                        angleTest = System.Windows.Vector.AngleBetween(pvmj, pvoj);
                                        if (Math.Abs(angleTest) < 0.5) // Math.Min(0.5, 500.0 / pvmj.Length))
                                        {
                                            // Check to see if this is an edge or a true corner



                                            if (pvmj.Length > dLineOneMaxLen)
                                            {
                                                nLineOneLongest = nLineOneCnt;
                                                dLineOneMaxLen = pvmj.Length;
                                            }
                                            nLineOneCnt++;
                                            possibleLineOneP1.Add(ptj);
                                            possibleLineOneP2.Add(ptm);
                                            //CvInvoke.Line(cvimgContours, pttemparr[j], pttemparr[m], new MCvScalar(255, 0, 0, 255));
                                        }
                                    }

                                    // Edge two
                                    if (rval == false && pvtj.Length > 100 && pvmj.Length > 100 && pvmt.Length > 100)
                                    {
                                        angleTest = System.Windows.Vector.AngleBetween(pvmj, pvtj);
                                        if (Math.Abs(angleTest) < 0.5) // Math.Min(0.5, 500.0 / pvmj.Length))
                                        {
                                            if (pvmj.Length > dLineTwoMaxLen)
                                            {
                                                nLineTwoLongest = nLineTwoCnt;
                                                dLineTwoMaxLen = pvmj.Length;
                                            }
                                            nLineTwoCnt++;
                                            possibleLineTwoP1.Add(ptj);
                                            possibleLineTwoP2.Add(ptm);
                                            //CvInvoke.Line(cvimgContours, pttemparr[j], pttemparr[m], new MCvScalar(0, 255, 0, 255));
                                        }
                                    }
                                }
                            }

                            // if (bShowDebug)
                            {
                                if (nLineOneCnt > 0)
                                    CvInvoke.Line(cvimgContours, possibleLineOneP1[nLineOneLongest], possibleLineOneP2[nLineOneLongest], new MCvScalar(255, 0, 0, 255));
                                if (nLineTwoCnt > 0)
                                    CvInvoke.Line(cvimgContours, possibleLineTwoP1[nLineTwoLongest], possibleLineTwoP2[nLineTwoLongest], new MCvScalar(0, 255, 0, 255));
                            }
                        }

                        // Sort the hash list by index
                        IndexedPoint[] sortedPoints = uniquePoints.ToArray();
                        Array.Sort(sortedPoints, new IndexedPointComparer());

                        ntempcnt = sortedPoints.Length;
                        // if (bShowDebug)
                        {
                            System.Drawing.Point[] polylinePts = IndexedPoint.ToArray(sortedPoints);
                            cvimgContours.DrawPolyline(polylinePts, true, new Bgra(63, 63, 63, 255));
                        }

                        // Now, loop through the pttemparr we just obtained

                        if (sortedPoints.Length > 3)
                        {
                            List<System.Drawing.Point> ptArrFinal = new List<System.Drawing.Point>();
                            List<System.Drawing.Point> ptArrTemp = new List<System.Drawing.Point>();
                            PointF pfo = ptOneSmallest;
                            PointF pft = ptTwoSmallest;
                            System.Windows.Vector pvo = new System.Windows.Vector(pfo.X, pfo.Y);
                            System.Windows.Vector pvt = new System.Windows.Vector(pft.X, pft.Y);
                            System.Windows.Vector pvto = pvt - pvo;


                            // Convert loop to line segments.  They will end up with the same order and
                            // will only  have two vertical points per polygon.  HOWEVER, the 
                            ptArrTemp.Clear();
                            ptArrFinal.Clear();

                            for (j=0; j<ntempcnt; j++)
                            {
                                PointF pfj1 = sortedPoints[j].pt;
                                PointF pfj2 = sortedPoints[(j+1)%ntempcnt].pt;
                                System.Windows.Vector pvj1 = new System.Windows.Vector(pfj1.X, pfj1.Y);
                                System.Windows.Vector pvj2 = new System.Windows.Vector(pfj2.X, pfj2.Y);
                                System.Windows.Vector pvj12 = pvj2 - pvj1;
                                double dTestAngle = Math.Abs(System.Windows.Vector.AngleBetween(pvj12, pvto));

                                if (dTestAngle > 70 && dTestAngle < 110)
                                {
                                    if (ptArrTemp.Count == 0)
                                    {
                                        ptArrTemp.Add(sortedPoints[j].pt);
                                        ptArrTemp.Add(sortedPoints[(j + 1) % ntempcnt].pt);
                                    }
                                    else
                                    {
                                        if (ptArrTemp[ptArrTemp.Count-1] == sortedPoints[j].pt)
                                            ptArrTemp.Add(sortedPoints[(j + 1) % ntempcnt].pt);
                                    }
                                }
                                else
                                {
                                    // Did we get any points added to the temporary array?
                                    // If so, they will all be in the same line, so simply
                                    // add the first and last point of the temp arr to the
                                    // final point arr.
                                    if (ptArrTemp.Count > 0)
                                    {
                                        ptArrFinal.Add(ptArrTemp[0]);
                                        ptArrFinal.Add(ptArrTemp[ptArrTemp.Count - 1]);
                                        ptArrTemp.Clear();
                                    }
                                }

                            }
                            if (ptArrTemp.Count > 0)
                            {
                                ptArrFinal.Add(ptArrTemp[0]);
                                ptArrFinal.Add(ptArrTemp[ptArrTemp.Count - 1]);
                                ptArrTemp.Clear();
                            }
                            ntempcnt = ptArrFinal.Count;




                            if (ntempcnt == 4 && bShowDebug==true)
                            {
                                ptarr.Clear();
                                ptarr.AddRange(ptArrFinal);

                                // No complex logic if we only have 4 points.  Just loop through
                                for (j = 0; j < ntempcnt; j++)
                                {
                                    CvInvoke.DrawMarker(cvimgContours
                                                        , ptarr[j]
                                                        , new MCvScalar(255, 0, 255, 255)
                                                        , MarkerTypes.Cross
                                                        , 50);
                                }

                            }
                            else
                            {

                                ntempcnt = sortedPoints.Length;
                                // If we have more than 4 points in the list, we need to figure
                                // out which ones are outliers.  To do this, we will need to 
                                // see how many coordinates are returned from the left and 
                                // right sides of the line segment.  We will add a few pixels
                                // distance in between  for good measure 
                                for (j = 0; j < ntempcnt; j++)
                                {
                                    ptCenter = sortedPoints[(j - 1 + ntempcnt) % ntempcnt].pt;
                                    ptOne = sortedPoints[j].pt;
                                    ptTwo = sortedPoints[(j + 1) % ntempcnt].pt;

                                    // What is the offset?
                                    Emgu.CV.Structure.LineSegment2D segOne = new Emgu.CV.Structure.LineSegment2D(ptCenter, ptOne);
                                    Emgu.CV.Structure.LineSegment2D segTwo = new Emgu.CV.Structure.LineSegment2D(ptCenter, ptTwo);

                                    // if (bShowDebug)
                                    {
                                        PointF segOneDir = segOne.Direction;
                                        PointF segTwoDir = segTwo.Direction;

                                        // Get the count of filled pixels above seg one
                                        nNumAboveOne = CountFilledGrayPoints(ref cvimgHarrisSource
                                                                          , ptOne.X + (int)(segOneDir.Y * 3.0)
                                                                          , ptOne.Y - (int)(segOneDir.X * 3.0)
                                                                          , ptCenter.X + (int)(segOneDir.Y * 3.0)
                                                                          , ptCenter.Y - (int)(segOneDir.X * 3.0)
                                                                            );

                                        // Get the count of filled pixels below seg one
                                        nNumBelowOne = CountFilledGrayPoints(ref cvimgHarrisSource
                                                                          , ptOne.X - (int)(segOneDir.Y * 3.0)
                                                                          , ptOne.Y + (int)(segOneDir.X * 3.0)
                                                                          , ptCenter.X - (int)(segOneDir.Y * 3.0)
                                                                          , ptCenter.Y + (int)(segOneDir.X * 3.0)
                                                                            );


                                        // Get the count of filled pixels above seg two
                                        nNumAboveTwo = CountFilledGrayPoints(ref cvimgHarrisSource
                                                                          , ptTwo.X + (int)(segTwoDir.Y * 3.0)
                                                                          , ptTwo.Y - (int)(segTwoDir.X * 3.0)
                                                                          , ptCenter.X + (int)(segTwoDir.Y * 3.0)
                                                                          , ptCenter.Y - (int)(segTwoDir.X * 3.0)
                                                                            );

                                        // Get the count of filled pixels below seg two
                                        nNumBelowTwo = CountFilledGrayPoints(ref cvimgHarrisSource
                                                                          , ptTwo.X - (int)(segTwoDir.Y * 3.0)
                                                                          , ptTwo.Y + (int)(segTwoDir.X * 3.0)
                                                                          , ptCenter.X - (int)(segTwoDir.Y * 3.0)
                                                                          , ptCenter.Y + (int)(segTwoDir.X * 3.0)
                                                                            );

                                        if (nNumBelowTwo + nNumAboveTwo > nNumAboveOne + nNumBelowOne)
                                        {

                                        }
                                        else
                                        {

                                        }

                                    }

                                    double dAngle = Math.Abs(segOne.GetExteriorAngleDegree(segTwo));
                                    if (dAngle < 110.0)
                                    {
                                        ptarr.Add(ptCenter);
                                        // if (bShowDebug)
                                        {
                                            CvInvoke.DrawMarker(cvimgContours
                                                                    , ptCenter
                                                                    , new MCvScalar(255, 0, 255, 255)
                                                                    , MarkerTypes.Cross
                                                                    , 20);
                                        }
                                    }
                                }
                            }
                        }


                        // if (bShowDebug)
                        {
                            CvInvoke.DrawMarker(cvimgContours
                            , ptOneSmallest
                            , new MCvScalar(255, 0, 255, 255)
                            , MarkerTypes.Cross
                            , 50
                            , 3);
                            CvInvoke.DrawMarker(cvimgContours
                            , ptTwoSmallest
                            , new MCvScalar(255, 0, 255, 255)
                            , MarkerTypes.Cross
                            , 50
                            , 3);
                            CvInvoke.Line(cvimgContours, ptOneSmallest, ptTwoSmallest, new MCvScalar(255,0,255,255));
                        }
                    }
                    else
                    {
                        for (j = 0; j < nPolyPtsSize; j++)
                        {
                            int k;

                            System.Drawing.PointF ptOne;
                            System.Drawing.PointF ptTwo;
                            System.Drawing.PointF ptCenter = polyPts[j];

                            k = 0;

                            // Only look at an endpoint IF it is within the Hough boundary
                            do
                            {
                                ptOne = polyPts[(j + k + nAngleIndDelta) % nPolyPtsSize];
                                k++;
                            } while (cvimgHoughLinesGray.Data[(int)ptOne.Y, (int)ptOne.X, 0] == 0);

                            k = 0;
                            do
                            {
                                ptTwo = polyPts[(j + k - nAngleIndDelta + nPolyPtsSize) % nPolyPtsSize];
                                k--;
                            } while (cvimgHoughLinesGray.Data[(int)ptTwo.Y, (int)ptTwo.X, 0] == 0);

                            // Now, create the segments and check the exterior angle in degrees
                            LineSegment2DF vecOne = new LineSegment2DF(ptCenter, ptOne);
                            LineSegment2DF vecTwo = new LineSegment2DF(ptCenter, ptTwo);
                            angles.Add(vecOne.GetExteriorAngleDegree(vecTwo));
                            Console.WriteLine(j.ToString("D5") + "  " + angles[j].ToString());
                        }

                        for (j = 0; j < angles.Count; j++)
                        {
                            System.Drawing.PointF ptCenter = polyPts[j];

                            // Possible corners
                            if ((cvimgHarrisThresholdMask.Data[(int)ptCenter.Y, (int)ptCenter.X, 0] != 0))// && Math.Abs(angles[j]) <= 110.0)
                            {
                                //CvInvoke.DrawMarker(cvimgContours, System.Drawing.Point.Round(polyPts[j]), new MCvScalar(0, 255, 0, 255), MarkerTypes.Cross, 10);
                                listAngles.Add(Math.Abs(angles[j]));
                                bCornerStarted = true;
                                //vecCorner.Add(polyPts[j]);
                            }
                            else
                            if (bCornerStarted)
                            {
                                int k;
                                // We only get here if we already one or more points
                                bCornerStarted = false;

                                // Loop around to see if the angle we added is 
                                // sitting on an edge of a test area.  If it is,
                                // then we shouldn't add it to the list.
                                k = vecCorner.Count / 2;
                                if (listAngles[k] < 100)
                                {
                                    //listCorners.Push(new VectorOfPointF(vecCorner.ToArray()));
                                    listCornerAngles.Add(listAngles);
                                }
                                listAngles = new List<double>();
                                vecCorner.Clear();
                            }
                        }

                        for (j = 0; j < listCornerAngles.Count; j++)
                        {
                            int k;
                            int nSmallestInd = 0;
                            double dSmallestAngle = 360;
                            List<double> listCornerAnglesElement = listCornerAngles[j];
                            int nCnt = listCornerAnglesElement.Count - 1;

                            k = nCnt / 2;

                            System.Drawing.Point pt = new System.Drawing.Point();
                            ptarr.Add(System.Drawing.Point.Round(listCorners[j][k]));
                            CvInvoke.DrawMarker(cvimgContours, System.Drawing.Point.Round(listCorners[j][k]), new MCvScalar(255, 0, 255, 255), MarkerTypes.Cross, 20);
                        }

                        // How many corner angles do we have?  If it's over 4, we need to cull a few of them.
                        // If it's under 4, we need to look at the harris corners.  It's unlikely to be under 4.
                        if (ptarr.Count > 4)
                        {
                            // We want to figure out the lengths of the rectangle edges by sorting them.
                            // the shortest lengths will get deleted, but we need to make sure 
                            for (j = 0; j < ptarr.Count; j++)
                            {

                            }
                        }
                    }



                    //cvimgContours.DrawPolyline(ptarr.ToArray(), true, new Bgra(0, 0, 255, 255));
                    bCornerStarted = false;
                    if (ptarr.Count == 4)
                    {
                        objCorners.Add(ptarr);
                    }

                    /*
                    for (j = 0; j < angles.Count; j++)
                    {
                        System.Drawing.Point pt = System.Drawing.Point.Round(polyPts[j]);

                        // Possible corners
                        if (cvimgHarrisThresholdMask.Data[pt.Y, pt.X, 0] != 0 && Math.Abs(angles[j]) <= 110.0)
                        {
                            bCornerStarted = true;
                            CvInvoke.DrawMarker(cvimgContours, pt, new MCvScalar(0, 255, 255, 255), MarkerTypes.Square, 3);
                        }
                        else
                        if (bCornerStarted)
                            bCornerStarted = false;
                    }

                    objCorners.Add(ptarr);
                    */
                }

                // if (bShowDebug)
                {
                    cvimgHarrisThreshold[0].SetZero();
                    cvimgHarrisThreshold[1].SetZero();
                    cvimgHarrisThreshold[2].SetZero();
                    cvimgHarrisThreshold[3] = cvimgHarrisThreshold[2] = cvimgHarrisThresholdGray.Convert<Gray, Byte>();

                    cvimgContours.Save(szOutputPath + "goofy01x11 - Found Contours Denoised.png");
                }

                //CvInvoke.DrawContours(cvimgContours, contours, nLargestContourInd, new MCvScalar(255,0,0, 255), 1);
                //cvimgContours = cvimgContours.Erode(3);


                // OK!  Using the corners we've found, along with adaptive threshold, we'll be able 
                // measure the approximate lengths.  To do this, we'll simply loop from corner to
                // corner and get a list of widths, and a list of heights.  Whatever the measurement
                // obtained, we'll sort the list, and then take the middle point.  That should give
                // us a reasonable measurement...  Don't use the Canny version... use the threshold

                // To do the intersection, we generate an up and a down linear set of pixel offsets
                // Then, we generate the linear pixel set for the edge of the rectangle.  We loop
                // through all of the edge points.  For each edge point, we use the up and down
                // linear sets to figure out the point that is filled in that direction.  The
                // first filled point gets picked.
                for (i=0; i<objCorners.Count; i++)
                {
                    System.Drawing.Point corner1 = objCorners[i][0];
                    System.Drawing.Point corner2 = objCorners[i][1];
                    System.Drawing.Point corner3 = objCorners[i][2];
                    System.Drawing.Point corner4 = objCorners[i][3];
                    System.Drawing.Point cornersCenter = (corner1
                                                        + (System.Drawing.Size)corner2
                                                        + (System.Drawing.Size)corner3
                                                        + (System.Drawing.Size)corner4
                                                          );
                    cornersCenter.X /= 4;
                    cornersCenter.Y /= 4;
                    System.Drawing.Point cornersCenterOrg = cornersCenter;

                    Point2D ptCorner1 = new Point2D(corner1.X, corner1.Y);
                    Point2D ptCorner2 = new Point2D(corner2.X, corner2.Y);
                    Point2D ptCorner3 = new Point2D(corner3.X, corner3.Y);
                    Point2D ptOrigin = new Point2D(0, 0);
                    PointF rval1 = new PointF(0, 0);
                    PointF rval2 = new PointF(0, 0);

                    Line2D line2DHorz = new Line2D(ptCorner1, ptCorner2);
                    Line2D line2DVert = new Line2D(ptCorner2, ptCorner3);

                    Angle by90 = Angle.FromDegrees(90);
                    Vector2D angleUp   = line2DVert.Direction*(100+line2DVert.Length);
                    Vector2D angleDown = line2DVert.Direction*(-line2DVert.Length-100);
                    Vector2D horzDir = line2DHorz.Direction * line2DHorz.Length;
                    Emgu.CV.Structure.LineSegment2D curline = new Emgu.CV.Structure.LineSegment2D();

                    List<System.Drawing.PointF> listUp   = GenerateLinearOffsets2D(ptOrigin, ptOrigin + angleUp);
                    List<System.Drawing.PointF> listDown = GenerateLinearOffsets2D(ptOrigin, ptOrigin + angleDown);
                    List<System.Drawing.PointF> listHorz = GenerateLinearOffsets2D(ptCorner1, ptCorner2);
                    List<IndexedLineSegment> listHorzLines = new List<IndexedLineSegment>();
                    List<IndexedLineSegment> listVertLines = new List<IndexedLineSegment>();
                    List<IndexedLineSegment> listrval = new List<IndexedLineSegment>();
                    IndexedLineSegment lineseghrval = new IndexedLineSegment();
                    IndexedLineSegment linesegvrval = new IndexedLineSegment();

                    DistanceToFilledPixel(ref cvimgThresholdAdaptiveMedian
                                             , ref cvimgContours
                                             , ref listHorz
                                             , ref listUp
                                             , ref listDown
                                             , ref listHorzLines
                                             , ref lineseghrval
                                             , new MCvScalar(255, 127, 255, 255));

                    // Draw the returned linesegrval
                    //if (linesegrval != null)
                    //{
                    //    CvInvoke.Line ( cvimgContours
                    //                  , System.Drawing.Point.Round(linesegrval.linesegment.P1)
                    //                  , System.Drawing.Point.Round(linesegrval.linesegment.P2)
                    //                  , new MCvScalar(255, 127, 255, 255));
                    //}






                    angleUp = line2DHorz.Direction * (100 + line2DHorz.Length);
                    angleDown = line2DHorz.Direction * (-line2DHorz.Length-100);
                    Vector2D vertDir = line2DVert.Direction * line2DVert.Length;

                    listUp = GenerateLinearOffsets2D(ptOrigin, ptOrigin + angleUp);
                    listDown = GenerateLinearOffsets2D(ptOrigin, ptOrigin + angleDown);
                    List<System.Drawing.PointF> listVert = GenerateLinearOffsets2D(ptCorner2, ptCorner3);

                    DistanceToFilledPixel(ref cvimgThresholdAdaptiveMedian
                                             , ref cvimgContours
                                             , ref listVert
                                             , ref listUp
                                             , ref listDown
                                             , ref listVertLines
                                             , ref linesegvrval
                                             , new MCvScalar(127, 255, 255, 255));


                    //cvimgContours.Save(szOutputPath + "goofy01x12 - Is This It " + i.ToString("DD") + ".png"); ;


                    // Draw the returned linesegrval
                    //if (linesegrval != null)
                    //{
                    //    CvInvoke.Line(cvimgContours
                    //                  , System.Drawing.Point.Round(linesegrval.linesegment.P1)
                    //                  , System.Drawing.Point.Round(linesegrval.linesegment.P2)
                    //                  , new MCvScalar(127, 255, 255, 255));
                    //}

                    string tstr;
                    System.Drawing.Size size;
                    int nBaseLine;

                    if (lineseghrval.linesegment.Length > linesegvrval.linesegment.Length)
                        tstr = "W="
                                + (lineseghrval.linesegment.Length / dScaleFactor).ToString("0.00")
                                + "\nH="
                                + (linesegvrval.linesegment.Length / dScaleFactor).ToString("0.00");
                    else
                        tstr = "W="
                                + (linesegvrval.linesegment.Length / dScaleFactor).ToString("0.00")
                                + "\nH="
                                + (lineseghrval.linesegment.Length / dScaleFactor).ToString("0.00");

                    nBaseLine = 0;
                    System.Drawing.Size sz = Emgu.CV.CvInvoke.GetTextSize(tstr.Split("\n", 2)[0], FontFace.HersheyPlain, 6, 3, ref nBaseLine);
                    size = new System.Drawing.Size() - sz;
                    size.Width /= 2;

                    foreach (string sstr in tstr.Split("\n", 2))
                    {
                        Emgu.CV.CvInvoke.PutText(cvimgContours, sstr, cornersCenter + size, FontFace.HersheyPlain, 6, new MCvScalar(255, 255, 255, 255), 60);
                        Emgu.CV.CvInvoke.PutText(cvimgContours, sstr, cornersCenter + size, FontFace.HersheyPlain, 6, new MCvScalar(0, 0, 0, 255), 3);
                        cornersCenter.Y += 100;
                    }


                    // if (bShowDebug)
                    {
                        curline.P1 = corner1;
                        curline.P2 = corner2;
                        cvimgContours.Draw(curline, new Bgra(255, 128, 128, 255), 1);

                        curline.P1 = corner2;
                        curline.P2 = corner3;
                        cvimgContours.Draw(curline, new Bgra(255, 128, 128, 255), 1);
                    }

                    // We will draw the arrows and text
                    //cvimgColorChannels = cvimgColor.Split();
                    //cvimgContoursChannels = cvimgContours.Split();


                    // Find the intersection in the up direction
                }


                cvimgContours.Save(szOutputPath + "goofy01x12 - Is This It.png");

                return;

                /*
                int nNumIterations = int.Parse(ctlCameraDataCtl.NumIterations.Text);
                int nSegmentLength = int.Parse(ctlCameraDataCtl.SegmentLength.Text);
                int nColorRadius = int.Parse(ctlCameraDataCtl.ColorRadius.Text);
                int nSpatialRadius = int.Parse(ctlCameraDataCtl.SpatialRadius.Text);

                //Emgu.CV.Image<Bgra, Byte> cvimgColorBgra = cvimgColorDiff.Convert<Bgra, Byte>();
                Emgu.CV.Image<Bgra, Byte> cvimgColorBgra = cvimgColor.Convert<Bgra, Byte>().SmoothMedian(3);//   new Image<Bgra, Byte>(szOrgImage).SmoothMedian(3);
                CudaImage<Bgra, byte> cudaImage = new CudaImage<Bgra, byte>(cvimgColorBgra);
                Emgu.CV.Image<Bgra, Byte> cvimgOutput = new Image<Bgra, Byte>(cvimgColorDiff.Width, cvimgColorDiff.Height);
                CudaInvoke.MeanShiftSegmentation(cudaImage, cvimgOutput, nSpatialRadius, nColorRadius, nSegmentLength, new MCvTermCriteria(nNumIterations), null);
                cvimgContoursChannels = cvimgOutput.Split();
                cvimgContoursChannels[0] 
                    = cvimgContoursChannels[1] 
                    = cvimgContoursChannels[3] = cvimgContoursChannels[0].Canny(100, 200)
                                                    .Or(cvimgContoursChannels[1].Canny(100, 200)
                                                    .Or(cvimgContoursChannels[2].Canny(100, 200)));
                cvimgContoursChannels[2].SetZero();
                (new Emgu.CV.Image<Bgra, Byte>(cvimgContoursChannels)).Save(szOutputPath + "goofy01-e-ColorCanny-MeanShift-Aqua.png");

                cvimgOutput.Save(szOutputPath + "goofy01x20-MeanShift.png");

                cvimgColorBgra = cvimgColorDiff.Convert<Bgra, Byte>();
                cudaImage = new CudaImage<Bgra, byte>(cvimgColorBgra);
                cvimgOutput = new Image<Bgra, Byte>(cvimgColorDiff.Width, cvimgColorDiff.Height);
                CudaInvoke.MeanShiftSegmentation(cudaImage, cvimgOutput, nSpatialRadius, nColorRadius, nSegmentLength, new MCvTermCriteria(nNumIterations), null);
                cvimgOutput.Save(szOutputPath + "goofy01x21-MeanShift-Negative.png");*/



                int nNumFoundCont = 0;

                Emgu.CV.Image<Gray, byte>  cvimgCanny = cvimgThresholdAdaptive.Canny(100.0, 200.0);
                cvimgCanny.Save(szOutputPath + "goofy01x13 - Canny.png");

                Emgu.CV.Image<Gray, Byte> cvimgCannyMask = new Emgu.CV.Image<Gray, Byte>(cvimgCanny.Size);
                // Get othe objects that match, and create a mask
                for (i = 0; i < contours.Size; i++)
                {
                    double epsilon = 0.04 * Emgu.CV.CvInvoke.ArcLength(contours[i], true);
                    Emgu.CV.CvInvoke.ApproxPolyDP(contours[i], approx, epsilon, true);

                    double dArea = Emgu.CV.CvInvoke.ContourArea(contours[i]);
                    double dSize = Emgu.CV.CvInvoke.ArcLength(contours[i], true);
                    if (approx.Size == 4 && dArea > 40000 && dArea < 7500000)
                    {
                        CvInvoke.FillPoly(cvimgCannyMask, contours[i], new MCvScalar(255));
                    }
                }
                cvimgCannyMask = cvimgCannyMask.Dilate(24);
                cvimgCannyMask.Save(szOutputPath + "goofy01x14 - dilate mask.png");

                Emgu.CV.Image<Gray, byte> cvimgCannyObjectOutlines = cvimgCanny.And(cvimgCannyMask);
                cvimgCannyObjectOutlines.Save(szOutputPath + "goofy01x15 - canny mask object outlines.png");

                Emgu.CV.Image<Bgr,Byte> cvimgThresholdAdaptiveBgr = cvimgThresholdAdaptive.Convert<Bgr,Byte>();
                for (i = 0; i < contours.Size; i++)
                {
                    double epsilon = 0.04 * Emgu.CV.CvInvoke.ArcLength(contours[i], true);
                    Emgu.CV.CvInvoke.ApproxPolyDP(contours[i], approx, epsilon, true);

                    double dArea = Emgu.CV.CvInvoke.ContourArea(contours[i]);
                    Boolean bAreaIsConvex = Emgu.CV.CvInvoke.IsContourConvex(approx);

                    // What we have MAY be a rectangle, but we need to make sure
                    // that the lines are parallel and 90 degrees.  Take
                    // the isolated rectangle, and .
                    if (approx.Size == 4
                        && dArea > 40000
                           )
                    {
                        if (dArea < 7500000)
                        {
                            CvInvoke.DrawContours(cvimgThresholdAdaptiveBgr, contours, i, new MCvScalar(0,0,255), 1);
                            rectContours.Push(approx);
                            objectContours.Push(contours[i]);

                            RotatedRect rrc = CvInvoke.MinAreaRect(contours[i]);
                            System.Drawing.Point[] vertices = Array.ConvertAll(rrc.GetVertices(), System.Drawing.Point.Round  );

                            cvimgThresholdAdaptiveBgr.DrawPolyline(vertices, true, new Bgr(0, 255, 255));

                            CvInvoke.DrawContours(cvimgThresholdAdaptiveBgr, rectContours, nNumFoundCont, new MCvScalar(255, 0, 0), 1);
                            nNumFoundCont++;
                        }
                    }
                }

                cvimgThresholdAdaptiveBgr.Save(szOutputPath + "goofy01x16 - MinAreaContour.png");


                Emgu.CV.Image<Bgr, Byte> cvimgGreen = new Emgu.CV.Image<Bgr,Byte>(cvimgThresholdAdaptive.Size);
                cvimgGreen.SetValue(new Bgr(0, 255, 0));
                cvimgThresholdAdaptiveBgr = cvimgThresholdAdaptiveBgr.Or(cvimgGreen, cvimgCannyObjectOutlines);

                cvimgThresholdAdaptiveBgr.Save(szOutputPath + "goofy01x17 - MinAreaContour.png");

                Emgu.CV.Image<Bgr,Byte> cvimgHoughLinesBgr = new Emgu.CV.Image<Bgr,byte>(cvimgThresholdAdaptiveBgr.Size);

                /*
                double rho = 0.5;
                double theta = Math.PI / 1440.0;
                Int32 threshold = 15;
                double min_line_length = 80;
                double max_line_gap = 20;

                Emgu.CV.Structure.LineSegment2D[][] lines = cvimgCannyObjectOutlines.HoughLinesBinary(rho, theta, threshold, min_line_length, max_line_gap);
                foreach (Emgu.CV.Structure.LineSegment2D line in lines[0])
                    cvimgHoughLinesBgr.Draw(line, new Bgr(0, 255, 0), 1);
                cvimgHoughLinesBgr.Save(szOutputPath + "goofy01x18 - HoughLines.png");
                */

                /*
                for (i = 0; i < rectContours.Size; i++)
                {
                    cvimgContours.SetZero();
                    CvInvoke.DrawContours(cvimgContours, rectContours, i, new MCvScalar(255), 1);
                    //cvimgContours = cvimgContours.Dilate(2); // makes sure we can get lines that are
                    // long enough.
                    cvimgContours.Save(szOutputPath + "goofy01x19 - Contour " + i.ToString("D3") + ".png");


                    // This is a rect contour, but we can probably maximize the area by moving the endpoints, 
                    // so what we'll do is move the endpoints forward until the area starts decreasing, stop 
                    // and try again...


                    
                    double rho = 0.5;
                    double theta = Math.PI / 1440.0;
                    Int32 threshold = 15;
                    double min_line_length = 160;
                    double max_line_gap = 100;

                    LineSegment2D[][] lines = cvimgCanny.HoughLinesBinary(rho, theta, threshold, min_line_length, max_line_gap);
                    
                }*/


            }







            return;

            /*
            int nNumIterations = int.Parse(ctlCameraDataCtl.NumIterations.Text);
            int nSegmentLength = int.Parse(ctlCameraDataCtl.SegmentLength.Text);
            int nColorRadius = int.Parse(ctlCameraDataCtl.ColorRadius.Text);
            int nSpatialRadius = int.Parse(ctlCameraDataCtl.SpatialRadius.Text);

            //Emgu.CV.Image<Bgra, Byte> cvimgColorBgra = cvimgColorDiff.Convert<Bgra, Byte>();
            Emgu.CV.Image<Bgra, Byte> cvimgColorBgra = new Image<Bgra, Byte>(szOrgImage).SmoothMedian(3);
            CudaImage<Bgra, byte> cudaImage = new CudaImage<Bgra, byte>(cvimgColorBgra);
            Emgu.CV.Image<Bgra, Byte> cvimgOutput = new Image<Bgra, Byte>(cvimgColorDiff.Width, cvimgColorDiff.Height);
            CudaInvoke.MeanShiftSegmentation(cudaImage, cvimgOutput, nSpatialRadius, nColorRadius, nSegmentLength, new MCvTermCriteria(nNumIterations), null);
            cvimgOutput.Save(szOutputPath + "goofy01x20-MeanShift.png");

            cvimgColorBgra = cvimgColorDiff.Convert<Bgra, Byte>();
            cudaImage = new CudaImage<Bgra, byte>(cvimgColorBgra);
            cvimgOutput = new Image<Bgra, Byte>(cvimgColorDiff.Width, cvimgColorDiff.Height);
            CudaInvoke.MeanShiftSegmentation(cudaImage, cvimgOutput, nSpatialRadius, nColorRadius, nSegmentLength, new MCvTermCriteria(nNumIterations), null);
            cvimgOutput.Save(szOutputPath + "goofy01x21-MeanShift-Negative.png");
            */











            Emgu.CV.Image<Bgr, Byte> cvimgUndistortedColor = UndistortBigImage<Bgr, Byte>(cvimgColorDiff, rc);
            cvimgUndistortedColor.Save(szOutputPath + "goofy01-e.png");
            //EraseDrawingBorders<Bgr, byte>(ref cvimgUndistortedColor, 1);
            cvimgUndistortedColor.Save(szOutputPath + "goofy02.png");

            /**************************************************************************'
             * 
             * Warp image to take out perspective distortion.
             *
             **************************************************************************/

            cvimgColor = cvimgUndistortedColor;
            cvimgColor = cvimgColor.WarpPerspective<double>(homographyMatrix
                                                            , (int)((dLength + cd.BorderLeft + cd.BorderRight) * dScaleFactor)
                                                            , (int)((dWidth + cd.BorderTop + cd.BorderBottom) * dScaleFactor)
                                                            , Inter.Cubic
                                                            , Warp.Default
                                                            , BorderType.Default
                                                            , new Bgr(0, 0, 0));
            cvimgColor.Save(szOutputPath + "goofy05fh.png");
            if (cvimgColorFlash != null)
            {
                cvimgColorFlash = UndistortBigImage<Bgr, Byte>(cvimgColorFlash, rc);

                cvimgColorFlash = cvimgColorFlash
                                        .WarpPerspective<double>(homographyMatrix
                                                , (int)((dLength + cd.BorderLeft + cd.BorderRight) * dScaleFactor)
                                                , (int)((dWidth + cd.BorderTop + cd.BorderBottom) * dScaleFactor)
                                                , Inter.Cubic
                                                , Warp.Default
                                                , BorderType.Default
                                                , new Bgr(0, 0, 0));
                //EraseDrawingBorders<Bgr, byte>(ref cvimgColorFlash, 1);
                cvimgColorFlash.Save(szOutputPath + "goofy05hh.png");
            }
            if (cvimgTempUndistorted != null)
            {
                cvimgTempUndistorted.Save(szOutputPath + "goofy05hhh.png");
                cvimgTempUndistorted = UndistortBigImage<Bgr, Byte>(cvimgTempUndistorted, rc);

                cvimgTempUndistorted.Save(szOutputPath + "goofy05hhhh.png");

                cvimgTempUndistorted = cvimgTempUndistorted
                                        .WarpPerspective<double>(homographyMatrix
                                                , (int)((dLength + cd.BorderLeft + cd.BorderRight) * dScaleFactor)
                                                , (int)((dWidth + cd.BorderTop + cd.BorderBottom) * dScaleFactor)
                                                , Inter.Cubic
                                                , Warp.Default
                                                , BorderType.Default
                                                , new Bgr(0, 0, 0));
                cvimgTempUndistorted.Save(szOutputPath + "goofy05hhhhh.png");
            }


            int smallw = cvimgColor.Width;
            int smallh = cvimgColor.Height;


            cvimgColor.Save(szOutputPath + "goofy04.png");



            Emgu.CV.Image<Gray, Byte> cvimgBW = cvimgColor.Convert<Gray, Byte>();
            //Emgu.CV.Image<Bgr, Byte> cvimgColorCopy = cvimgMedianFilter.Copy();
            cvimgBW.Save(szOutputPath + "goofy05c.png");



            /*******************************************************************
             * 
             * THRESHOLD BINARY INVERT
             * 
             *******************************************************************/

            Emgu.CV.Image<Gray, Byte> cvimgThreshold = cvimgBW.ThresholdBinaryInv(new Gray(1), new Gray(255));
            cvimgThreshold.Save(szOutputPath + "goofy05d - Threshold 001.png");
            cvimgThreshold.Erode(5).Save(szOutputPath + "goofy05d - Threshold Erode 001.png");

            cvimgThreshold = cvimgBW.ThresholdBinaryInv(new Gray(5), new Gray(255));
            cvimgThreshold.Save(szOutputPath + "goofy05d - Threshold 005.png");
            cvimgThreshold.Erode(5).Save(szOutputPath + "goofy05d - Threshold Erode 005.png");

            cvimgThreshold = cvimgBW.ThresholdBinaryInv(new Gray(2), new Gray(255));
            cvimgThreshold.Save(szOutputPath + "goofy05d - Threshold 002.png");
            cvimgThreshold.Erode(5).Save(szOutputPath + "goofy05d - Threshold Erode 002.png");

            cvimgThreshold = cvimgBW.ThresholdBinaryInv(new Gray(10), new Gray(255));
            cvimgThreshold.Save(szOutputPath + "goofy05d - Threshold 010.png");
            cvimgThreshold.Erode(8).Convert<Bgr,Byte>().Save(szOutputPath + "goofy05d - Threshold Erode 010.png");
            cvimgThreshold.Erode(8).Dilate(8).Convert<Bgr, Byte>().Save(szOutputPath + "goofy05d - Threshold ErodeDilate 010.png");
            cvimgThreshold = cvimgThreshold.Erode(8).Dilate(8);


            //int nLargestContourInd = -1;
            //int nLargestContourSize = 0;
            //Emgu.CV.Image<Gray, byte> cvimgContours = new Image<Gray, Byte>(cvimgThreshold.Width, cvimgThreshold.Height);
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(cvimgThreshold, contours, null, RetrType.Ccomp, ChainApproxMethod.ChainApproxTc89L1);

                // Find the largest object...  It will likely be the perimeter
                for (i = 0; i < contours.Size; i++)
                {
                    if ((contours[i] != null) && (contours[i].Size > nLargestContourSize))
                    {
                        nLargestContourSize = contours[i].Size;
                        nLargestContourInd = i;
                    }
                }

                // Draw the objects larger than a base size
                nLargestContourSize = 11800;
                for (i = 0; i < contours.Size; i++)
                {
                    if ((contours[i] != null) && (contours[i].Size > 450) && (contours[i].Size < nLargestContourSize))
                        CvInvoke.DrawContours(cvimgContours, contours, i, new MCvScalar(255), 1);
                }
                cvimgContours.Save(szOutputPath + "goofy05dd2.png");
                CvInvoke.DrawContours(cvimgContours, contours, nLargestContourInd, new MCvScalar(255), 1);
                //cvimgContours = cvimgContours.Erode(3);
                cvimgContours.Save(szOutputPath + "goofy05dd3.png");

                Emgu.CV.Util.VectorOfPoint approx = new VectorOfPoint();
                int nNumFoundCont = 0;

                VectorOfVectorOfPoint rectContours = new VectorOfVectorOfPoint();

                for (i = 0; i < contours.Size; i++)
                {
                    double epsilon = 0.04 * Emgu.CV.CvInvoke.ArcLength(contours[i], true);
                    Emgu.CV.CvInvoke.ApproxPolyDP(contours[i], approx, epsilon, true);

                    double dArea = Emgu.CV.CvInvoke.ContourArea(contours[i]);
                    Boolean bAreaIsConvex = Emgu.CV.CvInvoke.IsContourConvex(approx);
                    
                    // What we have MAY be a rectangle, but we need to make sure
                    // that the lines are parallel and 90 degrees.  Take
                    // the isolated rectangle, and .
                    if (approx.Size == 4
                        && dArea > 40000
                           )
                    {
                        if (dArea < 6000000)
                        {
                            nNumFoundCont++;
                            cvimgContours.SetZero();
                            rectContours.Push(contours[i]);
                            CvInvoke.DrawContours(cvimgContours, contours, i, new MCvScalar(255), 1);
                            cvimgContours.Save(szOutputPath + "goofy05dd4 - Contour " + nNumFoundCont.ToString("D3") + ".png");

                            //rectContours.Push(approx);
                        }
                    }
                }


                for (i = 0; i < rectContours.Size; i++)
                {
                    cvimgContours.SetZero();
                    CvInvoke.DrawContours(cvimgContours, rectContours, i, new MCvScalar(255), 1);
                    //cvimgContours = cvimgContours.Dilate(2); // makes sure we can get lines that are
                                                             // long enough.
                    //cvimgContours.Save(szOutputPath + "goofy05dd4 - Contour " + i.ToString("D3") + ".png");

                    /*
                    double rho = 0.5;
                    double theta = Math.PI / 1440.0;
                    Int32 threshold = 15;
                    double min_line_length = 160;
                    double max_line_gap = 100;

                    LineSegment2D[][] lines = cvimgCanny.HoughLinesBinary(rho, theta, threshold, min_line_length, max_line_gap);
                    */
                }


            }

            /*
            cvimgCanny = cvimgThreshold.Canny(100, 200.0).ThresholdBinary(new Gray(2), new Gray(255));
            cvimgCanny.Save(szOutputPath +  "goofy05f.png");
            cvimgCanny = cvimgCanny.Dilate(1);
            cvimgCanny.Save(szOutputPath + "goofy05fj.png");

            
            // Let's see if we can get the individual pixel values from the image
            int x;
            int y;
            int nCnt = 0;
            int nCannyCnt = 0;
            int nNumSucceeded = 0;
            int nLargestContiguous = 0;
            System.Drawing.Size imsz = cvimgCanny.Size;
            byte[,,] cannyData = cvimgCanny.Data;
            byte[,,] threshData = cvimgThreshold.Data;
            List<System.Drawing.Point> offsets = GenerateCircularOffsets(49);
            List<System.Drawing.Point> offsetsLinear;
            System.Drawing.Point[] arrOffsets = offsets.ToArray();
            int nNumOffsets = offsets.Count - 1;
            byte[] arrValues = new byte[nNumOffsets];
            int nNumToLoop = (2 * nNumOffsets);
            int nNumForSuccess = ((nNumOffsets >> 1) - 4);  // try a smaller value
            int nNumForLargeFailure = (nNumOffsets * 9) / 10;
            int nNumFilled;
            //int nNumForSuccess = ((nNumOffsets >> 1) - 10);

            // We have an image that has just the edges detected.  We also have
            // the thresholded image where the rectangles are indeed filled.
            // The purpose of this loop is to see if we can determine the edges
            // of the rectangle better so that we get an accurate hough result.
            // We will use the canny trace of the edges to limit the number of
            // pixels we need to test.  Once we have the candidates trimmed down,
            // we'll run the resultant canny image through the hough transform.
            // The thresholded image is also used because it includes the interior
            // pixels of the rectangles, albeit with outside noise also present.
            // We obtain a list of values from a circle of pixels around each
            // canny image's valid pixel we find.  If the edge is valid, it 
            // should have at around 180 degrees of points filled.

            /*
            // Erase unlikely candidate points
            bool bStartTrace = false;
            //MessageBox.Show("Hi1");
            for (y = 0; y < imsz.Height; y++)
            {
                for (x = 0; x < imsz.Width; x++)
                {
                    // Is this a valid point in the edge trace?
                    if (cannyData[y, x, 0] != 0)
                    {
                        nCannyCnt++;
                        nNumFilled = 0;
                        // obtain the pixel values in the circle centered
                        // at x,y in the threshold  image.
                        for (j = 0; j < nNumOffsets; j++)
                        {
                            System.Drawing.Point mypt = arrOffsets[j];
                            mypt.X += x;
                            mypt.Y += y;

                            // Get right up to the boundaries
                            if ((mypt.X < 0) || (mypt.Y < 0) || (mypt.X >= imsz.Width) || (mypt.Y >= imsz.Height))
                                arrValues[j] = 0;
                            else
                            {
                                arrValues[j] = threshData[mypt.Y,  mypt.X, 0];
                                if (arrValues[j] != 0)
                                    nNumFilled++;
                            }
                        }

                        // now, test the array to see if there are nNumOffsets/2
                        // contiguously set points.  We will need to loop around
                        // 360 + 90 degrees to get an accurate result.  This 
                        // is 5/4*nNumOffsets.
                        int nNumContiguous = 0;
                        byte nFillValue = 0;

                        nLargestContiguous = 0;

                        for (j = 0;
                              (j < nNumToLoop); //&& (nNumContiguous < nNumForSuccess) ;
                              j++)
                        {
                            // If we have a white pixel, then increment the 
                            // contiguous count.  Otherwise, reset it to zero.
                            if (arrValues[j % nNumOffsets] != 0)
                            {
                                nNumContiguous++;
                                if (nLargestContiguous < nNumContiguous)
                                {
                                    nLargestContiguous = nNumContiguous;
                                    // Check to see if we really found a valid edge
                                    // by drawing a line from the nLargestContiguous index to
                                    // nNumForSuccess index in the arrOffsets.  If all of the
                                    // pixels are nonblack, we can fill the cannyData with
                                    // white.  If none can be found, it will end up black.
                                    if (nLargestContiguous >= nNumForSuccess)
                                    {
                                        System.Drawing.Point pa1, pa2;
                                        pa1 = arrOffsets[(j % nNumOffsets)];
                                        pa2 = arrOffsets[((j-nNumForSuccess+nNumOffsets) % nNumOffsets)];

                                        pa1.X += x;
                                        pa1.Y += y;
                                        pa2.X += x;
                                        pa2.Y += y;

                                        offsetsLinear = GenerateLinearOffsets(pa1.X, pa1.Y, pa2.X, pa2.Y);
                                        System.Drawing.Point[] arrOffsetsLinear = offsetsLinear.ToArray();

                                        // In order for the color not to be erased (i.e. it is nonzero)
                                        // all the coordinates in the linear offsets must have a nonzero
                                        // pixel value from the threshold image.
                                        int nNumLinearFilled = 0;

                                        for (int k = 0; k < arrOffsetsLinear.Length; k++)
                                        {
                                            System.Drawing.Point mpt = arrOffsetsLinear[k];
                                            //mpt.X += x;
                                            //mpt.Y += y;
                                            // Get right up to the boundaries
                                            if ((mpt.X < 0) || (mpt.Y < 0) || (mpt.X >= imsz.Width) || (mpt.Y >= imsz.Height))
                                            {
                                                // Don't increment the count it's out of bound
                                            }
                                            else
                                            {
                                                byte nVal = threshData[mpt.Y, mpt.X, 0];
                                                if (nVal != 0)
                                                    nNumLinearFilled++;
                                            }
                                        }

                                        if (bStartTrace)
                                            Debug.WriteLine("num filled = " + nNumLinearFilled.ToString());

                                        // Set the fill value to white (255) if we found a
                                        // valid linear array of pixels in the threshold image.
                                        if (nNumLinearFilled == arrOffsetsLinear.Length)
                                        {
                                            nFillValue = 255;
                                        }
                                    }
                                }
                            }
                            else
                                nNumContiguous = 0;
                        }

                        // If we failed, erase the test center point from the canny image.
                        if (   (nLargestContiguous < nNumForSuccess)
                            || (nFillValue==0)
                            || (nNumFilled > nNumForLargeFailure)
                            )
                        {
                            cannyData[y, x, 0] = 0;
                            if (nFillValue != 0)
                            {
                                nFillValue -= 2;
                                nFillValue++;
                                nFillValue++;
                            }
                        }
                        else
                            nNumSucceeded++;

                        bStartTrace = false;
                    }
                }
            }
            //cvimgCanny = cvimgCanny.Dilate(2);
            
            cvimgCanny.Save(szOutputPath + "goofy05fi.png");
            */


            /* Hough Experiment
            double rho = 0.5;
            double theta = Math.PI / 1440.0;
            Int32 threshold = 15;
            double min_line_length = 400;
            double max_line_gap = 100;
            int j;
            
            LineSegment2D[][] lines = cvimgContours.HoughLinesBinary(rho, theta, threshold, min_line_length, max_line_gap);
            //LineSegment2D[][] lines = cvimgCanny.HoughLinesBinary(rho, theta, threshold, min_line_length, max_line_gap);
            Emgu.CV.Image<Bgr, byte> cvimgThresholdColor = new Emgu.CV.Image<Bgr, byte>(cvimgThreshold.Size);  //cvimgThreshold.Convert<Bgr, Byte>();

            int nNum90DegAngles = 0;

            Emgu.CV.Image<Gray, byte> cvimgHoughLines = new Emgu.CV.Image<Gray, byte>(cvimgThreshold.Size);
            cvimgHoughLines.SetValue(new Gray(0));

            foreach (LineSegment2D line in lines[0])
            {
                cvimgHoughLines.Draw(line, new Gray(255), 1);
                cvimgThresholdColor.Draw(line, new Bgr(Color.Red), 1);
            }
            if (lines.Length > 1)
            {
                foreach (LineSegment2D line in lines[1])
                {
                    cvimgHoughLines.Draw(line, new Gray(255), 1);
                    cvimgThresholdColor.Draw(line, new Bgr(Color.Red), 1);
                }
            }
            cvimgHoughLines.Save(szOutputPath + "goofy05gg.png");
            cvimgThresholdColor.Save(szOutputPath + "goofy05g.png");



            #region oldcode6 houghangles
            /*
            /* Hough experiment looking for 90 degree intersections 
            cvimgThresholdColor.SetValue(0);

            //Byte[,,] byteThresholdColorData = cvimgThresholdColor.Data;
            int rows = cvimgThresholdColor.Rows;
            int cols = cvimgThresholdColor.Cols;
            System.Drawing.Point pt = new System.Drawing.Point();   

            List<LinkedLineSegment2D> linkedLines = new List<LinkedLineSegment2D>();
            for (i=0; i < lines[0].Length; i++)
                linkedLines.Add(new LinkedLineSegment2D(lines[0][i]));

            //int j;
            for (i=0; i < linkedLines.Count; i++)
            {
                for (j=i; j < linkedLines.Count; j++)
                {
                    if (i != j)
                    {
                        // Unique compares only
                        LinkedLineSegment2D first = linkedLines[i];
                        LinkedLineSegment2D second = linkedLines[j];
                        PointF ?pointIntersection = first.Intersect90(ref second);

                        if (pointIntersection != null)
                        {
                            pt.X = (int)pointIntersection.Value.X ;
                            pt.Y = (int)pointIntersection.Value.Y ;
                            //if (pt.X >300 && pt.Y > 300 && pt.X < 3700 && pt.Y < 3700)
                            {
                                nNum90DegAngles++;
                                cvimgThresholdColor.Draw(first.lineOriginal, new Bgr(Color.Aqua), 1);
                                cvimgThresholdColor.Draw(second.lineOriginal, new Bgr(Color.Aqua), 1);
                                //cvimgThresholdColor.Draw(new CircleF(pt, 10.0f), new Bgr(Color.Magenta), 0);
                                cvimgThresholdColor.Draw(new Cross2DF(pt, 10.0f, 10.0f), new Bgr(Color.Magenta), 1);

                                //cvimgThresholdColor.Draw(new LineSegment2D(pt,first.lineOriginal.P1), new Bgr(Color.Aqua), 1);
                                //cvimgThresholdColor.Draw(new LineSegment2D(pt,first.lineOriginal.P2), new Bgr(Color.Aqua), 1);
                                //cvimgThresholdColor.Draw(new LineSegment2D(pt, second.lineOriginal.P1), new Bgr(Color.Aqua), 1);
                                //cvimgThresholdColor.Draw(new LineSegment2D(pt, second.lineOriginal.P2), new Bgr(Color.Aqua), 1);

                                //cvimgThresholdColor.Save(szOutputPath + "goofy05h-" + i.ToString("D3") + "-" + j.ToString("D3") + ").png");
                            }
                        }
                    }
                }
            }
            cvimgThresholdColor.Save(szOutputPath + "goofy05h.png");
            gdiGreyImage.Source = ToBitmapSource(cvimgThresholdColor);

            /*
                switch (nImageIndex)
                {
                    case 0: gdiImage.Source = ToBitmapSource(cvimgDewarpedShaded); gdiGreyImage.Source = ToBitmapSource(cvimgDewarped); break;
                    case 1: gdiImage.Source = ToBitmapSource(thresh); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 2: gdiImage.Source = ToBitmapSource(cudaImageOut.ToMat()); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 3: gdiImage.Source = ToBitmapSource(thg); gdiGreyImage.Source = ToBitmapSource(cvimgUndistortedColor); break;
                    case 4: gdiImage.Source = ToBitmapSource(cvimgUndistorted); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 5: gdiImage.Source = ToBitmapSource(cvimgDewarpedThg); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 6: gdiImage.Source = ToBitmapSource(cvimgDewarpedSeg); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 7: gdiImage.Source = ToBitmapSource(cvimgTestColumns); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 8: gdiImage.Source = ToBitmapSource(thg); gdiGreyImage.Source = ToBitmapSource(thresh); break;
                    case 9: gdiImage.Source = ToBitmapSource(threshDilateErode); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 10: gdiImage.Source = ToBitmapSource(cvimgUndistortedEmptyPolys); gdiGreyImage.Source = ToBitmapSource(timgOrg); break;
                }
                //gdiImage.Source = ToBitmapSource(cudaImageOut.ToMat());
            }*/



           // #endregion oldcode previous algorithm

        }


        /*
        private void DetectEdges(bool bDoFileOpen)
        {
            double dLength = Properties.Settings.Default.ResultLength;
            double dWidth = Properties.Settings.Default.ResultWidth;
            double dScaleFactor = Properties.Settings.Default.ResultScale;
            float dScaleFactorF = (float)dScaleFactor;
            string szOrgImage;
            float dMedianWidthF;
            float dMedianHeightF;
            string szOutputPath = "C:\\Users\\G.Norkus\\Desktop\\Projects\\Security Cameras\\Test Images\\2024-05-22\\Output\\";

            Rectangle rc2;

            if (dLength * dWidth * dScaleFactor <= 0)
            {
                MessageBox.Show("Please press the Load ARuCo Images button and select a calibration image before attempting this action.");
                return;
            }

            CheckMatricesLoaded();

            if (bDoFileOpen)
            {
                OpenFileDialog openPic = new OpenFileDialog();
                openPic.Multiselect = false;
                openPic.Title = "Open Background Image";
                if (openPic.ShowDialog() == false)
                {
                    return;
                }

                szOrgImage = openPic.FileName;
                szLastFileName = szOrgImage;

                Title = "MainWindow - Undistort File - " + System.IO.Path.GetFileName(szLastFileName); ;

            }
            else
            {
                szOrgImage = szLastFileName;
                if (szOrgImage.Length == 0)
                {
                    MessageBox.Show("Please select a file using UndistortImage before trying to Redo");
                    return;
                }
                Title = "MainWindow - Undistort Recalculating - " + System.IO.Path.GetFileName(szLastFileName); ;
            }

            Emgu.CV.Image<Bgr, Byte> cvimgColor = new Image<Bgr, Byte>(szOrgImage);
            Emgu.CV.Image<Bgra, Byte> cvimgColorBgra = new Image<Bgra, Byte>(szOrgImage);
            int smallw = cvimgColor.Width / 4;
            int smallh = cvimgColor.Height / 4;

            CvInvoke.Resize(cvimgColor, cvimgColor, new System.Drawing.Size(smallw, smallh), 0, 0, Inter.Area);
            CvInvoke.Resize(cvimgColorBgra, cvimgColor, new System.Drawing.Size(smallw, smallh), 0, 0, Inter.Area);
            cvimgColor.Save(szOutputPath + "goofy01.png");

            Emgu.CV.Image<Bgra, Byte> cvimgOutput = new Image<Bgra, Byte>(cvimgColorBgra.Width, cvimgColorBgra.Height);

            CudaImage<Bgra, byte> cudaImage = new CudaImage<Bgra, byte>(cvimgColorBgra);
            CudaImage<Bgra, byte> cudaImageOut = new CudaImage<Bgra, byte>(cvimgColorBgra);

            long nSum = SumData(cvimgColorBgra.Mat);

            int nNumIterations = int.Parse(ctlCameraDataCtl.NumIterations.Text);
            int nSegmentLength = int.Parse(ctlCameraDataCtl.SegmentLength.Text);
            int nColorRadius = int.Parse(ctlCameraDataCtl.ColorRadius.Text);
            int nSpatialRadius = int.Parse(ctlCameraDataCtl.SpatialRadius.Text);

            CudaInvoke.MeanShiftSegmentation(cudaImage, cvimgOutput, nSpatialRadius, nColorRadius, nSegmentLength, new MCvTermCriteria(nNumIterations), null);
            nSum = SumData(cvimgOutput.Mat);

            Emgu.CV.Image<Bgr, Byte> cvimgUndistorted = UndistortBigImage<Bgra, Byte>(cvimgOutput).Convert<Bgr, Byte>();
            Emgu.CV.Image<Bgr, Byte> cvimgUndistortedColor = UndistortBigImage<Bgr, Byte>(cvimgColor);

            cvimgUndistorted.Save(szOutputPath + "goofy03.png");

            EraseDrawingBorders<Bgr, byte>(ref cvimgUndistorted);

            // Difference of gaussians math
            double grad1 = double.Parse(ctlCameraDataCtl.GW1Factor.Text) + 1.0;
            double gsigma1 = Math.Sqrt(-(grad1 * grad1) / (2 * Math.Log10(1.0 / 255.0)));
            double gsigmasq1 = 2 * gsigma1 * gsigma1;
            double gL1 = Math.Sqrt(-gsigmasq1 * Math.Log10(1.0 / 255.0));
            int n1 = (int)(Math.Ceiling(gL1) * 2.0);
            int gw1 = (n1 % 2 == 1) ? n1 : n1 + 1;

            double grad2 = double.Parse(ctlCameraDataCtl.GW2Factor.Text) + 1.0;
            double gsigma2 = Math.Sqrt(-(grad2 * grad2) / (2 * Math.Log10(1.0 / 255.0)));
            double gsigmasq2 = 2 * gsigma2 * gsigma2;
            double gL2 = Math.Sqrt(-gsigmasq2 * Math.Log10(1.0 / 255.0));
            int n2 = (int)(Math.Ceiling(gL2) * 2.0);
            int gw2 = (n2 % 2 == 1) ? n2 : n2 + 1;

            Emgu.CV.Image<Bgr, Byte> cvimgTemp = cvimgUndistorted.SmoothMedian(7);
            Emgu.CV.Image<Bgr, Byte> g1 = cvimgTemp.SmoothGaussian(gw1, gw1, gsigma1, gsigma1).Convert<Bgr, Byte>();
            Emgu.CV.Image<Bgr, Byte> g2 = cvimgTemp.SmoothGaussian(gw2, gw2, gsigma2, gsigma2).Convert<Bgr, Byte>();

            cvimgTemp = g1.Sub(g2);
            cvimgTemp.Save(szOutputPath + "goofy09.png");

            Emgu.CV.Image<Bgr, Byte> cvimgTemp2 = cvimgTemp.ThresholdBinary(new Bgr(0, 0, 0), new Bgr(255.0, 255.0, 255.0));
            Emgu.CV.Image<Gray, Byte>[] thgArr = cvimgTemp2.Split();
            Emgu.CV.Image<Gray, Byte> cvimgTemp3 = thgArr[0].Or(thgArr[1].Or(thgArr[2]));
            Emgu.CV.Image<Gray, ushort> cvimgTemp4 = cvimgTemp3.Convert<Gray, ushort>();
            Emgu.CV.Image<Gray, ushort> thg = cvimgTemp4.ThresholdBinary(new Gray(1), new Gray(65535));

            GenerateDetectMaskPolys(cvimgUndistorted.Size);

            Emgu.CV.Image<Bgr, Byte> cvimgUndistortedEmptyPolys = cvimgUndistorted.Convert<Bgr, Byte>();
            Emgu.CV.Image<Gray, ushort> timg = DrawRectDetectMask<Gray, ushort>(ref thg);
            Emgu.CV.Image<Bgr, Byte> timgOrg = DrawRectDetectMaskPolys<Gray, ushort>(ref thg);
            timgOrg.Save(szOutputPath + "goofy15.png");
            timg = thg.And(timg);

            float[] histIgnoreVals = new float[1];

            nSum = SumData(thg.Mat);
            nSum = SumData(timg.Mat);
            nSum = SumData(timgOrg.Mat);


            CalcEmptyPolys(ref thg, ref timg, ref histIgnoreVals);
            DisplayEmptyPolys<Gray, ushort>(ref timg, ref histIgnoreVals);
            timgOrg.Save(szOutputPath + "goofy18.png");

            DisplayEmptyPolys<Bgr, Byte>(ref cvimgUndistortedEmptyPolys, ref histIgnoreVals);

            CameraData? cd = this.ctlCameraDataCtl.CameraIPComboBox.SelectedItem as CameraData;
            Emgu.CV.Image<Bgr, Byte> cvimgDewarpedOrg = cvimgUndistorted
                                    .WarpPerspective<double>(homographyMatrix
                                                            , (int)((dLength + cd.BorderLeft + cd.BorderRight) * dScaleFactor)
                                                            , (int)((dWidth + cd.BorderTop + cd.BorderBottom) * dScaleFactor)
                                                            , Inter.Nearest // Inter.Cubic
                                                            , Warp.Default
                                                            , BorderType.Default
                                                            , new Bgr(0, 0, 0));

            Emgu.CV.Image<Bgr, Byte> cvimgDewarped = new Emgu.CV.Image<Bgr, Byte>(cvimgDewarpedOrg.Size);
            cvimgDewarpedOrg.CopyTo(cvimgDewarped);

            DisplayEmptyPolys<Bgr, Byte>(ref cvimgDewarpedOrg, ref histIgnoreVals, true);

            Emgu.CV.Image<Bgr, Byte> cvimgDewarpedSeg = new Emgu.CV.Image<Bgr, Byte>(cvimgDewarpedOrg.Size);
            cvimgDewarpedOrg.CopyTo(cvimgDewarpedSeg);


            Emgu.CV.Image<Bgr, Byte> cvimgDewarpedShaded = cvimgUndistortedColor
                                    .WarpPerspective<double>(homographyMatrix
                                                            , (int)(dLength * dScaleFactor/4)
                                                            , (int)(dWidth * dScaleFactor/4)
                                                            , Inter.Cubic
                                                            , Warp.Default
                                                            , BorderType.Default
                                                            , new Bgr(0, 0, 0));

            Emgu.CV.Image<Gray, Byte> cvimgDewarpedThg = thg
                    .WarpPerspective<double>(homographyMatrix
                                            , (int)(dLength * dScaleFactor/4)
                                            , (int)(dWidth * dScaleFactor/4)
                                            , Inter.Cubic
                                            , Warp.Default
                                            , BorderType.Default
                                            , new Gray(0)).Convert<Gray, Byte>();

            cvimgDewarpedShaded.Save(szOutputPath + "goofy20.png");
            cvimgDewarpedThg.Save(szOutputPath + "goofy21.png");

            int nNumFills = 0;

            // vecRackPolys is a member value of the MainWindow class.
            // We may need to isolate the major objects before attempting to 
            // do our adjustment algorithm.  The adjustment algorithm
            // will look at the 4 quadrants of the image area and
            // see if they can be adjusted individually.  They will also
            // determine if 
            if (vecRackPolys.Size > 0)
            {
                System.Drawing.Point seedpt;
                System.Drawing.Point seedptd;

                System.Drawing.Size[] tl = new System.Drawing.Size[4]
                {
                        new System.Drawing.Size(-1,-1),
                        new System.Drawing.Size(1, -1),
                        new System.Drawing.Size(1, 1),
                        new System.Drawing.Size(-1, 1)
                };

                System.Drawing.Size tr = new System.Drawing.Size(1, -1);
                System.Drawing.Size br = new System.Drawing.Size(1, 1);
                System.Drawing.Size bl = new System.Drawing.Size(-1, 1);
                Rectangle rc = new Rectangle();

                System.Drawing.Size masksz = cvimgDewarpedOrg.Size + new System.Drawing.Size(2, 2);
                Emgu.CV.Image<Gray, Byte> mask = new Emgu.CV.Image<Gray, Byte>(masksz);

                int nFillToleranceLow = (int)float.Parse(ctlCameraDataCtl.FillToleranceLow.Text);
                int nFillToleranceHigh = (int)float.Parse(ctlCameraDataCtl.FillToleranceHigh.Text);

                for (int i = 0; i < vecRackPolys.Size; i++)
                {
                    if (histIgnoreVals[i + 1] == 0)
                    {
                        VectorOfPointF vpf = vecRackPolys[i];

                        for (int k = 0; k < 4; k++)
                        {
                            seedpt = System.Drawing.Point.Round(vecRackPolys[i][k]);
                            seedptd = seedpt + tl[k];

                            // Is the point to be filled already white?  If so, ignore it.
                            if (seedptd.X >= 0 && seedptd.Y >= 0
                                && seedptd.X < cvimgDewarpedOrg.Width
                                && seedptd.Y < cvimgDewarpedOrg.Height)
                            {
                                byte Red_val = cvimgDewarpedOrg.Data[seedptd.Y, seedptd.X, 0];
                                byte Green_val = cvimgDewarpedOrg.Data[seedptd.Y, seedptd.X, 1];
                                byte Blue_val = cvimgDewarpedOrg.Data[seedptd.Y, seedptd.X, 2];

                                if (Red_val != 255 || Green_val != 255 || Blue_val != 255)
                                {
                                    CvInvoke.FloodFill(cvimgDewarpedOrg
                                                        , null // mask
                                                        , seedptd
                                                        , new Bgr(255, 255, 255).MCvScalar
                                                        , out rc
                                                        , new Bgr(nFillToleranceLow, nFillToleranceLow, nFillToleranceLow).MCvScalar
                                                        , new Bgr(nFillToleranceHigh, nFillToleranceHigh, nFillToleranceHigh).MCvScalar
                                                        , Connectivity.EightConnected
                                                        , FloodFillType.FixedRange);
                                    nNumFills++;
                                }
                            }
                        }
                    }
                }

                Emgu.CV.Image<Gray, byte> thresh = cvimgDewarpedOrg.ThresholdBinary(new Bgr(254, 254, 254), new Bgr(255, 255, 255)).Convert<Gray, byte>();

                int nImageIndex = int.Parse(ctlCameraDataCtl.ImageIndex.Text);

                // Draw on top of the dewarped thg a set of rectangles spaced by 2.629 inches, 0.75 inchs wide
                RectangleF rectangleF = new RectangleF(new PointF(1F * dScaleFactorF / 4F, 11F * dScaleFactorF / 4F), new SizeF(1F * dScaleFactorF / 4F, 63.125F * dScaleFactorF));
                Rectangle rectangle = Rectangle.Round(rectangleF);
                Emgu.CV.Image<Bgr, Byte> cvimgTestColumns = new Image<Bgr, Byte>(cvimgDewarpedThg.Size);


                do
                {
                    System.Drawing.Point[] pts = new System.Drawing.Point[4];
                    pts[0] = rectangle.Location;
                    pts[1] = pts[0] + new System.Drawing.Size(rectangle.Width, 0);
                    pts[2] = pts[1] + new System.Drawing.Size(0, rectangle.Height);
                    pts[3] = pts[2] + new System.Drawing.Size(-rectangle.Width, 0);

                    cvimgTestColumns.FillConvexPoly(pts, new Bgr(0, 0, 160));
                    cvimgTestColumns.Draw(rectangle, new Bgr(0, 0, 255), 1);

                    rectangleF.Offset(2.629F * dScaleFactorF / 4F, 0F);
                    rectangle = Rectangle.Round(rectangleF);
                } while (rectangle.Left < cvimgDewarpedThg.Width);


                cvimgDewarpedThg.Save(szOutputPath + "goofy22.png");
                Emgu.CV.Image<Gray, Byte> threshDilateErode = thresh.Dilate(10).Erode(10);
                cvimgDewarpedThg.Save(szOutputPath + "goofy24.png");

                // threshDilateErode now contains the contours we want to search through.
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(threshDilateErode, contours, null, RetrType.Ccomp, ChainApproxMethod.ChainApproxNone);
                    int count = contours.Size;
                    Emgu.CV.Util.VectorOfPoint approx = new VectorOfPoint();
                    LineSegment2D widthLine = new LineSegment2D();
                    LineSegment2D heightLine = new LineSegment2D();

                    for (int i = 0; i < count; i++)
                    {
                        if (contours[i] != null && (contours[i].Size > 150))
                        {
                            //int kk;
                            //RotatedRect rc = CvInvoke.MinAreaRect(contours[i]);
                            //System.Drawing.Point[] vertices = Array.ConvertAll(rc.GetVertices(), System.Drawing.Point.Round);
                            double epsilon = 0.05 * Emgu.CV.CvInvoke.ArcLength(contours[i], true);
                            Emgu.CV.CvInvoke.ApproxPolyDP(contours[i], approx, epsilon, true);

                            double dArea = Emgu.CV.CvInvoke.ContourArea(contours[i]);
                            Boolean bAreaIsConvex = Emgu.CV.CvInvoke.IsContourConvex(approx);

                            if (approx.Size == 4
                                && dArea > 40000
                                   )
                            {
                                System.Drawing.Point center;
                                System.Drawing.Size size;
                                string tstr;
                                int nBaseLine;

                                RotatedRect rotrc = CvInvoke.MinAreaRect(contours[i]);

                                System.Drawing.Point[] vertices = Array.ConvertAll(rotrc.GetVertices(), System.Drawing.Point.Round);
                                CvInvoke.Polylines(cvimgDewarpedOrg
                                                    //, approx
                                                    , contours[i].ToArray()
                                                    //, vertices
                                                    , true
                                                    , new Bgr(0, 255, 255).MCvScalar
                                                    , 3
                                                        );

                                CvInvoke.Polylines(cvimgDewarpedOrg
                                                    //, approx
                                                    //, contours[i].ToArray()
                                                    , vertices
                                                    , true
                                                    , new Bgr(0, 255, 0).MCvScalar
                                                    , 5
                                                        );

                                List<Point3D> pmeasurelist = new List<Point3D>();
                                List<System.Drawing.Point> plist = new List<System.Drawing.Point>();
                                plist.AddRange(contours[i].ToArray());

                                // First, sort by y, and create a list of widths
                                var psortedlist = plist.OrderBy(p => p.Y).ThenBy(p => p.X);

                                dMedianHeightF = dMedianWidthF = 0F;

                                int nelem = psortedlist.Count();
                                if (nelem > 0)
                                {
                                    List<int> widths = new List<int>();
                                    int nCurX = psortedlist.ElementAt(0).X;
                                    int nCurY = psortedlist.ElementAt(0).Y;
                                    int nFirstX = nCurX;
                                    int nWidth = 0;

                                    foreach (System.Drawing.Point p in psortedlist)
                                    {
                                        if (nCurY != p.Y)
                                        {
                                            // are we on a new line?  Save in a way that we can both
                                            // sort width and know the segment that was used for measuring
                                            pmeasurelist.Add(new Point3D(nCurX, nCurY, nCurX - nFirstX));

                                            widths.Add(nCurX - nFirstX);
                                            nCurY = p.Y;
                                            nCurX = p.X;
                                            nFirstX = nCurX;
                                        }
                                        else
                                            nCurX = p.X;
                                    }
                                    widths.Add(nCurX - nFirstX);
                                    pmeasurelist.Add(new Point3D(nCurX, nCurY, nCurX - nFirstX));

                                    var orderedMeasureList = pmeasurelist.OrderBy(p => p.Z);

                                    Point3D pLeft = orderedMeasureList.ElementAt(orderedMeasureList.Count() / 2);
                                    double nMedianWidth = pLeft.Z;
                                    dMedianWidthF = (float)nMedianWidth / dScaleFactorF;
                                    widthLine.P1 = new System.Drawing.Point((int)pLeft.X, (int)pLeft.Y);
                                    widthLine.P2 = new System.Drawing.Point((int)(pLeft.X - pLeft.Z), (int)pLeft.Y);
                                }


                                // Then, sort by x, and create a list of heights
                                psortedlist = plist.OrderBy(p => p.X).ThenBy(p => p.Y);
                                pmeasurelist.Clear();

                                nelem = psortedlist.Count();
                                if (nelem > 0)
                                {
                                    List<int> heights = new List<int>();
                                    int nCurX = psortedlist.ElementAt(0).X;
                                    int nCurY = psortedlist.ElementAt(0).Y;
                                    int nFirstY = nCurY;

                                    foreach (System.Drawing.Point p in psortedlist)
                                    {
                                        if (nCurX != p.X)
                                        {
                                            // are we on a new line?
                                            pmeasurelist.Add(new Point3D(nCurX, nCurY, nCurY - nFirstY));
                                            heights.Add(nCurY - nFirstY);
                                            nCurY = p.Y;
                                            nCurX = p.X;
                                            nFirstY = nCurY;
                                        }
                                        else
                                            nCurY = p.Y;
                                    }
                                    heights.Add(nCurY - nFirstY);
                                    pmeasurelist.Add(new Point3D(nCurX, nCurY, nCurY - nFirstY));

                                    heights.Sort();
                                    var orderedMeasureList = pmeasurelist.OrderBy(p => p.Z);

                                    //int nMedianHeight = heights.ElementAt(heights.Count() / 2);
                                    Point3D pLeft = orderedMeasureList.ElementAt(orderedMeasureList.Count() / 2);
                                    double nMedianHeight = pLeft.Z;
                                    dMedianHeightF = (float)nMedianHeight / dScaleFactorF ;
                                    heightLine.P1 = new System.Drawing.Point((int)pLeft.X, (int)pLeft.Y);
                                    heightLine.P2 = new System.Drawing.Point((int)(pLeft.X), (int)(pLeft.Y - pLeft.Z));
                                }

                                center = approx[0];
                                center.Offset(approx[1]);
                                center.Offset(approx[2]);
                                center.Offset(approx[3]);
                                center.X /= 4;
                                center.Y /= 4;

                                tstr = "W=" + dMedianWidthF.ToString("0.00") + "\nH=" + dMedianHeightF.ToString("0.00");

                                nBaseLine = 0;
                                System.Drawing.Size sz = Emgu.CV.CvInvoke.GetTextSize(tstr.Split("\n", 2)[0], FontFace.HersheyPlain, 6, 3, ref nBaseLine);
                                size = new System.Drawing.Size() - sz;
                                size.Width /= 2;

                                Emgu.CV.CvInvoke.ArrowedLine(cvimgDewarpedOrg, heightLine.P1, heightLine.P2, new MCvScalar(0, 0, 255), 3, LineType.EightConnected, 0, 0.1);
                                Emgu.CV.CvInvoke.ArrowedLine(cvimgDewarpedOrg, widthLine.P1, widthLine.P2, new MCvScalar(0, 0, 255), 3, LineType.EightConnected, 0, 0.1);

                                foreach (string sstr in tstr.Split("\n", 2))
                                {
                                    Emgu.CV.CvInvoke.PutText(cvimgDewarpedOrg, sstr, center + size, FontFace.HersheyPlain, 6, new MCvScalar(0, 0, 0), 3);
                                    center.Y += 100;
                                }
                            }
                            else
                            if (dArea > 40000)
                            {
                                Emgu.CV.Structure.RotatedRect rcRotated = CvInvoke.MinAreaRect(contours[i]);
                                System.Drawing.Point[] vertices = Array.ConvertAll(rcRotated.GetVertices(), System.Drawing.Point.Round);

                                CvInvoke.Polylines(cvimgDewarpedOrg
                                                    , contours[i].ToArray()
                                                    , true
                                                    , new Bgr(0, 0, 255).MCvScalar
                                                    , 3
                                                        );
                                CvInvoke.Polylines(cvimgDewarpedOrg
                                                    , vertices
                                                    , true
                                                    , new Bgr(0, 255, 0).MCvScalar
                                                    , 5
                                                        );
                            }
                        }
                    }


                    Title = "MainWindow - Undistort Done - "
                            + System.IO.Path.GetFileName(szLastFileName)
                            + "  Contour Count=" + contours.Size.ToString();
                }



                switch (nImageIndex)
                {
                    case 0: gdiImage.Source = ToBitmapSource(cvimgDewarpedShaded); gdiGreyImage.Source = ToBitmapSource(cvimgDewarped); break;
                    case 1: gdiImage.Source = ToBitmapSource(thresh); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 2: gdiImage.Source = ToBitmapSource(cudaImageOut.ToMat()); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 3: gdiImage.Source = ToBitmapSource(thg); gdiGreyImage.Source = ToBitmapSource(cvimgUndistortedColor); break;
                    case 4: gdiImage.Source = ToBitmapSource(cvimgUndistorted); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 5: gdiImage.Source = ToBitmapSource(cvimgDewarpedThg); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 6: gdiImage.Source = ToBitmapSource(cvimgDewarpedSeg); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 7: gdiImage.Source = ToBitmapSource(cvimgTestColumns); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 8: gdiImage.Source = ToBitmapSource(thg); gdiGreyImage.Source = ToBitmapSource(thresh); break;
                    case 9: gdiImage.Source = ToBitmapSource(threshDilateErode); gdiGreyImage.Source = ToBitmapSource(cvimgDewarpedOrg); break;
                    case 10: gdiImage.Source = ToBitmapSource(cvimgUndistortedEmptyPolys); gdiGreyImage.Source = ToBitmapSource(timgOrg); break;
                }
                //gdiImage.Source = ToBitmapSource(cudaImageOut.ToMat());
            }
        }*/


    }
}
