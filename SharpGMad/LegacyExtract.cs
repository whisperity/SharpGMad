using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SharpGMad
{
    partial class LegacyExtract : Form
    {
        public LegacyExtract()
        {
            InitializeComponent();
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.Dispose();
        }

        private void btnFolderBrowse_Click(object sender, EventArgs e)
        {
            DialogResult folder = fbdFolder.ShowDialog();
            if (folder == DialogResult.OK)
                txtFolder.Text = fbdFolder.SelectedPath;
        }

        private void btnFileBrowse_Click(object sender, EventArgs e)
        {
            DialogResult file = ofdFile.ShowDialog();

            if (file == DialogResult.OK)
            {
                txtFile.Text = ofdFile.FileName;

                if (txtFolder.Text == String.Empty)
                {
                    fbdFolder.SelectedPath = Path.GetDirectoryName(ofdFile.FileName);

                    txtFolder.Text = fbdFolder.SelectedPath;
                }
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            List<string> extractFailures = new List<string>();

            //
            // If an out path hasn't been provided, make our own
            //
            if (txtFolder.Text == String.Empty)
            {
                txtFolder.Text = Path.GetFileNameWithoutExtension(txtFile.Text);
            }

            //
            // Remove slash, add slash (enforces a slash)
            //
            txtFolder.Text = txtFolder.Text.TrimEnd('/');
            txtFolder.Text = txtFolder.Text + '/';
            Addon addon;
            try
            {
                FileStream fs = new FileStream(txtFile.Text, FileMode.Open, FileAccess.ReadWrite);
                addon = new Addon(new Reader(fs));
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem opening or parsing the selected file: \n" + ex.Message,
                    "Failed to extract the addon", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            foreach (ContentFile entry in addon.Files)
            {
                // Make sure folder exists
                try
                {
                    Directory.CreateDirectory(txtFolder.Text + Path.GetDirectoryName(entry.Path));
                }
                catch (Exception)
                {
                    // Noop
                }
                // Write the file to the disk
                try
                {
                    using (FileStream file = new FileStream(txtFolder.Text + entry.Path, FileMode.Create, FileAccess.Write))
                    {
                        file.Write(entry.Content, 0, (int)entry.Size);
                    }
                }
                catch (Exception)
                {
                    extractFailures.Add(entry.Path);
                }
            }

            if (extractFailures.Count == 0)
                MessageBox.Show("Successfully extracted the addon.", "Extract addon", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Successfully extracted the addon.\n\nThe following files failed to extract:\n" +
                    String.Join("\n", extractFailures), "Extract addon",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

            return;
        }
    }
}
