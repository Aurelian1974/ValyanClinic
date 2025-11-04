-- ========================================
-- Stored Procedure: sp_Ocupatii_ISCO08_GetStatistics
-- Database: ValyanMed
-- Created: 10/08/2025 17:19:55
-- Modified: 10/08/2025 17:19:55
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

CREATE   PROCEDURE sp_Ocupatii_ISCO08_GetStatistics
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
GO
