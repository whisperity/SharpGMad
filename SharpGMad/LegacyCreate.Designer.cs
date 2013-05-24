namespace SharpGMad
{
    partial class LegacyCreate
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LegacyCreate));
            this.lblFolder = new System.Windows.Forms.Label();
            this.lblFile = new System.Windows.Forms.Label();
            this.chkWarnInvalid = new System.Windows.Forms.CheckBox();
            this.btnFolderBrowse = new System.Windows.Forms.Button();
            this.btnFileBrowse = new System.Windows.Forms.Button();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.fbdFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.sfdOutFile = new System.Windows.Forms.SaveFileDialog();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblFolder
            // 
            this.lblFolder.AutoSize = true;
            this.lblFolder.Location = new System.Drawing.Point(12, 9);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(234, 13);
            this.lblFolder.TabIndex = 0;
            this.lblFolder.Text = "Create an addon from the contents of this folder:";
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(12, 31);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(114, 13);
            this.lblFile.TabIndex = 1;
            this.lblFile.Text = "Save the output file to:";
            // 
            // chkWarnInvalid
            // 
            this.chkWarnInvalid.AutoSize = true;
            this.chkWarnInvalid.Location = new System.Drawing.Point(15, 47);
            this.chkWarnInvalid.Name = "chkWarnInvalid";
            this.chkWarnInvalid.Size = new System.Drawing.Size(204, 17);
            this.chkWarnInvalid.TabIndex = 4;
            this.chkWarnInvalid.Text = "Continue even if files failed to validate";
            this.chkWarnInvalid.UseVisualStyleBackColor = true;
            // 
            // btnFolderBrowse
            // 
            this.btnFolderBrowse.Location = new System.Drawing.Point(533, 4);
            this.btnFolderBrowse.Name = "btnFolderBrowse";
            this.btnFolderBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnFolderBrowse.TabIndex = 1;
            this.btnFolderBrowse.Text = "Browse";
            this.btnFolderBrowse.UseVisualStyleBackColor = true;
            this.btnFolderBrowse.Click += new System.EventHandler(this.btnFolderBrowse_Click);
            // 
            // btnFileBrowse
            // 
            this.btnFileBrowse.Location = new System.Drawing.Point(533, 30);
            this.btnFileBrowse.Name = "btnFileBrowse";
            this.btnFileBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnFileBrowse.TabIndex = 3;
            this.btnFileBrowse.Text = "Browse";
            this.btnFileBrowse.UseVisualStyleBackColor = true;
            this.btnFileBrowse.Click += new System.EventHandler(this.btnFileBrowse_Click);
            // 
            // txtFile
            // 
            this.txtFile.Location = new System.Drawing.Point(252, 32);
            this.txtFile.Name = "txtFile";
            this.txtFile.Size = new System.Drawing.Size(275, 20);
            this.txtFile.TabIndex = 2;
            // 
            // txtFolder
            // 
            this.txtFolder.Location = new System.Drawing.Point(252, 6);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(275, 20);
            this.txtFolder.TabIndex = 0;
            // 
            // fbdFolder
            // 
            this.fbdFolder.Description = "Select the folder you want to compile into an addon";
            this.fbdFolder.ShowNewFolderButton = false;
            // 
            // sfdOutFile
            // 
            this.sfdOutFile.CheckFileExists = true;
            this.sfdOutFile.DefaultExt = "gma";
            this.sfdOutFile.Filter = "Garry\'s Mod Addons|*.gma";
            this.sfdOutFile.Title = "Save as addon";
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(15, 70);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(75, 23);
            this.btnCreate.TabIndex = 5;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnAbort.Location = new System.Drawing.Point(96, 70);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 6;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // LegacyCreate
            // 
            this.AcceptButton = this.btnCreate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnAbort;
            this.ClientSize = new System.Drawing.Size(618, 107);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.btnFileBrowse);
            this.Controls.Add(this.btnFolderBrowse);
            this.Controls.Add(this.chkWarnInvalid);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.lblFolder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LegacyCreate";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create addon";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.CheckBox chkWarnInvalid;
        private System.Windows.Forms.Button btnFolderBrowse;
        private System.Windows.Forms.Button btnFileBrowse;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.FolderBrowserDialog fbdFolder;
        private System.Windows.Forms.SaveFileDialog sfdOutFile;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnAbort;
    }
}