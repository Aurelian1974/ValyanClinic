# ========================================
# Script Investigare Baz? Date HelperValyanMed
# Obiectiv: Identificare structur? tabele Analize Medicale
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$Server = "DESKTOP-3Q8HI82\ERP",
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "HelperValyanMed"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "INVESTIGARE STRUCTUR? ANALIZE MEDICALE" -ForegroundColor Cyan
Write-Host "Database: $Database" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Connection String
$connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"

# Func?ie executare query
function Invoke-DbQuery {
    param(
        [string]$Query,
        [string]$ConnectionString
    )
 
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $ConnectionString
    
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $Query
        $command.CommandTimeout = 30
        
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
        $dataset = New-Object System.Data.DataSet
        [void]$adapter.Fill($dataset)
        
        return $dataset.Tables[0]
    }
    catch {
        Write-Host "Eroare: $_" -ForegroundColor Red
        return $null
    }
    finally {
        if ($connection.State -eq 'Open') {
            $connection.Close()
        }
    }
}

# ========================================
# TEST CONEXIUNE
# ========================================
Write-Host "[1/6] Testare conexiune..." -ForegroundColor Yellow
try {
    $testQuery = "SELECT @@VERSION as Version, DB_NAME() as CurrentDatabase"
    $result = Invoke-DbQuery -Query $testQuery -ConnectionString $connectionString
    Write-Host "? Conexiune reu?it?!" -ForegroundColor Green
    if ($result -and $result.Rows.Count -gt 0) {
        Write-Host "  Database: $($result.Rows[0]['CurrentDatabase'])" -ForegroundColor Gray
    }
    Write-Host ""
}
catch {
    Write-Host "? Eroare conexiune: $_" -ForegroundColor Red
    exit 1
}

# ========================================
# G?SIRE TABELE CU "ANALIZ*"
# ========================================
Write-Host "[2/6] C?utare tabele cu 'Analiz*'..." -ForegroundColor Yellow
$searchTablesQuery = @"
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%Analiz%'
   OR TABLE_NAME LIKE '%Medical%'
   OR TABLE_NAME LIKE '%Laborator%'
   OR TABLE_NAME LIKE '%Investigat%'
ORDER BY TABLE_NAME
"@

$tables = Invoke-DbQuery -Query $searchTablesQuery -ConnectionString $connectionString
if ($tables -and $tables.Rows.Count -gt 0) {
    Write-Host "? G?site $($tables.Rows.Count) tabele:" -ForegroundColor Green
    $tables | Format-Table -AutoSize | Out-String | Write-Host
}
else {
    Write-Host "?? NICIO TABEL? G?SIT?!" -ForegroundColor Red
    Write-Host "  Verific? manual în SSMS ce tabele exist? în $Database" -ForegroundColor Yellow
}
Write-Host ""

# ========================================
# PENTRU FIECARE TABEL? G?SIT? - STRUCTUR?
# ========================================
if ($tables -and $tables.Rows.Count -gt 0) {
    foreach ($tableRow in $tables.Rows) {
        $tableName = $tableRow['TABLE_NAME']
        Write-Host "[3/$($tables.Rows.Count + 2)] Analiz? structur?: $tableName" -ForegroundColor Yellow
        
        $structureQuery = @"
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = '$tableName'
ORDER BY ORDINAL_POSITION
"@
        
        $structure = Invoke-DbQuery -Query $structureQuery -ConnectionString $connectionString
        if ($structure) {
            $structure | Format-Table -AutoSize | Out-String | Write-Host
        }
        
        # Sample data
        $sampleQuery = "SELECT TOP 3 * FROM [$tableName]"
        $sampleData = Invoke-DbQuery -Query $sampleQuery -ConnectionString $connectionString
        if ($sampleData -and $sampleData.Rows.Count -gt 0) {
            Write-Host "  ?? Sample Data (primele 3 rânduri):" -ForegroundColor Cyan
            $sampleData | Format-Table -AutoSize | Out-String | Write-Host
        }
        else {
            Write-Host "  ?? Tabel? goal? (0 înregistr?ri)" -ForegroundColor Yellow
        }
        
        # Statistici
        $countQuery = "SELECT COUNT(*) AS Total FROM [$tableName]"
        $count = Invoke-DbQuery -Query $countQuery -ConnectionString $connectionString
        Write-Host "  Total înregistr?ri: $($count.Rows[0]['Total'])" -ForegroundColor Gray
        Write-Host ""
    }
}

# ========================================
# C?UTARE STORED PROCEDURES
# ========================================
Write-Host "[5/6] C?utare Stored Procedures pentru analize..." -ForegroundColor Yellow
$spQuery = @"
SELECT 
    ROUTINE_NAME,
    ROUTINE_TYPE,
    CREATED,
    LAST_ALTERED
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_NAME LIKE '%Analiz%'
   OR ROUTINE_NAME LIKE '%Medical%'
   OR ROUTINE_NAME LIKE '%Laborator%'
ORDER BY ROUTINE_NAME
"@

$storedProcs = Invoke-DbQuery -Query $spQuery -ConnectionString $connectionString
if ($storedProcs -and $storedProcs.Rows.Count -gt 0) {
    Write-Host "? G?site $($storedProcs.Rows.Count) proceduri:" -ForegroundColor Green
    $storedProcs | Format-Table -AutoSize | Out-String | Write-Host
}
else {
    Write-Host "?? Nicio procedur? g?sit?" -ForegroundColor Yellow
}
Write-Host ""

# ========================================
# G?SIRE FOREIGN KEYS
# ========================================
Write-Host "[6/6] Verificare Foreign Keys..." -ForegroundColor Yellow
if ($tables -and $tables.Rows.Count -gt 0) {
    foreach ($tableRow in $tables.Rows) {
        $tableName = $tableRow['TABLE_NAME']
        
        $fkQuery = @"
SELECT 
    fk.name AS FK_Name,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc 
    ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = '$tableName'
"@
        
        $foreignKeys = Invoke-DbQuery -Query $fkQuery -ConnectionString $connectionString
        if ($foreignKeys -and $foreignKeys.Rows.Count -gt 0) {
            Write-Host "  Tabel: $tableName" -ForegroundColor Cyan
            $foreignKeys | Format-Table -AutoSize | Out-String | Write-Host
        }
    }
}

# ========================================
# SUMAR & EXPORT JSON
# ========================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SUMAR INVESTIGARE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$summary = @{
    Database = $Database
    Server = $Server
    TablesFound = if ($tables) { $tables.Rows.Count } else { 0 }
    StoredProcsFound = if ($storedProcs) { $storedProcs.Rows.Count } else { 0 }
    DateInvestigare = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
}

Write-Host "Database: $($summary.Database)" -ForegroundColor Gray
Write-Host "Tabele g?site: $($summary.TablesFound)" -ForegroundColor Gray
Write-Host "Proceduri g?site: $($summary.StoredProcsFound)" -ForegroundColor Gray
Write-Host ""

# Export JSON pentru documentare
$outputJson = ".\AnalizeMedicale_HelperValyanMed_Investigation.json"
$summary | ConvertTo-Json | Out-File -FilePath $outputJson -Encoding UTF8
Write-Host "? Rezultate exportate în: $outputJson" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Investigare complet?!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
