using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GooseScript
{
    internal sealed class ScriptEditor : RichTextBox
    {
        public void LoadText()
        {
            if (File.Exists(_srcPath))
                Text = File.ReadAllText(_srcPath);

            if (Text.Length == 0)
                Text = DefaultText;

            HighlightText();
        }

        public void HighlightText()
        {
            int pos = SelectionStart;

            SelectAll();
            SelectionColor = Color.Black;

            Font font = new Font(Font, FontStyle.Bold);

            string[] mmsTypes = { "BOOLEAN", "INT32", "INT32U", "FLOAT32", "BIT_STRING", "OCTET_STRING" };
            foreach (var str in mmsTypes)
            {
                HighlightString(Color.DarkSlateGray, font, str);
            }

            HighlightChars(Color.FromArgb(0x8C4B0E), new char[]
            {
                '=', '!',
                ':', ';',
                '.', ',',
                '{', '}',
                '(', ')',
                '[', ']',
                '"', '\'',
            });

            SelectionStart = pos;

            ClearUndo();
            SelectionStart = TextLength;
        }

        private void HighlightString(Color color, Font font, string str)
        {
            int pos = 0;

            while (true)
            {
                pos = Find(str, pos, RichTextBoxFinds.WholeWord);
                if (pos == -1)
                    break;
                else
                    pos++;
                
                SelectionColor = color;
                SelectionFont  = font;
            }
        }

        private void HighlightChars(Color color, char[] charsSet)
        {
            int pos = 0;

            while (true)
            {
                pos = Find(charsSet, pos);
                if (pos == -1)
                    break;
                else
                {
                    Select(pos, 1);
                    SelectionColor = color;
                    pos++;
                }
            }
        }

        public void InsertTab()
        {
            if (TextLength == 0)
                return;

            string[] lines = Lines;

            int cursorPos = SelectionStart;

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
                case 1: tab =  "   "; break;
                case 2: tab =   "  "; break;
                case 3: tab =    " "; break;
            }

            string newString = lines[lineIndex].Insert(charIndex, tab);
            lines[lineIndex] = newString;
            Lines = lines;

            SelectionStart = cursorPos += tab.Length;
        }

        public void DeleteString()
        {
            if (TextLength == 0)
                return;

            var lines = Lines.ToList();

            int cursorPos = SelectionStart;

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
            Lines = lines.ToArray();

            SelectionStart = sumLength;
        }

        public void Save()
        {
            SaveFile(_srcPath, RichTextBoxStreamType.PlainText);
        }

        private readonly string _srcPath = "GooseScript.cs";

        private readonly string DefaultText =
@"{
    var publisher = new GoosePublisher(new GooseSettings()
    {
        interfaceName = ""Ethernet N"",

        dstMac  = 0x01FF,
        appID   = 0xDEAD,
        vlanID  = 0x005,
        hasVlan = true,

        gocbRef = ""IED1LD1/LLN0$GO$GSE1"",
        datSet  = ""IED1LD1/LLN0$DataSet"",
        goId    = ""IED1LD1/LLN0.GSE1"",

        confRev = 1000,
        TAL =  400,

        mmsType = MMS_TYPE.BOOLEAN
    });

    publisher.Run(100, 1000);

    while (true)
    {
        Timer.Sleep(2500);

        publisher.Value = !publisher.Value;
    }
}
";
    }
}
