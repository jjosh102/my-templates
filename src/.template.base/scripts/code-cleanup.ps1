param(
  [string]$Solution = "",

  [switch]$Full,

  [switch]$VerifyOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ScriptRoot = $PSScriptRoot
$RepoRoot = [System.IO.Path]::GetFullPath((Join-Path $ScriptRoot ".."))

if ([string]::IsNullOrWhiteSpace($Solution)) {
  $Solution = Join-Path $RepoRoot "MyToolkit.sln"
}
elseif (-not [System.IO.Path]::IsPathRooted($Solution)) {
  $Solution = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Solution))
}

$StartedAt = Get-Date
$Script:StepNumber = 0

function Write-Line {
  param([string]$Text = "")
  Write-Host $Text
}

function Write-Title {
  param([string]$Text)

  Write-Line ""
  Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
  Write-Host "  $Text" -ForegroundColor Cyan
  Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
}

function Write-Step {
  param([string]$Text)

  $Script:StepNumber++
  Write-Line ""
  Write-Host ("[{0:00}] " -f $Script:StepNumber) -NoNewline -ForegroundColor DarkGray
  Write-Host $Text -ForegroundColor Yellow
}

function Write-Ok {
  param([string]$Text)

  Write-Host "     OK  " -NoNewline -ForegroundColor Green
  Write-Host $Text -ForegroundColor DarkGray
}

function Write-Fail {
  param([string]$Text)

  Write-Host "     FAIL " -NoNewline -ForegroundColor Red
  Write-Host $Text -ForegroundColor Red
}

function Invoke-Step {
  param(
    [string]$Name,
    [string]$Command,
    [string[]]$Arguments
  )

  Write-Step $Name

  $timer = [System.Diagnostics.Stopwatch]::StartNew()

  Write-Host "     > $Command $($Arguments -join ' ')" -ForegroundColor DarkGray

  & $Command @Arguments

  $exitCode = $LASTEXITCODE
  $timer.Stop()

  if ($exitCode -ne 0) {
    Write-Fail "$Name failed after $($timer.Elapsed.ToString('mm\:ss'))."
    exit $exitCode
  }

  Write-Ok "$Name completed in $($timer.Elapsed.ToString('mm\:ss'))."
}

function Test-CommandExists {
  param([string]$Command)

  return $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
}

function Ensure-JetBrainsCleanupCode {
  Write-Step "Checking JetBrains ReSharper Global Tools"

  $toolManifest = Join-Path $RepoRoot ".config\dotnet-tools.json"
  $toolManifestRoot = Join-Path $RepoRoot "dotnet-tools.json"

  if (-not (Test-Path $toolManifest) -and -not (Test-Path $toolManifestRoot)) {
    Write-Host "     No local tool manifest found." -ForegroundColor DarkGray
    Write-Host "     Creating local tool manifest..." -ForegroundColor DarkGray

    & dotnet new tool-manifest | Out-Host

    if ($LASTEXITCODE -ne 0) {
      Write-Fail "Failed to create local .NET tool manifest."
      exit $LASTEXITCODE
    }

    Write-Ok "Created local tool manifest."
  }
  else {
    Write-Ok "Local tool manifest found."
  }

  $toolList = & dotnet tool list --local 2>$null | Out-String

  if ($toolList -notmatch "jetbrains\.resharper\.globaltools") {
    Write-Host "     JetBrains tool is not installed locally." -ForegroundColor DarkGray
    Write-Host "     Installing JetBrains.ReSharper.GlobalTools..." -ForegroundColor DarkGray

    & dotnet tool install JetBrains.ReSharper.GlobalTools --local | Out-Host

    if ($LASTEXITCODE -ne 0) {
      Write-Fail "Failed to install JetBrains.ReSharper.GlobalTools."
      exit $LASTEXITCODE
    }

    Write-Ok "Installed JetBrains.ReSharper.GlobalTools locally."
  }
  else {
    Write-Ok "JetBrains.ReSharper.GlobalTools is already installed locally."
  }

  Invoke-Step `
    -Name "Restoring local .NET tools" `
    -Command "dotnet" `
    -Arguments @("tool", "restore")
}

function Invoke-DotNetFormat {
  Invoke-Step `
    -Name "Formatting whitespace, indentation, and line endings" `
    -Command "dotnet" `
    -Arguments @(
    "format",
    "whitespace",
    $Solution,
    "--no-restore",
    "--verbosity",
    "minimal"
  )

  Invoke-Step `
    -Name "Removing unused using directives and applying selected safe .NET style fixes" `
    -Command "dotnet" `
    -Arguments @(
    "format",
    "style",
    $Solution,
    "--no-restore",
    "--diagnostics",
    "IDE0005",
    "--severity",
    "info",
    "--verbosity",
    "minimal"
  )
}

function Refresh-GitIndex {
  if (-not (Test-CommandExists "git")) {
    return
  }

  & git -C $RepoRoot rev-parse --is-inside-work-tree *> $null

  if ($LASTEXITCODE -ne 0) {
    return
  }

  Write-Step "Refreshing Git index stat cache"

  & git -C $RepoRoot update-index --refresh -- *> $null

  if ($LASTEXITCODE -eq 0) {
    Write-Ok "Git index refreshed."
  }
  else {
    Write-Ok "Git index refreshed; real file changes may remain."
  }
}

Write-Title ".NET Code Cleanup"

Write-Host " Solution   : " -NoNewline -ForegroundColor DarkGray
Write-Host $Solution -ForegroundColor White

Write-Host " Mode       : " -NoNewline -ForegroundColor DarkGray
if ($VerifyOnly) {
  Write-Host "Verify only" -ForegroundColor Magenta
}
elseif ($Full) {
  Write-Host "Full cleanup with JetBrains, then dotnet format" -ForegroundColor Magenta
}
else {
  Write-Host "Standard cleanup" -ForegroundColor Magenta
}

Write-Host " Started    : " -NoNewline -ForegroundColor DarkGray
Write-Host $StartedAt.ToString("yyyy-MM-dd HH:mm:ss") -ForegroundColor White

if (-not (Test-Path $Solution)) {
  Write-Fail "Solution file was not found: $Solution"
  exit 1
}

Push-Location $RepoRoot

Invoke-Step `
  -Name "Restoring NuGet packages" `
  -Command "dotnet" `
  -Arguments @("restore", $Solution)

if ($VerifyOnly) {
  Invoke-Step `
    -Name "Checking formatting without changing files" `
    -Command "dotnet" `
    -Arguments @(
    "format",
    $Solution,
    "--no-restore",
    "--verify-no-changes",
    "--exclude-diagnostics",
    "IDE1006",
    "--verbosity",
    "minimal"
  )
}
else {
  if ($Full) {
    Ensure-JetBrainsCleanupCode

    Invoke-Step `
      -Name "Running JetBrains CleanupCode" `
      -Command "dotnet" `
      -Arguments @(
      "tool",
      "run",
      "jb",
      "--",
      "cleanupcode",
      $Solution,
      "--verbosity=WARN"
    )
  }

  Invoke-DotNetFormat
  Refresh-GitIndex
}

Pop-Location

$FinishedAt = Get-Date
$Duration = $FinishedAt - $StartedAt

Write-Title "Cleanup Complete"

Write-Host " Finished   : " -NoNewline -ForegroundColor DarkGray
Write-Host $FinishedAt.ToString("yyyy-MM-dd HH:mm:ss") -ForegroundColor White

Write-Host " Duration   : " -NoNewline -ForegroundColor DarkGray
Write-Host $Duration.ToString("mm\:ss") -ForegroundColor White

Write-Host " Result     : " -NoNewline -ForegroundColor DarkGray
Write-Host "Success" -ForegroundColor Green
