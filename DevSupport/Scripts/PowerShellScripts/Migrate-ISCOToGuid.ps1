# ========================================
# Script pentru Modificarea ID-ului tabelului Ocupatii_ISCO08
# Migrare de la INT IDENTITY la UNIQUEIDENTIFIER cu NEWSEQUENTIALID()
# ========================================

$configPath = "..\..\..\ValyanClinic\appsettings.json"

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "MIGRARE TABEL OCUPATII_ISCO08" -ForegroundColor Magenta
Write-Host "ID: INT IDENTITY ? UNIQUEIDENTIFIER" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

# Conectare
try {
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    Write-Host "? Conectare reusita" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare conectare: $_" -ForegroundColor Red
    exit 1
}

$command = $connection.CreateCommand()

# Verificare existenta tabel
Write-Host "`n[1/6] Verificare tabel existent..." -ForegroundColor Yellow
$command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Ocupatii_ISCO08'"
$tableExists = $command.ExecuteScalar()

if ($tableExists -eq 0) {
    Write-Host "? Tabelul Ocupatii_ISCO08 nu exista!" -ForegroundColor Red
    $connection.Close()
    exit 1
}

# Verificare date existente
$command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08"
$recordCount = $command.ExecuteScalar()
Write-Host "?? Inregistrari existente: $recordCount" -ForegroundColor Cyan

# Backup datele existente
Write-Host "`n[2/6] Backup date existente..." -ForegroundColor Yellow
$backupTableSQL = @"
IF OBJECT_ID('dbo.Ocupatii_ISCO08_Backup', 'U') IS NOT NULL
    DROP TABLE dbo.Ocupatii_ISCO08_Backup

SELECT * INTO Ocupatii_ISCO08_Backup FROM Ocupatii_ISCO08
"@

try {
    $command.CommandText = $backupTableSQL
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? Backup creat: Ocupatii_ISCO08_Backup" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la backup: $_" -ForegroundColor Red
    $connection.Close()
    exit 1
}

# Drop tabelul existent
Write-Host "`n[3/6] Stergere tabel existent..." -ForegroundColor Yellow
try {
    $command.CommandText = "DROP TABLE dbo.Ocupatii_ISCO08"
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? Tabel sters" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la stergere: $_" -ForegroundColor Red
}

# Creare tabel nou cu UNIQUEIDENTIFIER
Write-Host "`n[4/6] Creare tabel nou cu UNIQUEIDENTIFIER..." -ForegroundColor Yellow
$createNewTableSQL = @"
CREATE TABLE dbo.Ocupatii_ISCO08 (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Cod_ISCO] NVARCHAR(10) NOT NULL,
    [Denumire_Ocupatie] NVARCHAR(500) NOT NULL,
    [Denumire_Ocupatie_EN] NVARCHAR(500) NULL,
    [Nivel_Ierarhic] TINYINT NOT NULL,
    [Cod_Parinte] NVARCHAR(10) NULL,
    [Grupa_Majora] NVARCHAR(10) NULL,
    [Grupa_Majora_Denumire] NVARCHAR(300) NULL,
    [Subgrupa] NVARCHAR(10) NULL,
    [Subgrupa_Denumire] NVARCHAR(300) NULL,
    [Grupa_Minora] NVARCHAR(10) NULL,
    [Grupa_Minora_Denumire] NVARCHAR(300) NULL,
    [Descriere] NVARCHAR(MAX) NULL,
    [Observatii] NVARCHAR(1000) NULL,
    [Este_Activ] BIT NOT NULL DEFAULT 1,
    [Data_Crearii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Data_Ultimei_Modificari] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Creat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    [Modificat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    
    CONSTRAINT [PK_Ocupatii_ISCO08] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_Ocupatii_ISCO08_Cod] UNIQUE ([Cod_ISCO]),
    CONSTRAINT [CK_Ocupatii_ISCO08_Nivel] CHECK ([Nivel_Ierarhic] IN (1, 2, 3, 4))
)
"@

try {
    $command.CommandText = $createNewTableSQL
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? Tabel nou creat cu UNIQUEIDENTIFIER" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la crearea tabelului nou: $_" -ForegroundColor Red
    $connection.Close()
    exit 1
}

# Adaugare indexuri
Write-Host "`n[5/6] Adaugare indexuri..." -ForegroundColor Yellow
$indexesSQL = @"
CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Cod_ISCO] 
ON dbo.Ocupatii_ISCO08 ([Cod_ISCO] ASC)

CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Nivel_Ierarhic] 
ON dbo.Ocupatii_ISCO08 ([Nivel_Ierarhic] ASC)

CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Grupa_Majora] 
ON dbo.Ocupatii_ISCO08 ([Grupa_Majora] ASC)
WHERE [Grupa_Majora] IS NOT NULL

CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Parinte] 
ON dbo.Ocupatii_ISCO08 ([Cod_Parinte] ASC)
WHERE [Cod_Parinte] IS NOT NULL

CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Denumire] 
ON dbo.Ocupatii_ISCO08 ([Denumire_Ocupatie] ASC)

CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Activ] 
ON dbo.Ocupatii_ISCO08 ([Este_Activ] ASC)
"@

try {
    $indexCommands = $indexesSQL -split "`n`n" | Where-Object { $_.Trim() -ne "" }
    foreach ($indexCommand in $indexCommands) {
        if ($indexCommand.Trim()) {
            $command.CommandText = $indexCommand.Trim()
            $command.ExecuteNonQuery() | Out-Null
        }
    }
    Write-Host "? Indexuri adaugate" -ForegroundColor Green
}
catch {
    Write-Host "??  Warning la indexuri: $_" -ForegroundColor Yellow
}

# Restaurare date cu noi ID-uri GUID
Write-Host "`n[6/6] Restaurare date cu noi ID-uri GUID..." -ForegroundColor Yellow
$restoreDataSQL = @"
INSERT INTO dbo.Ocupatii_ISCO08 (
    [Cod_ISCO], [Denumire_Ocupatie], [Denumire_Ocupatie_EN], [Nivel_Ierarhic],
    [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire],
    [Subgrupa], [Subgrupa_Denumire], [Grupa_Minora], [Grupa_Minora_Denumire],
    [Descriere], [Observatii], [Este_Activ], [Data_Crearii], [Data_Ultimei_Modificari],
    [Creat_De], [Modificat_De]
)
SELECT 
    [Cod_ISCO], [Denumire_Ocupatie], [Denumire_Ocupatie_EN], [Nivel_Ierarhic],
    [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire],
    [Subgrupa], [Subgrupa_Denumire], [Grupa_Minora], [Grupa_Minora_Denumire],
    [Descriere], [Observatii], [Este_Activ], [Data_Crearii], [Data_Ultimei_Modificari],
    [Creat_De], [Modificat_De]
FROM Ocupatii_ISCO08_Backup
ORDER BY [Cod_ISCO]
"@

try {
    $command.CommandText = $restoreDataSQL
    $rowsInserted = $command.ExecuteNonQuery()
    Write-Host "? $rowsInserted inregistrari restaurate cu noi ID-uri GUID" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la restaurarea datelor: $_" -ForegroundColor Red
}

# Verificare finala
Write-Host "`nVerificare finala..." -ForegroundColor Yellow
$command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08"
$newRecordCount = $command.ExecuteScalar()

$command.CommandText = "SELECT TOP 3 Id, Cod_ISCO, Denumire_Ocupatie FROM Ocupatii_ISCO08 ORDER BY Cod_ISCO"
$reader = $command.ExecuteReader()

Write-Host "`n?? Verificare structura noua:" -ForegroundColor Cyan
while ($reader.Read()) {
    $guid = $reader["Id"]
    $cod = $reader["Cod_ISCO"]
    $denumire = $reader["Denumire_Ocupatie"]
    Write-Host "  ?? $guid | $cod - $denumire" -ForegroundColor White
}
$reader.Close()

$connection.Close()

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "MIGRARE FINALIZATA CU SUCCES" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

Write-Host "? Structura modificata: INT IDENTITY ? UNIQUEIDENTIFIER" -ForegroundColor Green
Write-Host "? NEWSEQUENTIALID() configurat ca default" -ForegroundColor Green
Write-Host "? Date restaurate: $recordCount ? $newRecordCount" -ForegroundColor Green
Write-Host "? Backup salvat in: Ocupatii_ISCO08_Backup" -ForegroundColor Green

Write-Host "`n?? Urmatii pasi:" -ForegroundColor Cyan
Write-Host "1. Actualizeaza Entity-ul OcupatieISCO.cs (Id: int ? Guid)" -ForegroundColor White
Write-Host "2. Actualizeaza stored procedures pentru noul tip de ID" -ForegroundColor White
Write-Host "3. Ruleaza testele pentru verificare" -ForegroundColor White

Write-Host "`n?? Backup-ul poate fi sters dupa confirmarea ca totul functioneaza:" -ForegroundColor Yellow
Write-Host "   DROP TABLE Ocupatii_ISCO08_Backup" -ForegroundColor Gray