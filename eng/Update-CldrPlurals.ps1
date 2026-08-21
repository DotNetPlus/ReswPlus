# Refreshes the plural rules of Unicode CLDR that the generator writes its providers from.
#
# The file is vendored byte for byte and nothing is transcribed out of it, so refreshing CLDR is replacing it
# and running the tests: they replay the sample quantities the file carries and say what changed.

[CmdletBinding()]
param(
    # The branch or tag of the cldr-json repository to take the rules from.
    [string] $Release = 'main'
)

$ErrorActionPreference = 'Stop'

$source = "https://raw.githubusercontent.com/unicode-org/cldr-json/$Release/cldr-json/cldr-core/supplemental/plurals.json"
$destination = Join-Path $PSScriptRoot '..\src\ReswPlus.SourceGenerator\Plurals\plurals.json'

Write-Host "Reading $source"

$response = Invoke-WebRequest -Uri $source -UseBasicParsing

# Written as bytes rather than as text, because the file is vendored exactly as published: letting PowerShell
# pick an encoding or a line ending would leave the copy differing from the thing it was taken from.
$bytes = if ($response.Content -is [byte[]]) { $response.Content } else { [System.Text.Encoding]::UTF8.GetBytes($response.Content) }
[System.IO.File]::WriteAllBytes((Resolve-Path $destination), $bytes)

$text = [System.Text.Encoding]::UTF8.GetString($bytes)
$version = ([regex]::Match($text, '"_cldrVersion":\s*"([^"]+)"')).Groups[1].Value

Write-Host "Wrote CLDR $version to $destination"
Write-Host ''
Write-Host 'Now run: dotnet test tests\ReswPlusUnitTests'
Write-Host 'See src\ReswPlus.SourceGenerator\Plurals\README.md for what a failure means.'
