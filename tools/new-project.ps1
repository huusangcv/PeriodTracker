param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$ProjectName,

    [string]$BaseName = "PeriodTracker",

    [string]$SourcePath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,

    [string]$DestinationRoot = (Split-Path (Resolve-Path (Join-Path $PSScriptRoot "..")).Path -Parent),

    [switch]$Force,

    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "[new-project] $Message"
}

function Convert-ToPascalCase {
    param([string]$Value)

    $parts = ($Value -replace "[^A-Za-z0-9]+", " ").Trim().Split(" ", [System.StringSplitOptions]::RemoveEmptyEntries)
    if ($parts.Count -eq 0) {
        throw "ProjectName is invalid."
    }

    return ($parts | ForEach-Object {
            if ($_.Length -eq 1) {
                $_.ToUpperInvariant()
            }
            else {
                $_.Substring(0, 1).ToUpperInvariant() + $_.Substring(1)
            }
        }) -join ""
}

function Get-RelativePath {
    param(
        [string]$Root,
        [string]$Path
    )

    if (-not $Path.StartsWith($Root, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $Path
    }

    $relative = $Path.Substring($Root.Length)
    return $relative.TrimStart('\\', '/')
}

function Should-SkipDirectory {
    param(
        [string]$Path,
        [string[]]$IgnoredDirectories
    )

    $segments = $Path -split "[\\/]"
    foreach ($segment in $segments) {
        if ($IgnoredDirectories -contains $segment) {
            return $true
        }
    }

    return $false
}

function Should-SkipFile {
    param(
        [string]$Path,
        [string[]]$IgnoredDirectories,
        [string[]]$IgnoredFilePatterns
    )

    if (Should-SkipDirectory -Path $Path -IgnoredDirectories $IgnoredDirectories) {
        return $true
    }

    $name = [System.IO.Path]::GetFileName($Path)
    foreach ($pattern in $IgnoredFilePatterns) {
        if ($name -like $pattern) {
            return $true
        }
    }

    return $false
}

function Get-TextExtensions {
    return @(
        ".sln", ".cs", ".csproj", ".esproj", ".props", ".targets",
        ".json", ".xml", ".config", ".http", ".md", ".txt",
        ".js", ".jsx", ".ts", ".tsx", ".css", ".scss", ".html",
        ".yml", ".yaml", ".sql", ".editorconfig", ".gitignore",
        ".cmd", ".bat", ".ps1", ".sh", ".env"
    )
}

function Apply-NameReplacements {
    param(
        [string]$Input,
        [System.Collections.Specialized.OrderedDictionary]$Map
    )

    $output = $Input
    foreach ($key in $Map.Keys) {
        $output = $output.Replace([string]$key, [string]$Map[$key])
    }
    return $output
}

$newName = Convert-ToPascalCase -Value $ProjectName.Trim()
$baseNameNormalized = Convert-ToPascalCase -Value $BaseName.Trim()

if ($newName.Equals($baseNameNormalized, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "ProjectName must be different from BaseName."
}

$baseLower = $baseNameNormalized.ToLowerInvariant()
$newLower = $newName.ToLowerInvariant()

$sourceRoot = (Resolve-Path -LiteralPath $SourcePath).Path
$destinationRootResolved = (Resolve-Path -LiteralPath $DestinationRoot).Path
$destinationPath = Join-Path $destinationRootResolved $newName

$nameMap = [ordered]@{}
$nameMap.Add("$baseLower.client", "$newLower.client")
$nameMap.Add("$baseNameNormalized.client", "$newName.client")
$nameMap.Add($baseNameNormalized, $newName)
$nameMap.Add($baseLower, $newLower)
$nameMap.Add($baseNameNormalized.ToUpperInvariant(), $newName.ToUpperInvariant())

$ignoredDirectories = @(".git", ".vs", "bin", "obj", "node_modules", "dist", "coverage")
$ignoredFilePatterns = @("*.suo", "*.user", "*.cache", "npm-debug.log*", "yarn-debug.log*", "yarn-error.log*", "pnpm-debug.log*")

Write-Step "Source: $sourceRoot"
Write-Step "Destination: $destinationPath"
Write-Step "Rename: $baseNameNormalized -> $newName"

if (Test-Path -LiteralPath $destinationPath) {
    if (-not $Force) {
        throw "Destination already exists: $destinationPath. Use -Force to overwrite."
    }

    if ($DryRun) {
        Write-Step "[DryRun] Would remove existing destination: $destinationPath"
    }
    else {
        Write-Step "Removing existing destination because -Force was provided"
        Remove-Item -LiteralPath $destinationPath -Recurse -Force
    }
}

if ($DryRun) {
    Write-Step "[DryRun] Would create destination folder"
}
else {
    New-Item -ItemType Directory -Path $destinationPath -Force | Out-Null
}

Write-Step "Copying template (excluding build and IDE artifacts)"

$allDirectories = Get-ChildItem -LiteralPath $sourceRoot -Directory -Recurse -Force
foreach ($directory in $allDirectories) {
    if (Should-SkipDirectory -Path $directory.FullName -IgnoredDirectories $ignoredDirectories) {
        continue
    }

    $relative = Get-RelativePath -Root $sourceRoot -Path $directory.FullName
    $targetDirectory = Join-Path $destinationPath $relative
    if ($DryRun) {
        Write-Step "[DryRun] Create directory: $targetDirectory"
    }
    else {
        New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
    }
}

$copiedFiles = 0
$allFiles = Get-ChildItem -LiteralPath $sourceRoot -File -Recurse -Force
foreach ($file in $allFiles) {
    if (Should-SkipFile -Path $file.FullName -IgnoredDirectories $ignoredDirectories -IgnoredFilePatterns $ignoredFilePatterns) {
        continue
    }

    $relative = Get-RelativePath -Root $sourceRoot -Path $file.FullName
    $targetFile = Join-Path $destinationPath $relative
    if ($DryRun) {
        Write-Step "[DryRun] Copy file: $targetFile"
    }
    else {
        Copy-Item -LiteralPath $file.FullName -Destination $targetFile -Force
    }
    $copiedFiles++
}

Write-Step "Files copied: $copiedFiles"

Write-Step "Renaming folders and files"

$renamedPaths = 0
$itemsToRename = Get-ChildItem -LiteralPath $destinationPath -Recurse -Force |
    Sort-Object { $_.FullName.Length } -Descending

foreach ($item in $itemsToRename) {
    if (Should-SkipDirectory -Path $item.FullName -IgnoredDirectories $ignoredDirectories) {
        continue
    }

    $newItemName = Apply-NameReplacements -Input $item.Name -Map $nameMap
    if ($newItemName -eq $item.Name) {
        continue
    }

    $parentPath = Split-Path -Path $item.FullName -Parent
    $newPath = Join-Path $parentPath $newItemName

    if (Test-Path -LiteralPath $newPath) {
        throw "Rename collision detected: $newPath"
    }

    if ($DryRun) {
        Write-Step "[DryRun] Rename '$($item.FullName)' -> '$newPath'"
    }
    else {
        Rename-Item -LiteralPath $item.FullName -NewName $newItemName
    }
    $renamedPaths++
}

Write-Step "Paths renamed: $renamedPaths"

Write-Step "Replacing text content identifiers"

$textExtensions = Get-TextExtensions
$updatedFiles = 0

$destinationFiles = Get-ChildItem -LiteralPath $destinationPath -File -Recurse -Force
foreach ($file in $destinationFiles) {
    if (Should-SkipFile -Path $file.FullName -IgnoredDirectories $ignoredDirectories -IgnoredFilePatterns $ignoredFilePatterns) {
        continue
    }

    $extension = [System.IO.Path]::GetExtension($file.Name)
    if ($textExtensions -notcontains $extension) {
        continue
    }

    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    if ($bytes.Length -gt 0 -and ($bytes -contains 0)) {
        continue
    }

    $content = [System.IO.File]::ReadAllText($file.FullName)
    $newContent = Apply-NameReplacements -Input $content -Map $nameMap

    if ($newContent -eq $content) {
        continue
    }

    if ($DryRun) {
        Write-Step "[DryRun] Update content: $($file.FullName)"
    }
    else {
        [System.IO.File]::WriteAllText($file.FullName, $newContent, [System.Text.UTF8Encoding]::new($false))
    }
    $updatedFiles++
}

Write-Step "Files updated: $updatedFiles"

Write-Step "Verifying unresolved name leftovers"

$leftoverNamePaths = Get-ChildItem -LiteralPath $destinationPath -Recurse -Force |
    Where-Object {
        $_.Name.Contains($baseNameNormalized, [System.StringComparison]::Ordinal) -or
        $_.Name.Contains($baseLower, [System.StringComparison]::Ordinal)
    }

$leftoverContentMatches = @()
if (-not $DryRun) {
    $scanFiles = Get-ChildItem -LiteralPath $destinationPath -File -Recurse -Force | Where-Object {
        -not (Should-SkipFile -Path $_.FullName -IgnoredDirectories $ignoredDirectories -IgnoredFilePatterns $ignoredFilePatterns)
    }

    foreach ($scanFile in $scanFiles) {
        $extension = [System.IO.Path]::GetExtension($scanFile.Name)
        if ($textExtensions -notcontains $extension) {
            continue
        }

        try {
            $matches = Select-String -Path $scanFile.FullName -Pattern $baseNameNormalized, $baseLower -SimpleMatch
            if ($matches) {
                $leftoverContentMatches += $matches
            }
        }
        catch {
            continue
        }
    }
}

if ($leftoverNamePaths.Count -gt 0) {
    Write-Step "Warning: leftover names found ($($leftoverNamePaths.Count))."
    $leftoverNamePaths | Select-Object -First 20 | ForEach-Object { Write-Host "  - $($_.FullName)" }
}
else {
    Write-Step "No leftover path names found."
}

if (-not $DryRun) {
    if ($leftoverContentMatches.Count -gt 0) {
        Write-Step "Warning: leftover content matches found ($($leftoverContentMatches.Count))."
        $leftoverContentMatches | Select-Object -First 20 | ForEach-Object { Write-Host "  - $($_.Path):$($_.LineNumber)" }
    }
    else {
        Write-Step "No leftover content matches found."
    }
}

$newSolutionPath = Join-Path $destinationPath "$newName.sln"
if (-not $DryRun -and (Test-Path -LiteralPath $newSolutionPath)) {
    Write-Step "Smoke check: dotnet sln list"
    dotnet sln $newSolutionPath list | Out-Null
    Write-Step "Smoke check passed."
}

if ($DryRun) {
    Write-Step "Dry-run completed. No files were changed."
}
else {
    Write-Step "Project scaffolding completed."
    Write-Step "New project folder: $destinationPath"
}
