
namespace GooseScript
{
    public enum MMS_TYPE : byte
    {
        BOOLEAN      = 0x83,
        BIT_STRING   = 0x84,
        INT32        = 0x85,
        INT32U       = 0x86,
        FLOAT32      = 0x87,
        OCTET_STRING = 0x89,
        TimeStamp    = 0x91,
    }
    
    public enum ASN1_Tag : byte
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
        allData           = 0xab,
        structure         = 0xa2,
    }

    public static class Dbpos
    {
        public static readonly string Intermediate = "00";
        public static readonly string Off          = "01";
        public static readonly string On           = "10";
        public static readonly string BadState     = "11";
    }
}
