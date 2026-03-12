using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CtrlCV.Util
{
    class System
    {
        // Constantes do Windows
        public const int WM_HOTKEY = 0x0312;
        public const int MOD_WIN = 0x0008;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;
        public const int SW_HIDE = 0;
        public const int SW_SHOWNA = 8; // mostra sem ativar
        public static IntPtr ExternalWindowPointer;

        // Importar funções do Windows
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, Keys vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void ShowForm(Form frm)
        {
            if (frm != null)
            {
                ShowWindow(frm.Handle, CtrlCV.Util.System.SW_SHOWNA);
            }
        }
        public static IntPtr GetExternalWindow()
        {
            if (ExternalWindowPointer == IntPtr.Zero)
            {
                ExternalWindowPointer = GetForegroundWindow();
            }
            return ExternalWindowPointer;
        }

        public static void PasteFile(string filepath)
        {
            try
            {
                filepath = Path.GetFullPath(filepath);
                Clipboard.SetText(File.ReadAllText(filepath));
                if (ExternalWindowPointer != IntPtr.Zero)
                    SetForegroundWindow(ExternalWindowPointer);

                Thread.Sleep(50);
                SendKeys.SendWait("^v");
                ExternalWindowPointer = IntPtr.Zero;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }
    }
}
