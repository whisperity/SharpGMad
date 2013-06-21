using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SharpGMad
{
    partial class LegacyCreate : Form
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
                MessageBox.Show(ex.Message, "addon.json parse error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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
            MemoryStream buffer = new MemoryStream();
            try
            {
                Writer.Create(addon, buffer);
            }
            catch (Exception be)
            {
                MessageBox.Show("An exception happened while compiling the addon.\n\n" + be.Message,
                    "Failed to create the addon", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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
                    "Failed to create the addon", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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
