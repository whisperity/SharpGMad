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
    partial class Main : Form
    {
        Addon addon;

        public Main()
        {
            InitializeComponent();
        }

        private void LoadFile()
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
    }
}
