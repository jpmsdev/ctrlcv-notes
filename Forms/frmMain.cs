using CtrlCV.Util;
using System.Diagnostics;

namespace CtrlCV
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();

            //System.RegisterHotKey(this.Handle, 1,System. MOD_WIN, Keys.Space);
            CtrlCV.Util.System.RegisterHotKey(this.Handle, 1, CtrlCV.Util.System.MOD_CONTROL | CtrlCV.Util.System.MOD_SHIFT, Keys.Space);
            Files.LoadAllNotes();
        }

        private void ShowPopup()
        {
            CtrlCV.Util.System.GetExternalWindow();
            FrmPopup frmPopup = new FrmPopup();
            frmPopup.TopMost = true;
            frmPopup.Show();
            frmPopup.BringToFront();
            frmPopup.Focus();
            frmPopup.Activate();
            CtrlCV.Util.System.SetForegroundWindow(frmPopup.Handle);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == CtrlCV.Util.System.WM_HOTKEY && m.WParam.ToInt32() == 1)
            {
                CtrlCV.Util.System.ShowForm(this);

                ShowPopup();
                return;
            }
            base.WndProc(ref m);
        }

        bool unregister = false;
        protected void Config_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (unregister) return;

            CtrlCV.Util.System.UnregisterHotKey(this.Handle, 1);
            unregister = true;

            base.OnFormClosing(e);
        }

        private void btnUpdateList_Click(object sender, EventArgs e)
        {
            Files.LoadAllNotes();
            MessageBox.Show("Lista atualizada");
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            Execute.OpenNotesFolder();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/jpmsdev/ctrlcv-notes/releases",
                UseShellExecute = true
            });
        }
    }
}
