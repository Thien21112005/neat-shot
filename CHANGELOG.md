# Changelog

All notable changes to NeatShot are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
