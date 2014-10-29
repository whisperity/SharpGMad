#if MONO
using System.Collections.Generic;
using System.Windows.Forms;
#endif

namespace SharpGMad
{
    partial class Main
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
#if MONO
                // There seems to be an error with how GDI+ disposes stuff related to resource bitmaps
                // in these ToolStripItem (and derived) objects.
                if (cmsFileEntry != null && !cmsFileEntry.IsDisposed)
                {
                    List<ToolStripItem> items = new List<ToolStripItem>(cmsFileEntry.Items.Count);

                    foreach (ToolStripItem item in cmsFileEntry.Items)
                        items.Add(item);

                    foreach (ToolStripItem item in items)
                        if (item != null && !item.IsDisposed)
                            item.Dispose();

                    cmsFileEntry.Dispose();
                    components.Remove(cmsFileEntry);

                    items.Clear();
                }

                if (tsddbLegacy != null && tsddbLegacy.HasDropDownItems && !tsddbLegacy.IsDisposed)
                {
                    List<ToolStripItem> items = new List<ToolStripItem>(tsddbLegacy.DropDownItems.Count);

                    foreach (ToolStripItem item in tsddbLegacy.DropDownItems)
                        items.Add(item);

                    foreach (ToolStripItem item in items)
                        if (item != null && !item.IsDisposed)
                            item.Dispose();

                    tsddbLegacy.Dispose();

                    items.Clear();
                }

                if (tsddbViewOptions != null && tsddbViewOptions.HasDropDownItems && !tsddbViewOptions.IsDisposed)
                {
                    List<ToolStripItem> items = new List<ToolStripItem>(tsddbViewOptions.DropDownItems.Count);

                    foreach (ToolStripItem item in tsddbViewOptions.DropDownItems)
                        items.Add(item);

                    foreach (ToolStripItem item in items)
                        if (item != null && !item.IsDisposed)
                            item.Dispose();

                    tsddbViewOptions.Dispose();

                    items.Clear();
                }
#endif
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
            this.components = new System.ComponentModel.Container();
            this.lstFiles = new System.Windows.Forms.ListView();
            this.chFilename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chFileType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ofdAddon = new System.Windows.Forms.OpenFileDialog();
            this.tsFileOperations = new System.Windows.Forms.ToolStrip();
            this.tsbAddFile = new System.Windows.Forms.ToolStripButton();
            this.tssExportHeaderSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tsbPullAll = new System.Windows.Forms.ToolStripButton();
            this.tsbDropAll = new System.Windows.Forms.ToolStripButton();
            this.tsddbViewOptions = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmiViewLargeIcons = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiViewSmallIcons = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiViewDetails = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiViewList = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiViewTiles = new System.Windows.Forms.ToolStripMenuItem();
            this.tssViewSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiViewShowAllFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiViewShowFolderTree = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiViewShowSubfolders = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlLeftSide = new System.Windows.Forms.Panel();
            this.spcFoldersAndFiles = new System.Windows.Forms.SplitContainer();
            this.tvFolders = new System.Windows.Forms.TreeView();
            this.pnlFileOpsToolbar = new System.Windows.Forms.Panel();
            this.ssStatus = new System.Windows.Forms.StatusStrip();
            this.tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlForm = new System.Windows.Forms.Panel();
            this.pnlRightSide = new System.Windows.Forms.Panel();
            this.txtMetadataDescription = new System.Windows.Forms.TextBox();
            this.tsMetadata = new System.Windows.Forms.ToolStrip();
            this.tsbUpdateMetadata = new System.Windows.Forms.ToolStripButton();
            this.tsbDiscardMetadataChanges = new System.Windows.Forms.ToolStripButton();
            this.lblDescription = new System.Windows.Forms.Label();
            this.cmbMetadataTag1 = new System.Windows.Forms.ComboBox();
            this.cmbMetadataTag2 = new System.Windows.Forms.ComboBox();
            this.lblTags = new System.Windows.Forms.Label();
            this.cmbMetadataType = new System.Windows.Forms.ComboBox();
            this.lblType = new System.Windows.Forms.Label();
            this.txtMetadataTitle = new System.Windows.Forms.TextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.tsToolbar = new System.Windows.Forms.ToolStrip();
            this.tsbCreateAddon = new System.Windows.Forms.ToolStripButton();
            this.tsbOpenAddon = new System.Windows.Forms.ToolStripButton();
            this.tsbSaveAddon = new System.Windows.Forms.ToolStripButton();
            this.tssAddonSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tsddbLegacy = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmiLegacyCreate = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiLegacyExtract = new System.Windows.Forms.ToolStripMenuItem();
            this.ofdAddFile = new System.Windows.Forms.OpenFileDialog();
            this.sfdAddon = new System.Windows.Forms.SaveFileDialog();
            this.sfdExportFile = new System.Windows.Forms.SaveFileDialog();
            this.tssExportSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.cmsFileEntry = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmFileRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmFileExtract = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmFileShellExec = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmFileExportTo = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmFilePull = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmFileOpenExport = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmFileDropExport = new System.Windows.Forms.ToolStripMenuItem();
            this.fbdFileExtractMulti = new System.Windows.Forms.FolderBrowserDialog();
            this.tsFileOperations.SuspendLayout();
            this.pnlLeftSide.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcFoldersAndFiles)).BeginInit();
            this.spcFoldersAndFiles.Panel1.SuspendLayout();
            this.spcFoldersAndFiles.Panel2.SuspendLayout();
            this.spcFoldersAndFiles.SuspendLayout();
            this.pnlFileOpsToolbar.SuspendLayout();
            this.ssStatus.SuspendLayout();
            this.pnlForm.SuspendLayout();
            this.pnlRightSide.SuspendLayout();
            this.tsMetadata.SuspendLayout();
            this.tsToolbar.SuspendLayout();
            this.cmsFileEntry.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstFiles
            // 
            this.lstFiles.AllowDrop = true;
            this.lstFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chFilename,
            this.chFileType,
            this.chSize});
            this.lstFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstFiles.Location = new System.Drawing.Point(0, 0);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.ShowGroups = false;
            this.lstFiles.ShowItemToolTips = true;
            this.lstFiles.Size = new System.Drawing.Size(383, 412);
            this.lstFiles.TabIndex = 0;
            this.lstFiles.TileSize = new System.Drawing.Size(150, 50);
            this.lstFiles.UseCompatibleStateImageBehavior = false;
            this.lstFiles.SelectedIndexChanged += new System.EventHandler(this.lstFiles_SelectedIndexChanged);
            this.lstFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.lstFiles_DragDrop);
            this.lstFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.lstFiles_DragEnter);
            this.lstFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstFiles_KeyDown);
            this.lstFiles.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lstFiles_MouseClick);
            this.lstFiles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstFiles_MouseDoubleClick);
            // 
            // chFilename
            // 
            this.chFilename.Text = "Filename";
            this.chFilename.Width = 182;
            // 
            // chFileType
            // 
            this.chFileType.Text = "Type";
            this.chFileType.Width = 113;
            // 
            // chSize
            // 
            this.chSize.Text = "Size";
            this.chSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chSize.Width = 75;
            // 
            // ofdAddon
            // 
            this.ofdAddon.Filter = "Garry\'s Mod Addons|*.gma";
            this.ofdAddon.Title = "Open addon file";
            // 
            // tsFileOperations
            // 
            this.tsFileOperations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tsFileOperations.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsFileOperations.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbAddFile,
            this.tssExportHeaderSeparator,
            this.tsbPullAll,
            this.tsbDropAll,
            this.tsddbViewOptions});
            this.tsFileOperations.Location = new System.Drawing.Point(0, 0);
            this.tsFileOperations.Name = "tsFileOperations";
            this.tsFileOperations.Size = new System.Drawing.Size(552, 26);
            this.tsFileOperations.TabIndex = 0;
            // 
            // tsbAddFile
            // 
            this.tsbAddFile.AutoToolTip = false;
            this.tsbAddFile.Enabled = false;
            this.tsbAddFile.Image = global::SharpGMad.Properties.Resources.add;
            this.tsbAddFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddFile.Name = "tsbAddFile";
            this.tsbAddFile.Size = new System.Drawing.Size(68, 23);
            this.tsbAddFile.Text = "Add file";
            this.tsbAddFile.Click += new System.EventHandler(this.tsbAddFile_Click);
            // 
            // tssExportHeaderSeparator
            // 
            this.tssExportHeaderSeparator.Name = "tssExportHeaderSeparator";
            this.tssExportHeaderSeparator.Size = new System.Drawing.Size(6, 26);
            // 
            // tsbPullAll
            // 
            this.tsbPullAll.AutoToolTip = false;
            this.tsbPullAll.Enabled = false;
            this.tsbPullAll.Image = global::SharpGMad.Properties.Resources.pull_all;
            this.tsbPullAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPullAll.Name = "tsbPullAll";
            this.tsbPullAll.Size = new System.Drawing.Size(138, 23);
            this.tsbPullAll.Text = "Update exported files";
            this.tsbPullAll.ToolTipText = "Updates all changes from external files into the addon";
            this.tsbPullAll.Click += new System.EventHandler(this.tsbPullAll_Click);
            // 
            // tsbDropAll
            // 
            this.tsbDropAll.AutoToolTip = false;
            this.tsbDropAll.Enabled = false;
            this.tsbDropAll.Image = global::SharpGMad.Properties.Resources.drop_all;
            this.tsbDropAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDropAll.Name = "tsbDropAll";
            this.tsbDropAll.Size = new System.Drawing.Size(94, 23);
            this.tsbDropAll.Text = "Drop exports";
            this.tsbDropAll.ToolTipText = "Deletes all exported files";
            this.tsbDropAll.Click += new System.EventHandler(this.tsbDropAll_Click);
            // 
            // tsddbViewOptions
            // 
            this.tsddbViewOptions.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsddbViewOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiViewLargeIcons,
            this.tsmiViewSmallIcons,
            this.tsmiViewDetails,
            this.tsmiViewList,
            this.tsmiViewTiles,
            this.tssViewSeparator,
            this.tsmiViewShowAllFiles,
            this.tsmiViewShowFolderTree,
            this.tsmiViewShowSubfolders});
            this.tsddbViewOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsddbViewOptions.Name = "tsddbViewOptions";
            this.tsddbViewOptions.Size = new System.Drawing.Size(45, 23);
            this.tsddbViewOptions.Text = "View";
            this.tsddbViewOptions.ToolTipText = "Change the view of the file list";
            // 
            // tsmiViewLargeIcons
            // 
            this.tsmiViewLargeIcons.Image = global::SharpGMad.Properties.Resources.view_largeicons;
            this.tsmiViewLargeIcons.Name = "tsmiViewLargeIcons";
            this.tsmiViewLargeIcons.Size = new System.Drawing.Size(134, 22);
            this.tsmiViewLargeIcons.Text = "Large Icons";
            this.tsmiViewLargeIcons.Click += new System.EventHandler(this.tsmiViewElements_changeView);
            // 
            // tsmiViewSmallIcons
            // 
            this.tsmiViewSmallIcons.Image = global::SharpGMad.Properties.Resources.view_smallicons;
            this.tsmiViewSmallIcons.Name = "tsmiViewSmallIcons";
            this.tsmiViewSmallIcons.Size = new System.Drawing.Size(134, 22);
            this.tsmiViewSmallIcons.Text = "Small Icons";
            this.tsmiViewSmallIcons.Click += new System.EventHandler(this.tsmiViewElements_changeView);
            // 
            // tsmiViewDetails
            // 
            this.tsmiViewDetails.Image = global::SharpGMad.Properties.Resources.view_details;
            this.tsmiViewDetails.Name = "tsmiViewDetails";
            this.tsmiViewDetails.Size = new System.Drawing.Size(134, 22);
            this.tsmiViewDetails.Text = "Details";
            this.tsmiViewDetails.Click += new System.EventHandler(this.tsmiViewElements_changeView);
            // 
            // tsmiViewList
            // 
            this.tsmiViewList.Image = global::SharpGMad.Properties.Resources.view_list;
            this.tsmiViewList.Name = "tsmiViewList";
            this.tsmiViewList.Size = new System.Drawing.Size(134, 22);
            this.tsmiViewList.Text = "List";
            this.tsmiViewList.Click += new System.EventHandler(this.tsmiViewElements_changeView);
            // 
            // tsmiViewTiles
            // 
            this.tsmiViewTiles.Image = global::SharpGMad.Properties.Resources.view_tiles;
            this.tsmiViewTiles.Name = "tsmiViewTiles";
            this.tsmiViewTiles.Size = new System.Drawing.Size(134, 22);
            this.tsmiViewTiles.Text = "Tiles";
            this.tsmiViewTiles.Click += new System.EventHandler(this.tsmiViewElements_changeView);
            // 
            // tssViewSeparator
            // 
            this.tssViewSeparator.Name = "tssViewSeparator";
            this.tssViewSeparator.Size = new System.Drawing.Size(131, 6);
            // 
            // tsmiViewShowAllFiles
            // 
            this.tsmiViewShowAllFiles.Image = global::SharpGMad.Properties.Resources.allfiles;
            this.tsmiViewShowAllFiles.Name = "tsmiViewShowAllFiles";
            this.tsmiViewShowAllFiles.Size = new System.Drawing.Size(134, 22);
            this.tsmiViewShowAllFiles.Text = "All files";
            this.tsmiViewShowAllFiles.ToolTipText = "Whether or not the file list view should show all files, not just current folder";
            this.tsmiViewShowAllFiles.Click += new System.EventHandler(this.tsmiViewShowAllFiles_Click);
            // 
            // tsmiViewShowFolderTree
            // 
            this.tsmiViewShowFolderTree.Checked = true;
            this.tsmiViewShowFolderTree.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiViewShowFolderTree.Image = global::SharpGMad.Properties.Resources.foldertree;
            this.tsmiViewShowFolderTree.Name = "tsmiViewShowFolderTree";
            this.tsmiViewShowFolderTree.Size = new System.Drawing.Size(134, 22);
            this.tsmiViewShowFolderTree.Text = "Folder tree";
            this.tsmiViewShowFolderTree.Click += new System.EventHandler(this.tsmiViewShowFolderTree_Click);
            // 
            // tsmiViewShowSubfolders
            // 
            this.tsmiViewShowSubfolders.Checked = true;
            this.tsmiViewShowSubfolders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiViewShowSubfolders.Image = global::SharpGMad.Properties.Resources.folder_s;
            this.tsmiViewShowSubfolders.Name = "tsmiViewShowSubfolders";
            this.tsmiViewShowSubfolders.Size = new System.Drawing.Size(134, 22);
            this.tsmiViewShowSubfolders.Text = "Subfolders";
            this.tsmiViewShowSubfolders.ToolTipText = "Switch whether the file list should show subfolders or not";
            this.tsmiViewShowSubfolders.Click += new System.EventHandler(this.tsmiViewShowSubfolders_Click);
            // 
            // pnlLeftSide
            // 
            this.pnlLeftSide.Controls.Add(this.spcFoldersAndFiles);
            this.pnlLeftSide.Controls.Add(this.pnlFileOpsToolbar);
            this.pnlLeftSide.Controls.Add(this.ssStatus);
            this.pnlLeftSide.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeftSide.Location = new System.Drawing.Point(0, 0);
            this.pnlLeftSide.Name = "pnlLeftSide";
            this.pnlLeftSide.Size = new System.Drawing.Size(552, 443);
            this.pnlLeftSide.TabIndex = 11;
            // 
            // spcFoldersAndFiles
            // 
            this.spcFoldersAndFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcFoldersAndFiles.Location = new System.Drawing.Point(0, 26);
            this.spcFoldersAndFiles.Name = "spcFoldersAndFiles";
            // 
            // spcFoldersAndFiles.Panel1
            // 
            this.spcFoldersAndFiles.Panel1.Controls.Add(this.tvFolders);
            // 
            // spcFoldersAndFiles.Panel2
            // 
            this.spcFoldersAndFiles.Panel2.Controls.Add(this.lstFiles);
            this.spcFoldersAndFiles.Size = new System.Drawing.Size(552, 412);
            this.spcFoldersAndFiles.SplitterDistance = 165;
            this.spcFoldersAndFiles.TabIndex = 5;
            // 
            // tvFolders
            // 
            this.tvFolders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvFolders.Location = new System.Drawing.Point(0, 0);
            this.tvFolders.Name = "tvFolders";
            this.tvFolders.PathSeparator = "/";
            this.tvFolders.ShowRootLines = false;
            this.tvFolders.Size = new System.Drawing.Size(165, 412);
            this.tvFolders.TabIndex = 4;
            this.tvFolders.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvFolders_AfterSelect);
            // 
            // pnlFileOpsToolbar
            // 
            this.pnlFileOpsToolbar.Controls.Add(this.tsFileOperations);
            this.pnlFileOpsToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFileOpsToolbar.Location = new System.Drawing.Point(0, 0);
            this.pnlFileOpsToolbar.Name = "pnlFileOpsToolbar";
            this.pnlFileOpsToolbar.Size = new System.Drawing.Size(552, 26);
            this.pnlFileOpsToolbar.TabIndex = 0;
            // 
            // ssStatus
            // 
            this.ssStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslStatus});
            this.ssStatus.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.ssStatus.Location = new System.Drawing.Point(0, 438);
            this.ssStatus.Name = "ssStatus";
            this.ssStatus.Size = new System.Drawing.Size(552, 5);
            this.ssStatus.SizingGrip = false;
            this.ssStatus.TabIndex = 3;
            // 
            // tsslStatus
            // 
            this.tsslStatus.Name = "tsslStatus";
            this.tsslStatus.Size = new System.Drawing.Size(0, 0);
            // 
            // pnlForm
            // 
            this.pnlForm.Controls.Add(this.pnlLeftSide);
            this.pnlForm.Controls.Add(this.pnlRightSide);
            this.pnlForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlForm.Location = new System.Drawing.Point(0, 25);
            this.pnlForm.Name = "pnlForm";
            this.pnlForm.Size = new System.Drawing.Size(759, 443);
            this.pnlForm.TabIndex = 12;
            // 
            // pnlRightSide
            // 
            this.pnlRightSide.Controls.Add(this.txtMetadataDescription);
            this.pnlRightSide.Controls.Add(this.tsMetadata);
            this.pnlRightSide.Controls.Add(this.lblDescription);
            this.pnlRightSide.Controls.Add(this.cmbMetadataTag1);
            this.pnlRightSide.Controls.Add(this.cmbMetadataTag2);
            this.pnlRightSide.Controls.Add(this.lblTags);
            this.pnlRightSide.Controls.Add(this.cmbMetadataType);
            this.pnlRightSide.Controls.Add(this.lblType);
            this.pnlRightSide.Controls.Add(this.txtMetadataTitle);
            this.pnlRightSide.Controls.Add(this.lblTitle);
            this.pnlRightSide.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlRightSide.Location = new System.Drawing.Point(552, 0);
            this.pnlRightSide.Name = "pnlRightSide";
            this.pnlRightSide.Size = new System.Drawing.Size(207, 443);
            this.pnlRightSide.TabIndex = 12;
            // 
            // txtMetadataDescription
            // 
            this.txtMetadataDescription.AcceptsTab = true;
            this.txtMetadataDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMetadataDescription.Location = new System.Drawing.Point(20, 153);
            this.txtMetadataDescription.Multiline = true;
            this.txtMetadataDescription.Name = "txtMetadataDescription";
            this.txtMetadataDescription.ReadOnly = true;
            this.txtMetadataDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMetadataDescription.Size = new System.Drawing.Size(184, 278);
            this.txtMetadataDescription.TabIndex = 7;
            // 
            // tsMetadata
            // 
            this.tsMetadata.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsMetadata.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbUpdateMetadata,
            this.tsbDiscardMetadataChanges});
            this.tsMetadata.Location = new System.Drawing.Point(0, 0);
            this.tsMetadata.Name = "tsMetadata";
            this.tsMetadata.Size = new System.Drawing.Size(207, 25);
            this.tsMetadata.TabIndex = 10;
            // 
            // tsbUpdateMetadata
            // 
            this.tsbUpdateMetadata.AutoToolTip = false;
            this.tsbUpdateMetadata.Enabled = false;
            this.tsbUpdateMetadata.Image = global::SharpGMad.Properties.Resources.metadata;
            this.tsbUpdateMetadata.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUpdateMetadata.Name = "tsbUpdateMetadata";
            this.tsbUpdateMetadata.Size = new System.Drawing.Size(118, 22);
            this.tsbUpdateMetadata.Text = "Update metadata";
            this.tsbUpdateMetadata.Click += new System.EventHandler(this.tsbUpdateMetadata_Click);
            // 
            // tsbDiscardMetadataChanges
            // 
            this.tsbDiscardMetadataChanges.Enabled = false;
            this.tsbDiscardMetadataChanges.Image = global::SharpGMad.Properties.Resources.discard;
            this.tsbDiscardMetadataChanges.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDiscardMetadataChanges.Name = "tsbDiscardMetadataChanges";
            this.tsbDiscardMetadataChanges.Size = new System.Drawing.Size(66, 22);
            this.tsbDiscardMetadataChanges.Text = "Discard";
            this.tsbDiscardMetadataChanges.ToolTipText = "Discard metadata changes";
            this.tsbDiscardMetadataChanges.Visible = false;
            this.tsbDiscardMetadataChanges.Click += new System.EventHandler(this.tsbDiscardMetadataChanges_Click);
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(6, 136);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(63, 13);
            this.lblDescription.TabIndex = 6;
            this.lblDescription.Text = "Description:";
            // 
            // cmbMetadataTag1
            // 
            this.cmbMetadataTag1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMetadataTag1.Enabled = false;
            this.cmbMetadataTag1.FormattingEnabled = true;
            this.cmbMetadataTag1.Location = new System.Drawing.Point(20, 113);
            this.cmbMetadataTag1.Name = "cmbMetadataTag1";
            this.cmbMetadataTag1.Size = new System.Drawing.Size(92, 21);
            this.cmbMetadataTag1.Sorted = true;
            this.cmbMetadataTag1.TabIndex = 12;
            // 
            // cmbMetadataTag2
            // 
            this.cmbMetadataTag2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMetadataTag2.Enabled = false;
            this.cmbMetadataTag2.FormattingEnabled = true;
            this.cmbMetadataTag2.Location = new System.Drawing.Point(112, 113);
            this.cmbMetadataTag2.Name = "cmbMetadataTag2";
            this.cmbMetadataTag2.Size = new System.Drawing.Size(92, 21);
            this.cmbMetadataTag2.Sorted = true;
            this.cmbMetadataTag2.TabIndex = 13;
            // 
            // lblTags
            // 
            this.lblTags.AutoSize = true;
            this.lblTags.Location = new System.Drawing.Point(6, 97);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(34, 13);
            this.lblTags.TabIndex = 4;
            this.lblTags.Text = "Tags:";
            // 
            // cmbMetadataType
            // 
            this.cmbMetadataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMetadataType.Enabled = false;
            this.cmbMetadataType.FormattingEnabled = true;
            this.cmbMetadataType.Location = new System.Drawing.Point(20, 73);
            this.cmbMetadataType.Name = "cmbMetadataType";
            this.cmbMetadataType.Size = new System.Drawing.Size(184, 21);
            this.cmbMetadataType.Sorted = true;
            this.cmbMetadataType.TabIndex = 11;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(6, 58);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(34, 13);
            this.lblType.TabIndex = 2;
            this.lblType.Text = "Type:";
            // 
            // txtMetadataTitle
            // 
            this.txtMetadataTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMetadataTitle.Location = new System.Drawing.Point(20, 42);
            this.txtMetadataTitle.Name = "txtMetadataTitle";
            this.txtMetadataTitle.ReadOnly = true;
            this.txtMetadataTitle.Size = new System.Drawing.Size(184, 13);
            this.txtMetadataTitle.TabIndex = 1;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(6, 26);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(30, 13);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Title:";
            // 
            // tsToolbar
            // 
            this.tsToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbCreateAddon,
            this.tsbOpenAddon,
            this.tsbSaveAddon,
            this.tssAddonSeparator,
            this.tsddbLegacy});
            this.tsToolbar.Location = new System.Drawing.Point(0, 0);
            this.tsToolbar.Name = "tsToolbar";
            this.tsToolbar.Size = new System.Drawing.Size(759, 25);
            this.tsToolbar.TabIndex = 13;
            // 
            // tsbCreateAddon
            // 
            this.tsbCreateAddon.AutoToolTip = false;
            this.tsbCreateAddon.Image = global::SharpGMad.Properties.Resources.newaddon;
            this.tsbCreateAddon.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCreateAddon.Name = "tsbCreateAddon";
            this.tsbCreateAddon.Size = new System.Drawing.Size(86, 22);
            this.tsbCreateAddon.Text = "Create new";
            this.tsbCreateAddon.Click += new System.EventHandler(this.tsbCreateAddon_Click);
            // 
            // tsbOpenAddon
            // 
            this.tsbOpenAddon.AutoToolTip = false;
            this.tsbOpenAddon.Image = global::SharpGMad.Properties.Resources.open;
            this.tsbOpenAddon.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpenAddon.Name = "tsbOpenAddon";
            this.tsbOpenAddon.Size = new System.Drawing.Size(56, 22);
            this.tsbOpenAddon.Text = "Open";
            this.tsbOpenAddon.Click += new System.EventHandler(this.tsbOpenAddon_Click);
            // 
            // tsbSaveAddon
            // 
            this.tsbSaveAddon.AutoToolTip = false;
            this.tsbSaveAddon.Enabled = false;
            this.tsbSaveAddon.Image = global::SharpGMad.Properties.Resources.save;
            this.tsbSaveAddon.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSaveAddon.Name = "tsbSaveAddon";
            this.tsbSaveAddon.Size = new System.Drawing.Size(51, 22);
            this.tsbSaveAddon.Text = "Save";
            this.tsbSaveAddon.Click += new System.EventHandler(this.tsbSaveAddon_Click);
            // 
            // tssAddonSeparator
            // 
            this.tssAddonSeparator.Name = "tssAddonSeparator";
            this.tssAddonSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // tsddbLegacy
            // 
            this.tsddbLegacy.AutoToolTip = false;
            this.tsddbLegacy.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiLegacyCreate,
            this.tsmiLegacyExtract});
            this.tsddbLegacy.Image = global::SharpGMad.Properties.Resources.legacy;
            this.tsddbLegacy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsddbLegacy.Name = "tsddbLegacy";
            this.tsddbLegacy.Size = new System.Drawing.Size(132, 22);
            this.tsddbLegacy.Text = "Legacy operations";
            this.tsddbLegacy.ToolTipText = "Access the legacy wrapper windows";
            // 
            // tsmiLegacyCreate
            // 
            this.tsmiLegacyCreate.Image = global::SharpGMad.Properties.Resources.create;
            this.tsmiLegacyCreate.Name = "tsmiLegacyCreate";
            this.tsmiLegacyCreate.Size = new System.Drawing.Size(171, 22);
            this.tsmiLegacyCreate.Text = "Create from folder";
            this.tsmiLegacyCreate.ToolTipText = "Use the legacy method to compile a folder into an addon";
            this.tsmiLegacyCreate.Click += new System.EventHandler(this.tsmiLegacyCreate_Click);
            // 
            // tsmiLegacyExtract
            // 
            this.tsmiLegacyExtract.Image = global::SharpGMad.Properties.Resources.extract;
            this.tsmiLegacyExtract.Name = "tsmiLegacyExtract";
            this.tsmiLegacyExtract.Size = new System.Drawing.Size(171, 22);
            this.tsmiLegacyExtract.Text = "Extract to folder";
            this.tsmiLegacyExtract.ToolTipText = "Use the legacy method to fully unpack an addon to a folder";
            this.tsmiLegacyExtract.Click += new System.EventHandler(this.tsmiLegacyExtract_Click);
            // 
            // ofdAddFile
            // 
            this.ofdAddFile.Title = "Add file";
            // 
            // sfdAddon
            // 
            this.sfdAddon.DefaultExt = "gma";
            this.sfdAddon.Filter = "Garry\'s Mod Addons|*.gma";
            this.sfdAddon.Title = "Create new addon as";
            // 
            // sfdExportFile
            // 
            this.sfdExportFile.Filter = "All files|*.*";
            // 
            // tssExportSeparator
            // 
            this.tssExportSeparator.Name = "tssExportSeparator";
            this.tssExportSeparator.Size = new System.Drawing.Size(149, 6);
            // 
            // cmsFileEntry
            // 
            this.cmsFileEntry.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmFileShellExec,
            this.tsmFileExtract,
            this.tsmFileRemove,
            this.tssExportSeparator,
            this.tsmFileExportTo,
            this.tsmFilePull,
            this.tsmFileOpenExport,
            this.tsmFileDropExport});
            this.cmsFileEntry.Name = "cmsFileEntry";
            this.cmsFileEntry.Size = new System.Drawing.Size(153, 186);
            // 
            // tsmFileRemove
            // 
            this.tsmFileRemove.Image = global::SharpGMad.Properties.Resources.drop;
            this.tsmFileRemove.Name = "tsmFileRemove";
            this.tsmFileRemove.Size = new System.Drawing.Size(152, 22);
            this.tsmFileRemove.Text = "Remove";
            this.tsmFileRemove.Click += new System.EventHandler(this.tsmFileRemove_Click);
            // 
            // tsmFileExtract
            // 
            this.tsmFileExtract.Image = global::SharpGMad.Properties.Resources.extract;
            this.tsmFileExtract.Name = "tsmFileExtract";
            this.tsmFileExtract.Size = new System.Drawing.Size(152, 22);
            this.tsmFileExtract.Text = "Extract";
            this.tsmFileExtract.ToolTipText = "Save the selected file somewhere on your computer";
            this.tsmFileExtract.Click += new System.EventHandler(this.tsmFileExtract_Click);
            // 
            // tsmFileShellExec
            // 
            this.tsmFileShellExec.Image = global::SharpGMad.Properties.Resources.execute;
            this.tsmFileShellExec.Name = "tsmFileShellExec";
            this.tsmFileShellExec.Size = new System.Drawing.Size(152, 22);
            this.tsmFileShellExec.Text = "Shell execute";
            this.tsmFileShellExec.ToolTipText = "Run the selected file like it was opened in Explorer";
            this.tsmFileShellExec.Click += new System.EventHandler(this.tsmFileShellExec_Click);
            // 
            // tsmFileExportTo
            // 
            this.tsmFileExportTo.Image = global::SharpGMad.Properties.Resources.export;
            this.tsmFileExportTo.Name = "tsmFileExportTo";
            this.tsmFileExportTo.Size = new System.Drawing.Size(152, 22);
            this.tsmFileExportTo.Text = "Export to...";
            this.tsmFileExportTo.ToolTipText = "Export the selected file to somewhere on your computer and set up a realtime chan" +
    "ge-watch. Changed files will be purple.";
            this.tsmFileExportTo.Click += new System.EventHandler(this.tsmFileExportTo_Click);
            // 
            // tsmFilePull
            // 
            this.tsmFilePull.Image = global::SharpGMad.Properties.Resources.pull;
            this.tsmFilePull.Name = "tsmFilePull";
            this.tsmFilePull.Size = new System.Drawing.Size(152, 22);
            this.tsmFilePull.Text = "Update";
            this.tsmFilePull.ToolTipText = "Update this file with the changes from the exported file on your computer";
            this.tsmFilePull.Click += new System.EventHandler(this.tsmFilePull_Click);
            // 
            // tsmFileOpenExport
            // 
            this.tsmFileOpenExport.Image = global::SharpGMad.Properties.Resources.open_export;
            this.tsmFileOpenExport.Name = "tsmFileOpenExport";
            this.tsmFileOpenExport.Size = new System.Drawing.Size(152, 22);
            this.tsmFileOpenExport.Text = "Open export";
            this.tsmFileOpenExport.ToolTipText = "Opens the exported file from your filesystem";
            this.tsmFileOpenExport.Click += new System.EventHandler(this.tsmFileOpenExport_Click);
            // 
            // tsmFileDropExport
            // 
            this.tsmFileDropExport.Image = global::SharpGMad.Properties.Resources.drop_export;
            this.tsmFileDropExport.Name = "tsmFileDropExport";
            this.tsmFileDropExport.Size = new System.Drawing.Size(152, 22);
            this.tsmFileDropExport.Text = "Drop export";
            this.tsmFileDropExport.ToolTipText = "Delete the exported file from your computer";
            this.tsmFileDropExport.Click += new System.EventHandler(this.tsmFileDropExport_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(759, 468);
            this.Controls.Add(this.pnlForm);
            this.Controls.Add(this.tsToolbar);
            this.Name = "Main";
            this.Text = "SharpGMad";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.Resize += new System.EventHandler(this.Main_Resize);
            this.tsFileOperations.ResumeLayout(false);
            this.tsFileOperations.PerformLayout();
            this.pnlLeftSide.ResumeLayout(false);
            this.pnlLeftSide.PerformLayout();
            this.spcFoldersAndFiles.Panel1.ResumeLayout(false);
            this.spcFoldersAndFiles.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcFoldersAndFiles)).EndInit();
            this.spcFoldersAndFiles.ResumeLayout(false);
            this.pnlFileOpsToolbar.ResumeLayout(false);
            this.pnlFileOpsToolbar.PerformLayout();
            this.ssStatus.ResumeLayout(false);
            this.ssStatus.PerformLayout();
            this.pnlForm.ResumeLayout(false);
            this.pnlRightSide.ResumeLayout(false);
            this.pnlRightSide.PerformLayout();
            this.tsMetadata.ResumeLayout(false);
            this.tsMetadata.PerformLayout();
            this.tsToolbar.ResumeLayout(false);
            this.tsToolbar.PerformLayout();
            this.cmsFileEntry.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstFiles;
        private System.Windows.Forms.OpenFileDialog ofdAddon;
        private System.Windows.Forms.ToolStrip tsFileOperations;
        private System.Windows.Forms.Panel pnlLeftSide;
        private System.Windows.Forms.Panel pnlFileOpsToolbar;
        private System.Windows.Forms.Panel pnlForm;
        private System.Windows.Forms.Panel pnlRightSide;
        private System.Windows.Forms.ToolStrip tsToolbar;
        private System.Windows.Forms.ToolStripButton tsbCreateAddon;
        private System.Windows.Forms.ToolStripButton tsbOpenAddon;
        private System.Windows.Forms.ToolStripButton tsbSaveAddon;
        private System.Windows.Forms.ToolStripSeparator tssAddonSeparator;
        private System.Windows.Forms.ToolStripDropDownButton tsddbLegacy;
        private System.Windows.Forms.ToolStripMenuItem tsmiLegacyCreate;
        private System.Windows.Forms.ToolStripMenuItem tsmiLegacyExtract;
        private System.Windows.Forms.ToolStripButton tsbAddFile;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtMetadataTitle;
        private System.Windows.Forms.TextBox txtMetadataDescription;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.ComboBox cmbMetadataTag1;
        private System.Windows.Forms.ComboBox cmbMetadataTag2;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.ComboBox cmbMetadataType;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ToolStrip tsMetadata;
        private System.Windows.Forms.ToolStripButton tsbUpdateMetadata;
        private System.Windows.Forms.OpenFileDialog ofdAddFile;
        private System.Windows.Forms.SaveFileDialog sfdAddon;
        private System.Windows.Forms.ToolStripButton tsbPullAll;
        private System.Windows.Forms.SaveFileDialog sfdExportFile;
        private System.Windows.Forms.ToolStripSeparator tssExportHeaderSeparator;
        private System.Windows.Forms.ToolStripButton tsbDropAll;
        private System.Windows.Forms.ToolStripMenuItem tsmFileRemove;
        private System.Windows.Forms.ToolStripSeparator tssExportSeparator;
        private System.Windows.Forms.ToolStripMenuItem tsmFileExportTo;
        private System.Windows.Forms.ToolStripMenuItem tsmFilePull;
        private System.Windows.Forms.ToolStripMenuItem tsmFileDropExport;
        private System.Windows.Forms.ContextMenuStrip cmsFileEntry;
        private System.Windows.Forms.ToolStripMenuItem tsmFileExtract;
        private System.Windows.Forms.FolderBrowserDialog fbdFileExtractMulti;
        private System.Windows.Forms.ToolStripMenuItem tsmFileShellExec;
        private System.Windows.Forms.ToolStripButton tsbDiscardMetadataChanges;
        private System.Windows.Forms.StatusStrip ssStatus;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatus;
        private System.Windows.Forms.TreeView tvFolders;
        private System.Windows.Forms.SplitContainer spcFoldersAndFiles;
        private System.Windows.Forms.ToolStripDropDownButton tsddbViewOptions;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewLargeIcons;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewSmallIcons;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewDetails;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewList;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewTiles;
        private System.Windows.Forms.ToolStripSeparator tssViewSeparator;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewShowSubfolders;
        private System.Windows.Forms.ColumnHeader chFilename;
        private System.Windows.Forms.ColumnHeader chFileType;
        private System.Windows.Forms.ColumnHeader chSize;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewShowFolderTree;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewShowAllFiles;
        private System.Windows.Forms.ToolStripMenuItem tsmFileOpenExport;





    }
}