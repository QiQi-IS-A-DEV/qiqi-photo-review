# Validation suite

`PhotoFileFilter.Tests` is a Windows executable test harness because it validates WPF rendering and dispatcher behavior in addition to Core logic. It creates all fixtures under the current user's temporary folder and never reads a personal photo library.

The suite covers four groups in one deterministic run:

1. TXT parsing, scanning, path safety and copy behavior.
2. Preview decoding, cache limits, histogram and review persistence.
3. View-model workflows, keyboard behavior and localization/theme changes.
4. WPF layout rendering at desktop, laptop and compact viewport sizes.

Run it through `../scripts/Verify.ps1`. Rendered images are written to `artifacts/screenshots` for local inspection and are uploaded by CI only when validation fails.
