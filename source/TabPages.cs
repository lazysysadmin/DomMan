using Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace TabPages
{
    /// <summary>
    /// Help AppTabPage Class
    /// </summary>
    public class Help : AppTabPage
    {
        /// <summary>
        /// Displays the help.html file in a new TabPage.
        /// </summary>
        public Help()
        {
            Text = "Help";
            Name = "tabHelp";
            ImageIndex = 1;

            AppWebBrowser wb = new AppWebBrowser
            {
                Name = "wbHelp"
            };

            byte[] help = DomMan.Properties.Resources.Help_html;
            Stream ms = new MemoryStream(help);
            StreamReader sr = new StreamReader(ms);
            wb.DocumentText = sr.ReadToEnd();
            sr.Close();

            this.Controls.Add(wb);

            DomMan.AppForm.appTabControl.TabPages.Add(this);
            DomMan.AppForm.appTabControl.SelectedTab = this;
        }
    }

    /// <summary>
    /// Password AppTabPage Class
    /// </summary>
    public class Password : AppTabPage
    {
        /// <summary>
        /// AppDataGridView for entering the application key password.
        /// </summary>
        private readonly PasswordDataGridView dgvPassword;

        /// <summary>
        /// AppDataGridView for creating and changing the application key password.
        /// </summary>
        private readonly PasswordDataGridView dgvPasswordCreate;

        /// <summary>
        /// Displays the password status.
        /// </summary>
        public static AppLabel lblPassword;

        /// <summary>
        /// Displays either create or change password.
        /// </summary>
        public static AppLabel lblPasswordCreate;

        /// <summary>
        /// Key image with click event for creating, changing and entering the application key password.
        /// </summary>
        public static AppPictureBox pbKey;

        /// <summary>
        /// Creates the Password AppTabPage for creating, entering and changing the application key password.
        /// </summary>
        public Password()
        {
            Text = "Password";
            Name = "tabPassword";
            Location = new Point(4, 24);
            ImageIndex = 4;

            lblPassword = new AppLabel
            {
                Location = new Point(147, 32),
                Name = "lblPassword",
                AutoSize = true,
                ForeColor = Color.Gray,
                Text = "Enter a new password and click the key.",
                Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)))
            };
            this.Controls.Add(lblPassword);

            pbKey = new AppPictureBox
            {
                Image = global::DomMan.Properties.Resources.key_add,
                InitialImage = global::DomMan.Properties.Resources.key_add,
                Location = new Point(634, 34),
                Name = "pbKey"
            };

            DomMan.AppForm.appToolTip.SetToolTip(pbKey, "Create Password");

            pbKey.Click += new EventHandler(PbKey_Click);

            this.Controls.Add(pbKey);

            dgvPassword = new PasswordDataGridView
            {
                Name = "dgvPassword",
                Location = new Point(150, 57),
                Size = new Size(500, 25)
            };

            dgvPassword.Rows.Add("Password", "");

            this.Controls.Add(dgvPassword);

            lblPasswordCreate = new AppLabel
            {
                Location = new Point(147, 91),
                Name = "lblPasswordCreate",
                Text = "Create Password"
            };

            this.Controls.Add(lblPasswordCreate);

            dgvPasswordCreate = new PasswordDataGridView
            {
                Name = "dgvPasswordCreate",
                Location = new Point(150, 116),
                Size = new Size(500, 47)
            };

            dgvPasswordCreate.Rows.Add("New Password", "");
            dgvPasswordCreate.Rows.Add("Confirm Password", "");

            this.Controls.Add(dgvPasswordCreate);

            DomMan.AppForm.appTabControl.TabPages.Add(this);
            DomMan.AppForm.appTabControl.SelectedTab = this;

            // pbKey click event for creating, entering and changing the application key password.
            void PbKey_Click(object sender, EventArgs e)
            {
                string key = Directory.GetCurrentDirectory() + @"\app.key";

                DataGridViewCell pwdCell = dgvPassword.Rows[0].Cells[1];
                DataGridViewCell createCell = dgvPasswordCreate.Rows[0].Cells[1];
                DataGridViewCell confirmCell = dgvPasswordCreate.Rows[1].Cells[1];

                if (DomMan.AppForm.keyPwdLength == -1 || DomMan.AppForm.keyPwdLength > 0)
                {
                    dgvPasswordCreate.EndEdit();
                    dgvPasswordCreate.CurrentCell = null;

                    if (createCell.Value != null && confirmCell.Value != null)
                    {
                        bool create = true;
                        string pwdCreate = createCell.Value.ToString();
                        string pwdConfirm = confirmCell.Value.ToString();

                        if (pwdCreate.Length > 0 && pwdCreate == pwdConfirm)
                        {
                            if (DomMan.AppForm.keyPwdLength > 0)
                            {
                                create = false;
                                File.Delete(key);
                            }

                            string xml = DomMan.Cryptography.rsa.ToXmlString(true);

                            File.WriteAllText(key, xml);

                            DomMan.Cryptography.EncryptKey(key, pwdCreate);

                            DomMan.Cryptography.LoadKey(key, pwdCreate);

                            DomMan.AppForm.keyPwdLength = pwdCreate.Length;
                            pwdCell.Value = new String('\u25CF', DomMan.AppForm.keyPwdLength);

                            lblPassword.Text = "Enter a new password and click the key to change your password.";
                            lblPasswordCreate.Text = "Change Password";

                            pbKey.Image = global::DomMan.Properties.Resources.key_change;

                            DomMan.AppForm.appToolTip.SetToolTip(pbKey, lblPasswordCreate.Text);

                            dgvPassword.Enabled = false;
                            dgvPasswordCreate.Enabled = true;

                            if (create == true)
                            {
                                MessageBox.Show("Successfully created application key file.\n\n" + key + "\n\nIf you forget the password you will not be able decrypt encrypted files.", "Created Application Key", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Successfully changed the application key password.", "Password Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            if (pwdCreate.Length == 0)
                            {
                                MessageBox.Show("Password must contain at least one character.", "Password Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else if (pwdCreate != pwdConfirm)
                            {
                                MessageBox.Show("Passwords don't match.", "Password Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        createCell.Value = "";
                        confirmCell.Value = "";
                    }
                }
                else
                {
                    dgvPassword.EndEdit();
                    dgvPassword.CurrentCell = null;

                    if (pwdCell.Value != null)
                    {
                        string password = pwdCell.Value.ToString();
                        if (password.Length > 0)
                        {
                            string message = DomMan.Cryptography.LoadKey(key, password);
                            if (message == "key_loaded")
                            {
                                DomMan.AppForm.keyPwdLength = password.Length;

                                dgvPassword.Enabled = false;
                                dgvPasswordCreate.Enabled = true;
                            }
                            else
                            {
                                MessageBox.Show(message, "CryptographicException", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                pwdCell.Value = "";
                            }
                        }
                        else
                        {
                            MessageBox.Show("Password must contain at least one character.", "Password Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Preview AppTabPage Class
    /// </summary>
    public class Preview : AppTabPage
    {
        /// <summary>
        /// Creates the Preview AppTabPage for displaying, encrypting, decrypting and editing JavaScript source files.
        /// </summary>
        public Preview(string filePath, bool encrypted)
        {
            TextBox txtPreview = new TextBox
            {
                Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))),
                Location = new Point(0, 26),
                WordWrap = false,
                TabStop = false,
                ScrollBars = ScrollBars.Both,
                Multiline = true,
                Name = "txtScriptPreview",
                BackColor = Color.White,
                Size = new Size(802, 414),
                Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)))
            };

            if (encrypted == true)
            {
                string exception = "CryptographicException";
                txtPreview.Text = DomMan.Cryptography.DecryptFile(filePath);
                if (txtPreview.Text.StartsWith(exception))
                {
                    MessageBox.Show(txtPreview.Text.Replace(exception, "An error occured decrypting the JavaScript File.\n\n" + filePath), exception, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPreview.Dispose();
                    return;
                }
            }
            else
            {
                txtPreview.Text = File.ReadAllText(filePath);
            }

            Text = "Preview";
            Name = "tabPreview";
            ImageIndex = 2;

            ToolStripButton btnEncrypt = new ToolStripButton
            {
                Image = ((Image)(DomMan.Properties.Resources.encrypt)),
                Name = "btnEncrypt",
                Text = "Encrypt",
                Alignment = ToolStripItemAlignment.Right
            };
            btnEncrypt.Click += new EventHandler(EncryptScript);

            ToolStripButton btnSave = new ToolStripButton
            {
                Image = ((Image)(DomMan.Properties.Resources.disk)),
                Name = "btnSave",
                Text = "Save",
                Alignment = ToolStripItemAlignment.Right
            };
            btnSave.Click += new EventHandler(SaveScript);

            ToolStripLabel lblFilePath = new ToolStripLabel
            {
                Name = "lblFilePath",
                Text = Path.GetFullPath(filePath)
            };

            ToolStrip tsPreview = new ToolStrip
            {
                Name = "toolStripPreview",
                AutoSize = true,
                Location = new Point(0, 0),
                GripStyle = ToolStripGripStyle.Hidden
            };
            tsPreview.Items.AddRange(new ToolStripItem[] { lblFilePath, btnSave, btnEncrypt });

            if (encrypted == true)
            {
                MarkEncrypted();
            }

            this.Controls.Add(tsPreview);
            this.Controls.Add(txtPreview);

            DomMan.AppForm.appTabControl.TabPages.Add(this);
            DomMan.AppForm.appTabControl.SelectedTab = this;

            // Changes the text colour to indicate that the file is encrypted.
            void MarkEncrypted()
            {
                if (btnEncrypt.Text == "Encrypt")
                {
                    lblFilePath.ForeColor = Color.Navy;
                    txtPreview.ForeColor = Color.Navy;
                    btnEncrypt.Text = "Decrypt";
                }
                else
                {
                    lblFilePath.ForeColor = Color.Black;
                    txtPreview.ForeColor = Color.Black;
                    btnEncrypt.Text = "Encrypt";
                }
            }

            // Encrypts the JavaScript file specified.
            void EncryptScript(object sender, EventArgs e)
            {
                if (DomMan.AppForm.keyPwdLength <= 0)
                {
                    if (DomMan.AppForm.PromptForPassword() == true)
                    {
                        ToolStripButton button = (ToolStripButton)sender;
                        TabControl appTabControl = (TabControl)button.Owner.Parent.Parent;
                        if (appTabControl.Controls.ContainsKey("tabPassword"))
                        {
                            AppTabPage tabPassword = (AppTabPage)appTabControl.Controls["tabPassword"];
                            appTabControl.SelectedTab = tabPassword;
                            return;
                        }

                        AppTabPage tab = new TabPages.Password();
                    }
                    return;
                }

                ToolStrip toolStripPreview = (ToolStrip)Controls.Find("toolStripPreview", true)[0];
                ToolStripLabel filePathLabel = (ToolStripLabel)toolStripPreview.Items.Find("lblFilePath", true)[0];
                TextBox txtScriptPreview = (TextBox)Controls.Find("txtScriptPreview", true)[0];

                if (btnEncrypt.Text == "Encrypt")
                {
                    DomMan.Cryptography.EncryptFile(filePath, txtScriptPreview.Text);
                    filePathLabel.ForeColor = Color.Navy;
                    txtScriptPreview.ForeColor = Color.Navy;
                    btnEncrypt.Text = "Decrypt";
                }
                else
                {
                    File.WriteAllText(filePath, txtScriptPreview.Text);
                    filePathLabel.ForeColor = Color.Black;
                    txtScriptPreview.ForeColor = Color.Black;
                    btnEncrypt.Text = "Encrypt";
                }
            }

            // Saves the JavaScript displayed in the Preview to a file.
            void SaveScript(object sender, EventArgs e)
            {
                TextBox txtScriptPreview = (TextBox)Controls.Find("txtScriptPreview", true)[0];
                if (btnEncrypt.Text == "Decrypt")
                {
                    DomMan.Cryptography.EncryptFile(filePath, txtScriptPreview.Text);
                }
                else
                {
                    File.WriteAllText(filePath, txtScriptPreview.Text);
                }
            }
        }
    }

    /// <summary>
    /// Settings AppTabPage Class
    /// </summary>
    public class Settings : AppTabPage
    {
        /// <summary>
        /// The version of Internet Explorer used by the WebBrowser control.
        /// </summary>
        public static int browserVersion = -1;

        /// <summary>
        /// AppDataGridView for entering transaction information.
        /// </summary>
        public static AppDataGridView dgvSiteDetails;

        /// <summary>
        /// AppDataGridView for entering JavaScript information.
        /// </summary>
        public static AppDataGridView dgvScriptDetails;

        // Displays the JavaScript source that will be injected and invoked.
        private static TextBox txtUserAgent;

        // Provides an option to encrypt the saved transaction file.
        private static CheckBox chkEncryptSaved;

        /// <summary>
        /// ComboBox to display the version of Internet Explorer used by the WebBrowser control.
        /// </summary>
        public static ComboBox cboWebBrowser;

        /// <summary>
        /// Creates the Settings AppTabPage for entering transaction details.
        /// </summary>
        public Settings()
        {
            ImageIndex = 0;
            Location = new Point(4, 23);
            Margin = new Padding(0);
            Name = "tabSettings";
            Size = new Size(802, 440);
            TabIndex = 0;
            Text = "Settings";

            chkEncryptSaved = new CheckBox
            {
                AutoSize = true,
                Location = new Point(147, 30),
                Name = "chkEncryptSaved",
                Size = new Size(115, 17),
                Text = "Encrypt Saved File",
                UseVisualStyleBackColor = true,
                TabStop = false
            };
            chkEncryptSaved.CheckedChanged += new EventHandler(ChkEncryptSaved_CheckedChanged);

            this.Controls.Add(chkEncryptSaved);

            AppPictureBox pbHelp = new AppPictureBox
            {
                Image = global::DomMan.Properties.Resources.help,
                InitialImage = global::DomMan.Properties.Resources.help,
                Location = new Point(505, 31),
                Name = "pbHelp"
            };

            DomMan.AppForm.appToolTip.SetToolTip(pbHelp, "Help");
            pbHelp.Click += new EventHandler(PbHelp_Click);

            this.Controls.Add(pbHelp);

            AppPictureBox pbPassword = new AppPictureBox
            {
                Image = global::DomMan.Properties.Resources.key_app,
                InitialImage = global::DomMan.Properties.Resources.key_app,
                Location = new Point(527, 31),
                Name = "pbPassword"
            };

            DomMan.AppForm.appToolTip.SetToolTip(pbPassword, "Password");
            pbPassword.Click += new EventHandler(PbPassword_Click);

            this.Controls.Add(pbPassword);

            AppPictureBox pbReset = new AppPictureBox
            {
                Image = global::DomMan.Properties.Resources.reset,
                InitialImage = global::DomMan.Properties.Resources.reset,
                Location = new Point(549, 31),
                Name = "pbReset"
            };

            DomMan.AppForm.appToolTip.SetToolTip(pbReset, "Reset");
            pbReset.Click += new EventHandler(PbReset_Click);

            this.Controls.Add(pbReset);

            AppPictureBox pbOpen = new AppPictureBox
            {
                Image = global::DomMan.Properties.Resources.open,
                InitialImage = global::DomMan.Properties.Resources.open,
                Location = new Point(571, 31),
                Name = "pbOpen"
            };

            DomMan.AppForm.appToolTip.SetToolTip(pbOpen, "Open");
            pbOpen.Click += new EventHandler(PbOpen_Click);

            this.Controls.Add(pbOpen);

            AppPictureBox pbSave = new AppPictureBox
            {
                Image = global::DomMan.Properties.Resources.save,
                InitialImage = global::DomMan.Properties.Resources.save,
                Location = new Point(593, 31),
                Name = "pbSave"
            };

            DomMan.AppForm.appToolTip.SetToolTip(pbSave, "Save");
            pbSave.Click += new EventHandler(PbSave_Click);

            this.Controls.Add(pbSave);

            AppPictureBox pbPreview = new AppPictureBox
            {
                Image = global::DomMan.Properties.Resources.preview,
                InitialImage = global::DomMan.Properties.Resources.preview,
                Location = new Point(615, 31),
                Name = "pbPreview"
            };

            DomMan.AppForm.appToolTip.SetToolTip(pbPreview, "Preview");
            pbPreview.Click += new EventHandler(PbPreview_Click);

            this.Controls.Add(pbPreview);

            AppPictureBox pbRun = new AppPictureBox
            {
                Image = global::DomMan.Properties.Resources.run,
                InitialImage = global::DomMan.Properties.Resources.run,
                Location = new Point(637, 31),
                Name = "pbRun"
            };

            DomMan.AppForm.appToolTip.SetToolTip(pbRun, "Run");
            pbRun.Click += new EventHandler(PbRun_Click);

            this.Controls.Add(pbRun);

            dgvSiteDetails = new AppDataGridView
            {
                Name = "dgvSiteDetails",
                Location = new Point(147, 53),
                Size = new Size(506, 91)
            };

            dgvSiteDetails.Rows.Add("* Name", "");
            dgvSiteDetails.Rows.Add("* Location", "");
            dgvSiteDetails.Rows.Add("Username", "");
            dgvSiteDetails.Rows.Add("Password", "");

            this.Controls.Add(dgvSiteDetails);

            AppLabel lblJavaScript = new AppLabel
            {
                Location = new Point(144, 153),
                Name = "lblJavaScript",
                Text = "JavaScript"
            };

            this.Controls.Add(lblJavaScript);

            dgvScriptDetails = new AppDataGridView
            {
                Name = "dgvScriptDetails",
                Location = new Point(147, 178),
                Size = new Size(506, 47)
            };
            dgvScriptDetails.CellClick += DgvScriptDetails_CellClick;

            dgvScriptDetails.Rows.Add("+ File ", "");
            dgvScriptDetails.Rows.Add("Arguments", "");

            this.Controls.Add(dgvScriptDetails);

            AppLabel lblWebBrowser = new AppLabel
            {
                Location = new Point(144, 234),
                Name = "lblWebBrowser",
                Text = "Web Browser"
            };

            this.Controls.Add(lblWebBrowser);

            cboWebBrowser = new ComboBox
            {
                Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right))),
                BackColor = Color.White,
                CausesValidation = false,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FormattingEnabled = true,
                Location = new Point(147, 259),
                Name = "cboWebBrowser",
                Size = new Size(506, 21),
                TabStop = false,
                DisplayMember = "Text",
                ValueMember = "Value"
            };
            cboWebBrowser.SelectedIndexChanged += new EventHandler(CboWebBrowser_SelectedIndexChanged);

            // List the available versions of Internet Explorer that the WebBrowser control can use.
            List<Object> wbItems = new List<Object>();
            int wbVersion = GetBrowserVersion();
            for (int i = wbVersion; i >= 7; i--)
            {
                switch (i)
                {
                    case 11:
                        wbItems.Add(new { Text = "Internet Explorer 11 (11001)", Value = 11001 });
                        wbItems.Add(new { Text = "Internet Explorer 11 (11000)", Value = 11000 });
                        break;

                    case 10:
                        wbItems.Add(new { Text = "Internet Explorer 10 (10001)", Value = 10001 });
                        wbItems.Add(new { Text = "Internet Explorer 10 (10000)", Value = 10000 });
                        break;

                    case 9:
                        wbItems.Add(new { Text = "Internet Explorer 9 (9999)", Value = 9999 });
                        wbItems.Add(new { Text = "Internet Explorer 9 (9000)", Value = 9000 });
                        break;

                    case 8:
                        wbItems.Add(new { Text = "Internet Explorer 8 (8888)", Value = 8888 });
                        wbItems.Add(new { Text = "Internet Explorer 8 (8000)", Value = 8000 });
                        break;

                    default:
                        wbItems.Add(new { Text = "Internet Explorer 7 (7000)", Value = 7000 });
                        break;
                }
            }

            cboWebBrowser.DataSource = wbItems;

            this.Controls.Add(cboWebBrowser);

            txtUserAgent = new TextBox
            {
                Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right))),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Location = new Point(147, 287),
                Margin = new Padding(0),
                Multiline = true,
                Name = "txtUserAgent",
                ReadOnly = true,
                Size = new Size(506, 33),
                TabStop = false
            };

            this.Controls.Add(txtUserAgent);

            // Provides an open file dialog for selecting a JavaScript file.
            void DgvScriptDetails_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex == 0 && e.ColumnIndex == 0)
                {
                    OpenFileDialog openJsDialog = new OpenFileDialog
                    {
                        Filter = "JavaScript Files (*.js)|*.js|All files (*.*)|*.*",
                        RestoreDirectory = true,
                        ShowReadOnly = true,
                        SupportMultiDottedExtensions = true
                    };

                    if (openJsDialog.ShowDialog() == DialogResult.OK)
                    {
                        dgvScriptDetails.Rows[0].Cells[1].Value = openJsDialog.FileName;
                    }
                }
            }

            // pbHelp click event to display the Help AppTabPage.
            void PbHelp_Click(object sender, EventArgs e)
            {
                if (DomMan.AppForm.appTabControl.Controls.ContainsKey("tabHelp"))
                {
                    DomMan.AppForm.appTabControl.SelectedTab = (TabPage)DomMan.AppForm.appTabControl.Controls["tabHelp"];
                    return;
                }

                AppTabPage tab = new TabPages.Help();
            }

            // pbPassword click event to display the Password AppTabPage.
            void PbPassword_Click(object sender, EventArgs e)
            {
                CreatePasswordTab();
            }

            // pbReset click event to reset the settings back to default values and close the Preview AppTabPage.
            void PbReset_Click(object sender, EventArgs e)
            {
                ResetSettings();
            }

            // pbOpen click event to open a saved transaction file.
            void PbOpen_Click(object sender, EventArgs e)
            {
                OpenFileDialog openDomDialog = new OpenFileDialog
                {
                    Filter = "DOM Files (*.dom)|*.dom|All files (*.*)|*.*",
                    RestoreDirectory = true,
                    ShowReadOnly = true,
                    SupportMultiDottedExtensions = true
                };

                if (openDomDialog.ShowDialog() == DialogResult.OK)
                {
                    ResetSettings();

                    string filePath = openDomDialog.FileName;
                    string xml;

                    if (DomMan.Cryptography.IsEncrypted(filePath))
                    {
                        if (DomMan.AppForm.keyPwdLength <= 0)
                        {
                            if (DomMan.AppForm.PromptForPassword() == true)
                            {
                                CreatePasswordTab();
                            }
                            return;
                        }

                        xml = DomMan.Cryptography.DecryptFile(filePath);

                        string exception = "CryptographicException";
                        if (xml.StartsWith(exception))
                        {
                            MessageBox.Show("An error occured decrypting the DOM File.\n\n" + filePath + xml.Replace(exception, String.Empty), exception, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        chkEncryptSaved.Checked = true;
                    }
                    else
                    {
                        xml = File.ReadAllText(filePath);
                        chkEncryptSaved.Checked = false;
                    }

                    XmlDocument xdoc = new XmlDocument();
                    try
                    {
                        xdoc.LoadXml(xml);
                    }
                    catch (XmlException x)
                    {
                        MessageBox.Show("Error loading configuration file " + filePath + "\n\n" + x.Message, "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    dgvSiteDetails.Rows[0].Cells[1].Value = "";
                    XmlNodeList name = xdoc.GetElementsByTagName("name");
                    if (name != null && name.Count > 0)
                    {
                        dgvSiteDetails.Rows[0].Cells[1].Value = XmlConvert.DecodeName(name[0].InnerText);
                    }

                    dgvSiteDetails.Rows[1].Cells[1].Value = "";
                    XmlNodeList location = xdoc.GetElementsByTagName("location");
                    if (location != null && location.Count > 0)
                    {
                        dgvSiteDetails.Rows[1].Cells[1].Value = XmlConvert.DecodeName(location[0].InnerText);
                    }

                    dgvSiteDetails.Rows[2].Cells[1].Value = "";
                    XmlNodeList username = xdoc.GetElementsByTagName("username");
                    if (username != null && username.Count > 0)
                    {
                        dgvSiteDetails.Rows[2].Cells[1].Value = XmlConvert.DecodeName(username[0].InnerText);
                    }

                    dgvSiteDetails.Rows[3].Cells[1].Value = "";
                    XmlNodeList password = xdoc.GetElementsByTagName("password");
                    if (password != null && password.Count > 0)
                    {
                        dgvSiteDetails.Rows[3].Cells[1].Value = XmlConvert.DecodeName(password[0].InnerText);
                    }

                    dgvScriptDetails.Rows[0].Cells[1].Value = "";
                    XmlNodeList file = xdoc.GetElementsByTagName("file");
                    if (file != null && file.Count > 0)
                    {
                        dgvScriptDetails.Rows[0].Cells[1].Value = XmlConvert.DecodeName(file[0].InnerText);
                    }

                    dgvScriptDetails.Rows[1].Cells[1].Value = "";
                    XmlNodeList arguments = xdoc.GetElementsByTagName("arguments");
                    if (arguments != null && arguments.Count > 0)
                    {
                        dgvScriptDetails.Rows[1].Cells[1].Value = XmlConvert.DecodeName(arguments[0].InnerText);
                    }
                }
            }

            // pbSave click event to save a transaction file.
            void PbSave_Click(object sender, EventArgs e)
            {
                dgvSiteDetails.EndEdit();
                dgvSiteDetails.CurrentCell = null;

                dgvScriptDetails.EndEdit();
                dgvSiteDetails.CurrentCell = null;

                SaveFileDialog saveDomDialog = new SaveFileDialog
                {
                    Filter = "DOM Files (*.dom)|*.dom|All files (*.*)|*.*"
                };

                if (saveDomDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveDomDialog.FileName;
                    string xml = "<settings>";

                    // Remove any non-alphanumeric characters from the field string.
                    Regex regex = new Regex(@"[^a-zA-Z]");
                    foreach (DataGridViewRow row in dgvSiteDetails.Rows)
                    {
                        string field = regex.Replace(row.Cells[0].Value.ToString(), String.Empty).ToLower();
                        DataGridViewCell dgvCell = row.Cells[1];
                        if (dgvCell.Value != null && dgvCell.Value.ToString().Length > 0)
                        {
                            xml += "<" + field + ">" + XmlConvert.EncodeName(dgvCell.Value.ToString()) + "</" + field + ">";
                        }
                    }

                    foreach (DataGridViewRow row in dgvScriptDetails.Rows)
                    {
                        string field = regex.Replace(row.Cells[0].Value.ToString(), String.Empty).ToLower();
                        DataGridViewCell dgvCell = row.Cells[1];
                        if (dgvCell.Value != null && dgvCell.Value.ToString().Length > 0)
                        {
                            xml += "<" + field + ">" + XmlConvert.EncodeName(dgvCell.Value.ToString()) + "</" + field + ">";
                        }
                    }

                    xml += "</settings>";

                    if (chkEncryptSaved.Checked)
                    {
                        DomMan.Cryptography.EncryptFile(filePath, xml);
                    }
                    else
                    {
                        File.WriteAllText(filePath, xml);
                    }
                }
            }

            // pbPreview click event to preview the JavaScript source that will be injected and invoked.
            void PbPreview_Click(object sender, EventArgs e)
            {
                if (DomMan.AppForm.appTabControl.Controls.ContainsKey("tabPreview"))
                {
                    DomMan.AppForm.appTabControl.SelectedTab = (TabPage)DomMan.AppForm.appTabControl.Controls["tabPreview"];
                    return;
                }

                dgvScriptDetails.EndEdit();
                dgvScriptDetails.CurrentCell = null;

                string filePath = dgvScriptDetails.Rows[0].Cells[1].Value.ToString();

                if (!File.Exists(filePath))
                {
                    MessageBox.Show("JavaScript file not found, nothing to preview.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    bool encrypted = DomMan.Cryptography.IsEncrypted(filePath);
                    if (DomMan.AppForm.keyPwdLength <= 0 && encrypted == true)
                    {
                        if (DomMan.AppForm.PromptForPassword() == true)
                        {
                            CreatePasswordTab();
                        }
                        return;
                    }

                    AppTabPage tab = new TabPages.Preview(filePath, encrypted);
                    if (tab.Name == null || tab.Name.Length == 0)
                    {
                        tab.Dispose();
                    }
                }
            }

            // pbRun click event to execute the transaction.
            void PbRun_Click(object sender, EventArgs e)
            {
                dgvSiteDetails.EndEdit();
                dgvSiteDetails.CurrentCell = null;

                dgvScriptDetails.EndEdit();
                dgvScriptDetails.CurrentCell = null;

                string validate = ValidateSettings();
                if (validate != "valid")
                {
                    MessageBox.Show(validate, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string filePath = dgvScriptDetails.Rows[0].Cells[1].Value.ToString();
                if (filePath.Length > 0)
                {
                    if (File.Exists(filePath))
                    {
                        if (DomMan.AppForm.keyPwdLength <= 0 && DomMan.Cryptography.IsEncrypted(filePath) == true)
                        {
                            if (DomMan.AppForm.PromptForPassword() == true)
                            {
                                CreatePasswordTab();
                            }
                            return;
                        }
                    }
                }

                string tabName = "tab" + dgvSiteDetails.Rows[0].Cells[1].Value.ToString();
                if (DomMan.AppForm.appTabControl.Controls.ContainsKey(tabName))
                {
                    DomMan.AppForm.appTabControl.SelectedTab = (TabPage)DomMan.AppForm.appTabControl.Controls[tabName];
                    return;
                }

                AppTabPage tab = new TabPages.WebBrowser();
                if (tab.Name == null || tab.Name.Length == 0)
                {
                    tab.Dispose();
                }
            }

            // chkEncryptSaved CheckedChanged event to check that the application key has been decrypted prior to encrypting the transaction file.
            void ChkEncryptSaved_CheckedChanged(object sender, EventArgs e)
            {
                CheckBox checkbox = (CheckBox)sender;
                if (checkbox.Checked == true && DomMan.AppForm.keyPwdLength <= 0)
                {
                    checkbox.Checked = false;
                    if (DomMan.AppForm.PromptForPassword() == true)
                    {
                        CreatePasswordTab();
                    }
                }
            }

            // cboWebBrowser SelectedIndexChanged event for setting the version of Internet Explorer used by the WebBrowser control.
            void CboWebBrowser_SelectedIndexChanged(object sender, EventArgs e)
            {
                ComboBox comboBox = (ComboBox)sender;
                if (browserVersion > -1)
                {
                    if (MessageBox.Show("Changing the web browser will restart the application.", "Application Restart", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true))
                        {
                            key.SetValue(Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location), (int)comboBox.SelectedValue, Microsoft.Win32.RegistryValueKind.DWord);
                        }

                        Application.Restart();
                    }
                    else
                    {
                        for (int i = 0; i < cboWebBrowser.Items.Count; i++)
                        {
                            if (cboWebBrowser.Items[i].ToString().Contains(browserVersion.ToString()))
                            {
                                browserVersion = -1;
                                cboWebBrowser.SelectedItem = cboWebBrowser.Items[i];
                            }
                        }
                    }
                }
                else
                {
                    string strKeyPath = @"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
                    object objVal = Microsoft.Win32.Registry.GetValue(strKeyPath, Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location), "7000");
                    string strKeyVal = Convert.ToString(objVal);

                    for (int i = 0; i < cboWebBrowser.Items.Count; i++)
                    {
                        if (cboWebBrowser.Items[i].ToString().Contains(strKeyVal))
                        {
                            cboWebBrowser.SelectedItem = cboWebBrowser.Items[i];
                        }
                    }

                    GetUserAgent();

                    browserVersion = Int32.Parse(strKeyVal);
                }
            }

            // WebBrowserDocumentCompletedEventHandler triggered on page load to retrieve the userAgent used by the WebBrowser control.
            void UserAgent(object sender, WebBrowserDocumentCompletedEventArgs e)
            {
                AppWebBrowser wb = (AppWebBrowser)sender;

                HtmlDocument doc = wb.Document;
                HtmlElement el = doc.CreateElement("div");
                el.SetAttribute("id", "userAgent");
                doc.Body.AppendChild(el);

                HtmlElement script = doc.CreateElement("script");
                script.SetAttribute("id", "userAgentScript");
                script.SetAttribute("text", "function userAgentScript() { document.getElementById('userAgent').innerHTML = navigator.userAgent; }");
                doc.Body.AppendChild(script);

                wb.Document.InvokeScript("userAgentScript");
                txtUserAgent.Text = doc.GetElementById("userAgent").InnerHtml;

                wb.Dispose();
            }

            // Create a WebBrowser instance to retrieve the userAgent used by the WebBrowser control.
            void GetUserAgent()
            {
                AppWebBrowser wb = new AppWebBrowser
                {
                    Url = new Uri("about:blank"),
                    Visible = false,
                    ScriptErrorsSuppressed = true
                };

                wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(UserAgent);
            }

            // Populate the cboWebBrowser control with the available versions of Internet Explorer.
            int GetBrowserVersion()
            {
                string strKeyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer";
                string[] ls = new string[] { "svcVersion", "svcUpdateVersion", "Version", "W2kVersion" };

                int maxVer = 0;
                for (int i = 0; i < ls.Length; ++i)
                {
                    object objVal = Microsoft.Win32.Registry.GetValue(strKeyPath, ls[i], "0");
                    string strVal = Convert.ToString(objVal);
                    if (strVal != null)
                    {
                        int iPos = strVal.IndexOf('.');
                        if (iPos > 0)
                        {
                            strVal = strVal.Substring(0, iPos);
                            if (int.TryParse(strVal, out int res))
                            {
                                maxVer = Math.Max(maxVer, res);
                            }
                        }
                    }
                }
                return maxVer;
            }

            // Creates the Password AppTabPage for creating, entering and changing the application key password.
            void CreatePasswordTab()
            {
                if (DomMan.AppForm.appTabControl.Controls.ContainsKey("tabPassword"))
                {
                    DomMan.AppForm.appTabControl.SelectedTab = (TabPage)DomMan.AppForm.appTabControl.Controls["tabPassword"];
                    return;
                }

                AppTabPage tab = new TabPages.Password();
            }

            // Validates the transaction settings. Return the error message or the string valid if there are no errors.
            string ValidateSettings()
            {
                DataGridViewCell cellName = dgvSiteDetails.Rows[0].Cells[1];
                DataGridViewCell cellLocation = dgvSiteDetails.Rows[1].Cells[1];
                DataGridViewCell cellFile = dgvScriptDetails.Rows[0].Cells[1];

                if (cellName.Value == null) cellName.Value = "";
                if (cellLocation.Value == null) cellLocation.Value = "";
                if (cellFile.Value == null) cellFile.Value = "";

                if (cellName.Value.ToString().Length == 0 && cellLocation.Value.ToString().Length == 0)
                {
                    return "Error: The name and location fields are required.";
                }
                else if (cellName.Value.ToString().Length == 0)
                {
                    return "Error: The name field is required.";
                }
                else if (cellLocation.Value.ToString().Length == 0)
                {
                    return "Error: The location field is required.";
                }
                else if (cellFile.Value.ToString().Length > 0)
                {
                    if (!File.Exists(cellFile.Value.ToString()))
                    {
                        return "Error: JavaScript file not found.";
                    }
                    else
                    {
                        return "valid";
                    }
                }
                else
                {
                    return "valid";
                }
            }

            // Reset the settings back to default values and close the Preview AppTabPage.
            void ResetSettings()
            {
                dgvSiteDetails.EndEdit();
                dgvSiteDetails.CurrentCell = null;

                dgvScriptDetails.EndEdit();
                dgvScriptDetails.CurrentCell = null;

                chkEncryptSaved.Checked = false;

                if (DomMan.AppForm.appTabControl.Controls.ContainsKey("tabPreview"))
                {
                    DomMan.AppForm.appTabControl.TabPages.Remove((TabPage)DomMan.AppForm.appTabControl.Controls["tabPreview"]);
                }

                foreach (DataGridViewRow row in dgvSiteDetails.Rows)
                {
                    row.Cells[1].Value = "";
                }

                foreach (DataGridViewRow row in dgvScriptDetails.Rows)
                {
                    row.Cells[1].Value = "";
                }
            }
        }
    }

    /// <summary>
    /// WebBrowser AppTabPage Class
    /// </summary>
    public class WebBrowser : AppTabPage
    {
        /// <summary>
        /// Creates a WebBrowser AppTabPage to execute the transaction.
        /// </summary>
        public WebBrowser()
        {
            string name = Settings.dgvSiteDetails.Rows[0].Cells[1].Value.ToString();
            string jsCode = "";
            DataGridViewCell file = Settings.dgvScriptDetails.Rows[0].Cells[1];
            string filePath = file.Value.ToString();
            if (filePath.Length > 0)
            {
                if (DomMan.AppForm.appTabControl.Controls.ContainsKey("tabPreview"))
                {
                    AppTabPage tabPreview = (AppTabPage)DomMan.AppForm.appTabControl.Controls["tabPreview"];
                    TextBox txtScriptPreview = (TextBox)tabPreview.Controls["txtScriptPreview"];
                    jsCode = txtScriptPreview.Text;
                }
                else
                {
                    if (DomMan.Cryptography.IsEncrypted(filePath) == true)
                    {
                        string exception = "CryptographicException";
                        jsCode = DomMan.Cryptography.DecryptFile(filePath);
                        if (jsCode.StartsWith(exception))
                        {
                            MessageBox.Show(jsCode.Replace(exception, "An error occured decrypting the JavaScript File.\n\n" + filePath), exception, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        jsCode = File.ReadAllText(filePath);
                    }
                }
            }

            Text = name;
            Name = "tab" + name;
            ImageIndex = 3;

            int counter = -1;
            string postBack = "";
            string args = Settings.dgvScriptDetails.Rows[1].Cells[1].Value.ToString();

            AppWebBrowser wb = new AppWebBrowser
            {
                Name = "wb" + name,
                Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right))),
                ScriptErrorsSuppressed = true
            };
            wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(DomLoad);
            wb.Navigating += new WebBrowserNavigatingEventHandler(DomNavigate);

            string location = Settings.dgvSiteDetails.Rows[1].Cells[1].Value.ToString();
            if (File.Exists(location))
            {
                location = "file:///" + Path.GetFullPath(location).Replace(@"\", "/");
            }

            string username = Settings.dgvSiteDetails.Rows[2].Cells[1].Value.ToString();
            string password = Settings.dgvSiteDetails.Rows[3].Cells[1].Value.ToString();
            if (username.Length > 0 && password.Length > 0)
            {
                string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
                string headers = "Authorization: Basic " + auth + "\r\n";
                wb.Navigate(location, "", null, headers);
            }
            else
            {
                wb.Navigate(location, "", null, "");
            }

            this.Controls.Add(wb);

            DomMan.AppForm.appTabControl.TabPages.Add(this);
            DomMan.AppForm.appTabControl.SelectedTab = this;

            // WebBrowserDocumentCompletedEventHandler to inject and invoke the JavaScript source when the page loads.
            void DomLoad(object sender, WebBrowserDocumentCompletedEventArgs e)
            {
                AppWebBrowser domLoad = (AppWebBrowser)sender;
                if (domLoad.ReadyState != WebBrowserReadyState.Complete)
                {
                    return;
                }

                if (e.Url.AbsolutePath != domLoad.Url.AbsolutePath)
                {
                    return;
                }

                if (domLoad.Document.GetElementById("WebBrowserInvokeScript") == null)
                {
                    HtmlDocument doc = domLoad.Document;
                    HtmlElement head = doc.GetElementsByTagName("head")[0];
                    HtmlElement script = doc.CreateElement("script");
                    script.SetAttribute("id", "WebBrowserInvokeScript");
                    script.SetAttribute("text", "function WebBrowserInvokeScript(args) { " + jsCode + " }");
                    head.AppendChild(script);

                    HtmlElement elPost = doc.GetElementById("WebBrowserPostBack");
                    if (elPost == null)
                    {
                        elPost = doc.CreateElement("textarea");
                        elPost.SetAttribute("id", "WebBrowserPostBack");
                        elPost.InnerText = postBack;
                        domLoad.Document.Body.AppendChild(elPost);
                    }
                    elPost.Style = "display:none;";

                    counter += 1;
                    string scriptArgs;
                    if (args.Length > 0)
                    {
                        scriptArgs = counter.ToString() + "," + args;
                    }
                    else
                    {
                        scriptArgs = counter.ToString();
                    }

                    Object[] objArray = new Object[1];
                    objArray[0] = (Object)scriptArgs;

                    domLoad.Document.InvokeScript("WebBrowserInvokeScript", objArray);
                }
            }

            // WebBrowserNavigatingEventHandler to pass data and write local text files on navigate.
            void DomNavigate(object sender, WebBrowserNavigatingEventArgs e)
            {
                AppWebBrowser domNavigate = (AppWebBrowser)sender;
                if (domNavigate.Parent != null)
                {
                    HtmlDocument doc = domNavigate.Document;
                    HtmlElement elFile = doc.GetElementById("WebBrowserFileWriter");
                    if (elFile != null)
                    {
                        string path = elFile.GetAttribute("path");
                        if (path != null)
                        {
                            File.AppendAllText(path, elFile.InnerText + "\r\n");
                        }
                        elFile.OuterHtml = "";
                    }

                    HtmlElement elPost = doc.GetElementById("WebBrowserPostBack");
                    if (elPost != null)
                    {
                        postBack += elPost.InnerText;
                        elPost.OuterHtml = "";
                    }
                }
            }
        }
    }
}