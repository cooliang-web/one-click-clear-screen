# Maintenance

This project is maintained as a small privacy utility with a conservative change policy.

## Maintainer responsibilities

- Review pull requests for permission changes and content-script behavior.
- Keep the Manifest V3 extension compatible with current Chromium-based browsers.
- Triage issues with clear reproduction steps.
- Package releases from reviewed source files.
- Keep documentation aligned with the actual permission model.

## Release process

1. Update `CHANGELOG.md`.
2. Run the extension structure check.
3. Package the extension.
4. Test load the unpacked extension in Chrome or Edge.
5. Create a GitHub release with the packaged zip attached when possible.

## Manual test plan

Before a release, test:

- Toolbar button clears a normal webpage.
- Toolbar button restores after clearing.
- `Esc` restores the page.
- Clicking the blank overlay restores the page.
- The extension does not run on restricted browser pages.
- The page scroll state and layout recover after restore.

Use [TEST_MATRIX.md](TEST_MATRIX.md) to record browser coverage and release readiness.

## Roadmap policy

Roadmap items should stay focused on the one-click privacy workflow. Features that require remote services, analytics, or broad page access should be rejected unless there is a strong documented reason.
