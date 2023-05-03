const fs = require("fs");

function validate(value, message) {
    if (value === undefined) {
        console.log(message);
        process.exit(1);
    }
}

function main() {
    let args = process.argv.slice(2);
    let versionFile = args[0];

    try {
        validate(versionFile, `Missing required argument version file`);

        let content = fs.readFileSync(versionFile, "utf8");
        validate(content, `Failed to read file: ${versionFile}`);

        let modified = false;
        let reg = /(?<header>^.*bundleVersion:.+\.)(?<buildNumber>\d+)$/;
    
        let lines = content.split("\n");
        for (let i = 0; i < lines.length; i++) {
            let line = lines[i];
            let result = line.match(reg);

            if (result != null) {
                let n = parseInt(result.groups.buildNumber);
                n = n + 1;

                let newLine = result.groups.header + n;

                console.log(`Bumping version from '${line}' to '${newLine}'...`);
                lines[i] = newLine;
                modified = true;
                break;
            }
        }

        if (modified) {
            fs.writeFileSync(versionFile, lines.join("\n"));
        }
    }
    catch (e) {
        console.log(`Unable to increment version in file ${versionFile}: ${e}`);
        process.exit(1);
    }
}

main();

