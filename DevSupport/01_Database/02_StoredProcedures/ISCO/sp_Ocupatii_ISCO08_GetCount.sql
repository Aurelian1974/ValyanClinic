-- ============================================================================
-- Stored Procedure: sp_Ocupatii_ISCO08_GetCount
-- Descriere: Returneaza numarul total de ocupatii ISCO-08 cu filtrare
-- Database: ValyanMed
-- Creat: 2025-01-08
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Ocupatii_ISCO08_GetCount')
    DROP PROCEDURE sp_Ocupatii_ISCO08_GetCount
GO

CREATE PROCEDURE sp_Ocupatii_ISCO08_GetCount
    @SearchText NVARCHAR(255) = NULL,
    @NivelIerarhic TINYINT = NULL,
    @GrupaMajora NVARCHAR(10) = NULL,
    @EsteActiv BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS TotalCount
    FROM Ocupatii_ISCO08
    WHERE (@SearchText IS NULL 
           OR Denumire_Ocupatie LIKE '%' + @SearchText + '%' 
           OR Cod_ISCO LIKE '%' + @SearchText + '%')
      AND (@NivelIerarhic IS NULL OR Nivel_Ierarhic = @NivelIerarhic)
      AND (@GrupaMajora IS NULL OR Grupa_Majora = @GrupaMajora)
      AND (@EsteActiv IS NULL OR Este_Activ = @EsteActiv);
END
GO

PRINT 'Stored procedure sp_Ocupatii_ISCO08_GetCount creat cu succes!';
GO
