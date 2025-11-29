# ========================================
# Script pentru Verificarea Tabelei Programari
# ValyanClinic - Database Check
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$Server = "DESKTOP-3Q8HI82\ERP",
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "ValyanMed"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "VERIFICARE BAZA DE DATE - PROGRAMARI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Connection String
$connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"

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
  catch {
    Write-Host "Eroare la executarea query-ului: $_" -ForegroundColor Red
        return $null
    }
    finally {
        if ($connection.State -eq 'Open') {
            $connection.Close()
      }
    }
}

# Test conexiune
Write-Host "[1/6] Testare conexiune..." -ForegroundColor Yellow
try {
    $testQuery = "SELECT @@VERSION as Version, DB_NAME() as CurrentDatabase"
    $result = Invoke-DbQuery -Query $testQuery -ConnectionString $connectionString
    Write-Host "? Conexiune reusita!" -ForegroundColor Green
    if ($result -and $result.Rows.Count -gt 0) {
        $versionText = $result.Rows[0]['Version'].ToString()
        $firstLine = ($versionText -split "`n")[0]
     Write-Host "  SQL Server: $firstLine" -ForegroundColor Gray
        Write-Host "  Database: $($result.Rows[0]['CurrentDatabase'])" -ForegroundColor Gray
    }
    Write-Host ""
}
catch {
    Write-Host "? Eroare la conexiune: $_" -ForegroundColor Red
    exit 1
}

# Verificare tabel Programari
Write-Host "[2/6] Verificare tabel Programari..." -ForegroundColor Yellow
$checkTableQuery = @"
SELECT 
    CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Programari') 
    THEN 1 ELSE 0 END AS TableExists
"@

$tableCheck = Invoke-DbQuery -Query $checkTableQuery -ConnectionString $connectionString
if ($tableCheck.Rows[0]['TableExists'] -eq 1) {
    Write-Host "? Tabelul Programari EXISTA!" -ForegroundColor Green
    
    # Extrage structura tabelului
    $structureQuery = @"
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable,
    c.is_identity AS IsIdentity,
    ISNULL(pk.is_primary_key, 0) AS IsPrimaryKey
FROM sys.tables tab
INNER JOIN sys.columns c ON tab.object_id = c.object_id
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
LEFT JOIN (
    SELECT ic.object_id, ic.column_id, 1 as is_primary_key
    FROM sys.index_columns ic
    INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    WHERE i.is_primary_key = 1
) pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
WHERE tab.name = 'Programari'
ORDER BY c.column_id
"@
    
    $structure = Invoke-DbQuery -Query $structureQuery -ConnectionString $connectionString
    Write-Host "`n  Structura tabelului Programari:" -ForegroundColor Cyan
    $structure | Format-Table -AutoSize | Out-String | Write-Host
    
    # Numarare inregistrari
    $countQuery = "SELECT COUNT(*) AS TotalRecords FROM Programari"
    $count = Invoke-DbQuery -Query $countQuery -ConnectionString $connectionString
    Write-Host "  Total inregistrari: $($count.Rows[0]['TotalRecords'])" -ForegroundColor Gray
}
else {
    Write-Host "? Tabelul Programari NU EXISTA!" -ForegroundColor Red
    Write-Host "  Trebuie rulat scriptul: DevSupport/Database/TableStructure/Programari_Complete.sql" -ForegroundColor Yellow
}
Write-Host ""

# Verificare Foreign Keys
Write-Host "[3/6] Verificare Foreign Keys..." -ForegroundColor Yellow
$fkQuery = @"
SELECT 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc 
    ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'Programari'
"@

$foreignKeys = Invoke-DbQuery -Query $fkQuery -ConnectionString $connectionString
if ($foreignKeys -and $foreignKeys.Rows.Count -gt 0) {
    Write-Host "? Foreign Keys gasite: $($foreignKeys.Rows.Count)" -ForegroundColor Green
    $foreignKeys | Format-Table -AutoSize | Out-String | Write-Host
}
else {
    Write-Host "? Nicio Foreign Key gasita pentru Programari" -ForegroundColor Red
}
Write-Host ""

# Verificare Indexes
Write-Host "[4/6] Verificare Indexes..." -ForegroundColor Yellow
$indexQuery = @"
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName,
  ic.is_included_column AS IsIncludedColumn
FROM sys.indexes AS i
INNER JOIN sys.index_columns AS ic 
    ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('Programari')
    AND i.type > 0
ORDER BY i.name, ic.key_ordinal
"@

$indexes = Invoke-DbQuery -Query $indexQuery -ConnectionString $connectionString
if ($indexes -and $indexes.Rows.Count -gt 0) {
    Write-Host "? Indexes gasite: $($indexes.Rows.Count)" -ForegroundColor Green
    $indexes | Format-Table -AutoSize | Out-String | Write-Host
}
else {
    Write-Host "! Niciun index gasit (in afara de Primary Key)" -ForegroundColor Yellow
}
Write-Host ""

# Verificare Stored Procedures pentru Programari
Write-Host "[5/6] Verificare Stored Procedures..." -ForegroundColor Yellow
$spQuery = @"
SELECT 
    name AS ProcedureName,
    create_date AS CreateDate,
    modify_date AS ModifyDate
FROM sys.procedures
WHERE name LIKE 'sp_Programari_%'
ORDER BY name
"@

$storedProcs = Invoke-DbQuery -Query $spQuery -ConnectionString $connectionString
if ($storedProcs -and $storedProcs.Rows.Count -gt 0) {
    Write-Host "? Stored Procedures gasite: $($storedProcs.Rows.Count)" -ForegroundColor Green
    $storedProcs | Format-Table -AutoSize | Out-String | Write-Host
}
else {
    Write-Host "? NICIO Stored Procedure gasita pentru Programari!" -ForegroundColor Red
    Write-Host "  Trebuie create stored procedures pentru:" -ForegroundColor Yellow
    Write-Host "    - sp_Programari_GetAll" -ForegroundColor Gray
    Write-Host "    - sp_Programari_GetCount" -ForegroundColor Gray
    Write-Host "    - sp_Programari_GetById" -ForegroundColor Gray
    Write-Host "    - sp_Programari_Create" -ForegroundColor Gray
    Write-Host "    - sp_Programari_Update" -ForegroundColor Gray
    Write-Host "    - sp_Programari_Delete" -ForegroundColor Gray
    Write-Host "    - sp_Programari_GetByDoctor" -ForegroundColor Gray
    Write-Host "    - sp_Programari_GetByDate" -ForegroundColor Gray
    Write-Host "- sp_Programari_CheckConflict" -ForegroundColor Gray
}
Write-Host ""

# Verificare tabele relationate
Write-Host "[6/6] Verificare tabele relationate..." -ForegroundColor Yellow
$relatedTablesQuery = @"
SELECT 
    name AS TableName,
    CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Consultatii') THEN 1 ELSE 0 END AS Consultatii_Exists,
    CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Pacienti') THEN 1 ELSE 0 END AS Pacienti_Exists,
    CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PersonalMedical') THEN 1 ELSE 0 END AS PersonalMedical_Exists
FROM (SELECT 'Check' AS name) AS t
"@

$relatedTables = Invoke-DbQuery -Query $relatedTablesQuery -ConnectionString $connectionString
if ($relatedTables -and $relatedTables.Rows.Count -gt 0) {
    $row = $relatedTables.Rows[0]
    
    Write-Host "  Pacienti: $(if ($row['Pacienti_Exists'] -eq 1) { '?' } else { '?' })" -ForegroundColor $(if ($row['Pacienti_Exists'] -eq 1) { 'Green' } else { 'Red' })
    Write-Host "  PersonalMedical: $(if ($row['PersonalMedical_Exists'] -eq 1) { '?' } else { '?' })" -ForegroundColor $(if ($row['PersonalMedical_Exists'] -eq 1) { 'Green' } else { 'Red' })
    Write-Host "  Consultatii: $(if ($row['Consultatii_Exists'] -eq 1) { '?' } else { '?' })" -ForegroundColor $(if ($row['Consultatii_Exists'] -eq 1) { 'Green' } else { 'Red' })
}
Write-Host ""

# Sample data check (daca exista date)
if ($tableCheck.Rows[0]['TableExists'] -eq 1) {
    $sampleQuery = @"
SELECT TOP 5
    ProgramareID,
    PacientID,
    DoctorID,
    DataProgramare,
    TipProgramare,
    Status,
    DataCreare
FROM Programari
ORDER BY DataCreare DESC
"@
    
    $sampleData = Invoke-DbQuery -Query $sampleQuery -ConnectionString $connectionString
    if ($sampleData -and $sampleData.Rows.Count -gt 0) {
        Write-Host "Sample data (ultimele 5 programari):" -ForegroundColor Cyan
        $sampleData | Format-Table -AutoSize | Out-String | Write-Host
    }
    else {
        Write-Host "! Nu exista date in tabelul Programari" -ForegroundColor Yellow
    }
}

# Sumar final
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SUMAR VERIFICARE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$summary = @"

STATUS IMPLEMENTARE PROGRAMARI:

1. Tabel Programari: $(if ($tableCheck.Rows[0]['TableExists'] -eq 1) { '? EXISTA' } else { '? NU EXISTA' })
2. Foreign Keys: $(if ($foreignKeys -and $foreignKeys.Rows.Count -gt 0) { "? $($foreignKeys.Rows.Count) gasite" } else { '? LIPSA' })
3. Indexes: $(if ($indexes -and $indexes.Rows.Count -gt 0) { "? $($indexes.Rows.Count) gasite" } else { '! Doar PK' })
4. Stored Procedures: $(if ($storedProcs -and $storedProcs.Rows.Count -gt 0) { "? $($storedProcs.Rows.Count) gasite" } else { '? LIPSA COMPLET' })

ACTIUNI NECESARE:
"@

Write-Host $summary

if ($tableCheck.Rows[0]['TableExists'] -eq 0) {
    Write-Host "  [ ] Ruleaza: DevSupport/Database/TableStructure/Programari_Complete.sql" -ForegroundColor Red
}

if (-not $storedProcs -or $storedProcs.Rows.Count -eq 0) {
    Write-Host "  [ ] Creeaza stored procedures pentru Programari (vezi planul de implementare)" -ForegroundColor Red
}

if ($relatedTables.Rows[0]['Pacienti_Exists'] -eq 0) {
    Write-Host "  [ ] Verifica tabelul Pacienti (necesar pentru FK)" -ForegroundColor Red
}

if ($relatedTables.Rows[0]['PersonalMedical_Exists'] -eq 0) {
    Write-Host "  [ ] Verifica tabelul PersonalMedical (necesar pentru FK)" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Verificare completa!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
