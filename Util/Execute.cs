using CtrlCV.ValueObjects;
using System.Diagnostics;

namespace CtrlCV.Util
{
    class Execute
    {
        public static void OpenNotesFolder()
        {
            Process.Start("explorer.exe", Files.GetNoteFolder());
        }

        public static void OpenWith(string path)
        {
            Process.Start("rundll32.exe", $"shell32.dll,OpenAs_RunDLL {path}");
        }

        public static void Open(string path)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $@"{path}",
                UseShellExecute = true
            });
        }
    }
}
