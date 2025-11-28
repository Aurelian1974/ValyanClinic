-- ========================================
-- Stored Procedure: sp_GetICD10ById
-- Database: ValyanMed
-- Descriere: Obtine detalii complete pentru un cod ICD-10 dupa ID
-- ========================================

USE [ValyanMed]
GO

IF OBJECT_ID('dbo.sp_GetICD10ById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetICD10ById
GO

CREATE PROCEDURE dbo.sp_GetICD10ById
    @ICD10_ID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        ICD10_ID,
        Code,
        FullCode,
        Category,
        ShortDescription,
        LongDescription,
        EnglishDescription,
        ParentCode,
        IsLeafNode,
        IsCommon,
        Severity,
        SearchTerms,
        Notes,
        DataCreare,
        DataModificare
    FROM dbo.ICD10_Codes
    WHERE ICD10_ID = @ICD10_ID
END
GO

PRINT '? Stored Procedure sp_GetICD10ById creat cu succes!'
GO
