using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpGMad
{
    partial class UpdateMetadata : Form
    {
        Addon addon;

        public UpdateMetadata(Addon a)
        {
            InitializeComponent();

            addon = a;
        }

        private void UpdateMetadata_Load(object sender, EventArgs e)
        {
            // Set up the form using the value properties

            txtTitle.Text = addon.Title;
            txtAuthor.Text = addon.Author;
            txtDescription.Text = addon.Description;

            // The comboboxes are a bit more tricky
            cmbType.Items.AddRange(Tags.Type);
            cmbType.SelectedItem = addon.Type;

            cmbTag1.Items.AddRange(Tags.Misc);
            cmbTag2.Items.AddRange(Tags.Misc);
            cmbTag1.Items.Add("(empty)");
            cmbTag2.Items.Add("(empty)");
            cmbTag1.SelectedItem = "(empty)";
            cmbTag2.SelectedItem = "(empty)";
            try
            {
                cmbTag1.SelectedItem = addon.Tags[0];
                cmbTag2.SelectedItem = addon.Tags[1];
            }
            catch (IndexOutOfRangeException)
            {
                // There are no first or second tag.
                // So NOOP.
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.Dispose();
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            if (cmbTag1.SelectedItem == cmbTag2.SelectedItem &&
                !(cmbTag1.SelectedItem == "(empty)" && cmbTag2.SelectedItem == "(empty)"))
            {
                MessageBox.Show("You selected the same tag twice!", "Update metadata",
                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            addon.Title = txtTitle.Text;
            addon.Author = txtAuthor.Text;
            addon.Description = txtDescription.Text;

            if (Tags.TypeExists(cmbType.SelectedItem.ToString()))
                addon.Type = cmbType.SelectedItem.ToString();
            else
            {
                // This should not happen in normal operation
                // nontheless we check against it
                MessageBox.Show("The selected type is invalid!", "Update metadata",
                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            if (((cmbTag1.SelectedItem != "(empty)") && !Tags.TagExists(cmbTag1.SelectedItem.ToString()))
                || ((cmbTag2.SelectedItem != "(empty)") && !Tags.TagExists(cmbTag2.SelectedItem.ToString())))
            {
                // This should not happen in normal operation
                // nontheless we check against it
                MessageBox.Show("The selected tags are invalid!", "Update metadata",
                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            addon.Tags = new List<string>(2);
            if (cmbTag1.SelectedItem != "(empty)")
                addon.Tags.Add(cmbTag1.SelectedItem.ToString());
            if (cmbTag2.SelectedItem != "(empty)")
                addon.Tags.Add(cmbTag2.SelectedItem.ToString());

            // Callback to update the metadata panel
            if (this.Owner is Main)
            {
                ((Main)this.Owner).UpdateMetadataPanel();
                ((Main)this.Owner).SetModified(true);
            }

            this.Hide();
            this.Dispose();
        }
    }
}
