
namespace GooseScript
{
    internal static class ScriptText
    {
        public static readonly string Default =
@"{
    var publisher = new GoosePublisher(new GooseSettings()
    {
        interfaceName = ""Ethernet 3"",

        dstMac  = 0x01FF,
        appID   = 0xDEAD,
        vlanID  = 0x005,
        hasVlan = true,

        gocbRef = ""IED1LD1/LLN0$GO$GSE1"",
        datSet  = ""IED1LD1/LLN0$DataSet"",
        goId    = ""IED1LD1/LLN0.GSE1"",

        confRev = 1000,
        TAL =  400,

        mmsType = MMS_TYPE.OCTET_STRING
    });

    publisher.Run(100, 1000);

    while(true)
    {
        NtTimer.Sleep(500);
    }
}
";
    }
}
