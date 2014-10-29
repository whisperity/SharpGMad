namespace SharpGMad
{
    partial class AddAs
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
            this.cOriginalFilename = new System.Windows.Forms.Label();
            this.filename = new System.Windows.Forms.TextBox();
            this.add = new System.Windows.Forms.Button();
            this.originalFilename = new System.Windows.Forms.Label();
            this.cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cOriginalFilename
            // 
            this.cOriginalFilename.AutoSize = true;
            this.cOriginalFilename.Location = new System.Drawing.Point(12, 9);
            this.cOriginalFilename.Name = "cOriginalFilename";
            this.cOriginalFilename.Size = new System.Drawing.Size(84, 13);
            this.cOriginalFilename.TabIndex = 0;
            this.cOriginalFilename.Text = "Original filename";
            // 
            // filename
            // 
            this.filename.Location = new System.Drawing.Point(12, 53);
            this.filename.Name = "filename";
            this.filename.Size = new System.Drawing.Size(472, 20);
            this.filename.TabIndex = 1;
            this.filename.TextChanged += new System.EventHandler(this.filename_TextChanged);
            // 
            // add
            // 
            this.add.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.add.Location = new System.Drawing.Point(328, 79);
            this.add.Name = "add";
            this.add.Size = new System.Drawing.Size(75, 23);
            this.add.TabIndex = 2;
            this.add.Text = "Add";
            this.add.UseVisualStyleBackColor = true;
            this.add.Click += new System.EventHandler(this.add_Click);
            // 
            // originalFilename
            // 
            this.originalFilename.Location = new System.Drawing.Point(102, 9);
            this.originalFilename.Name = "originalFilename";
            this.originalFilename.Size = new System.Drawing.Size(382, 41);
            this.originalFilename.TabIndex = 3;
            // 
            // cancel
            // 
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(409, 79);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 4;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // AddAs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(496, 108);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.originalFilename);
            this.Controls.Add(this.add);
            this.Controls.Add(this.filename);
            this.Controls.Add(this.cOriginalFilename);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddAs";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AddAs";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AddAs_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox filename;
        private System.Windows.Forms.Button add;
        private System.Windows.Forms.Label originalFilename;
        private System.Windows.Forms.Label cOriginalFilename;
        private System.Windows.Forms.Button cancel;

    }
}