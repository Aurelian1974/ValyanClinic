# ========================================
# Script Complet pentru Extragerea Tuturor Tabelelor Relevante
# ValyanClinic - Complete Extraction
# ========================================

$configPath = "..\..\..\ValyanClinic\appsettings.json"
$outputPath = "..\..\Database"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EXTRAGERE COMPLETA TABELE RELEVANTE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Citire configuratie
$config = Get-Content $configPath -Raw | ConvertFrom-Json
$connectionString = $config.ConnectionStrings.DefaultConnection

# Conectare
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
$connection.Open()

Write-Host "? Conectat la baza de date ValyanMed" -ForegroundColor Green

# Tabele relevante pentru aplicatie
$relevantTables = @(
    'Personal',
    'PersonalMedical',
    'Judet',
    'Localitate', 
    'Departamente',
    'Pacienti'
)

# Asigura directoarele
if (-not (Test-Path "$outputPath\TableStructure")) {
    New-Item -ItemType Directory -Path "$outputPath\TableStructure" -Force | Out-Null
}
if (-not (Test-Path "$outputPath\StoredProcedures")) {
    New-Item -ItemType Directory -Path "$outputPath\StoredProcedures" -Force | Out-Null
}

Write-Host "`n[1/2] Extragere structura tabele..." -ForegroundColor Yellow

foreach ($tableName in $relevantTables) {
    Write-Host "  Procesez tabel: $tableName" -ForegroundColor Cyan
    
    # Verifica daca tabelul exista
    $checkQuery = "SELECT COUNT(*) FROM sys.tables WHERE name = '$tableName'"
    $command = $connection.CreateCommand()
    $command.CommandText = $checkQuery
    $tableExists = $command.ExecuteScalar()
    
    if ($tableExists -eq 0) {
        Write-Host "    ? Tabel $tableName nu exista" -ForegroundColor Red
        continue
    }
    
    # Extrage structura tabelului
    $structureQuery = @"
SELECT 
    c.name,
    t.name as type_name,
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
    
    $command.CommandText = $structureQuery
    $reader = $command.ExecuteReader()
    
    $columns = @()
    $primaryKeys = @()
    
    while ($reader.Read()) {
        $columnInfo = @{
            Name = $reader["name"]
            Type = $reader["type_name"]
            MaxLength = $reader["max_length"]
            Precision = $reader["precision"]
            Scale = $reader["scale"]
            IsNullable = $reader["is_nullable"]
            IsIdentity = $reader["is_identity"]
            IsPrimaryKey = $reader["IsPrimaryKey"]
        }
        $columns += $columnInfo
        
        if ($columnInfo.IsPrimaryKey -eq 1) {
            $primaryKeys += $columnInfo.Name
        }
    }
    $reader.Close()
    
    if ($columns.Count -gt 0) {
        # Generare script CREATE
        $createScript = @"
-- ========================================
-- Tabel: $tableName
-- Database: ValyanMed
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- Coloane: $($columns.Count)
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
        
        $columnDefs = @()
        foreach ($col in $columns) {
            $colName = $col.Name
            $dataType = $col.Type.ToUpper()
            
            # Construire tip de date cu lungime/precizie
            if ($dataType -in @('VARCHAR', 'NVARCHAR', 'CHAR', 'NCHAR')) {
                $length = if ($col.MaxLength -eq -1) { 'MAX' } 
                         elseif ($dataType -like 'N*') { $col.MaxLength / 2 } 
                         else { $col.MaxLength }
                $dataType += "($length)"
            }
            elseif ($dataType -in @('DECIMAL', 'NUMERIC') -and $col.Precision -gt 0) {
                $dataType += "($($col.Precision),$($col.Scale))"
            }
            elseif ($dataType -eq 'FLOAT' -and $col.Precision -gt 0) {
                $dataType += "($($col.Precision))"
            }
            
            $nullable = if ($col.IsNullable) { "NULL" } else { "NOT NULL" }
            $identity = if ($col.IsIdentity) { "IDENTITY(1,1)" } else { "" }
            
            $columnDefs += "    [$colName] $dataType $identity $nullable"
        }
        
        $createScript += ($columnDefs -join ",`n")
        
        # Adauga cheia primara daca exista
        if ($primaryKeys.Count -gt 0) {
            $pkKeys = $primaryKeys | ForEach-Object { "[$_]" }
            $pkConstraint = "    ,CONSTRAINT [PK_$tableName] PRIMARY KEY (" + ($pkKeys -join ", ") + ")"
            $createScript += ",`n$pkConstraint"
        }
        
        $createScript += @"

)
GO

PRINT 'Tabel $tableName creat cu succes cu $($columns.Count) coloane.'
"@
        
        # Salvare fisier
        $filePath = "$outputPath\TableStructure\$tableName`_Complete.sql"
        $createScript | Out-File -FilePath $filePath -Encoding UTF8
        Write-Host "    ? $tableName`_Complete.sql generat ($($columns.Count) coloane)" -ForegroundColor Green
    }
}

Write-Host "`n[2/2] Extragere stored procedures..." -ForegroundColor Yellow

# Patternuri pentru SP-uri relevante
$spPatterns = @(
    'sp_Personal%',
    'sp_PersonalMedical%', 
    'sp_Judet%',
    'sp_Localitat%',
    'sp_Departament%'
)

$totalSPs = 0

foreach ($pattern in $spPatterns) {
    Write-Host "  Pattern: $pattern" -ForegroundColor Cyan
    
    # Gaseste SP-urile care se potrivesc cu pattern-ul
    $spListQuery = "SELECT name FROM sys.procedures WHERE name LIKE '$pattern' ORDER BY name"
    $command.CommandText = $spListQuery
    $reader = $command.ExecuteReader()
    
    $spNames = @()
    while ($reader.Read()) {
        $spNames += $reader["name"]
    }
    $reader.Close()
    
    Write-Host "    Gasite $($spNames.Count) SP-uri" -ForegroundColor Gray
    
    foreach ($spName in $spNames) {
        Write-Host "      Procesez: $spName" -ForegroundColor Gray
        
        # Extrage definitia SP-ului
        $spDefQuery = @"
SELECT m.definition
FROM sys.sql_modules m
INNER JOIN sys.objects o ON m.object_id = o.object_id
WHERE o.name = '$spName'
"@
        
        $command.CommandText = $spDefQuery
        $spDefinition = $command.ExecuteScalar()
        
        if ($spDefinition) {
            $spContent = @"
-- ========================================
-- Stored Procedure: $spName
-- Database: ValyanMed
-- Pattern: $pattern
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [ValyanMed]
GO

$spDefinition
GO
"@
            
            $spFilePath = "$outputPath\StoredProcedures\$spName.sql"
            $spContent | Out-File -FilePath $spFilePath -Encoding UTF8
            Write-Host "        ? $spName.sql generat" -ForegroundColor Green
            $totalSPs++
        }
    }
}

$connection.Close()

# Generare README actualizat
$readmeContent = @"
# Database Schema Documentation - ValyanMed

This folder contains the extracted database schema from ValyanMed database for ValyanClinic application.

## Last Update
Generated on: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Database Connection
- Server: TS1828\ERP
- Database: ValyanMed
- Authentication: Windows Authentication (Trusted Connection)

## Extracted Tables

"@

foreach ($tableName in $relevantTables) {
    $filePath = "$outputPath\TableStructure\$tableName`_Complete.sql"
    if (Test-Path $filePath) {
        $readmeContent += "- ? **$tableName** - Complete table structure with constraints`n"
    } else {
        $readmeContent += "- ? **$tableName** - Table not found in database`n"
    }
}

$readmeContent += @"

## Extracted Stored Procedures

Total procedures extracted: $totalSPs

### By Pattern:
"@

foreach ($pattern in $spPatterns) {
    $patternFiles = Get-ChildItem "$outputPath\StoredProcedures" -Filter "$($pattern.Replace('%', '*')).sql" -ErrorAction SilentlyContinue
    $readmeContent += "`n- **$pattern** : $($patternFiles.Count) procedures"
}

$readmeContent += @"


## Usage

These files can be used to:
1. **Recreate database structure** in development environments
2. **Compare schema** with entity models in C# code
3. **Document database design** for development team
4. **Track schema changes** over time

## Files Structure

```
Database/
??? README.md                          # This file
??? TableStructure/                    # Table creation scripts
?   ??? Personal_Complete.sql
?   ??? PersonalMedical_Complete.sql
?   ??? Judet_Complete.sql
?   ??? Localitate_Complete.sql
?   ??? Departamente_Complete.sql
?   ??? Pacienti_Complete.sql
??? StoredProcedures/                  # Stored procedure scripts
    ??? sp_Personal_*.sql
    ??? sp_PersonalMedical_*.sql
    ??? sp_Judet_*.sql
    ??? sp_Localitat_*.sql
    ??? sp_Departament_*.sql
```

---
*Generated automatically by ValyanClinic Database Extraction Scripts*
*For updates, run the PowerShell scripts in Scripts/PowerShellScripts/*
"@

$readmeContent | Out-File -FilePath "$outputPath\README.md" -Encoding UTF8

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "EXTRAGERE COMPLETA FINALIZATA!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Statistici finale
$tableFiles = Get-ChildItem "$outputPath\TableStructure" -Filter "*.sql" -ErrorAction SilentlyContinue
$spFiles = Get-ChildItem "$outputPath\StoredProcedures" -Filter "*.sql" -ErrorAction SilentlyContinue

Write-Host "`nRezultat final:" -ForegroundColor Green
Write-Host "  ? Tabele extrase: $($tableFiles.Count)/$($relevantTables.Count)" -ForegroundColor White
Write-Host "  ? Stored Procedures: $($spFiles.Count)" -ForegroundColor White
Write-Host "  ? README actualizat: README.md" -ForegroundColor White

Write-Host "`nFisiere generate:" -ForegroundColor Cyan
Write-Host "  TableStructure:" -ForegroundColor Yellow
foreach ($file in $tableFiles) {
    Write-Host "    - $($file.Name)" -ForegroundColor White
}

Write-Host "  StoredProcedures (primele 10):" -ForegroundColor Yellow
$spFiles | Select-Object -First 10 | ForEach-Object {
    Write-Host "    - $($_.Name)" -ForegroundColor White
}

if ($spFiles.Count -gt 10) {
    Write-Host "    ... si inca $($spFiles.Count - 10) fisiere" -ForegroundColor Gray
}

Write-Host "`nVezi toate fisierele in: $outputPath" -ForegroundColor Cyan