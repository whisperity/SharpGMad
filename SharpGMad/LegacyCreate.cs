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
    public partial class LegacyCreate : Form
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

        private enum CreateErrorType
        {
            FileRead,
            Ignored,
            WhitelistFailure
        }

        private struct CreateError
        {
            public string Path;
            public CreateErrorType Type;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
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
                MessageBox.Show(ex.Message, "addon.json parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Addon addon = new Addon(addonInfo);

            //
            // Get a list of files in the specified folder
            //
            foreach (string f in Directory.GetFiles(txtFolder.Text, "*", SearchOption.AllDirectories))
            {
                string file = f;
                file = file.Replace(txtFolder.Text, String.Empty);
                file = file.Replace('\\', '/');

                Console.WriteLine("\t" + file);

                try
                {
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
                            "Failed to create the addon", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            MemoryStream buffer;
            try
            {
                Writer.Create(addon, out buffer);
            }
            catch (Exception be)
            {
                MessageBox.Show("An exception happened while compiling the addon.\n\n" + be.Message,
                    "Failed to create the addon", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //
            // Save the buffer to the provided name
            //
            buffer.Seek(0, SeekOrigin.Begin);
            byte[] bytes = new byte[buffer.Length];
            buffer.Read(bytes, 0, (int)buffer.Length);

            try
            {
                File.WriteAllBytes(txtFile.Text, bytes);
            }
            catch (Exception)
            {
                MessageBox.Show("Couldn't save to file " + txtFile.Text,
                    "Failed to create the addon", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //
            // Success!
            //
            if (errors.Count == 0)
            {
                MessageBox.Show("Successfully saved to " + txtFile.Text + " (" + ((int)buffer.Length).HumanReadableSize() + ")",
                    "Created successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string msgboxMessage = "Successfully saved to " + txtFile.Text + " (" + ((int)buffer.Length).HumanReadableSize() +
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

            return;
        }
    }
}
