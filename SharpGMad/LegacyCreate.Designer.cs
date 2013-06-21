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
            this.cmbTag2 = new System.Windows.Forms.ComboBox();
            this.cmbTag1 = new System.Windows.Forms.ComboBox();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.lblTags = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.gboConvertMetadata = new System.Windows.Forms.GroupBox();
            this.chkConvertNeeded = new System.Windows.Forms.CheckBox();
            this.gboConvertMetadata.SuspendLayout();
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
            this.chkWarnInvalid.Location = new System.Drawing.Point(358, 60);
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
            this.btnCreate.Location = new System.Drawing.Point(358, 95);
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
            this.btnAbort.Location = new System.Drawing.Point(439, 95);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 6;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // cmbTag2
            // 
            this.cmbTag2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTag2.FormattingEnabled = true;
            this.cmbTag2.Location = new System.Drawing.Point(204, 46);
            this.cmbTag2.Name = "cmbTag2";
            this.cmbTag2.Size = new System.Drawing.Size(121, 21);
            this.cmbTag2.Sorted = true;
            this.cmbTag2.TabIndex = 15;
            // 
            // cmbTag1
            // 
            this.cmbTag1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTag1.FormattingEnabled = true;
            this.cmbTag1.Location = new System.Drawing.Point(77, 46);
            this.cmbTag1.Name = "cmbTag1";
            this.cmbTag1.Size = new System.Drawing.Size(121, 21);
            this.cmbTag1.Sorted = true;
            this.cmbTag1.TabIndex = 14;
            // 
            // cmbType
            // 
            this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Location = new System.Drawing.Point(77, 19);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(248, 21);
            this.cmbType.Sorted = true;
            this.cmbType.TabIndex = 13;
            // 
            // lblTags
            // 
            this.lblTags.AutoSize = true;
            this.lblTags.Location = new System.Drawing.Point(8, 49);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(34, 13);
            this.lblTags.TabIndex = 17;
            this.lblTags.Text = "Tags:";
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(8, 22);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(34, 13);
            this.lblType.TabIndex = 16;
            this.lblType.Text = "Type:";
            // 
            // gboConvertMetadata
            // 
            this.gboConvertMetadata.Controls.Add(this.cmbType);
            this.gboConvertMetadata.Controls.Add(this.cmbTag2);
            this.gboConvertMetadata.Controls.Add(this.lblType);
            this.gboConvertMetadata.Controls.Add(this.cmbTag1);
            this.gboConvertMetadata.Controls.Add(this.lblTags);
            this.gboConvertMetadata.Location = new System.Drawing.Point(12, 83);
            this.gboConvertMetadata.Name = "gboConvertMetadata";
            this.gboConvertMetadata.Size = new System.Drawing.Size(335, 78);
            this.gboConvertMetadata.TabIndex = 18;
            this.gboConvertMetadata.TabStop = false;
            this.gboConvertMetadata.Visible = false;
            // 
            // chkConvertNeeded
            // 
            this.chkConvertNeeded.AutoSize = true;
            this.chkConvertNeeded.Location = new System.Drawing.Point(15, 60);
            this.chkConvertNeeded.Name = "chkConvertNeeded";
            this.chkConvertNeeded.Size = new System.Drawing.Size(181, 17);
            this.chkConvertNeeded.TabIndex = 19;
            this.chkConvertNeeded.Text = "Convert Garry\'s Mod 12 structure";
            this.chkConvertNeeded.UseVisualStyleBackColor = true;
            this.chkConvertNeeded.CheckedChanged += new System.EventHandler(this.chkConvertNeeded_CheckedChanged);
            // 
            // LegacyConvert
            // 
            this.AcceptButton = this.btnCreate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnAbort;
            this.ClientSize = new System.Drawing.Size(617, 166);
            this.Controls.Add(this.chkConvertNeeded);
            this.Controls.Add(this.gboConvertMetadata);
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
            this.Name = "LegacyConvert";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create addon";
            this.Load += new System.EventHandler(this.LegacyCreate_Load);
            this.gboConvertMetadata.ResumeLayout(false);
            this.gboConvertMetadata.PerformLayout();
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
        private System.Windows.Forms.ComboBox cmbTag2;
        private System.Windows.Forms.ComboBox cmbTag1;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.GroupBox gboConvertMetadata;
        private System.Windows.Forms.CheckBox chkConvertNeeded;
    }
}