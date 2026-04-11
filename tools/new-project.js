#!/usr/bin/env node

const fs = require("node:fs");
const fsp = require("node:fs/promises");
const path = require("node:path");
const os = require("node:os");
const { spawnSync } = require("node:child_process");

function log(message) {
  process.stdout.write(`[new-project] ${message}\n`);
}

function parseArgs(argv) {
  const options = {
    force: false,
    dryRun: false,
    skipBuild: false,
    baseName: "PeriodTracker",
    sourcePath: path.resolve(__dirname, ".."),
    destinationRoot: path.dirname(path.resolve(__dirname, "..")),
    projectName: "",
  };

  const positional = [];
  const requireValue = (index, optionName) => {
    const value = argv[index + 1];
    if (!value || value.startsWith("-")) {
      throw new Error(`Missing value for ${optionName}`);
    }
    return value;
  };

  for (let i = 0; i < argv.length; i += 1) {
    const arg = argv[i];
    if (arg === "--force") {
      options.force = true;
      continue;
    }
    if (arg === "--dry-run") {
      options.dryRun = true;
      continue;
    }
    if (arg === "--skip-build") {
      options.skipBuild = true;
      continue;
    }
    if (arg === "--base-name") {
      options.baseName = requireValue(i, "--base-name");
      i += 1;
      continue;
    }
    if (arg === "--source") {
      options.sourcePath = path.resolve(requireValue(i, "--source"));
      i += 1;
      continue;
    }
    if (arg === "--destination-root") {
      options.destinationRoot = path.resolve(
        requireValue(i, "--destination-root"),
      );
      i += 1;
      continue;
    }
    if (arg === "-h" || arg === "--help") {
      options.help = true;
      continue;
    }
    positional.push(arg);
  }

  options.projectName = positional[0] || "";
  return options;
}

function normalizePathForCompare(inputPath) {
  return path
    .resolve(inputPath)
    .replace(/[\\/]+$/, "")
    .toLowerCase();
}

function isSameOrNestedPath(parentPath, childPath) {
  const parent = normalizePathForCompare(parentPath);
  const child = normalizePathForCompare(childPath);
  return (
    child === parent || child.startsWith(`${parent}${path.sep.toLowerCase()}`)
  );
}

function printHelp() {
  process.stdout.write(
    [
      "Usage:",
      "  create-project.cmd Trading",
      "  node tools/new-project.js Trading [--force] [--dry-run]",
      "",
      "Options:",
      "  --base-name <name>          Default: PeriodTracker",
      "  --source <path>             Default: current template repo",
      "  --destination-root <path>   Default: parent folder of template repo",
      "  --force                     Overwrite destination if exists",
      "  --dry-run                   Show actions without writing files",
      "  --skip-build                Skip dotnet restore/build after generation",
    ].join("\n") + "\n",
  );
}

function toPascalCase(input) {
  const parts = String(input)
    .trim()
    .replace(/[^a-zA-Z0-9]+/g, " ")
    .split(/\s+/)
    .filter(Boolean);

  if (parts.length === 0) {
    throw new Error("Project name is invalid.");
  }

  return parts
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join("");
}

function applyReplacements(input, replacements) {
  let output = input;
  for (const [from, to] of replacements) {
    output = output.split(from).join(to);
  }
  return output;
}

const IGNORED_DIRS = new Set([
  ".git",
  ".vs",
  ".artifacts",
  "bin",
  "obj",
  "node_modules",
  "dist",
  "coverage",
]);

const IGNORED_FILE_REGEX = [
  /^.*\.suo$/i,
  /^.*\.user$/i,
  /^.*\.cache$/i,
  /^npm-debug\.log.*$/i,
  /^yarn-debug\.log.*$/i,
  /^yarn-error\.log.*$/i,
  /^pnpm-debug\.log.*$/i,
];

const TEXT_EXTENSIONS = new Set([
  ".sln",
  ".cs",
  ".csproj",
  ".esproj",
  ".props",
  ".targets",
  ".json",
  ".xml",
  ".config",
  ".http",
  ".md",
  ".txt",
  ".js",
  ".jsx",
  ".ts",
  ".tsx",
  ".css",
  ".scss",
  ".html",
  ".yml",
  ".yaml",
  ".sql",
  ".editorconfig",
  ".gitignore",
  ".cmd",
  ".bat",
  ".ps1",
  ".sh",
  ".env",
]);

function hasIgnoredDirSegment(relativePath) {
  if (!relativePath) {
    return false;
  }
  const segments = relativePath.split(path.sep).filter(Boolean);
  return segments.some((segment) => IGNORED_DIRS.has(segment));
}

function isIgnoredFileName(name) {
  return IGNORED_FILE_REGEX.some((re) => re.test(name));
}

function isLikelyBinary(buffer) {
  const sampleLength = Math.min(buffer.length, 8000);
  for (let i = 0; i < sampleLength; i += 1) {
    if (buffer[i] === 0) {
      return true;
    }
  }
  return false;
}

async function walk(root) {
  const result = [];
  const stack = [root];
  while (stack.length > 0) {
    const current = stack.pop();
    const entries = await fsp.readdir(current, { withFileTypes: true });
    for (const entry of entries) {
      const fullPath = path.join(current, entry.name);
      result.push(fullPath);
      if (entry.isDirectory()) {
        stack.push(fullPath);
      }
    }
  }
  return result;
}

function updateConnectionStringDatabase(connectionString, databaseName) {
  let value = connectionString;

  if (/Database\s*=\s*[^;]+/i.test(value)) {
    value = value.replace(/Database\s*=\s*[^;]+/i, `Database=${databaseName}`);
    return value;
  }

  if (/Initial Catalog\s*=\s*[^;]+/i.test(value)) {
    value = value.replace(
      /Initial Catalog\s*=\s*[^;]+/i,
      `Initial Catalog=${databaseName}`,
    );
    return value;
  }

  if (value.endsWith(";")) {
    return `${value}Database=${databaseName}`;
  }

  return `${value};Database=${databaseName}`;
}

async function updateAppSettingsDatabaseName(workingPath, projectName) {
  const serverFolder = path.join(workingPath, `${projectName}.Server`);
  const candidates = [
    path.join(serverFolder, "appsettings.json"),
    path.join(serverFolder, "appsettings.Development.json"),
  ];

  let updatedCount = 0;
  for (const filePath of candidates) {
    if (!fs.existsSync(filePath)) {
      continue;
    }

    const raw = await fsp.readFile(filePath, "utf8");
    let json;
    try {
      json = JSON.parse(raw);
    } catch {
      log(`Warning: skipped invalid JSON file: ${filePath}`);
      continue;
    }

    const conn = json?.ConnectionStrings?.DataContext;
    if (typeof conn !== "string" || conn.trim().length === 0) {
      continue;
    }

    const updated = updateConnectionStringDatabase(conn, projectName);
    if (updated === conn) {
      continue;
    }

    json.ConnectionStrings.DataContext = updated;
    await fsp.writeFile(filePath, `${JSON.stringify(json, null, 2)}\n`, "utf8");
    updatedCount += 1;
  }

  return updatedCount;
}

async function createPublishScripts(workingPath, projectName) {
  const publishCmdPath = path.join(workingPath, "publish.cmd");
  const publishPs1Path = path.join(workingPath, "publish.ps1");

  const publishCmd = [
    "@echo off",
    "setlocal",
    `dotnet publish .\\${projectName}.Server\\${projectName}.Server.csproj -c Release -o .\\.artifacts\\publish\\${projectName}.Server`,
    "exit /b %ERRORLEVEL%",
    "",
  ].join("\r\n");

  const publishPs1 = [
    "param(",
    '    [string]$Configuration = "Release"',
    ")",
    "",
    '$ErrorActionPreference = "Stop"',
    `dotnet publish \"./${projectName}.Server/${projectName}.Server.csproj\" -c $Configuration -o \"./.artifacts/publish/${projectName}.Server\"`,
    "",
  ].join("\n");

  await fsp.writeFile(publishCmdPath, publishCmd, "utf8");
  await fsp.writeFile(publishPs1Path, publishPs1, "utf8");
}

function runDotnetCommand(args, cwd) {
  const commandText = `dotnet ${args.join(" ")}`;
  log(`Running: ${commandText}`);
  const result = spawnSync("dotnet", args, {
    cwd,
    stdio: "inherit",
    shell: false,
  });

  if (result.status !== 0) {
    throw new Error(`Command failed: ${commandText}`);
  }
}

async function main() {
  const options = parseArgs(process.argv.slice(2));
  if (options.help) {
    printHelp();
    return;
  }

  if (!options.projectName) {
    printHelp();
    throw new Error("Missing required argument: <projectName>");
  }

  const baseName = toPascalCase(options.baseName);
  const newName = toPascalCase(options.projectName);
  if (baseName.toLowerCase() === newName.toLowerCase()) {
    throw new Error("Project name must be different from base name.");
  }

  const sourcePath = path.resolve(options.sourcePath);
  const destinationRoot = path.resolve(options.destinationRoot);
  const destinationPath = path.join(destinationRoot, newName);

  if (!fs.existsSync(sourcePath)) {
    throw new Error(`Source path does not exist: ${sourcePath}`);
  }
  if (!fs.existsSync(destinationRoot)) {
    throw new Error(`Destination root does not exist: ${destinationRoot}`);
  }

  if (isSameOrNestedPath(sourcePath, destinationPath)) {
    throw new Error(
      "Unsafe destination: destination cannot be the source path or inside source path.",
    );
  }

  if (isSameOrNestedPath(destinationPath, sourcePath)) {
    throw new Error(
      "Unsafe destination: source path cannot be inside destination path.",
    );
  }

  const baseLower = baseName.toLowerCase();
  const newLower = newName.toLowerCase();
  const replacements = [
    [`${baseLower}.client`, `${newLower}.client`],
    [`${baseName}.client`, `${newName}.client`],
    [baseName, newName],
    [baseLower, newLower],
    [baseName.toUpperCase(), newName.toUpperCase()],
  ];

  log(`Source: ${sourcePath}`);
  log(`Destination: ${destinationPath}`);
  log(`Rename: ${baseName} -> ${newName}`);

  if (fs.existsSync(destinationPath)) {
    if (!options.force) {
      throw new Error(
        `Destination already exists: ${destinationPath}. Use --force.`,
      );
    }
    if (options.dryRun) {
      log(`[DryRun] Would remove existing destination: ${destinationPath}`);
    } else {
      await fsp.rm(destinationPath, { recursive: true, force: true });
    }
  }

  let workingPath = destinationPath;
  if (options.dryRun) {
    const tempFolderName = `scaffold-dryrun-${Date.now()}-${newName}`;
    workingPath = path.join(os.tmpdir(), tempFolderName);
    await fsp.rm(workingPath, { recursive: true, force: true });
    log(`[DryRun] Using temporary staging path: ${workingPath}`);
  }

  await fsp.mkdir(workingPath, { recursive: true });
  await fsp.cp(sourcePath, workingPath, {
    recursive: true,
    force: true,
    filter: (src) => {
      const rel = path.relative(sourcePath, src);
      if (!rel || rel === "") {
        return true;
      }

      if (hasIgnoredDirSegment(rel)) {
        return false;
      }

      const stat = fs.statSync(src);
      if (stat.isFile() && isIgnoredFileName(path.basename(src))) {
        return false;
      }

      return true;
    },
  });

  const allItems = await walk(workingPath);
  allItems.sort((a, b) => b.length - a.length);

  let renamedPaths = 0;
  for (const itemPath of allItems) {
    const rel = path.relative(workingPath, itemPath);
    if (hasIgnoredDirSegment(rel)) {
      continue;
    }

    const currentName = path.basename(itemPath);
    const nextName = applyReplacements(currentName, replacements);
    if (currentName === nextName) {
      continue;
    }

    const targetPath = path.join(path.dirname(itemPath), nextName);
    if (fs.existsSync(targetPath)) {
      throw new Error(`Rename collision: ${targetPath}`);
    }

    if (options.dryRun) {
      log(`[DryRun] Rename: ${itemPath} -> ${targetPath}`);
    }
    await fsp.rename(itemPath, targetPath);
    renamedPaths += 1;
  }

  const destinationItems = await walk(workingPath);
  let updatedFiles = 0;

  for (const itemPath of destinationItems) {
    const rel = path.relative(workingPath, itemPath);
    if (hasIgnoredDirSegment(rel)) {
      continue;
    }

    const stat = await fsp.stat(itemPath);
    if (!stat.isFile()) {
      continue;
    }

    if (isIgnoredFileName(path.basename(itemPath))) {
      continue;
    }

    const ext = path.extname(itemPath);
    if (!TEXT_EXTENSIONS.has(ext)) {
      continue;
    }

    const buffer = await fsp.readFile(itemPath);
    if (isLikelyBinary(buffer)) {
      continue;
    }

    const original = buffer.toString("utf8");
    const replaced = applyReplacements(original, replacements);
    if (original === replaced) {
      continue;
    }

    if (options.dryRun) {
      log(`[DryRun] Update content: ${itemPath}`);
    }
    await fsp.writeFile(itemPath, replaced, "utf8");
    updatedFiles += 1;
  }

  let leftoverNames = [];
  let leftoverContent = [];

  {
    const postItems = await walk(workingPath);
    leftoverNames = postItems.filter((itemPath) => {
      const name = path.basename(itemPath).toLowerCase();
      return name.includes(baseLower);
    });

    for (const itemPath of postItems) {
      const stat = await fsp.stat(itemPath);
      if (!stat.isFile()) {
        continue;
      }

      const rel = path.relative(workingPath, itemPath);
      if (hasIgnoredDirSegment(rel)) {
        continue;
      }

      const ext = path.extname(itemPath);
      if (!TEXT_EXTENSIONS.has(ext)) {
        continue;
      }

      const content = (await fsp.readFile(itemPath, "utf8")).toLowerCase();
      const hasBase = content.includes(baseLower);
      if (hasBase) {
        leftoverContent.push(itemPath);
      }
    }
  }

  log(`Paths renamed: ${renamedPaths}`);
  log(`Files updated: ${updatedFiles}`);

  log(`Leftover names: ${leftoverNames.length}`);
  log(`Leftover content files: ${leftoverContent.length}`);

  if (leftoverNames.length > 0 || leftoverContent.length > 0) {
    throw new Error(
      "Detected unresolved base-name leftovers after replacement.",
    );
  }

  const newSolutionPath = path.join(workingPath, `${newName}.sln`);
  if (fs.existsSync(newSolutionPath)) {
    const smoke = spawnSync("dotnet", ["sln", newSolutionPath, "list"], {
      stdio: "ignore",
      shell: false,
    });
    if (smoke.status === 0) {
      log("Smoke check passed: dotnet sln list");
    } else {
      log("Smoke check warning: dotnet sln list failed");
    }
  }

  const appSettingsUpdated = await updateAppSettingsDatabaseName(
    workingPath,
    newName,
  );
  log(`AppSettings DB name updated: ${appSettingsUpdated}`);

  await createPublishScripts(workingPath, newName);
  log("Publish scripts generated: publish.cmd, publish.ps1");

  if (!options.dryRun && !options.skipBuild) {
    runDotnetCommand(["restore", `${newName}.sln`], workingPath);
    runDotnetCommand(["build", `${newName}.sln`, "-v", "minimal"], workingPath);
  } else if (options.skipBuild) {
    log("Skipped dotnet restore/build because --skip-build was provided.");
  }

  if (options.dryRun) {
    await fsp.rm(workingPath, { recursive: true, force: true });
    log("Dry-run completed. No files were written to destination.");
  } else {
    log(`Completed. New project: ${workingPath}`);
  }
}

main().catch((error) => {
  process.stderr.write(`[new-project] ERROR: ${error.message}\n`);
  process.exitCode = 1;
});
