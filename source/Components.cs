using System;
using System.Drawing;
using System.Windows.Forms;

// Reusable components that encapsulate the default user interface style and functionality.
namespace Components
{
    /// <summary>
    /// AppTabPage Class
    /// </summary>
    public class AppTabPage : TabPage
    {
        /// <summary>
        /// Default TabPage configuration.
        /// </summary>
        public AppTabPage()
        {
            BackColor = Color.White;
            Padding = new Padding(3);
            Size = new Size(802, 440);
            Padding = new Padding(0);
            Margin = new Padding(0);
        }
    }

    /// <summary>
    /// AppWebBrowser Class
    /// </summary>
    public class AppWebBrowser : WebBrowser
    {
        /// <summary>
        /// Default WebBrowser configuration.
        /// </summary>
        public AppWebBrowser()
        {
            Size = new Size(802, 440);
            ScriptErrorsSuppressed = true;
            Anchor = ((
                AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)
            ));
        }
    }

    /// <summary>
    /// AppLabel Class
    /// </summary>
    public class AppLabel : Label
    {
        /// <summary>
        /// Default Label configuration.
        /// </summary>
        public AppLabel()
        {
            AutoSize = true;
            Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            ForeColor = Color.Gray;
            TabStop = false;
        }
    }

    /// <summary>
    /// AppPictureBox Class
    /// </summary>
    public class AppPictureBox : PictureBox
    {
        /// <summary>
        /// Default PictureBox configuration.
        /// </summary>
        public AppPictureBox()
        {
            Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            Cursor = Cursors.Hand;
            Size = new Size(16, 16);
            TabStop = false;
        }
    }

    /// <summary>
    /// AppDataGridView Class
    /// </summary>
    public class AppDataGridView : DataGridView
    {
        /// <summary>
        /// Default DataGridView configuration.
        /// </summary>
        public AppDataGridView()
        {
            DataGridViewCellStyle dataGridViewCellStyleColumn1 = new DataGridViewCellStyle
            {
                Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0))),
                ForeColor = Color.SteelBlue,
                SelectionBackColor = Color.White,
                SelectionForeColor = Color.SteelBlue
            };

            DataGridViewCellStyle dataGridViewCellStyleColumn2 = new DataGridViewCellStyle
            {
                Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))),
                ForeColor = Color.Black,
                SelectionBackColor = Color.White,
                SelectionForeColor = Color.Black
            };

            DataGridViewTextBoxColumn column1 = new DataGridViewTextBoxColumn
            {
                ReadOnly = true,
                Width = 120,
                DefaultCellStyle = dataGridViewCellStyleColumn1
            };

            DataGridViewTextBoxColumn column2 = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = dataGridViewCellStyleColumn2
            };

            Columns.AddRange(new DataGridViewColumn[] { column1, column2 });

            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeColumns = false;
            AllowUserToResizeRows = false;
            Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
            ColumnHeadersVisible = false;
            RowHeadersVisible = false;
            EditMode = DataGridViewEditMode.EditOnEnter;
            GridColor = SystemColors.Control;
            MultiSelect = false;
            ScrollBars = ScrollBars.None;
            SelectionMode = DataGridViewSelectionMode.CellSelect;
            CellFormatting += AppDataGridView_CellFormatting;
            TabStop = false;
            GridColor = Color.LightGray;
            BorderStyle = BorderStyle.FixedSingle;

            // Mask the input text if the field text contains the word password.
            void AppDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
            {
                if (e != null && e.ColumnIndex == 1 && this.Rows[e.RowIndex].Cells[0].Value.ToString().ToLower().Contains("password"))
                {
                    //e.Value = new String('\u25CF', e.Value.ToString().Length);
                }
            }
        }
    }

    /// <summary>
    /// PasswordDataGridView Class
    /// </summary>
    public class PasswordDataGridView : AppDataGridView
    {
        /// <summary>
        /// Display the appropriate text and fields to create, enter or change the application key password.
        /// </summary>
        /// <param name="e">PaintEventArgs</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Name == "dgvPassword")
            {
                switch (DomMan.AppForm.keyPwdLength)
                {
                    // Application key file does not exist.
                    case -1:
                        this.Enabled = false;
                        break;

                    // Application key file exists but is encrypted.
                    case 0:
                        TabPages.Password.pbKey.Image = global::DomMan.Properties.Resources.key;
                        TabPages.Password.lblPassword.Text = "Enter the password and click the key.";
                        TabPages.Password.lblPasswordCreate.Text = "Change Password";
                        DomMan.AppForm.appToolTip.SetToolTip(TabPages.Password.pbKey, "Enter Password");
                        this.Enabled = true;
                        break;

                    // Application key file exists and has been decrypted.
                    default:
                        TabPages.Password.pbKey.Image = global::DomMan.Properties.Resources.key_change;
                        TabPages.Password.lblPassword.Text = "Enter a new password and click the key to change your password.";
                        DomMan.AppForm.appToolTip.SetToolTip(TabPages.Password.pbKey, "Change Password");
                        this.Rows[0].Cells[1].Value = new String('\u25CF', DomMan.AppForm.keyPwdLength);
                        this.Enabled = false;
                        break;
                }
            }
            else if (this.Name == "dgvPasswordCreate")
            {
                if (DomMan.AppForm.keyPwdLength == 0)
                {
                    this.Enabled = false;
                }
                else
                {
                    this.Enabled = true;
                }
            }

            if (this.Enabled == false)
            {
                using (Pen pen = new Pen(Color.DarkGray, 0))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }

                this.Columns[0].DefaultCellStyle.ForeColor = Color.Silver;
                this.Columns[0].DefaultCellStyle.SelectionForeColor = Color.Silver;

                this.Columns[1].DefaultCellStyle.ForeColor = Color.Silver;
                this.Columns[1].DefaultCellStyle.SelectionForeColor = Color.Silver;
            }
            else
            {
                using (Pen pen = new Pen(Color.Black, 0))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }

                this.Columns[0].DefaultCellStyle.ForeColor = Color.SteelBlue;
                this.Columns[0].DefaultCellStyle.SelectionForeColor = Color.SteelBlue;

                this.Columns[1].DefaultCellStyle.ForeColor = Color.Black;
                this.Columns[1].DefaultCellStyle.SelectionForeColor = Color.Black;
            }
        }
    }
}