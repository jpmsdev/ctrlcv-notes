using CtrlCV.Collections;
using System.Diagnostics;

namespace CtrlCV.Util
{
    class Files
    {
        public const string note_dir_bookmarks = "Favoritos";
        public const string note_dir_name = "Notas";
        private static string note_folder = "";

        public static string GetAppDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetNoteFolder()
        {
            if (!string.IsNullOrEmpty(note_folder))
            {
                return note_folder;
            }

            for (var i = 0; i <= 3; i++)
            {
                note_folder = GetAppDir() + string.Concat(Enumerable.Repeat("\\..", i)) + $"\\{note_dir_name}\\";

                if (Directory.Exists(note_folder))
                {
                    note_folder = Path.GetFullPath(note_folder);
                    return note_folder;
                }
            }

            note_folder = GetAppDir() + $"\\{note_dir_name}\\";
            Directory.CreateDirectory(note_folder);
            return Path.GetFullPath(note_folder);
        }

        public static void LoadAllNotes()
        {
            LoadNoteFiles();
        }
        private static void LoadNoteFiles(string path = "", bool isRoot = true)
        {
            var f = NoteCollection.Singleton;
            if (isRoot) f.Clear();

            if (string.IsNullOrEmpty(path)) path = GetNoteFolder();

            var files = Directory.GetFiles(path, "*.*");
            foreach (var file in files)
            {
                f.Add(new ValueObjects.Note(file, isRoot));
            }

            if (isRoot)
            {
                var _dir = Directory.GetDirectories(path);
                foreach (var d in _dir)
                {
                    LoadNoteFiles(d, false);
                }
            }
        }
    }
}
