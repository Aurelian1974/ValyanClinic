# ========================================
# Script Master de Sincronizare Cod-Database
# ValyanClinic - Full Alignment
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SINCRONIZARE COMPLETA COD-DATABASE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$ErrorActionPreference = "Continue"

Write-Host "`n[FAZA 1] Analiza situatie curenta..." -ForegroundColor Yellow

# Citeste connection string
$configPath = "..\..\ValyanClinic\appsettings.json"
$config = Get-Content $configPath -Raw | ConvertFrom-Json
$connectionString = $config.ConnectionStrings.DefaultConnection

Write-Host "? Connection string incarcat" -ForegroundColor Green
Write-Host "  Database: $($connectionString -split 'Database=' | Select-Object -Last 1 | ForEach-Object { ($_ -split ';')[0] })" -ForegroundColor Gray

Write-Host "`n[FAZA 2] Verificare stored procedures existente..." -ForegroundColor Yellow

$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString

try {
    $connection.Open()
    
    $query = @"
SELECT 
    p.name AS ProcedureName,
    SCHEMA_NAME(p.schema_id) AS SchemaName
FROM sys.procedures p
WHERE p.name LIKE 'sp_%'
ORDER BY p.name
"@
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
    $dataset = New-Object System.Data.DataSet
    [void]$adapter.Fill($dataset)
    
    $existingSPs = $dataset.Tables[0]
    
    Write-Host "? Gasite $($existingSPs.Rows.Count) stored procedures" -ForegroundColor Green
    
    # Analizeaza ce SP-uri lipsesc
    $requiredSPs = @(
        # Personal - EXISTA
        'sp_Personal_GetAll',
        'sp_Personal_GetById',
        'sp_Personal_GetStatistics',
        'sp_Personal_CheckUnique',
        'sp_Personal_Create',
        'sp_Personal_Update',
        'sp_Personal_Delete',
        'sp_Personal_GetDropdownOptions',
        
        # PersonalMedical - EXISTA
        'sp_PersonalMedical_GetAll',
        'sp_PersonalMedical_GetById',
        'sp_PersonalMedical_Create',
        'sp_PersonalMedical_Update',
        'sp_PersonalMedical_Delete',
        'sp_PersonalMedical_CheckUnique',
        'sp_PersonalMedical_GetStatistics',
        'sp_PersonalMedical_GetDropdownOptions',
        
        # Location - EXISTA (cu alte nume)
        'sp_Judete_GetAll',
        'sp_Judete_GetById',
        'sp_Localitati_GetAll',
        'sp_Localitati_GetById',
        'sp_Localitati_GetByJudetId',
        
        # Departamente - EXISTA
        'sp_Departamente_GetAll',
        'sp_Departamente_GetByTip'
    )
    
    $missingSPs = @()
    foreach ($sp in $requiredSPs) {
        $exists = $existingSPs.Select("ProcedureName = '$sp'").Count -gt 0
        if (-not $exists) {
            $missingSPs += $sp
        }
    }
    
    if ($missingSPs.Count -gt 0) {
        Write-Host "`n? ATENTIE: $($missingSPs.Count) stored procedures lipsesc:" -ForegroundColor Red
        foreach ($sp in $missingSPs) {
            Write-Host "  - $sp" -ForegroundColor Red
        }
    } else {
        Write-Host "`n? Toate stored procedures necesare exista!" -ForegroundColor Green
    }
    
    Write-Host "`n[FAZA 3] Verificare tabele..." -ForegroundColor Yellow
    
    $tableQuery = @"
SELECT 
    t.name AS TableName,
    COUNT(c.column_id) AS ColumnCount
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE t.name IN ('Personal', 'PersonalMedical', 'Patient', 'User', 'Judete', 'Localitati', 'Departamente')
GROUP BY t.name
ORDER BY t.name
"@
    
    $command.CommandText = $tableQuery
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
    $dataset = New-Object System.Data.DataSet
    [void]$adapter.Fill($dataset)
    
    $existingTables = $dataset.Tables[0]
    
    Write-Host "? Tabele gasite:" -ForegroundColor Green
    foreach ($row in $existingTables.Rows) {
        Write-Host "  - $($row['TableName']): $($row['ColumnCount']) coloane" -ForegroundColor Gray
    }
    
    # Verifica ce tabele lipsesc
    $requiredTables = @('Personal', 'PersonalMedical', 'Judete', 'Localitati', 'Departamente')
    $missingTables = @()
    
    foreach ($table in $requiredTables) {
        $exists = $existingTables.Select("TableName = '$table'").Count -gt 0
        if (-not $exists) {
            $missingTables += $table
        }
    }
    
    if ($missingTables.Count -gt 0) {
        Write-Host "`n? ATENTIE: $($missingTables.Count) tabele necesare lipsesc:" -ForegroundColor Red
        foreach ($table in $missingTables) {
            Write-Host "  - $table" -ForegroundColor Red
        }
    }
    
    Write-Host "`n[FAZA 4] Generare plan de actiune..." -ForegroundColor Yellow
    
    $actionPlan = @"

========================================
PLAN DE ACTIUNE - SINCRONIZARE
========================================

PRIORITATE 1 - TABELE EXISTENTE (FUNCTIONAL)
--------------------------------------------
? Personal - $($existingTables.Select("TableName = 'Personal'").Count -gt 0 ? "EXISTA" : "LIPSESTE")
? PersonalMedical - $($existingTables.Select("TableName = 'PersonalMedical'").Count -gt 0 ? "EXISTA" : "LIPSESTE")
? Judete - $($existingTables.Select("TableName = 'Judete'").Count -gt 0 ? "EXISTA" : "LIPSESTE")
? Localitati - $($existingTables.Select("TableName = 'Localitati'").Count -gt 0 ? "EXISTA" : "LIPSESTE")
? Departamente - $($existingTables.Select("TableName = 'Departamente'").Count -gt 0 ? "EXISTA" : "LIPSESTE")

ACTIUNI NECESARE:
1. Actualizare Domain Entities (Personal, PersonalMedical) - COMPLETAT
2. Actualizare Repository Interfaces - COMPLETAT
3. Actualizare Repository Implementations - IN LUCRU
4. Actualizare CQRS Features - IN LUCRU
5. Stergere cod pentru Patient, User (tabele lipsa) - NECESAR

PRIORITATE 2 - COD DE STERS (NON-FUNCTIONAL)
--------------------------------------------
? Patient.cs - STERGE (tabel nu exista)
? User.cs - STERGE (tabel nu exista sau structura diferita)
? PatientRepository.cs - STERGE
? UserRepository.cs - STERGE
? Features pentru Patient/User - STERGE

PRIORITATE 3 - STORED PROCEDURES
---------------------------------
$(if ($missingSPs.Count -gt 0) {
    "Lipsesc $($missingSPs.Count) SP-uri - trebuie create"
} else {
    "Toate SP-urile necesare exista!"
})

ESTIMARE TIMP:
- Actualizare Repositories: 2 ore
- Actualizare CQRS Features: 2 ore
- Stergere cod nefolosit: 30 min
- Creare SP-uri lipsa: 1 ora
- Testing: 1 ora
TOTAL: 6.5 ore

========================================
"@
    
    Write-Host $actionPlan -ForegroundColor White
    
    # Salvare raport
    $reportPath = ".\Database_Sync_Report.txt"
    $actionPlan | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Host "`n? Raport salvat: $reportPath" -ForegroundColor Green
    
} catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
} finally {
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "ANALIZA COMPLETA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nUrmatorul pas: Executare plan de actiune" -ForegroundColor Yellow
Write-Host "Rulati: .\Execute-SyncPlan.ps1" -ForegroundColor Cyan
