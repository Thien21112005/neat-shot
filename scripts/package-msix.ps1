<#
.SYNOPSIS
    Đóng gói NeatShot thành gói MSIX để nộp lên Microsoft Store.

.DESCRIPTION
    - Chạy unit test (bỏ qua nhóm Desktop vì chúng gửi phím Esc toàn hệ thống).
    - Publish self-contained win-x64 (không single-file: gói MSIX đã nén sẵn, app mở nhanh hơn).
    - Ghép AppxManifest.xml + logo trong packaging/msix, tạo resources.pri.
    - Tạo artifacts/msix/NeatShot-<Version>-x64.msix (chưa ký: Microsoft Store tự ký khi phát hành).

    makeappx.exe / makepri.exe lấy từ gói NuGet chính thức Microsoft.Windows.SDK.BuildTools,
    tải một lần vào %LOCALAPPDATA%\NeatShot.BuildTools — không cần cài Windows SDK.

.PARAMETER Register
    Cài thử bản vừa đóng gói lên máy này (Add-AppxPackage -Register, cần bật Developer Mode).

.EXAMPLE
    ./scripts/package-msix.ps1
    ./scripts/package-msix.ps1 -SkipTests -Register
#>
param(
    [switch]$SkipTests,
    [switch]$Register
)

$ErrorActionPreference = "Stop"

$BuildToolsVersion = "10.0.26100.9169"
$Runtime = "win-x64"

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src/NeatShot/NeatShot.csproj"
$testProject = Join-Path $root "tests/NeatShot.Tests/NeatShot.Tests.csproj"
$packagingDir = Join-Path $root "packaging/msix"
$outDir = Join-Path $root "artifacts/msix"
$layoutDir = Join-Path $outDir "layout"

[xml]$csproj = Get-Content $project
$version = ($csproj.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1).Version
if (-not $version) { throw "Không tìm thấy <Version> trong $project" }
$packageVersion = "$version.0"

Write-Host "==> NeatShot MSIX v$packageVersion" -ForegroundColor Cyan

# --- Công cụ đóng gói ---
function Get-BuildTools {
    $toolsRoot = Join-Path $env:LOCALAPPDATA "NeatShot.BuildTools/$BuildToolsVersion"
    $binDir = Get-ChildItem (Join-Path $toolsRoot "bin") -Directory -ErrorAction SilentlyContinue |
        Sort-Object Name -Descending | Select-Object -First 1
    if (-not $binDir) {
        Write-Host "==> Tải Microsoft.Windows.SDK.BuildTools $BuildToolsVersion" -ForegroundColor Cyan
        New-Item -ItemType Directory -Force $toolsRoot | Out-Null
        $nupkg = Join-Path $toolsRoot "buildtools.zip"
        $url = "https://api.nuget.org/v3-flatcontainer/microsoft.windows.sdk.buildtools/$BuildToolsVersion/microsoft.windows.sdk.buildtools.$BuildToolsVersion.nupkg"
        Invoke-WebRequest $url -OutFile $nupkg -UseBasicParsing
        Expand-Archive $nupkg -DestinationPath $toolsRoot -Force
        Remove-Item $nupkg
        $binDir = Get-ChildItem (Join-Path $toolsRoot "bin") -Directory | Sort-Object Name -Descending | Select-Object -First 1
    }
    $x64 = Join-Path $binDir.FullName "x64"
    return @{
        MakeAppx = Join-Path $x64 "makeappx.exe"
        MakePri  = Join-Path $x64 "makepri.exe"
    }
}

$tools = Get-BuildTools

if (-not $SkipTests) {
    Write-Host "==> Chạy unit test" -ForegroundColor Cyan
    dotnet test $testProject --configuration Release --filter "Category!=Desktop" --nologo
    if ($LASTEXITCODE -ne 0) { throw "Unit test thất bại, dừng đóng gói." }
}

if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }

Write-Host "==> Publish self-contained ($Runtime)" -ForegroundColor Cyan
dotnet publish $project `
    --configuration Release `
    --runtime $Runtime `
    --self-contained true `
    --output $layoutDir `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish thất bại." }

Write-Host "==> Ghép manifest và logo" -ForegroundColor Cyan
$manifest = (Get-Content (Join-Path $packagingDir "AppxManifest.xml") -Raw -Encoding UTF8).Replace('$(Version)', $packageVersion)
[System.IO.File]::WriteAllText((Join-Path $layoutDir "AppxManifest.xml"), $manifest, [System.Text.UTF8Encoding]::new($false))
Copy-Item (Join-Path $packagingDir "Assets") (Join-Path $layoutDir "Assets") -Recurse

Write-Host "==> Tạo resources.pri" -ForegroundColor Cyan
$priConfig = Join-Path $outDir "priconfig.xml"
& $tools.MakePri createconfig /cf $priConfig /dq vi-VN /pv 10.0.0 /o | Out-Null
if ($LASTEXITCODE -ne 0) { throw "makepri createconfig thất bại." }
& $tools.MakePri new /pr $layoutDir /cf $priConfig /mn (Join-Path $layoutDir "AppxManifest.xml") /of (Join-Path $layoutDir "resources.pri") /o | Out-Null
if ($LASTEXITCODE -ne 0) { throw "makepri new thất bại." }

$msixPath = Join-Path $outDir "NeatShot-$packageVersion-x64.msix"
Write-Host "==> Đóng gói MSIX" -ForegroundColor Cyan
& $tools.MakeAppx pack /d $layoutDir /p $msixPath /o | Out-Null
if ($LASTEXITCODE -ne 0) { throw "makeappx pack thất bại." }

$msix = Get-Item $msixPath
Write-Host ""
Write-Host ("==> Xong: {0} ({1:N1} MB)" -f $msix.FullName, ($msix.Length / 1MB)) -ForegroundColor Green
Write-Host "    Tải file này lên Partner Center → Packages (Store tự ký khi phát hành)." -ForegroundColor Green

if ($Register) {
    Write-Host "==> Cài thử lên máy này (cần Developer Mode)" -ForegroundColor Cyan
    Add-AppxPackage -Register (Join-Path $layoutDir "AppxManifest.xml") -ForceApplicationShutdown
    Write-Host "    Đã cài: mở NeatShot từ Start menu. Gỡ: Get-AppxPackage NguynNgcThin.NeatShot | Remove-AppxPackage" -ForegroundColor Green
}
