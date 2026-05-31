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

Double-click `OneClickClearScreen.exe`. It immediately covers all connected screens with a black privacy screen.

Restore with either action:

- Press `Esc`.
- Click the black screen.

## Scope

The executable is local-only. It does not use network access, storage, telemetry, or elevated permissions.

It does not lock Windows or hide system notifications. Use `Win+L` when you need to lock the whole device.
