# Windows Executable

The Windows executable is a native one-click screen blanker.

## Build

Run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build-windows-exe.ps1
```

The output is:

```text
dist\OneClickClearScreen.exe
```

## Use

Double-click `OneClickClearScreen.exe`. It covers the desktop with a clean wallpaper-style layer so desktop icons and existing windows are hidden behind the app. The visible center interface is titled `一键清屏` and is about 30% of the main screen area.

Restore with either action:

- Press `Esc`.
- Click the clean desktop overlay or the center interface.

## Scope

The executable is local-only. It does not use network access, storage, telemetry, or elevated permissions.

It reads the current Windows wallpaper and desktop background color only to draw the overlay.

It does not lock Windows or hide system notifications. Use `Win+L` when you need to lock the whole device.
