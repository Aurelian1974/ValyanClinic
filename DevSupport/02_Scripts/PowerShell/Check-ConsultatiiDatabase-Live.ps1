# =============================================
# Script verificare LIVE structura tabele Consultatii
# Database: ValyanMed
# Server: DESKTOP-3Q8HI82\ERP
# =============================================

$serverName = "DESKTOP-3Q8HI82\ERP"
$databaseName = "ValyanMed"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verificare LIVE Tabele Consultatii" -ForegroundColor Cyan
Write-Host "Database: $databaseName" -ForegroundColor Cyan
Write-Host "Server: $serverName" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# =============================================
# 1. VERIFICARE TABELE EXISTENTE (legate de consultatii)
# =============================================
Write-Host "1. Tabele existente (legate de consultatii):" -ForegroundColor Yellow

$checkTablesQuery = @"
SELECT 
    t.name AS TableName,
    (SELECT COUNT(*) FROM sys.columns c WHERE c.object_id = t.object_id) AS ColumnCount,
    (SELECT COUNT(*) FROM sys.foreign_keys fk WHERE fk.parent_object_id = t.object_id) AS ForeignKeyCount,
    (SELECT COUNT(*) FROM sys.indexes i WHERE i.object_id = t.object_id AND i.is_primary_key = 1) AS HasPrimaryKey
FROM sys.tables t
WHERE t.schema_id = SCHEMA_ID('dbo')
AND (
    t.name LIKE '%Consultati%' 
    OR t.name LIKE '%Diagnostic%' 
    OR t.name LIKE '%Investigati%' 
    OR t.name LIKE '%Retet%'
    OR t.name LIKE '%Prescripti%'
    OR t.name LIKE '%Tratament%'
)
ORDER BY t.name;
"@

try {
    $tables = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkTablesQuery -ErrorAction Stop
    
    if ($tables) {
        $tables | Format-Table -AutoSize
        Write-Host ""
    } else {
        Write-Host "   Nu exista tabele legate de consultatii!" -ForegroundColor Red
        Write-Host ""
    }
} catch {
    Write-Host "? EROARE la verificare tabele: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

# =============================================
# 2. STRUCTURA TABEL CONSULTATII (daca exista)
# =============================================
if ($tables | Where-Object { $_.TableName -eq 'Consultatii' }) {
    Write-Host "2. Structura detaliata tabel Consultatii:" -ForegroundColor Yellow
    
    $checkColumnsQuery = @"
SELECT 
    c.COLUMN_NAME AS ColumnName,
    c.DATA_TYPE AS DataType,
    CASE 
        WHEN c.CHARACTER_MAXIMUM_LENGTH = -1 THEN 'MAX'
        WHEN c.CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN CAST(c.CHARACTER_MAXIMUM_LENGTH AS VARCHAR)
        WHEN c.NUMERIC_PRECISION IS NOT NULL THEN CAST(c.NUMERIC_PRECISION AS VARCHAR) + ',' + CAST(c.NUMERIC_SCALE AS VARCHAR)
        ELSE ''
    END AS Size,
    c.IS_NULLABLE AS IsNullable,
    ISNULL(dc.definition, '') AS DefaultValue,
    CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 'YES' ELSE 'NO' END AS IsPrimaryKey
FROM INFORMATION_SCHEMA.COLUMNS c
LEFT JOIN sys.default_constraints dc ON dc.parent_object_id = OBJECT_ID('dbo.Consultatii')
    AND dc.parent_column_id = COLUMNPROPERTY(OBJECT_ID('dbo.Consultatii'), c.COLUMN_NAME, 'ColumnId')
LEFT JOIN (
    SELECT ku.COLUMN_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
    WHERE tc.TABLE_NAME = 'Consultatii' AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
) pk ON pk.COLUMN_NAME = c.COLUMN_NAME
WHERE c.TABLE_NAME = 'Consultatii'
AND c.TABLE_SCHEMA = 'dbo'
ORDER BY c.ORDINAL_POSITION;
"@
    
    $columns = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkColumnsQuery
    
    Write-Host "   Coloane ($($columns.Count) total):" -ForegroundColor Cyan
    $columns | Format-Table -AutoSize
    Write-Host ""
}

# =============================================
# 3. FOREIGN KEYS TABEL CONSULTATII
# =============================================
if ($tables | Where-Object { $_.TableName -eq 'Consultatii' }) {
    Write-Host "3. Foreign Keys tabel Consultatii:" -ForegroundColor Yellow
    
    $checkFKQuery = @"
SELECT 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS From_Table,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS From_Column,
    OBJECT_NAME(fk.referenced_object_id) AS To_Table,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS To_Column
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fc ON fk.object_id = fc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'Consultatii'
ORDER BY fk.name;
"@
    
    $fks = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkFKQuery
    
    if ($fks) {
        $fks | Format-Table -AutoSize
    } else {
        Write-Host "   Nu exista Foreign Keys!" -ForegroundColor Gray
    }
    Write-Host ""
}

# =============================================
# 4. VERIFICARE STORED PROCEDURES
# =============================================
Write-Host "4. Stored Procedures (legate de consultatii):" -ForegroundColor Yellow

$checkSPQuery = @"
SELECT 
    p.name AS ProcedureName,
    p.create_date AS DateCreated,
    p.modify_date AS DateModified
FROM sys.procedures p
WHERE p.schema_id = SCHEMA_ID('dbo')
AND (
    p.name LIKE '%Consultati%' 
    OR p.name LIKE '%Diagnostic%' 
    OR p.name LIKE '%Investigati%'
)
ORDER BY p.name;
"@

$procedures = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkSPQuery

if ($procedures) {
    $procedures | Format-Table -AutoSize
} else {
    Write-Host "   Nu exista Stored Procedures legate de consultatii!" -ForegroundColor Gray
}
Write-Host ""

# =============================================
# 5. VERIFICARE DATE IN TABELE
# =============================================
Write-Host "5. Numar inregistrari in tabele:" -ForegroundColor Yellow

foreach ($table in $tables) {
    $tableName = $table.TableName
    $countQuery = "SELECT COUNT(*) AS RecordCount FROM dbo.[$tableName];"
    
    try {
        $count = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $countQuery
        Write-Host "   $tableName : $($count.RecordCount) inregistrari" -ForegroundColor Cyan
    } catch {
        Write-Host "   $tableName : EROARE la citire" -ForegroundColor Red
    }
}
Write-Host ""

# =============================================
# 6. VERIFICARE TABELE REFERENTIATE (Pacienti, PersonalMedical, Programari)
# =============================================
Write-Host "6. Verificare tabele referentiate:" -ForegroundColor Yellow

$checkRelatedTablesQuery = @"
SELECT 
    t.name AS TableName,
    pk.COLUMN_NAME AS PrimaryKeyColumn,
    (SELECT COUNT(*) FROM sys.columns c WHERE c.object_id = t.object_id) AS ColumnCount
FROM sys.tables t
LEFT JOIN (
    SELECT ku.TABLE_NAME, ku.COLUMN_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
    WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
) pk ON pk.TABLE_NAME = t.name
WHERE t.schema_id = SCHEMA_ID('dbo')
AND t.name IN ('Pacienti', 'PersonalMedical', 'Programari')
ORDER BY t.name;
"@

$relatedTables = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkRelatedTablesQuery

if ($relatedTables) {
    $relatedTables | Format-Table -AutoSize
    Write-Host ""
    
    # Detalii Primary Keys
    Write-Host "   Detalii Primary Keys:" -ForegroundColor Cyan
    foreach ($rt in $relatedTables) {
        Write-Host "   - $($rt.TableName) ? PK: $($rt.PrimaryKeyColumn)" -ForegroundColor White
    }
} else {
    Write-Host "   ATENTIE: Nu exista tabelele Pacienti, PersonalMedical sau Programari!" -ForegroundColor Red
}
Write-Host ""

# =============================================
# 7. RAPORT FINAL
# =============================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RAPORT FINAL" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$consultatiiExists = $tables | Where-Object { $_.TableName -eq 'Consultatii' }

if ($consultatiiExists) {
    Write-Host "? Tabel Consultatii EXISTA" -ForegroundColor Green
    Write-Host "  - Coloane: $($columns.Count)" -ForegroundColor White
    Write-Host "  - Foreign Keys: $($fks.Count)" -ForegroundColor White
    
    if ($columns.Count -lt 50) {
        Write-Host ""
        Write-Host "?  ATENTIE: Structura pare VECHE (< 50 coloane)!" -ForegroundColor Yellow
        Write-Host "   Scrisoare Medicala Completa necesita ~85 coloane!" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "   RECOMANDARE: Ruleaza scriptul de recreare:" -ForegroundColor Cyan
        Write-Host "   DevSupport\Database\StoredProcedures\Consultatie\Consultatie_StoredProcedures.sql" -ForegroundColor White
    } else {
        Write-Host ""
        Write-Host "? Structura pare OK (>= 50 coloane)" -ForegroundColor Green
    }
} else {
    Write-Host "?  Tabel Consultatii NU EXISTA!" -ForegroundColor Red
    Write-Host ""
    Write-Host "   RECOMANDARE: Ruleaza scriptul de creare:" -ForegroundColor Cyan
    Write-Host "   DevSupport\Database\StoredProcedures\Consultatie\Consultatie_StoredProcedures.sql" -ForegroundColor White
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verificare finalizata!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
