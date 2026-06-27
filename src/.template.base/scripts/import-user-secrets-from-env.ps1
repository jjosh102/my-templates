param(
    [string] $EnvFile = ".env",
    [string] $Project = "",
    [switch] $DryRun
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $EnvFile)) {
    throw "Env file '$EnvFile' was not found."
}

$lines = Get-Content -LiteralPath $EnvFile
foreach ($line in $lines) {
    $trimmed = $line.Trim()
    if ($trimmed.Length -eq 0 -or $trimmed.StartsWith("#")) {
        continue
    }

    $separatorIndex = $trimmed.IndexOf("=")
    if ($separatorIndex -le 0) {
        Write-Warning "Skipping invalid .env line: $line"
        continue
    }

    $key = $trimmed.Substring(0, $separatorIndex).Trim()
    $value = $trimmed.Substring($separatorIndex + 1).Trim()

    if (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'"))) {
        $value = $value.Substring(1, $value.Length - 2)
    }

    $secretKey = $key.Replace("__", ":")

    if ($DryRun) {
        Write-Host "dotnet user-secrets set `"$secretKey`" `"$value`" --project `"$Project`""
        continue
    }

    dotnet user-secrets set $secretKey $value --project $Project
}
