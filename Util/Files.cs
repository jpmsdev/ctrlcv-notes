using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace CtrlCV.Util
{
    class Files
    {
        public const string note_dir_bookmarks = "Favoritos";
        public const string note_dir_name = "Notas";
        private static string note_folder = "";
        private static string[] all_notes;
        public static string[] GetFiles(string folder)
        {
            return Directory.GetFiles(folder + "", "*.txt");
        }

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

        public static void OpenNotesFolder()
        {
            Process.Start("explorer.exe", GetNoteFolder());
        }

        public static string[] SearchNotes(string search = "", bool force_update = false)
        {
            string[] filtered_notes;
            if (all_notes == null || all_notes.Length == 0 || force_update)
            {
                all_notes = GetNoteFiles();
            }
            if (search.Length == 0)
            {
                filtered_notes = all_notes;
            }
            else
            {
                List<string> filtered_notes_list = new List<string> { };
                string[] search_splitted = Text.Normalize(search).ToLower().Split(' ');

                foreach (var f in all_notes)
                {
                    string filename = Text.Normalize(Path.GetFileName(f)).ToLower();
                    string pathname = Text.Normalize(Path.GetDirectoryName(f)).ToLower();
                    bool filtered = true;
                    foreach (string s in search_splitted)
                    {
                        if (filename.IndexOf(s) == -1 && pathname.IndexOf(s) == -1)
                        {
                            filtered = false;
                            break;
                        }
                    }
                    if (filtered)
                    {
                        filtered_notes_list.Add(f);
                    }
                }

                filtered_notes = filtered_notes_list.ToArray();
            }

            return filtered_notes;
        }

        private static string[] GetNoteFiles(string path = "", bool recursive = true)
        {
            if (string.IsNullOrEmpty(path)) path = GetNoteFolder();

            List<string> notes = new List<string>();
            notes.AddRange(Directory.GetFiles(path, "*.txt"));
            if (recursive)
            {
                var _dir = Directory.GetDirectories(path);
                foreach (var d in _dir)
                {
                    notes.AddRange(GetNoteFiles(d, false));
                }
            }

            return notes.ToArray<string>();
        }
    }
}
