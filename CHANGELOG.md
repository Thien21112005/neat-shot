# Changelog

All notable changes to NeatShot are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
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

### Fixed
- The Beautify selector could not be clicked (the WPF-UI theme shrank its hit area to ~2px).

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

[1.0.0]: https://github.com/Thien21112005/neat-shot/releases/tag/v1.0.0
