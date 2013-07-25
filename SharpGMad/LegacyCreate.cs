using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace SharpGMad
{
    partial class LegacyCreate : Form
    {
        public LegacyCreate()
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
            if (txtFolder.Text == String.Empty)
                fbdFolder.SelectedPath = Directory.GetCurrentDirectory();
            
            DialogResult folder = fbdFolder.ShowDialog();
            if (folder == DialogResult.OK)
            {
                txtFolder.Text = fbdFolder.SelectedPath;

                if (txtFile.Text == String.Empty)
                {
                    sfdOutFile.FileName = fbdFolder.SelectedPath.Split(Path.DirectorySeparatorChar)
                        [fbdFolder.SelectedPath.Split(Path.DirectorySeparatorChar).Length - 1] + ".gma";
                    txtFile.Text = sfdOutFile.FileName;
                }
            }
        }

        private void btnFileBrowse_Click(object sender, EventArgs e)
        {
            DialogResult file = sfdOutFile.ShowDialog();

            if (file == DialogResult.OK)
                txtFile.Text = sfdOutFile.FileName;
        }

        /// <summary>
        /// Specifies the type of addon creation error.
        /// </summary>
        private enum CreateErrorType
        {
            /// <summary>
            /// Indicates inability to read the file from the source device.
            /// </summary>
            FileRead,
            /// <summary>
            /// Indicates the file path matching the addon's ignore patterns.
            /// </summary>
            Ignored,
            /// <summary>
            /// Indicates the file path violating the global whitelist patterns.
            /// </summary>
            WhitelistFailure
        }

        /// <summary>
        /// Provides an instance to represent addon creation errors.
        /// </summary>
        private struct CreateError
        {
            /// <summary>
            /// Gets or seth the path of the errorneous file.
            /// </summary>
            public string Path;
            /// <summary>
            /// Gets or sets the type of the error.
            /// </summary>
            public CreateErrorType Type;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (cmbTag1.SelectedItem == cmbTag2.SelectedItem &&
                !(cmbTag1.SelectedItem.ToString() == "(empty)" && cmbTag2.SelectedItem.ToString() == "(empty)"))
            {
                MessageBox.Show("You selected the same tag twice!", "Update metadata",
                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            List<CreateError> errors = new List<CreateError>();

            //
            // Make sure there's a slash on the end
            //
            txtFolder.Text = txtFolder.Text.TrimEnd('/');
            txtFolder.Text = txtFolder.Text + "/";
            //
            // Make sure OutFile ends in .gma
            //
            txtFile.Text = Path.GetFileNameWithoutExtension(txtFile.Text);
            txtFile.Text += ".gma";

            Addon addon = null;
            if (chkConvertNeeded.Checked == false)
            {
                //
                // Load the Addon Info file
                //
                Json addonInfo;
                try
                {
                    addonInfo = new Json(txtFolder.Text + "addon.json");
                }
                catch (AddonJSONException ex)
                {
                    MessageBox.Show(ex.Message,
                        "addon.json parse error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                addon = new Addon(addonInfo);
            }
            else if (chkConvertNeeded.Checked == true)
            {
                // Load the addon metadata from the old file structure: info.txt or addon.txt.
                string legacyInfoFile;
                if (File.Exists(txtFolder.Text + "\\info.txt"))
                    legacyInfoFile = "info.txt";
                else if (File.Exists(txtFolder.Text + "\\addon.txt"))
                    legacyInfoFile = "addon.txt";
                else
                {
                    MessageBox.Show("A legacy metadata file \"info.txt\" or \"addon.txt\" could not be found!",
                        "Failed to create the addon", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                string legacyInfo;
                try
                {
                    legacyInfo = File.ReadAllText(txtFolder.Text + Path.DirectorySeparatorChar + legacyInfoFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error reading the metadata.\n\n" + ex.Message,
                        "Failed to create the addon", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                addon = new Addon();

                // Parse the read data
                Regex regex = new System.Text.RegularExpressions.Regex("\"([A-Za-z_\r\n])*\"", RegexOptions.IgnoreCase);
                MatchCollection matches = regex.Matches(legacyInfo);

                foreach (Match keyMatch in matches)
                {
                    if (keyMatch.Value.ToLowerInvariant() == "\"name\"")
                        addon.Title = keyMatch.NextMatch().Value;
                    else if (keyMatch.Value.ToLowerInvariant() == "\"info\"")
                        addon.Description = keyMatch.NextMatch().Value;
                    else if (keyMatch.Value.ToLowerInvariant() == "\"author_name\"")
                        addon.Author = keyMatch.NextMatch().Value;
                    // Current GMAD writer only writes "Author Name", not real value
                }

                if (cmbType.SelectedItem != null && Tags.TypeExists(cmbType.SelectedItem.ToString()))
                    addon.Type = cmbType.SelectedItem.ToString();
                else
                {
                    // This should not happen in normal operation
                    // nontheless we check against it
                    MessageBox.Show("The selected type is invalid!", "Update metadata",
                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                if (((cmbTag1.SelectedItem.ToString() != "(empty)") && !Tags.TagExists(cmbTag1.SelectedItem.ToString()))
                    || ((cmbTag2.SelectedItem.ToString() != "(empty)") && !Tags.TagExists(cmbTag2.SelectedItem.ToString())))
                {
                    // This should not happen in normal operation
                    // nontheless we check against it
                    MessageBox.Show("The selected tags are invalid!", "Update metadata",
                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                addon.Tags = new List<string>(2);
                if (cmbTag1.SelectedItem.ToString() != "(empty)")
                    addon.Tags.Add(cmbTag1.SelectedItem.ToString());
                if (cmbTag2.SelectedItem.ToString() != "(empty)")
                    addon.Tags.Add(cmbTag2.SelectedItem.ToString());
            }

            //
            // Get a list of files in the specified folder
            //
            foreach (string f in Directory.GetFiles(txtFolder.Text, "*", SearchOption.AllDirectories))
            {
                string file = f;
                file = file.Replace(txtFolder.Text, String.Empty);
                file = file.Replace('\\', '/');

                try
                {
                    addon.CheckRestrictions(file);
                    addon.AddFile(file, File.ReadAllBytes(f));
                }
                catch (IOException)
                {
                    errors.Add(new CreateError() { Path = file, Type = CreateErrorType.FileRead });
                    continue;
                }
                catch (IgnoredException)
                {
                    errors.Add(new CreateError() { Path = file, Type = CreateErrorType.Ignored });
                    continue;
                }
                catch (WhitelistException)
                {
                    errors.Add(new CreateError() { Path = file, Type = CreateErrorType.WhitelistFailure });

                    if (!chkWarnInvalid.Checked)
                    {
                        MessageBox.Show("The following file is not allowed by the whitelist:\n" + file,
                            "Failed to create the addon", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                }
            }

            //
            // Sort the list into alphabetical order, no real reason - we're just ODC
            //
            addon.Sort();

            //
            // Create an addon file in a buffer
            //
            //
            // Save the buffer to the provided name
            //
            FileStream gmaFS;
            try
            {
                gmaFS = new FileStream(txtFile.Text, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                gmaFS.SetLength(0); // Truncate the file

                Writer.Create(addon, gmaFS);
            }
            catch (Exception be)
            {
                MessageBox.Show("An exception happened while compiling the addon.\n\n" + be.Message,
                    "Failed to create the addon", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            gmaFS.Flush();

            //
            // Success!
            //
            if (errors.Count == 0)
            {
                MessageBox.Show("Successfully saved to " + txtFile.Text + " (" + ((int)gmaFS.Length).HumanReadableSize() + ")",
                    "Created successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string msgboxMessage = "Successfully saved to " + txtFile.Text + " (" + ((int)gmaFS.Length).HumanReadableSize() +
                    ")\n\nThe following files were not added:\n";

                foreach (CreateError err in errors)
                {
                    msgboxMessage += err.Path + ", ";
                    switch (err.Type)
                    {
                        case CreateErrorType.FileRead:
                            msgboxMessage += "the program failed to read the file";
                            break;
                        case CreateErrorType.Ignored:
                            msgboxMessage += "the file is ignored by the addon's configuration";
                            break;
                        case CreateErrorType.WhitelistFailure:
                            msgboxMessage += "the file is not allowed by the global whitelist";
                            break;
                    }
                    msgboxMessage += ".\n";
                }

                msgboxMessage = msgboxMessage.TrimEnd('\n');

                MessageBox.Show(msgboxMessage, "Created successfully", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            gmaFS.Dispose();
            return;
        }

        private void LegacyCreate_Load(object sender, EventArgs e)
        {
            // The comboboxes are a bit more tricky
            cmbType.Items.AddRange(Tags.Type);

            cmbTag1.Items.AddRange(Tags.Misc);
            cmbTag2.Items.AddRange(Tags.Misc);
            cmbTag1.Items.Add("(empty)");
            cmbTag2.Items.Add("(empty)");
            cmbTag1.SelectedItem = "(empty)";
            cmbTag2.SelectedItem = "(empty)";
        }

        private void chkConvertNeeded_CheckedChanged(object sender, EventArgs e)
        {
            gboConvertMetadata.Visible = ((System.Windows.Forms.CheckBox)sender).Checked;
        }
    }
}
