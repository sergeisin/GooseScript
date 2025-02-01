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

        public static bool IsBitString(string str)
        {
            return str.All("01".Contains);
        }

        public static byte[] GetOctetString(string hexString)
        {
            return Enumerable.Range(0, hexString.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                .ToArray();
        }

        public static byte[] GetBitString(string bitString)
        {
            const int HighOrderBit = 7;

            int len = bitString.Length;

            int numBytes = len / 8;
            if (len % 8 != 0)
                numBytes++;

            int padding = numBytes * 8 - len;

            byte[] buffer = new byte[1 + numBytes];

            buffer[0] = (byte)padding;

            int buf_Pos = 1;
            int bit_Pos = HighOrderBit;

            foreach (char bit in bitString)
            {
                if (bit == '1')
                    buffer[buf_Pos] |= (byte)(1 << bit_Pos);

                if (--bit_Pos < 0)
                {
                    buf_Pos++;
                    bit_Pos = HighOrderBit;
                }
            }

            return buffer;
        }
    }
}
