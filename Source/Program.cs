using System;
using System.Threading;

namespace GooseScript
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            RunTest();
        }

        static void RunTest()
        {
            var publisher = new GoosePublisher(new GooseSettings()
            {
                interfaceName = "Ethernet 3",

                dstMac  = 0x01FF,
                appID   = 0xDEAD,
                vlanID  = 0x005,
                hasVlan = true,

                gocbRef = "IED1LD1/LLN0$GO$GSE1",
                datSet  = "IED1LD1/LLN0$DataSet",
                goId    = "IED1LD1/LLN0.GSE1",

                confRev = 1000,

                mmsType = MMS_TYPE.BIT_STRING
            });

            publisher.Send();

            Thread.Sleep(60_000);
        }
    }
}
