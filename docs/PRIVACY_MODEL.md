# Privacy Model

One Click Clear Screen is designed as a local-only browser action.

## Data collection

The extension does not collect, store, transmit, or analyze page content.

## Network access

The extension does not make network requests.

## Permissions

The extension uses:

- `activeTab`: allows the extension to act on the current tab only after a user action.
- `scripting`: allows the extension to inject the temporary clear-screen overlay.

It intentionally avoids broad host permissions such as `<all_urls>`.

## Runtime behavior

When the user clicks the extension button or uses the shortcut, the extension injects a content script into the active tab. The content script adds a full-page overlay and removes it when the user restores the page.

The extension does not persist state across browser restarts and does not use extension storage.

## Review checklist

Before accepting changes, maintainers should check:

- No new network calls are introduced.
- No broad host permissions are added without a documented reason.
- No page content is stored or logged.
- Restore behavior remains available through click and `Esc`.
- Content-script changes do not break common page layouts.
