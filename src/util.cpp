#include "util.hpp"
#include "notes.hpp"

#include <shellapi.h>
#include <cstring>
#include <vector>

HWND g_externalWindow = nullptr;

void CaptureExternalWindow() {
    if (!g_externalWindow) {
        g_externalWindow = GetForegroundWindow();
    }
}

void ClearExternalWindow() {
    g_externalWindow = nullptr;
}

std::wstring ReadFileText(const std::wstring& filepath) {
    HANDLE h = CreateFileW(filepath.c_str(), GENERIC_READ, FILE_SHARE_READ, nullptr,
                           OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, nullptr);
    if (h == INVALID_HANDLE_VALUE) return {};

    LARGE_INTEGER size{};
    if (!GetFileSizeEx(h, &size) || size.QuadPart < 0 || size.QuadPart > 8 * 1024 * 1024) {
        CloseHandle(h);
        return {};
    }

    std::vector<char> bytes(static_cast<size_t>(size.QuadPart));
    DWORD read = 0;
    if (size.QuadPart > 0) {
        if (!ReadFile(h, bytes.data(), static_cast<DWORD>(bytes.size()), &read, nullptr)) {
            CloseHandle(h);
            return {};
        }
    }
    CloseHandle(h);
    bytes.resize(read);

    if (bytes.size() >= 2 && (unsigned char)bytes[0] == 0xFF && (unsigned char)bytes[1] == 0xFE) {
        // UTF-16 LE
        const wchar_t* w = reinterpret_cast<const wchar_t*>(bytes.data() + 2);
        const size_t n = (bytes.size() - 2) / sizeof(wchar_t);
        return std::wstring(w, n);
    }
    if (bytes.size() >= 3 &&
        (unsigned char)bytes[0] == 0xEF &&
        (unsigned char)bytes[1] == 0xBB &&
        (unsigned char)bytes[2] == 0xBF) {
        int n = MultiByteToWideChar(CP_UTF8, 0, bytes.data() + 3, static_cast<int>(bytes.size() - 3), nullptr, 0);
        std::wstring out(n, L'\0');
        MultiByteToWideChar(CP_UTF8, 0, bytes.data() + 3, static_cast<int>(bytes.size() - 3), &out[0], n);
        return out;
    }

    // Try UTF-8, fallback ACP
    int n = MultiByteToWideChar(CP_UTF8, MB_ERR_INVALID_CHARS, bytes.data(), static_cast<int>(bytes.size()), nullptr, 0);
    if (n > 0) {
        std::wstring out(n, L'\0');
        MultiByteToWideChar(CP_UTF8, 0, bytes.data(), static_cast<int>(bytes.size()), &out[0], n);
        return out;
    }
    n = MultiByteToWideChar(CP_ACP, 0, bytes.data(), static_cast<int>(bytes.size()), nullptr, 0);
    std::wstring out(n, L'\0');
    if (n > 0) {
        MultiByteToWideChar(CP_ACP, 0, bytes.data(), static_cast<int>(bytes.size()), &out[0], n);
    }
    return out;
}

bool SetClipboardUnicodeText(const std::wstring& text) {
    if (!OpenClipboard(nullptr)) return false;
    EmptyClipboard();
    const size_t bytes = (text.size() + 1) * sizeof(wchar_t);
    HGLOBAL hMem = GlobalAlloc(GMEM_MOVEABLE, bytes);
    if (!hMem) {
        CloseClipboard();
        return false;
    }
    void* ptr = GlobalLock(hMem);
    if (!ptr) {
        GlobalFree(hMem);
        CloseClipboard();
        return false;
    }
    memcpy(ptr, text.c_str(), bytes);
    GlobalUnlock(hMem);
    SetClipboardData(CF_UNICODETEXT, hMem);
    CloseClipboard();
    return true;
}

void SendCtrlV() {
    INPUT inputs[4]{};
    inputs[0].type = INPUT_KEYBOARD;
    inputs[0].ki.wVk = VK_CONTROL;
    inputs[1].type = INPUT_KEYBOARD;
    inputs[1].ki.wVk = 'V';
    inputs[2].type = INPUT_KEYBOARD;
    inputs[2].ki.wVk = 'V';
    inputs[2].ki.dwFlags = KEYEVENTF_KEYUP;
    inputs[3].type = INPUT_KEYBOARD;
    inputs[3].ki.wVk = VK_CONTROL;
    inputs[3].ki.dwFlags = KEYEVENTF_KEYUP;
    SendInput(4, inputs, sizeof(INPUT));
}

bool PasteFileContents(const std::wstring& filepath) {
    for (int attempt = 1; attempt <= 10; ++attempt) {
        const std::wstring text = ReadFileText(filepath);
        if (text.empty() && GetFileAttributesW(filepath.c_str()) == INVALID_FILE_ATTRIBUTES) {
            MessageBoxW(nullptr, L"Erro ao ler o arquivo.", L"CtrlCV Notes", MB_ICONERROR);
            return false;
        }
        if (!SetClipboardUnicodeText(text)) {
            Sleep(50);
            continue;
        }
        if (g_externalWindow) {
            SetForegroundWindow(g_externalWindow);
        }
        Sleep(50);
        SendCtrlV();
        ClearExternalWindow();
        return true;
    }
    MessageBoxW(nullptr, L"Erro ao colar o texto.", L"CtrlCV Notes", MB_ICONERROR);
    return false;
}

void OpenPath(const std::wstring& path) {
    ShellExecuteW(nullptr, L"open", path.c_str(), nullptr, nullptr, SW_SHOWNORMAL);
}

void OpenWithPath(const std::wstring& path) {
    std::wstring params = L"shell32.dll,OpenAs_RunDLL ";
    params += path;
    ShellExecuteW(nullptr, L"open", L"rundll32.exe", params.c_str(), nullptr, SW_SHOWNORMAL);
}

void OpenNotesFolder() {
    ShellExecuteW(nullptr, L"explore", GetNotesFolder().c_str(), nullptr, nullptr, SW_SHOWNORMAL);
}

void OpenGitHub() {
    ShellExecuteW(nullptr, L"open", L"https://github.com/jpmsdev/ctrlcv-notes/releases",
                  nullptr, nullptr, SW_SHOWNORMAL);
}
