# ========================================
# Script pentru Verificarea Schemei DB si Aliniere Cod
# ValyanClinic - Database Schema Validation
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "..\ValyanClinic\appsettings.json"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "VALIDARE SCHEMA BAZA DE DATE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Citeste connection string din appsettings.json
Write-Host "`n[1/5] Citire configuratie..." -ForegroundColor Yellow
try {
    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    Write-Host "? Connection string incarcat cu succes" -ForegroundColor Green
    Write-Host "  Server: $(($connectionString -split ';')[0] -replace 'Server=','')" -ForegroundColor Gray
    Write-Host "  Database: $(($connectionString -split ';')[1] -replace 'Database=','')" -ForegroundColor Gray
}
catch {
    Write-Host "? Eroare la citirea configuratiei: $_" -ForegroundColor Red
    exit 1
}

# Functie pentru executare query
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
    finally {
        if ($connection.State -eq 'Open') {
            $connection.Close()
        }
    }
}

# Verifica conexiunea
Write-Host "`n[2/5] Testare conexiune..." -ForegroundColor Yellow
try {
    $testQuery = "SELECT @@VERSION as Version, DB_NAME() as CurrentDatabase"
    $result = Invoke-DbQuery -Query $testQuery -ConnectionString $connectionString
    Write-Host "? Conexiune reusita!" -ForegroundColor Green
    if ($result -and $result.Rows.Count -gt 0) {
        $versionText = $result.Rows[0]['Version'].ToString()
        $firstLine = ($versionText -split "`n")[0]
        Write-Host "  SQL Server Version: $firstLine" -ForegroundColor Gray
        Write-Host "  Database: $($result.Rows[0]['CurrentDatabase'])" -ForegroundColor Gray
    }
}
catch {
    Write-Host "? Eroare la conexiune: $_" -ForegroundColor Red
    exit 1
}

# Extrage tabele Personal si PersonalMedical
Write-Host "`n[3/5] Extragere structura tabele..." -ForegroundColor Yellow

$tableSchemaQuery = @"
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable,
    c.is_identity AS IsIdentity,
    ISNULL(pk.is_primary_key, 0) AS IsPrimaryKey
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
LEFT JOIN (
    SELECT ic.object_id, ic.column_id, 1 as is_primary_key
    FROM sys.index_columns ic
    INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    WHERE i.is_primary_key = 1
) pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
WHERE t.name IN ('Personal', 'PersonalMedical', 'Patient', 'User', 'Judete', 'Localitati', 'Departamente', 'PozitiiMedicale')
ORDER BY t.name, c.column_id
"@

try {
    $tableSchema = Invoke-DbQuery -Query $tableSchemaQuery -ConnectionString $connectionString
    Write-Host "? Structura tabelelor extrasa:" -ForegroundColor Green
    
    $tables = $tableSchema | Group-Object TableName
    foreach ($table in $tables) {
        Write-Host "`n  Tabel: $($table.Name)" -ForegroundColor Cyan
        $columns = $table.Group | Select-Object ColumnName, DataType, IsNullable, IsPrimaryKey
        $columns | Format-Table -AutoSize | Out-String | Write-Host
    }
}
catch {
    Write-Host "? Eroare la extragere structura: $_" -ForegroundColor Red
    exit 1
}

# Extrage stored procedures
Write-Host "`n[4/5] Extragere stored procedures..." -ForegroundColor Yellow

$spQuery = @"
SELECT 
    SCHEMA_NAME(p.schema_id) AS SchemaName,
    p.name AS ProcedureName,
    p.create_date AS CreateDate,
    p.modify_date AS ModifyDate
FROM sys.procedures p
WHERE p.name LIKE 'sp_Personal%' 
   OR p.name LIKE 'sp_Patient%'
   OR p.name LIKE 'sp_User%'
   OR p.name LIKE 'sp_Location%'
   OR p.name LIKE 'sp_Lookup%'
   OR p.name LIKE 'sp_Judete%'
   OR p.name LIKE 'sp_Localitati%'
   OR p.name LIKE 'sp_Departamente%'
ORDER BY p.name
"@

try {
    $storedProcs = Invoke-DbQuery -Query $spQuery -ConnectionString $connectionString
    Write-Host "? Stored procedures gasite: $($storedProcs.Rows.Count)" -ForegroundColor Green
    
    Write-Host "`n  Lista Stored Procedures:" -ForegroundColor Cyan
    $storedProcs | Select-Object ProcedureName | Format-Table -AutoSize | Out-String | Write-Host
}
catch {
    Write-Host "? Eroare la extragere SP: $_" -ForegroundColor Red
}

# Generare raport
Write-Host "`n[5/5] Generare raport..." -ForegroundColor Yellow

$reportPath = ".\Database_Schema_Report.md"
$report = @"
# RAPORT SCHEMA BAZA DE DATE
**Generat:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
**Database:** $($result.Rows[0]['CurrentDatabase'])
**Server:** $(($connectionString -split ';')[0] -replace 'Server=','')

## Tabele Analizate

"@

foreach ($table in $tables) {
    $report += @"

### Tabel: $($table.Name)

| Coloana | Tip Date | Nullable | Primary Key |
|---------|----------|----------|-------------|
"@
    foreach ($col in $table.Group) {
        $nullable = if ($col.IsNullable) { "DA" } else { "NU" }
        $pk = if ($col.IsPrimaryKey) { "DA" } else { "" }
        $report += "`n| $($col.ColumnName) | $($col.DataType) | $nullable | $pk |"
    }
}

$report += @"


## Stored Procedures Disponibile

| Procedura | Data Creare | Data Modificare |
|-----------|-------------|-----------------|
"@

foreach ($sp in $storedProcs.Rows) {
    $report += "`n| $($sp['ProcedureName']) | $($sp['CreateDate'].ToString('yyyy-MM-dd')) | $($sp['ModifyDate'].ToString('yyyy-MM-dd')) |"
}

$report += @"


## Recomandari pentru Cod

### Entity Classes
- Verificati ca proprietatile din `Personal.cs`, `PersonalMedical.cs`, `Patient.cs`, `User.cs` corespund cu coloanele din tabele
- Tipurile de date trebuie sa corespunda (ex: `NVARCHAR` -> `string`, `INT` -> `int`, `BIT` -> `bool`)
- Coloanele nullable din DB trebuie sa fie `nullable` si in C# (ex: `string?`, `int?`)

### Repository Methods
- Verificati ca stored procedures folosite in repositories existe in baza de date
- Parametrii SP trebuie sa corespunda cu proprietatile entitatilor

### Connection String
- Server actual: $(($connectionString -split ';')[0] -replace 'Server=','')
- Database actual: $(($connectionString -split ';')[1] -replace 'Database=','')

---
*Generat automat de Database Schema Validation Script*
"@

$report | Out-File -FilePath $reportPath -Encoding UTF8
Write-Host "? Raport generat: $reportPath" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "VALIDARE COMPLETA!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nVerificati raportul pentru detalii complete." -ForegroundColor Green
