using System;
using System.Text;

namespace GooseScript
{
    internal static class BerEncoder
    {
        public static int GetEncoded_INT32U_Size(uint value)
        {
            if (value > 0x7FFFFFFF) return 5;
            if (value >   0x7FFFFF) return 4;
            if (value >     0x7FFF) return 3;
            if (value >       0x7F) return 2;
                                    return 1;
        }

        public static int GetEncoded_INT32S_Size(int value)
        {
            if (value >= 0)
            {
                return GetEncoded_INT32U_Size((uint)value);
            }

            if (value < -0x800000) return 4;
            if (value <   -0x8000) return 3;
            if (value <     -0x80) return 2;
                                   return 1;
        }

        public static int GetEncoded_L_Size(int length)
        {
            if (length < 0x80)      // Short define form
                return 1;           // [00 .. 7f]

            if (length < 0x100)     // Long define form
                return 2;           // 81 + [80 .. ff]
            else
                return 3;           // 82 + [01 00 .. ff ff]
        }

        public static byte[] Create_VisibleString_TLV(byte tag, string str)
        {
            byte[] data = Encoding.ASCII.GetBytes(str);

            int sLen = GetEncoded_L_Size(data.Length);
            int vLen = data.Length;

            var buffer = new byte[1 + sLen + vLen];
            int offset = 0;

            Encode_RawBytes_TLV(buffer, ref offset, tag, data);

            return buffer;
        }

        public static void Encode_TL_Only(Span<byte> frame, ref int offset, byte tag, int length)
        {
            frame[offset++] = tag;
            int sLen = GetEncoded_L_Size(length);

            switch (sLen)
            {
                case 1:
                    frame[offset++] = (byte)length;
                    break;

                case 2:
                    frame[offset++] = 0x81;
                    frame[offset++] = (byte)length;
                    break;

                case 3:
                    frame[offset++] = 0x82;
                    frame[offset++] = (byte)(length >> 8);
                    frame[offset++] = (byte)(length & 0xFF);
                    break;
            }
        }

        public static void Encode_RawBytes(Span<byte> frame, ref int offset, ReadOnlySpan<byte> src, int count = 0)
        {
            int iterations = count != 0 ? count : src.Length;

            for (int i = 0; i < iterations; i++)
            {
                frame[offset + i] = src[i];
            }

            offset += iterations;
        }

        public static void Encode_RawBytes_TLV(Span<byte> frame, ref int offset, byte tag, byte[] data)
        {
            frame[offset++] = tag;

            int sLen = GetEncoded_L_Size(data.Length);
            int vLen = data.Length;

            switch (sLen)
            {
                case 1:
                    frame[offset++] = (byte)vLen;
                    break;

                case 2:
                    frame[offset++] = 0x81;
                    frame[offset++] = (byte)vLen;
                    break;

                case 3:
                    frame[offset++] = 0x82;
                    frame[offset++] = (byte)(vLen >> 8);
                    frame[offset++] = (byte)(vLen & 0xFF);
                    break;
            }

            Encode_RawBytes(frame, ref offset, data);
        }

        public static void Encode_INT32U_TLV(Span<byte> frame, ref int offset, byte tag, uint value)
        {
            frame[offset++] = tag;

            switch (GetEncoded_INT32U_Size(value))
            {
                case 1:
                    frame[offset++] = 0x01;
                    frame[offset++] = (byte)(0xFF & value);
                    break;
                case 2:
                    frame[offset++] = 0x02;
                    frame[offset++] = (byte)(0xFF & value >> 8);
                    frame[offset++] = (byte)(0xFF & value);
                    break;
                case 3:
                    frame[offset++] = 0x03;
                    frame[offset++] = (byte)(0xFF & value >> 16);
                    frame[offset++] = (byte)(0xFF & value >> 8);
                    frame[offset++] = (byte)(0xFF & value);
                    break;
                case 4:
                    frame[offset++] = 0x04;
                    frame[offset++] = (byte)(0xFF & value >> 24);
                    frame[offset++] = (byte)(0xFF & value >> 16);
                    frame[offset++] = (byte)(0xFF & value >> 8);
                    frame[offset++] = (byte)(0xFF & value);
                    break;
                case 5:
                    frame[offset++] = 0x05;
                    frame[offset++] = 0x00;
                    frame[offset++] = (byte)(0xFF & value >> 24);
                    frame[offset++] = (byte)(0xFF & value >> 16);
                    frame[offset++] = (byte)(0xFF & value >> 8);
                    frame[offset++] = (byte)(0xFF & value);
                    break;
            }
        }

        public static void Encode_INT32S_TLV(Span<byte> frame, ref int offset, byte tag, int value)
        {
            frame[offset++] = tag;

            switch (GetEncoded_INT32S_Size(value))
            {
                case 1:
                    frame[offset++] = 0x01;
                    frame[offset++] = (byte)(0xFF & value);
                    break;
                case 2:
                    frame[offset++] = 0x02;
                    frame[offset++] = (byte)(0xFF & value >> 8);
                    frame[offset++] = (byte)(0xFF & value);
                    break;
                case 3:
                    frame[offset++] = 0x03;
                    frame[offset++] = (byte)(0xFF & value >> 16);
                    frame[offset++] = (byte)(0xFF & value >>  8);
                    frame[offset++] = (byte)(0xFF & value);
                    break;
                case 4:
                    frame[offset++] = 0x04;
                    frame[offset++] = (byte)(0xFF & value >> 24);
                    frame[offset++] = (byte)(0xFF & value >> 16);
                    frame[offset++] = (byte)(0xFF & value >>  8);
                    frame[offset++] = (byte)(0xFF & value);
                    break;
            }
        }

        public static void Encode_FLOAT_TLV(Span<byte> frame, ref int offset, byte tag, float value)
        {
            byte[] floatBytes = BitConverter.GetBytes(value);

            frame[offset++] = tag;
            frame[offset++] = 0x05;
            frame[offset++] = 0x08;

            if (BitConverter.IsLittleEndian)
            {
                for (int i = 3; i >= 0; i--)
                {
                    frame[offset++] = floatBytes[i];
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    frame[offset++] = floatBytes[i];
                }
            }
        }

        public static void Encode_Boolean_TLV(Span<byte> frame, ref int offset, byte tag, bool value)
        {
            frame[offset++] = tag;
            frame[offset++] = 0x01;

            if (value)
                frame[offset++] = 0xff;
            else
                frame[offset++] = 0x00;
        }

        public static void Encode_Quality_TLV(Span<byte> frame, ref int offset, Quality q)
        {
            frame[offset++] = 0x84;
            frame[offset++] = 0x03;
            frame[offset++] = 0x03;

            byte b1 = 0;
            byte b2 = 0;

            switch (q.Validity)
            {
                case Validity.Good:         b1 |= 0b_0000_0000; break;
                case Validity.Invalid:      b1 |= 0b_0100_0000; break;
                case Validity.Reserved:     b1 |= 0b_1000_0000; break;
                case Validity.Questionable: b1 |= 0b_1100_0000; break;
            }

            if (q.Overflow)        b1 |= 0b_0010_0000;
            if (q.OutofRange)      b1 |= 0b_0001_0000;
            if (q.BadReference)    b1 |= 0b_0000_1000;
            if (q.Oscillatory)     b1 |= 0b_0000_0100;
            if (q.Failure)         b1 |= 0b_0000_0010;
            if (q.OldData)         b1 |= 0b_0000_0001;

            if (q.Inconsistent)    b2 |= 0b_1000_0000;
            if (q.Inaccurate)      b2 |= 0b_0100_0000;
            if (q.Source)          b2 |= 0b_0010_0000;
            if (q.Test)            b2 |= 0b_0001_0000;
            if (q.OperatorBlocked) b2 |= 0b_0000_1000;

            frame[offset++] = b1;
            frame[offset++] = b2;
        }

        public static void Encode_TimeSt_TLV(Span<byte> frame, ref int offset, byte tag, long ticks)
        {
            long seconds = ticks / 10000000;
            long fractions = 0xFFFFFFFF * (ticks % 10000000) / 10000000 + 1;

            frame[offset++] = tag;
            frame[offset++] = 0x08;

            frame[offset++] = (byte)(0xFF & seconds >> 24);
            frame[offset++] = (byte)(0xFF & seconds >> 16);
            frame[offset++] = (byte)(0xFF & seconds >>  8);
            frame[offset++] = (byte)(0xFF & seconds);

            frame[offset++] = (byte)(0xFF & fractions >> 24);
            frame[offset++] = (byte)(0xFF & fractions >> 16);
            frame[offset++] = (byte)(0xFF & fractions >>  8);
            frame[offset++] = 0x0A;
        }

        public static void Encode_OctetString_TLV(Span<byte> frame, ref int offset, byte tag, string str)
        {
            string hexStr = str.Replace(" ", "");

            if (hexStr.Length % 2 != 0)
            {
                hexStr = "0" + hexStr;
            }

            if (!Utils.IsHexString(hexStr))
            {
                throw new ArgumentException("Octet string must contain only hex characters");
            }

            Encode_RawBytes_TLV(frame, ref offset, tag, Utils.GetOctets(hexStr));
        }

        public static void Encode_BitString_TLV(Span<byte> frame, ref int offset, byte tag, string str)
        {
            string bitStr = str.Replace(" ", "");

            if (!Utils.IsHexString(bitStr))
            {
                throw new ArgumentException("Bit string must contain only '0' or '1' characters");
            }

            Encode_RawBytes_TLV(frame, ref offset, tag, Utils.GetBitString(bitStr));
        }
    }
}
