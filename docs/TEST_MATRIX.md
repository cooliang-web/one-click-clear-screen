# Test Matrix

This matrix defines the manual checks required before a release. The project is intentionally small, so the first quality gate is a repeatable browser test pass rather than a heavy automated suite.

## Current Status

| Area | Chrome | Edge | Notes |
| --- | --- | --- | --- |
| Extension structure check | Pass | Not run | Validated locally with the manifest and required files present. |
| Package generation | Pass | Not run | `dist/one-click-clear-screen.zip` generated from reviewed source files. |
| Load unpacked extension | Pending | Pending | Run from `chrome://extensions` or `edge://extensions`. |
| Toolbar button clears normal webpage | Pending | Pending | Use any standard HTTPS page. |
| Toolbar button restores after clearing | Pending | Pending | Click the extension button a second time. |
| Blank overlay click restores page | Pending | Pending | Click anywhere on the overlay. |
| `Esc` restores page | Pending | Pending | Press `Esc` while the overlay is focused. |
| Keyboard shortcut toggles overlay | Pending | Pending | Default is `Ctrl+Shift+K` or `Command+Shift+K`. |
| Restricted browser page behavior | Pending | Pending | Browser internal pages should fail safely. |
| Page overflow recovers after restore | Pending | Pending | Check that scrolling works after restore. |

## Test Cases

### Normal Webpage Clear And Restore

1. Load the unpacked extension.
2. Open a normal webpage such as `https://example.com`.
3. Click the extension button.
4. Confirm the page is covered by a blank screen.
5. Click the extension button again.
6. Confirm the original page is visible and usable.

Expected result: the overlay appears and then fully restores.

### Overlay Click Restore

1. Clear a normal webpage.
2. Click the blank overlay.
3. Confirm the original page is visible and usable.

Expected result: the overlay is removed.

### Escape Key Restore

1. Clear a normal webpage.
2. Press `Esc`.
3. Confirm the original page is visible and usable.

Expected result: the overlay is removed.

### Keyboard Shortcut

1. Confirm the extension shortcut at `chrome://extensions/shortcuts`.
2. Open a normal webpage.
3. Press the shortcut.
4. Confirm the overlay appears.
5. Press `Esc`.
6. Confirm the overlay is removed.

Expected result: shortcut toggles clear screen without additional permissions.

### Restricted Page Handling

1. Open a restricted browser page such as `chrome://extensions`.
2. Click the extension button.
3. Confirm the extension does not break the page.

Expected result: browser restrictions prevent injection, and the page remains usable.

### Restore Layout And Scroll

1. Open a long webpage.
2. Scroll down.
3. Click the extension button.
4. Restore with `Esc`.
5. Confirm page scrolling still works.

Expected result: document overflow is restored.

## Release Rule

A release should not be published as stable unless the Chrome column is passing for the current source. Edge should be checked before claiming Edge support in release notes.
