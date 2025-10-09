# ========================================
# Script pentru Extragerea Tabelelor Specifice ValyanClinic
# Target: Personal, PersonalMedical, Judete, Localitati, Departamente
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "..\..\..\ValyanClinic\appsettings.json",
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "..\..\Database"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EXTRAGERE TABELE SPECIFICE VAYLAN CLINIC" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Lista tabelelor importante pentru aplicatie
$targetTables = @(
    'Personal',
    'PersonalMedical', 
    'Judete',
    'Localitati',
    'Departamente',
    'PozitiiMedicale',
    'Patient',
    'User'
)

$targetStoredProcs = @(
    'sp_Personal_%',
    'sp_PersonalMedical_%',
    'sp_Judete_%',
    'sp_Localitati_%', 
    'sp_Departamente_%',
    'sp_Patient_%',
    'sp_User_%'
)

# ========================================
# CONFIGURARE
# ========================================
Write-Host "`n[1/5] Configurare initiala..." -ForegroundColor Yellow

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

# Functii helper
function Invoke-DbQuery {
    param(
        [string]$Query,
        [string]$ConnectionString,
        [int]$CommandTimeout = 30
    )
    
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $ConnectionString
    
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $Query
        $command.CommandTimeout = $CommandTimeout
        
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

# ========================================
# VERIFICARE CONEXIUNE
# ========================================
Write-Host "`n[2/5] Verificare conexiune..." -ForegroundColor Yellow

try {
    $testQuery = "SELECT DB_NAME() as CurrentDatabase, GETDATE() as CurrentTime"
    $result = Invoke-DbQuery -Query $testQuery -ConnectionString $connectionString
    
    if ($result -and $result.Rows.Count -gt 0) {
        Write-Host "? Conexiune reusita!" -ForegroundColor Green
        Write-Host "  Database: $($result.Rows[0]['CurrentDatabase'])" -ForegroundColor Gray
    }
}
catch {
    Write-Host "? Eroare la conexiune: $_" -ForegroundColor Red
    exit 1
}

# ========================================
# VERIFICARE TABELE EXISTENTE
# ========================================
Write-Host "`n[3/5] Verificare tabele existente..." -ForegroundColor Yellow

$tableListQuery = "SELECT name FROM sys.tables WHERE name IN ('" + ($targetTables -join "','") + "') ORDER BY name"

try {
    $existingTables = Invoke-DbQuery -Query $tableListQuery -ConnectionString $connectionString
    
    Write-Host "? Tabele gasite:" -ForegroundColor Green
    $foundTables = @()
    foreach ($row in $existingTables.Rows) {
        $tableName = $row['name']
        $foundTables += $tableName
        Write-Host "  ? $tableName" -ForegroundColor Green
    }
    
    $missingTables = $targetTables | Where-Object { $_ -notin $foundTables }
    if ($missingTables.Count -gt 0) {
        Write-Host "`n? Tabele care lipsesc:" -ForegroundColor Yellow
        foreach ($table in $missingTables) {
            Write-Host "  - $table" -ForegroundColor Yellow
        }
    }
}
catch {
    Write-Host "? Eroare la verificarea tabelelor: $_" -ForegroundColor Red
    exit 1
}

# ========================================
# EXTRAGERE STRUCTURA TABELE EXISTENTE
# ========================================
Write-Host "`n[4/5] Extragere structura tabele..." -ForegroundColor Yellow

foreach ($tableName in $foundTables) {
    Write-Host "  Procesez: $tableName" -ForegroundColor Gray
    
    # Query pentru a extrage structura completa a tabelei
    $tableStructureQuery = @"
-- Generare script CREATE TABLE pentru $tableName
DECLARE @TableName NVARCHAR(128) = '$tableName'
DECLARE @SQL NVARCHAR(MAX) = ''

-- Header comentarii
SET @SQL = @SQL + '-- ========================================' + CHAR(13) + CHAR(10)
SET @SQL = @SQL + '-- Tabel: ' + @TableName + CHAR(13) + CHAR(10)
SET @SQL = @SQL + '-- Database: $databaseName' + CHAR(13) + CHAR(10)
SET @SQL = @SQL + '-- Generat: ' + CONVERT(VARCHAR, GETDATE(), 120) + CHAR(13) + CHAR(10)
SET @SQL = @SQL + '-- ========================================' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10)

SET @SQL = @SQL + 'USE [$databaseName]' + CHAR(13) + CHAR(10)
SET @SQL = @SQL + 'GO' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10)

-- Drop table if exists
SET @SQL = @SQL + 'IF OBJECT_ID(''dbo.' + @TableName + ''', ''U'') IS NOT NULL' + CHAR(13) + CHAR(10)
SET @SQL = @SQL + 'BEGIN' + CHAR(13) + CHAR(10)
SET @SQL = @SQL + '    DROP TABLE dbo.' + @TableName + CHAR(13) + CHAR(10)
SET @SQL = @SQL + '    PRINT ''Tabel ' + @TableName + ' sters.''' + CHAR(13) + CHAR(10)
SET @SQL = @SQL + 'END' + CHAR(13) + CHAR(10)
SET @SQL = @SQL + 'GO' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10)

-- Create table
SET @SQL = @SQL + 'CREATE TABLE dbo.' + @TableName + ' (' + CHAR(13) + CHAR(10)

-- Adauga coloanele
DECLARE @ColumnSQL NVARCHAR(MAX) = ''
SELECT @ColumnSQL = @ColumnSQL + 
    '    ' + QUOTENAME(c.name) + ' ' +
    UPPER(t.name) + 
    CASE 
        WHEN t.name IN ('varchar', 'nvarchar', 'char', 'nchar') THEN 
            '(' + CASE WHEN c.max_length = -1 THEN 'MAX' 
                      ELSE CAST(CASE WHEN t.name IN ('nvarchar', 'nchar') THEN c.max_length/2 ELSE c.max_length END AS VARCHAR) 
                  END + ')'
        WHEN t.name IN ('decimal', 'numeric') THEN 
            '(' + CAST(c.precision AS VARCHAR) + ',' + CAST(c.scale AS VARCHAR) + ')'
        WHEN t.name IN ('float') THEN 
            '(' + CAST(c.precision AS VARCHAR) + ')'
        WHEN t.name IN ('datetime2', 'time', 'datetimeoffset') THEN 
            '(' + CAST(c.scale AS VARCHAR) + ')'
        ELSE ''
    END +
    CASE WHEN c.is_identity = 1 THEN ' IDENTITY(1,1)' ELSE '' END +
    CASE WHEN c.is_nullable = 0 THEN ' NOT NULL' ELSE ' NULL' END +
    CASE WHEN EXISTS(
        SELECT 1 FROM sys.default_constraints dc 
        WHERE dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    ) THEN ' DEFAULT ' + (
        SELECT dc.definition 
        FROM sys.default_constraints dc 
        WHERE dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    ) ELSE '' END +
    CASE WHEN c.column_id < (SELECT MAX(column_id) FROM sys.columns WHERE object_id = c.object_id) THEN ',' ELSE '' END +
    CHAR(13) + CHAR(10)
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.' + @TableName)
ORDER BY c.column_id

SET @SQL = @SQL + @ColumnSQL

-- Primary Key
DECLARE @PK NVARCHAR(MAX) = ''
DECLARE @PKName NVARCHAR(128) = ''

SELECT 
    @PK = @PK + CASE WHEN @PK = '' THEN '' ELSE ', ' END + QUOTENAME(c.name),
    @PKName = i.name
FROM sys.index_columns ic
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
WHERE i.is_primary_key = 1 AND ic.object_id = OBJECT_ID('dbo.' + @TableName)
ORDER BY ic.key_ordinal

IF @PK <> ''
    SET @SQL = @SQL + '    ,CONSTRAINT ' + QUOTENAME(@PKName) + ' PRIMARY KEY (' + @PK + ')' + CHAR(13) + CHAR(10)

SET @SQL = @SQL + ')' + CHAR(13) + CHAR(10)
SET @SQL = @SQL + 'GO' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10)

-- Adauga foreign keys
DECLARE @FK NVARCHAR(MAX) = ''
SELECT @FK = @FK + 
    'ALTER TABLE dbo.' + @TableName + CHAR(13) + CHAR(10) +
    'ADD CONSTRAINT ' + QUOTENAME(fk.name) + ' FOREIGN KEY (' + 
    QUOTENAME(c1.name) + ') REFERENCES dbo.' + QUOTENAME(t2.name) + '(' + QUOTENAME(c2.name) + ')' +
    CASE WHEN fk.delete_referential_action > 0 THEN 
        ' ON DELETE ' + 
        CASE fk.delete_referential_action 
            WHEN 1 THEN 'CASCADE'
            WHEN 2 THEN 'SET NULL'
            WHEN 3 THEN 'SET DEFAULT'
        END
    ELSE '' END +
    CASE WHEN fk.update_referential_action > 0 THEN 
        ' ON UPDATE ' + 
        CASE fk.update_referential_action 
            WHEN 1 THEN 'CASCADE'
            WHEN 2 THEN 'SET NULL'
            WHEN 3 THEN 'SET DEFAULT'
        END
    ELSE '' END +
    CHAR(13) + CHAR(10) + 'GO' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10)
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c1 ON fkc.parent_object_id = c1.object_id AND fkc.parent_column_id = c1.column_id
INNER JOIN sys.columns c2 ON fkc.referenced_object_id = c2.object_id AND fkc.referenced_column_id = c2.column_id
INNER JOIN sys.tables t2 ON fkc.referenced_object_id = t2.object_id
WHERE fkc.parent_object_id = OBJECT_ID('dbo.' + @TableName)

IF @FK <> ''
BEGIN
    SET @SQL = @SQL + '-- Foreign Keys' + CHAR(13) + CHAR(10)
    SET @SQL = @SQL + @FK
END

-- Adauga indexes (non-clustered)
DECLARE @IDX NVARCHAR(MAX) = ''
SELECT @IDX = @IDX + 
    'CREATE' + 
    CASE WHEN i.is_unique = 1 THEN ' UNIQUE' ELSE '' END +
    ' NONCLUSTERED INDEX ' + QUOTENAME(i.name) + 
    ' ON dbo.' + @TableName + ' (' +
    STUFF((
        SELECT ', ' + QUOTENAME(c.name) + 
               CASE WHEN ic.is_descending_key = 1 THEN ' DESC' ELSE ' ASC' END
        FROM sys.index_columns ic
        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
        ORDER BY ic.key_ordinal
        FOR XML PATH('')
    ), 1, 2, '') + ')' +
    CASE WHEN EXISTS(
        SELECT 1 FROM sys.index_columns ic2
        WHERE ic2.object_id = i.object_id AND ic2.index_id = i.index_id AND ic2.is_included_column = 1
    ) THEN 
        ' INCLUDE (' + 
        STUFF((
            SELECT ', ' + QUOTENAME(c.name)
            FROM sys.index_columns ic
            INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1
            ORDER BY ic.key_ordinal
            FOR XML PATH('')
        ), 1, 2, '') + ')'
    ELSE '' END +
    CHAR(13) + CHAR(10) + 'GO' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10)
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('dbo.' + @TableName)
    AND i.is_primary_key = 0 
    AND i.type = 2  -- NONCLUSTERED

IF @IDX <> ''
BEGIN
    SET @SQL = @SQL + '-- Indexes' + CHAR(13) + CHAR(10)
    SET @SQL = @SQL + @IDX
END

SET @SQL = @SQL + 'PRINT ''Tabel ' + @TableName + ' creat cu succes.''' + CHAR(13) + CHAR(10)

SELECT @SQL AS TableScript
"@
    
    try {
        $structureResult = Invoke-DbQuery -Query $tableStructureQuery -ConnectionString $connectionString
        if ($structureResult.Rows.Count -gt 0) {
            $tableScript = $structureResult.Rows[0]['TableScript'].ToString()
            
            # Asigura-te ca directorul exista
            if (-not (Test-Path "$OutputPath\TableStructure")) {
                New-Item -ItemType Directory -Path "$OutputPath\TableStructure" -Force | Out-Null
            }
            
            $filePath = "$OutputPath\TableStructure\$tableName`_Complete.sql"
            $tableScript | Out-File -FilePath $filePath -Encoding UTF8
            Write-Host "    ? $tableName`_Complete.sql generat" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "    ? Eroare la $tableName : $_" -ForegroundColor Red
    }
}

# ========================================
# EXTRAGERE STORED PROCEDURES SPECIFICE
# ========================================
Write-Host "`n[5/5] Extragere stored procedures specifice..." -ForegroundColor Yellow

$spPattern = ($targetStoredProcs -join "' OR p.name LIKE '")
$spQuery = @"
SELECT 
    p.name AS ProcedureName,
    p.create_date,
    p.modify_date
FROM sys.procedures p
INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
WHERE s.name = 'dbo' AND (p.name LIKE '$spPattern')
ORDER BY p.name
"@

try {
    $procedures = Invoke-DbQuery -Query $spQuery -ConnectionString $connectionString
    Write-Host "? Gasite $($procedures.Rows.Count) stored procedures relevante" -ForegroundColor Green
    
    if (-not (Test-Path "$OutputPath\StoredProcedures")) {
        New-Item -ItemType Directory -Path "$OutputPath\StoredProcedures" -Force | Out-Null
    }
    
    foreach ($procRow in $procedures.Rows) {
        $procName = $procRow['ProcedureName']
        Write-Host "  Procesez: $procName" -ForegroundColor Gray
        
        # Obtine definitia stored procedure
        $spDefQuery = @"
SELECT m.definition
FROM sys.sql_modules m
INNER JOIN sys.objects o ON m.object_id = o.object_id
WHERE o.name = '$procName' AND o.type = 'P'
"@
        
        try {
            $defResult = Invoke-DbQuery -Query $spDefQuery -ConnectionString $connectionString
            if ($defResult.Rows.Count -gt 0) {
                $definition = $defResult.Rows[0]['definition'].ToString()
                
                $header = @"
-- ========================================
-- Stored Procedure: $procName
-- Database: $databaseName
-- Created: $($procRow['create_date'])
-- Modified: $($procRow['modify_date'])
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [$databaseName]
GO

"@
                $fullScript = $header + $definition + "`nGO"
                $filePath = "$OutputPath\StoredProcedures\$procName.sql"
                $fullScript | Out-File -FilePath $filePath -Encoding UTF8
                Write-Host "    ? $procName.sql generat" -ForegroundColor Green
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
# RAPORT FINAL
# ========================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "EXTRAGERE SPECIFICA COMPLETA!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`nRezumat:" -ForegroundColor Green
Write-Host "  ? Tabele gasite: $($foundTables.Count)/$($targetTables.Count)" -ForegroundColor White
Write-Host "  ? Stored Procedures: $($procedures.Rows.Count)" -ForegroundColor White

if ($missingTables.Count -gt 0) {
    Write-Host "`n? Tabele care lipsesc si trebuie create:" -ForegroundColor Yellow
    foreach ($table in $missingTables) {
        Write-Host "  - $table" -ForegroundColor Red
    }
}

Write-Host "`nFisierele au fost generate in: $OutputPath" -ForegroundColor Cyan