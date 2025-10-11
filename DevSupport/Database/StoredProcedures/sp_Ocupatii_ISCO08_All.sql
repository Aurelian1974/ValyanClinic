-- ========================================
-- Stored Procedures pentru Ocupatii_ISCO08
-- Database: ValyanMed
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [ValyanMed]
GO

-- ========================================
-- SP pentru obtinerea tuturor ocupatiilor cu paginare
-- ========================================
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SearchText NVARCHAR(255) = NULL,
    @NivelIerarhic TINYINT = NULL,
    @GrupaMajora NVARCHAR(10) = NULL,
    @EsteActiv BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
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
        -- Informatii suplimentare
        p.[Denumire_Ocupatie] AS [Ocupatie_Parinte],
        COUNT(*) OVER() AS [TotalRecords]
    FROM dbo.Ocupatii_ISCO08 o
    LEFT JOIN dbo.Ocupatii_ISCO08 p ON o.[Cod_Parinte] = p.[Cod_ISCO]
    WHERE 
        (@SearchText IS NULL OR 
         o.[Denumire_Ocupatie] LIKE '%' + @SearchText + '%' OR
         o.[Cod_ISCO] LIKE '%' + @SearchText + '%' OR
         o.[Descriere] LIKE '%' + @SearchText + '%')
        AND (@NivelIerarhic IS NULL OR o.[Nivel_Ierarhic] = @NivelIerarhic)
        AND (@GrupaMajora IS NULL OR o.[Grupa_Majora] = @GrupaMajora)
        AND (@EsteActiv IS NULL OR o.[Este_Activ] = @EsteActiv)
    ORDER BY o.[Cod_ISCO]
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ========================================
-- SP pentru obtinerea unei ocupatii dupa ID
-- ========================================
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
        o.[Observatii],
        o.[Este_Activ],
        o.[Data_Crearii],
        o.[Data_Ultimei_Modificari],
        o.[Creat_De],
        o.[Modificat_De],
        -- Informatii despre parinte
        p.[Denumire_Ocupatie] AS [Ocupatie_Parinte]
    FROM dbo.Ocupatii_ISCO08 o
    LEFT JOIN dbo.Ocupatii_ISCO08 p ON o.[Cod_Parinte] = p.[Cod_ISCO]
    WHERE o.[Id] = @Id;
END
GO

-- ========================================
-- SP pentru obtinerea unei ocupatii dupa cod ISCO
-- ========================================
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_GetByCod
    @CodISCO NVARCHAR(10)
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
        o.[Observatii],
        o.[Este_Activ],
        o.[Data_Crearii],
        o.[Data_Ultimei_Modificari],
        -- Informatii despre parinte
        p.[Denumire_Ocupatie] AS [Ocupatie_Parinte]
    FROM dbo.Ocupatii_ISCO08 o
    LEFT JOIN dbo.Ocupatii_ISCO08 p ON o.[Cod_Parinte] = p.[Cod_ISCO]
    WHERE o.[Cod_ISCO] = @CodISCO;
END
GO

-- ========================================
-- SP pentru obtinerea ierarhiei (copii ai unei ocupatii)
-- ========================================
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_GetCopii
    @CodParinte NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.[Id],
        o.[Cod_ISCO],
        o.[Denumire_Ocupatie],
        o.[Denumire_Ocupatie_EN],
        o.[Nivel_Ierarhic],
        o.[Grupa_Majora],
        o.[Grupa_Majora_Denumire],
        o.[Este_Activ]
    FROM dbo.Ocupatii_ISCO08 o
    WHERE o.[Cod_Parinte] = @CodParinte
        AND o.[Este_Activ] = 1
    ORDER BY o.[Cod_ISCO];
END
GO

-- ========================================
-- SP pentru obtinerea grupelor majore
-- ========================================
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_GetGrupeMajore
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Cod_ISCO],
        [Denumire_Ocupatie],
        [Denumire_Ocupatie_EN],
        [Descriere],
        [Este_Activ]
    FROM dbo.Ocupatii_ISCO08
    WHERE [Nivel_Ierarhic] = 1
        AND [Este_Activ] = 1
    ORDER BY [Cod_ISCO];
END
GO

-- ========================================
-- SP pentru cautare avansata ocupatii
-- ========================================
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_Search
    @SearchText NVARCHAR(255),
    @NivelIerarhic TINYINT = NULL,
    @MaxResults INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@MaxResults)
        o.[Id],
        o.[Cod_ISCO],
        o.[Denumire_Ocupatie],
        o.[Denumire_Ocupatie_EN],
        o.[Nivel_Ierarhic],
        o.[Grupa_Majora],
        o.[Grupa_Majora_Denumire],
        -- Scor de relevanta
        CASE 
            WHEN o.[Cod_ISCO] = @SearchText THEN 100
            WHEN o.[Denumire_Ocupatie] LIKE @SearchText + '%' THEN 90
            WHEN o.[Denumire_Ocupatie] LIKE '%' + @SearchText + '%' THEN 80
            WHEN o.[Descriere] LIKE '%' + @SearchText + '%' THEN 70
            ELSE 60
        END AS [ScorRelevanta]
    FROM dbo.Ocupatii_ISCO08 o
    WHERE 
        (o.[Denumire_Ocupatie] LIKE '%' + @SearchText + '%' OR
         o.[Denumire_Ocupatie_EN] LIKE '%' + @SearchText + '%' OR
         o.[Cod_ISCO] LIKE '%' + @SearchText + '%' OR
         o.[Descriere] LIKE '%' + @SearchText + '%')
        AND (@NivelIerarhic IS NULL OR o.[Nivel_Ierarhic] = @NivelIerarhic)
        AND o.[Este_Activ] = 1
    ORDER BY [ScorRelevanta] DESC, o.[Cod_ISCO];
END
GO

-- ========================================
-- SP pentru statistici ocupatii
-- ========================================
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_GetStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Total Ocupatii' AS [Categorie],
        COUNT(*) AS [Numar],
        COUNT(CASE WHEN [Este_Activ] = 1 THEN 1 END) AS [Active],
        COUNT(CASE WHEN [Este_Activ] = 0 THEN 1 END) AS [Inactive]
    FROM dbo.Ocupatii_ISCO08
    
    UNION ALL
    
    SELECT 
        'Grupe Majore' AS [Categorie],
        COUNT(*) AS [Numar],
        COUNT(CASE WHEN [Este_Activ] = 1 THEN 1 END) AS [Active],
        COUNT(CASE WHEN [Este_Activ] = 0 THEN 1 END) AS [Inactive]
    FROM dbo.Ocupatii_ISCO08
    WHERE [Nivel_Ierarhic] = 1
    
    UNION ALL
    
    SELECT 
        'Subgrupe' AS [Categorie],
        COUNT(*) AS [Numar],
        COUNT(CASE WHEN [Este_Activ] = 1 THEN 1 END) AS [Active],
        COUNT(CASE WHEN [Este_Activ] = 0 THEN 1 END) AS [Inactive]
    FROM dbo.Ocupatii_ISCO08
    WHERE [Nivel_Ierarhic] = 2
    
    UNION ALL
    
    SELECT 
        'Grupe Minore' AS [Categorie],
        COUNT(*) AS [Numar],
        COUNT(CASE WHEN [Este_Activ] = 1 THEN 1 END) AS [Active],
        COUNT(CASE WHEN [Este_Activ] = 0 THEN 1 END) AS [Inactive]
    FROM dbo.Ocupatii_ISCO08
    WHERE [Nivel_Ierarhic] = 3
    
    UNION ALL
    
    SELECT 
        'Ocupatii Detaliate' AS [Categorie],
        COUNT(*) AS [Numar],
        COUNT(CASE WHEN [Este_Activ] = 1 THEN 1 END) AS [Active],
        COUNT(CASE WHEN [Este_Activ] = 0 THEN 1 END) AS [Inactive]
    FROM dbo.Ocupatii_ISCO08
    WHERE [Nivel_Ierarhic] = 4;
END
GO

-- ========================================
-- SP pentru dropdown/selectare (pentru formulare)
-- ========================================
CREATE OR ALTER PROCEDURE sp_Ocupatii_ISCO08_GetDropdownOptions
    @NivelIerarhic TINYINT = 4, -- Default: ocupatii complete
    @GrupaMajora NVARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id] AS [Value],
        [Cod_ISCO] + ' - ' + [Denumire_Ocupatie] AS [Text],
        [Cod_ISCO],
        [Denumire_Ocupatie],
        [Nivel_Ierarhic]
    FROM dbo.Ocupatii_ISCO08
    WHERE [Nivel_Ierarhic] = @NivelIerarhic
        AND (@GrupaMajora IS NULL OR [Grupa_Majora] = @GrupaMajora)
        AND [Este_Activ] = 1
    ORDER BY [Cod_ISCO];
END
GO

PRINT 'Stored procedures pentru Ocupatii_ISCO08 create cu succes.'