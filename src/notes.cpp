#include "notes.hpp"

#include <windows.h>
#include <algorithm>
#include <cctype>
#include <map>

namespace {

std::wstring JoinPath(const std::wstring& a, const std::wstring& b) {
    if (a.empty()) return b;
    if (a.back() == L'\\' || a.back() == L'/') return a + b;
    return a + L"\\" + b;
}

bool DirExists(const std::wstring& path) {
    const DWORD attrs = GetFileAttributesW(path.c_str());
    return attrs != INVALID_FILE_ATTRIBUTES && (attrs & FILE_ATTRIBUTE_DIRECTORY);
}

void BuildSearchKey(Note& note) {
    std::wstring key = GetDirectoryLabel(note);
    if (!key.empty()) key += L"\\";
    key += GetFileName(note.fullPath);
    if (note.isRoot) {
        key = L"Favoritos\\" + GetFileName(note.fullPath);
    }
    note.searchKey = ToLowerCopy(NormalizeText(key));
}

void LoadNoteFiles(const std::wstring& path, bool isRoot, std::vector<Note>& out) {
    const std::wstring pattern = JoinPath(path, L"*.*");
    WIN32_FIND_DATAW fd{};
    HANDLE h = FindFirstFileW(pattern.c_str(), &fd);
    if (h == INVALID_HANDLE_VALUE) return;

    do {
        if (fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) continue;
        Note note;
        note.fullPath = JoinPath(path, fd.cFileName);
        note.isRoot = isRoot;
        BuildSearchKey(note);
        out.push_back(std::move(note));
    } while (FindNextFileW(h, &fd));
    FindClose(h);

    if (!isRoot) return;

    h = FindFirstFileW(pattern.c_str(), &fd);
    if (h == INVALID_HANDLE_VALUE) return;
    do {
        if (!(fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)) continue;
        if (wcscmp(fd.cFileName, L".") == 0 || wcscmp(fd.cFileName, L"..") == 0) continue;
        LoadNoteFiles(JoinPath(path, fd.cFileName), false, out);
    } while (FindNextFileW(h, &fd));
    FindClose(h);
}

} // namespace

std::wstring NormalizeText(const std::wstring& text) {
    static const std::map<wchar_t, const wchar_t*> map = {
        {L'á', L"a"}, {L'à', L"a"}, {L'ã', L"a"}, {L'â', L"a"}, {L'ä', L"a"}, {L'å', L"a"},
        {L'Á', L"A"}, {L'À', L"A"}, {L'Ã', L"A"}, {L'Â', L"A"}, {L'Ä', L"A"}, {L'Å', L"A"},
        {L'é', L"e"}, {L'è', L"e"}, {L'ê', L"e"}, {L'ë', L"e"},
        {L'É', L"E"}, {L'È', L"E"}, {L'Ê', L"E"}, {L'Ë', L"E"},
        {L'í', L"i"}, {L'ì', L"i"}, {L'î', L"i"}, {L'ï', L"i"},
        {L'Í', L"I"}, {L'Ì', L"I"}, {L'Î', L"I"}, {L'Ï', L"I"},
        {L'ó', L"o"}, {L'ò', L"o"}, {L'õ', L"o"}, {L'ô', L"o"}, {L'ö', L"o"}, {L'ø', L"o"},
        {L'Ó', L"O"}, {L'Ò', L"O"}, {L'Õ', L"O"}, {L'Ô', L"O"}, {L'Ö', L"O"}, {L'Ø', L"O"},
        {L'ú', L"u"}, {L'ù', L"u"}, {L'û', L"u"}, {L'ü', L"u"},
        {L'Ú', L"U"}, {L'Ù', L"U"}, {L'Û', L"U"}, {L'Ü', L"U"},
        {L'ç', L"c"}, {L'Ç', L"C"},
        {L'ñ', L"n"}, {L'Ñ', L"N"},
        {L'ý', L"y"}, {L'ÿ', L"y"}, {L'Ý', L"Y"},
        {L'æ', L"ae"}, {L'Æ', L"AE"}, {L'œ', L"oe"}, {L'Œ', L"OE"},
        {L'ß', L"ss"},
        {L'ð', L"d"}, {L'Ð', L"D"}, {L'þ', L"th"}, {L'Þ', L"Th"},
        {L'ł', L"l"}, {L'Ł', L"L"},
    };

    std::wstring out;
    out.reserve(text.size());
    for (wchar_t c : text) {
        auto it = map.find(c);
        if (it != map.end()) out += it->second;
        else out.push_back(c);
    }
    return out;
}

std::wstring ToLowerCopy(const std::wstring& text) {
    std::wstring out = text;
    for (wchar_t& c : out) {
        c = static_cast<wchar_t>(towlower(c));
    }
    return out;
}

std::wstring GetAppDir() {
    wchar_t buf[MAX_PATH]{};
    GetModuleFileNameW(nullptr, buf, MAX_PATH);
    std::wstring path(buf);
    const size_t pos = path.find_last_of(L"\\/");
    if (pos != std::wstring::npos) path.resize(pos + 1);
    return path;
}

std::wstring GetNotesFolder() {
    static std::wstring cached;
    if (!cached.empty()) return cached;

    const std::wstring appDir = GetAppDir();
    for (int i = 0; i <= 3; ++i) {
        std::wstring folder = appDir;
        for (int u = 0; u < i; ++u) folder += L"..\\";
        folder += L"Notas";
        wchar_t full[MAX_PATH]{};
        if (GetFullPathNameW(folder.c_str(), MAX_PATH, full, nullptr) == 0) continue;
        if (DirExists(full)) {
            cached = full;
            if (!cached.empty() && cached.back() != L'\\') cached += L'\\';
            return cached;
        }
    }

    cached = JoinPath(appDir, L"Notas");
    CreateDirectoryW(cached.c_str(), nullptr);
    wchar_t full[MAX_PATH]{};
    GetFullPathNameW(cached.c_str(), MAX_PATH, full, nullptr);
    cached = full;
    if (!cached.empty() && cached.back() != L'\\') cached += L'\\';
    return cached;
}

void LoadAllNotes(std::vector<Note>& out) {
    out.clear();
    LoadNoteFiles(GetNotesFolder(), true, out);
}

std::wstring GetFileName(const std::wstring& path) {
    const size_t pos = path.find_last_of(L"\\/");
    return pos == std::wstring::npos ? path : path.substr(pos + 1);
}

std::wstring GetFileNameWithoutExtension(const std::wstring& path) {
    std::wstring name = GetFileName(path);
    const size_t dot = name.find_last_of(L'.');
    if (dot == std::wstring::npos) return name;
    return name.substr(0, dot);
}

std::wstring GetExtensionLower(const std::wstring& path) {
    const std::wstring name = GetFileName(path);
    const size_t dot = name.find_last_of(L'.');
    if (dot == std::wstring::npos) return {};
    return ToLowerCopy(name.substr(dot));
}

std::wstring GetParentDirName(const std::wstring& path) {
    size_t end = path.find_last_of(L"\\/");
    if (end == std::wstring::npos || end == 0) return {};
    size_t start = path.find_last_of(L"\\/", end - 1);
    if (start == std::wstring::npos) return path.substr(0, end);
    return path.substr(start + 1, end - start - 1);
}

std::wstring GetTreeDisplayName(const Note& note) {
    if (GetExtensionLower(note.fullPath) == L".txt")
        return GetFileNameWithoutExtension(note.fullPath);
    return GetFileName(note.fullPath);
}

std::wstring GetDirectoryLabel(const Note& note) {
    if (note.isRoot) return {};
    return GetParentDirName(note.fullPath);
}

bool AllowPaste(const Note& note) {
    return GetExtensionLower(note.fullPath) == L".txt";
}

std::vector<const Note*> FilterNotes(const std::vector<Note>& notes, const std::wstring& search) {
    std::wstring q = ToLowerCopy(NormalizeText(search));
    // trim
    while (!q.empty() && iswspace(q.front())) q.erase(q.begin());
    while (!q.empty() && iswspace(q.back())) q.pop_back();

    std::vector<const Note*> result;
    if (q.empty()) {
        result.reserve(notes.size());
        for (const auto& n : notes) result.push_back(&n);
        return result;
    }

    std::vector<std::wstring> parts;
    size_t i = 0;
    while (i < q.size()) {
        while (i < q.size() && iswspace(q[i])) ++i;
        size_t j = i;
        while (j < q.size() && !iswspace(q[j])) ++j;
        if (i < j) parts.emplace_back(q.substr(i, j - i));
        i = j;
    }

    for (const auto& n : notes) {
        bool ok = true;
        for (const auto& p : parts) {
            if (n.searchKey.find(p) == std::wstring::npos) {
                ok = false;
                break;
            }
        }
        if (ok) result.push_back(&n);
    }
    return result;
}
