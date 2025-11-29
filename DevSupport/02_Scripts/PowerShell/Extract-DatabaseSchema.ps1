# ========================================
# Script Master pentru Extragerea Schemei de Baza de Date
# ValyanClinic - Database Schema Extraction (FIXED)
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "..\..\..\ValyanClinic\appsettings.json",
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "..\..\Database"
)

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EXTRAGERE SCHEMA BAZA DE DATE (CORRIGAT)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# ========================================
# CONFIGURARE SI VERIFICARI INITIALE
# ========================================
Write-Host "`n[1/6] Configurare initiala..." -ForegroundColor Yellow

# Verifica si citeste configuratia
if (-not (Test-Path $ConfigPath)) {
    Write-Host "? Eroare: Fisierul de configuratie nu exista: $ConfigPath" -ForegroundColor Red
    exit 1
}

try {
    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    Write-Host "? Connection string incarcat cu succes" -ForegroundColor Green
    
    # Parseaza connection string pentru informatii
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

# Verifica directoarele de output
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
# EXTRAGERE STRUCTURA TABELE
# ========================================
Write-Host "`n[3/6] Extragere structura tabele..." -ForegroundColor Yellow

$command = $connection.CreateCommand()
$command.CommandText = "SELECT name FROM sys.tables ORDER BY name"
$reader = $command.ExecuteReader()

$allTables = @()
while ($reader.Read()) {
    $allTables += $reader["name"]
}
$reader.Close()

Write-Host "? Gasite $($allTables.Count) tabele" -ForegroundColor Green

$tableSuccessCount = 0
foreach ($tableName in $allTables) {
    Write-Host "  Procesez: $tableName" -ForegroundColor Gray
    
    try {
        # Extrage structura tabelului cu metoda functionala
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
-- Database: $databaseName
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- Coloane: $($columns.Count)
-- ========================================

USE [$databaseName]
GO

IF OBJECT_ID('dbo.$tableName', 'U') IS NOT NULL
    DROP TABLE dbo.$tableName
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

PRINT 'Tabel $tableName creat cu succes.'
"@
            
            $filePath = "$OutputPath\TableStructure\$tableName`_Structure.sql"
            $createScript | Out-File -FilePath $filePath -Encoding UTF8
            Write-Host "    ? $tableName`_Structure.sql" -ForegroundColor Green
            $tableSuccessCount++
        }
    }
    catch {
        Write-Host "    ? Eroare la $tableName : $_" -ForegroundColor Red
        if ($reader -and -not $reader.IsClosed) {
            $reader.Close()
        }
    }
}

# ========================================
# EXTRAGERE STORED PROCEDURES
# ========================================
Write-Host "`n[4/6] Extragere stored procedures..." -ForegroundColor Yellow

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
    foreach ($proc in $allProcedures) {
        $procName = $proc.Name
        Write-Host "  Procesez: $procName" -ForegroundColor Gray
        
        try {
            $spDefQuery = @"
SELECT m.definition
FROM sys.sql_modules m
INNER JOIN sys.objects o ON m.object_id = o.object_id
WHERE o.name = '$procName'
"@
            
            $command.CommandText = $spDefQuery
            $spDefinition = $command.ExecuteScalar()
            
            if ($spDefinition) {
                $header = @"
-- ========================================
-- Stored Procedure: $procName
-- Created: $($proc.CreateDate)
-- Modified: $($proc.ModifyDate)
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [$databaseName]
GO

"@
                $fullScript = $header + $spDefinition + "`nGO"
                $filePath = "$OutputPath\StoredProcedures\$procName.sql"
                $fullScript | Out-File -FilePath $filePath -Encoding UTF8
                Write-Host "    ? $procName.sql" -ForegroundColor Green
                $spSuccessCount++
            }
        }
        catch {
            Write-Host "    ? Eroare la $procName : $_" -ForegroundColor Red
        }
    }
}
catch {
    Write-Host "? Eroare la extragerea stored procedures: $_" -ForegroundColor Red
}

# ========================================
# EXTRAGERE FUNCTIONS
# ========================================
Write-Host "`n[5/6] Extragere functions..." -ForegroundColor Yellow

try {
    $functionQuery = "SELECT name, create_date, modify_date, type_desc FROM sys.objects WHERE type IN ('FN', 'IF', 'TF') ORDER BY name"
    $command.CommandText = $functionQuery
    $reader = $command.ExecuteReader()
    
    $allFunctions = @()
    while ($reader.Read()) {
        $allFunctions += @{
            Name = $reader["name"]
            CreateDate = $reader["create_date"]
            ModifyDate = $reader["modify_date"]
            TypeDesc = $reader["type_desc"]
        }
    }
    $reader.Close()
    
    Write-Host "? Gasite $($allFunctions.Count) functions" -ForegroundColor Green
    
    $funcSuccessCount = 0
    foreach ($func in $allFunctions) {
        $funcName = $func.Name
        Write-Host "  Procesez: $funcName ($($func.TypeDesc))" -ForegroundColor Gray
        
        try {
            $funcDefQuery = @"
SELECT m.definition
FROM sys.sql_modules m
INNER JOIN sys.objects o ON m.object_id = o.object_id
WHERE o.name = '$funcName'
"@
            
            $command.CommandText = $funcDefQuery
            $funcDefinition = $command.ExecuteScalar()
            
            if ($funcDefinition) {
                $header = @"
-- ========================================
-- Function: $funcName
-- Type: $($func.TypeDesc)
-- Created: $($func.CreateDate)
-- Modified: $($func.ModifyDate)
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [$databaseName]
GO

"@
                $fullScript = $header + $funcDefinition + "`nGO"
                $filePath = "$OutputPath\Functions\$funcName.sql"
                $fullScript | Out-File -FilePath $filePath -Encoding UTF8
                Write-Host "    ? $funcName.sql" -ForegroundColor Green
                $funcSuccessCount++
            }
        }
        catch {
            Write-Host "    ? Eroare la $funcName : $_" -ForegroundColor Red
        }
    }
}
catch {
    Write-Host "? Eroare la extragerea functions: $_" -ForegroundColor Red
}

# ========================================
# EXTRAGERE VIEWS
# ========================================
Write-Host "`n[6/6] Extragere views..." -ForegroundColor Yellow

try {
    $viewQuery = "SELECT name, create_date, modify_date FROM sys.views ORDER BY name"
    $command.CommandText = $viewQuery
    $reader = $command.ExecuteReader()
    
    $allViews = @()
    while ($reader.Read()) {
        $allViews += @{
            Name = $reader["name"]
            CreateDate = $reader["create_date"]
            ModifyDate = $reader["modify_date"]
        }
    }
    $reader.Close()
    
    Write-Host "? Gasite $($allViews.Count) views" -ForegroundColor Green
    
    $viewSuccessCount = 0
    foreach ($view in $allViews) {
        $viewName = $view.Name
        Write-Host "  Procesez: $viewName" -ForegroundColor Gray
        
        try {
            $viewDefQuery = @"
SELECT m.definition
FROM sys.sql_modules m
INNER JOIN sys.objects o ON m.object_id = o.object_id
WHERE o.name = '$viewName'
"@
            
            $command.CommandText = $viewDefQuery
            $viewDefinition = $command.ExecuteScalar()
            
            if ($viewDefinition) {
                $header = @"
-- ========================================
-- View: $viewName
-- Created: $($view.CreateDate)
-- Modified: $($view.ModifyDate)
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [$databaseName]
GO

"@
                $fullScript = $header + $viewDefinition + "`nGO"
                $filePath = "$OutputPath\Views\$viewName.sql"
                $fullScript | Out-File -FilePath $filePath -Encoding UTF8
                Write-Host "    ? $viewName.sql" -ForegroundColor Green
                $viewSuccessCount++
            }
        }
        catch {
            Write-Host "    ? Eroare la $viewName : $_" -ForegroundColor Red
        }
    }
}
catch {
    Write-Host "? Eroare la extragerea views: $_" -ForegroundColor Red
}

$connection.Close()

# ========================================
# GENERARE RAPORT FINAL
# ========================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "EXTRAGERE COMPLETA!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Actualizeaza README principal
$readmeContent = @"
# Database Schema Documentation

This folder contains the extracted database schema from $databaseName database.

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
- Server: $serverName
- Database: $databaseName
- Authentication: Windows Authentication (Trusted Connection)

### Statistics
- Tables: $tableSuccessCount/$($allTables.Count)
- Stored Procedures: $spSuccessCount/$($allProcedures.Count)
- Functions: $funcSuccessCount/$($allFunctions.Count)
- Views: $viewSuccessCount/$($allViews.Count)
"@

$readmeContent | Out-File -FilePath "$OutputPath\README.md" -Encoding UTF8

Write-Host "`nStatistici:" -ForegroundColor Green
Write-Host "  ? Tables: $tableSuccessCount/$($allTables.Count)" -ForegroundColor White
Write-Host "  ? Stored Procedures: $spSuccessCount/$($allProcedures.Count)" -ForegroundColor White
Write-Host "  ? Functions: $funcSuccessCount/$($allFunctions.Count)" -ForegroundColor White
Write-Host "  ? Views: $viewSuccessCount/$($allViews.Count)" -ForegroundColor White

Write-Host "`nFisierele au fost generate in: $OutputPath" -ForegroundColor Cyan
Write-Host "Pentru a rula din nou: .\Extract-DatabaseSchema.ps1" -ForegroundColor Gray