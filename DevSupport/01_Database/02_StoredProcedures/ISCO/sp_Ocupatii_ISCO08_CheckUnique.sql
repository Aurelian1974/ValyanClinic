-- ============================================================================
-- Stored Procedure: sp_Ocupatii_ISCO08_CheckUnique
-- Descriere: Verifica daca un cod ISCO este unic (pentru validare la insert/update)
-- Database: ValyanMed
-- Creat: 2025-01-08
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Ocupatii_ISCO08_CheckUnique')
    DROP PROCEDURE sp_Ocupatii_ISCO08_CheckUnique
GO

CREATE PROCEDURE sp_Ocupatii_ISCO08_CheckUnique
    @CodISCO NVARCHAR(10),
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Returneaza 1 daca exista, 0 daca este unic
    SELECT CASE 
        WHEN EXISTS (
            SELECT 1 FROM Ocupatii_ISCO08 
            WHERE Cod_ISCO = @CodISCO 
              AND (@ExcludeId IS NULL OR Id != @ExcludeId)
        ) THEN 1 
        ELSE 0 
    END AS CodeExists;
END
GO

PRINT 'Stored procedure sp_Ocupatii_ISCO08_CheckUnique creat cu succes!';
GO
