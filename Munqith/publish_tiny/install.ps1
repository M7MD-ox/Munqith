$TargetDir = "$env:LOCALAPPDATA\Programs\Munqith"
$Desktop = [Environment]::GetFolderPath("Desktop")
$StartMenu = [Environment]::GetFolderPath("Programs")
$CurrentDir = $PSScriptRoot

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host "         Munqith Installer / تثبيت برنامج منقذ" -ForegroundColor Yellow
Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Stop existing process
Write-Host "[1/5] Stopping any running Munqith process..." -ForegroundColor Gray
Get-Process -Name "Munqith" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500

# 2. Create Target Directories
Write-Host "[2/5] Creating application folder in LocalAppData..." -ForegroundColor Gray
New-Item -ItemType Directory -Force -Path $TargetDir | Out-Null
New-Item -ItemType Directory -Force -Path "$TargetDir\Resources" | Out-Null
New-Item -ItemType Directory -Force -Path "$TargetDir\wwwroot" | Out-Null

# 3. Copy files
Write-Host "[3/5] Copying program files..." -ForegroundColor Gray
Copy-Item "$CurrentDir\Munqith.exe" "$TargetDir\Munqith.exe" -Force
Copy-Item "$CurrentDir\WebView2Loader.dll" "$TargetDir\WebView2Loader.dll" -Force -ErrorAction SilentlyContinue
Copy-Item "$CurrentDir\uninstall.cmd" "$TargetDir\uninstall.cmd" -Force
Copy-Item "$CurrentDir\uninstall.ps1" "$TargetDir\uninstall.ps1" -Force
Copy-Item "$CurrentDir\Resources\*" "$TargetDir\Resources\" -Recurse -Force
Copy-Item "$CurrentDir\wwwroot\*" "$TargetDir\wwwroot\" -Recurse -Force

# 4. Windows Startup Registry
Write-Host "[4/5] Registering in Windows Startup (Run at boot)..." -ForegroundColor Gray
Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "Munqith" -Value "`"$TargetDir\Munqith.exe`""

# 5. Create Shortcuts
Write-Host "[5/5] Creating Desktop and Start Menu Shortcuts..." -ForegroundColor Gray
$WshShell = New-Object -ComObject WScript.Shell
$DesktopLnk = $WshShell.CreateShortcut("$Desktop\Munqith.lnk")
$DesktopLnk.TargetPath = "$TargetDir\Munqith.exe"
$DesktopLnk.WorkingDirectory = $TargetDir
$DesktopLnk.IconLocation = "$TargetDir\Resources\app.ico"
$DesktopLnk.Save()

$StartLnk = $WshShell.CreateShortcut("$StartMenu\Munqith.lnk")
$StartLnk.TargetPath = "$TargetDir\Munqith.exe"
$StartLnk.WorkingDirectory = $TargetDir
$StartLnk.IconLocation = "$TargetDir\Resources\app.ico"
$StartLnk.Save()

# 6. Start Munqith
Start-Process "$TargetDir\Munqith.exe"

Write-Host ""
Write-Host "=======================================================" -ForegroundColor Green
Write-Host "   [SUCCESS] Munqith installed successfully (3.08 MB)!" -ForegroundColor Green
Write-Host "   تم تثبيت البرنامج بنجاح وإضافته للتشغيل التلقائي!" -ForegroundColor Green
Write-Host "=======================================================" -ForegroundColor Green
Write-Host ""
