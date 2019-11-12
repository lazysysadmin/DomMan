using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace DomMan
{
    /// <summary>
    /// The application user interface.
    /// </summary>
    public partial class AppForm : Form
    {
        /// <summary>
        /// Stores the application key password length for checking if the key exists and has been loaded.
        /// It is also used to fill the password input.
        /// </summary>
        public static int keyPwdLength = -1;

        /// <summary>
        /// TabControl variable.
        /// </summary>
        public static TabControl appTabControl;

        /// <summary>
        /// Instantiate the ToolTip class.
        /// </summary>
        public static ToolTip appToolTip = new ToolTip();

        /// <summary>
        /// Creates the application TabControl and Settings TabPage.
        /// </summary>
        public AppForm()
        {
            InitializeComponent();

            // Instantiate the RSACryptoServiceProvider class.
            Cryptography.rsa = new RSACryptoServiceProvider(Cryptography.cspp)
            {
                PersistKeyInCsp = false
            };

            // Instantiate the application TabControl.
            appTabControl = new TabControl
            {
                Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right))),
                ImageList = this.appImageList,
                Location = new Point(0, 0),
                Margin = new Padding(0),
                Name = "appTabControl",
                SelectedIndex = 0,
                Size = new Size(810, 467),
                TabIndex = 0
            };

            // Attach a MouseClick event handler to provide a ContextMenuStrip for closing the tab page.
            appTabControl.MouseClick += TabControl_MouseClick;

            // Instantiate TabPages.Settings and add it to the TabControl.
            TabPage tab = new TabPages.Settings();
            appTabControl.TabPages.Add(tab);
            appTabControl.SelectedTab = tab;

            Controls.Add(appTabControl);

            // ContextMenuStrip Click event handler to dispose of any WebBrowser instances within a tab page before it's removed.
            appContextMenuStrip.Click += ContextMenuStrip_Click;
        }

        /// <summary>
        /// Displays a prompt to create an application key password or to enter the password.
        /// </summary>
        public static bool PromptForPassword()
        {
            string key = Directory.GetCurrentDirectory() + @"\app.key";
            if (!File.Exists(key))
            {
                // Display prompt to create the application key.
                if (MessageBox.Show("To encrypt and decrypt files a password protected application key must be created. Would you like to create the application key now?", "Create Application Key", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    return true;
                }
                return false;
            }
            else
            {
                // Display a prompt to enter the application key password.
                if (keyPwdLength == 0 && MessageBox.Show("To encrypt and decrypt files you must enter the application key password. Would you like to enter the application key password now?", "Password Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    return true;
                }
                // Set keyPwdLength to 0 as the application key exists.
                keyPwdLength = 0;
                return false;
            }
        }

        /// <summary>
        /// Form load event to trigger the WebBrowser selection combo box SelectedIndexChanged event.
        /// </summary>
        protected void AppForm_Load(object sender, EventArgs e)
        {
            TabPages.Settings.cboWebBrowser.SelectedIndex = -1;
        }

        /// <summary>
        /// AppForm_Shown event handler.
        /// Prompts the user to create an application key file and password if not found.
        /// Instantiates the TabPages.Password class and adds it to the TabControl.
        /// </summary>
        /// <param name="sender">Form</param>
        /// <param name="e">EventArgs</param>
        protected void AppForm_Shown(object sender, EventArgs e)
        {
            if (PromptForPassword() == true)
            {
                // Check if the tab page already exists.
                if (appTabControl.Controls.ContainsKey("tabPassword"))
                {
                    appTabControl.SelectedTab = (TabPage)appTabControl.Controls["tabPassword"];
                    return;
                }

                // Create a new Password tab page and add it the TabControl.
                TabPage tab = new TabPages.Password();
            }
        }

        /// <summary>
        /// ContextMenuStrip_Click event handler for closing TabPages.
        /// Disposes any WebBrowser instances within the tab page and removes it from the TabControl.
        /// </summary>
        /// <param name="sender">ContextMenuStrip</param>
        /// <param name="e">EventArgs</param>
        protected void ContextMenuStrip_Click(object sender, EventArgs e)
        {
            TabPage tab = (TabPage)appContextMenuStrip.Tag;
            if (tab.Controls.ContainsKey("wb" + tab.Text))
            {
                WebBrowser wb = (WebBrowser)tab.Controls["wb" + tab.Text];
                wb.Navigate("about:blank", "", null, "");
                wb.Dispose();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            appTabControl.TabPages.Remove(tab);
        }

        /// <summary>
        /// TabControl_MouseClick event handler for displaying the ContextMenuStrip.
        /// </summary>
        /// <param name="sender">TabControl</param>
        /// <param name="e">EventArgs</param>
        protected void TabControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Iterate through the tab pages
                for (int i = 1; i < appTabControl.TabCount; i++)
                {
                    // Get the rectangle area and check if it contains the mouse cursor.
                    Rectangle r = appTabControl.GetTabRect(i);
                    if (r.Contains(e.Location))
                    {
                        appContextMenuStrip.Tag = appTabControl.TabPages[i];
                        appContextMenuStrip.Show(appTabControl, e.Location);
                        break;
                    }
                }
            }
        }
    }
}