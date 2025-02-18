using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SharpPcap;
using SharpPcap.LibPcap;

namespace GooseScript
{
    public class GoosePublisher
    {
        public GoosePublisher(GooseSettings settings)
        {
            _mainThread = Thread.CurrentThread;

            _settings = settings;
            _settings.Init();

            OpenDevice();
            MakeHeader();

            _raw_GoCbRef = BerEncoder.Create_VisibleString_TLV((byte)ASN1_Tag.goCBRef, settings.gocbRef);
            _raw_DatSet  = BerEncoder.Create_VisibleString_TLV((byte)ASN1_Tag.dataSet, settings.datSet);
            _raw_GoId    = BerEncoder.Create_VisibleString_TLV((byte)ASN1_Tag.goID,    settings.goId);

            _appID   = settings.appID;
            _TAL     = settings.TAL;
            _confRev = settings.confRev;
            _ndsCom  = settings.ndsCom;

            _simReserved = settings.simulation_reserved;
            _simGoosePDU = settings.simulation_goosePdu;

            // Reserved for AllData
            _DatSet_Buffer = new byte[MaxGooseSize];

            _mmsType = settings.mmsType;
            _value   = settings.initVal;

            UpdateState();
        }

        public void Run(int minTime, int maxTime)
        {
            if (minTime < 2)
                minTime = 2;

            if (maxTime > 10_000)
                maxTime = 10_000;

            _running = true;

            Task.Run(() =>
            {
                long ticksInMs = Stopwatch.Frequency / 1000;

                int retInterval = 0;

                var sw = Stopwatch.StartNew();

                while (_running)
                {
                    // Retransmission mechanism defined by the standard
                    // СТО 56947007-25.040.30.309-2020

                    if (sw.ElapsedTicks >= Interlocked.Read(ref _nextTicks))
                    {
                        // Stopping when the parent thread terminates
                        if (!_mainThread.IsAlive)
                            break;

                        if (_sqNum < 4)
                        {
                            retInterval = minTime;
                        }
                        else if (_sqNum < 7)
                        {
                            retInterval *= 2;

                            if (retInterval > maxTime)
                                retInterval = maxTime;
                        }
                        else
                        {
                            retInterval = maxTime;
                        }

                        long nextTicks = sw.ElapsedTicks + retInterval * ticksInMs;
                        Interlocked.Exchange(ref _nextTicks, nextTicks);

                        _TAL = (uint)(retInterval * 3);
                        
                        Send();
                    }

                    Timer.Sleep(1);
                }
                
                sw.Stop();
            });
        }

        public void Stop()
        {
            _running = false;
        }

        public void Send()
        {
            lock (_locker)
            {
                int goosePduSize = GetGoosePduSize();
                int gooseLength  = 9 + BerEncoder.GetEncoded_L_Size(goosePduSize) + goosePduSize;
                int rawFrameSize = gooseLength + 18;
                
                if (rawFrameSize > MaxFrameSize)
                {
                    throw new Exception($"Frame size '{rawFrameSize}' exceeds MTU ({MaxFrameSize} bytes)");
                }

                Span<byte> frame = stackalloc byte[rawFrameSize];
                int offset = 0;

                // Ethernet header
                BerEncoder.Encode_RawBytes(frame, ref offset, _raw_Ethernet);

                // Goose.AppID
                frame[offset++] = (byte)(_appID >> 8);
                frame[offset++] = (byte)(_appID & 0xFF);

                // Gooose.Length
                frame[offset++] = (byte)(gooseLength >> 8);
                frame[offset++] = (byte)(gooseLength & 0xFF);

                // Goose.Reserveed
                if (_simReserved)
                    frame[offset] = 0x80;
                offset += 4;

                // BER encoed
                BerEncoder.Encode_TL_Only     (frame, ref offset, (byte)ASN1_Tag.goosePDU, goosePduSize);
                BerEncoder.Encode_RawBytes    (frame, ref offset, _raw_GoCbRef);
                BerEncoder.Encode_INT32U_TLV  (frame, ref offset, (byte)ASN1_Tag.timeAllowedToLive, _TAL);
                BerEncoder.Encode_RawBytes    (frame, ref offset, _raw_DatSet);
                BerEncoder.Encode_RawBytes    (frame, ref offset, _raw_GoId);
                BerEncoder.Encode_TimeSt_TLV  (frame, ref offset, (byte)ASN1_Tag.TimeStamp, _timeTicks);
                BerEncoder.Encode_INT32U_TLV  (frame, ref offset, (byte)ASN1_Tag.stNum, _stNum);
                BerEncoder.Encode_INT32U_TLV  (frame, ref offset, (byte)ASN1_Tag.sqNum, _sqNum);
                BerEncoder.Encode_Boolean_TLV (frame, ref offset, (byte)ASN1_Tag.simulation, _simGoosePDU);
                BerEncoder.Encode_INT32U_TLV  (frame, ref offset, (byte)ASN1_Tag.confRev, _confRev);
                BerEncoder.Encode_Boolean_TLV (frame, ref offset, (byte)ASN1_Tag.ndsCom, _ndsCom);

                frame[offset++] = (byte)ASN1_Tag.numDatSetEntries;
                frame[offset++] = 0x01;
                frame[offset++] = 0x03;

                BerEncoder.Encode_TL_Only  (frame, ref offset, (byte)ASN1_Tag.allData, _DatSet_Size);
                BerEncoder.Encode_RawBytes (frame, ref offset, _DatSet_Buffer, _DatSet_Size);

                // Send
                _device.SendPacket(frame.Slice(0, offset));

                _sqNum = (_sqNum == uint.MaxValue) ? 1 : _sqNum + 1;
            }
        }

        public void SendFew(int count, int sleepTime)
        {
            for (int i = 0; i < count; i++)
            {
                Send();
                Timer.Sleep(sleepTime);
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
            {
                throw new ArgumentException($"Interface '{_settings.interfaceName}' not found!");
            }
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
            TypeCheck();

            long epochTicks = 621355968000000000;
            _timeTicks = DateTime.UtcNow.Ticks - epochTicks;

            try
            {
                MakeDataSet();
            }
            catch (IndexOutOfRangeException)
            {
                throw new Exception("DataSet is too large");
            }

            _stNum = (_sqNum == uint.MaxValue) ? 1 : _stNum + 1;
            _sqNum = 0;

            Interlocked.Exchange(ref _nextTicks, 0);
        }

        private void TypeCheck()
        {
            Type valueType = _value?.GetType();

            bool typeMismatch = true;

            switch (_mmsType)
            {
                case MMS_TYPE.BOOLEAN:
                    if (valueType == typeof(bool))
                        typeMismatch = false;
                    break;

                case MMS_TYPE.BIT_STRING:
                    if (valueType == typeof(string))
                        typeMismatch = false;
                    break;

                case MMS_TYPE.INT32:
                    if (valueType == typeof(uint) ||
                        valueType == typeof(int))
                        typeMismatch = false;
                    break;

                case MMS_TYPE.INT32U:
                    if (valueType == typeof(uint) ||
                        valueType == typeof(int))
                        typeMismatch = false;
                    break;

                case MMS_TYPE.FLOAT32:
                    if (valueType == typeof(double) ||
                        valueType == typeof(float)  ||
                        valueType == typeof(int))
                        typeMismatch = false;
                    break;

                case MMS_TYPE.OCTET_STRING:
                    if (valueType == typeof(string))
                        typeMismatch = false;
                    break;

                case MMS_TYPE.TimeStamp:
                    throw new Exception("Unknown MMS data type");
            }

            if (typeMismatch)
            {
                throw new Exception("MMS type mismatch!");
            }
        }

        private void MakeDataSet()
        {
            _DatSet_Size = 0;

            switch (_mmsType)
            {
                case MMS_TYPE.BOOLEAN:
                    BerEncoder.Encode_Boolean_TLV(_DatSet_Buffer, ref _DatSet_Size, (byte)_mmsType, Convert.ToBoolean(_value));
                    break;

                case MMS_TYPE.INT32:
                    BerEncoder.Encode_INT32S_TLV(_DatSet_Buffer, ref _DatSet_Size, (byte)_mmsType, Convert.ToInt32(_value));
                    break;

                case MMS_TYPE.INT32U:
                    BerEncoder.Encode_INT32U_TLV(_DatSet_Buffer, ref _DatSet_Size, (byte)_mmsType, Convert.ToUInt32(_value));
                    break;

                case MMS_TYPE.FLOAT32:
                    BerEncoder.Encode_FLOAT_TLV(_DatSet_Buffer, ref _DatSet_Size, (byte)_mmsType, Convert.ToSingle(_value));
                    break;

                case MMS_TYPE.BIT_STRING:
                    BerEncoder.Encode_BitString_TLV(_DatSet_Buffer, ref _DatSet_Size, (byte)_mmsType, Convert.ToString(_value));
                    break;

                case MMS_TYPE.OCTET_STRING:
                    BerEncoder.Encode_OctetString_TLV(_DatSet_Buffer, ref _DatSet_Size, (byte)_mmsType, Convert.ToString(_value));
                    break;
            }

            BerEncoder.Encode_Quality_TLV(_DatSet_Buffer, ref _DatSet_Size, _quality);
            BerEncoder.Encode_TimeSt_TLV(_DatSet_Buffer, ref _DatSet_Size, (byte)MMS_TYPE.TimeStamp, _timeTicks);
        }

        private int GetGoosePduSize()
        {
            int goosePduSize = 0;

            // Encoded AllData size
            goosePduSize += 1;
            goosePduSize += BerEncoder.GetEncoded_L_Size(_DatSet_Size);
            goosePduSize += _DatSet_Size;

            // Encoded NumDataSetEntries size
            goosePduSize += 3;

            // Encoded NdsCom size
            goosePduSize += 3;

            // Encoded ConfRev size
            goosePduSize += (2 + BerEncoder.GetEncoded_INT32U_Size(_confRev));

            // Encoded Simulation size
            goosePduSize += 3;

            // Encoded sqNum size
            goosePduSize += (2 + BerEncoder.GetEncoded_INT32U_Size(_sqNum));

            // Encoded stNum size
            goosePduSize += (2 + BerEncoder.GetEncoded_INT32U_Size(_stNum));

            // Encoded TimeStamp size
            goosePduSize += 10;

            // Encoded GoID size
            goosePduSize += _raw_GoId.Length;

            // Encoded datSet size
            goosePduSize += _raw_DatSet.Length;

            // Encoded TAL size
            goosePduSize += (2 + BerEncoder.GetEncoded_INT32U_Size(_TAL));

            // Encoded goCbRef size
            goosePduSize += _raw_GoCbRef.Length;

            return goosePduSize;
        }

        public dynamic Value
        {
            get
            {
                lock (_locker)
                {
                    return _value;
                }
            }
            set
            {
                lock (_locker)
                {
                    _value = value;
                    UpdateState();
                }
            }
        }

        public Quality Quality
        {
            get
            {
                lock (_locker)
                {
                    return _quality;
                }
            }
            set
            {
                lock (_locker)
                {
                    _quality = value;
                    UpdateState();
                }
            }
        }

        public bool Simulation
        {
            get
            {
                lock (_locker)
                {
                    return _simGoosePDU || _simReserved;
                }
            }
            set
            {
                lock (_locker)
                {
                    _simReserved = value;
                    _simGoosePDU = value;
                }
            }
        }

        public uint StNum
        {
            get { lock (_locker) { return _stNum;  } }
            set { lock (_locker) { _stNum = value; } }
        }

        public uint SqNum
        {
            get { lock (_locker) { return _sqNum;  } }
            set { lock (_locker) { _sqNum = value; } }
        }

        public uint TAL
        {
            get { lock (_locker) { return _TAL;  } }
            set { lock (_locker) { _TAL = value; } }
        }

        private object   _value;
        private MMS_TYPE _mmsType;
        private Quality  _quality;

        private uint _appID;
        private uint _confRev;
        private uint _TAL;

        private uint _stNum = 0;
        private uint _sqNum = 0;
        private long _timeTicks;

        private bool _ndsCom;
        private bool _simReserved;
        private bool _simGoosePDU;

        private byte[] _raw_Ethernet;
        private byte[] _raw_GoCbRef;
        private byte[] _raw_DatSet;
        private byte[] _raw_GoId;

        private byte[] _DatSet_Buffer;
        private int    _DatSet_Size;

        private bool _running;

        private GooseSettings _settings;
        private LibPcapLiveDevice _device;

        private Thread _mainThread;
        private long _nextTicks;

        private object _locker = new object();

        private const int MaxFrameSize = 1500;
        private const int MaxGooseSize = 1400;
    }
}
