-- ========================================
-- Stored Procedure: sp_Ocupatii_ISCO08_Search
-- Database: ValyanMed
-- Created: 10/08/2025 17:19:55
-- Modified: 10/08/2025 17:19:55
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

CREATE   PROCEDURE sp_Ocupatii_ISCO08_Search
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
GO
