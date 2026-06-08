const fs = require("fs");
const path = require("path");
let checkCode;
try {
    const checker = require("./checker.js");
    checkCode = checker.checkCode;
} catch (e) {
    console.error("Failed to load checker:", e.message);
    process.exit(1);
}

const filesToCheck = [
    path.join(__dirname, "..", "EXAMPLE_ALL_FEATURES.a"),
    path.join(__dirname, "..", "EXAMPLE_ARABIC.a"),
    path.join(__dirname, "..", "EXAMPLE_ENGLISH.a")
];

let hasError = false;
for (const file of filesToCheck) {
    console.log("\nChecking " + path.basename(file) + "...");
    try {
        const code = fs.readFileSync(file, "utf8");
        const errors = checkCode(code);
        if (errors.length > 0) {
            hasError = true;
            console.log("ERRORS:", errors.length);
            errors.forEach(e => console.log("  L" + e.line + ":" + e.column + " " + e.message));
        } else {
            console.log("OK");
        }
    } catch (e) {
        hasError = true;
        console.error("FAILED:", e.message);
    }
}

if (hasError) process.exit(1);
else console.log("\nAll files validated successfully!");
