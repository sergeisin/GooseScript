using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.CSharp;

namespace GooseScript
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            scriptEditor.LoadText();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            scriptEditor.Save();

            _scrThread?.Abort();
        }

        private void ScriptEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                // Tab
                case '\t':
                    e.Handled = true;
                    (sender as ScriptEditor).InsertTab(); 
                    break;

                // Ctrl + X
                case '\u0018':
                    e.Handled = true;
                    (sender as ScriptEditor).DeleteString();
                    break;
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            scriptEditor.HighlightText();

            if (_scrThread != null && _scrThread.IsAlive)
            {
                StopScript();
            }
            else
            {
                StartScript();
            }

            scriptEditor.Focus();
        }

        private void StartScript()
        {
            MethodInfo script = CompileUserScript();

            if (script is null)
                return;

            _scrThread = new Thread(() =>
            {
                try
                {
                    script.Invoke(null, null);
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.InnerException.Message, "Script exception",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                button.BeginInvoke((Action)(() => button.Text = "Run Script"));
            });

            _scrThread.Start();

            button.Text = "Stop";
        }

        private void StopScript()
        {
            if (_scrThread.IsAlive)
                _scrThread.Abort();

            _scrThread = null;
            GC.Collect();

            button.Text = "Run Script";
        }

        private MethodInfo CompileUserScript()
        {
            string programText =
                $"using GooseScript;\n" +
                $"namespace UserCode {{ " +
                $"public static class Program {{ " +
                $"public static void Script() {{ \n { scriptEditor.Text } \n}} }} }}";

            var parameters = new CompilerParameters()
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                CompilerOptions = "/optimize+"
            };

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                parameters.ReferencedAssemblies.Add(assembly.Location);
            }

            // To Do
            string wtf_1 = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Microsoft.CSharp.dll";
            string wtf_2 = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Core.dll";
            parameters.ReferencedAssemblies.Add(wtf_1);
            parameters.ReferencedAssemblies.Add(wtf_2);

            CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromSource(parameters, programText);

            if (results.Errors.HasErrors)
            {
                var sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                    sb.AppendLine(error.ErrorText);

                MessageBox.Show(sb.ToString(), "Compile error",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                return results.CompiledAssembly.GetType("UserCode.Program").GetMethod("Script");
            }

            return null;
        }

        private Thread _scrThread;
    }
}
