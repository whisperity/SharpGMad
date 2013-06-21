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
        /// The System.IO.FileStream connection to the current open addon on the disk.
        /// </summary>
        FileStream addonFS;
        /// <summary>
        /// The current open addon.
        /// </summary>
        Addon addon;
        /// <summary>
        /// Contains a list of externally exported watched files.
        /// </summary>
        List<FileWatch> watches;
        /// <summary>
        /// Gets or sets the full path of the current open addon.
        /// </summary>
        string path;
        /// <summary>
        /// Gets or sets whether the current addon is modified.
        /// </summary>
        bool modified;
        
        private Main()
        {
            InitializeComponent();
            watches = new List<FileWatch>();
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
            if (addon is Addon)
            {
                dropChanges = MessageBox.Show("Do you want to open another addon without saving the current first?",
                    "An addon is already open", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (dropChanges == DialogResult.Yes || addon == null)
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
                addonFS = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
                addon = new Addon(new Reader(addonFS));
            }
            catch (IndexOutOfRangeException)
            {
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

            if (addon is Addon)
            {
                this.path = path;

                SetModified(false);

                foreach (FileWatch watch in watches)
                    watch.Watcher.Dispose();

                watches.Clear();

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
            txtTitle.Text = addon.Title;
            txtAuthor.Text = addon.Author;
            txtType.Text = addon.Type;
            txtTags.Text = String.Join(", ", addon.Tags.ToArray());
            txtDescription.Text = addon.Description;
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
                    addon.Files.GroupBy(f => Path.GetDirectoryName(f.Path).Replace('\\', '/'));
                foreach (IGrouping<string, ContentFile> folder in folders)
                {
                    lstFiles.Groups.Add(folder.Key, folder.Key);
                }

                // Get and add the files
                foreach (ContentFile cfile in addon.Files)
                {
                    ListViewItem item = new ListViewItem(Path.GetFileName(cfile.Path),
                        lstFiles.Groups[Path.GetDirectoryName(cfile.Path).Replace('\\', '/')]);

                    IEnumerable<FileWatch> watch = watches.Where(f => f.ContentPath == cfile.Path);
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

            this.path = null;

            SetModified(false);
            this.Text = "SharpGMad";
            tsbSaveAddon.Enabled = false;

            if (addonFS != null)
            {
                addonFS.Dispose();
                addonFS = null;
            }

            addon = null;
            if (addonFS != null)
                addonFS.Dispose();

            foreach (FileWatch watch in watches)
                watch.Watcher.Dispose();

            watches.Clear();

            tsbUpdateMetadata.Enabled = false;
            tsbAddFile.Enabled = false;
        }

        /// <summary>
        /// Sets the currently open addon's modified state.
        /// </summary>
        public void SetModified(bool modified)
        {
            if (modified)
            {
                this.modified = true;
                tsbSaveAddon.Enabled = true;

                this.Text = Path.GetFileName(this.path) + "* - SharpGMad";
            }
            else
            {
                this.modified = false;
                tsbSaveAddon.Enabled = false;

                this.Text = Path.GetFileName(this.path) + " - SharpGMad";
            }
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
            if (addon == null)
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
                    addon.AddFile(Whitelist.GetMatchingString(filename), bytes);
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

                SetModified(true);
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
                    // Allow remove and export
                    tsmFileRemove.Enabled = true;
                    tsmFileRemove.Visible = true;

                    tsmFileExtract.Enabled = true;
                    tsmFileExtract.Visible = true;

                    // Allow export (and related) options
                    IEnumerable<FileWatch> isExported = watches.Where(f => f.ContentPath ==
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
                // Multiple files support remove and extract, no export-related stuff
                tsmFileRemove.Enabled = true;
                tsmFileRemove.Visible = true;
                
                tsmFileExtract.Enabled = true;
                tsmFileExtract.Visible = true;

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
                            addon.RemoveFile(lstFiles.FocusedItem.Group.Header + "/" + lstFiles.FocusedItem.Text);
                        }
                        catch (FileNotFoundException)
                        {
                            MessageBox.Show("The file was not found in the archive!", "Remove file",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }

                        SetModified(true);
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
                            addon.RemoveFile(file.Group.Header + "/" + file.Text);
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

                    SetModified(true);
                    UpdateFileList();
                }
            }
        }

        private void tsbUpdateMetadata_Click(object sender, EventArgs e)
        {
            UpdateMetadata mdForm = new UpdateMetadata(addon);
            mdForm.Owner = this;
            mdForm.ShowDialog(this);
        }

        private void tsbSaveAddon_Click(object sender, EventArgs e)
        {
            if (this.modified)
            {
                addon.Sort();

                MemoryStream ms = new MemoryStream();

                try
                {
                    Writer.Create(addon, ms);
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

                addonFS.Seek(0, SeekOrigin.Begin);
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(addonFS);

                addonFS.Flush();
                ms.Dispose();

                SetModified(false);

                if (!(e is FormClosingEventArgs))
                {
                    MessageBox.Show("Successfully saved the addon.", "Save addon",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
                if (addon is Addon && this.modified)
                {
                    DialogResult yesClose = MessageBox.Show("Do you want to save your changes before quiting?",
                        this.path, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

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

                if (addon is Addon)
                {
                    UnloadAddon();
                }
        }

        private void tsbCreateAddon_Click(object sender, EventArgs e)
        {
            DialogResult dropChanges = new DialogResult();
            if (addon is Addon)
            {
                dropChanges = MessageBox.Show("Do you want to open another addon without saving the current first?",
                    "An addon is already open", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (dropChanges == DialogResult.Yes || addon == null)
            {
                UnloadAddon();
                DialogResult file = sfdAddon.ShowDialog();
                
                if (file == DialogResult.OK)
                {
                    try
                    {
                        this.addonFS = new FileStream(sfdAddon.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("There was a problem creating the addon.", "New addon",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    this.path = sfdAddon.FileName;

                    addon = new Addon();
                    addon.Title = Path.GetFileNameWithoutExtension(sfdAddon.FileName);
                    addon.Author = "Author Name"; // This is currently not changable
                    addon.Description = String.Empty;
                    addon.Type = String.Empty;
                    addon.Tags = new List<string>();
                    tsbUpdateMetadata_Click(sender, e); // This will make the metadata form pop up setting the initial value

                    // Fire the save event for an initial addon saving
                    SetModified(true);
                    tsbSaveAddon_Click(sender, e);

                    // (Excerpt from LoadAddon)
                    SetModified(false);

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

                IEnumerable<FileWatch> isExported = watches.Where(f => f.ContentPath == contentPath);
                if (isExported.Count() != 0)
                {
                    MessageBox.Show("This file is already exported. Drop the extract first!", "Export file",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                sfdExportFile.FileName = Path.GetFileName(lstFiles.FocusedItem.Text);
                sfdExportFile.DefaultExt = Path.GetExtension(lstFiles.FocusedItem.Text);
                sfdExportFile.Title = "Export " + Path.GetFileName(lstFiles.FocusedItem.Text) + " to...";

                DialogResult save = sfdExportFile.ShowDialog();

                if (save == DialogResult.OK)
                {
                    string exportPath = sfdExportFile.FileName;

                    IEnumerable<FileWatch> checkPathCollision = watches.Where(f => f.FilePath == exportPath);
                    if (checkPathCollision.Count() != 0)
                    {
                        MessageBox.Show("Another file is already exported as " + exportPath, "Export file",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    IEnumerable<ContentFile> file = addon.Files.Where(f => f.Path == contentPath);
                    
                    FileStream export;
                    try
                    {
                        export = new FileStream(exportPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("There was a problem opening the file.", "Export file",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                    export.SetLength(0); // Truncate the file.
                    export.Write(file.First().Content, 0, (int)file.First().Size);
                    export.Flush();
                    export.Dispose();

                    // Set up a watcher
                    FileSystemWatcher fsw = new FileSystemWatcher(Path.GetDirectoryName(exportPath), Path.GetFileName(exportPath));
                    fsw.NotifyFilter = NotifyFilters.LastWrite;
                    fsw.Changed += new FileSystemEventHandler(fsw_Changed);
                    fsw.EnableRaisingEvents = true;

                    FileWatch watch = new FileWatch();
                    watch.FilePath = exportPath;
                    watch.ContentPath = contentPath;
                    watch.Watcher = fsw;

                    watches.Add(watch);
                }

                sfdExportFile.Reset();
                UpdateFileList();
            }
        }

        private void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            IEnumerable<FileWatch> search = watches.Where(f => f.FilePath == e.FullPath);

            if (search.Count() != 0)
            {
                IEnumerable<ContentFile> content = addon.Files.Where(f => f.Path == search.First().ContentPath);

                if (content.Count() == 1)
                {
                    search.First().Modified = true;

                }
                else
                {
                    watches.Remove(search.First());
                    ((FileSystemWatcher)sender).Dispose();
                }
            }
            else
            {
                watches.Remove(search.First());
                ((FileSystemWatcher)sender).Dispose();
            }

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

            foreach (FileWatch watch in watches)
            {
                watch.Watcher.Dispose();
                try
                {
                    File.Delete(watch.FilePath);
                }
                catch (Exception)
                {
                    pathsFailedToDelete.Add(watch.FilePath);
                }
            }

            watches.Clear();

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
            IEnumerable<FileWatch> search = watches.Where(f => f.ContentPath == filename);

                if (search.Count() == 0)
                {
                    MessageBox.Show("This file is not exported!", "Drop extract",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                search.First().Watcher.Dispose();
                try
                {

                    File.Delete(search.First().FilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete the exported file:" +
                        "\n" + search.First().FilePath + ".\n\n" + ex.Message, "Drop extract",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                watches.Remove(search.First());
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

            foreach (FileWatch watch in watches)
            {
                if (watch.Modified)
                {
                    IEnumerable<ContentFile> content = addon.Files.Where(f => f.Path == watch.ContentPath);

                    FileStream fs;
                    try
                    {
                        fs = new FileStream(watch.FilePath, FileMode.Open, FileAccess.Read);
                    }
                    catch (IOException)
                    {
                        pathsFailedToPull.Add(watch.ContentPath + " from " + watch.FilePath);
                        return;
                    }

                    // Load and write the changes to memory
                    byte[] contBytes = new byte[fs.Length];
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.Read(contBytes, 0, (int)fs.Length);
                    content.First().Content = contBytes;

                    fs.Dispose();

                    watch.Modified = false;
                }
            }

            SetModified(true);

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

            UpdateFileList();
        }

        /// <summary>
        /// Pulls the changes of the specified file from its exported version.
        /// </summary>
        /// <param name="filename">The internal path of the file changes should be pulled into.
        /// The exported path is known automatically.</param>
        private void PullFile(string filename)
        {
            IEnumerable<FileWatch> search = watches.Where(f => f.ContentPath == filename);

            if (search.Count() == 0)
            {
                MessageBox.Show("This file is not exported!", "Pull changes",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            if (search.First().Modified == false)
            {
                MessageBox.Show("The file is not modified.", "Pull changes",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            IEnumerable<ContentFile> content = addon.Files.Where(f => f.Path == search.First().ContentPath);

            FileStream fs;
            try
            {
                fs = new FileStream(search.First().FilePath, FileMode.Open, FileAccess.Read);
            }
            catch (IOException ex)
            {
                MessageBox.Show("Failed to open the exported file on the disk (" +
                    search.First().FilePath + "). An exception happened:\n\n" + ex.Message, "Pull changes",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            // Load and write the changes to memory
            byte[] contBytes = new byte[fs.Length];
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(contBytes, 0, (int)fs.Length);
            content.First().Content = contBytes;

            fs.Dispose();

            MessageBox.Show("Successfully pulled the changes.", "Pull changes",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Consider the file unmodified
            search.First().Modified = false;

            // But consider the addon itself modified
            SetModified(true);

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

                        IEnumerable<ContentFile> file = addon.Files.Where(f => f.Path == contentPath);

                        FileStream export;
                        try
                        {
                            export = new FileStream(extractPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("There was a problem opening the file.", "Extract file",
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }
                        export.SetLength(0); // Truncate the file.
                        export.Write(file.First().Content, 0, (int)file.First().Size);
                        export.Flush();
                        export.Dispose();
                    }

                    sfdExportFile.Reset();
                }
            }
            else if (lstFiles.SelectedItems.Count >= 1)
            {
                fbdFileExtractMulti.Description = "Export the selected " + Convert.ToString(lstFiles.SelectedItems.Count) +
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
                    IEnumerable<ContentFile> files = addon.Files.Join(contentPaths,
                        cfile => cfile.Path, cpath => cpath, (cfile, cpath) => cfile);

                    List<string> failed_paths = new List<string>(lstFiles.SelectedItems.Count);

                    foreach (ContentFile file in files)
                    {
                        string outpath = extractPath + Path.DirectorySeparatorChar + Path.GetFileName(file.Path);

                        FileStream export;
                        try
                        {
                            export = new FileStream(outpath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                        }
                        catch (Exception)
                        {
                            failed_paths.Add(file.Path);
                            continue;
                        }

                        export.SetLength(0); // Truncate the file.
                        export.Write(file.Content, 0, (int)file.Size);
                        export.Flush();
                        export.Dispose();
                    }

                    if (failed_paths.Count != 0)
                    {
                        MessageBox.Show("The following files failed to remove:\n\n" + String.Join("\n", failed_paths),
                            "Remove files", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }

                fbdFileExtractMulti.Reset();
            }
        }

        private void tsmiGMod12Pack_Click(object sender, EventArgs e)
        {
            LegacyConvert convForm = new LegacyConvert();
            convForm.ShowDialog(this);
        }
    }
}