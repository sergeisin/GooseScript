using System;

namespace GooseScript
{
    public class GooseSettings
    {
        /// <summary>
        /// Interface Name
        /// </summary>
        public string interfaceName;

        /// <summary>
        /// Destination MAC-address (last two octets in range 0x0000 .. 0x03FF)
        /// </summary>
        public ushort dstMac;

        /// <summary>
        /// Adding VLan tag to Goose frame
        /// </summary>
        public bool hasVlan;

        /// <summary>
        /// Virtual lan ID in range 0x000 - 0xFFF
        /// </summary>
        public ushort vlanID;

        /// <summary>
        /// Application Id
        /// </summary>
        public ushort appID;

        /// <summary>
        /// Reference to a GOOSE Control Block
        /// </summary>
        public string gocbRef;

        /// <summary>
        /// Reference to source Data Set
        /// </summary>
        public string datSet;

        /// <summary>
        /// Goose ID
        /// </summary>
        public string goId;

        /// <summary>
        /// Configuration revision. Default - 1.
        /// </summary>
        public uint confRev = 1;

        /// <summary>
        /// Simulation flag in Reserved1 section
        /// </summary>
        public bool simulation_reserved;

        /// <summary>
        /// Simulation flag in goosePdu section
        /// </summary>
        public bool simulation_goosePdu;

        /// <summary>
        /// ndsCom flag
        /// </summary>
        public bool ndsCom;

        /// <summary>
        /// Time allowed to live. Default - 1000 ms.
        /// </summary>
        public uint TAL = 1000;

        /// <summary>
        /// MMS Type of stVal
        /// </summary>
        public MMS_TYPE mmsType;

        /// <summary>
        /// Initial value of stVal
        /// </summary>
        public object initVal;

        /// <summary>
        /// Adding TimeStamp attribute to DataSet
        /// </summary>
        public bool hasTimeStamp;

        /// <summary>
        /// DO encoded as a structure
        /// </summary>
        public bool isStruct;

        /// <summary>
        /// Validate settings and set default initVal
        /// </summary>
        public void Init()
        {
            if (dstMac > 0x03FF)
            {
                throw new ArgumentException("The valid range for the 'dstMac' is from 0 to 3FF");
            }

            if (vlanID > 0x0FFF)
            {
                throw new ArgumentException("The valid range for the 'vlanID' is from 0 to FFF");
            }

            if (gocbRef is null)
            {
                throw new ArgumentException("The value 'gocbRef' cannot be null");
            }

            if (gocbRef.Length > 129)
            {
                throw new ArgumentException("'gocbRef' max length is 129 characters");
            }

            if (datSet is null)
            {
                throw new ArgumentException("The value 'datSet' cannot be null");
            }

            if (datSet.Length > 129)
            {
                throw new ArgumentException("'datSet' max length is 129 characters");
            }

            if (goId is null)
            {
                throw new ArgumentException("The value 'goId' cannot be null");
            }

            if (goId.Length > 129)
            {
                throw new ArgumentException("'goId' max length is 129 characters");
            }

            switch (mmsType)
            {
                case MMS_TYPE.BOOLEAN:
                    initVal = initVal ?? default(bool);
                    break;

                case MMS_TYPE.INT32:
                    initVal = initVal ?? default(int);
                    break;

                case MMS_TYPE.INT32U:
                    initVal = initVal ?? default(uint);
                    break;

                case MMS_TYPE.FLOAT32:
                    initVal = initVal ?? default(float);
                    break;

                case MMS_TYPE.BIT_STRING:
                    initVal = initVal ?? Dbpos.Off;
                    break;

                case MMS_TYPE.OCTET_STRING:
                    initVal = initVal ?? "6769746875622e636f6d2f73657267656973696E";
                    break;

                case MMS_TYPE.TimeStamp:
                    throw new ArgumentException("MMS type 'TimeStamp' is not available as stVal type");

                default:
                    mmsType = MMS_TYPE.BOOLEAN;
                    initVal = false;
                    break;
            }
        }
    }
}
