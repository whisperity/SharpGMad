using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SharpGMad
{
    partial class Main : Form
    {
        /// <summary>
        /// The currently open addon.
        /// </summary>
        RealtimeAddon AddonHandle;

        private Main()
        {
            InitializeComponent();
            UnloadAddon();
        }

        public Main(string[] args)
            : this()
        {
            // Try to autoload the addon if there's a first parameter specified.
            // This supports drag&dropping an addon file onto the EXE in Explorer.

            string path = String.Join(" ", args);
            if (path != String.Empty)
            {
                try
                {
                    LoadAddon(path);
                }
                catch (IOException)
                {
                    return;
                }
            }
        }

        private void tsbOpenAddon_Click(object sender, EventArgs e)
        {
            DialogResult dropChanges = new DialogResult();
            if (AddonHandle is RealtimeAddon && AddonHandle.Modified)
            {
                dropChanges = MessageBox.Show("Do you want to open another addon without saving the current first?",
                    "An addon is already open", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (dropChanges == DialogResult.Yes || 
                AddonHandle == null || (AddonHandle is RealtimeAddon && !AddonHandle.Modified))
            {
                UnloadAddon();
                DialogResult file = ofdAddon.ShowDialog();

                if (file == DialogResult.OK)
                    LoadAddon(ofdAddon.FileName);
            }
        }

        /// <summary>
        /// Loads an addon from the filesystem.
        /// </summary>
        /// <param name="filename">The path of the addon to load.</param>
        private void LoadAddon(string path)
        {
            try
            {
                AddonHandle = RealtimeAddon.Load(path, !FileExtensions.CanWrite(path));

                if (!AddonHandle.CanWrite)
                {
                    DialogResult openReadOnly = MessageBox.Show("This addon is locked by another process, " +
                        "and cannot be written.\n\n" +
                        "Would you like to open it in read-only mode?\nAll modification options will be disabled.",
                        "Addon locked", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (openReadOnly != DialogResult.Yes)
                    {
                        AddonHandle.Close();
                        return;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return;
            }
            catch (IgnoredException e)
            {
                MessageBox.Show(e.Message, "Addon is corrupted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (WhitelistException e)
            {
                MessageBox.Show(e.Message, "Addon is corrupted", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            catch (ArgumentException e)
            {
                MessageBox.Show(e.Message, "Addon is corrupted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (IOException ex)
            {
                MessageBox.Show("Unable to load the addon.\nAn exception happened.\n\n" + ex.Message, "Addon reading error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (ReaderException ex)
            {
                MessageBox.Show("There was an error parsing the file.\n\n" + ex.Message, "Addon reading error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (AddonHandle is RealtimeAddon)
            {
                UpdateMetadataPanel();
                UpdateFileList();
                UpdateModified();
                UpdateStatus("Loaded the addon" + (AddonHandle.CanWrite ? null : " (read-only mode)"));

                tsbAddFile.Enabled = AddonHandle.CanWrite;
                tsbUpdateMetadata.Enabled = AddonHandle.CanWrite;
            }
        }

        /// <summary>
        /// Updates the form so it reflects the modified or not modified state.
        /// </summary>
        private void UpdateModified()
        {
            // Invoke the method if it was called from a thread which is not the thread Main was created in.
            //
            // Prevents the exception:
            // Cross-thread operation not valid: Control 'Main' accessed from a
            // thread other than the thread it was created on.
            if (lstFiles.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { UpdateModified(); });
            }
            else
            {
                this.Text = Path.GetFileName(AddonHandle.AddonPath) + (AddonHandle.CanWrite ? null : " (read-only)") +
                    (AddonHandle.Modified ? "*" : null) + " - SharpGMad";

                tsbSaveAddon.Enabled = AddonHandle.CanWrite && AddonHandle.Modified;
            }
        }

        /// <summary>
        /// Updates the filelist (lstFiles) with the changes administered to the known files.
        /// </summary>
        private void UpdateFileList()
        {
            // Invoke the method if it was called from a thread which is not the thread lstFiles was created in.
            //
            // (For example when fsw_Changed fires.)
            //
            // Prevents the exception:
            // Cross-thread operation not valid: Control 'lstFiles' accessed from a
            // thread other than the thread it was created on.
            if (lstFiles.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { UpdateFileList(); });
            }
            else
            {
                // Reset the export counters
                tsbPullAll.Enabled = false;
                tsbDropAll.Enabled = false;


                // Clear the list
                lstFiles.Items.Clear();
                lstFiles.Groups.Clear();

                // Get and add the groups (folders)
                IEnumerable<IGrouping<string, ContentFile>> folders =
                    AddonHandle.OpenAddon.Files.GroupBy(f => Path.GetDirectoryName(f.Path).Replace('\\', '/'));
                foreach (IGrouping<string, ContentFile> folder in folders)
                {
                    lstFiles.Groups.Add(folder.Key, folder.Key);
                }

                // Get and add the files
                foreach (ContentFile cfile in AddonHandle.OpenAddon.Files)
                {
                    ListViewItem item = new ListViewItem(Path.GetFileName(cfile.Path),
                        lstFiles.Groups[Path.GetDirectoryName(cfile.Path).Replace('\\', '/')]);

                    IEnumerable<FileWatch> watch = AddonHandle.WatchedFiles.Where(f => f.ContentPath == cfile.Path);
                    if (watch.Count() == 1)
                    {
                        tsbDropAll.Enabled = true; // At least one file is exported
                        item.ForeColor = Color.Blue;

                        if (watch.First().Modified)
                        {
                            tsbPullAll.Enabled = AddonHandle.CanWrite; // At least one file is modified externally
                            item.ForeColor = Color.Indigo;
                        }
                    }

                    lstFiles.Items.Add(item);
                }
            }
        }

        /// <summary>
        /// Update the status label on the bottom of the form.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="foreColor">The color the text should have.</param>
        private void UpdateStatus(string text, Color foreColor = default(Color))
        {
            if (foreColor == default(Color))
                foreColor = System.Drawing.SystemColors.ControlText;

            tsslStatus.ForeColor = foreColor;
            tsslStatus.Text = "[" + DateTime.Now.ToString() + "] " + text;
        }

        /// <summary>
        /// Updates the metadata panel with the information fetched from the current addon.
        /// </summary>
        private void UpdateMetadataPanel()
        {
            txtMetadataTitle.Text = AddonHandle.OpenAddon.Title;
            /*txtMetadataAuthor.Text = AddonHandle.OpenAddon.Author;*/
            txtMetadataDescription.Text = AddonHandle.OpenAddon.Description;

            cmbMetadataType.Items.Clear();
            cmbMetadataType.Items.AddRange(Tags.Type);
            cmbMetadataType.SelectedItem = AddonHandle.OpenAddon.Type;

            cmbMetadataTag1.Items.Clear();
            cmbMetadataTag1.Items.AddRange(Tags.Misc);
            cmbMetadataTag1.Items.Add("");
            try
            {
                cmbMetadataTag1.SelectedItem = AddonHandle.OpenAddon.Tags[0];
            }
            catch (ArgumentOutOfRangeException)
            {
                // No first tag, select the empty one
                cmbMetadataTag1.SelectedItem = "";
            }

            cmbMetadataTag2.Items.Clear();
            cmbMetadataTag2.Items.AddRange(Tags.Misc);
            cmbMetadataTag2.Items.Add("");
            try
            {
                cmbMetadataTag2.SelectedItem = AddonHandle.OpenAddon.Tags[1];
            }
            catch (ArgumentOutOfRangeException)
            {
                // No second tag, select the empty one
                cmbMetadataTag2.SelectedItem = "";
            }
        }

        /// <summary>
        /// Closes the currently open addon connection.
        /// </summary>
        private void UnloadAddon()
        {
            txtMetadataTitle.Text = String.Empty;
            /*txtMetadataAuthor.Text = String.Empty;*/
            cmbMetadataType.Items.Clear();
            cmbMetadataTag1.Items.Clear();
            cmbMetadataTag2.Items.Clear();
            txtMetadataDescription.Text = String.Empty;

            lstFiles.Items.Clear();
            lstFiles.Groups.Clear();

            this.Text = "SharpGMad";
            tsbSaveAddon.Enabled = false;

            if (AddonHandle != null)
                AddonHandle.Close();
            AddonHandle = null;

            tsbUpdateMetadata.Enabled = false;
            tsbDiscardMetadataChanges_Click(null, new EventArgs());
            tsbAddFile.Enabled = false;
        }

        // Dock the txtDescription text box.
        // It gets automatically resized when the form is resized.
        Size txtDescriptionSizeDifference;
        private void Main_Load(object sender, EventArgs e)
        {
            txtDescriptionSizeDifference = new Size(pnlRightSide.Size.Width - txtMetadataDescription.Size.Width,
                pnlRightSide.Size.Height - txtMetadataDescription.Size.Height);

            UpdateStatus("SharpGMad welcomes you!");
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            txtMetadataDescription.Size = new Size(pnlRightSide.Size.Width - txtDescriptionSizeDifference.Width,
                pnlRightSide.Size.Height - txtDescriptionSizeDifference.Height);
        }
        // Dock the txtDescription text box.

        private void tsmiLegacyCreate_Click(object sender, EventArgs e)
        {
            LegacyCreate lcForm = new LegacyCreate();
            lcForm.ShowDialog(this);
        }

        private void tsmiLegacyExtract_Click(object sender, EventArgs e)
        {
            LegacyExtract leForm = new LegacyExtract();
            leForm.ShowDialog(this);
        }

        private void tsbAddFile_Click(object sender, EventArgs e)
        {
            if (AddonHandle == null)
                return;

            // If there is no value for file filtering, load a file list
            if (ofdAddFile.Filter == String.Empty)
            {
                string filter = String.Empty;
                foreach (KeyValuePair<string, string[]> filetype in Whitelist.WildcardFileTypes)
                {
                    filter += filetype.Key + "|" + String.Join(";", filetype.Value) + "|";
                }
                filter += "All files|*.*";

                ofdAddFile.Filter = filter;
                ofdAddFile.FilterIndex = Whitelist.WildcardFileTypes.Count + 1;
            }

            DialogResult result = ofdAddFile.ShowDialog();
            if (result != DialogResult.Cancel)
            {
                string filename = ofdAddFile.FileName.Replace("\\", "/");

                try
                {
                    AddonHandle.OpenAddon.CheckRestrictions(Whitelist.GetMatchingString(filename));
                    AddonHandle.AddFile(Whitelist.GetMatchingString(filename), File.ReadAllBytes(ofdAddFile.FileName));
                }
                catch (IOException)
                {
                    MessageBox.Show("Error happened while reading " + ofdAddFile.FileName, "Add file",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
                catch (IgnoredException)
                {
                    MessageBox.Show("File is ignored (" + ofdAddFile.FileName + ")", "Add file",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (WhitelistException)
                {
                    MessageBox.Show("File is not whitelisted (" + ofdAddFile.FileName + ")", "Add file",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
                catch (ArgumentException)
                {
                    MessageBox.Show("Similar file has already been added (" + ofdAddFile.FileName + ")",
                        "Add file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UpdateModified();
                UpdateFileList();
                ofdAddFile.Reset();
            }
        }

        private void lstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((System.Windows.Forms.ListView)sender).SelectedItems.Count == 1)
            {
                // One file is selected
                if (((System.Windows.Forms.ListView)sender).FocusedItem != null)
                {
                    // Allow remove, export and execution
                    tsmFileRemove.Visible = true;
                    tsmFileRemove.Enabled = AddonHandle.CanWrite;

                    tsmFileExtract.Enabled = true;
                    tsmFileExtract.Visible = true;

                    tsmShellExec.Enabled = true;
                    tsmShellExec.Visible = true;

                    // Allow export (and related) options
                    IEnumerable<FileWatch> isExported = AddonHandle.WatchedFiles.Where(f => f.ContentPath ==
                        lstFiles.FocusedItem.Group.Header + "/" + lstFiles.FocusedItem.Text);
                    if (isExported.Count() == 0)
                    {
                        // Export is the file is not exported
                        tsmFileExportTo.Enabled = AddonHandle.CanWrite;
                        tsmFilePull.Enabled = false;
                        tsmFileDropExport.Enabled = false;
                    }
                    else
                    {
                        // Pull (applicable if the file is changed) and drop
                        tsmFileExportTo.Enabled = false;
                        tsmFilePull.Enabled = isExported.First().Modified && AddonHandle.CanWrite;
                        tsmFileDropExport.Enabled = true;
                    }

                    // But the buttons should be visible
                    tssExportSeparator.Visible = true;
                    tsmFileExportTo.Visible = true;
                    tsmFilePull.Visible = true;
                    tsmFileDropExport.Visible = true;
                    tsmFilePull.Enabled = AddonHandle.CanWrite;
                    tsmFileDropExport.Enabled = AddonHandle.CanWrite;
                }
            }
            else if (((System.Windows.Forms.ListView)sender).SelectedItems.Count > 1)
            {
                // Multiple files support remove, extract, but no exec and export-related stuff
                tsmFileRemove.Enabled = true;
                tsmFileRemove.Visible = true;

                tsmFileExtract.Enabled = true;
                tsmFileExtract.Visible = true;

                tsmShellExec.Enabled = false;
                tsmShellExec.Visible = false;

                tssExportSeparator.Visible = false;

                tsmFileExportTo.Enabled = false;
                tsmFileExportTo.Visible = false;

                tsmFilePull.Enabled = false;
                tsmFilePull.Visible = false;

                tsmFileDropExport.Enabled = false;
                tsmFileDropExport.Visible = false;
            }
            else
            {
                // Nothing if there are no files selected
                tsmFileRemove.Enabled = false;
                tsmFileRemove.Visible = false;

                tsmFileExtract.Enabled = false;
                tsmFileExtract.Visible = false;

                tsmShellExec.Enabled = false;
                tsmShellExec.Visible = false;

                tssExportSeparator.Visible = false;

                tsmFileExportTo.Enabled = false;
                tsmFileExportTo.Visible = false;

                tsmFilePull.Enabled = false;
                tsmFilePull.Visible = false;

                tsmFileDropExport.Enabled = false;
                tsmFileDropExport.Visible = false;
            }
        }

        private void lstFiles_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                if (((System.Windows.Forms.ListView)sender).FocusedItem.Bounds.Contains(e.Location) == true)
                    cmsFileEntry.Show(Cursor.Position);
        }

        private void lstFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Clicks == 2)
                if (((System.Windows.Forms.ListView)sender).FocusedItem.Bounds.Contains(e.Location) == true)
                    tsmShellExec_Click(sender, e);
        }

        private void lstFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (((System.Windows.Forms.ListView)sender).FocusedItem != null)
                    tsmFileRemove_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                if (((System.Windows.Forms.ListView)sender).FocusedItem != null)
                    tsmShellExec_Click(sender, e);
            }
        }

        private void tsmFileRemove_Click(object sender, EventArgs e)
        {
            if (!AddonHandle.CanWrite)
                return;

            if (lstFiles.SelectedItems.Count == 1)
            {
                if (lstFiles.FocusedItem != null)
                {
                    DialogResult remove = MessageBox.Show("Really remove " +
                        lstFiles.FocusedItem.Group.Header + "/" + lstFiles.FocusedItem.Text + "?", "Remove file",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (remove == DialogResult.Yes)
                    {
                        try
                        {
                            AddonHandle.RemoveFile(lstFiles.FocusedItem.Group.Header + "/" + lstFiles.FocusedItem.Text);
                        }
                        catch (FileNotFoundException)
                        {
                            MessageBox.Show("File not in archive (" + lstFiles.FocusedItem.Group.Header +
                                "/" + lstFiles.FocusedItem.Text + ")", "Remove file",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }

                        UpdateStatus("Removed file " + lstFiles.FocusedItem.Group.Header + "/" + lstFiles.FocusedItem.Text);
                        UpdateModified();
                        UpdateFileList();
                    }
                }
            }
            else if (lstFiles.SelectedItems.Count >= 1)
            {
                DialogResult remove = MessageBox.Show("Remove " +
                    Convert.ToString(lstFiles.SelectedItems.Count) + " files?", "Remove files",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (remove == DialogResult.Yes)
                {
                    List<string> failed_paths = new List<string>(lstFiles.SelectedItems.Count);

                    foreach (System.Windows.Forms.ListViewItem file in lstFiles.SelectedItems)
                    {
                        try
                        {
                            AddonHandle.RemoveFile(file.Group.Header + "/" + file.Text);
                        }
                        catch (FileNotFoundException)
                        {
                            failed_paths.Add(file.Group.Header + "/" + file.Text);
                        }
                    }

                    if (failed_paths.Count == 0)
                        UpdateStatus("Removed " + lstFiles.SelectedItems.Count + " files");
                    else if (failed_paths.Count == 1)
                        MessageBox.Show("Failed to remove " + failed_paths[0], "Remove files",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    else if (failed_paths.Count > 1)
                    {
                        DialogResult showFailedFiles = MessageBox.Show(Convert.ToString(failed_paths.Count) +
                            " files failed to remove.\n\nShow a list of failures?", "Remove files",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                        if (showFailedFiles == DialogResult.Yes)
                        {
                            string temppath = ContentFile.GenerateExternalPath(
                                    new Random().Next() + "_failedRemovals") + ".txt";

                            try
                            {
                                File.WriteAllText(temppath,
                                    "These files failed to get removed:\r\n\r\n" +
                                    String.Join("\r\n", failed_paths.ToArray()));
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Can't show the list, an error happened generating it.", "Remove files",
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

                    UpdateModified();
                    UpdateFileList();
                }
            }
        }

        private void tsbUpdateMetadata_Click(object sender, EventArgs e)
        {
            if (!AddonHandle.CanWrite)
                return;

            // Use a toggle mechanism to enable and disable the changing of metadata
            if (!tsbUpdateMetadata.Checked)
            {
                txtMetadataTitle.ReadOnly = false;
                //txtMetadataAuthor.ReadOnly = false;
                cmbMetadataType.Enabled = true;
                cmbMetadataTag1.Enabled = true;
                cmbMetadataTag2.Enabled = true;
                txtMetadataDescription.ReadOnly = false;

                tsbUpdateMetadata.Checked = true;
                tsbDiscardMetadataChanges.Enabled = true;
                tsbDiscardMetadataChanges.Visible = true;
            }
            else if (tsbUpdateMetadata.Checked)
            {
                // Save the metadata changes
                if (cmbMetadataTag1.SelectedItem != null && cmbMetadataTag2.SelectedItem != null)
                {
                    if (cmbMetadataTag1.SelectedItem.ToString() == cmbMetadataTag2.SelectedItem.ToString() &&
                        !(cmbMetadataTag1.SelectedItem.ToString() == "" || cmbMetadataTag2.SelectedItem.ToString() == ""))
                    {
                        MessageBox.Show("The same tag is selected twice", "Update metadata",
                            MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                }

                if (cmbMetadataType.SelectedItem == null || !Tags.TypeExists(cmbMetadataType.SelectedItem.ToString()))
                {
                    MessageBox.Show("Invalid type selected", "Update metadata",
                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                if (cmbMetadataTag1.SelectedItem != null)
                {
                    if (!Tags.TagExists(cmbMetadataTag1.SelectedItem.ToString()) && cmbMetadataTag1.SelectedItem.ToString() != "")
                    {
                        MessageBox.Show("Invalid tag selected", "Update metadata",
                            MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                }

                if (cmbMetadataTag2.SelectedItem != null)
                {
                    if (!Tags.TagExists(cmbMetadataTag2.SelectedItem.ToString()) && cmbMetadataTag2.SelectedItem.ToString() != "")
                    {
                        MessageBox.Show("Invalid tag selected", "Update metadata",
                            MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                }

                AddonHandle.OpenAddon.Title = txtMetadataTitle.Text;
                //AddonHandle.OpenAddon.Author = txtMetadataAuthor.Text;
                if (cmbMetadataType.SelectedItem != null)
                    AddonHandle.OpenAddon.Type = cmbMetadataType.SelectedItem.ToString();
                AddonHandle.OpenAddon.Tags = new List<string>(2);
                if (cmbMetadataTag1.SelectedItem != null && cmbMetadataTag1.SelectedItem.ToString() != "")
                    AddonHandle.OpenAddon.Tags.Add(cmbMetadataTag1.SelectedItem.ToString());
                if (cmbMetadataTag2.SelectedItem != null && cmbMetadataTag2.SelectedItem.ToString() != "")
                    AddonHandle.OpenAddon.Tags.Add(cmbMetadataTag2.SelectedItem.ToString());
                AddonHandle.OpenAddon.Description = txtMetadataDescription.Text;

                AddonHandle.Modified = true;
                UpdateModified();
                UpdateMetadataPanel(); // Force reload the values in the metadata panel so we're sure the addon is correctly set.

                // Resets the controls
                txtMetadataTitle.ReadOnly = true;
                //txtMetadataAuthor.ReadOnly = true;
                cmbMetadataType.Enabled = false;
                cmbMetadataTag1.Enabled = false;
                cmbMetadataTag2.Enabled = false;
                txtMetadataDescription.ReadOnly = true;

                tsbUpdateMetadata.Checked = false;
                tsbDiscardMetadataChanges.Enabled = false;
                tsbDiscardMetadataChanges.Visible = false;
            }
        }

        private void tsbDiscardMetadataChanges_Click(object sender, EventArgs e)
        {
            if (AddonHandle is RealtimeAddon)
            {
                // Reset the metadata information to what is already in memory and lock the controls
                UpdateMetadataPanel();

                txtMetadataTitle.ReadOnly = true;
                //txtMetadataAuthor.ReadOnly = true;
                cmbMetadataType.Enabled = false;
                cmbMetadataTag1.Enabled = false;
                cmbMetadataTag2.Enabled = false;
                txtMetadataDescription.ReadOnly = true;

                tsbUpdateMetadata.Checked = false;
                tsbDiscardMetadataChanges.Enabled = false;
                tsbDiscardMetadataChanges.Visible = false;
            }
        }

        private void tsbSaveAddon_Click(object sender, EventArgs e)
        {
            if (!AddonHandle.CanWrite)
                return;

            if (!tsbUpdateMetadata.Checked)
            {
                if (AddonHandle.Modified)
                {
                    try
                    {
                        AddonHandle.Save();
                    }
                    catch (AddonJSONException ex)
                    // Writer.Create access addon.DescriptionJSON which calls
                    // Json.BuildDescription() which can throw the AddonJSONException
                    {
                        MessageBox.Show("Error happened: " + ex.Message + "\n\n" +
                            "This is usually caused by invalid metadata values.", "Save addon",
                             MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    if (!(e is FormClosingEventArgs))
                        UpdateStatus("Addon saved successfully");

                    UpdateModified();
                }
            }
            else if (tsbUpdateMetadata.Checked == true)
            {
                MessageBox.Show("There might be unsaved changes to the metadata.", "Save addon",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AddonHandle is RealtimeAddon && AddonHandle.Modified)
            {
                DialogResult yesClose = MessageBox.Show("Save the addon before quitting?",
                    Path.GetFileName(AddonHandle.AddonPath), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                switch (yesClose)
                {
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                    case DialogResult.No:
                        // Noop, we just close.
                        break;
                    case DialogResult.Yes:
                        tsbSaveAddon_Click(sender, e); // Invoke the addon saving mechanism
                        break;
                    default:
                        break;
                }
            }

            if (AddonHandle is RealtimeAddon)
            {
                UnloadAddon();
            }
        }

        private void tsbCreateAddon_Click(object sender, EventArgs e)
        {
            DialogResult dropChanges = new DialogResult();
            if (AddonHandle is RealtimeAddon && AddonHandle.Modified)
            {
                dropChanges = MessageBox.Show("Open an another addon without saving the current one?\nYou'll lose the changes.",
                    "Addon already open", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (dropChanges == DialogResult.Yes || AddonHandle == null ||
                (AddonHandle is RealtimeAddon && !AddonHandle.Modified))
            {
                UnloadAddon();
                DialogResult file = sfdAddon.ShowDialog();

                if (file == DialogResult.OK)
                {
                    try
                    {
                        AddonHandle = RealtimeAddon.New(sfdAddon.FileName);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("There was a problem creating the addon.", "Create new addon",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    AddonHandle.OpenAddon.Title = Path.GetFileNameWithoutExtension(sfdAddon.FileName);
                    /*AddonHandle.OpenAddon.Author = "Author Name"; // This is currently not changable*/
                    AddonHandle.OpenAddon.Description = String.Empty;
                    AddonHandle.OpenAddon.Type = String.Empty;
                    AddonHandle.OpenAddon.Tags = new List<string>();
                    tsbUpdateMetadata_Click(sender, e); // This will make the metadata change enabled to set the initial values

                    // But the "Discard" button must be disabled so that the user cannot leave the metadata blank
                    tsbDiscardMetadataChanges.Enabled = false;
                    tsbDiscardMetadataChanges.Visible = false;

                    UpdateModified();
                    UpdateMetadataPanel();
                    UpdateFileList();

                    tsbAddFile.Enabled = true;
                    tsbUpdateMetadata.Enabled = true;
                }
            }
        }

        private void tsmFileExportTo_Click(object sender, EventArgs e)
        {
            if (!AddonHandle.CanWrite)
                return;

            if (lstFiles.FocusedItem != null)
            {
                string contentPath = lstFiles.FocusedItem.Group.Header + "/" + lstFiles.FocusedItem.Text;

                sfdExportFile.FileName = Path.GetFileName(lstFiles.FocusedItem.Text);
                sfdExportFile.DefaultExt = Path.GetExtension(lstFiles.FocusedItem.Text);
                sfdExportFile.Title = "Export " + Path.GetFileName(lstFiles.FocusedItem.Text) + " to...";

                DialogResult save = sfdExportFile.ShowDialog();

                if (save == DialogResult.OK)
                {
                    string exportPath = sfdExportFile.FileName;
                    
                    try
                    {
                        AddonHandle.ExportFile(contentPath, exportPath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Another file is already exported at " + exportPath, "Export file",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show("The file is already exported elsewhere.", "Export file",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("The file cannot be created on the disk.", "Export file",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    // Add a custom event handler so that the form gets updated when a file is pullable.
                    AddonHandle.WatchedFiles.Where(f => f.ContentPath == contentPath).First().Watcher.Changed += 
                        new FileSystemEventHandler(fsw_Changed);
                }

                sfdExportFile.Reset();
                UpdateFileList();
            }
        }

        private void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            UpdateModified();
            UpdateFileList();
        }

        private void tsmFileDropExport_Click(object sender, EventArgs e)
        {
            if (lstFiles.FocusedItem != null)
            {
                DropFileExport(lstFiles.FocusedItem.Group.Header + "/" + lstFiles.FocusedItem.Text);
            }
        }

        private void tsbDropAll_Click(object sender, EventArgs e)
        {
            List<string> pathsFailedToDelete = new List<string>();

            foreach (FileWatch watch in AddonHandle.WatchedFiles)
            {
                try
                {
                    AddonHandle.DropExport(watch.ContentPath);
                }
                catch (IOException)
                {
                    pathsFailedToDelete.Add(watch.FilePath);
                }
            }

            if (pathsFailedToDelete.Count == 0)
                UpdateStatus("Removed all current exported files");
            else if (pathsFailedToDelete.Count == 1)
                MessageBox.Show("Failed to remove " + pathsFailedToDelete[0], "Drop all exports",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (pathsFailedToDelete.Count > 1)
            {
                DialogResult showFailedFiles = MessageBox.Show(pathsFailedToDelete.Count + " files failed to get removed." +
                    "\n\nShow a list of failures?", "Drop all exports", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (showFailedFiles == DialogResult.Yes)
                {
                    string temppath = ContentFile.GenerateExternalPath(
                            new Random().Next() + "_failedRemovals") + ".txt";

                    try
                    {
                        File.WriteAllText(temppath,
                            "These files failed to get removed:\r\n\r\n" +
                            String.Join("\r\n", pathsFailedToDelete.ToArray()));
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Can't show the list, an error happened generating it.", "Drop all exports",
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

            UpdateFileList();
        }

        private void DropFileExport(string filename)
        {
            try
            {
                AddonHandle.DropExport(filename);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("This file is not exported!", "Drop extract",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            catch (IOException ex)
            {
                MessageBox.Show("Failed to delete the exported file:" +
                    "\n" + AddonHandle.WatchedFiles.Where(f => f.ContentPath == filename).First().FilePath +
                    ".\n\nBecause error happened: " + ex.Message, "Drop extract",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            UpdateFileList();
        }

        private void tsmFilePull_Click(object sender, EventArgs e)
        {
            if (lstFiles.FocusedItem != null)
                PullFile(lstFiles.FocusedItem.Group.Header + "/" + lstFiles.FocusedItem.Text);
        }

        private void tsbPullAll_Click(object sender, EventArgs e)
        {
            List<string> pathsFailedToPull = new List<string>();

            foreach (FileWatch watch in AddonHandle.WatchedFiles)
            {
                if (watch.Modified)
                {
                    try
                    {
                        AddonHandle.Pull(watch.ContentPath);
                    }
                    catch (IOException)
                    {
                        pathsFailedToPull.Add(watch.ContentPath + " from " + watch.FilePath);
                        continue;
                    }
                }
            }

            if (pathsFailedToPull.Count == 0)
                UpdateStatus("Successfully pulled all changes");
            else if (pathsFailedToPull.Count == 1)
                MessageBox.Show("Failed to pull " + pathsFailedToPull[0], "Pull all changes",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (pathsFailedToPull.Count > 1)
            {
                DialogResult showFailedFiles = MessageBox.Show(pathsFailedToPull.Count + " files failed to get pulled." +
                    "\n\nShow a list of failures?", "Pull all changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (showFailedFiles == DialogResult.Yes)
                {
                    string temppath = ContentFile.GenerateExternalPath(
                            new Random().Next() + "_failedPulls") + ".txt";

                    try
                    {
                        File.WriteAllText(temppath,
                            "These files failed to get pulled:\r\n\r\n" +
                            String.Join("\r\n", pathsFailedToPull.ToArray()));
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Can't show the list, an error happened generating it.", "Pull all changes",
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

            UpdateModified();
            UpdateFileList();
        }

        /// <summary>
        /// Pulls the changes of the specified file from its exported version.
        /// </summary>
        /// <param name="filename">The internal path of the file changes should be pulled into.
        /// The exported path is known automatically.</param>
        private void PullFile(string filename)
        {
            if (!AddonHandle.CanWrite)
                return;

            try
            {
                AddonHandle.Pull(filename);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("This file is not exported!", "Pull changes",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            catch (IOException ex)
            {
                MessageBox.Show("Failed to open the exported file (" +
                    AddonHandle.WatchedFiles.Where(f => f.ContentPath == filename).First().FilePath +
                    ").\n\nAn error happened: " + ex.Message, "Pull changes",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            UpdateStatus("Successfully pulled changes for " + filename);
            UpdateModified();
            UpdateFileList();
        }

        private void tsmFileExtract_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItems.Count == 1)
            {
                if (lstFiles.FocusedItem != null)
                {
                    string contentPath = lstFiles.FocusedItem.Group.Header + "/" + lstFiles.FocusedItem.Text;

                    sfdExportFile.FileName = Path.GetFileName(lstFiles.FocusedItem.Text);
                    sfdExportFile.DefaultExt = Path.GetExtension(lstFiles.FocusedItem.Text);
                    sfdExportFile.Title = "Extract " + Path.GetFileName(lstFiles.FocusedItem.Text) + " to...";

                    DialogResult save = sfdExportFile.ShowDialog();

                    if (save == DialogResult.OK)
                    {
                        string extractPath = sfdExportFile.FileName;

                        try
                        {
                            AddonHandle.ExtractFile(contentPath, extractPath);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            MessageBox.Show("This file is already exported at " + extractPath, "Extract file",
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }
                        catch (IOException)
                        {
                            MessageBox.Show("There was a problem creating the file.", "Extract file",
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }
                    }

                    sfdExportFile.Reset();
                }
            }
            else if (lstFiles.SelectedItems.Count >= 1)
            {
                fbdFileExtractMulti.Description = "Extract the selected " + Convert.ToString(lstFiles.SelectedItems.Count) +
                    " files to...";
                fbdFileExtractMulti.SelectedPath = Directory.GetCurrentDirectory();

                DialogResult save = fbdFileExtractMulti.ShowDialog();
                if (save == DialogResult.OK)
                {
                    string extractPath = fbdFileExtractMulti.SelectedPath;
                    List<string> contentPaths = new List<string>(lstFiles.SelectedItems.Count);

                    foreach (System.Windows.Forms.ListViewItem file in lstFiles.SelectedItems)
                        contentPaths.Add(file.Group.Header + "/" + file.Text);

                    // Get all the ContentFile objects from the open addon which has path
                    // matching to the paths we've selected in the list view.
                    IEnumerable<ContentFile> files = AddonHandle.OpenAddon.Files.Join(contentPaths,
                        cfile => cfile.Path, cpath => cpath, (cfile, cpath) => cfile);

                    List<string> failed_paths = new List<string>(lstFiles.SelectedItems.Count);

                    foreach (ContentFile file in files)
                    {
                        string outpath = extractPath + Path.DirectorySeparatorChar + Path.GetFileName(file.Path);

                        try
                        {
                            AddonHandle.ExtractFile(file.Path, outpath);
                        }
                        catch (Exception)
                        {
                            failed_paths.Add(file.Path);
                            continue;
                        }
                    }

                    if (failed_paths.Count == 0)
                        UpdateStatus("Extracted " + files.Count() + " files successfully");
                    else if (failed_paths.Count == 1)
                        MessageBox.Show("Failed to extract " + failed_paths[0], "Extract files",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    else if (failed_paths.Count > 1)
                    {
                        DialogResult showFailedFiles = MessageBox.Show(Convert.ToString(failed_paths.Count) +
                            " files failed to export.\n\nShow a list of failures?", "Extract files",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                        if (showFailedFiles == DialogResult.Yes)
                        {
                            string temppath = ContentFile.GenerateExternalPath(
                                    new Random().Next() + "_failedExtracts") + ".txt";

                            try
                            {
                                File.WriteAllText(temppath,
                                    "These files failed to get removed:\r\n\r\n" +
                                    String.Join("\r\n", failed_paths.ToArray()));
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Can't show the list, an error happened generating it.", "Extract files",
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
                }

                fbdFileExtractMulti.Reset();
            }
        }

        private void tsmShellExec_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItems.Count == 1)
            {
                if (lstFiles.FocusedItem != null)
                {
                    string temppath;
                    try
                    {
                        temppath = Path.GetTempPath() + "/" + Path.GetFileName(lstFiles.FocusedItem.Text);

                        try
                        {
                            File.WriteAllBytes(temppath, AddonHandle.GetFile(lstFiles.FocusedItem.Group.Header +
                                "/" + lstFiles.FocusedItem.Text).Content);
                        }
                        catch (FileNotFoundException)
                        {
                            MessageBox.Show("The file is not in the archive.", "Shell execute",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Failed to extract file to disk.", "Shell execute",
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show("The file is not in the archive.", "Shell execute",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    // Start the file
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(temppath)
                    {
                        UseShellExecute = true,
                    });
                }
            }
        }

        private void lstFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void lstFiles_DragDrop(object sender, DragEventArgs e)
        {
            List<string> files = ((String[])e.Data.GetData(DataFormats.FileDrop)).ToList();

            if (files.Count == 0)
            {
                return;
            }
            else if (files.Count == 1 && files[0].EndsWith(".gma"))
            {
                if (files[0].EndsWith(".gma"))
                {
                    DialogResult dropChanges = new DialogResult();
                    if (AddonHandle is RealtimeAddon && AddonHandle.Modified)
                    {
                        dropChanges = MessageBox.Show("Do you want to open another addon without saving the current first?",
                            "An addon is already open", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    }

                    if (dropChanges == DialogResult.Yes || AddonHandle == null ||
                        (AddonHandle is RealtimeAddon && !AddonHandle.Modified))
                    {
                        UnloadAddon();
                        LoadAddon(files[0]);
                    }
                }
            }
            else
            {
                if (files.Any(f => f.EndsWith(".gma")))
                {
                    MessageBox.Show("One or more drag-and-dropped files are GMAs.\nTo load a GMA, only drop one file " +
                        "into SharpGMad.", "Add files",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                if (!AddonHandle.CanWrite)
                    return;

                List<string> addFailures = new List<string>(files.Count);
                List<string> filesToParse = new List<string>(files); // Create a new list so we can run the foreach below

                foreach (string f in files)
                {
                    if ((File.GetAttributes(f) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        // The file is in fact a directory, parse the files within and add them
                        foreach (string subfile in Directory.GetFiles(f, "*", SearchOption.AllDirectories))
                        {
                            filesToParse.Add(subfile);
                        }

                        filesToParse.Remove(f);
                    }
                }

                foreach (string f in filesToParse)
                {
                    string filename = f.Replace("\\", "/");

                    try
                    {
                        AddonHandle.OpenAddon.CheckRestrictions(Whitelist.GetMatchingString(filename));
                        AddonHandle.AddFile(Whitelist.GetMatchingString(filename), File.ReadAllBytes(f));
                    }
                    catch (IOException)
                    {
                        addFailures.Add("Error happened while reading " + f);
                        continue;
                    }
                    catch (IgnoredException)
                    {
                        addFailures.Add("File is ignored (" + f + ")");
                        continue;
                    }
                    catch (WhitelistException)
                    {
                        addFailures.Add("File is not whitelisted (" + f + ")");
                        continue;
                    }
                    catch (ArgumentException)
                    {
                        addFailures.Add("Similar file has already been added (" + f + ")");
                        continue;
                    }
                }

                UpdateModified();
                UpdateFileList();

                if (addFailures.Count == 0)
                    UpdateStatus("Successfully added " + (filesToParse.Count - addFailures.Count) + " files");
                else if (addFailures.Count == 1)
                    UpdateStatus(addFailures[0]);
                else if (addFailures.Count > 1)
                {
                    UpdateStatus(addFailures.Count + " files failed to add out of " + filesToParse.Count);
                    DialogResult showFailedFiles = MessageBox.Show(addFailures.Count + " files failed to add." +
                        "\n\nShow a list of failures?", "Add files", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (showFailedFiles == DialogResult.Yes)
                    {
                        string temppath = ContentFile.GenerateExternalPath(
                                new Random().Next() + "_failedAdds") + ".txt";

                        try
                        {
                            File.WriteAllText(temppath,
                                "These files failed to add:\r\n\r\n" +
                                String.Join("\r\n", addFailures.ToArray()));
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Can't show the list, an error happened generating it.", "Add files",
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
            }
        }
    }
}