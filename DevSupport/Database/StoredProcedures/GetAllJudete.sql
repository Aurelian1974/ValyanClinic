-- ========================================
-- Stored Procedure: GetAllJudete
-- Database: ValyanMed
-- Created: 07/13/2025 10:28:53
-- Modified: 08/21/2025 18:14:03
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


CREATE PROCEDURE dbo.GetAllJudete
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Return all counties - matches JudetDto structure
    SELECT 
        IdJudet,
        JudetGuid,
        CodJudet,
        Nume,
        Siruta,
        CodAuto,
        Ordine
    FROM dbo.Judet
    ORDER BY Nume;
END

GO
