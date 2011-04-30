namespace AGS.Editor
{
    partial class TabbedDocumentContainer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabsPanel = new System.Windows.Forms.Panel();
            this.btnListAll = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.contentPane1 = new System.Windows.Forms.Panel();
            this.toolTipManager = new System.Windows.Forms.ToolTip(this.components);
            this.tabsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabsPanel
            // 
            this.tabsPanel.Controls.Add(this.btnListAll);
            this.tabsPanel.Controls.Add(this.btnClose);
            this.tabsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabsPanel.Location = new System.Drawing.Point(0, 0);
            this.tabsPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabsPanel.Name = "tabsPanel";
            this.tabsPanel.Size = new System.Drawing.Size(645, 27);
            this.tabsPanel.TabIndex = 0;
            this.tabsPanel.MouseLeave += new System.EventHandler(this.tabsPanel_MouseLeave);
            this.tabsPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.tabsPanel_Paint);
            this.tabsPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tabsPanel_MouseUp);
            this.tabsPanel.MouseEnter += new System.EventHandler(this.tabsPanel_MouseEnter);
            // 
            // btnListAll
            // 
            this.btnListAll.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnListAll.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnListAll.FlatAppearance.BorderSize = 0;
            this.btnListAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnListAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnListAll.ForeColor = System.Drawing.SystemColors.Control;
            this.btnListAll.Location = new System.Drawing.Point(584, 0);
            this.btnListAll.Margin = new System.Windows.Forms.Padding(0);
            this.btnListAll.Name = "btnListAll";
            this.btnListAll.Size = new System.Drawing.Size(27, 25);
            this.btnListAll.TabIndex = 1;
            this.btnListAll.UseVisualStyleBackColor = true;
            this.btnListAll.Click += new System.EventHandler(this.btnListAll_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.ForeColor = System.Drawing.SystemColors.Control;
            this.btnClose.Location = new System.Drawing.Point(611, 0);
            this.btnClose.Margin = new System.Windows.Forms.Padding(0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(27, 25);
            this.btnClose.TabIndex = 0;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // contentPane1
            // 
            this.contentPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPane1.Location = new System.Drawing.Point(0, 27);
            this.contentPane1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.contentPane1.Name = "contentPane1";
            this.contentPane1.Size = new System.Drawing.Size(645, 383);
            this.contentPane1.TabIndex = 1;
            // 
            // TabbedDocumentContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.contentPane1);
            this.Controls.Add(this.tabsPanel);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "TabbedDocumentContainer";
            this.Size = new System.Drawing.Size(645, 410);
            this.Resize += new System.EventHandler(this.TabbedDocumentContainer_Resize);
            this.tabsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel tabsPanel;
        private System.Windows.Forms.Panel contentPane1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ToolTip toolTipManager;
        private System.Windows.Forms.Button btnListAll;
    }
}
