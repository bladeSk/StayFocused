namespace StayFocused
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent () {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.buttonExit = new System.Windows.Forms.Button();
            this.linkHomepage = new System.Windows.Forms.LinkLabel();
            this.buttonGotoConfig = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(13, 13);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(439, 218);
            this.textBoxLog.TabIndex = 0;
            // 
            // buttonExit
            // 
            this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExit.Location = new System.Drawing.Point(350, 246);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(102, 23);
            this.buttonExit.TabIndex = 1;
            this.buttonExit.Text = "Unload && Exit";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // linkHomepage
            // 
            this.linkHomepage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkHomepage.AutoSize = true;
            this.linkHomepage.Location = new System.Drawing.Point(12, 251);
            this.linkHomepage.Name = "linkHomepage";
            this.linkHomepage.Size = new System.Drawing.Size(95, 13);
            this.linkHomepage.TabIndex = 2;
            this.linkHomepage.TabStop = true;
            this.linkHomepage.Text = "Github/Homepage";
            this.linkHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHomepage_LinkClicked);
            // 
            // buttonGotoConfig
            // 
            this.buttonGotoConfig.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonGotoConfig.Location = new System.Drawing.Point(179, 246);
            this.buttonGotoConfig.Name = "buttonGotoConfig";
            this.buttonGotoConfig.Size = new System.Drawing.Size(106, 23);
            this.buttonGotoConfig.TabIndex = 3;
            this.buttonGotoConfig.Text = "Open Config File";
            this.buttonGotoConfig.UseVisualStyleBackColor = true;
            this.buttonGotoConfig.Click += new System.EventHandler(this.buttonGotoConfig_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 281);
            this.Controls.Add(this.buttonGotoConfig);
            this.Controls.Add(this.linkHomepage);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.textBoxLog);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 200);
            this.Name = "Form1";
            this.Text = "Stay Focused (Beta3)";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.LinkLabel linkHomepage;
        private System.Windows.Forms.Button buttonGotoConfig;
    }
}

