using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
                txtFolder.Text = Path.GetFileNameWithoutExtension(txtFile.Text);

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
                        file.Write(entry.Content, 0, (int)entry.Size);
                }
                catch (Exception)
                {
                    extractFailures.Add(entry.Path);
                }
            }

            if (chkWriteLegacy.Checked) // Write a legacy info.txt schema
            {
                // The description has paramteres if the addon was created by a conversion.
                // Extract them out.

                Regex regex = new System.Text.RegularExpressions.Regex("^# ([\\s\\S]*?): ([\\s\\S]*?)$",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);
                MatchCollection matches = regex.Matches(addon.Description);

                // info.txt/addon.txt files usually have these values not directly mapped into GMAs as well.
                string AuthorName = String.Empty;
                string AuthorEmail = String.Empty;
                string AuthorURL = String.Empty;
                string Version = String.Empty;
                string Date = String.Empty;

                foreach (Match keyMatch in matches)
                {
                    if (keyMatch.Groups.Count == 3)
                    {
                        // All match should have 2 groups matched (the 0th group is the whole match.)
                        switch (keyMatch.Groups[1].Value.ToLowerInvariant())
                        {
                            case "version":
                                Version = keyMatch.Groups[2].Value.TrimEnd('\n', '\r', '\t');
                                break;
                            case "date":
                                Date = keyMatch.Groups[2].Value.TrimEnd('\n', '\r', '\t');
                                break;
                            case "authorname":
                                AuthorName = keyMatch.Groups[2].Value.TrimEnd('\n', '\r', '\t');
                                break;
                            case "authoremail":
                                AuthorEmail = keyMatch.Groups[2].Value.TrimEnd('\n', '\r', '\t');
                                break;
                            case "authorurl":
                                AuthorURL = keyMatch.Groups[2].Value.TrimEnd('\n', '\r', '\t');
                                break;
                        }
                    }
                }

                string endConversionInfo = "## End conversion info";
                string description = addon.Description;
                if (addon.Description.IndexOf(endConversionInfo) > 0)
                {
                    description = addon.Description.Substring(addon.Description.IndexOf(endConversionInfo) +
                        endConversionInfo.Length);
                    description = description.TrimStart('\r', '\n');
                }

                File.WriteAllText(txtFolder.Text + Path.DirectorySeparatorChar +  "info.txt", "\"AddonInfo\"\n" +
                    "{\n" +
                    "\t" + "\"name\"" + "\t" + "\"" + addon.Title + "\"\n" +
                    "\t" + "\"version\"" + "\t" + "\"" + Version + "\"\n" +
                    "\t" + "\"up_date\"" + "\t" + "\"" + (String.IsNullOrWhiteSpace(Date) ?
                        addon.Timestamp.ToString("ddd MM dd hh:mm:ss yyyy", System.Globalization.CultureInfo.InvariantCulture) :
                        DateTime.Now.ToString("ddd MM dd hh:mm:ss yyyy", System.Globalization.CultureInfo.InvariantCulture) +
                        " (+" + TimeZoneInfo.Local.BaseUtcOffset.ToString("hhmm") + ")") + "\"\n" +
                    "\t" + "\"author_name\"" + "\t" + "\"" + AuthorName + "\"\n" + // addon.Author would be nice
                    "\t" + "\"author_email\"" + "\t" + "\"" + AuthorEmail + "\"\n" +
                    "\t" + "\"author_url\"" + "\t" + "\"" + AuthorURL + "\"\n" +
                    "\t" + "\"info\"" + "\t" + "\"" + description + "\"\n" +
                    "\t" + "\"override\"" + "\t" + "\"1\"\n" +
                    "}");
            }

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
