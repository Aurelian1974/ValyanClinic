-- ========================================
-- Stored Procedure: sp_Ocupatii_ISCO08_GetGrupeMajore
-- Database: ValyanMed
-- Created: 10/08/2025 17:19:55
-- Modified: 10/08/2025 17:19:55
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

CREATE   PROCEDURE sp_Ocupatii_ISCO08_GetGrupeMajore
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
GO
