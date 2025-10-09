# ========================================
# Script Final pentru Extragerea Tabelelor Specifice
# ValyanClinic - Final Table Extraction
# ========================================

$configPath = "..\..\..\ValyanClinic\appsettings.json"
$outputPath = "..\..\Database"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EXTRAGERE FINALA TABELE SPECIFICE" -ForegroundColor Cyan
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

# Lista tabele target (numele corecte din DB)
$targetTables = @(
    'Personal',
    'PersonalMedical', 
    'Judet',
    'Localitate',
    'Departamente',
    'Pacienti'
)

Write-Host "`n[1/4] Verificare tabele existente..." -ForegroundColor Yellow

$existingTables = @()
foreach ($tableName in $targetTables) {
    try {
        $checkQuery = "SELECT COUNT(*) as RecordCount FROM $tableName"
        $result = Get-DbData -Query $checkQuery -ConnectionString $connectionString
        $count = $result.Rows[0]['RecordCount']
        Write-Host "  ? $tableName : $count records" -ForegroundColor Green
        $existingTables += $tableName
    }
    catch {
        Write-Host "  ? $tableName : NU EXISTA" -ForegroundColor Red
    }
}

Write-Host "`n[2/4] Extragere structura tabele..." -ForegroundColor Yellow

# Asigura-te ca directorul exista
if (-not (Test-Path "$outputPath\TableStructure")) {
    New-Item -ItemType Directory -Path "$outputPath\TableStructure" -Force | Out-Null
}

foreach ($tableName in $existingTables) {
    Write-Host "  Procesez: $tableName" -ForegroundColor Cyan
    
    try {
        $structureQuery = @"
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.precision,
    c.scale,
    c.is_nullable,
    c.is_identity,
    CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
LEFT JOIN (
    SELECT ic.object_id, ic.column_id
    FROM sys.index_columns ic
    INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    WHERE i.is_primary_key = 1
) pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
WHERE c.object_id = OBJECT_ID('dbo.$tableName')
ORDER BY c.column_id
"@
        
        $columns = Get-DbData -Query $structureQuery -ConnectionString $connectionString
        
        if ($columns.Rows.Count -gt 0) {
            # Generare script CREATE
            $createScript = @"
-- ========================================
-- Tabel: $tableName
-- Database: ValyanMed
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [ValyanMed]
GO

IF OBJECT_ID('dbo.$tableName', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.$tableName
    PRINT 'Tabel $tableName sters.'
END
GO

CREATE TABLE dbo.$tableName (
"@
            
            $columnDefinitions = @()
            $primaryKeys = @()
            
            for ($i = 0; $i -lt $columns.Rows.Count; $i++) {
                $col = $columns.Rows[$i]
                $colName = $col['ColumnName']
                $dataType = $col['DataType'].ToUpper()
                
                # Ajustare tip de date cu lungime
                if ($dataType -in @('VARCHAR', 'NVARCHAR', 'CHAR', 'NCHAR')) {
                    $length = if ($col['max_length'] -eq -1) { 'MAX' } 
                             elseif ($dataType -like 'N*') { $col['max_length'] / 2 } 
                             else { $col['max_length'] }
                    $dataType += "($length)"
                }
                elseif ($dataType -in @('DECIMAL', 'NUMERIC')) {
                    $dataType += "($($col['precision']),$($col['scale']))"
                }
                
                $nullable = if ($col['is_nullable']) { "NULL" } else { "NOT NULL" }
                $identity = if ($col['is_identity']) { "IDENTITY(1,1)" } else { "" }
                
                $columnDefinitions += "    [$colName] $dataType $identity $nullable"
                
                # Colecteaza cheile primare
                if ($col['IsPrimaryKey'] -eq 1) {
                    $primaryKeys += "[$colName]"
                }
            }
            
            $createScript += ($columnDefinitions -join ",`n")
            
            # Adauga cheia primara daca exista
            if ($primaryKeys.Count -gt 0) {
                $pkConstraint = "    ,CONSTRAINT [PK_$tableName] PRIMARY KEY (" + ($primaryKeys -join ", ") + ")"
                $createScript += ",`n$pkConstraint"
            }
            
            $createScript += @"

)
GO

PRINT 'Tabel $tableName creat cu succes.'
"@
            
            # Salvare fisier
            $filePath = "$outputPath\TableStructure\$tableName`_Complete.sql"
            $createScript | Out-File -FilePath $filePath -Encoding UTF8
            Write-Host "    ? $tableName`_Complete.sql generat" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "    ? Eroare la $tableName : $_" -ForegroundColor Red
    }
}

Write-Host "`n[3/4] Extragere stored procedures..." -ForegroundColor Yellow

# Asigura-te ca directorul exista
if (-not (Test-Path "$outputPath\StoredProcedures")) {
    New-Item -ItemType Directory -Path "$outputPath\StoredProcedures" -Force | Out-Null
}

$spPatterns = @('sp_Personal%', 'sp_PersonalMedical%', 'sp_Judet%', 'sp_Localitat%', 'sp_Departament%')

foreach ($pattern in $spPatterns) {
    try {
        $spQuery = "SELECT name FROM sys.procedures WHERE name LIKE '$pattern' ORDER BY name"
        $procedures = Get-DbData -Query $spQuery -ConnectionString $connectionString
        
        if ($procedures.Rows.Count -gt 0) {
            Write-Host "  Pattern $pattern : $($procedures.Rows.Count) SP-uri" -ForegroundColor Cyan
            
            for ($i = 0; $i -lt $procedures.Rows.Count; $i++) {
                $spName = $procedures.Rows[$i]['name']
                Write-Host "    Procesez: $spName" -ForegroundColor Gray
                
                try {
                    $spDefQuery = @"
SELECT m.definition
FROM sys.sql_modules m
INNER JOIN sys.objects o ON m.object_id = o.object_id
WHERE o.name = '$spName'
"@
                    
                    $spDef = Get-DbData -Query $spDefQuery -ConnectionString $connectionString
                    if ($spDef.Rows.Count -gt 0) {
                        $spContent = @"
-- ========================================
-- Stored Procedure: $spName
-- Database: ValyanMed
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [ValyanMed]
GO

$($spDef.Rows[0]['definition'])
GO
"@
                        
                        $spFilePath = "$outputPath\StoredProcedures\$spName.sql"
                        $spContent | Out-File -FilePath $spFilePath -Encoding UTF8
                        Write-Host "      ? $spName.sql generat" -ForegroundColor Green
                    }
                }
                catch {
                    Write-Host "      ? Eroare la $spName : $_" -ForegroundColor Red
                }
            }
        }
    }
    catch {
        Write-Host "  ? Eroare la pattern $pattern : $_" -ForegroundColor Red
    }
}

Write-Host "`n[4/4] Generare raport final..." -ForegroundColor Yellow

# Generare README actualizat
$readmeContent = @"
# Database Schema Documentation

This folder contains the extracted database schema from ValyanMed database.

## Folder Structure

- **TableStructure/** - Contains table creation scripts and schema definitions
- **StoredProcedures/** - Contains all stored procedures  
- **Functions/** - Contains user-defined functions
- **Views/** - Contains database views

## Generated Files

All files are automatically generated by the PowerShell extraction scripts located in ``Scripts\PowerShellScripts\``.

### Last Update
Generated on: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

### Database Connection
- Server: TS1828\ERP
- Database: ValyanMed
- Authentication: Windows Authentication (Trusted Connection)

### Extracted Tables
"@

foreach ($tableName in $existingTables) {
    $readmeContent += "`n- **$tableName** - $tableName`_Complete.sql"
}

$readmeContent += @"

### Extracted Stored Procedures
Based on patterns: sp_Personal%, sp_PersonalMedical%, sp_Judet%, sp_Localitat%, sp_Departament%

---
*Generated automatically by Final Table Extraction Script*
"@

$readmeContent | Out-File -FilePath "$outputPath\README.md" -Encoding UTF8

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "EXTRAGERE FINALA COMPLETA!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Afiseaza statistici finale
if (Test-Path "$outputPath\TableStructure") {
    $tableFiles = Get-ChildItem "$outputPath\TableStructure" -Filter "*.sql"
    Write-Host "`n? Fisiere tabele generate: $($tableFiles.Count)" -ForegroundColor Green
    foreach ($file in $tableFiles) {
        Write-Host "  - $($file.Name)" -ForegroundColor White
    }
}

if (Test-Path "$outputPath\StoredProcedures") {
    $spFiles = Get-ChildItem "$outputPath\StoredProcedures" -Filter "*.sql"
    Write-Host "`n? Fisiere SP generate: $($spFiles.Count)" -ForegroundColor Green
    foreach ($file in $spFiles) {
        Write-Host "  - $($file.Name)" -ForegroundColor White
    }
}

Write-Host "`nToate fisierele au fost generate in: $outputPath" -ForegroundColor Cyan