<#
.SYNOPSIS
    Đóng gói NeatShot thành file exe chạy độc lập (self-contained, single-file) cho Windows x64.

.DESCRIPTION
    - Chạy toàn bộ unit test (bỏ qua nhóm Desktop vì chúng gửi phím Esc toàn hệ thống).
    - Publish ra artifacts/publish/NeatShot.exe (không cần cài .NET Runtime trên máy người dùng).
    - Nén thành artifacts/NeatShot-v<Version>-win-x64.zip kèm README và LICENSE.

.EXAMPLE
    ./scripts/publish.ps1
    ./scripts/publish.ps1 -SkipTests
#>
param(
    [string]$Runtime = "win-x64",
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src/NeatShot/NeatShot.csproj"
$testProject = Join-Path $root "tests/NeatShot.Tests/NeatShot.Tests.csproj"
$artifacts = Join-Path $root "artifacts"
$publishDir = Join-Path $artifacts "publish"

[xml]$csproj = Get-Content $project
$version = ($csproj.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1).Version
if (-not $version) { throw "Không tìm thấy <Version> trong $project" }

Write-Host "==> NeatShot v$version ($Runtime)" -ForegroundColor Cyan

if (-not $SkipTests) {
    Write-Host "==> Chạy unit test" -ForegroundColor Cyan
    dotnet test $testProject --configuration Release --filter "Category!=Desktop" --nologo
    if ($LASTEXITCODE -ne 0) { throw "Unit test thất bại, dừng đóng gói." }
}

if (Test-Path $artifacts) { Remove-Item $artifacts -Recurse -Force }

Write-Host "==> Publish self-contained single-file" -ForegroundColor Cyan
dotnet publish $project `
    --configuration Release `
    --runtime $Runtime `
    --self-contained true `
    --output $publishDir `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish thất bại." }

Copy-Item (Join-Path $root "README.md") $publishDir
Copy-Item (Join-Path $root "LICENSE") $publishDir

$zipPath = Join-Path $artifacts "NeatShot-v$version-$Runtime.zip"
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force

$exe = Get-Item (Join-Path $publishDir "NeatShot.exe")
$zip = Get-Item $zipPath
Write-Host ""
Write-Host ("==> Xong: {0} ({1:N1} MB)" -f $exe.FullName, ($exe.Length / 1MB)) -ForegroundColor Green
Write-Host ("==> Gói:  {0} ({1:N1} MB)" -f $zip.FullName, ($zip.Length / 1MB)) -ForegroundColor Green
