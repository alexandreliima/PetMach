[CmdletBinding()]
param(
    [switch]$SkipRestore,
    [string]$AndroidBinUtilsDirectory
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$localDotnet = Join-Path $root '.dotnet\dotnet.exe'
$dotnet = if (Test-Path -LiteralPath $localDotnet) { $localDotnet } else { 'dotnet' }

Push-Location $root
try {
    if (-not $SkipRestore) {
        & $dotnet tool restore
        & $dotnet restore PetMach.slnx
    }

    & $dotnet format PetMach.slnx --verify-no-changes --no-restore
    $buildArguments = @('build', 'PetMach.slnx', '--no-restore')
    if ($AndroidBinUtilsDirectory) {
        $buildArguments += "-p:AndroidBinUtilsDirectory=$AndroidBinUtilsDirectory"
    }

    & $dotnet @buildArguments
    & $dotnet test PetMach.slnx --no-build --collect:'XPlat Code Coverage'
}
finally {
    Pop-Location
}
