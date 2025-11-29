-- ========================================
-- Stored Procedures: Additional ICD10 SPs
-- Database: ValyanMed
-- Descriere: SP-uri suplimentare pentru ICD10
-- ========================================

USE [ValyanMed]
GO

-- ==================== sp_GetICD10ByCode ====================
IF OBJECT_ID('dbo.sp_GetICD10ByCode', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetICD10ByCode
GO

CREATE PROCEDURE dbo.sp_GetICD10ByCode
    @Code NVARCHAR(10)
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
    WHERE Code = @Code
END
GO

PRINT '? sp_GetICD10ByCode created'
GO

-- ==================== sp_GetICD10ByCategory ====================
IF OBJECT_ID('dbo.sp_GetICD10ByCategory', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetICD10ByCategory
GO

CREATE PROCEDURE dbo.sp_GetICD10ByCategory
    @Category NVARCHAR(50),
    @OnlyLeafNodes BIT = 1
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
        IsLeafNode,
        IsCommon,
        Severity,
        SearchTerms
    FROM dbo.ICD10_Codes
    WHERE
        Category = @Category
        AND (@OnlyLeafNodes = 0 OR IsLeafNode = 1)
    ORDER BY Code
END
GO

PRINT '? sp_GetICD10ByCategory created'
GO

-- ==================== sp_GetCommonICD10 ====================
IF OBJECT_ID('dbo.sp_GetCommonICD10', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetCommonICD10
GO

CREATE PROCEDURE dbo.sp_GetCommonICD10
    @Category NVARCHAR(50) = NULL,
    @MaxResults INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@MaxResults)
        ICD10_ID,
        Code,
        FullCode,
        Category,
        ShortDescription,
        LongDescription,
        IsLeafNode,
        IsCommon,
        Severity,
        SearchTerms
    FROM dbo.ICD10_Codes
    WHERE
        IsCommon = 1
        AND IsLeafNode = 1
        AND (@Category IS NULL OR Category = @Category)
    ORDER BY Category, Code
END
GO

PRINT '? sp_GetCommonICD10 created'
GO

-- ==================== sp_GetICD10Children ====================
IF OBJECT_ID('dbo.sp_GetICD10Children', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetICD10Children
GO

CREATE PROCEDURE dbo.sp_GetICD10Children
    @ParentCode NVARCHAR(10)
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
        ParentCode,
        IsLeafNode,
        IsCommon,
        Severity
    FROM dbo.ICD10_Codes
    WHERE ParentCode = @ParentCode
    ORDER BY Code
END
GO

PRINT '? sp_GetICD10Children created'
GO

-- ==================== sp_GetICD10Categories ====================
IF OBJECT_ID('dbo.sp_GetICD10Categories', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetICD10Categories
GO

CREATE PROCEDURE dbo.sp_GetICD10Categories
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT Category
    FROM dbo.ICD10_Codes
    ORDER BY Category
END
GO

PRINT '? sp_GetICD10Categories created'
GO

-- ==================== sp_GetICD10Statistics ====================
IF OBJECT_ID('dbo.sp_GetICD10Statistics', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetICD10Statistics
GO

CREATE PROCEDURE dbo.sp_GetICD10Statistics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        COUNT(*) AS TotalCodes,
        SUM(CASE WHEN IsCommon = 1 THEN 1 ELSE 0 END) AS CommonCodes,
        COUNT(DISTINCT Category) AS Categories
    FROM dbo.ICD10_Codes
END
GO

PRINT '? sp_GetICD10Statistics created'
GO

-- ==================== sp_ValidateICD10Code ====================
IF OBJECT_ID('dbo.sp_ValidateICD10Code', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ValidateICD10Code
GO

CREATE PROCEDURE dbo.sp_ValidateICD10Code
    @Code NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*)
    FROM dbo.ICD10_Codes
    WHERE Code = @Code AND IsLeafNode = 1
END
GO

PRINT '? sp_ValidateICD10Code created'
GO

-- ==================== sp_InsertICD10Code ====================
IF OBJECT_ID('dbo.sp_InsertICD10Code', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertICD10Code
GO

CREATE PROCEDURE dbo.sp_InsertICD10Code
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
BEGIN
    SET NOCOUNT ON;
    
    -- Check if code already exists
    IF EXISTS (SELECT 1 FROM dbo.ICD10_Codes WHERE Code = @Code)
    BEGIN
        RAISERROR('Codul ICD-10 %s exist? deja.', 16, 1, @Code)
        RETURN
    END
    
    INSERT INTO dbo.ICD10_Codes
    (
        Code,
        FullCode,
        Category,
        ShortDescription,
        LongDescription,
        ParentCode,
        IsLeafNode,
        IsCommon,
        Severity,
        SearchTerms,
        DataCreare
    )
    VALUES
    (
        @Code,
        @FullCode,
        @Category,
        @ShortDescription,
        @LongDescription,
        @ParentCode,
        @IsLeafNode,
        @IsCommon,
        @Severity,
        @SearchTerms,
        GETDATE()
    )
END
GO

PRINT '? sp_InsertICD10Code created'
GO

PRINT ''
PRINT '========================================='
PRINT '? ALL ICD10 STORED PROCEDURES CREATED!'
PRINT '========================================='
GO
