using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleOpenCV
{
    //public class IndexedPoint : IComparable<IndexedPoint>

    class IndexedPointComparer : IComparer<IndexedPoint>
    {
        public int Compare(IndexedPoint x, IndexedPoint y)
        {
            if (x.ind < y.ind) return -1;
            if (x.ind > y.ind) return 1;
            return 0;
        }
    }



    public class IndexedPoint 
    {
        public System.Drawing.Point pt {  get; set; }
        public int ind {  get; set; }

        public override int GetHashCode()
        {
            return ind;
        }

        public override bool Equals(object obj)
        {
            IndexedPoint other = obj as IndexedPoint;
            return ind == other.ind;
        }

        public IndexedPoint(System.Drawing.Point newpt, int newind)
        {
            pt = newpt; ind = newind;
        }

        static public System.Drawing.Point[] ToArray(IndexedPoint[] ptArrIndex)
        {
            System.Drawing.Point[] ptArr = null;
            int i;

            ptArr = new System.Drawing.Point[ptArrIndex.Length];
            for (i=0; i<ptArrIndex.Length; i++)
                ptArr[i] = ptArrIndex[i].pt;

            return ptArr;
        }
    }
}
