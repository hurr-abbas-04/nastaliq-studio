# Nastaliq Studio

Nastaliq Studio is an original Windows desktop Urdu writing and exam-sheet editor. It is not an InPage clone and does not use InPage branding or assets.

## Features

- A4 Urdu writing document with an on-screen keyboard, Latin-to-Urdu mappings, and Islamic symbols.
- Selectable text formatting: font, size, bold, italic, underline, text color, and highlighting.
- Editable exam-sheet generator with title, subject, class, student details, duration, marks, instructions, and 3–8 questions with answer lines.
- Save and reopen formatted `.nastaliq` documents; export multi-page documents and exam sheets to A4 PDF.
- Native Windows resize, minimize, maximize/restore and close controls, plus visible Open, Save, and Close buttons.
- Keyboard shortcuts: Ctrl+N (new), Ctrl+O (open), Ctrl+S (save), Ctrl+P (export PDF), Ctrl+B/I/U (format text), Ctrl+Z/Y (undo/redo).

PDF export uses the Windows **Microsoft Print to PDF** printer. Documents are laid out on A4 pages; no manual print-area setup is required. Windows prompts for the PDF filename. Enable **Microsoft Print to PDF** in Windows Features if it is unavailable.

Custom keyboard mappings are saved for the current Windows user under `%LOCALAPPDATA%\NastaliqStudio`.

## Build a portable single-file executable

Run from this folder:

```powershell
dotnet publish .\NastaliqStudio.csproj -c Release
Copy-Item .\bin\Release\net10.0-windows\win-x64\publish\NastaliqStudio.exe .\NastaliqStudio-Standalone.exe -Force
```

The resulting `NastaliqStudio-Standalone.exe` is self-contained and can be launched directly from this folder with a double-click; it does not require an installer, a .NET download, or companion application files.
