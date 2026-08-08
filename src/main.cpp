#include "notes.hpp"
#include "resource.h"
#include "util.hpp"

#include <commctrl.h>
#include <windowsx.h>

#include <string>
#include <vector>

#pragma comment(lib, "comctl32.lib")
#pragma comment(lib, "shell32.lib")
#pragma comment(lib, "user32.lib")
#pragma comment(lib, "gdi32.lib")

namespace {

HINSTANCE g_inst = nullptr;
HWND g_mainWnd = nullptr;
HWND g_popupWnd = nullptr;
HWND g_editSearch = nullptr;
HWND g_treeItems = nullptr;
std::vector<Note> g_notes;
bool g_openWith = false;
bool g_closingPopup = false;
bool g_suppressDeactivateClose = false;

constexpr wchar_t kMainClass[] = L"CtrlCVNotesMain";
constexpr wchar_t kPopupClass[] = L"CtrlCVNotesPopup";
constexpr int kHotkeyId = HOTKEY_ID_POPUP;

const wchar_t kHelpText[] =
    L"1: Aperte Ctrl+Shift+Espaço para abrir a busca;\n"
    L"2: Setas (↑ e ↓) para navegar;\n"
    L"3: Duplo clique ou ENTER para colar as informações;\n"
    L"4: Segurar o botão SHIFT seleciona o app para abrir o arquivo.\n"
    L"\n"
    L"Arquivos que não são de texto (exemplo: pdf, doc) não serão\n"
    L"copiados e sim abertos nos respectivos programas.";

HTREEITEM GetFirstLeaf(HWND tree) {
    for (HTREEITEM parent = TreeView_GetRoot(tree); parent; parent = TreeView_GetNextSibling(tree, parent)) {
        HTREEITEM child = TreeView_GetChild(tree, parent);
        if (child) return child;
    }
    return nullptr;
}

std::vector<HTREEITEM> GetAllLeaves(HWND tree) {
    std::vector<HTREEITEM> leaves;
    for (HTREEITEM parent = TreeView_GetRoot(tree); parent; parent = TreeView_GetNextSibling(tree, parent)) {
        for (HTREEITEM child = TreeView_GetChild(tree, parent); child; child = TreeView_GetNextSibling(tree, child)) {
            leaves.push_back(child);
        }
    }
    return leaves;
}

const Note* NoteFromItem(HWND tree, HTREEITEM item) {
    if (!item) return nullptr;
    TVITEMW tvi{};
    tvi.mask = TVIF_PARAM | TVIF_CHILDREN;
    tvi.hItem = item;
    if (!TreeView_GetItem(tree, &tvi)) return nullptr;
    if (tvi.cChildren != 0) return nullptr;
    if (tvi.lParam == 0) return nullptr;
    return reinterpret_cast<const Note*>(tvi.lParam);
}

void SelectLeaf(HWND tree, HTREEITEM item) {
    if (!item) return;
    TreeView_SelectItem(tree, item);
    TreeView_EnsureVisible(tree, item);
}

void SelectUp(HWND tree) {
    auto leaves = GetAllLeaves(tree);
    if (leaves.empty()) return;
    HTREEITEM current = TreeView_GetSelection(tree);
    if (!current || !TreeView_GetParent(tree, current)) {
        SelectLeaf(tree, leaves.front());
        return;
    }
    size_t index = 0;
    for (size_t i = 0; i < leaves.size(); ++i) {
        if (leaves[i] == current) {
            index = i;
            break;
        }
    }
    if (index == 0) index = leaves.size() - 1;
    else --index;
    SelectLeaf(tree, leaves[index]);
}

void SelectDown(HWND tree) {
    auto leaves = GetAllLeaves(tree);
    if (leaves.empty()) return;
    HTREEITEM current = TreeView_GetSelection(tree);
    if (!current || !TreeView_GetParent(tree, current)) {
        SelectLeaf(tree, leaves.front());
        return;
    }
    size_t index = 0;
    for (size_t i = 0; i < leaves.size(); ++i) {
        if (leaves[i] == current) {
            index = i;
            break;
        }
    }
    ++index;
    if (index >= leaves.size()) index = 0;
    SelectLeaf(tree, leaves[index]);
}

void SetOpenWith(bool enable) {
    g_openWith = enable;
    if (g_popupWnd) {
        SetWindowTextW(g_popupWnd, enable ? L"Busca (Abrir com...)" : L"Busca");
    }
}

void ClosePopup() {
    if (!g_popupWnd || g_closingPopup) return;
    g_closingPopup = true;
    DestroyWindow(g_popupWnd);
    g_popupWnd = nullptr;
    g_editSearch = nullptr;
    g_treeItems = nullptr;
    g_openWith = false;
    g_closingPopup = false;
}

void ActivateSelectedNote() {
    if (!g_treeItems) return;
    HTREEITEM sel = TreeView_GetSelection(g_treeItems);
    const Note* note = NoteFromItem(g_treeItems, sel);
    if (!note) return;

    const bool openWith = g_openWith || (GetKeyState(VK_SHIFT) & 0x8000);
    g_suppressDeactivateClose = true;
    if (openWith) {
        OpenWithPath(note->fullPath);
    } else if (AllowPaste(*note)) {
        PasteFileContents(note->fullPath);
    } else {
        OpenPath(note->fullPath);
    }
    g_suppressDeactivateClose = false;
    ClosePopup();
}

HTREEITEM InsertTreeItem(HWND tree, HTREEITEM parent, const wchar_t* text, LPARAM param, bool hasChildren) {
    TVINSERTSTRUCTW is{};
    is.hParent = parent ? parent : TVI_ROOT;
    is.hInsertAfter = TVI_LAST;
    is.item.mask = TVIF_TEXT | TVIF_PARAM | TVIF_CHILDREN;
    is.item.pszText = const_cast<wchar_t*>(text);
    is.item.lParam = param;
    is.item.cChildren = hasChildren ? 1 : 0;
    return TreeView_InsertItem(tree, &is);
}

void UpdateNotesList() {
    if (!g_treeItems || !g_editSearch) return;

    wchar_t buf[512]{};
    GetWindowTextW(g_editSearch, buf, 512);
    const auto filtered = FilterNotes(g_notes, buf);

    SetWindowRedraw(g_treeItems, FALSE);
    TreeView_DeleteAllItems(g_treeItems);

    HTREEITEM bookmarks = nullptr;
    HTREEITEM lastDir = nullptr;
    std::wstring lastDirName;

    for (const Note* n : filtered) {
        if (n->isRoot) {
            if (!bookmarks) {
                bookmarks = InsertTreeItem(g_treeItems, nullptr, L"Favoritos", 0, true);
            }
            InsertTreeItem(g_treeItems, bookmarks, GetTreeDisplayName(*n).c_str(),
                           reinterpret_cast<LPARAM>(n), false);
            continue;
        }

        const std::wstring dir = GetDirectoryLabel(*n);
        if (!lastDir || dir != lastDirName) {
            lastDirName = dir;
            lastDir = InsertTreeItem(g_treeItems, nullptr, dir.c_str(), 0, true);
        }
        InsertTreeItem(g_treeItems, lastDir, GetTreeDisplayName(*n).c_str(),
                       reinterpret_cast<LPARAM>(n), false);
    }

    // Expand policy matches C#: only Favoritos when search empty; else expand all
    std::wstring q = buf;
    while (!q.empty() && iswspace(q.front())) q.erase(q.begin());
    while (!q.empty() && iswspace(q.back())) q.pop_back();

    if (bookmarks && q.empty()) {
        TreeView_Expand(g_treeItems, bookmarks, TVE_EXPAND);
    } else {
        for (HTREEITEM parent = TreeView_GetRoot(g_treeItems); parent;
             parent = TreeView_GetNextSibling(g_treeItems, parent)) {
            TreeView_Expand(g_treeItems, parent, TVE_EXPAND);
        }
    }

    SetWindowRedraw(g_treeItems, TRUE);
    InvalidateRect(g_treeItems, nullptr, TRUE);
    SelectUp(g_treeItems);
}

LRESULT CALLBACK PopupProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam) {
    switch (msg) {
    case WM_CREATE: {
        g_editSearch = CreateWindowExW(WS_EX_CLIENTEDGE, L"EDIT", L"",
            WS_CHILD | WS_VISIBLE | ES_AUTOHSCROLL,
            13, 12, 614, 27, hwnd, reinterpret_cast<HMENU>(IDC_EDIT_SEARCH), g_inst, nullptr);

        g_treeItems = CreateWindowExW(WS_EX_CLIENTEDGE, WC_TREEVIEWW, L"",
            WS_CHILD | WS_VISIBLE | TVS_HASBUTTONS | TVS_HASLINES | TVS_LINESATROOT |
            TVS_SHOWSELALWAYS | TVS_FULLROWSELECT,
            13, 45, 614, 586, hwnd, reinterpret_cast<HMENU>(IDC_TREE_ITEMS), g_inst, nullptr);

        HFONT font = reinterpret_cast<HFONT>(GetStockObject(DEFAULT_GUI_FONT));
        SendMessageW(g_editSearch, WM_SETFONT, reinterpret_cast<WPARAM>(font), TRUE);
        SendMessageW(g_treeItems, WM_SETFONT, reinterpret_cast<WPARAM>(font), TRUE);

        // Double-buffer TreeView
        TreeView_SetExtendedStyle(g_treeItems, TVS_EX_DOUBLEBUFFER, TVS_EX_DOUBLEBUFFER);

        SetFocus(g_editSearch);
        UpdateNotesList();
        return 0;
    }
    case WM_ACTIVATE:
        if (LOWORD(wParam) == WA_INACTIVE && !g_closingPopup && !g_suppressDeactivateClose) {
            // Close when focus leaves (same as Deactivate in WinForms)
            PostMessageW(hwnd, WM_CLOSE, 0, 0);
        }
        return 0;
    case WM_COMMAND:
        if (LOWORD(wParam) == IDC_EDIT_SEARCH && HIWORD(wParam) == EN_CHANGE) {
            UpdateNotesList();
            return 0;
        }
        break;
    case WM_NOTIFY: {
        const auto* hdr = reinterpret_cast<LPNMHDR>(lParam);
        if (hdr->idFrom != IDC_TREE_ITEMS) break;

        if (hdr->code == NM_DBLCLK) {
            ActivateSelectedNote();
            return TRUE;
        }

        // Always paint selection in highlight blue (like WinForms HideSelection=false + owner-draw)
        if (hdr->code == NM_CUSTOMDRAW) {
            auto* cd = reinterpret_cast<LPNMTVCUSTOMDRAW>(lParam);
            switch (cd->nmcd.dwDrawStage) {
            case CDDS_PREPAINT:
                return CDRF_NOTIFYITEMDRAW;
            case CDDS_ITEMPREPAINT: {
                const HTREEITEM item = reinterpret_cast<HTREEITEM>(cd->nmcd.dwItemSpec);
                const bool selected =
                    item == TreeView_GetSelection(hdr->hwndFrom) ||
                    (cd->nmcd.uItemState & CDIS_SELECTED) != 0;
                if (selected) {
                    RECT rc{};
                    if (TreeView_GetItemRect(hdr->hwndFrom, item, &rc, FALSE)) {
                        FillRect(cd->nmcd.hdc, &rc, GetSysColorBrush(COLOR_HIGHLIGHT));
                    }
                    cd->clrText = GetSysColor(COLOR_HIGHLIGHTTEXT);
                    cd->clrTextBk = GetSysColor(COLOR_HIGHLIGHT);
                    // Avoid gray "inactive selection" overlay from the control
                    cd->nmcd.uItemState &= ~(CDIS_SELECTED | CDIS_FOCUS);
                    return CDRF_NEWFONT;
                }
                return CDRF_DODEFAULT;
            }
            default:
                break;
            }
        }
        break;
    }
    case WM_HOTKEY:
        break;
    case WM_KEYDOWN:
    case WM_SYSKEYDOWN:
        if (wParam == VK_SHIFT) SetOpenWith(true);
        break;
    case WM_KEYUP:
    case WM_SYSKEYUP:
        if (wParam == VK_SHIFT) SetOpenWith(false);
        if (wParam == VK_ESCAPE) {
            ClosePopup();
            return 0;
        }
        if (wParam == VK_RETURN) {
            ActivateSelectedNote();
            return 0;
        }
        if (wParam == VK_UP) {
            SelectUp(g_treeItems);
            return 0;
        }
        if (wParam == VK_DOWN) {
            SelectDown(g_treeItems);
            return 0;
        }
        break;
    case WM_CLOSE:
        ClosePopup();
        return 0;
    case WM_DESTROY:
        if (g_popupWnd == hwnd) {
            g_popupWnd = nullptr;
            g_editSearch = nullptr;
            g_treeItems = nullptr;
        }
        return 0;
    }
    return DefWindowProcW(hwnd, msg, wParam, lParam);
}

// Subclass edit/tree so arrow/enter/escape work while they have focus
LRESULT CALLBACK SearchSubclass(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam,
                                UINT_PTR, DWORD_PTR) {
    if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN) {
        if (wParam == VK_SHIFT) SetOpenWith(true);
        if (wParam == VK_DOWN) { SelectDown(g_treeItems); return 0; }
        if (wParam == VK_UP) { SelectUp(g_treeItems); return 0; }
        if (wParam == VK_RETURN) { ActivateSelectedNote(); return 0; }
        if (wParam == VK_ESCAPE) { ClosePopup(); return 0; }
    }
    if (msg == WM_KEYUP || msg == WM_SYSKEYUP) {
        if (wParam == VK_SHIFT) SetOpenWith(false);
    }
    return DefSubclassProc(hwnd, msg, wParam, lParam);
}

LRESULT CALLBACK TreeSubclass(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam,
                              UINT_PTR, DWORD_PTR) {
    if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN) {
        if (wParam == VK_SHIFT) SetOpenWith(true);
        if (wParam == VK_RETURN) { ActivateSelectedNote(); return 0; }
        if (wParam == VK_ESCAPE) { ClosePopup(); return 0; }
    }
    if (msg == WM_KEYUP || msg == WM_SYSKEYUP) {
        if (wParam == VK_SHIFT) SetOpenWith(false);
    }
    return DefSubclassProc(hwnd, msg, wParam, lParam);
}

void ShowPopup() {
    if (g_popupWnd && IsWindow(g_popupWnd)) {
        SetForegroundWindow(g_popupWnd);
        SetFocus(g_editSearch ? g_editSearch : g_popupWnd);
        return;
    }

    CaptureExternalWindow();

    const int width = 655;
    const int height = 680;
    const int x = (GetSystemMetrics(SM_CXSCREEN) - width) / 2;
    const int y = (GetSystemMetrics(SM_CYSCREEN) - height) / 2;

    g_popupWnd = CreateWindowExW(
        WS_EX_TOPMOST | WS_EX_TOOLWINDOW,
        kPopupClass,
        L"Busca",
        WS_POPUP | WS_CAPTION | WS_SYSMENU,
        x, y, width, height,
        g_mainWnd, nullptr, g_inst, nullptr);

    if (!g_popupWnd) return;

    SetWindowSubclass(g_editSearch, SearchSubclass, 1, 0);
    SetWindowSubclass(g_treeItems, TreeSubclass, 2, 0);

    ShowWindow(g_popupWnd, SW_SHOW);
    SetForegroundWindow(g_popupWnd);
    SetFocus(g_editSearch);
}

LRESULT CALLBACK MainProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam) {
    switch (msg) {
    case WM_CREATE: {
        CreateWindowW(L"STATIC", L"Instruções",
            WS_CHILD | WS_VISIBLE | SS_LEFT,
            11, 7, 200, 20, hwnd, reinterpret_cast<HMENU>(IDC_LBL_TITLE), g_inst, nullptr);

        CreateWindowW(L"STATIC", kHelpText,
            WS_CHILD | WS_VISIBLE | SS_LEFT,
            11, 31, 506, 150, hwnd, reinterpret_cast<HMENU>(IDC_LBL_HELP), g_inst, nullptr);

        CreateWindowW(L"BUTTON", L"Abrir pasta 'Notas'",
            WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
            11, 190, 256, 34, hwnd, reinterpret_cast<HMENU>(IDC_BTN_OPEN), g_inst, nullptr);

        CreateWindowW(L"BUTTON", L"Atualizar lista",
            WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
            269, 190, 256, 34, hwnd, reinterpret_cast<HMENU>(IDC_BTN_UPDATE), g_inst, nullptr);

        // Simple GitHub "link" as button for reliability without SysLink complexity
        CreateWindowW(L"BUTTON", L"GitHub",
            WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
            432, 5, 80, 24, hwnd, reinterpret_cast<HMENU>(IDC_LNK_GITHUB), g_inst, nullptr);

        HFONT font = reinterpret_cast<HFONT>(GetStockObject(DEFAULT_GUI_FONT));
        EnumChildWindows(hwnd, [](HWND child, LPARAM lp) -> BOOL {
            SendMessageW(child, WM_SETFONT, lp, TRUE);
            return TRUE;
        }, reinterpret_cast<LPARAM>(font));

        RegisterHotKey(hwnd, kHotkeyId, MOD_CONTROL | MOD_SHIFT, VK_SPACE);
        LoadAllNotes(g_notes);
        return 0;
    }
    case WM_HOTKEY:
        if (wParam == kHotkeyId) {
            ShowWindow(hwnd, SW_SHOWNA);
            ShowPopup();
            return 0;
        }
        break;
    case WM_COMMAND:
        switch (LOWORD(wParam)) {
        case IDC_BTN_OPEN:
            OpenNotesFolder();
            return 0;
        case IDC_BTN_UPDATE:
            LoadAllNotes(g_notes);
            MessageBoxW(hwnd, L"Lista atualizada", L"CtrlCV Notes", MB_OK | MB_ICONINFORMATION);
            return 0;
        case IDC_LNK_GITHUB:
            OpenGitHub();
            return 0;
        }
        break;
    case WM_DESTROY:
        UnregisterHotKey(hwnd, kHotkeyId);
        ClosePopup();
        PostQuitMessage(0);
        return 0;
    }
    return DefWindowProcW(hwnd, msg, wParam, lParam);
}

bool RegisterClasses(HINSTANCE inst) {
    WNDCLASSW wc{};
    wc.lpfnWndProc = MainProc;
    wc.hInstance = inst;
    wc.lpszClassName = kMainClass;
    wc.hCursor = LoadCursor(nullptr, IDC_ARROW);
    wc.hbrBackground = reinterpret_cast<HBRUSH>(COLOR_BTNFACE + 1);
    wc.hIcon = LoadIconW(inst, MAKEINTRESOURCEW(IDI_APPICON));
    if (!RegisterClassW(&wc)) return false;

    WNDCLASSW pc{};
    pc.lpfnWndProc = PopupProc;
    pc.hInstance = inst;
    pc.lpszClassName = kPopupClass;
    pc.hCursor = LoadCursor(nullptr, IDC_ARROW);
    pc.hbrBackground = reinterpret_cast<HBRUSH>(COLOR_BTNFACE + 1);
    if (!RegisterClassW(&pc)) return false;
    return true;
}

} // namespace

int APIENTRY wWinMain(HINSTANCE inst, HINSTANCE, LPWSTR, int) {
    g_inst = inst;

    INITCOMMONCONTROLSEX icc{ sizeof(icc), ICC_TREEVIEW_CLASSES | ICC_STANDARD_CLASSES };
    InitCommonControlsEx(&icc);

    if (!RegisterClasses(inst)) return 1;

    g_mainWnd = CreateWindowExW(
        0, kMainClass, L"CtrlCV Notes",
        WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX,
        CW_USEDEFAULT, CW_USEDEFAULT, 557, 275,
        nullptr, nullptr, inst, nullptr);
    if (!g_mainWnd) return 1;

    // Start minimized like the C# app
    ShowWindow(g_mainWnd, SW_SHOWMINNOACTIVE);

    MSG msg;
    while (GetMessageW(&msg, nullptr, 0, 0) > 0) {
        // Let Shift state update title while popup is open
        if (g_popupWnd && (msg.message == WM_KEYDOWN || msg.message == WM_KEYUP ||
                           msg.message == WM_SYSKEYDOWN || msg.message == WM_SYSKEYUP)) {
            if (msg.wParam == VK_SHIFT) {
                SetOpenWith(msg.message == WM_KEYDOWN || msg.message == WM_SYSKEYDOWN);
            }
        }
        TranslateMessage(&msg);
        DispatchMessageW(&msg);
    }
    return static_cast<int>(msg.wParam);
}
