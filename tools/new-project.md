# New Project Generator

This repository can now be used as a base template.

## One-command usage

From the repository root:

```powershell
.\create-project.cmd Trading
```

With overwrite:

```powershell
.\create-project.cmd Trading --force
```

This command will:

1. Copy current base project to a sibling folder named `Trading`.
2. Rename paths and file names from `PeriodTracker` and `periodtracker` to the new name.
3. Replace text content identifiers in solution/project/code/config/client files.
4. Skip build and local artifacts (`.vs`, `bin`, `obj`, `node_modules`, etc.).
5. Update `ConnectionStrings:DataContext` database name in server appsettings to project name.
6. Create publish scripts: `publish.cmd` and `publish.ps1`.
7. Run `dotnet restore` and `dotnet build` automatically.

## More options

```powershell
node .\tools\new-project.js Trading --dry-run
node .\tools\new-project.js Trading --destination-root D:\MyProjects
node .\tools\new-project.js Trading --force
node .\tools\new-project.js Trading --skip-build
```

## Notes

1. `ProjectName` is normalized to PascalCase for .NET namespaces and project names.
2. Lowercase variant is also replaced for client folder/file names.
3. `--dry-run` prints planned actions without writing files.
4. `create-project.cmd` requires Node.js in PATH.
