#Requires -Version 7
<#
.SYNOPSIS
    builds the ToolRunner distribution — one self-contained single-file executable.
.DESCRIPTION
    publishes the win-x64 profile (no .NET runtime needed on the target PC), then stages the
    executable under bin\dist with the version in its name and wraps it in a .zip, because a raw
    .exe is frequently blocked or quarantined by mail and download channels.
    배포판 빌드 스크립트 (단일 실행 파일 + zip 래퍼)
#>
[CmdletBinding()]
param(
	# skips the .zip wrapper when only the bare executable is wanted
	[switch] $NoZip
)

$ErrorActionPreference = 'Stop'

# resolve the project layout relative to this script so it can be run from anywhere
$projectDir  = $PSScriptRoot
$projectFile = Join-Path $projectDir 'ToolRunner.csproj'
$publishDir  = Join-Path $projectDir 'bin\publish\x64'
$distDir     = Join-Path $projectDir 'bin\dist'
$publishExe  = Join-Path $publishDir 'ToolRunner.exe'

# read the single source of truth for the product version out of the csproj
$version = ([xml] (Get-Content $projectFile)).Project.PropertyGroup.Version | Where-Object { $_ }

# a missing version means the csproj was edited incorrectly — stop before producing a mislabelled build
if (-not $version) {
	throw "no <Version> element found in $projectFile"
}

Write-Host "ToolRunner $version — self-contained single-file publish (win-x64)" -ForegroundColor Cyan

# refuse to publish while a copy is running — the linker cannot overwrite a locked executable
$running = Get-Process -Name 'ToolRunner' -ErrorAction SilentlyContinue
if ($running) {
	throw 'ToolRunner is running — close it before publishing'
}

# clear the previous publish output so nothing stale is picked up
if (Test-Path $publishDir) {
	Remove-Item $publishDir -Recurse -Force
}

# clear the previous distribution artifacts
if (Test-Path $distDir) {
	Remove-Item $distDir -Recurse -Force
}

# publish self-contained into a single file — every setting is passed here rather than kept in a
# .pubxml, because the repository .gitignore excludes *.pubxml and an ignored profile would leave
# this script broken on a fresh clone. trimming stays off: WPF and the DI container resolve types
# by reflection
dotnet publish $projectFile -c Release -r win-x64 `
	--self-contained true `
	-p:PublishDir=bin\publish\x64\ `
	-p:PublishSingleFile=true `
	-p:EnableCompressionInSingleFile=true `
	-p:IncludeNativeLibrariesForSelfExtract=true `
	-p:IncludeAllContentForSelfExtract=true `
	-p:PublishReadyToRun=true `
	-p:PublishTrimmed=false `
	-p:DebugType=none `
	-p:DebugSymbols=false `
	-p:SatelliteResourceLanguages=en

# a non-zero exit code means the publish failed — surface it instead of staging a partial build
if ($LASTEXITCODE -ne 0) {
	throw "dotnet publish failed with exit code $LASTEXITCODE"
}

# verify the single-file executable actually landed where the profile said it would
if (-not (Test-Path $publishExe)) {
	throw "expected published executable not found: $publishExe"
}

# stage the versioned artifact in its own folder
New-Item -ItemType Directory -Force $distDir | Out-Null
$distExe = Join-Path $distDir "ToolRunner_$($version)_x64.exe"
Copy-Item $publishExe $distExe -Force

# wrap the executable in a .zip unless the caller asked for the bare file
if (-not $NoZip) {
	$distZip = [IO.Path]::ChangeExtension($distExe, 'zip')
	Compress-Archive -Path $distExe -DestinationPath $distZip -Force
}

# report what was produced, with sizes in MB for a quick sanity check
Write-Host ''
Write-Host 'output:' -ForegroundColor Green
foreach ($file in Get-ChildItem $distDir) {
	$mb = [math]::Round($file.Length / 1MB, 1)
	Write-Host ("  {0,-34} {1,7} MB" -f $file.Name, $mb)
}
