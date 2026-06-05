# One Click Clear Screen

One Click Clear Screen is a tiny browser extension by [cooliang-web](https://github.com/cooliang-web) that blanks the current tab instantly. Click the extension button once to cover the page with a clean screen, then click again or press `Esc` to restore the page.

## Why it exists

People often need a fast privacy switch when sharing a screen, working in public, recording demos, or stepping away from a browser tab. This project keeps that action simple: one click, no account, no tracking, no page data collection.

## Features

- One-click clear screen for the active tab
- Restore by clicking the extension button again
- Restore by clicking the blank screen or pressing `Esc`
- Optional keyboard shortcut: `Ctrl+Shift+K` on Windows/Linux, `Command+Shift+K` on macOS
- No remote server and no analytics

## Install locally

1. Download or clone this repository.

   ```bash
   git clone https://github.com/cooliang-web/one-click-clear-screen.git
   ```

2. Open `chrome://extensions`.
3. Enable **Developer mode**.
4. Click **Load unpacked**.
5. Select this project folder.

The extension should appear in your browser toolbar. Pin it if you want one-click access.

See [docs/USAGE.md](docs/USAGE.md) for usage notes and limitations.

## Browser support

The extension is built with Manifest V3 and targets Chromium-based browsers, including Chrome and Edge.

## Windows executable

This repository also includes a Windows executable version. It covers the desktop with a clean wallpaper-style layer so icons and windows are hidden, while showing a compact center interface titled `一键清屏`. Restore on `Esc` or click. See [docs/WINDOWS_EXE.md](docs/WINDOWS_EXE.md).

## Privacy

This extension only injects a temporary blank overlay into the active tab after you click the toolbar button or use the shortcut. It does not read, store, transmit, or analyze page content.

See [docs/PRIVACY_MODEL.md](docs/PRIVACY_MODEL.md) for the permission and privacy model.

## Security

The extension intentionally avoids broad host permissions. It uses `activeTab` and `scripting` so it can run only after a user action on the current tab. See [SECURITY.md](SECURITY.md) for the security model.

## Development

The project has no build step.

```text
manifest.json        Extension metadata and permissions
src/background.js    Toolbar button and shortcut handling
src/content.js       Temporary clear-screen overlay
windows/             Windows executable source
```

See [docs/MAINTENANCE.md](docs/MAINTENANCE.md) for release and review guidance.
See [docs/TEST_MATRIX.md](docs/TEST_MATRIX.md) for the manual browser test matrix.

## Roadmap

- Custom blank-screen color
- Configurable restore behavior
- Firefox support
- Small toolbar status indicator

## License

MIT
