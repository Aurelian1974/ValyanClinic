-- ========================================
-- Stored Procedure: sp_Judete_GetAll
-- Database: ValyanMed
-- Created: 09/17/2025 08:41:52
-- Modified: 09/17/2025 09:22:01
-- Generat: 2025-10-08 16:36:44
-- ========================================

USE [ValyanMed]
GO

-- SP pentru obtinerea tuturor judetelor
CREATE   PROCEDURE [dbo].[sp_Judete_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdJudet,
        JudetGuid,
        CodJudet,
        Nume,
        Siruta,
        CodAuto,
        Ordine
    FROM Judet
    ORDER BY Ordine ASC, Nume ASC;
END;


GO
