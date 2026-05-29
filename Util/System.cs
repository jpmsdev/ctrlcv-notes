using System.Runtime.InteropServices;

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

        // Mensagens para habilitar double buffering nativo do TreeView
        private const int TV_FIRST = 0x1100;
        private const int TVM_SETEXTENDEDSTYLE = TV_FIRST + 44;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;

        // Importar funções do Windows
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, Keys vk);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static void EnableDoubleBuffer(IntPtr handle)
        {
            SendMessage(handle, TVM_SETEXTENDEDSTYLE, TVS_EX_DOUBLEBUFFER, TVS_EX_DOUBLEBUFFER);
        }

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

        public static void PasteFile(string filepath, int attempt = 1)
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
                if (attempt <= 10)
                {
                    Thread.Sleep(50);
                    PasteFile(filepath, ++attempt);
                }
            }
        }
    }
}
