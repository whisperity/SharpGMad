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
            this.Icon = global::SharpGMad.Properties.Resources.extract_ico;
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
                    fbdFolder.SelectedPath = Path.GetDirectoryName(ofdFile.FileName) +
                        Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(ofdFile.FileName);

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
                MessageBox.Show("Can't open the selected file.\nError happened: " + ex.Message,
                    "Failed to extract addon", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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

            if (chkWriteLegacy.Checked)
                File.WriteAllText(txtFolder.Text + "info.txt", "\"AddonInfo\"\n" +
                    "{\n" +
                    "\t" + "\"name\"" + "\t" + "\"" + addon.Title + "\"\n" +
                    "\t" + "\"version\"" + "\t" + "\"1.0\"\n" +
                    "\t" + "\"up_date\"" + "\t" + "\"" + addon.Timestamp.ToString() + "\"\n" +
                    "\t" + "\"author_name\"" + "\t" + "\"unknown\"" + "\"\n" + // addon.Author would be nice
                    "\t" + "\"author_email\"" + "\t" + "\"\"\n" +
                    "\t" + "\"author_url\"" + "\t" + "\"\"\n" +
                    "\t" + "\"info\"" + "\t" + "\"" + addon.Description + "\"\n" +
                    "\t" + "\"override\"" + "\t" + "\"1\"\n" +
                    "}");

            if (extractFailures.Count == 0)
                MessageBox.Show("Successfully extracted the addon.", "Extract addon",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (extractFailures.Count == 1)
                MessageBox.Show("Failed to extract " + extractFailures[0], "Extract addon",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (extractFailures.Count > 1)
            {
                DialogResult showFailedFiles = MessageBox.Show(extractFailures.Count + " files failed to extract." +
                    "\n\nShow a list of failures?", "Extract addon", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (showFailedFiles == DialogResult.Yes)
                {
                    string temppath = ContentFile.GenerateExternalPath(
                            new Random().Next() + "_failedExtracts") + ".txt";

                    try
                    {
                        File.WriteAllText(temppath,
                            "These files failed to extract:\r\n\r\n" +
                            String.Join("\r\n", extractFailures.ToArray()));
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Can't show the list, an error happened generating it.", "Extract addon",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    // The file will be opened by the user's default text file handler (Notepad?)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(temppath)
                    {
                        UseShellExecute = true,
                    });
                }
            }

            btnAbort_Click(sender, e); // Close the form
            return;
        }
    }
}
