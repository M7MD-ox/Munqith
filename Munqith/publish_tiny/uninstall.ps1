$TargetDir = "$env:LOCALAPPDATA\Programs\Munqith"
$Desktop = [Environment]::GetFolderPath("Desktop")
$StartMenu = [Environment]::GetFolderPath("Programs")

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host "         Munqith Uninstaller / إلغاء تثبيت منقذ" -ForegroundColor Yellow
Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Stop existing process
Write-Host "[1/4] Stopping Munqith process..." -ForegroundColor Gray
Get-Process -Name "Munqith" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500

# 2. Remove Startup Registry
Write-Host "[2/4] Removing from Windows Startup..." -ForegroundColor Gray
Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "Munqith" -ErrorAction SilentlyContinue

# 3. Remove Shortcuts
Write-Host "[3/4] Removing Desktop and Start Menu Shortcuts..." -ForegroundColor Gray
Remove-Item -Path "$Desktop\Munqith.lnk" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$StartMenu\Munqith.lnk" -Force -ErrorAction SilentlyContinue

# 4. Remove Target Directory
Write-Host "[4/4] Removing program files..." -ForegroundColor Gray
if (Test-Path $TargetDir) {
    Remove-Item -Path $TargetDir -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "=======================================================" -ForegroundColor Green
Write-Host "   [SUCCESS] Munqith has been completely uninstalled." -ForegroundColor Green
Write-Host "   تم إلغاء تثبيت برنامج منقذ بنجاح بالكامل." -ForegroundColor Green
Write-Host "=======================================================" -ForegroundColor Green
Write-Host ""
