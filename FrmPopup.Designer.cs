namespace CtrlCV
{
    partial class FrmPopup
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
            txtSearch = new TextBox();
            tvItems = new TreeView();
            SuspendLayout();
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(13, 12);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(614, 27);
            txtSearch.TabIndex = 1;
            txtSearch.TextChanged += txtSearch_TextChanged;
            txtSearch.KeyUp += txtSearch_KeyUp;
            // 
            // tvItems
            // 
            tvItems.DrawMode = TreeViewDrawMode.OwnerDrawText;
            tvItems.HideSelection = false;
            tvItems.ItemHeight = 25;
            tvItems.Location = new Point(13, 45);
            tvItems.Name = "tvItems";
            tvItems.Size = new Size(614, 449);
            tvItems.TabIndex = 2;
            tvItems.DrawNode += tvItems_DrawNode;
            tvItems.DoubleClick += tvItems_DoubleClick;
            tvItems.KeyUp += tvItems_KeyUp;
            // 
            // FrmPopup
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(639, 506);
            Controls.Add(tvItems);
            Controls.Add(txtSearch);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmPopup";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Busca";
            TopMost = true;
            Deactivate += FrmPopup_Deactivate;
            Shown += FrmPopup_Shown;
            Leave += FrmPopup_Leave;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox txtSearch;
        private TreeView tvItems;
    }
}