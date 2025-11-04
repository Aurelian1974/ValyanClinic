-- ========================================
-- Stored Procedure: Localitate_GetByJudet
-- Database: ValyanMed
-- Created: 07/19/2025 19:08:20
-- Modified: 08/21/2025 18:14:03
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


CREATE PROCEDURE dbo.Localitate_GetByJudet
    @IdJudet INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Return localities for a specific county - matches LocalitateDto structure
    -- LocalitateDto has: IdOras, LocalitateGuid, IdJudet, Nume, Siruta
    SELECT 
        IdOras,               -- Primary key (not IdLocalitate)
        LocalitateGuid,
        IdJudet,
        Nume,
        Siruta
    FROM dbo.Localitate
    WHERE IdJudet = @IdJudet
    ORDER BY Nume;
END

GO
