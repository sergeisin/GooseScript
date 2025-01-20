using System;
using System.Windows.Forms;

namespace GooseScript
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var publisher = new GoosePublisher<int>(new GooseSettings()
            {
                interfaceName = "Ethernet 3",

                dstMac = 0x0001,
                appID  = 0xDEAD,
                vlanID = 0x005,
                hasVlan = true,

                gocbRef = "IED1LD1/LLN0$GO$GSE1",
                datSet = "IED1LD1/LLN0$DS1",
                goId = "IED1LD1/LLN0.GSE1",

                confRev = 1000,
                TAL = 200
            });

            publisher.Value = 42;

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());
        }
    }
}
