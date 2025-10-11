# ========================================
# Script Simplificat pentru Extragerea Tabelelor
# ValyanClinic - Simple Table Extraction
# ========================================

$configPath = "..\..\..\ValyanClinic\appsettings.json"
$outputPath = "..\..\Database"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EXTRAGERE SIMPLIFICATA TABELE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Citire configuratie
try {
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    Write-Host "? Connection string incarcat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la configuratie: $_" -ForegroundColor Red
    exit 1
}

# Functie helper pentru query-uri
function Get-DbData {
    param([string]$Query, [string]$ConnectionString)
    
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $ConnectionString
    
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $Query
        
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
        $dataset = New-Object System.Data.DataSet
        [void]$adapter.Fill($dataset)
        
        return $dataset.Tables[0]
    }
    finally {
        if ($connection.State -eq 'Open') {
            $connection.Close()
        }
    }
}

# Lista tabele target
$targetTables = @('Personal', 'PersonalMedical', 'Judete', 'Localitati', 'Departamente')

Write-Host "`n[1/3] Verificare tabele..." -ForegroundColor Yellow

foreach ($tableName in $targetTables) {
    try {
        $checkQuery = "SELECT COUNT(*) as RecordCount FROM $tableName"
        $result = Get-DbData -Query $checkQuery -ConnectionString $connectionString
        $count = $result.Rows[0]['RecordCount']
        Write-Host "  ? $tableName : $count records" -ForegroundColor Green
    }
    catch {
        Write-Host "  ? $tableName : NU EXISTA" -ForegroundColor Red
    }
}

Write-Host "`n[2/3] Extragere structura Personal..." -ForegroundColor Yellow

try {
    $personalStructure = @"
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.is_nullable,
    c.is_identity
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Personal')
ORDER BY c.column_id
"@
    
    $columns = Get-DbData -Query $personalStructure -ConnectionString $connectionString
    
    Write-Host "  Structura tabel Personal:" -ForegroundColor Cyan
    foreach ($col in $columns.Rows) {
        $nullable = if ($col['is_nullable']) { "NULL" } else { "NOT NULL" }
        $identity = if ($col['is_identity']) { "IDENTITY" } else { "" }
        Write-Host "    $($col['ColumnName']) $($col['DataType']) $nullable $identity" -ForegroundColor White
    }
    
    # Generare script CREATE pentru Personal
    $createScript = @"
-- ========================================
-- Tabel: Personal
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [ValyanMed]
GO

IF OBJECT_ID('dbo.Personal', 'U') IS NOT NULL
    DROP TABLE dbo.Personal
GO

CREATE TABLE dbo.Personal (
"@
    
    $columnDefinitions = @()
    foreach ($col in $columns.Rows) {
        $colName = $col['ColumnName']
        $dataType = $col['DataType'].ToUpper()
        
        # Ajustare tip de date
        if ($dataType -in @('VARCHAR', 'NVARCHAR', 'CHAR', 'NCHAR')) {
            $length = if ($col['max_length'] -eq -1) { 'MAX' } 
                     elseif ($dataType -like 'N*') { $col['max_length'] / 2 } 
                     else { $col['max_length'] }
            $dataType += "($length)"
        }
        
        $nullable = if ($col['is_nullable']) { "NULL" } else { "NOT NULL" }
        $identity = if ($col['is_identity']) { "IDENTITY(1,1)" } else { "" }
        
        $columnDefinitions += "    [$colName] $dataType $identity $nullable"
    }
    
    $createScript += ($columnDefinitions -join ",`n")
    $createScript += @"

)
GO

PRINT 'Tabel Personal creat cu succes.'
"@
    
    # Salvare fisier
    if (-not (Test-Path "$outputPath\TableStructure")) {
        New-Item -ItemType Directory -Path "$outputPath\TableStructure" -Force | Out-Null
    }
    
    $filePath = "$outputPath\TableStructure\Personal_Simple.sql"
    $createScript | Out-File -FilePath $filePath -Encoding UTF8
    Write-Host "  ? Personal_Simple.sql generat" -ForegroundColor Green
}
catch {
    Write-Host "  ? Eroare la Personal: $_" -ForegroundColor Red
}

Write-Host "`n[3/3] Extragere stored procedures Personal..." -ForegroundColor Yellow

try {
    $spQuery = "SELECT name FROM sys.procedures WHERE name LIKE 'sp_Personal%' ORDER BY name"
    $procedures = Get-DbData -Query $spQuery -ConnectionString $connectionString
    
    Write-Host "  Stored procedures Personal:" -ForegroundColor Cyan
    foreach ($sp in $procedures.Rows) {
        Write-Host "    - $($sp['name'])" -ForegroundColor White
    }
    
    # Extrage definitia unui SP pentru test
    if ($procedures.Rows.Count -gt 0) {
        $firstSP = $procedures.Rows[0]['name']
        $spDefQuery = @"
SELECT m.definition
FROM sys.sql_modules m
INNER JOIN sys.objects o ON m.object_id = o.object_id
WHERE o.name = '$firstSP'
"@
        
        $spDef = Get-DbData -Query $spDefQuery -ConnectionString $connectionString
        if ($spDef.Rows.Count -gt 0) {
            if (-not (Test-Path "$outputPath\StoredProcedures")) {
                New-Item -ItemType Directory -Path "$outputPath\StoredProcedures" -Force | Out-Null
            }
            
            $spContent = @"
-- ========================================
-- Stored Procedure: $firstSP
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [ValyanMed]
GO

$($spDef.Rows[0]['definition'])
GO
"@
            
            $spFilePath = "$outputPath\StoredProcedures\$firstSP.sql"
            $spContent | Out-File -FilePath $spFilePath -Encoding UTF8
            Write-Host "  ? $firstSP.sql generat" -ForegroundColor Green
        }
    }
}
catch {
    Write-Host "  ? Eroare la SP-uri: $_" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "EXTRAGERE SIMPLIFICATA COMPLETA!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Verifica ce s-a generat
if (Test-Path "$outputPath\TableStructure") {
    $tableFiles = Get-ChildItem "$outputPath\TableStructure" -Filter "*.sql"
    Write-Host "`nFisiere tabele generate: $($tableFiles.Count)" -ForegroundColor Green
    foreach ($file in $tableFiles) {
        Write-Host "  - $($file.Name)" -ForegroundColor White
    }
}

if (Test-Path "$outputPath\StoredProcedures") {
    $spFiles = Get-ChildItem "$outputPath\StoredProcedures" -Filter "*.sql"
    Write-Host "`nFisiere SP generate: $($spFiles.Count)" -ForegroundColor Green
    foreach ($file in $spFiles) {
        Write-Host "  - $($file.Name)" -ForegroundColor White
    }
}