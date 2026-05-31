async function toggleClearScreen(tab) {
  if (!tab || !tab.id) {
    return;
  }

  try {
    await chrome.scripting.executeScript({
      target: { tabId: tab.id },
      files: ["src/content.js"]
    });
  } catch (error) {
    console.warn("One Click Clear Screen could not run on this page.", error);
  }
}

chrome.action.onClicked.addListener(toggleClearScreen);

chrome.commands.onCommand.addListener(async (command) => {
  if (command !== "toggle-clear-screen") {
    return;
  }

  const [tab] = await chrome.tabs.query({
    active: true,
    currentWindow: true
  });

  await toggleClearScreen(tab);
});
