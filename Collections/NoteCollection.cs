using CtrlCV.Util;
using CtrlCV.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CtrlCV.Collections
{
    class NoteCollection
    {
        public static NoteCollection Singleton = new NoteCollection();
        private List<Note> files = new();

        public void Clear()
        {
            files.Clear();
        }

        public void Add(Note f)
        {
            files.Add(f);
        }
        public Note[] Contains(string search)
        {
            search = Text.Normalize(search).Trim().ToLower();
            if (search.Length == 0)
            {
                return files.ToArray();
            }

            List<Note> filter = new List<Note>();
            foreach (var f in files)
            {
                string[] search_splitted = search.Split(' ');

                string filename = Text.Normalize(f.GetDirectoryAndFileNameWithoutExtension(true)).ToLower();
                bool filtered = true;
                foreach (string s in search_splitted)
                {
                    if (filename.IndexOf(s) == -1)
                    {
                        filtered = false;
                        break;
                    }
                }

                if (filtered)
                {
                    filter.Add(f);
                }
            }

            return filter.ToArray();
        }
    }
}
