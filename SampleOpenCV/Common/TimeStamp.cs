using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mfi_erp_api
{
    public struct TimeStamp
    {
        public byte[] byteArray;

        public TimeStamp(byte[] byteArray)
        {
            this.byteArray = byteArray;
        }

        public TimeStamp(object byteArrayObject)
        {
            if (byteArrayObject != null)
                this.byteArray = (byte[])byteArrayObject;
            else
                this.byteArray = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        public static implicit operator TimeStamp(byte[] byteArray)
        {
            return new TimeStamp(byteArray);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TimeStamp))
                return false;

            return this == (TimeStamp)obj;
        }

        public static bool operator ==(TimeStamp timeStamp1, TimeStamp timeStamp2)
        {
            return Comparison(timeStamp1, timeStamp2) == 0;
        }

        public static bool operator !=(TimeStamp timeStamp1, TimeStamp timeStamp2)
        {
            return Comparison(timeStamp1, timeStamp2) != 0;
        }

        public static bool operator >(TimeStamp timeStamp1, TimeStamp timeStamp2)
        {
            return Comparison(timeStamp1, timeStamp2) > 0;
        }

        public static bool operator <(TimeStamp timeStamp1, TimeStamp timeStamp2)
        {
            return Comparison(timeStamp1, timeStamp2) < 0;
        }

        public static bool operator >=(TimeStamp timeStamp1, TimeStamp timeStamp2)
        {
            return Comparison(timeStamp1, timeStamp2) >= 0;
        }

        public static bool operator <=(TimeStamp timeStamp1, TimeStamp timeStamp2)
        {
            return Comparison(timeStamp1, timeStamp2) <= 0;
        }

        public static int Comparison(TimeStamp timeStamp1, TimeStamp timeStamp2)
        {
            //if (timeStamp1 == null && timeStamp2 == null)
            //    return 0;

            //if (timeStamp1 == null)
            //    return 1;
            //else if (timeStamp2 == null)
            //    return -1;

            return System.Collections.StructuralComparisons.StructuralComparer.Compare(timeStamp1.byteArray, timeStamp2.byteArray);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            if (byteArray == null)
                return "0x0000000000000000";
            else
                return ByteArrayToSqlHex(byteArray);
        }

        /// <summary>
        /// Converts a byte array to a hex string.
        /// </summary>
        /// <param name="byteArray">Byte array to convert</param>
        /// <returns>Hex string</returns>
        public static string ByteArrayToSqlHex(byte[] byteArray)
        {
            return "0x" + BitConverter.ToString(byteArray).Replace("-", "");
        }

    }

}
