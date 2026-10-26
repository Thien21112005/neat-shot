# Changelog

All notable changes to NeatShot are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2026-10-26

### Added
- **Microsoft Store package:** MSIX manifest, Store logos and `scripts/package-msix.ps1`; when installed from the Store,
  start-with-Windows uses a Windows startup task (manageable in *Settings → Apps → Startup*).
- **Privacy policy** page (`site/privacy.html`, EN/VI) and a GitHub Pages workflow that publishes the site.
- **App icon:** the exe, tray and Store package now use the NeatShot logo instead of a generic camera.
- **Settings window** (tray → *Cài đặt…*): configurable capture hotkeys that apply instantly (keeps the old key and warns
  when another app owns the new one), save folder, PNG/JPG with quality, auto-save to the gallery, start with Windows,
  and "copy as soon as the selection is released".
- **Window detection:** the capture overlay outlines the window or control under the cursor; one click selects it.
- **Pin from the clipboard** (`F3`, configurable): pins images copied from browsers, chat apps, design tools or Explorer.
- **Delayed capture** (3 / 5 / 10 s) from the tray menu; keeps open menus and tooltips in the shot, `Esc` cancels.
- **Repeat the last region** (`Ctrl + Shift + R`, configurable, also in the tray menu): reopens the overlay with the previous region preselected, clamped to the current screen layout.
- **Landing page** in `site/` (static HTML/CSS/JS with GSAP animations, EN/VI), ready for GitHub Pages.
- Capture overlay shows a "drag to select" hint until a region is selected.
- Copying a capture shows a toast confirming the copy and where the file was saved.
- Pinned captures show a temporary "Zoom · Opacity" badge while you scroll to adjust them.
- **Start with Windows:** NeatShot registers itself to run in the tray at sign-in (on by default, like Lightshot);
  toggle it from the tray menu ("Khởi động cùng Windows").

### Changed
- Annotation toolbar redesigned to match the website: thin-line vector icons instead of emoji,
  solid accent highlight for the active tool, dark rounded bars and an icon-based action bar.
- Redesigned the text extraction result card and the Beautify preset selector.
- **Save** (button / `Ctrl + S`) writes straight to the screenshots folder and shows a toast; `Ctrl + Shift + S`
  or Shift + click opens *Save as…*.
- Settings window: light, dark or system theme, higher contrast, and a scrolling body so it always fits the screen.
- "Trích xuất chữ" is now an outlined button with an icon, clearly separate from Pin.
- Pinned captures no longer show a pencil button (clicking the image already toggles annotation).

### Fixed
- The Beautify selector could not be clicked (the WPF-UI theme shrank its hit area to ~2px).
- Saving failed with "File not found" and the gallery / "open folder" items did nothing when Windows
  *Controlled folder access* blocked `Pictures`; NeatShot now falls back to `%USERPROFILE%\NeatShot` and still shows a normal "saved" toast.
- The tray menu stayed on screen after clicking elsewhere.

## [1.0.0] - 2026-10-25

First public release.

### Added
- **Clean Shot capture** across all monitors with Per-Monitor V2 DPI awareness; dismisses tray flyouts before capturing.
- Global hotkeys `Ctrl + Shift + A` and `PrtSc`; system tray icon with capture, full-screen capture, gallery and folder shortcuts.
- **Annotation tools:** pencil, highlighter, rectangle, ellipse, line, arrow, text (font family/size), step counter, pixelate and blur; select/move/delete, undo/redo, eyedropper and persistent recent colors.
- **Beautify:** gradient background, padding, rounded corners and soft shadow with live preview.
- **Export:** copy to clipboard, save as PNG, and automatic saving of copied/pinned captures to `Pictures\NeatShot`.
- **Pin to screen:** always-on-top pins with zoom, opacity, drawing on the pin and copy of the annotated result; multiple pins at once.
- **Offline text extraction (OCR)** via `Windows.Media.Ocr`, preserving line breaks and normalizing Vietnamese diacritics.
- **Screenshot gallery** to browse, pin, copy and delete past captures.

### Fixed (pre-release audit)
- `Ctrl + O` shortcut now triggers text extraction.
- Typing in a pin's text annotation no longer triggers pin shortcuts.
- Pin annotations stay anchored to the image when zooming; beautified pins align with the original selection.
- Gallery file sizes display correctly (e.g. `2.5 MB`), and gallery pins are centered correctly on scaled displays.
- "Full-screen capture" tray item now preselects the whole screen.
- Tray icon is restored after Windows Explorer restarts.
- A click without dragging no longer adds an invisible stroke to the undo history.

[Unreleased]: https://github.com/Thien21112005/neat-shot/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/Thien21112005/neat-shot/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/Thien21112005/neat-shot/releases/tag/v1.0.0
