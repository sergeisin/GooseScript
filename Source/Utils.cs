using System;
using System.Linq;

namespace GooseScript
{
    public static class Utils
    {
        public static bool IsHexString(string str)
        {
            return str.All("0123456789abcdefABCDEF".Contains);
        }

        public static byte[] GetOctets(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public static bool IsBitString(string str)
        {
            return str.All("01".Contains);
        }

        public static byte[] GetBitString(string str)
        {
            throw new NotImplementedException();
        }
    }
}
