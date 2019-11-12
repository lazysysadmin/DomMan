namespace DomMan
{
    partial class AppForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppForm));
            this.appImageList = new System.Windows.Forms.ImageList(this.components);
            this.appContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.closeTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.appContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // appImageList
            // 
            this.appImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("appImageList.ImageStream")));
            this.appImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.appImageList.Images.SetKeyName(0, "settings.png");
            this.appImageList.Images.SetKeyName(1, "help.png");
            this.appImageList.Images.SetKeyName(2, "preview.png");
            this.appImageList.Images.SetKeyName(3, "tab.png");
            this.appImageList.Images.SetKeyName(4, "key.png");
            // 
            // appContextMenuStrip
            // 
            this.appContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeTabToolStripMenuItem});
            this.appContextMenuStrip.Name = "contextMenuStrip1";
            this.appContextMenuStrip.Size = new System.Drawing.Size(126, 26);
            this.appContextMenuStrip.Text = "Close Tab";
            // 
            // closeTabToolStripMenuItem
            // 
            this.closeTabToolStripMenuItem.Image = global::DomMan.Properties.Resources.close;
            this.closeTabToolStripMenuItem.Name = "closeTabToolStripMenuItem";
            this.closeTabToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.closeTabToolStripMenuItem.Text = "Close Tab";
            // 
            // AppForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(809, 467);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(825, 420);
            this.Name = "AppForm";
            this.Text = "DOM Manipulator";
            this.Load += new System.EventHandler(this.AppForm_Load);
            this.Shown += new System.EventHandler(this.AppForm_Shown);
            this.appContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.ImageList appImageList;
        private System.Windows.Forms.ContextMenuStrip appContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem closeTabToolStripMenuItem;
    }
}