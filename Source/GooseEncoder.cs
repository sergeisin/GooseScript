using System;

namespace GooseScript
{
    internal static class GooseEncoder
    {
        public static byte[] GetTriplet(byte tag, byte[] data)
        {
            int lenBytes = data.Length < 0x80 ? 2 : 3;

            byte[] result = new byte[data.Length + lenBytes];

            result[0] = tag;

            if (lenBytes == 2)
            {
                result[1] = (byte)data.Length;
            }
            else
            {
                result[1] = 0x81;
                result[2] = (byte)data.Length;
            }

            for (int i = 0; i < data.Length; i++)
                result[i + lenBytes] = data[i];

            return result;
        }

        public static uint GetBerSize(uint value)
        {
            if (value > 0x7FFFFFFF) return 5;
            if (value >   0x7FFFFF) return 4;
            if (value >     0x7FFF) return 3;
            if (value >       0x7F) return 2;
                                    return 1;
        }

        public static uint GetBerSize(int value)
        {
            throw new NotImplementedException();
        }

        public static void AddRawBytes(Span<byte> frame, ref int offset, ReadOnlySpan<byte> src)
        {
            for (int i = 0; i < src.Length; i++)
            {
                frame[offset + i] = src[i];
            }

            offset += src.Length;
        }

        public static void AddUintTLV(Span<byte> frame, ref int offset, byte tag, uint value)
        {
            frame[offset++] = tag;

            switch (GetBerSize(value))
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

        public static void AddBoolTLV(Span<byte> frame, ref int offset, byte tag, bool value)
        {
            frame[offset++] = tag;
            frame[offset++] = 0x01;

            if (value)
                frame[offset++] = 0xff;
            else
                frame[offset++] = 0x00;
        }

        public static void AddQualityTLV(Span<byte> frame, ref int offset, Quality q)
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

        public static void AddTimeTLV(Span<byte> frame, ref int offset, byte tag, long ticks)
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
