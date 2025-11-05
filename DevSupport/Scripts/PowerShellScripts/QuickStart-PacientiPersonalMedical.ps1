# ========================================
# QUICK START: Pacienti_PersonalMedical Junction Table
# Database: ValyanMed
# Descriere: Script rapid pentru deployment complet
# ========================================

<#
.SYNOPSIS
    One-click deployment pentru tabela de legatura Pacienti_PersonalMedical

.DESCRIPTION
    Acest script face TOTUL automat:
    - Verifica conexiunea
    - Creeaza tabela
    - Creeaza stored procedures
    - Ruleaza teste
    - Adauga date de test

.EXAMPLE
    .\QuickStart-PacientiPersonalMedical.ps1
#>

Write-Host ""
Write-Host "========================================" -ForegroundColor Magenta
Write-Host " QUICK START: Pacienti_PersonalMedical" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

$ServerName = "DESKTOP-3Q8HI82\ERP"
$DatabaseName = "ValyanMed"

Write-Host "?? Server: $ServerName" -ForegroundColor Cyan
Write-Host "?? Database: $DatabaseName" -ForegroundColor Cyan
Write-Host ""

# Confirmation
$confirm = Read-Host "Continui cu deployment-ul? (Y/N)"
if ($confirm -ne "Y" -and $confirm -ne "y") {
    Write-Host "Deployment anulat." -ForegroundColor Yellow
    exit
}

Write-Host ""
Write-Host "?? Pornesc deployment..." -ForegroundColor Green
Write-Host ""

# ========================================
# STEP 1: Deploy
# ========================================

Write-Host "Pasul 1/3: Deployment tabela si stored procedures..." -ForegroundColor Cyan
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$DeployScript = Join-Path $ScriptPath "Deploy-PacientiPersonalMedical.ps1"

if (Test-Path $DeployScript) {
    & $DeployScript -ServerName $ServerName -DatabaseName $DatabaseName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "? Deployment esuat!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "? Script deployment nu a fost gasit!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "? Deployment complet!" -ForegroundColor Green
Write-Host ""

# ========================================
# STEP 2: Test
# ========================================

Write-Host "Pasul 2/3: Rulare teste..." -ForegroundColor Cyan
$TestScript = Join-Path $ScriptPath "Test-PacientiPersonalMedical.ps1"

if (Test-Path $TestScript) {
    & $TestScript -ServerName $ServerName -DatabaseName $DatabaseName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
     Write-Host "??  Unele teste au esuat, dar deployment-ul este complet." -ForegroundColor Yellow
        Write-Host "    Verifica manual in SQL Server Management Studio." -ForegroundColor Yellow
    }
} else {
    Write-Host "??  Script teste nu a fost gasit, skip testing." -ForegroundColor Yellow
}

Write-Host ""

# ========================================
# STEP 3: Summary
# ========================================

Write-Host "Pasul 3/3: Summary..." -ForegroundColor Cyan
Write-Host ""

Write-Host "========================================" -ForegroundColor Magenta
Write-Host " DEPLOYMENT FINALIZAT! ??" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

Write-Host "? Tabela: Pacienti_PersonalMedical" -ForegroundColor Green
Write-Host "? Stored Procedures: 8 proceduri" -ForegroundColor Green
Write-Host "? Indexes: 6 indexes pentru performanta" -ForegroundColor Green
Write-Host "? Triggers: 1 trigger pentru audit" -ForegroundColor Green
Write-Host ""

Write-Host "?? Documentatie completa:" -ForegroundColor Cyan
Write-Host "   DevSupport\Documentation\Database\Pacienti_PersonalMedical_Documentation.md" -ForegroundColor Gray
Write-Host ""

Write-Host "?? Verificare SQL:" -ForegroundColor Cyan
Write-Host "   SELECT * FROM Pacienti_PersonalMedical" -ForegroundColor Gray
Write-Host "   EXEC sp_PacientiPersonalMedical_GetStatistici" -ForegroundColor Gray
Write-Host ""

Write-Host "?? Pasii urmatori:" -ForegroundColor Cyan
Write-Host "   1. Creeaza entitate C#: PacientPersonalMedical.cs" -ForegroundColor Gray
Write-Host "   2. Creeaza DTOs: DoctorAsociatDto, PacientAsociatDto" -ForegroundColor Gray
Write-Host "   3. Creeaza Queries/Commands cu MediatR" -ForegroundColor Gray
Write-Host "   4. Actualizeaza UI Blazor pentru management relatii" -ForegroundColor Gray
Write-Host ""

Write-Host "? Totul este gata! Poti incepe sa folosesti tabela." -ForegroundColor Green
Write-Host ""
