# ========================================
# EXECUTARE MIGRARE ANALIZE MEDICALE CU BACKUP
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$Server = "DESKTOP-3Q8HI82\ERP",
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "ValyanMed",
    
    [Parameter(Mandatory=$false)]
    [string]$BackupPath = "D:\Lucru\CMS\DevSupport\Backups"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EXECUTARE MIGRARE ANALIZE MEDICALE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificare existen?? director backup
if (-not (Test-Path $BackupPath)) {
    Write-Host "Creeare director backup: $BackupPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
}

# Generare nume fi?ier backup
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = Join-Path $BackupPath "ValyanMed_BeforeAnalizeMigration_$timestamp.bak"

Write-Host "[1/5] BACKUP BAZ? DE DATE" -ForegroundColor Yellow
Write-Host "  Surs?: $Server.$Database" -ForegroundColor Gray
Write-Host "  Destina?ie: $backupFile" -ForegroundColor Gray
Write-Host ""

try {
    # Executare backup
    $backupQuery = @"
BACKUP DATABASE [$Database] 
TO DISK = '$backupFile'
WITH FORMAT, INIT, 
NAME = 'ValyanMed Before Analize Migration',
DESCRIPTION = 'Backup automatic înainte de migrarea analizelor medicale din HelperValyanMed',
COMPRESSION,
STATS = 10;
"@

    Write-Host "  Se creeaz? backup-ul..." -ForegroundColor Gray
    sqlcmd -S $Server -d master -Q $backupQuery -b
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? BACKUP REU?IT!" -ForegroundColor Green
        
        # Verificare dimensiune backup
        $backupInfo = Get-Item $backupFile
        $sizeMB = [math]::Round($backupInfo.Length / 1MB, 2)
        Write-Host "  Dimensiune backup: $sizeMB MB" -ForegroundColor Gray
    }
    else {
        throw "Backup e?uat cu cod eroare: $LASTEXITCODE"
    }
}
catch {
    Write-Host "  ? EROARE LA BACKUP: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "MIGRAREA A FOST ANULAT?!" -ForegroundColor Red
    Write-Host "Verific? permisiunile ?i încearc? din nou." -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "[2/5] VERIFICARE CONEXIUNE SURS? (HelperValyanMed)" -ForegroundColor Yellow

try {
    $testQuery = "SELECT COUNT(*) AS Total FROM [DESKTOP-3Q8HI82\ERP].[HelperValyanMed].dbo.AnalizeLaborator WHERE ISNULL(Activ, 1) = 1"
    $result = sqlcmd -S $Server -d $Database -Q $testQuery -h -1 -W
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? Conexiune HelperValyanMed OK" -ForegroundColor Green
        Write-Host "  Analize active în surs?: $($result.Trim())" -ForegroundColor Gray
    }
    else {
        throw "Conexiune e?uat?"
    }
}
catch {
    Write-Host "  ? EROARE: Nu se poate conecta la HelperValyanMed" -ForegroundColor Red
    Write-Host "  Verific? c? SQL Server Agent ruleaz? ?i ai permisiuni linked server" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "[3/5] EXECUTARE SCRIPT MIGRARE" -ForegroundColor Yellow
Write-Host "  Script: 004_Migrare_AnalizeMedicale_HelperValyanMed.sql" -ForegroundColor Gray
Write-Host "  Aceasta poate dura 1-2 minute..." -ForegroundColor Gray
Write-Host ""

$scriptPath = Join-Path (Get-Location).Path "..\..\01_Database\06_Migrations\004_Migrare_AnalizeMedicale_HelperValyanMed.sql"

if (-not (Test-Path $scriptPath)) {
    Write-Host "  ? EROARE: Script-ul nu a fost g?sit la:" -ForegroundColor Red
    Write-Host "  $scriptPath" -ForegroundColor Gray
    exit 1
}

try {
    $startTime = Get-Date
    
    # Rulare script cu output în fi?ier
    $outputFile = Join-Path $BackupPath "Migration_Output_$timestamp.txt"
    sqlcmd -S $Server -d $Database -i $scriptPath -o $outputFile
    
    $endTime = Get-Date
    $duration = ($endTime - $startTime).TotalSeconds
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? MIGRARE REU?IT?!" -ForegroundColor Green
        Write-Host "  Durat?: $([math]::Round($duration, 2)) secunde" -ForegroundColor Gray
        Write-Host "  Log salvat în: $outputFile" -ForegroundColor Gray
    }
    else {
        throw "Script e?uat cu cod eroare: $LASTEXITCODE"
    }
}
catch {
    Write-Host "  ? EROARE LA EXECUTARE SCRIPT!" -ForegroundColor Red
    Write-Host "  $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "ROLLBACK ÎN CURS..." -ForegroundColor Yellow
    
    # Rollback - restore backup
    try {
        $restoreQuery = @"
USE master;
ALTER DATABASE [$Database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [$Database] FROM DISK = '$backupFile' WITH REPLACE;
ALTER DATABASE [$Database] SET MULTI_USER;
"@
        sqlcmd -S $Server -d master -Q $restoreQuery -b
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ? ROLLBACK REU?IT - baza de date restaurat? din backup" -ForegroundColor Green
        }
        else {
            Write-Host "  ?? ROLLBACK MANUAL NECESAR!" -ForegroundColor Red
            Write-Host "  Ruleaz? manual: RESTORE DATABASE [$Database] FROM DISK = '$backupFile'" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "  ?? EROARE LA ROLLBACK: $_" -ForegroundColor Red
        Write-Host "  Restaurare manual? necesar? din: $backupFile" -ForegroundColor Yellow
    }
    
    exit 1
}

Write-Host ""
Write-Host "[4/5] VERIFICARE INTEGRITATE DATE" -ForegroundColor Yellow

try {
    # Verificare tabele create
    $verifyQuery = @"
SELECT 
    'AnalizeMedicaleLaboratoare' AS Tabel,
    COUNT(*) AS Total,
    SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END) AS Active
FROM dbo.AnalizeMedicaleLaboratoare
UNION ALL
SELECT 
    'AnalizeMedicaleCategorii',
    COUNT(*),
    SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END)
FROM dbo.AnalizeMedicaleCategorii
UNION ALL
SELECT 
    'AnalizeMedicale',
    COUNT(*),
    SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END)
FROM dbo.AnalizeMedicale
UNION ALL
SELECT 
    'AnalizeMedicaleIstoricScraping',
    COUNT(*),
    NULL
FROM dbo.AnalizeMedicaleIstoricScraping;
"@

    $verifyOutput = sqlcmd -S $Server -d $Database -Q $verifyQuery -W
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? Verificare integritate OK" -ForegroundColor Green
        Write-Host ""
        Write-Host "  Statistici migrate:" -ForegroundColor Cyan
        Write-Host $verifyOutput
    }
    else {
        Write-Host "  ?? Verificare e?uat? - verific? manual datele" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "  ?? Eroare la verificare: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "[5/5] FINALIZARE" -ForegroundColor Yellow

# Sample date migrate
try {
    $sampleQuery = @"
SELECT TOP 5
    a.NumeAnaliza,
    c.NumeCategorie,
    l.NumeLaborator,
    CAST(a.Pret AS VARCHAR(20)) + ' ' + a.Moneda AS Pret
FROM dbo.AnalizeMedicale a
INNER JOIN dbo.AnalizeMedicaleCategorii c ON a.CategorieID = c.CategorieID
INNER JOIN dbo.AnalizeMedicaleLaboratoare l ON a.LaboratorID = l.LaboratorID
WHERE a.EsteActiv = 1
ORDER BY NEWID();
"@

    Write-Host ""
    Write-Host "  Sample analize migrate (5 random):" -ForegroundColor Cyan
    $sampleOutput = sqlcmd -S $Server -d $Database -Q $sampleQuery -W
    Write-Host $sampleOutput
}
catch {
    # Ignore sample errors
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "? MIGRARE COMPLET? CU SUCCES!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? SUMAR:" -ForegroundColor Cyan
Write-Host "  Backup: $backupFile" -ForegroundColor Gray
Write-Host "  Dimensiune: $sizeMB MB" -ForegroundColor Gray
Write-Host "  Durat? migrare: $([math]::Round($duration, 2)) secunde" -ForegroundColor Gray
Write-Host ""
Write-Host "?? NEXT STEPS:" -ForegroundColor Cyan
Write-Host "  1. Verific? datele în SSMS:" -ForegroundColor White
Write-Host "     SELECT * FROM AnalizeMedicale WHERE EsteActiv = 1" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Continu? implementarea cu:" -ForegroundColor White
Write-Host "     - Creare entit??i Domain (Step 4)" -ForegroundColor Gray
Write-Host "     - Creare DTOs" -ForegroundColor Gray
Write-Host "     - Creare Commands/Queries MediatR" -ForegroundColor Gray
Write-Host "     - Implementare UI Component" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. Dac? întâmpini probleme:" -ForegroundColor White
Write-Host "     - Backup disponibil: $backupFile" -ForegroundColor Gray
Write-Host "     - Log migrare: $outputFile" -ForegroundColor Gray
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

# Pause pentru citire
Write-Host ""
Write-Host "Apas? orice tast? pentru a închide..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
