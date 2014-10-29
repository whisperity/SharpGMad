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
            this.Icon = global::SharpGMad.Properties.Resources.create_ico;
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
            if (gboConvertMetadata.Visible == false)
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
            else if (gboConvertMetadata.Visible == true)
            {
                // Load the addon metadata from the old file structure: info.txt/addon.txt
                if (!File.Exists(txtFolder.Text + Path.DirectorySeparatorChar + "info.txt") &&
                    !File.Exists(txtFolder.Text + Path.DirectorySeparatorChar + "addon.txt"))
                {
                    MessageBox.Show("A legacy metadata file \"info.txt\" or \"addon.txt\" could not be found!",
                        "Failed to create the addon", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                string legacyInfo = String.Empty;;
                try
                {
                    if (File.Exists(txtFolder.Text + Path.DirectorySeparatorChar + "info.txt"))
                        legacyInfo = File.ReadAllText(txtFolder.Text + Path.DirectorySeparatorChar + "info.txt");
                    else if (File.Exists(txtFolder.Text + Path.DirectorySeparatorChar + "addon.txt"))
                        legacyInfo = File.ReadAllText(txtFolder.Text + Path.DirectorySeparatorChar + "addon.txt");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error reading the metadata.\n\n" + ex.Message,
                        "Failed to create the addon", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                addon = new Addon();

                // Parse the read data
                Regex regex = new System.Text.RegularExpressions.Regex("\"([A-Za-z_\r\n]*)\"[\\s]*\"(.*)\"", RegexOptions.IgnoreCase);
                MatchCollection matches = regex.Matches(legacyInfo);

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
                            case "name":
                                addon.Title = keyMatch.Groups[2].Value;
                                break;
                            case "version":
                                Version = keyMatch.Groups[2].Value;
                                break;
                            case "up_date":
                                Date = keyMatch.Groups[2].Value;
                                break;
                            case "author_name":
                                //addon.Author = keyMatch.Groups[2].Value;
                                // GMAD writer only writes "Author Name" right now...
                                AuthorName = keyMatch.Groups[2].Value;
                                break;
                            case "author_email":
                                AuthorEmail = keyMatch.Groups[2].Value;
                                break;
                            case "author_url":
                                AuthorURL = keyMatch.Groups[2].Value;
                                break;
                            case "info":
                                addon.Description = keyMatch.Groups[2].Value;
                                break;
                        }
                    }
                }

                // Prettify the loaded Description.
                string newDescription = "by ";
                if (!String.IsNullOrWhiteSpace(AuthorName))
                    newDescription += AuthorName;
                else
                    newDescription += "unknown";

                if (!String.IsNullOrWhiteSpace(AuthorEmail))
                    newDescription += " (" + AuthorEmail;
                else
                    if (!String.IsNullOrWhiteSpace(AuthorURL))
                        newDescription += " (";

                if (!String.IsNullOrWhiteSpace(AuthorEmail) && !String.IsNullOrWhiteSpace(AuthorURL))
                    newDescription += ", ";

                if (!String.IsNullOrWhiteSpace(AuthorURL))
                    newDescription += AuthorURL + ")";
                else
                    if (!String.IsNullOrWhiteSpace(AuthorEmail))
                        newDescription += ")";

                if (!String.IsNullOrWhiteSpace(Version))
                    newDescription += " v" + Version;

                if (!String.IsNullOrWhiteSpace(Date))
                    newDescription += " (" + Date + ")";

                if (newDescription != "by " && newDescription != "by unknown")
                    // If anything was added to the prettifiction
                    addon.Description = newDescription +
                        (!String.IsNullOrWhiteSpace(addon.Description) ? '\n' + addon.Description : null);

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

                if (file == "addon.json" || file == "info.txt")
                    continue; // Don't read the metadata file

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
                MessageBox.Show("Successfully created the addon.", "Create addon",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (errors.Count == 1)
                MessageBox.Show("Successfully created the addon.\nThe file " + errors[0].Path + " was not added " +
                    "because " + errors[0].Type, "Create addon",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (errors.Count > 1)
            {
                DialogResult showFailedFiles = MessageBox.Show(errors.Count + " files failed to add." +
                    "\n\nShow a list of failures?", "Create addon", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (showFailedFiles == DialogResult.Yes)
                {
                    string temppath = ContentFile.GenerateExternalPath(
                            new Random().Next() + "_failedCreations") + ".txt";

                    string msgboxMessage = String.Empty;

                    foreach (CreateError err in errors)
                    {
                        msgboxMessage += err.Path + ", ";
                        switch (err.Type)
                        {
                            case CreateErrorType.FileRead:
                                msgboxMessage += "failed to read the file";
                                break;
                            case CreateErrorType.Ignored:
                                msgboxMessage += "the file is ignored by the addon's configuration";
                                break;
                            case CreateErrorType.WhitelistFailure:
                                msgboxMessage += "the file is not allowed by the global whitelist";
                                break;
                        }
                        msgboxMessage += "\r\n";
                    }
                    msgboxMessage = msgboxMessage.TrimEnd('\r', '\n');

                    try
                    {
                        File.WriteAllText(temppath, "These files failed to add:\r\n\r\n" + msgboxMessage);
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

            gmaFS.Dispose();
            btnAbort_Click(sender, e); // Close the form
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

        private void txtFolder_TextChanged(object sender, EventArgs e)
        {
            if (File.Exists(txtFolder.Text + Path.DirectorySeparatorChar + "addon.json"))
            {
                btnCreate.Enabled = true;
                gboConvertMetadata.Visible = false;
            }
            else if (File.Exists(txtFolder.Text + Path.DirectorySeparatorChar + "info.txt") ||
                File.Exists(txtFolder.Text + Path.DirectorySeparatorChar + "addon.txt"))
            {
                btnCreate.Enabled = true;
                gboConvertMetadata.Visible = true;
            }
            else
            {
                btnCreate.Enabled = false;
                gboConvertMetadata.Visible = false;
            }
        }
    }
}
