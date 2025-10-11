# ========================================
# Script pentru Extragerea TUTUROR Tabelelor din Baza de Date
# ValyanClinic - All Tables Extraction
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "..\..\..\ValyanClinic\appsettings.json",
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "..\..\Database"
)

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "EXTRAGERE TOATE TABELELE DIN BAZA DE DATE" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

# ========================================
# CONFIGURARE SI VERIFICARI
# ========================================
Write-Host "`n[1/4] Configurare initiala..." -ForegroundColor Yellow

# Citire configuratie
if (-not (Test-Path $ConfigPath)) {
    Write-Host "? Eroare: Fisierul de configuratie nu exista: $ConfigPath" -ForegroundColor Red
    exit 1
}

try {
    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    Write-Host "? Connection string incarcat cu succes" -ForegroundColor Green
    
    $serverMatch = [regex]::Match($connectionString, "Server=([^;]+)")
    $databaseMatch = [regex]::Match($connectionString, "Database=([^;]+)")
    
    $serverName = if ($serverMatch.Success) { $serverMatch.Groups[1].Value } else { "Unknown" }
    $databaseName = if ($databaseMatch.Success) { $databaseMatch.Groups[1].Value } else { "Unknown" }
    
    Write-Host "  Server: $serverName" -ForegroundColor Gray
    Write-Host "  Database: $databaseName" -ForegroundColor Gray
}
catch {
    Write-Host "? Eroare la citirea configuratiei: $_" -ForegroundColor Red
    exit 1
}

# Conectare la baza de date
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString

try {
    $connection.Open()
    Write-Host "? Conectare reusita la $databaseName" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la conectare: $_" -ForegroundColor Red
    exit 1
}

# Asigura directoarele de output
$outputDirs = @(
    "$OutputPath\TableStructure",
    "$OutputPath\StoredProcedures", 
    "$OutputPath\Functions",
    "$OutputPath\Views"
)

foreach ($dir in $outputDirs) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "? Director creat: $dir" -ForegroundColor Green
    }
}

# ========================================
# EXTRAGERE LISTA TUTUROR TABELELOR
# ========================================
Write-Host "`n[2/4] Extragere lista tuturor tabelelor..." -ForegroundColor Yellow

$command = $connection.CreateCommand()
$command.CommandText = "SELECT name FROM sys.tables ORDER BY name"
$reader = $command.ExecuteReader()

$allTables = @()
while ($reader.Read()) {
    $allTables += $reader["name"]
}
$reader.Close()

Write-Host "? Gasite $($allTables.Count) tabele in total:" -ForegroundColor Green
$allTables | ForEach-Object { Write-Host "  - $_" -ForegroundColor White }

# ========================================
# EXTRAGERE STRUCTURA PENTRU TOATE TABELELE
# ========================================
Write-Host "`n[3/4] Extragere structura pentru toate tabelele..." -ForegroundColor Yellow

$successCount = 0
$errorCount = 0

foreach ($tableName in $allTables) {
    Write-Host "  [$($successCount + $errorCount + 1)/$($allTables.Count)] Procesez: $tableName" -ForegroundColor Cyan
    
    try {
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
            # Extrage informatii despre Foreign Keys
            $fkQuery = @"
SELECT 
    fk.name AS ForeignKeyName,
    c1.name AS ColumnName,
    t2.name AS ReferencedTable,
    c2.name AS ReferencedColumn,
    fk.delete_referential_action,
    fk.update_referential_action
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c1 ON fkc.parent_object_id = c1.object_id AND fkc.parent_column_id = c1.column_id
INNER JOIN sys.columns c2 ON fkc.referenced_object_id = c2.object_id AND fkc.referenced_column_id = c2.column_id
INNER JOIN sys.tables t2 ON fkc.referenced_object_id = t2.object_id
WHERE fkc.parent_object_id = OBJECT_ID('dbo.$tableName')
"@
            
            $command.CommandText = $fkQuery
            $reader = $command.ExecuteReader()
            
            $foreignKeys = @()
            while ($reader.Read()) {
                $foreignKeys += @{
                    Name = $reader["ForeignKeyName"]
                    Column = $reader["ColumnName"]
                    ReferencedTable = $reader["ReferencedTable"]
                    ReferencedColumn = $reader["ReferencedColumn"]
                    DeleteAction = $reader["delete_referential_action"]
                    UpdateAction = $reader["update_referential_action"]
                }
            }
            $reader.Close()
            
            # Extrage informatii despre Indexes
            $indexQuery = @"
SELECT 
    i.name AS IndexName,
    i.is_unique,
    c.name AS ColumnName,
    ic.is_descending_key,
    ic.is_included_column
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('dbo.$tableName')
    AND i.is_primary_key = 0 
    AND i.type = 2
ORDER BY i.name, ic.key_ordinal
"@
            
            $command.CommandText = $indexQuery
            $reader = $command.ExecuteReader()
            
            $indexes = @{}
            while ($reader.Read()) {
                $indexName = $reader["IndexName"]
                if (-not $indexes.ContainsKey($indexName)) {
                    $indexes[$indexName] = @{
                        Name = $indexName
                        IsUnique = $reader["is_unique"]
                        Columns = @()
                        IncludedColumns = @()
                    }
                }
                
                $columnName = $reader["ColumnName"]
                $isDescending = $reader["is_descending_key"]
                $isIncluded = $reader["is_included_column"]
                
                if ($isIncluded) {
                    $indexes[$indexName].IncludedColumns += $columnName
                } else {
                    $sortOrder = if ($isDescending) { " DESC" } else { " ASC" }
                    $indexes[$indexName].Columns += "$columnName$sortOrder"
                }
            }
            $reader.Close()
            
            # Generare script CREATE complet
            $createScript = @"
-- ========================================
-- Tabel: $tableName
-- Database: $databaseName
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- Coloane: $($columns.Count)
-- Primary Keys: $($primaryKeys.Count)
-- Foreign Keys: $($foreignKeys.Count)
-- Indexes: $($indexes.Count)
-- ========================================

USE [$databaseName]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.$tableName', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.$tableName
    PRINT 'Tabel $tableName sters.'
END
GO

-- Create table
CREATE TABLE dbo.$tableName (
"@
            
            # Generare definitii coloane
            $columnDefs = @()
            foreach ($col in $columns) {
                $colName = $col.Name
                $dataType = $col.Type.ToUpper()
                
                # Construire tip de data cu lungime/precizie
                if ($dataType -in @('VARCHAR', 'NVARCHAR', 'CHAR', 'NCHAR')) {
                    $length = if ($col.MaxLength -eq -1) { 'MAX' } 
                             elseif ($dataType -like 'N*') { $col.MaxLength / 2 } 
                             else { $col.MaxLength }
                    $dataType += "($length)"
                }
                elseif ($dataType -in @('DECIMAL', 'NUMERIC') -and $col.Precision -gt 0) {
                    $dataType += "($($col.Precision),$($col.Scale))"
                }
                elseif ($dataType -eq 'FLOAT' -and $col.Precision -gt 0 -and $col.Precision -ne 53) {
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

"@
            
            # Adauga Foreign Keys
            if ($foreignKeys.Count -gt 0) {
                $createScript += "`n-- Foreign Keys`n"
                foreach ($fk in $foreignKeys) {
                    $deleteAction = switch ($fk.DeleteAction) {
                        1 { " ON DELETE CASCADE" }
                        2 { " ON DELETE SET NULL" }
                        3 { " ON DELETE SET DEFAULT" }
                        default { "" }
                    }
                    
                    $updateAction = switch ($fk.UpdateAction) {
                        1 { " ON UPDATE CASCADE" }
                        2 { " ON UPDATE SET NULL" }
                        3 { " ON UPDATE SET DEFAULT" }
                        default { "" }
                    }
                    
                    $createScript += @"
ALTER TABLE dbo.$tableName
ADD CONSTRAINT [$($fk.Name)] FOREIGN KEY ([$($fk.Column)]) 
    REFERENCES dbo.[$($fk.ReferencedTable)] ([$($fk.ReferencedColumn)])$deleteAction$updateAction
GO

"@
                }
            }
            
            # Adauga Indexes
            if ($indexes.Count -gt 0) {
                $createScript += "`n-- Indexes`n"
                foreach ($indexName in $indexes.Keys) {
                    $index = $indexes[$indexName]
                    $uniqueKeyword = if ($index.IsUnique) { "UNIQUE " } else { "" }
                    $columnsStr = $index.Columns -join ", "
                    
                    $includeClause = ""
                    if ($index.IncludedColumns.Count -gt 0) {
                        $includedStr = ($index.IncludedColumns | ForEach-Object { "[$_]" }) -join ", "
                        $includeClause = " INCLUDE ($includedStr)"
                    }
                    
                    $createScript += @"
CREATE ${uniqueKeyword}NONCLUSTERED INDEX [$indexName] 
ON dbo.$tableName ($columnsStr)$includeClause
GO

"@
                }
            }
            
            $createScript += "PRINT 'Tabel $tableName creat cu succes cu $($columns.Count) coloane.'"
            
            # Salvare fisier
            $filePath = "$OutputPath\TableStructure\$tableName`_Complete.sql"
            $createScript | Out-File -FilePath $filePath -Encoding UTF8
            Write-Host "    ? $tableName`_Complete.sql generat ($($columns.Count) coloane)" -ForegroundColor Green
            $successCount++
        }
        else {
            Write-Host "    ? Nicio coloana gasita pentru $tableName" -ForegroundColor Yellow
            $errorCount++
        }
    }
    catch {
        Write-Host "    ? Eroare la $tableName : $_" -ForegroundColor Red
        $errorCount++
        
        # Inchide reader-ul daca este deschis
        if ($reader -and -not $reader.IsClosed) {
            $reader.Close()
        }
    }
}

# ========================================
# EXTRAGERE TOATE STORED PROCEDURES
# ========================================
Write-Host "`n[4/4] Extragere toate stored procedures..." -ForegroundColor Yellow

try {
    $spQuery = "SELECT name, create_date, modify_date FROM sys.procedures ORDER BY name"
    $command.CommandText = $spQuery
    $reader = $command.ExecuteReader()
    
    $allProcedures = @()
    while ($reader.Read()) {
        $allProcedures += @{
            Name = $reader["name"]
            CreateDate = $reader["create_date"]
            ModifyDate = $reader["modify_date"]
        }
    }
    $reader.Close()
    
    Write-Host "? Gasite $($allProcedures.Count) stored procedures" -ForegroundColor Green
    
    $spSuccessCount = 0
    $spErrorCount = 0
    
    foreach ($proc in $allProcedures) {
        $spName = $proc.Name
        Write-Host "  [$($spSuccessCount + $spErrorCount + 1)/$($allProcedures.Count)] Procesez: $spName" -ForegroundColor Gray
        
        try {
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
-- Database: $databaseName
-- Created: $($proc.CreateDate)
-- Modified: $($proc.ModifyDate)
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [$databaseName]
GO

$spDefinition
GO
"@
                
                $spFilePath = "$OutputPath\StoredProcedures\$spName.sql"
                $spContent | Out-File -FilePath $spFilePath -Encoding UTF8
                Write-Host "    ? $spName.sql generat" -ForegroundColor Green
                $spSuccessCount++
            }
            else {
                Write-Host "    ? Nu s-a putut extrage definitia pentru $spName" -ForegroundColor Yellow
                $spErrorCount++
            }
        }
        catch {
            Write-Host "    ? Eroare la $spName : $_" -ForegroundColor Red
            $spErrorCount++
        }
    }
}
catch {
    Write-Host "? Eroare la extragerea stored procedures: $_" -ForegroundColor Red
}

$connection.Close()

# ========================================
# GENERARE RAPORT FINAL COMPLET
# ========================================
Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "EXTRAGERE COMPLETA FINALIZATA!" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

# Generare README actualizat complet
$readmeContent = @"
# Database Schema Documentation - ValyanMed Complete

This folder contains the COMPLETE extracted database schema from ValyanMed database.

## Last Update
Generated on: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Database Connection
- Server: $serverName  
- Database: $databaseName
- Authentication: Windows Authentication (Trusted Connection)

## Complete Extraction Results

### Tables ($successCount/$($allTables.Count) extracted successfully)
"@

foreach ($tableName in $allTables) {
    $filePath = "$OutputPath\TableStructure\$tableName`_Complete.sql"
    if (Test-Path $filePath) {
        $readmeContent += "`n- ? **$tableName** - Complete table structure with constraints and indexes"
    } else {
        $readmeContent += "`n- ? **$tableName** - Extraction failed"
    }
}

$readmeContent += @"

### Stored Procedures ($spSuccessCount/$($allProcedures.Count) extracted successfully)

All stored procedures have been extracted with complete definitions.

## Files Structure

```
Database/
??? README.md                          # This documentation
??? TableStructure/                    # All table creation scripts
?   ??? Audit_Persoana_Complete.sql
?   ??? Audit_Utilizator_Complete.sql
?   ??? ComenziTeste_Complete.sql
?   ??? Consultatii_Complete.sql
?   ??? Departamente_Complete.sql
?   ??? DepartamenteIerarhie_Complete.sql
?   ??? Diagnostice_Complete.sql
?   ??? DispozitiveMedicale_Complete.sql
?   ??? FormulareConsimtamant_Complete.sql
?   ??? IstoricMedical_Complete.sql
?   ??? Judet_Complete.sql
?   ??? Localitate_Complete.sql
?   ??? MaterialeSanitare_Complete.sql
?   ??? Medicament_Complete.sql
?   ??? MedicamenteNoi_Complete.sql
?   ??? Pacienti_Complete.sql
?   ??? Partener_Complete.sql
?   ??? Personal_Complete.sql
?   ??? PersonalMedical_Complete.sql
?   ??? PersonalMedical_Backup_Migration_Complete.sql
?   ??? Prescriptii_Complete.sql
?   ??? Programari_Complete.sql
?   ??? RezultateTeste_Complete.sql
?   ??? RoluriSistem_Complete.sql
?   ??? SemneVitale_Complete.sql
?   ??? TipLocalitate_Complete.sql
?   ??? TipuriTeste_Complete.sql
?   ??? TriajPacienti_Complete.sql
??? StoredProcedures/                  # All stored procedures
    ??? [All SP files].sql
```

## Usage

These files provide:
1. **Complete database recreation** capability
2. **Full schema documentation** for all tables
3. **All stored procedures** with original definitions  
4. **Foreign key relationships** and constraints
5. **Index definitions** for performance optimization
6. **Data type specifications** with exact lengths and precision

## Statistics
- Total Tables: $($allTables.Count)
- Tables Successfully Extracted: $successCount
- Tables with Errors: $errorCount
- Total Stored Procedures: $($allProcedures.Count)
- SP Successfully Extracted: $spSuccessCount
- SP with Errors: $spErrorCount

---
*Generated automatically by Complete Database Extraction Script*
*This represents the FULL database schema as of $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')*
"@

$readmeContent | Out-File -FilePath "$OutputPath\README.md" -Encoding UTF8

# Statistici finale detaliate
Write-Host "`nRezultat final complet:" -ForegroundColor Green
Write-Host "  ? Tabele total: $($allTables.Count)" -ForegroundColor White
Write-Host "  ? Tabele extrase cu succes: $successCount" -ForegroundColor Green
Write-Host "  ? Tabele cu erori: $errorCount" -ForegroundColor $(if($errorCount -gt 0){'Red'}else{'Green'})
Write-Host "  ? Stored Procedures total: $($allProcedures.Count)" -ForegroundColor White  
Write-Host "  ? SP extrase cu succes: $spSuccessCount" -ForegroundColor Green
Write-Host "  ? SP cu erori: $spErrorCount" -ForegroundColor $(if($spErrorCount -gt 0){'Red'}else{'Green'})

Write-Host "`nFisiere generate:" -ForegroundColor Cyan
$tableFiles = Get-ChildItem "$OutputPath\TableStructure" -Filter "*_Complete.sql" -ErrorAction SilentlyContinue
$spFiles = Get-ChildItem "$OutputPath\StoredProcedures" -Filter "*.sql" -ErrorAction SilentlyContinue

Write-Host "  TableStructure: $($tableFiles.Count) fisiere" -ForegroundColor Yellow
Write-Host "  StoredProcedures: $($spFiles.Count) fisiere" -ForegroundColor Yellow

if ($errorCount -gt 0 -or $spErrorCount -gt 0) {
    Write-Host "`n??  Au existat erori in timpul extractiei." -ForegroundColor Yellow
    Write-Host "Verifica log-urile de mai sus pentru detalii." -ForegroundColor Yellow
} else {
    Write-Host "`n? Extragerea completa a fost realizata cu succes!" -ForegroundColor Green
}

Write-Host "`nVezi toate fisierele in: $OutputPath" -ForegroundColor Cyan
Write-Host "README actualizat: $OutputPath\README.md" -ForegroundColor Cyan