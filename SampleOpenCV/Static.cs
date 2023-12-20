using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpTech.Sql;

namespace SampleOpenCV
{






    internal class Static
    {
        public string? strSelectedCameraURL ;
        public string? strSelectedWorkstation ;

        Static()
        {
            strSelectedCameraURL = "";
            strSelectedWorkstation = "";
        }


        public static string NormalizeWhiteSpace(string input, char normalizeTo = ' ')
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            int current = 0;
            char[] output = new char[input.Length];
            bool skipped = false;

            foreach (char c in input.ToCharArray())
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!skipped)
                    {
                        if (current > 0)
                            output[current++] = normalizeTo;

                        skipped = true;
                    }
                }
                else
                {
                    skipped = false;
                    output[current++] = c;
                }
            }

            return new string(output, 0, skipped ? current - 1 : current);
        }

        private static CameraObservableColl _CameraDataColl;
        public static CameraObservableColl CameraDataColl
        {
            get
            {
                if (_CameraDataColl == null)
                {
                    // Do the necessary initialization
                    _CameraDataColl = new CameraObservableColl();

                    // Query the database to get the existing values

                }
                return _CameraDataColl;
            }
        }


        private static WorkStationObservableColl _WorkStationColl;
        public static WorkStationObservableColl WorkStationColl
        {
            get
            {
                if (_WorkStationColl == null)
                {
                    // Do the necessary initialization
                    _WorkStationColl = new WorkStationObservableColl();

                    // Query the database to get the existing values

                }
                return _WorkStationColl;
            }
        }

    }
}