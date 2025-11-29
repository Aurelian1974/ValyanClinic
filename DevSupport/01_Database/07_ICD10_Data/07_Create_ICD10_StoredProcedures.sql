-- ========================================
-- ICD-10 Stored Procedures SIMPLE
-- Database: ValyanMed
-- ========================================

USE [ValyanMed]
GO

-- sp_GetICD10ById
IF OBJECT_ID('sp_GetICD10ById') IS NOT NULL DROP PROCEDURE sp_GetICD10ById
GO
CREATE PROCEDURE sp_GetICD10ById @ICD10_ID UNIQUEIDENTIFIER
AS SELECT * FROM ICD10_Codes WHERE ICD10_ID = @ICD10_ID
GO

-- sp_GetICD10ByCode  
IF OBJECT_ID('sp_GetICD10ByCode') IS NOT NULL DROP PROCEDURE sp_GetICD10ByCode
GO
CREATE PROCEDURE sp_GetICD10ByCode @Code NVARCHAR(10)
AS SELECT * FROM ICD10_Codes WHERE Code = @Code
GO

-- sp_GetICD10ByCategory
IF OBJECT_ID('sp_GetICD10ByCategory') IS NOT NULL DROP PROCEDURE sp_GetICD10ByCategory
GO
CREATE PROCEDURE sp_GetICD10ByCategory @Category NVARCHAR(50), @OnlyLeafNodes BIT = 1
AS 
SELECT * FROM ICD10_Codes 
WHERE Category = @Category AND (@OnlyLeafNodes = 0 OR IsLeafNode = 1)
ORDER BY Code
GO

-- sp_GetCommonICD10
IF OBJECT_ID('sp_GetCommonICD10') IS NOT NULL DROP PROCEDURE sp_GetCommonICD10  
GO
CREATE PROCEDURE sp_GetCommonICD10 @Category NVARCHAR(50) = NULL, @MaxResults INT = 50
AS
SELECT TOP (@MaxResults) * FROM ICD10_Codes 
WHERE IsCommon = 1 AND (@Category IS NULL OR Category = @Category)
ORDER BY Code
GO

-- sp_GetICD10Children
IF OBJECT_ID('sp_GetICD10Children') IS NOT NULL DROP PROCEDURE sp_GetICD10Children
GO
CREATE PROCEDURE sp_GetICD10Children @ParentCode NVARCHAR(10)
AS
SELECT * FROM ICD10_Codes WHERE ParentCode = @ParentCode ORDER BY Code
GO

-- sp_GetICD10Categories
IF OBJECT_ID('sp_GetICD10Categories') IS NOT NULL DROP PROCEDURE sp_GetICD10Categories
GO  
CREATE PROCEDURE sp_GetICD10Categories
AS
SELECT DISTINCT Category FROM ICD10_Codes ORDER BY Category
GO

-- sp_GetICD10Statistics
IF OBJECT_ID('sp_GetICD10Statistics') IS NOT NULL DROP PROCEDURE sp_GetICD10Statistics
GO
CREATE PROCEDURE sp_GetICD10Statistics  
AS
SELECT 
    COUNT(*) as TotalCodes,
    SUM(CASE WHEN IsCommon = 1 THEN 1 ELSE 0 END) as CommonCodes,
    COUNT(DISTINCT Category) as Categories
FROM ICD10_Codes
GO

-- sp_ValidateICD10Code
IF OBJECT_ID('sp_ValidateICD10Code') IS NOT NULL DROP PROCEDURE sp_ValidateICD10Code
GO
CREATE PROCEDURE sp_ValidateICD10Code @Code NVARCHAR(10)
AS
SELECT COUNT(*) FROM ICD10_Codes WHERE Code = @Code AND IsLeafNode = 1
GO

-- sp_InsertICD10Code
IF OBJECT_ID('sp_InsertICD10Code') IS NOT NULL DROP PROCEDURE sp_InsertICD10Code
GO
CREATE PROCEDURE sp_InsertICD10Code
    @Code NVARCHAR(10),
    @FullCode NVARCHAR(20),
    @Category NVARCHAR(50),
    @ShortDescription NVARCHAR(200),
    @LongDescription NVARCHAR(1000) = NULL,
    @ParentCode NVARCHAR(10) = NULL,
    @IsLeafNode BIT = 1,
    @IsCommon BIT = 0,
    @Severity NVARCHAR(20) = NULL,
    @SearchTerms NVARCHAR(MAX) = NULL
AS
INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, 
    ParentCode, IsLeafNode, IsCommon, Severity, SearchTerms)
VALUES (@Code, @FullCode, @Category, @ShortDescription, @LongDescription,
    @ParentCode, @IsLeafNode, @IsCommon, @Severity, @SearchTerms)
GO

PRINT '? ICD-10 Stored Procedures created successfully!'