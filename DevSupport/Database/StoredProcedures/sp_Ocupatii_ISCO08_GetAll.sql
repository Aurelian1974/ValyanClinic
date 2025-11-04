-- ========================================
-- Stored Procedure: sp_Ocupatii_ISCO08_GetAll
-- Database: ValyanMed
-- Created: 10/08/2025 17:19:55
-- Modified: 10/08/2025 17:27:03
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

CREATE   PROCEDURE sp_Ocupatii_ISCO08_GetAll
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
GO
