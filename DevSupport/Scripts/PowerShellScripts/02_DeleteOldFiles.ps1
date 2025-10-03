# ========================================
# Script Stergere pentru Refactorizare
# ValyanClinic - Clean Slate
# ========================================

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Red
Write-Host "STERGERE COMPLETA - CLEAN SLATE" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""
Write-Host "ATENTIE: Acest script va sterge toate fisierele vechi!" -ForegroundColor Yellow
Write-Host "Backup-ul a fost deja creat in pasul anterior." -ForegroundColor Yellow
Write-Host ""

$confirmation = Read-Host "Doresti sa continui? (DA pentru confirmare)"
if ($confirmation -ne "DA") {
    Write-Host "Operatie anulata." -ForegroundColor Yellow
    exit
}

Write-Host "`nIncep stergerea..." -ForegroundColor Cyan

# ========================================
# VALYAN CLINIC - Proiect Principal Blazor
# ========================================
Write-Host "`n[1/4] Sterg fisiere din ValyanClinic (Blazor)..." -ForegroundColor Yellow

# Sterg Components (tot)
if (Test-Path "ValyanClinic\Components") {
    Remove-Item "ValyanClinic\Components" -Recurse -Force
    Write-Host "  - Components: STERS" -ForegroundColor Green
}

# Sterg wwwroot (tot)
if (Test-Path "ValyanClinic\wwwroot") {
    Remove-Item "ValyanClinic\wwwroot" -Recurse -Force
    Write-Host "  - wwwroot: STERS" -ForegroundColor Green
}

# Sterg Program.cs (rescriem)
if (Test-Path "ValyanClinic\Program.cs") {
    Remove-Item "ValyanClinic\Program.cs" -Force
    Write-Host "  - Program.cs: STERS" -ForegroundColor Green
}

# Sterg HealthChecks daca exista
if (Test-Path "ValyanClinic\HealthChecks") {
    Remove-Item "ValyanClinic\HealthChecks" -Recurse -Force
    Write-Host "  - HealthChecks: STERS" -ForegroundColor Green
}

# ========================================
# VALYAN CLINIC DOMAIN
# ========================================
Write-Host "`n[2/4] Sterg fisiere din ValyanClinic.Domain..." -ForegroundColor Yellow

# Sterg Models
if (Test-Path "ValyanClinic.Domain\Models") {
    Remove-Item "ValyanClinic.Domain\Models" -Recurse -Force
    Write-Host "  - Models: STERS" -ForegroundColor Green
}

# Sterg Enums (le rescriem)
if (Test-Path "ValyanClinic.Domain\Enums") {
    Remove-Item "ValyanClinic.Domain\Enums" -Recurse -Force
    Write-Host "  - Enums: STERS" -ForegroundColor Green
}

# Sterg Interfaces (le rescriem)
if (Test-Path "ValyanClinic.Domain\Interfaces") {
    Remove-Item "ValyanClinic.Domain\Interfaces" -Recurse -Force
    Write-Host "  - Interfaces: STERS" -ForegroundColor Green
}

# Sterg Validators (mutam in Application)
if (Test-Path "ValyanClinic.Domain\Validators") {
    Remove-Item "ValyanClinic.Domain\Validators" -Recurse -Force
    Write-Host "  - Validators: STERS" -ForegroundColor Green
}

# Sterg Extensions
if (Test-Path "ValyanClinic.Domain\Extensions") {
    Remove-Item "ValyanClinic.Domain\Extensions" -Recurse -Force
    Write-Host "  - Extensions: STERS" -ForegroundColor Green
}

# Sterg Entities vechi (le rescriem)
if (Test-Path "ValyanClinic.Domain\Entities") {
    Remove-Item "ValyanClinic.Domain\Entities" -Recurse -Force
    Write-Host "  - Entities: STERS" -ForegroundColor Green
}

# Sterg Common
if (Test-Path "ValyanClinic.Domain\Common") {
    Remove-Item "ValyanClinic.Domain\Common" -Recurse -Force
    Write-Host "  - Common: STERS" -ForegroundColor Green
}

# ========================================
# VALYAN CLINIC APPLICATION
# ========================================
Write-Host "`n[3/4] Sterg fisiere din ValyanClinic.Application..." -ForegroundColor Yellow

# Sterg Services (mutam in Features)
if (Test-Path "ValyanClinic.Application\Services") {
    Remove-Item "ValyanClinic.Application\Services" -Recurse -Force
    Write-Host "  - Services: STERS" -ForegroundColor Green
}

# Sterg DTOs (mutam in Features)
if (Test-Path "ValyanClinic.Application\DTOs") {
    Remove-Item "ValyanClinic.Application\DTOs" -Recurse -Force
    Write-Host "  - DTOs: STERS" -ForegroundColor Green
}

# Sterg Validators (mutam in Features)
if (Test-Path "ValyanClinic.Application\Validators") {
    Remove-Item "ValyanClinic.Application\Validators" -Recurse -Force
    Write-Host "  - Validators: STERS" -ForegroundColor Green
}

# Sterg Models
if (Test-Path "ValyanClinic.Application\Models") {
    Remove-Item "ValyanClinic.Application\Models" -Recurse -Force
    Write-Host "  - Models: STERS" -ForegroundColor Green
}

# Sterg Exceptions
if (Test-Path "ValyanClinic.Application\Exceptions") {
    Remove-Item "ValyanClinic.Application\Exceptions" -Recurse -Force
    Write-Host "  - Exceptions: STERS" -ForegroundColor Green
}

# Sterg Interfaces
if (Test-Path "ValyanClinic.Application\Interfaces") {
    Remove-Item "ValyanClinic.Application\Interfaces" -Recurse -Force
    Write-Host "  - Interfaces: STERS" -ForegroundColor Green
}

# PASTRAM DOAR Common/Result.cs - il copiem temporar
if (Test-Path "ValyanClinic.Application\Common\Result.cs") {
    Copy-Item "ValyanClinic.Application\Common\Result.cs" "ValyanClinic.Application\Result.cs.bak" -Force
    Write-Host "  - Result.cs: SALVAT temporar" -ForegroundColor Cyan
}

if (Test-Path "ValyanClinic.Application\Common") {
    Remove-Item "ValyanClinic.Application\Common" -Recurse -Force
    Write-Host "  - Common: STERS" -ForegroundColor Green
}

# ========================================
# VALYAN CLINIC INFRASTRUCTURE
# ========================================
Write-Host "`n[4/4] Sterg fisiere din ValyanClinic.Infrastructure..." -ForegroundColor Yellow

# Pastram Repositories si Data, stergem restul
if (Test-Path "ValyanClinic.Infrastructure\Services") {
    Remove-Item "ValyanClinic.Infrastructure\Services" -Recurse -Force
    Write-Host "  - Services: STERS" -ForegroundColor Green
}

if (Test-Path "ValyanClinic.Infrastructure\Persistence") {
    Remove-Item "ValyanClinic.Infrastructure\Persistence" -Recurse -Force
    Write-Host "  - Persistence: STERS" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Red
Write-Host "STERGERE COMPLETA!" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host "`nStructura minima ramasa:" -ForegroundColor Cyan
Write-Host "  - ValyanClinic.csproj, appsettings.json, Properties/" -ForegroundColor White
Write-Host "  - ValyanClinic.Domain.csproj (gol)" -ForegroundColor White
Write-Host "  - ValyanClinic.Application.csproj (Result.cs.bak)" -ForegroundColor White
Write-Host "  - ValyanClinic.Infrastructure.csproj (Repositories/, Data/)" -ForegroundColor White
Write-Host "`nGata pentru reconstructie!" -ForegroundColor Green
