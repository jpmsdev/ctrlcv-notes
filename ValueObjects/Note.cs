using System;
using System.Collections.Generic;
using System.Text;

namespace CtrlCV.ValueObjects
{
    class Note
    {
        private string[] extensions_past = { ".txt" };
        public string FullPath { get; }
        public bool IsRootPath { get; }
        public Note(string fullPath, bool isRootPath)
        {
            FullPath = fullPath;
            IsRootPath = isRootPath;
        }

        public string GetFileNameTreeView()
        {
            return GetExtension().Equals(".txt")? GetFileNameWithouExtension(): GetFileName();
        }
        public string GetFileName()
        {
            return Path.GetFileName(FullPath);
        }

        public string GetFileNameWithouExtension()
        {
            return Path.GetFileNameWithoutExtension(FullPath);
        }

        public string GetDirectoryName()
        {
            string d = Path.GetFileName(Path.GetDirectoryName(FullPath));
            return this.IsRootPath ? "": d;
        }

        public string GetExtension()
        {
            return Path.GetExtension(FullPath).ToLower();
        }
        public string GetDirectoryAndFileNameWithoutExtension()
        {
            return GetDirectoryName() + "\\" + GetFileName();
        }
        public bool AllowPast()
        {
            return (extensions_past.IndexOf(GetExtension()) != -1);
        }
    }
}
