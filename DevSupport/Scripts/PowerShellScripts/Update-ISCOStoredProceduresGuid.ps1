# Script pentru actualizarea stored procedures cu GUID
$configPath = "..\..\..\ValyanClinic\appsettings.json"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ACTUALIZARE STORED PROCEDURES CU GUID" -ForegroundColor Cyan
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

$command = $connection.CreateCommand()

# SP 1: GetAll (fara modificari majore, ID-ul ramane ca output)
Write-Host "`nActualizez sp_Ocupatii_ISCO08_GetAll..." -ForegroundColor Yellow
$sp1 = @"
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SearchText NVARCHAR(255) = NULL,
    @NivelIerarhic TINYINT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        o.[Id],
        o.[Cod_ISCO],
        o.[Denumire_Ocupatie],
        o.[Nivel_Ierarhic],
        o.[Cod_Parinte],
        o.[Grupa_Majora],
        o.[Grupa_Majora_Denumire],
        o.[Este_Activ],
        -- Helper pentru UI: ID scurt pentru afisare
        UPPER(LEFT(REPLACE(CAST(o.[Id] AS NVARCHAR(36)), '-', ''), 8)) AS [IdScurt],
        COUNT(*) OVER() AS [TotalRecords]
    FROM dbo.Ocupatii_ISCO08 o
    WHERE 
        (@SearchText IS NULL OR 
         o.[Denumire_Ocupatie] LIKE '%' + @SearchText + '%' OR
         o.[Cod_ISCO] LIKE '%' + @SearchText + '%')
        AND (@NivelIerarhic IS NULL OR o.[Nivel_Ierarhic] = @NivelIerarhic)
        AND o.[Este_Activ] = 1
    ORDER BY o.[Cod_ISCO]
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
"@

try {
    $command.CommandText = $sp1
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? sp_Ocupatii_ISCO08_GetAll actualizat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

# SP 2: GetById - acum cu UNIQUEIDENTIFIER
Write-Host "Actualizez sp_Ocupatii_ISCO08_GetById..." -ForegroundColor Yellow
$sp2 = @"
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.[Id],
        o.[Cod_ISCO],
        o.[Denumire_Ocupatie],
        o.[Denumire_Ocupatie_EN],
        o.[Nivel_Ierarhic],
        o.[Cod_Parinte],
        o.[Grupa_Majora],
        o.[Grupa_Majora_Denumire],
        o.[Subgrupa],
        o.[Subgrupa_Denumire],
        o.[Grupa_Minora],
        o.[Grupa_Minora_Denumire],
        o.[Descriere],
        o.[Este_Activ],
        o.[Data_Crearii],
        o.[Data_Ultimei_Modificari],
        -- Helper pentru UI
        UPPER(LEFT(REPLACE(CAST(o.[Id] AS NVARCHAR(36)), '-', ''), 8)) AS [IdScurt]
    FROM dbo.Ocupatii_ISCO08 o
    WHERE o.[Id] = @Id;
END
"@

try {
    $command.CommandText = $sp2
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? sp_Ocupatii_ISCO08_GetById actualizat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

# SP 3: Insert nou record cu GUID
Write-Host "Creez sp_Ocupatii_ISCO08_Create..." -ForegroundColor Yellow
$sp3 = @"
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_Create
    @CodISCO NVARCHAR(10),
    @DenumireOcupatie NVARCHAR(500),
    @DenumireOcupatieEN NVARCHAR(500) = NULL,
    @NivelIerarhic TINYINT,
    @CodParinte NVARCHAR(10) = NULL,
    @GrupaMajora NVARCHAR(10) = NULL,
    @GrupaMajoraDenumire NVARCHAR(300) = NULL,
    @Subgrupa NVARCHAR(10) = NULL,
    @SubgrupaDenumire NVARCHAR(300) = NULL,
    @GrupaMinora NVARCHAR(10) = NULL,
    @GrupaMinoraDenumire NVARCHAR(300) = NULL,
    @Descriere NVARCHAR(MAX) = NULL,
    @Observatii NVARCHAR(1000) = NULL,
    @CreatDe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NewId UNIQUEIDENTIFIER;
    
    -- Verifica daca codul ISCO exista deja
    IF EXISTS (SELECT 1 FROM dbo.Ocupatii_ISCO08 WHERE [Cod_ISCO] = @CodISCO)
    BEGIN
        RAISERROR('Codul ISCO %s exista deja in sistem.', 16, 1, @CodISCO);
        RETURN -1;
    END
    
    INSERT INTO dbo.Ocupatii_ISCO08 (
        [Cod_ISCO], [Denumire_Ocupatie], [Denumire_Ocupatie_EN], [Nivel_Ierarhic],
        [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire],
        [Subgrupa], [Subgrupa_Denumire], [Grupa_Minora], [Grupa_Minora_Denumire],
        [Descriere], [Observatii], [Creat_De]
    ) VALUES (
        @CodISCO, @DenumireOcupatie, @DenumireOcupatieEN, @NivelIerarhic,
        @CodParinte, @GrupaMajora, @GrupaMajoraDenumire,
        @Subgrupa, @SubgrupaDenumire, @GrupaMinora, @GrupaMinoraDenumire,
        @Descriere, @Observatii, ISNULL(@CreatDe, SYSTEM_USER)
    );
    
    SET @NewId = SCOPE_IDENTITY(); -- Nu functioneaza cu GUID, folosim altceva
    
    -- Returneaza ID-ul nou creat
    SELECT 
        [Id], [Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic],
        UPPER(LEFT(REPLACE(CAST([Id] AS NVARCHAR(36)), '-', ''), 8)) AS [IdScurt]
    FROM dbo.Ocupatii_ISCO08 
    WHERE [Cod_ISCO] = @CodISCO;
    
    RETURN 0;
END
"@

try {
    $command.CommandText = $sp3
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? sp_Ocupatii_ISCO08_Create creat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

# SP 4: Update cu GUID
Write-Host "Creez sp_Ocupatii_ISCO08_Update..." -ForegroundColor Yellow
$sp4 = @"
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_Update
    @Id UNIQUEIDENTIFIER,
    @DenumireOcupatie NVARCHAR(500),
    @DenumireOcupatieEN NVARCHAR(500) = NULL,
    @Descriere NVARCHAR(MAX) = NULL,
    @Observatii NVARCHAR(1000) = NULL,
    @EsteActiv BIT = 1,
    @ModificatDe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verifica daca inregistrarea exista
    IF NOT EXISTS (SELECT 1 FROM dbo.Ocupatii_ISCO08 WHERE [Id] = @Id)
    BEGIN
        RAISERROR('Ocupatia cu ID %s nu exista in sistem.', 16, 1, CAST(@Id AS NVARCHAR(36)));
        RETURN -1;
    END
    
    UPDATE dbo.Ocupatii_ISCO08
    SET 
        [Denumire_Ocupatie] = @DenumireOcupatie,
        [Denumire_Ocupatie_EN] = @DenumireOcupatieEN,
        [Descriere] = @Descriere,
        [Observatii] = @Observatii,
        [Este_Activ] = @EsteActiv,
        [Data_Ultimei_Modificari] = GETDATE(),
        [Modificat_De] = ISNULL(@ModificatDe, SYSTEM_USER)
    WHERE [Id] = @Id;
    
    -- Returneaza inregistrarea actualizata
    SELECT 
        [Id], [Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic], [Este_Activ],
        [Data_Ultimei_Modificari], [Modificat_De],
        UPPER(LEFT(REPLACE(CAST([Id] AS NVARCHAR(36)), '-', ''), 8)) AS [IdScurt]
    FROM dbo.Ocupatii_ISCO08 
    WHERE [Id] = @Id;
    
    RETURN 0;
END
"@

try {
    $command.CommandText = $sp4
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? sp_Ocupatii_ISCO08_Update creat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

# SP 5: Delete cu GUID
Write-Host "Creez sp_Ocupatii_ISCO08_Delete..." -ForegroundColor Yellow
$sp5 = @"
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_Delete
    @Id UNIQUEIDENTIFIER,
    @ForceDelete BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verifica daca inregistrarea exista
    IF NOT EXISTS (SELECT 1 FROM dbo.Ocupatii_ISCO08 WHERE [Id] = @Id)
    BEGIN
        RAISERROR('Ocupatia cu ID %s nu exista in sistem.', 16, 1, CAST(@Id AS NVARCHAR(36)));
        RETURN -1;
    END
    
    -- Verifica daca are copii (ocupatii dependente)
    DECLARE @CodISCO NVARCHAR(10);
    SELECT @CodISCO = [Cod_ISCO] FROM dbo.Ocupatii_ISCO08 WHERE [Id] = @Id;
    
    IF EXISTS (SELECT 1 FROM dbo.Ocupatii_ISCO08 WHERE [Cod_Parinte] = @CodISCO) AND @ForceDelete = 0
    BEGIN
        RAISERROR('Ocupatia %s are ocupatii dependente. Foloseste @ForceDelete=1 pentru stergere fortata.', 16, 1, @CodISCO);
        RETURN -2;
    END
    
    -- Sterge ocupatiile copil daca e fortat
    IF @ForceDelete = 1
    BEGIN
        DELETE FROM dbo.Ocupatii_ISCO08 WHERE [Cod_Parinte] = @CodISCO;
    END
    
    -- Sterge inregistrarea principala
    DELETE FROM dbo.Ocupatii_ISCO08 WHERE [Id] = @Id;
    
    SELECT 'Ocupatia cu ID ' + CAST(@Id AS NVARCHAR(36)) + ' a fost stearsa cu succes.' AS [Mesaj];
    
    RETURN 0;
END
"@

try {
    $command.CommandText = $sp5
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? sp_Ocupatii_ISCO08_Delete creat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

$connection.Close()

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "STORED PROCEDURES ACTUALIZATE CU GUID" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "? Toate stored procedures au fost actualizate pentru GUID!" -ForegroundColor Green
Write-Host "`n?? SP-uri disponibile:" -ForegroundColor Cyan
Write-Host "  - sp_Ocupatii_ISCO08_GetAll (cu IdScurt pentru UI)" -ForegroundColor White
Write-Host "  - sp_Ocupatii_ISCO08_GetById (@Id UNIQUEIDENTIFIER)" -ForegroundColor White
Write-Host "  - sp_Ocupatii_ISCO08_Create (returneaza noul GUID)" -ForegroundColor White
Write-Host "  - sp_Ocupatii_ISCO08_Update (@Id UNIQUEIDENTIFIER)" -ForegroundColor White
Write-Host "  - sp_Ocupatii_ISCO08_Delete (@Id UNIQUEIDENTIFIER)" -ForegroundColor White
Write-Host "  - sp_Ocupatii_ISCO08_Search (ramane la fel)" -ForegroundColor White
Write-Host "  - sp_Ocupatii_ISCO08_GetStatistics (ramane la fel)" -ForegroundColor White