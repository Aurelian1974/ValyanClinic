-- =============================================
-- ICD-10 STORED PROCEDURES - COMPLETE SETUP
-- Database: ValyanMed
-- Table: ICD10_Codes (15 columns, existing)
-- 
-- Acest script:
-- 1. ?terge SP-urile vechi cu naming convention diferit
-- 2. Creeaz? SP-uri noi cu naming convention standard
-- 3. Optimizat pentru ICD10AutocompleteComponent
-- =============================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'ICD-10 STORED PROCEDURES SETUP';
PRINT 'Database: ValyanMed';
PRINT 'Table: ICD10_Codes';
PRINT '========================================';
PRINT '';

-- =============================================
-- STEP 1: DROP OLD STORED PROCEDURES
-- =============================================
PRINT '1. Stergere Stored Procedures vechi...';

DECLARE @OldProcedures TABLE (ProcName NVARCHAR(128));
INSERT INTO @OldProcedures VALUES
    ('sp_SearchICD10'),
    ('sp_GetICD10ById'),
    ('sp_GetICD10ByCode'),
    ('sp_GetICD10Categories'),
    ('sp_GetCommonICD10'),
    ('sp_GetICD10Children'),
    ('sp_GetICD10Statistics'),
    ('sp_ValidateICD10Code'),
    ('sp_InsertICD10Code'),
    ('sp_GetICD10ByCategory'); -- bonus if exists

DECLARE @ProcName NVARCHAR(128);
DECLARE proc_cursor CURSOR FOR SELECT ProcName FROM @OldProcedures;

OPEN proc_cursor;
FETCH NEXT FROM proc_cursor INTO @ProcName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = @ProcName)
    BEGIN
        DECLARE @DropSQL NVARCHAR(MAX) = 'DROP PROCEDURE [dbo].[' + @ProcName + '];';
        EXEC sp_executesql @DropSQL;
        PRINT '   ? Sters: ' + @ProcName;
    END
    ELSE
    BEGIN
        PRINT '   - Skip: ' + @ProcName + ' (nu exista)';
    END
    
    FETCH NEXT FROM proc_cursor INTO @ProcName;
END

CLOSE proc_cursor;
DEALLOCATE proc_cursor;

PRINT '';
PRINT '========================================';
PRINT 'CREARE STORED PROCEDURES NOI';
PRINT '========================================';
PRINT '';

-- =============================================
-- SP 1: sp_ICD10_Search
-- C?utare coduri ICD-10 cu relevance scoring
-- PRINCIPAL pentru autocomplete component
-- =============================================
PRINT '2. Creare sp_ICD10_Search...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ICD10_Search]
    @SearchTerm NVARCHAR(255),
    @Category NVARCHAR(100) = NULL,
    @OnlyCommon BIT = 0,
    @OnlyLeafNodes BIT = 1,
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
        ParentCode,
        IsLeafNode,
        IsCommon,
        Severity,
        SearchTerms,
        -- Relevance scoring
        CASE 
            WHEN Code = @SearchTerm THEN 100
            WHEN Code LIKE @SearchTerm + '%' THEN 90
            WHEN ShortDescription LIKE @SearchTerm + '%' THEN 80
            WHEN ShortDescription LIKE '%' + @SearchTerm + '%' THEN 70
            WHEN LongDescription LIKE '%' + @SearchTerm + '%' THEN 60
            WHEN SearchTerms LIKE '%' + @SearchTerm + '%' THEN 50
            ELSE 40
        END AS RelevanceScore
    FROM ICD10_Codes
    WHERE 
        (Code LIKE '%' + @SearchTerm + '%'
         OR ShortDescription LIKE '%' + @SearchTerm + '%'
         OR LongDescription LIKE '%' + @SearchTerm + '%'
         OR SearchTerms LIKE '%' + @SearchTerm + '%')
        AND (@Category IS NULL OR Category = @Category)
        AND (@OnlyCommon = 0 OR IsCommon = 1)
        AND (@OnlyLeafNodes = 0 OR IsLeafNode = 1)
    ORDER BY 
        CASE 
            WHEN Code = @SearchTerm THEN 100
            WHEN Code LIKE @SearchTerm + '%' THEN 90
            WHEN ShortDescription LIKE @SearchTerm + '%' THEN 80
            WHEN ShortDescription LIKE '%' + @SearchTerm + '%' THEN 70
            WHEN LongDescription LIKE '%' + @SearchTerm + '%' THEN 60
            WHEN SearchTerms LIKE '%' + @SearchTerm + '%' THEN 50
            ELSE 40
        END DESC,
        Code ASC;
END
GO

PRINT '   ? sp_ICD10_Search creat';

-- =============================================
-- SP 2: sp_ICD10_GetById
-- Ob?ine detalii cod ICD-10 dup? GUID
-- =============================================
PRINT '3. Creare sp_ICD10_GetById...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ICD10_GetById]
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
    FROM ICD10_Codes
    WHERE ICD10_ID = @ICD10_ID;
END
GO

PRINT '   ? sp_ICD10_GetById creat';

-- =============================================
-- SP 3: sp_ICD10_GetByCode
-- Ob?ine detalii cod ICD-10 dup? cod (ex: "I10")
-- =============================================
PRINT '4. Creare sp_ICD10_GetByCode...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ICD10_GetByCode]
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
    FROM ICD10_Codes
    WHERE Code = @Code;
END
GO

PRINT '   ? sp_ICD10_GetByCode creat';

-- =============================================
-- SP 4: sp_ICD10_GetCategories
-- List? categorii unice pentru filtrare
-- =============================================
PRINT '5. Creare sp_ICD10_GetCategories...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ICD10_GetCategories]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT 
        Category,
        COUNT(*) as CodeCount
    FROM ICD10_Codes
    WHERE Category IS NOT NULL
    GROUP BY Category
    ORDER BY Category;
END
GO

PRINT '   ? sp_ICD10_GetCategories creat';

-- =============================================
-- SP 5: sp_ICD10_GetCommon
-- Coduri ICD-10 frecvente (IsCommon = 1)
-- =============================================
PRINT '6. Creare sp_ICD10_GetCommon...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ICD10_GetCommon]
    @Category NVARCHAR(100) = NULL,
    @MaxResults INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@MaxResults)
        ICD10_ID,
        Code,
        Category,
        ShortDescription,
        Severity,
        IsCommon
    FROM ICD10_Codes
    WHERE IsCommon = 1
        AND (@Category IS NULL OR Category = @Category)
    ORDER BY Code;
END
GO

PRINT '   ? sp_ICD10_GetCommon creat';

-- =============================================
-- SP 6: sp_ICD10_GetChildren
-- Coduri copil pentru un cod p?rinte
-- =============================================
PRINT '7. Creare sp_ICD10_GetChildren...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ICD10_GetChildren]
    @ParentCode NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ICD10_ID,
        Code,
        ShortDescription,
        IsLeafNode,
        IsCommon
    FROM ICD10_Codes
    WHERE ParentCode = @ParentCode
    ORDER BY Code;
END
GO

PRINT '   ? sp_ICD10_GetChildren creat';

-- =============================================
-- SP 7: sp_ICD10_GetStatistics
-- Statistici pentru dashboard
-- =============================================
PRINT '8. Creare sp_ICD10_GetStatistics...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ICD10_GetStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) as TotalCodes,
        SUM(CASE WHEN IsCommon = 1 THEN 1 ELSE 0 END) as CommonCodes,
        COUNT(DISTINCT Category) as Categories,
        SUM(CASE WHEN IsLeafNode = 1 THEN 1 ELSE 0 END) as LeafNodes,
        SUM(CASE WHEN Severity = 'Mild' THEN 1 ELSE 0 END) as MildCodes,
        SUM(CASE WHEN Severity = 'Moderate' THEN 1 ELSE 0 END) as ModerateCodes,
        SUM(CASE WHEN Severity = 'Severe' THEN 1 ELSE 0 END) as SevereCodes,
        SUM(CASE WHEN Severity = 'Critical' THEN 1 ELSE 0 END) as CriticalCodes
    FROM ICD10_Codes;
END
GO

PRINT '   ? sp_ICD10_GetStatistics creat';

-- =============================================
-- SP 8: sp_ICD10_ValidateCode
-- Valideaz? dac? un cod ICD-10 exist?
-- =============================================
PRINT '9. Creare sp_ICD10_ValidateCode...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ICD10_ValidateCode]
    @Code NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) as IsValid
    FROM ICD10_Codes
    WHERE Code = @Code;
END
GO

PRINT '   ? sp_ICD10_ValidateCode creat';

-- =============================================
-- SP 9: sp_ICD10_Insert
-- Insert nou cod ICD-10 (pentru import bulk)
-- =============================================
PRINT '10. Creare sp_ICD10_Insert...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ICD10_Insert]
    @Code NVARCHAR(10),
    @FullCode NVARCHAR(20) = NULL,
    @Category NVARCHAR(100),
    @ShortDescription NVARCHAR(255),
    @LongDescription NVARCHAR(MAX) = NULL,
    @EnglishDescription NVARCHAR(MAX) = NULL,
    @ParentCode NVARCHAR(10) = NULL,
    @IsLeafNode BIT = 1,
    @IsCommon BIT = 0,
    @Severity NVARCHAR(20) = NULL,
    @SearchTerms NVARCHAR(MAX) = NULL,
    @Notes NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if code already exists
    IF EXISTS (SELECT 1 FROM ICD10_Codes WHERE Code = @Code)
    BEGIN
        -- Update existing
        UPDATE ICD10_Codes
        SET 
            FullCode = @FullCode,
            Category = @Category,
            ShortDescription = @ShortDescription,
            LongDescription = @LongDescription,
            EnglishDescription = @EnglishDescription,
            ParentCode = @ParentCode,
            IsLeafNode = @IsLeafNode,
            IsCommon = @IsCommon,
            Severity = @Severity,
            SearchTerms = @SearchTerms,
            Notes = @Notes,
            DataModificare = GETDATE()
        WHERE Code = @Code;
        
        SELECT 'Updated' as Result, ICD10_ID FROM ICD10_Codes WHERE Code = @Code;
    END
    ELSE
    BEGIN
        -- Insert new
        DECLARE @NewID UNIQUEIDENTIFIER = NEWID();
        
        INSERT INTO ICD10_Codes (
            ICD10_ID, Code, FullCode, Category, ShortDescription, 
            LongDescription, EnglishDescription, ParentCode, IsLeafNode, 
            IsCommon, Severity, SearchTerms, Notes, DataCreare, DataModificare
        )
        VALUES (
            @NewID, @Code, @FullCode, @Category, @ShortDescription,
            @LongDescription, @EnglishDescription, @ParentCode, @IsLeafNode,
            @IsCommon, @Severity, @SearchTerms, @Notes, GETDATE(), GETDATE()
        );
        
        SELECT 'Inserted' as Result, @NewID as ICD10_ID;
    END
END
GO

PRINT '   ? sp_ICD10_Insert creat';

PRINT '';
PRINT '========================================';
PRINT 'VERIFICARE FINALA';
PRINT '========================================';

-- Count new procedures
SELECT COUNT(*) as TotalNewProcedures
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
AND ROUTINE_SCHEMA = 'dbo'
AND ROUTINE_NAME LIKE 'sp_ICD10_%';

-- List all new procedures
SELECT 
    ROUTINE_NAME as ProcedureName,
    CREATED as DateCreated
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
AND ROUTINE_SCHEMA = 'dbo'
AND ROUTINE_NAME LIKE 'sp_ICD10_%'
ORDER BY ROUTINE_NAME;

PRINT '';
PRINT '? Setup complet! Toate stored procedures ICD-10 au fost create.';
PRINT '  Ruleaza: Verify-ICD10Database.ps1 pentru verificare finala';
PRINT '';
GO
