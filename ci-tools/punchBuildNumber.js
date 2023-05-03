const fs = require("fs");

function validate(value, message) {
    if (value === undefined) {
        console.log(message);
        process.exit(1);
    }
}

function getFormattedDate() {
    const currentDate = new Date();
    const year = currentDate.getUTCFullYear();
    const month = ('0' + (currentDate.getUTCMonth() + 1)).slice(-2);
    const day = ('0' + currentDate.getUTCDate()).slice(-2);
    const hours = ('0' + currentDate.getUTCHours()).slice(-2);
    const minutes = ('0' + currentDate.getUTCMinutes()).slice(-2);
    const seconds = ('0' + currentDate.getUTCSeconds()).slice(-2);
    return `${year}${month}${day}-${hours}${minutes}${seconds}`;
}

function ChangeVersion(file, version, name, regex)
{
    let modified = false;

    let content = fs.readFileSync(file, "utf8");
    validate(content, `Failed to read file: ${file}`);

    let lines = content.split("\n");
    for (let i = 0; i < lines.length; i++) {
        let line = lines[i];
        let result = line.match(regex);

        if (result != null) {
            let newLine = line.replace(result.groups.version, version);

            if (result.groups.name != null)
            {
                newLine = newLine.replace(result.groups.name, name);
            }

            console.log(`Bumping version from '${line}' to '${newLine}' in '${file}'...`);
            lines[i] = newLine;
            modified = true;
            break;
        }
    }

    if (modified) {
        fs.writeFileSync(file, lines.join("\n"));
    } else {
        console.log(`Unable to change version in '${file}'.`);
    }
}

function main() {
    let args = process.argv.slice(2);
    let root = args[0];
    let name = args[1];
    let version = args[2];

    try {
        validate(version, 'Missing required argument version file');

        // AssemblyInfo.cs
        ChangeVersion(root + "/ProjectSettings/ProjectSettings.asset", `${version}.${getFormattedDate()}`, name,
            new RegExp(/bundleVersion: (?<version>.+)/));

        // ProjectSettings.asset
        ChangeVersion(root + "/Assets/_Application/AssemblyInfo.cs", version, name,
            new RegExp(/assembly: ApiSourceVersion[ ]*\([ ]*\"(?<name>[^\"]+)\"[ ]*,[ ]*\"(?<version>[^\"]+)/));
    }
    catch (e) {
        console.log(`Unable to increment version: ${e}`);
        process.exit(1);
    }
}

main();

