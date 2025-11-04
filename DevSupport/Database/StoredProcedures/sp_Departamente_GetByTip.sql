-- ========================================
-- Stored Procedure: sp_Departamente_GetByTip
-- Database: ValyanMed
-- Created: 09/22/2025 20:38:07
-- Modified: 09/22/2025 20:38:07
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

-- =============================================
-- SP pentru ob?inerea departamentelor din tabela Departamente
-- =============================================
CREATE   PROCEDURE [dbo].[sp_Departamente_GetByTip]
    @Tip NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        DepartamentID,
        Nume,
        Tip
    FROM Departamente 
    WHERE Tip = @Tip
    ORDER BY Nume;
END;

GO
