-- ========================================
-- Stored Procedure: sp_Departamente_GetAll
-- Database: ValyanMed
-- Created: 09/22/2025 20:38:07
-- Modified: 09/22/2025 20:38:07
-- Generat: 2025-10-08 16:36:44
-- ========================================

USE [ValyanMed]
GO


-- =============================================
-- SP pentru ob?inerea tuturor departamentelor
-- =============================================
CREATE   PROCEDURE [dbo].[sp_Departamente_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        DepartamentID,
        Nume,
        Tip
    FROM Departamente 
    ORDER BY Nume;
END;

GO
