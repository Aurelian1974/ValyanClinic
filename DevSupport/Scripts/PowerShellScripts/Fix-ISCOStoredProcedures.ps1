# Script pentru corectarea SP-urilor cu GUID - versiunea simplificata
$configPath = "..\..\..\ValyanClinic\appsettings.json"

Write-Host "========================================" -ForegroundColor Yellow
Write-Host "CORECTARE STORED PROCEDURES GUID" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow

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

# SP Create simplificat
Write-Host "`nCreez sp_Ocupatii_ISCO08_Create (simplificat)..." -ForegroundColor Yellow
$spCreate = @"
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_Create
    @CodISCO NVARCHAR(10),
    @DenumireOcupatie NVARCHAR(500),
    @NivelIerarhic TINYINT,
    @CodParinte NVARCHAR(10) = NULL,
    @GrupaMajora NVARCHAR(10) = NULL,
    @GrupaMajoraDenumire NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM dbo.Ocupatii_ISCO08 WHERE [Cod_ISCO] = @CodISCO)
    BEGIN
        SELECT 'EROARE: Codul ISCO exista deja' AS [Mesaj];
        RETURN -1;
    END
    
    INSERT INTO dbo.Ocupatii_ISCO08 (
        [Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic],
        [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire]
    ) VALUES (
        @CodISCO, @DenumireOcupatie, @NivelIerarhic,
        @CodParinte, @GrupaMajora, @GrupaMajoraDenumire
    );
    
    SELECT 
        [Id], [Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic]
    FROM dbo.Ocupatii_ISCO08 
    WHERE [Cod_ISCO] = @CodISCO;
END
"@

try {
    $command.CommandText = $spCreate
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? sp_Ocupatii_ISCO08_Create creat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

# SP Update simplificat
Write-Host "Creez sp_Ocupatii_ISCO08_Update (simplificat)..." -ForegroundColor Yellow
$spUpdate = @"
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_Update
    @Id UNIQUEIDENTIFIER,
    @DenumireOcupatie NVARCHAR(500),
    @EsteActiv BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM dbo.Ocupatii_ISCO08 WHERE [Id] = @Id)
    BEGIN
        SELECT 'EROARE: ID-ul nu exista' AS [Mesaj];
        RETURN -1;
    END
    
    UPDATE dbo.Ocupatii_ISCO08
    SET 
        [Denumire_Ocupatie] = @DenumireOcupatie,
        [Este_Activ] = @EsteActiv,
        [Data_Ultimei_Modificari] = GETDATE()
    WHERE [Id] = @Id;
    
    SELECT 
        [Id], [Cod_ISCO], [Denumire_Ocupatie], [Este_Activ]
    FROM dbo.Ocupatii_ISCO08 
    WHERE [Id] = @Id;
END
"@

try {
    $command.CommandText = $spUpdate
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? sp_Ocupatii_ISCO08_Update creat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

# SP Delete simplificat
Write-Host "Creez sp_Ocupatii_ISCO08_Delete (simplificat)..." -ForegroundColor Yellow
$spDelete = @"
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_Delete
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM dbo.Ocupatii_ISCO08 WHERE [Id] = @Id)
    BEGIN
        SELECT 'EROARE: ID-ul nu exista' AS [Mesaj];
        RETURN -1;
    END
    
    DELETE FROM dbo.Ocupatii_ISCO08 WHERE [Id] = @Id;
    
    SELECT 'Inregistrare stearsa cu succes' AS [Mesaj];
END
"@

try {
    $command.CommandText = $spDelete
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? sp_Ocupatii_ISCO08_Delete creat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

$connection.Close()

Write-Host "`n? Stored procedures corectate pentru GUID!" -ForegroundColor Green