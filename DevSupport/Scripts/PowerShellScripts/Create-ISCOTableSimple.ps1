# Script simplificat pentru crearea tabelului ISCO
$configPath = "..\..\..\ValyanClinic\appsettings.json"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CREARE TABEL OCUPATII ISCO-08 SIMPLIFICAT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

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

# Creare tabel simplu
Write-Host "`nCreez tabelul..." -ForegroundColor Yellow

$createTableSQL = @"
IF OBJECT_ID('dbo.Ocupatii_ISCO08', 'U') IS NOT NULL
    DROP TABLE dbo.Ocupatii_ISCO08

CREATE TABLE dbo.Ocupatii_ISCO08 (
    [Id] INT IDENTITY(1,1) NOT NULL,
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
    CONSTRAINT [UK_Ocupatii_ISCO08_Cod] UNIQUE ([Cod_ISCO])
)
"@

try {
    $command = $connection.CreateCommand()
    $command.CommandText = $createTableSQL
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? Tabel creat cu succes" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la crearea tabelului: $_" -ForegroundColor Red
    $connection.Close()
    exit 1
}

# Inserare date exemple simple
Write-Host "`nInserez date exemple..." -ForegroundColor Yellow

$insertedCount = 0

# Date cu valori fixe pentru a evita problemele de sintaxa
$insertSQL1 = "INSERT INTO dbo.Ocupatii_ISCO08 ([Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic], [Grupa_Majora], [Grupa_Majora_Denumire]) VALUES ('0', 'Ocupa?ii din domeniul militar', 1, '0', 'Ocupa?ii din domeniul militar')"
$insertSQL2 = "INSERT INTO dbo.Ocupatii_ISCO08 ([Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic], [Grupa_Majora], [Grupa_Majora_Denumire]) VALUES ('1', 'Manageri', 1, '1', 'Manageri')"
$insertSQL3 = "INSERT INTO dbo.Ocupatii_ISCO08 ([Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic], [Grupa_Majora], [Grupa_Majora_Denumire]) VALUES ('2', 'Profesioni?ti', 1, '2', 'Profesioni?ti')"
$insertSQL4 = "INSERT INTO dbo.Ocupatii_ISCO08 ([Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic], [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire], [Subgrupa], [Subgrupa_Denumire]) VALUES ('22', 'Profesioni?ti în s?n?tate', 2, '2', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate')"
$insertSQL5 = "INSERT INTO dbo.Ocupatii_ISCO08 ([Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic], [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire], [Subgrupa], [Subgrupa_Denumire], [Grupa_Minora], [Grupa_Minora_Denumire]) VALUES ('221', 'Medici', 3, '22', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '221', 'Medici')"
$insertSQL6 = "INSERT INTO dbo.Ocupatii_ISCO08 ([Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic], [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire], [Subgrupa], [Subgrupa_Denumire], [Grupa_Minora], [Grupa_Minora_Denumire]) VALUES ('2211', 'Medici generali?ti', 4, '221', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '221', 'Medici')"
$insertSQL7 = "INSERT INTO dbo.Ocupatii_ISCO08 ([Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic], [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire], [Subgrupa], [Subgrupa_Denumire], [Grupa_Minora], [Grupa_Minora_Denumire]) VALUES ('2212', 'Medici speciali?ti', 4, '221', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '221', 'Medici')"
$insertSQL8 = "INSERT INTO dbo.Ocupatii_ISCO08 ([Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic], [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire], [Subgrupa], [Subgrupa_Denumire]) VALUES ('25', 'Profesioni?ti în IT ?i comunica?ii', 2, '2', '2', 'Profesioni?ti', '25', 'Profesioni?ti în IT ?i comunica?ii')"
$insertSQL9 = "INSERT INTO dbo.Ocupatii_ISCO08 ([Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic], [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire], [Subgrupa], [Subgrupa_Denumire], [Grupa_Minora], [Grupa_Minora_Denumire]) VALUES ('2512', 'Dezvoltatori de software', 4, '25', '2', 'Profesioni?ti', '25', 'Profesioni?ti în IT ?i comunica?ii', '251', 'Analisti si dezvoltatori software')"

$sqlCommands = @($insertSQL1, $insertSQL2, $insertSQL3, $insertSQL4, $insertSQL5, $insertSQL6, $insertSQL7, $insertSQL8, $insertSQL9)

foreach ($sql in $sqlCommands) {
    try {
        $command.CommandText = $sql
        $command.ExecuteNonQuery() | Out-Null
        $insertedCount++
        Write-Host "  ? Inregistrare $insertedCount inserata" -ForegroundColor Green
    }
    catch {
        Write-Host "  ? Eroare la inserarea inregistrarii $insertedCount : $_" -ForegroundColor Red
    }
}

# Verificare finala
Write-Host "`nVerificare finala..." -ForegroundColor Yellow
$command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08"
$totalRecords = $command.ExecuteScalar()

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SETUP FINALIZAT CU SUCCES" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "?? Total inregistrari: $totalRecords" -ForegroundColor Green
Write-Host "?? Inregistrari inserate: $insertedCount" -ForegroundColor Green

if ($totalRecords -gt 0) {
    Write-Host "`n? SUCCES! Tabelul este gata de utilizare" -ForegroundColor Green
    Write-Host "`n?? Test rapid - primele 5 inregistrari:" -ForegroundColor Cyan
    
    $command.CommandText = "SELECT TOP 5 Cod_ISCO, Denumire_Ocupatie, Nivel_Ierarhic FROM Ocupatii_ISCO08 ORDER BY Cod_ISCO"
    $reader = $command.ExecuteReader()
    
    while ($reader.Read()) {
        Write-Host "  $($reader['Cod_ISCO']) - $($reader['Denumire_Ocupatie']) (Nivel $($reader['Nivel_Ierarhic']))" -ForegroundColor White
    }
    $reader.Close()
}

$connection.Close()

Write-Host "`n?? Tabelul Ocupatii_ISCO08 este gata!" -ForegroundColor Green
Write-Host "?? Poti acum sa adaugi entity-ul in aplicatia Blazor" -ForegroundColor Cyan