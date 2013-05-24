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

        public Main()
        {
            InitializeComponent();
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

                if (file != DialogResult.Cancel)
                {
                    try
                    {
                        addon = new Addon(new Reader(ofdAddon.FileName));
                    }
                    catch (System.IO.IOException ex)
                    {
                        MessageBox.Show("Unable to load the addon.\nAn exception happened.\n\n" + ex.Message, "Addon reading error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (ReaderException ex)
                    {
                        MessageBox.Show("There was an error parsing the file.\n\n" + ex.Message, "Addon reading error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                if (addon is Addon)
                {
                    txtTitle.Text = addon.Title;
                    txtAuthor.Text = addon.Author;
                    txtType.Text = addon.Type;
                    txtTags.Text = String.Join(", ", addon.Tags.ToArray());
                    txtDescription.Text = addon.Description;

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
            }
        }

        // Dock the txtDescription text box.
        // It gets automatically resized when the form is resized.
        Size txtDescriptionSizeDifference;
        private void Main_Load(object sender, EventArgs e)
        {
            txtDescriptionSizeDifference = new Size(this.pnlRightSide.Size.Width - this.txtDescription.Size.Width,
                this.pnlRightSide.Size.Height - this.txtDescription.Size.Height);
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            this.txtDescription.Size = new Size(this.pnlRightSide.Size.Width - txtDescriptionSizeDifference.Width,
                this.pnlRightSide.Size.Height - txtDescriptionSizeDifference.Height);
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
    }
}
