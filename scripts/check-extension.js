const fs = require("fs");
const path = require("path");

const root = path.resolve(__dirname, "..");
const manifestPath = path.join(root, "manifest.json");
const requiredFiles = [
  "README.md",
  "LICENSE",
  "manifest.json",
  "src/background.js",
  "src/content.js"
];

for (const file of requiredFiles) {
  const fullPath = path.join(root, file);
  if (!fs.existsSync(fullPath)) {
    throw new Error(`Missing required file: ${file}`);
  }
}

const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));

if (manifest.manifest_version !== 3) {
  throw new Error("manifest_version must be 3");
}

if (!manifest.action || !manifest.background || !manifest.permissions) {
  throw new Error("Manifest is missing action, background, or permissions");
}

if (!manifest.permissions.includes("activeTab")) {
  throw new Error("Manifest must include activeTab permission");
}

if (!manifest.permissions.includes("scripting")) {
  throw new Error("Manifest must include scripting permission");
}

console.log("Extension structure looks good.");
