# ========================================
# Script Backup pentru Refactorizare
# ValyanClinic - Pre-Refactor Backup
# ========================================

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "BACKUP PRE-REFACTORIZARE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# PASUL 1: Creaza branch de backup
Write-Host "`n[1/3] Creez branch de backup..." -ForegroundColor Yellow
try {
    git checkout master
    git branch backup-pre-refactor
    git push origin backup-pre-refactor
    Write-Host "? Branch backup-pre-refactor creat cu succes" -ForegroundColor Green
} catch {
    Write-Host "? Eroare la crearea branch-ului: $_" -ForegroundColor Red
}

# PASUL 2: Creaza tag pentru versiune curenta
Write-Host "`n[2/3] Creez tag pentru versiunea curenta..." -ForegroundColor Yellow
try {
    git tag v1.0-pre-refactor
    git push origin v1.0-pre-refactor
    Write-Host "? Tag v1.0-pre-refactor creat cu succes" -ForegroundColor Green
} catch {
    Write-Host "? Eroare la crearea tag-ului: $_" -ForegroundColor Red
}

# PASUL 3: Backup local extern
Write-Host "`n[3/3] Creez backup local..." -ForegroundColor Yellow
$backupDate = Get-Date -Format 'yyyy-MM-dd_HHmmss'
$backupPath = "D:\Backup\ValyanClinic-$backupDate"

try {
    if (!(Test-Path "D:\Backup")) {
        New-Item -ItemType Directory -Path "D:\Backup" -Force | Out-Null
    }
    
    robocopy "D:\Lucru\CMS" $backupPath /E /XD .git bin obj node_modules /XF *.suo *.user /NFL /NDL /NJH /NJS /nc /ns /np
    
    Write-Host "? Backup local creat: $backupPath" -ForegroundColor Green
} catch {
    Write-Host "? Eroare la backup local: $_" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "BACKUP FINALIZAT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nPoti continua cu refactorizarea!" -ForegroundColor Green
