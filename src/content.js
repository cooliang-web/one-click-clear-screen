(() => {
  const overlayId = "one-click-clear-screen-overlay";
  const styleId = "one-click-clear-screen-style";
  const existingOverlay = document.getElementById(overlayId);

  if (existingOverlay) {
    existingOverlay.dispatchEvent(new CustomEvent("one-click-clear-screen:restore"));
    return;
  }

  const previousOverflow = document.documentElement.style.overflow;
  document.documentElement.dataset.oneClickClearScreenPreviousOverflow = previousOverflow;

  const controller = new AbortController();
  const overlay = document.createElement("div");
  const restoreButton = document.createElement("button");
  const style = document.createElement("style");

  overlay.id = overlayId;
  overlay.setAttribute("role", "button");
  overlay.setAttribute("aria-label", "Restore page");
  overlay.tabIndex = 0;

  restoreButton.type = "button";
  restoreButton.textContent = "Restore";
  restoreButton.setAttribute("aria-label", "Restore page");

  style.id = styleId;
  style.textContent = `
    #${overlayId} {
      position: fixed;
      inset: 0;
      z-index: 2147483647;
      display: block;
      width: 100vw;
      height: 100vh;
      background: #080808;
      color: #ffffff;
      cursor: pointer;
      pointer-events: auto;
    }

    #${overlayId} button {
      position: fixed;
      right: 16px;
      bottom: 16px;
      border: 0;
      border-radius: 6px;
      padding: 10px 12px;
      background: #ffffff;
      color: #111111;
      box-shadow: 0 8px 28px rgba(0, 0, 0, 0.22);
      font: 14px/1.2 system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
      opacity: 0;
      transition: opacity 120ms ease;
    }

    #${overlayId}:hover button,
    #${overlayId}:focus-within button,
    #${overlayId} button:focus-visible {
      opacity: 0.88;
    }
  `;

  function restore() {
    const savedOverflow = document.documentElement.dataset.oneClickClearScreenPreviousOverflow;
    document.documentElement.style.overflow = savedOverflow || "";
    delete document.documentElement.dataset.oneClickClearScreenPreviousOverflow;

    document.getElementById(styleId)?.remove();
    document.getElementById(overlayId)?.remove();
    controller.abort();
  }

  overlay.addEventListener("one-click-clear-screen:restore", restore, {
    signal: controller.signal
  });

  overlay.addEventListener("click", restore, {
    signal: controller.signal
  });

  restoreButton.addEventListener("click", (event) => {
    event.stopPropagation();
    restore();
  }, {
    signal: controller.signal
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      event.preventDefault();
      restore();
    }
  }, {
    capture: true,
    signal: controller.signal
  });

  overlay.append(restoreButton);
  document.documentElement.append(style, overlay);
  document.documentElement.style.overflow = "hidden";
  overlay.focus({ preventScroll: true });
})();
