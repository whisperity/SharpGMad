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
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    fs.Dispose();

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
            if (AddonHandle is RealtimeAddon)
            {
                dropChanges = MessageBox.Show("Do you want to open another addon without saving the current first?",
                    "An addon is already open", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (dropChanges == DialogResult.Yes || AddonHandle == null)
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
                AddonHandle = RealtimeAddon.Load(path);
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

                tsbAddFile.Enabled = true;
                tsbUpdateMetadata.Enabled = true;
            }
        }

        /// <summary>
        /// Updates the metadata information printed to the user with the current metadata of the open addon.
        /// </summary>
        public void UpdateMetadataPanel()
        {
            txtTitle.Text = AddonHandle.OpenAddon.Title;
            txtAuthor.Text = AddonHandle.OpenAddon.Author;
            txtType.Text = AddonHandle.OpenAddon.Type;
            txtTags.Text = String.Join(", ", AddonHandle.OpenAddon.Tags.ToArray());
            txtDescription.Text = AddonHandle.OpenAddon.Description;
        }

        private void UpdateModified()
        {
            // Invoke the method if it was called from a thread which is not the thread Main was created in.
            //
            // (For example when fsw_Changed fires.)
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
                this.Text = Path.GetFileName(AddonHandle.AddonPath) +
                    (AddonHandle.Modified ? "*" : null) + " - SharpGMad";

                tsbSaveAddon.Enabled = AddonHandle.Modified;
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
                            tsbPullAll.Enabled = true; // At least one file is modified externally
                            item.ForeColor = Color.Indigo;
                        }
                    }

                    lstFiles.Items.Add(item);
                }
            }
        }

        /// <summary>
        /// Closes the currently open addon connection.
        /// </summary>
        private void UnloadAddon()
        {
            txtTitle.Text = String.Empty;
            txtAuthor.Text = String.Empty;
            txtType.Text = String.Empty;
            txtTags.Text = String.Empty;
            txtDescription.Text = String.Empty;

            lstFiles.Items.Clear();
            lstFiles.Groups.Clear();

            this.Text = "SharpGMad";
            tsbSaveAddon.Enabled = false;

            if (AddonHandle != null)
                AddonHandle.Close();
            AddonHandle = null;

            tsbUpdateMetadata.Enabled = false;
            tsbAddFile.Enabled = false;
        }

        // Dock the txtDescription text box.
        // It gets automatically resized when the form is resized.
        Size txtDescriptionSizeDifference;
        private void Main_Load(object sender, EventArgs e)
        {
            txtDescriptionSizeDifference = new Size(pnlRightSide.Size.Width - txtDescription.Size.Width,
                pnlRightSide.Size.Height - txtDescription.Size.Height);
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            txtDescription.Size = new Size(pnlRightSide.Size.Width - txtDescriptionSizeDifference.Width,
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
                byte[] bytes;
                try
                {
                    using (FileStream fs = new FileStream(ofdAddFile.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        bytes = new byte[fs.Length];
                        fs.Read(bytes, 0, (int)fs.Length);
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show(ofdAddFile.FileName + "\n\nThere was an error reading the file.", "Cannot add file",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                string filename = ofdAddFile.FileName.Replace("\\", "/");

                try
                {
                    AddonHandle.AddFile(Whitelist.GetMatchingString(filename), bytes);
                }
                catch (IgnoredException)
                {
                    MessageBox.Show(ofdAddFile.FileName + "\n\nThis file is ignored by the addon.", "Cannot add file",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (WhitelistException)
                {
                    MessageBox.Show(ofdAddFile.FileName + "\n\nThis file is not allowed by the whitelist!", "Cannot add file",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
                catch (ArgumentException)
                {
                    MessageBox.Show(ofdAddFile.FileName + "\n\nA file like this has already been added. Please remove it first.",
                        "Cannot add file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    tsmFileRemove.Enabled = true;
                    tsmFileRemove.Visible = true;

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
                        tsmFileExportTo.Enabled = true;
                        tsmFilePull.Enabled = false;
                        tsmFileDropExport.Enabled = false;
                    }
                    else
                    {
                        // Pull (applicable if the file is changed) and drop
                        tsmFileExportTo.Enabled = false;
                        tsmFilePull.Enabled = isExported.First().Modified;
                        tsmFileDropExport.Enabled = true;
                    }

                    // But the buttons should be visible
                    tssExportSeparator.Visible = true;
                    tsmFileExportTo.Visible = true;
                    tsmFilePull.Visible = true;
                    tsmFileDropExport.Visible = true;
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
            if (lstFiles.SelectedItems.Count == 1)
            {
                if (lstFiles.FocusedItem != null)
                {
                    DialogResult remove = MessageBox.Show("Do you really wish to remove " +
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
                            MessageBox.Show("The file was not found in the archive!", "Remove file",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }

                        UpdateModified();
                        UpdateFileList();
                    }
                }
            }
            else if (lstFiles.SelectedItems.Count >= 1)
            {
                DialogResult remove = MessageBox.Show("Do you really wish to remove " +
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

                    if (failed_paths.Count != 0)
                    {
                        MessageBox.Show("The following files failed to remove:\n\n" + String.Join("\n", failed_paths),
                            "Remove files", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }

                    UpdateModified();
                    UpdateFileList();
                }
            }
        }

        private void tsbUpdateMetadata_Click(object sender, EventArgs e)
        {
            UpdateMetadata mdForm = new UpdateMetadata(AddonHandle);
            mdForm.Owner = this;
            mdForm.ShowDialog(this);
        }

        private void tsbSaveAddon_Click(object sender, EventArgs e)
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
                    MessageBox.Show("There was an error saving the addon:\n" + ex.Message + "\n\nThis usually indicates" +
                         " problems with the addon's metadata containing invalid values.\nPlease use the \"Update metadata\"" +
                         " panel to set the values properly.", "Save addon",
                         MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                if (!(e is FormClosingEventArgs))
                {
                    MessageBox.Show("Successfully saved the addon.", "Save addon",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                UpdateModified();
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AddonHandle is RealtimeAddon && AddonHandle.Modified)
            {
                DialogResult yesClose = MessageBox.Show("Do you want to save your changes before quiting?",
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
            if (AddonHandle is RealtimeAddon)
            {
                dropChanges = MessageBox.Show("Do you want to open another addon without saving the current first?",
                    "An addon is already open", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (dropChanges == DialogResult.Yes || AddonHandle == null)
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
                        MessageBox.Show("There was a problem creating the addon.", "New addon",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    AddonHandle.OpenAddon.Title = Path.GetFileNameWithoutExtension(sfdAddon.FileName);
                    AddonHandle.OpenAddon.Author = "Author Name"; // This is currently not changable
                    AddonHandle.OpenAddon.Description = String.Empty;
                    AddonHandle.OpenAddon.Type = String.Empty;
                    AddonHandle.OpenAddon.Tags = new List<string>();
                    tsbUpdateMetadata_Click(sender, e); // This will make the metadata form pop up setting the initial value

                    // Fire the save event for an initial addon saving
                    tsbSaveAddon_Click(sender, e);

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
                        MessageBox.Show("Another file is already exported as " + exportPath, "Export file",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show("This file is already exported. Drop the export first!", "Export file",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("There was a problem creating the file.", "Export file",
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
            {
                MessageBox.Show("Successfully removed all currently exported files.", "Drop all exports",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Removed all currently exported files.\n\nThe following files failed to remove:" +
                    "\n\n" + String.Join("\n", pathsFailedToDelete), "Drop all exports",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    ".\n\n" + ex.Message, "Drop extract",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            UpdateFileList();
        }

        private void tsmFilePull_Click(object sender, EventArgs e)
        {
            if (lstFiles.FocusedItem != null)
            {
                PullFile(lstFiles.FocusedItem.Group.Header + "/" + lstFiles.FocusedItem.Text);
            }
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
            {
                MessageBox.Show("Successfully updated changes from all exported files.", "Update exported files",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Successfully updated changes from all exported files.\n\nThe following files failed to update:" +
                    "\n\n" + String.Join("\n", pathsFailedToPull), "Update exported files",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("Failed to open the exported file on the disk (" +
                    AddonHandle.WatchedFiles.Where(f => f.ContentPath == filename).First().FilePath +
                    "). An exception happened:\n\n" + ex.Message, "Pull changes",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            MessageBox.Show("Successfully pulled the changes.", "Pull changes",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

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

                    if (failed_paths.Count != 0)
                    {
                        MessageBox.Show("The following files failed extract:\n\n" + String.Join("\n", failed_paths),
                            "Extract multiple files", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                            MessageBox.Show("The file was not found in the archive!", "Remove file",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("The file couldn't be saved to the disk.", "Shell execute",
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show("The file was not found in the archive!", "Remove file",
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
    }
}