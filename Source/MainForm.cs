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
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(srcFile))
            {
                srcCode = File.ReadAllText(srcFile);
            }
            else
            {
                srcCode = ScriptText.Default;
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            if (thread is null)
            {
                MethodInfo script = CompileUserScript();

                thread = new Thread(() =>
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

                thread.Start();

                button.Text = "Stop";
            }
            else
            {
                if (thread.IsAlive)
                {
                    thread.Abort();
                    thread.Join();
                }

                thread = null;
                GC.Collect();

                button.Text = "Run Script";
            }
        }

        private MethodInfo CompileUserScript()
        {
            string programText = 
                $"using GooseScript;\n" +
                $"namespace UserCode {{ " +
                $"public static class Program {{ " +
                $"public static void Script() {srcCode} }} }}";

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

        private Thread thread;

        private string srcCode;
        private readonly string srcFile = "GooseScript.cs";
    }
}
