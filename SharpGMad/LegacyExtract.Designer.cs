namespace SharpGMad
{
    partial class LegacyExtract
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
            this.lblFolder = new System.Windows.Forms.Label();
            this.lblFile = new System.Windows.Forms.Label();
            this.btnFileBrowse = new System.Windows.Forms.Button();
            this.btnFolderBrowse = new System.Windows.Forms.Button();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.fbdFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.btnExtract = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.ofdFile = new System.Windows.Forms.OpenFileDialog();
            this.chkWriteLegacy = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lblFolder
            // 
            this.lblFolder.AutoSize = true;
            this.lblFolder.Location = new System.Drawing.Point(12, 9);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(212, 13);
            this.lblFolder.TabIndex = 0;
            this.lblFolder.Text = "Extract the contents of the following addon:";
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(12, 31);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(71, 13);
            this.lblFile.TabIndex = 1;
            this.lblFile.Text = "To this folder:";
            // 
            // btnFileBrowse
            // 
            this.btnFileBrowse.Location = new System.Drawing.Point(533, 4);
            this.btnFileBrowse.Name = "btnFileBrowse";
            this.btnFileBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnFileBrowse.TabIndex = 1;
            this.btnFileBrowse.Text = "Browse";
            this.btnFileBrowse.UseVisualStyleBackColor = true;
            this.btnFileBrowse.Click += new System.EventHandler(this.btnFileBrowse_Click);
            // 
            // btnFolderBrowse
            // 
            this.btnFolderBrowse.Location = new System.Drawing.Point(533, 30);
            this.btnFolderBrowse.Name = "btnFolderBrowse";
            this.btnFolderBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnFolderBrowse.TabIndex = 3;
            this.btnFolderBrowse.Text = "Browse";
            this.btnFolderBrowse.UseVisualStyleBackColor = true;
            this.btnFolderBrowse.Click += new System.EventHandler(this.btnFolderBrowse_Click);
            // 
            // txtFolder
            // 
            this.txtFolder.Location = new System.Drawing.Point(252, 32);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(275, 20);
            this.txtFolder.TabIndex = 2;
            // 
            // txtFile
            // 
            this.txtFile.Location = new System.Drawing.Point(252, 6);
            this.txtFile.Name = "txtFile";
            this.txtFile.Size = new System.Drawing.Size(275, 20);
            this.txtFile.TabIndex = 0;
            // 
            // fbdFolder
            // 
            this.fbdFolder.Description = "Select the destination folder where the addon will be extracted to";
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(15, 72);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(75, 23);
            this.btnExtract.TabIndex = 5;
            this.btnExtract.Text = "Extract";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnAbort.Location = new System.Drawing.Point(96, 72);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 6;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // ofdFile
            // 
            this.ofdFile.Filter = "Garry\'s Mod Addons|*.gma";
            this.ofdFile.Title = "Extract addon";
            // 
            // chkWriteLegacy
            // 
            this.chkWriteLegacy.AutoSize = true;
            this.chkWriteLegacy.Location = new System.Drawing.Point(252, 58);
            this.chkWriteLegacy.Name = "chkWriteLegacy";
            this.chkWriteLegacy.Size = new System.Drawing.Size(277, 17);
            this.chkWriteLegacy.TabIndex = 7;
            this.chkWriteLegacy.Text = "Create a legacy (Garry\'s Mod 12) info.txt metadata file";
            this.chkWriteLegacy.UseVisualStyleBackColor = true;
            // 
            // LegacyExtract
            // 
            this.AcceptButton = this.btnExtract;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnAbort;
            this.ClientSize = new System.Drawing.Size(618, 107);
            this.Controls.Add(this.chkWriteLegacy);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.btnExtract);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.btnFolderBrowse);
            this.Controls.Add(this.btnFileBrowse);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.lblFolder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LegacyExtract";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Extract addon";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.Button btnFileBrowse;
        private System.Windows.Forms.Button btnFolderBrowse;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.FolderBrowserDialog fbdFolder;
        private System.Windows.Forms.Button btnExtract;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.OpenFileDialog ofdFile;
        private System.Windows.Forms.CheckBox chkWriteLegacy;
    }
}