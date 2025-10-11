-- ========================================
-- Stored Procedure: sp_PersonalMedical_CheckUnique
-- Database: ValyanMed
-- Created: 09/22/2025 20:33:39
-- Modified: 09/22/2025 20:33:39
-- Generat: 2025-10-08 16:36:44
-- ========================================

USE [ValyanMed]
GO

-- =============================================
-- SP pentru verificarea unicit??ii Email ?i NumarLicenta
-- =============================================
CREATE   PROCEDURE [dbo].[sp_PersonalMedical_CheckUnique]
    @Email VARCHAR(100) = NULL,
    @NumarLicenta VARCHAR(50) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CASE WHEN EXISTS (
            SELECT 1 FROM PersonalMedical 
            WHERE Email = @Email 
            AND (@ExcludeId IS NULL OR PersonalID != @ExcludeId)
        ) THEN 1 ELSE 0 END as Email_Exists,
        
        CASE WHEN EXISTS (
            SELECT 1 FROM PersonalMedical 
            WHERE NumarLicenta = @NumarLicenta 
            AND (@ExcludeId IS NULL OR PersonalID != @ExcludeId)
        ) THEN 1 ELSE 0 END as NumarLicenta_Exists;
END;
GO
