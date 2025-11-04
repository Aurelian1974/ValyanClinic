-- ========================================
-- Stored Procedure: sp_Departamente_CheckUnique
-- Database: ValyanMed
-- Created: 10/13/2025 14:54:02
-- Modified: 10/13/2025 14:54:02
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


CREATE PROCEDURE dbo.sp_Departamente_CheckUnique
    @DenumireDepartament VARCHAR(200),
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM Departamente 
            WHERE DenumireDepartament = @DenumireDepartament 
            AND (@ExcludeId IS NULL OR IdDepartament != @ExcludeId)
        ) THEN 1 
        ELSE 0 
    END AS Denumire_Exists;
END

GO
