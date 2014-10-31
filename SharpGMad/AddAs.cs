using System;
using System.Windows.Forms;

namespace SharpGMad
{
    partial class AddAs : Form
    {
        public string Filename;

        public AddAs(string originalFilename, string txtFilename)
        {
            InitializeComponent();
            
            Filename = originalFilename;
            this.Text = "Add file as...";
            this.originalFilename.Text = originalFilename;
            this.filename.Text = txtFilename ?? originalFilename;
        }

        private void filename_TextChanged(object sender, EventArgs e)
        {
            Filename = filename.Text;
        }

        private void add_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void AddAs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                add_Click(sender, e);
            else if (e.KeyCode == Keys.Escape)
                cancel_Click(sender, e);
        }
    }
}
