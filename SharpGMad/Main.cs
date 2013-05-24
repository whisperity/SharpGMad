using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SharpGMad
{
    partial class Main : Form
    {
        Addon addon;

        private Main()
        {
            InitializeComponent();

            string filter = String.Empty;
            foreach (KeyValuePair<string, string[]> filetype in Whitelist.WildcardFileTypes)
            {
                filter += filetype.Key + "|" + String.Join(";", filetype.Value) + "|";
            }
            filter += "All files|*.*";

            ofdAddFile.Filter = filter;
            ofdAddFile.FilterIndex = Whitelist.WildcardFileTypes.Count + 1;
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
                DialogResult file = ofdAddon.ShowDialog();

                if (file == DialogResult.OK)
                    LoadAddon(ofdAddon.FileName);
            }
        }

        private void LoadAddon(string path)
        {
            try
            {
                addon = new Addon(new Reader(path));
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
                this.Text = "[" + Path.GetFileName(path) + "] - SharpGMad";

                txtTitle.Text = addon.Title;
                txtAuthor.Text = addon.Author;
                txtType.Text = addon.Type;
                txtTags.Text = String.Join(", ", addon.Tags.ToArray());
                txtDescription.Text = addon.Description;

                UpdateFileList();

                tsbAddFile.Enabled = true;
            }
        }

        private void UpdateFileList()
        {
            // Put the files into the list
            lstFiles.Items.Clear();
            lstFiles.Groups.Clear();

            IEnumerable<IGrouping<string, ContentFile>> folders =
                addon.Files.GroupBy(f => Path.GetDirectoryName(f.Path));
            foreach (IGrouping<string, ContentFile> folder in folders)
            {
                lstFiles.Groups.Add(folder.Key, folder.Key);
            }

            foreach (ContentFile cfile in addon.Files)
            {
                lstFiles.Items.Add(new ListViewItem(Path.GetFileName(cfile.Path),
                    lstFiles.Groups[Path.GetDirectoryName(cfile.Path)]));
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

            DialogResult result = ofdAddFile.ShowDialog();
            if ( result != DialogResult.Cancel )
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

                UpdateFileList();
                ofdAddFile.Reset();
            }
        }
    }
}
