# =============================================
# Script verificare structura tabel Consultatii
# Database: ValyanMed
# =============================================

$serverName = "localhost"
$databaseName = "ValyanMed"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verificare Tabel Consultatii" -ForegroundColor Cyan
Write-Host "Database: $databaseName" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# SQL Query pentru verificare existenta tabel
$checkTableQuery = @"
SELECT 
    CASE WHEN EXISTS (
        SELECT * FROM sys.tables 
        WHERE name = 'Consultatii' AND schema_id = SCHEMA_ID('dbo')
    ) THEN 1 ELSE 0 END AS TableExists
"@

# SQL Query pentru coloane
$checkColumnsQuery = @"
SELECT 
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.IS_NULLABLE,
    c.COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME = 'Consultatii'
AND c.TABLE_SCHEMA = 'dbo'
ORDER BY c.ORDINAL_POSITION
"@

# SQL Query pentru Foreign Keys
$checkFKQuery = @"
SELECT 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS Table_Name,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS Column_Name,
    OBJECT_NAME(fk.referenced_object_id) AS Referenced_Table,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS Referenced_Column
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fc ON fk.object_id = fc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'Consultatii'
ORDER BY fk.name
"@

# SQL Query pentru Primary Keys
$checkPKQuery = @"
SELECT 
    kc.CONSTRAINT_NAME,
    kc.COLUMN_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kc 
    ON tc.CONSTRAINT_NAME = kc.CONSTRAINT_NAME
WHERE tc.TABLE_NAME = 'Consultatii'
AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
"@

# SQL Query pentru Indexes
$checkIndexesQuery = @"
SELECT 
    i.name AS Index_Name,
    i.type_desc AS Index_Type,
    COL_NAME(ic.object_id, ic.column_id) AS Column_Name,
    i.is_unique,
    i.is_primary_key
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('dbo.Consultatii')
AND i.name IS NOT NULL
ORDER BY i.name, ic.key_ordinal
"@

try {
    # Verificare existenta tabel (fara TrustServerCertificate pentru compatibilitate)
    Write-Host "1. Verificare existenta tabel..." -ForegroundColor Yellow
    
    try {
        # Incearca cu TrustServerCertificate (versiuni noi)
        $tableExists = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkTableQuery -TrustServerCertificate -ErrorAction Stop
    } catch {
        # Fallback fara TrustServerCertificate (versiuni vechi)
        $tableExists = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkTableQuery -ErrorAction Stop
    }
    
    if ($tableExists.TableExists -eq 1) {
        Write-Host "   ? Tabelul Consultatii EXISTA" -ForegroundColor Green
        Write-Host ""
        
        # Afisare coloane
        Write-Host "2. Structura coloane:" -ForegroundColor Yellow
        try {
            $columns = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkColumnsQuery -TrustServerCertificate -ErrorAction Stop
        } catch {
            $columns = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkColumnsQuery -ErrorAction Stop
        }
        $columns | Format-Table -AutoSize | Out-String | Write-Host
        
        # Afisare Foreign Keys
        Write-Host "3. Foreign Keys:" -ForegroundColor Yellow
        try {
            $fks = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkFKQuery -TrustServerCertificate -ErrorAction Stop
        } catch {
            $fks = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkFKQuery -ErrorAction Stop
        }
        
        if ($fks) {
            $fks | Format-Table -AutoSize | Out-String | Write-Host
            Write-Host "   ?  ATENTIE: Exista Foreign Keys care trebuie sterse inainte de DROP TABLE!" -ForegroundColor Red
        } else {
            Write-Host "   Nu exista Foreign Keys" -ForegroundColor Gray
        }
        Write-Host ""
        
        # Afisare Primary Keys
        Write-Host "4. Primary Keys:" -ForegroundColor Yellow
        try {
            $pks = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkPKQuery -TrustServerCertificate -ErrorAction Stop
        } catch {
            $pks = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkPKQuery -ErrorAction Stop
        }
        
        if ($pks) {
            $pks | Format-Table -AutoSize | Out-String | Write-Host
        } else {
            Write-Host "   Nu exista Primary Keys" -ForegroundColor Gray
        }
        Write-Host ""
        
        # Afisare Indexes
        Write-Host "5. Indexes:" -ForegroundColor Yellow
        try {
            $indexes = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkIndexesQuery -TrustServerCertificate -ErrorAction Stop
        } catch {
            $indexes = Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -Query $checkIndexesQuery -ErrorAction Stop
        }
        
        if ($indexes) {
            $indexes | Format-Table -AutoSize | Out-String | Write-Host
        } else {
            Write-Host "   Nu exista Indexes (fara Primary Key)" -ForegroundColor Gray
        }
        Write-Host ""
        
        # Comparatie cu structura noua
        Write-Host "6. Comparatie structura:" -ForegroundColor Yellow
        Write-Host "   Structura VECHE (existenta):" -ForegroundColor Cyan
        Write-Host "   - 9 coloane: ConsultatieID, ProgramareID, PlangereaPrincipala, IstoricBoalaActuala," -ForegroundColor Gray
        Write-Host "     ExamenFizic, Evaluare, Plan, DataConsultatie, Durata" -ForegroundColor Gray
        Write-Host ""
        Write-Host "   Structura NOUA (necesara):" -ForegroundColor Cyan
        Write-Host "   - 80+ coloane pentru Scrisoare Medicala Completa" -ForegroundColor Gray
        Write-Host "   - Sectiuni: Motive, Antecedente, Examen Obiectiv, Investigatii, Diagnostic, Tratament" -ForegroundColor Gray
        Write-Host ""
        Write-Host "   ?  CONCLUZIE: Structura TREBUIE RECREATA COMPLET!" -ForegroundColor Red
        Write-Host ""
        
        # Generare script DROP cu stergere FK
        Write-Host "7. Script DROP generat:" -ForegroundColor Yellow
        
        # Creare director daca nu exista
        $scriptDir = "DevSupport\Database\Scripts"
        if (-not (Test-Path $scriptDir)) {
            New-Item -ItemType Directory -Path $scriptDir -Force | Out-Null
        }
        
        $scriptPath = "$scriptDir\DROP_Consultatii_WithFK.sql"
        Write-Host "   Salvat in: $scriptPath" -ForegroundColor Cyan
        
        $dropScript = @"
-- =============================================
-- Script DROP Consultatii cu stergere FK
-- Generat: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
-- =============================================

USE ValyanMed;
GO

PRINT 'Incepere stergere tabel Consultatii...';

-- 1. Stergere Foreign Keys
"@
        
        if ($fks) {
            foreach ($fk in $fks) {
                $dropScript += @"

PRINT 'Stergere FK: $($fk.FK_Name)';
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = '$($fk.FK_Name)')
BEGIN
    ALTER TABLE dbo.$($fk.Table_Name) DROP CONSTRAINT [$($fk.FK_Name)];
    PRINT '  ? FK $($fk.FK_Name) sters';
END
"@
            }
        }
        
        $dropScript += @"

GO

-- 2. Stergere Tabel
PRINT 'Stergere tabel Consultatii...';
IF OBJECT_ID('dbo.Consultatii', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Consultatii;
    PRINT '  ? Tabel Consultatii sters';
END
ELSE
BEGIN
    PRINT '  ? Tabelul nu exista';
END
GO

PRINT 'Proces finalizat!';
"@
        
        # Salvare script
        $dropScript | Out-File -FilePath $scriptPath -Encoding UTF8
        Write-Host "   ? Script salvat cu succes" -ForegroundColor Green
        
    } else {
        Write-Host "   ? Tabelul Consultatii NU EXISTA" -ForegroundColor Green
        Write-Host "   ? Poti rula direct scriptul de CREATE" -ForegroundColor Cyan
    }
    
} catch {
    Write-Host "? EROARE: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack Trace: $($_.Exception.StackTrace)" -ForegroundColor Red
    Write-Host ""
    Write-Host "SUGESTII:" -ForegroundColor Yellow
    Write-Host "1. Verifica ca SQL Server este pornit" -ForegroundColor Gray
    Write-Host "2. Verifica conexiunea: sqlcmd -S localhost -E -Q `"SELECT @@VERSION`"" -ForegroundColor Gray
    Write-Host "3. Instaleaza SqlServer module: Install-Module -Name SqlServer -Force" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verificare finalizata!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
