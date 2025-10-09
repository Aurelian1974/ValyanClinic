# Script pentru crearea stored procedures ISCO
$configPath = "..\..\..\ValyanClinic\appsettings.json"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CREARE STORED PROCEDURES ISCO-08" -ForegroundColor Cyan
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

# SP 1: GetAll
Write-Host "`nCreez sp_Ocupatii_ISCO08_GetAll..." -ForegroundColor Yellow
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
    Write-Host "? sp_Ocupatii_ISCO08_GetAll creat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

# SP 2: GetById
Write-Host "Creez sp_Ocupatii_ISCO08_GetById..." -ForegroundColor Yellow
$sp2 = @"
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_GetById
    @Id INT
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
        o.[Data_Crearii]
    FROM dbo.Ocupatii_ISCO08 o
    WHERE o.[Id] = @Id;
END
"@

try {
    $command.CommandText = $sp2
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? sp_Ocupatii_ISCO08_GetById creat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

# SP 3: GetGrupeMajore
Write-Host "Creez sp_Ocupatii_ISCO08_GetGrupeMajore..." -ForegroundColor Yellow
$sp3 = @"
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_GetGrupeMajore
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Cod_ISCO],
        [Denumire_Ocupatie],
        [Descriere],
        [Este_Activ]
    FROM dbo.Ocupatii_ISCO08
    WHERE [Nivel_Ierarhic] = 1
        AND [Este_Activ] = 1
    ORDER BY [Cod_ISCO];
END
"@

try {
    $command.CommandText = $sp3
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? sp_Ocupatii_ISCO08_GetGrupeMajore creat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

# SP 4: Search
Write-Host "Creez sp_Ocupatii_ISCO08_Search..." -ForegroundColor Yellow
$sp4 = @"
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_Search
    @SearchText NVARCHAR(255),
    @MaxResults INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@MaxResults)
        o.[Id],
        o.[Cod_ISCO],
        o.[Denumire_Ocupatie],
        o.[Nivel_Ierarhic],
        o.[Grupa_Majora_Denumire],
        CASE 
            WHEN o.[Cod_ISCO] = @SearchText THEN 100
            WHEN o.[Denumire_Ocupatie] LIKE @SearchText + '%' THEN 90
            WHEN o.[Denumire_Ocupatie] LIKE '%' + @SearchText + '%' THEN 80
            ELSE 70
        END AS [ScorRelevanta]
    FROM dbo.Ocupatii_ISCO08 o
    WHERE 
        (o.[Denumire_Ocupatie] LIKE '%' + @SearchText + '%' OR
         o.[Cod_ISCO] LIKE '%' + @SearchText + '%')
        AND o.[Este_Activ] = 1
    ORDER BY [ScorRelevanta] DESC, o.[Cod_ISCO];
END
"@

try {
    $command.CommandText = $sp4
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? sp_Ocupatii_ISCO08_Search creat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

# SP 5: GetStatistics
Write-Host "Creez sp_Ocupatii_ISCO08_GetStatistics..." -ForegroundColor Yellow
$sp5 = @"
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_GetStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Total Ocupatii' AS [Categorie],
        COUNT(*) AS [Numar],
        COUNT(CASE WHEN [Este_Activ] = 1 THEN 1 END) AS [Active]
    FROM dbo.Ocupatii_ISCO08
    
    UNION ALL
    
    SELECT 
        'Grupe Majore' AS [Categorie],
        COUNT(*) AS [Numar],
        COUNT(CASE WHEN [Este_Activ] = 1 THEN 1 END) AS [Active]
    FROM dbo.Ocupatii_ISCO08
    WHERE [Nivel_Ierarhic] = 1
    
    UNION ALL
    
    SELECT 
        'Ocupatii Detaliate' AS [Categorie],
        COUNT(*) AS [Numar],
        COUNT(CASE WHEN [Este_Activ] = 1 THEN 1 END) AS [Active]
    FROM dbo.Ocupatii_ISCO08
    WHERE [Nivel_Ierarhic] = 4;
END
"@

try {
    $command.CommandText = $sp5
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "? sp_Ocupatii_ISCO08_GetStatistics creat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}

$connection.Close()

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "STORED PROCEDURES CREATE CU SUCCES" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "? Toate stored procedures au fost create!" -ForegroundColor Green
Write-Host "`n?? Testare rapida:" -ForegroundColor Cyan
Write-Host "EXEC sp_Ocupatii_ISCO08_GetStatistics" -ForegroundColor Gray
Write-Host "EXEC sp_Ocupatii_ISCO08_Search @SearchText = 'medic'" -ForegroundColor Gray