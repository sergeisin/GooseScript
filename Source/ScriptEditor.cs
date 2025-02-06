﻿using System.Linq;
using System.Windows.Forms;

namespace GooseScript
{
    internal static class ScriptEditor
    {
        public static void InsertTab(RichTextBox editor)
        {
            if (editor.TextLength == 0)
                return;

            string[] lines = editor.Lines;

            int cursorPos = editor.SelectionStart;

            int sumLength = 0;
            int lineIndex = 0;

            while (sumLength <= cursorPos)
            {
                sumLength += (lines[lineIndex].Length + 1);
                lineIndex++;
            }

            lineIndex--;
            sumLength -= (lines[lineIndex].Length + 1);

            int charIndex = cursorPos - sumLength;
            int padding = charIndex % 4;

            string tab = "";
            switch (padding)
            {
                case 0: tab = "    "; break;
                case 1: tab = "   "; break;
                case 2: tab = "  "; break;
                case 3: tab = " "; break;
            }

            string newString = lines[lineIndex].Insert(charIndex, tab);
            lines[lineIndex] = newString;
            editor.Lines = lines;

            editor.SelectionStart = cursorPos += tab.Length;
        }

        public static void DeleteString(RichTextBox editor)
        {
            if (editor.TextLength == 0)
                return;

            var lines = editor.Lines.ToList();

            int cursorPos = editor.SelectionStart;

            int sumLength = 0;
            int lineIndex = 0;

            while (sumLength <= cursorPos)
            {
                sumLength += (lines[lineIndex].Length + 1);
                lineIndex++;
            }

            lineIndex--;
            sumLength -= (lines[lineIndex].Length + 1);

            lines.RemoveAt(lineIndex);
            editor.Lines = lines.ToArray();

            editor.SelectionStart = sumLength;
        }

        public static readonly string DefaultText =
@"{
    var publisher = new GoosePublisher(new GooseSettings()
    {
        interfaceName = ""Ethernet X"",

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
        Timer.Sleep(500);
    }
}
";

    }
}
