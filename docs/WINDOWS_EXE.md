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

Double-click `OneClickClearScreen.exe`. It immediately covers all connected screens with a desktop-background overlay. Desktop icons and existing windows stay behind the overlay, so the screen looks clean without turning black.

Restore with either action:

- Press `Esc`.
- Click the clean desktop overlay.

## Scope

The executable is local-only. It does not use network access, storage, telemetry, or elevated permissions.

It reads the current Windows wallpaper and desktop background color only to draw the overlay.

It does not lock Windows or hide system notifications. Use `Win+L` when you need to lock the whole device.
