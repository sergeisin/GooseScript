using System;
using System.Collections.Generic;
using System.Text;
using SharpPcap;
using SharpPcap.LibPcap;

namespace GooseScript
{
    public class GoosePublisher<T>
    {
        private enum MMS_TYPE: byte
        {
            BOOLEAN = 0x83,
            INT32   = 0x85,
            INT32U  = 0x86,
            FLOAT32 = 0x87
        }

        private enum ASN1_Tag : byte
        {
            goosePDU          = 0x61,
            goCBRef           = 0x80,
            timeAllowedToLive = 0x81,
            dataSet           = 0x82,
            goID              = 0x83,
            TimeStamp         = 0x84,
            stNum             = 0x85,
            sqNum             = 0x86,
            simulation        = 0x87,
            confRev           = 0x88,
            ndsCom            = 0x89,
            numDatSetEntries  = 0x8a,
            allData           = 0xab
        }

        public GoosePublisher(GooseSettings settings)
        {
            settings.Validate();
            _settings = settings;

            switch (Value)
            {
                case bool  _: _mmsType = MMS_TYPE.BOOLEAN; break;
                case int   _: _mmsType = MMS_TYPE.INT32;   break;
                case uint  _: _mmsType = MMS_TYPE.INT32U;  break;
                case float _: _mmsType = MMS_TYPE.FLOAT32; break;

                default:
                    throw new NotImplementedException($"Type '{typeof(T).Name}' is not implemented!");
            }

            OpenDevice();
            MakeHeader();

            _raw_GoCbRef  = GooseEncoder.GetTriplet((byte)ASN1_Tag.goCBRef, Encoding.ASCII.GetBytes(settings.gocbRef));
            _raw_DatSet   = GooseEncoder.GetTriplet((byte)ASN1_Tag.dataSet, Encoding.ASCII.GetBytes(settings.datSet));
            _raw_GoId     = GooseEncoder.GetTriplet((byte)ASN1_Tag.goID,    Encoding.ASCII.GetBytes(settings.goId));

            AppID   = settings.appID;
            TAL     = settings.TAL;
            ConfRev = settings.confRev;
            NdsCom  = settings.ndsCom;

            Simulation_Reserved = settings.simulation_reserved;
            Simulation_GoosePDU = settings.simulation_goosePdu;
            
            UpdateState();
        }

        public void Send()
        {
            Span<byte> frame = stackalloc byte[512];
            int offset = 0;

            // Ethernet header
            GooseEncoder.AddRawBytes(frame, ref offset, _raw_Ethernet);

            // Goose.AppID
            frame[offset++] = (byte)(AppID >> 8);
            frame[offset++] = (byte)(AppID & 0xFF);

            // Gooose.Length
            frame[offset++] = 0x00;     // Not Implemented
            frame[offset++] = 0x00;     // Not Implemented

            // Goose.Reserveed
            if (Simulation_Reserved)
                frame[offset] = 0x80;
            offset += 4;

            // Goose.goosePDU
            frame[offset++] = (byte)ASN1_Tag.goosePDU;
            // Not Implemented
            // Not Implemented

            GooseEncoder.AddRawBytes(frame, ref offset, _raw_GoCbRef);
            GooseEncoder.AddUintTLV (frame, ref offset, (byte)ASN1_Tag.timeAllowedToLive, TAL);
            GooseEncoder.AddRawBytes(frame, ref offset, _raw_DatSet);
            GooseEncoder.AddRawBytes(frame, ref offset, _raw_GoId);

            GooseEncoder.AddTimeTLV(frame, ref offset, (byte)ASN1_Tag.TimeStamp,  _timeTicks);
            GooseEncoder.AddUintTLV(frame, ref offset, (byte)ASN1_Tag.stNum,      _stNum);
            GooseEncoder.AddUintTLV(frame, ref offset, (byte)ASN1_Tag.sqNum,      _sqNum);
            GooseEncoder.AddBoolTLV(frame, ref offset, (byte)ASN1_Tag.simulation, Simulation_GoosePDU);
            GooseEncoder.AddUintTLV(frame, ref offset, (byte)ASN1_Tag.confRev,    ConfRev);
            GooseEncoder.AddBoolTLV(frame, ref offset, (byte)ASN1_Tag.ndsCom,     NdsCom);

            frame[offset++] = (byte)ASN1_Tag.numDatSetEntries;
            frame[offset++] = 0x01;
            frame[offset++] = 0x02;

            frame[offset++] = (byte)ASN1_Tag.allData;
            // Not Implemented

            // Add MMS_TYPE Value
            // Add Quality

            // Send
            _device.SendPacket(frame.Slice(0, offset));

            _sqNum = (_sqNum == uint.MaxValue) ? 1 : _sqNum + 1;
        }

        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                UpdateState();
            }
        }

        public Quality Quality
        {
            get { return _quality; }
            set
            {
                _quality = value;
                UpdateState();
            }
        }

        private void OpenDevice()
        {
            foreach (var liveDevice in LibPcapLiveDeviceList.Instance)
            {
                if (liveDevice.Interface.FriendlyName == _settings.interfaceName)
                {
                    _device = liveDevice;
                    _device.Open();
                    break;
                }
            }

            if (_device == null)
                throw new FormatException($"Interface '{_settings.interfaceName}' not found!");
        }

        private void MakeHeader()
        {
            var header = new List<byte>(16);

            // DstMAC
            header.AddRange(new byte[] { 0x01, 0x0C, 0xCD, 0x01 });
            header.Add((byte)(_settings.dstMac >> 8));
            header.Add((byte)(_settings.dstMac & 0xff));

            // SrcMAC
            header.AddRange(_device.MacAddress.GetAddressBytes());

            if (_settings.hasVlan)
            {
                // VLan ethertype
                header.Add(0x81);
                header.Add(0x00);

                ushort vlanID = (ushort)(0x8000u + _settings.vlanID);
                header.Add((byte)(vlanID >> 8));
                header.Add((byte)(vlanID & 0xFF));
            }

            // Goose Ethertype
            header.Add(0x88);
            header.Add(0xb8);

            _raw_Ethernet = header.ToArray();
        }

        private void UpdateState()
        {
            _stNum = (_sqNum == uint.MaxValue) ? 1 : _stNum + 1;
            _sqNum = 0;

            long epochTicks = 621355968000000000;
            _timeTicks = DateTime.UtcNow.Ticks - epochTicks;
        }

        public uint AppID   { get; set; }
        public uint TAL     { get; set; }
        public uint ConfRev { get; set; }
        public bool NdsCom  { get; set; }

        public bool Simulation_Reserved { get; set; }
        public bool Simulation_GoosePDU { get; set; }
        public bool Simulation
        {
            get
            {
                return Simulation_Reserved || Simulation_GoosePDU;
            }

            set
            {
                Simulation_GoosePDU = value;
                Simulation_Reserved = value;
            }
        }

        private T _value;
        private MMS_TYPE _mmsType;
        private Quality _quality;

        private uint _stNum = 0;
        private uint _sqNum = 0;
        private long _timeTicks;

        private byte[] _raw_Ethernet;
        private byte[] _raw_GoCbRef;
        private byte[] _raw_DatSet;
        private byte[] _raw_GoId;

        private GooseSettings _settings;
        private LibPcapLiveDevice _device;
    }
}
