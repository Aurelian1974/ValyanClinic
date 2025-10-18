-- ========================================
-- Stored Procedure: sp_Judete_GetOrderedByName
-- Database: ValyanMed
-- Created: 09/17/2025 08:41:52
-- Modified: 09/17/2025 09:22:01
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


-- SP pentru obtinerea judetelor ordonate dupa nume
CREATE   PROCEDURE [dbo].[sp_Judete_GetOrderedByName]
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
    ORDER BY Nume ASC;
END;


GO
