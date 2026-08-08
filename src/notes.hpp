#pragma once

#include <string>
#include <vector>

struct Note {
    std::wstring fullPath;
    bool isRoot = false;
    std::wstring searchKey;
};

std::wstring NormalizeText(const std::wstring& text);
std::wstring ToLowerCopy(const std::wstring& text);

std::wstring GetAppDir();
std::wstring GetNotesFolder();
void LoadAllNotes(std::vector<Note>& out);

std::wstring GetFileName(const std::wstring& path);
std::wstring GetFileNameWithoutExtension(const std::wstring& path);
std::wstring GetExtensionLower(const std::wstring& path);
std::wstring GetParentDirName(const std::wstring& path);
std::wstring GetTreeDisplayName(const Note& note);
std::wstring GetDirectoryLabel(const Note& note);
bool AllowPaste(const Note& note);

std::vector<const Note*> FilterNotes(const std::vector<Note>& notes, const std::wstring& search);
