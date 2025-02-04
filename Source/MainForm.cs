using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
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

            int[] tabs = new int[32];

            for (int i = 0; i < tabs.Length; i++)
                tabs[i] = 28 + 28 * i;

            scriptEditor.SelectionTabs = tabs;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(srcFile))
            {
                scriptEditor.Text = File.ReadAllText(srcFile);
            }
            else
            {
                scriptEditor.Text = ScriptText.Default;
            }

            scriptEditor.SelectionStart = scriptEditor.TextLength;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            File.WriteAllText(srcFile, scriptEditor.Text);

            _scrThread?.Abort();
        }

        private void Button_Click(object sender, EventArgs e)
        {
            if (_scrThread is null)
            {
                StartScript();
            }
            else
            {
                StopScript();
            }
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
                catch (ThreadAbortException ex)
                {
                    Debug.WriteLine(ex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.InnerException.Message, "Script exception",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });

            _scrThread.Start();

            button.Text = "Stop";
        }

        private void StopScript()
        {
            if (_scrThread.IsAlive)
            {
                _scrThread.Abort();
                _scrThread.Join();
            }

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
                $"public static void Script() { scriptEditor.Text } }} }}";

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

        private readonly string srcFile = "GooseScript.cs";
    }
}
