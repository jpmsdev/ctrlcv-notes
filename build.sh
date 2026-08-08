#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"

ROOT="$(pwd -W 2>/dev/null || cygpath -w "$(pwd)" 2>/dev/null || pwd)"
ROOT="${ROOT//\//\\}"
ARCH="${1:-x86}"
OUT_DIR="$(pwd)/bin"
mkdir -p "${OUT_DIR}"

VCVARS='C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvarsall.bat'

TMP_CMD="$(pwd)/bin/_build_msvc.cmd"
cat > "${TMP_CMD}" <<EOF
@echo off
setlocal
call "${VCVARS}" ${ARCH}
if errorlevel 1 exit /b 1
cd /d "${ROOT}\\src"
rc /nologo /fo "${ROOT}\\bin\\app.res" app.rc
if errorlevel 1 exit /b 1
cl /nologo /O2 /EHsc /MT /utf-8 /DUNICODE /D_UNICODE /DWIN32_LEAN_AND_MEAN /D_WIN32_WINNT=0x0601 /W3 ^
  /Fe:"${ROOT}\\bin\\CtrlCVNotes.exe" main.cpp notes.cpp util.cpp "${ROOT}\\bin\\app.res" ^
  /link /SUBSYSTEM:WINDOWS comctl32.lib shell32.lib user32.lib gdi32.lib advapi32.lib
if errorlevel 1 exit /b 1
del /q main.obj notes.obj util.obj 2>nul
exit /b 0
EOF

cmd.exe //C "$(cygpath -w "${TMP_CMD}" 2>/dev/null || echo "${TMP_CMD}")"
rm -f "${TMP_CMD}"

ls -la "${OUT_DIR}/CtrlCVNotes.exe"
echo "Build OK: ${OUT_DIR}/CtrlCVNotes.exe"
