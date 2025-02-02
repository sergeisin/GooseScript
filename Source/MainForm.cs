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
            if (File.Exists(codePath))
            {
                srcCode = File.ReadAllText(codePath);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            if (thread is null)
            {
                MethodInfo script = CompileUserScript();

                if (script is null)
                    return;

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

                button.Text = "Run";
            }
        }

        private MethodInfo CompileUserScript()
        {
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

            CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromSource(parameters, srcCode);

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
                MethodInfo mInfo = results.CompiledAssembly.GetType("UserCode.Program").GetMethod("Script");

                if (mInfo is null)
                {
                    MessageBox.Show("Method 'Script' not found", "Compile error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    return mInfo;
                }
            }

            return null;
        }

        private string codePath = @"C:\Users\Sergei\Desktop\WorkSpace\Script.cs";
        private string srcCode;
        private Thread thread;
    }
}
