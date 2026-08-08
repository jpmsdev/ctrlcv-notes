#pragma once

#include <string>
#include <windows.h>

extern HWND g_externalWindow;

void CaptureExternalWindow();
void ClearExternalWindow();
bool PasteFileContents(const std::wstring& filepath);
void OpenPath(const std::wstring& path);
void OpenWithPath(const std::wstring& path);
void OpenNotesFolder();
void OpenGitHub();
std::wstring ReadFileText(const std::wstring& filepath);
bool SetClipboardUnicodeText(const std::wstring& text);
void SendCtrlV();
