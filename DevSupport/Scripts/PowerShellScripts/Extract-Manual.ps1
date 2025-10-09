# ========================================
# Script Ultra-Simplu pentru Extragerea Tabelelor
# ValyanClinic - Ultra Simple Extraction
# ========================================

$configPath = "..\..\..\ValyanClinic\appsettings.json"
$outputPath = "..\..\Database"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EXTRAGERE ULTRA-SIMPLA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Citire configuratie
$config = Get-Content $configPath -Raw | ConvertFrom-Json
$connectionString = $config.ConnectionStrings.DefaultConnection

# Conectare directa
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
$connection.Open()

Write-Host "? Conectat la baza de date ValyanMed" -ForegroundColor Green

# Extragere structura tabel Personal
Write-Host "`nExtragere Personal..." -ForegroundColor Yellow

$personalQuery = @"
SELECT 
    c.name,
    t.name as type_name,
    c.max_length,
    c.is_nullable,
    c.is_identity
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Personal')
ORDER BY c.column_id
"@

$command = $connection.CreateCommand()
$command.CommandText = $personalQuery
$reader = $command.ExecuteReader()

$personalColumns = @()
while ($reader.Read()) {
    $personalColumns += @{
        Name = $reader["name"]
        Type = $reader["type_name"] 
        MaxLength = $reader["max_length"]
        IsNullable = $reader["is_nullable"]
        IsIdentity = $reader["is_identity"]
    }
}
$reader.Close()

Write-Host "Gasite $($personalColumns.Count) coloane in Personal" -ForegroundColor Green

# Generare script CREATE pentru Personal
if ($personalColumns.Count -gt 0) {
    $createScript = @"
-- ========================================
-- Tabel: Personal
-- Database: ValyanMed
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [ValyanMed]
GO

IF OBJECT_ID('dbo.Personal', 'U') IS NOT NULL
    DROP TABLE dbo.Personal
GO

CREATE TABLE dbo.Personal (
"@
    
    $columnDefs = @()
    foreach ($col in $personalColumns) {
        $colName = $col.Name
        $dataType = $col.Type.ToUpper()
        
        if ($dataType -in @('VARCHAR', 'NVARCHAR', 'CHAR', 'NCHAR')) {
            $length = if ($col.MaxLength -eq -1) { 'MAX' } 
                     elseif ($dataType -like 'N*') { $col.MaxLength / 2 } 
                     else { $col.MaxLength }
            $dataType += "($length)"
        }
        
        $nullable = if ($col.IsNullable) { "NULL" } else { "NOT NULL" }
        $identity = if ($col.IsIdentity) { "IDENTITY(1,1)" } else { "" }
        
        $columnDefs += "    [$colName] $dataType $identity $nullable"
    }
    
    $createScript += ($columnDefs -join ",`n")
    $createScript += @"

)
GO

PRINT 'Tabel Personal creat cu succes.'
"@
    
    # Asigura directorul
    if (-not (Test-Path "$outputPath\TableStructure")) {
        New-Item -ItemType Directory -Path "$outputPath\TableStructure" -Force | Out-Null
    }
    
    $filePath = "$outputPath\TableStructure\Personal_Manual.sql"
    $createScript | Out-File -FilePath $filePath -Encoding UTF8
    Write-Host "? Personal_Manual.sql generat" -ForegroundColor Green
}

# Extragere un stored procedure
Write-Host "`nExtragere SP_Personal_GetAll..." -ForegroundColor Yellow

$spQuery = @"
SELECT m.definition
FROM sys.sql_modules m
INNER JOIN sys.objects o ON m.object_id = o.object_id
WHERE o.name = 'sp_Personal_GetAll'
"@

$command.CommandText = $spQuery
$spDefinition = $command.ExecuteScalar()

if ($spDefinition) {
    $spContent = @"
-- ========================================
-- Stored Procedure: sp_Personal_GetAll
-- Database: ValyanMed
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [ValyanMed]
GO

$spDefinition
GO
"@
    
    if (-not (Test-Path "$outputPath\StoredProcedures")) {
        New-Item -ItemType Directory -Path "$outputPath\StoredProcedures" -Force | Out-Null
    }
    
    $spFilePath = "$outputPath\StoredProcedures\sp_Personal_GetAll.sql"
    $spContent | Out-File -FilePath $spFilePath -Encoding UTF8
    Write-Host "? sp_Personal_GetAll.sql generat" -ForegroundColor Green
}

$connection.Close()

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "EXTRAGERE COMPLETA!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Verifica rezultatele
if (Test-Path "$outputPath\TableStructure\Personal_Manual.sql") {
    Write-Host "? Fisier tabel generat: Personal_Manual.sql" -ForegroundColor Green
}

if (Test-Path "$outputPath\StoredProcedures\sp_Personal_GetAll.sql") {
    Write-Host "? Fisier SP generat: sp_Personal_GetAll.sql" -ForegroundColor Green
}

Write-Host "`nVerifica directorul: $outputPath" -ForegroundColor Cyan