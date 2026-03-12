namespace CtrlCV
{
    partial class frmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            btnOpenFolder = new Button();
            label1 = new Label();
            btnUpdateList = new Button();
            label2 = new Label();
            linkLabel1 = new LinkLabel();
            SuspendLayout();
            // 
            // btnOpenFolder
            // 
            btnOpenFolder.Location = new Point(11, 205);
            btnOpenFolder.Name = "btnOpenFolder";
            btnOpenFolder.Size = new Size(213, 36);
            btnOpenFolder.TabIndex = 0;
            btnOpenFolder.Text = "Abrir pasta";
            btnOpenFolder.UseVisualStyleBackColor = true;
            btnOpenFolder.Click += btnOpenFolder_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 42);
            label1.Name = "label1";
            label1.Size = new Size(424, 140);
            label1.TabIndex = 1;
            label1.Text = resources.GetString("label1.Text");
            // 
            // btnUpdateList
            // 
            btnUpdateList.Location = new Point(224, 205);
            btnUpdateList.Name = "btnUpdateList";
            btnUpdateList.Size = new Size(213, 36);
            btnUpdateList.TabIndex = 2;
            btnUpdateList.Text = "Atualizar";
            btnUpdateList.UseVisualStyleBackColor = true;
            btnUpdateList.Click += btnUpdateList_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(12, 20);
            label2.Name = "label2";
            label2.Size = new Size(82, 20);
            label2.TabIndex = 3;
            label2.Text = "Instruções";
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(381, 9);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(56, 20);
            linkLabel1.TabIndex = 4;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "GitHub";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(448, 253);
            Controls.Add(linkLabel1);
            Controls.Add(label2);
            Controls.Add(btnUpdateList);
            Controls.Add(label1);
            Controls.Add(btnOpenFolder);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "frmMain";
            ShowIcon = false;
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CtrlCV Notes";
            WindowState = FormWindowState.Minimized;
            FormClosing += Config_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnOpenFolder;
        private Label label1;
        private Button btnUpdateList;
        private Label label2;
        private LinkLabel linkLabel1;
    }
}
