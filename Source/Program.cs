using System;

namespace GooseScript
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var publisher = new GoosePublisher<bool>(new GooseSettings()
            {
                interfaceName = "Ethernet",

                dstMac = 0x0001,
                appID  = 0xDEAD,
                vlanID = 0x005,
                hasVlan = true,

                gocbRef = "IED1LD1/LLN0$GO$GSE1",
                datSet  = "IED1LD1/LLN0$DataSet",
                goId    = "IED1LD1/LLN0.GSE1",

                confRev = 1000,
                TAL = 200
            });

            publisher.Value = true;
        }
    }
}
