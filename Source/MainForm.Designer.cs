namespace GooseScript
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.button = new System.Windows.Forms.Button();
            this.externalPannel = new System.Windows.Forms.Panel();
            this.internalPanel = new System.Windows.Forms.Panel();
            this.scriptEditor = new GooseScript.ScriptEditor();
            this.externalPannel.SuspendLayout();
            this.internalPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // button
            // 
            this.button.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button.Location = new System.Drawing.Point(5, 572);
            this.button.Margin = new System.Windows.Forms.Padding(0);
            this.button.Name = "button";
            this.button.Size = new System.Drawing.Size(494, 34);
            this.button.TabIndex = 0;
            this.button.TabStop = false;
            this.button.Text = "Run Script";
            this.button.UseVisualStyleBackColor = true;
            this.button.Click += new System.EventHandler(this.Button_Click);
            // 
            // externalPannel
            // 
            this.externalPannel.Controls.Add(this.internalPanel);
            this.externalPannel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.externalPannel.Location = new System.Drawing.Point(5, 5);
            this.externalPannel.Name = "externalPannel";
            this.externalPannel.Padding = new System.Windows.Forms.Padding(1, 1, 1, 5);
            this.externalPannel.Size = new System.Drawing.Size(494, 567);
            this.externalPannel.TabIndex = 1;
            // 
            // internalPanel
            // 
            this.internalPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.internalPanel.Controls.Add(this.scriptEditor);
            this.internalPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.internalPanel.Location = new System.Drawing.Point(1, 1);
            this.internalPanel.Name = "internalPanel";
            this.internalPanel.Size = new System.Drawing.Size(492, 561);
            this.internalPanel.TabIndex = 0;
            // 
            // scriptEditor
            // 
            this.scriptEditor.AcceptsTab = true;
            this.scriptEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(249)))), ((int)(((byte)(251)))));
            this.scriptEditor.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scriptEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptEditor.Font = new System.Drawing.Font("Consolas", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.scriptEditor.ForeColor = System.Drawing.Color.Black;
            this.scriptEditor.Location = new System.Drawing.Point(0, 0);
            this.scriptEditor.Name = "scriptEditor";
            this.scriptEditor.Size = new System.Drawing.Size(490, 559);
            this.scriptEditor.TabIndex = 0;
            this.scriptEditor.Text = "";
            this.scriptEditor.WordWrap = false;
            this.scriptEditor.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ScriptEditor_KeyPress);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 611);
            this.Controls.Add(this.externalPannel);
            this.Controls.Add(this.button);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(400, 500);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GooseScript v1.2";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.externalPannel.ResumeLayout(false);
            this.internalPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button;
        private ScriptEditor scriptEditor;
        private System.Windows.Forms.Panel externalPannel;
        private System.Windows.Forms.Panel internalPanel;
    }
}