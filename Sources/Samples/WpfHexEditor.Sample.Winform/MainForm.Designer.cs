namespace WpfHexEditor.Winform.Sample
{
    partial class MainForm
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.elementHost = new System.Windows.Forms.Integration.ElementHost();
            this.hexEditor = new WpfHexaEditor.HexEditor();
            this.toolStripBar1 = new System.Windows.Forms.ToolStrip();
            this.OpenFileButton = new System.Windows.Forms.ToolStripButton();
            this.OpenTBLButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStripBar1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.elementHost);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(868, 412);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(868, 437);
            this.toolStripContainer1.TabIndex = 4;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStripBar1);
            // 
            // elementHost
            // 
            this.elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost.Location = new System.Drawing.Point(0, 0);
            this.elementHost.Name = "elementHost";
            this.elementHost.Size = new System.Drawing.Size(868, 412);
            this.elementHost.TabIndex = 3;
            this.elementHost.Text = "elementHost";
            this.elementHost.Child = this.hexEditor;
            // 
            // toolStripBar1
            // 
            this.toolStripBar1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripBar1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripBar1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenFileButton,
            this.OpenTBLButton});
            this.toolStripBar1.Location = new System.Drawing.Point(3, 0);
            this.toolStripBar1.Name = "toolStripBar1";
            this.toolStripBar1.Size = new System.Drawing.Size(159, 25);
            this.toolStripBar1.TabIndex = 0;
            // 
            // OpenFileButton
            // 
            this.OpenFileButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.OpenFileButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenFileButton.Name = "OpenFileButton";
            this.OpenFileButton.Size = new System.Drawing.Size(59, 22);
            this.OpenFileButton.Text = "Open file";
            this.OpenFileButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.OpenFileButton.Click += new System.EventHandler(this.OpenFileButton_Click);
            // 
            // OpenTBLButton
            // 
            this.OpenTBLButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.OpenTBLButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenTBLButton.Name = "OpenTBLButton";
            this.OpenTBLButton.Size = new System.Drawing.Size(88, 22);
            this.OpenTBLButton.Text = "Import TBL file";
            this.OpenTBLButton.Click += new System.EventHandler(this.OpenTBLButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(868, 437);
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "MainForm";
            this.Text = "Wpf HexEditor WinForm sample";
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStripBar1.ResumeLayout(false);
            this.toolStripBar1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.Integration.ElementHost elementHost;
        private WpfHexaEditor.HexEditor hexEditor;
        private System.Windows.Forms.ToolStrip toolStripBar1;
        private System.Windows.Forms.ToolStripButton OpenFileButton;
        private System.Windows.Forms.ToolStripButton OpenTBLButton;
    }
}

