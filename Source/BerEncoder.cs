using System;
using System.Text;

namespace GooseScript
{
    internal static class BerEncoder
    {
        public static int GetEncoded_V_Size(uint value)
        {
            if (value > 0x7FFFFFFF) return 5;
            if (value >   0x7FFFFF) return 4;
            if (value >     0x7FFF) return 3;
            if (value >       0x7F) return 2;
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

        public static byte[] GetEncodedTLV(byte tag, string str)
        {
            byte[] data = Encoding.ASCII.GetBytes(str);

            int sizeof_TAG = 1;
            int sizeof_LEN = GetEncoded_L_Size(data.Length);
            int sizeof_VAL = data.Length;

            var buffer = new byte[sizeof_TAG + sizeof_LEN + sizeof_VAL];
            int bufPos = 0;

            buffer[bufPos++] = tag;

            switch (sizeof_LEN)
            {
                case 1:
                    buffer[bufPos++] = (byte)sizeof_VAL;
                    break;

                case 2:
                    buffer[bufPos++] = 0x81;
                    buffer[bufPos++] = (byte)sizeof_VAL;
                    break;

                case 3:
                    buffer[bufPos++] = 0x82;
                    buffer[bufPos++] = (byte)(sizeof_VAL >> 8);
                    buffer[bufPos++] = (byte)(sizeof_VAL & 0xFF);
                    break;
            }

            Array.Copy(data, 0, buffer, bufPos, data.Length);

            return buffer;
        }

        public static void Encode_TL(Span<byte> frame, ref int offset, byte tag, int len)
        {
            throw new NotImplementedException();
        }

        public static void Encode_RawBytes(Span<byte> frame, ref int offset, ReadOnlySpan<byte> src)
        {
            for (int i = 0; i < src.Length; i++)
            {
                frame[offset + i] = src[i];
            }

            offset += src.Length;
        }

        public static void Encode_INT32U_TLV(Span<byte> frame, ref int offset, byte tag, uint value)
        {
            frame[offset++] = tag;

            switch (GetEncoded_V_Size(value))
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

        public static void Encode_INT32_TLV(Span<byte> frame, ref int offset, byte tag, int value)
        {
            throw new NotImplementedException();
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

        public static void Encode_TimeStamp_TLV(Span<byte> frame, ref int offset, byte tag, long ticks)
        {
            long seconds = ticks / 10000000;
            long fractions = 0xFFFFFFFF * (ticks % 10000000) / 10000000 + 1;

            frame[offset++] = tag;      // Tag
            frame[offset++] = 0x08;     // Len

            frame[offset++] = (byte)(0xFF & seconds >> 24);
            frame[offset++] = (byte)(0xFF & seconds >> 16);
            frame[offset++] = (byte)(0xFF & seconds >> 8);
            frame[offset++] = (byte)(0xFF & seconds);

            frame[offset++] = (byte)(0xFF & fractions >> 24);
            frame[offset++] = (byte)(0xFF & fractions >> 16);
            frame[offset++] = (byte)(0xFF & fractions >> 8);
            frame[offset++] = 0x0A;
        }
    }
}
