-- ========================================
-- ICD-10 Views ?i Stored Procedures
-- Database: ValyanMed
-- Versiune: 2.0
-- Data: 2025-01-15
-- ========================================

USE [ValyanMed]
GO

PRINT '=========================================='
PRINT '  ICD-10 VIEWS & STORED PROCEDURES'
PRINT '=========================================='
PRINT ''

-- ==========================================
-- VIEW 1: vw_ICD10_CodesComplete
-- Vizualizare complet? cu capitole ?i sec?iuni
-- ==========================================
PRINT '?? Creare view vw_ICD10_CodesComplete...'

IF OBJECT_ID('dbo.vw_ICD10_CodesComplete', 'V') IS NOT NULL
    DROP VIEW dbo.vw_ICD10_CodesComplete
GO

CREATE VIEW dbo.vw_ICD10_CodesComplete
AS
SELECT 
    c.ICD10_ID,
    c.Code,
    c.FullCode,
    
    -- Descriere (prefer? român?, fallback la englez?)
    COALESCE(c.ShortDescriptionRo, c.ShortDescriptionEn) AS ShortDescription,
    COALESCE(c.LongDescriptionRo, c.LongDescriptionEn) AS LongDescription,
    
    -- Descrieri separate
    c.ShortDescriptionRo,
    c.ShortDescriptionEn,
    c.LongDescriptionRo,
    c.LongDescriptionEn,
    
    -- Ierarhie
    c.ParentCode,
    c.ParentId,
    c.HierarchyLevel,
    c.IsLeafNode,
    c.IsBillable,
    
    -- Clasificare
    c.Category,
    c.IsCommon,
    c.Severity,
    
    -- Capitol
    ch.ChapterNumber,
    COALESCE(ch.DescriptionRo, ch.DescriptionEn) AS ChapterDescription,
    
    -- Sec?iune
    s.SectionCode,
    COALESCE(s.DescriptionRo, s.DescriptionEn) AS SectionDescription,
    
    -- Status traducere
    c.IsTranslated,
    c.TranslatedAt,
    
    -- C?utare
    c.SearchTermsRo,
    c.SearchTermsEn,
    
    -- Display combinat
    c.Code + ' - ' + COALESCE(c.ShortDescriptionRo, c.ShortDescriptionEn) AS DisplayText,
    
    -- Metadata
    c.Version,
    c.IsActive,
    c.CreatedAt,
    c.UpdatedAt
    
FROM dbo.ICD10_Codes c
INNER JOIN dbo.ICD10_Chapters ch ON c.ChapterId = ch.ChapterId
LEFT JOIN dbo.ICD10_Sections s ON c.SectionId = s.SectionId
WHERE c.IsActive = 1
GO

PRINT '? View vw_ICD10_CodesComplete creat.'

-- ==========================================
-- VIEW 2: vw_ICD10_CommonCodes
-- Doar codurile frecvent utilizate
-- ==========================================
PRINT '?? Creare view vw_ICD10_CommonCodes...'

IF OBJECT_ID('dbo.vw_ICD10_CommonCodes', 'V') IS NOT NULL
    DROP VIEW dbo.vw_ICD10_CommonCodes
GO

CREATE VIEW dbo.vw_ICD10_CommonCodes
AS
SELECT 
    ICD10_ID,
    Code,
    ShortDescription,
    LongDescription,
    Category,
    Severity,
    ChapterNumber,
    DisplayText,
    IsTranslated
FROM dbo.vw_ICD10_CodesComplete
WHERE IsCommon = 1 
  AND IsLeafNode = 1
  AND IsActive = 1
GO

PRINT '? View vw_ICD10_CommonCodes creat.'

-- ==========================================
-- VIEW 3: vw_ICD10_UntranslatedCodes
-- Coduri f?r? traducere în român?
-- ==========================================
PRINT '?? Creare view vw_ICD10_UntranslatedCodes...'

IF OBJECT_ID('dbo.vw_ICD10_UntranslatedCodes', 'V') IS NOT NULL
    DROP VIEW dbo.vw_ICD10_UntranslatedCodes
GO

CREATE VIEW dbo.vw_ICD10_UntranslatedCodes
AS
SELECT 
    c.ICD10_ID,
    c.Code,
    c.ShortDescriptionEn,
    c.LongDescriptionEn,
    ch.ChapterNumber,
    COALESCE(ch.DescriptionRo, ch.DescriptionEn) AS ChapterDescription,
    c.IsCommon,
    c.HierarchyLevel
FROM dbo.ICD10_Codes c
INNER JOIN dbo.ICD10_Chapters ch ON c.ChapterId = ch.ChapterId
WHERE c.IsTranslated = 0
  AND c.IsActive = 1
GO

PRINT '? View vw_ICD10_UntranslatedCodes creat.'

-- ==========================================
-- STORED PROCEDURE 1: sp_ICD10_Search
-- C?utare coduri ICD-10
-- ==========================================
PRINT '?? Creare stored procedure sp_ICD10_Search...'

IF OBJECT_ID('dbo.sp_ICD10_Search', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_Search
GO

CREATE PROCEDURE dbo.sp_ICD10_Search
    @SearchTerm NVARCHAR(100),
    @Category NVARCHAR(50) = NULL,
    @ChapterNumber INT = NULL,
    @OnlyCommon BIT = 0,
    @OnlyLeaf BIT = 1,
    @OnlyTranslated BIT = 0,
    @MaxResults INT = 50,
    @Language NVARCHAR(2) = 'ro'  -- 'ro' sau 'en'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SearchPattern NVARCHAR(102) = '%' + @SearchTerm + '%'
    
    SELECT TOP (@MaxResults)
        c.ICD10_ID,
        c.Code,
        c.FullCode,
        
        -- Descriere bazat? pe limba selectat?
        CASE @Language 
            WHEN 'en' THEN c.ShortDescriptionEn
            ELSE COALESCE(c.ShortDescriptionRo, c.ShortDescriptionEn)
        END AS ShortDescription,
        
        CASE @Language 
            WHEN 'en' THEN c.LongDescriptionEn
            ELSE COALESCE(c.LongDescriptionRo, c.LongDescriptionEn)
        END AS LongDescription,
        
        c.Category,
        c.Severity,
        c.IsCommon,
        c.IsLeafNode,
        c.IsBillable,
        c.IsTranslated,
        
        ch.ChapterNumber,
        CASE @Language 
            WHEN 'en' THEN ch.DescriptionEn
            ELSE COALESCE(ch.DescriptionRo, ch.DescriptionEn)
        END AS ChapterDescription,
        
        -- Relevance Score
        CASE 
            WHEN c.Code = @SearchTerm THEN 100
            WHEN c.Code LIKE @SearchTerm + '%' THEN 90
            WHEN c.ShortDescriptionRo LIKE @SearchTerm + '%' THEN 80
            WHEN c.ShortDescriptionEn LIKE @SearchTerm + '%' THEN 75
            WHEN c.Code LIKE @SearchPattern THEN 70
            WHEN c.ShortDescriptionRo LIKE @SearchPattern THEN 60
            WHEN c.ShortDescriptionEn LIKE @SearchPattern THEN 55
            WHEN c.SearchTermsRo LIKE @SearchPattern THEN 50
            WHEN c.SearchTermsEn LIKE @SearchPattern THEN 45
            ELSE 10
        END AS RelevanceScore
        
    FROM dbo.ICD10_Codes c
    INNER JOIN dbo.ICD10_Chapters ch ON c.ChapterId = ch.ChapterId
    WHERE c.IsActive = 1
      AND (
          c.Code LIKE @SearchPattern
          OR c.ShortDescriptionRo LIKE @SearchPattern
          OR c.ShortDescriptionEn LIKE @SearchPattern
          OR c.LongDescriptionRo LIKE @SearchPattern
          OR c.LongDescriptionEn LIKE @SearchPattern
          OR c.SearchTermsRo LIKE @SearchPattern
          OR c.SearchTermsEn LIKE @SearchPattern
      )
      AND (@Category IS NULL OR c.Category = @Category)
      AND (@ChapterNumber IS NULL OR ch.ChapterNumber = @ChapterNumber)
      AND (@OnlyCommon = 0 OR c.IsCommon = 1)
      AND (@OnlyLeaf = 0 OR c.IsLeafNode = 1)
      AND (@OnlyTranslated = 0 OR c.IsTranslated = 1)
      
    ORDER BY 
        RelevanceScore DESC,
        c.IsCommon DESC,
        c.Code
END
GO

PRINT '? Stored procedure sp_ICD10_Search creat.'

-- ==========================================
-- STORED PROCEDURE 2: sp_ICD10_GetByCode
-- Ob?ine detalii complete pentru un cod
-- ==========================================
PRINT '?? Creare stored procedure sp_ICD10_GetByCode...'

IF OBJECT_ID('dbo.sp_ICD10_GetByCode', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_GetByCode
GO

CREATE PROCEDURE dbo.sp_ICD10_GetByCode
    @Code NVARCHAR(10),
    @Language NVARCHAR(2) = 'ro'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Returneaz? codul principal
    SELECT 
        c.ICD10_ID,
        c.Code,
        c.FullCode,
        
        CASE @Language 
            WHEN 'en' THEN c.ShortDescriptionEn
            ELSE COALESCE(c.ShortDescriptionRo, c.ShortDescriptionEn)
        END AS ShortDescription,
        
        CASE @Language 
            WHEN 'en' THEN c.LongDescriptionEn
            ELSE COALESCE(c.LongDescriptionRo, c.LongDescriptionEn)
        END AS LongDescription,
        
        c.ShortDescriptionRo,
        c.ShortDescriptionEn,
        c.LongDescriptionRo,
        c.LongDescriptionEn,
        
        c.ParentCode,
        c.HierarchyLevel,
        c.IsLeafNode,
        c.IsBillable,
        c.Category,
        c.IsCommon,
        c.Severity,
        c.IsTranslated,
        
        ch.ChapterNumber,
        COALESCE(ch.DescriptionRo, ch.DescriptionEn) AS ChapterDescription,
        
        s.SectionCode,
        COALESCE(s.DescriptionRo, s.DescriptionEn) AS SectionDescription
        
    FROM dbo.ICD10_Codes c
    INNER JOIN dbo.ICD10_Chapters ch ON c.ChapterId = ch.ChapterId
    LEFT JOIN dbo.ICD10_Sections s ON c.SectionId = s.SectionId
    WHERE c.Code = @Code AND c.IsActive = 1
    
    -- Returneaz? termenii de includere
    SELECT 
        TermType,
        CASE @Language 
            WHEN 'en' THEN TermTextEn
            ELSE COALESCE(TermTextRo, TermTextEn)
        END AS TermText
    FROM dbo.ICD10_InclusionTerms
    WHERE ICD10_ID = (SELECT ICD10_ID FROM dbo.ICD10_Codes WHERE Code = @Code)
    ORDER BY SortOrder
    
    -- Returneaz? excluderile
    SELECT 
        ExclusionType,
        CASE @Language 
            WHEN 'en' THEN NoteTextEn
            ELSE COALESCE(NoteTextRo, NoteTextEn)
        END AS NoteText,
        ReferencedCode
    FROM dbo.ICD10_Exclusions
    WHERE ICD10_ID = (SELECT ICD10_ID FROM dbo.ICD10_Codes WHERE Code = @Code)
    ORDER BY ExclusionType, SortOrder
    
    -- Returneaz? instruc?iunile de codificare
    SELECT 
        InstructionType,
        CASE @Language 
            WHEN 'en' THEN InstructionTextEn
            ELSE COALESCE(InstructionTextRo, InstructionTextEn)
        END AS InstructionText,
        ReferencedCode
    FROM dbo.ICD10_CodingInstructions
    WHERE ICD10_ID = (SELECT ICD10_ID FROM dbo.ICD10_Codes WHERE Code = @Code)
    ORDER BY InstructionType, SortOrder
    
    -- Returneaz? notele
    SELECT 
        NoteType,
        CASE @Language 
            WHEN 'en' THEN NoteTextEn
            ELSE COALESCE(NoteTextRo, NoteTextEn)
        END AS NoteText
    FROM dbo.ICD10_Notes
    WHERE ICD10_ID = (SELECT ICD10_ID FROM dbo.ICD10_Codes WHERE Code = @Code)
    ORDER BY SortOrder
END
GO

PRINT '? Stored procedure sp_ICD10_GetByCode creat.'

-- ==========================================
-- STORED PROCEDURE 3: sp_ICD10_GetHierarchy
-- Ob?ine ierarhia pentru un cod
-- ==========================================
PRINT '?? Creare stored procedure sp_ICD10_GetHierarchy...'

IF OBJECT_ID('dbo.sp_ICD10_GetHierarchy', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_GetHierarchy
GO

CREATE PROCEDURE dbo.sp_ICD10_GetHierarchy
    @Code NVARCHAR(10),
    @Language NVARCHAR(2) = 'ro'
AS
BEGIN
    SET NOCOUNT ON;
    
    ;WITH Hierarchy AS (
        -- Anchor: codul curent
        SELECT 
            ICD10_ID,
            Code,
            CASE @Language 
                WHEN 'en' THEN ShortDescriptionEn
                ELSE COALESCE(ShortDescriptionRo, ShortDescriptionEn)
            END AS ShortDescription,
            ParentId,
            ParentCode,
            HierarchyLevel,
            0 AS PathLevel
        FROM dbo.ICD10_Codes
        WHERE Code = @Code AND IsActive = 1
        
        UNION ALL
        
        -- Recursive: p?rin?ii
        SELECT 
            p.ICD10_ID,
            p.Code,
            CASE @Language 
                WHEN 'en' THEN p.ShortDescriptionEn
                ELSE COALESCE(p.ShortDescriptionRo, p.ShortDescriptionEn)
            END AS ShortDescription,
            p.ParentId,
            p.ParentCode,
            p.HierarchyLevel,
            h.PathLevel + 1
        FROM dbo.ICD10_Codes p
        INNER JOIN Hierarchy h ON p.ICD10_ID = h.ParentId
        WHERE p.IsActive = 1
    )
    SELECT * FROM Hierarchy
    ORDER BY PathLevel DESC
END
GO

PRINT '? Stored procedure sp_ICD10_GetHierarchy creat.'

-- ==========================================
-- STORED PROCEDURE 4: sp_ICD10_GetChildren
-- Ob?ine copiii unui cod
-- ==========================================
PRINT '?? Creare stored procedure sp_ICD10_GetChildren...'

IF OBJECT_ID('dbo.sp_ICD10_GetChildren', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_GetChildren
GO

CREATE PROCEDURE dbo.sp_ICD10_GetChildren
    @ParentCode NVARCHAR(10),
    @Language NVARCHAR(2) = 'ro'
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.ICD10_ID,
        c.Code,
        CASE @Language 
            WHEN 'en' THEN c.ShortDescriptionEn
            ELSE COALESCE(c.ShortDescriptionRo, c.ShortDescriptionEn)
        END AS ShortDescription,
        c.IsLeafNode,
        c.IsBillable,
        c.IsCommon,
        c.HierarchyLevel
    FROM dbo.ICD10_Codes c
    WHERE c.ParentCode = @ParentCode
      AND c.IsActive = 1
    ORDER BY c.Code
END
GO

PRINT '? Stored procedure sp_ICD10_GetChildren creat.'

-- ==========================================
-- STORED PROCEDURE 5: sp_ICD10_GetCommonCodes
-- Ob?ine codurile frecvent utilizate
-- ==========================================
PRINT '?? Creare stored procedure sp_ICD10_GetCommonCodes...'

IF OBJECT_ID('dbo.sp_ICD10_GetCommonCodes', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_GetCommonCodes
GO

CREATE PROCEDURE dbo.sp_ICD10_GetCommonCodes
    @Category NVARCHAR(50) = NULL,
    @MaxResults INT = 100,
    @Language NVARCHAR(2) = 'ro'
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@MaxResults)
        c.ICD10_ID,
        c.Code,
        CASE @Language 
            WHEN 'en' THEN c.ShortDescriptionEn
            ELSE COALESCE(c.ShortDescriptionRo, c.ShortDescriptionEn)
        END AS ShortDescription,
        c.Category,
        c.Severity,
        c.Code + ' - ' + COALESCE(c.ShortDescriptionRo, c.ShortDescriptionEn) AS DisplayText
    FROM dbo.ICD10_Codes c
    WHERE c.IsCommon = 1
      AND c.IsLeafNode = 1
      AND c.IsActive = 1
      AND (@Category IS NULL OR c.Category = @Category)
    ORDER BY c.Category, c.Code
END
GO

PRINT '? Stored procedure sp_ICD10_GetCommonCodes creat.'

-- ==========================================
-- STORED PROCEDURE 6: sp_ICD10_UpdateTranslation
-- Actualizeaz? traducerea în român?
-- ==========================================
PRINT '?? Creare stored procedure sp_ICD10_UpdateTranslation...'

IF OBJECT_ID('dbo.sp_ICD10_UpdateTranslation', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_UpdateTranslation
GO

CREATE PROCEDURE dbo.sp_ICD10_UpdateTranslation
    @Code NVARCHAR(10),
    @ShortDescriptionRo NVARCHAR(250),
    @LongDescriptionRo NVARCHAR(1000) = NULL,
    @SearchTermsRo NVARCHAR(MAX) = NULL,
    @TranslatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.ICD10_Codes
    SET 
        ShortDescriptionRo = @ShortDescriptionRo,
        LongDescriptionRo = @LongDescriptionRo,
        SearchTermsRo = @SearchTermsRo,
        IsTranslated = 1,
        TranslatedAt = SYSUTCDATETIME(),
        TranslatedBy = @TranslatedBy,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Code = @Code
    
    SELECT @@ROWCOUNT AS RowsAffected
END
GO

PRINT '? Stored procedure sp_ICD10_UpdateTranslation creat.'

-- ==========================================
-- STORED PROCEDURE 7: sp_ICD10_GetStatistics
-- Statistici despre baza de date ICD-10
-- ==========================================
PRINT '?? Creare stored procedure sp_ICD10_GetStatistics...'

IF OBJECT_ID('dbo.sp_ICD10_GetStatistics', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_GetStatistics
GO

CREATE PROCEDURE dbo.sp_ICD10_GetStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Statistici generale
    SELECT 
        (SELECT COUNT(*) FROM dbo.ICD10_Chapters) AS TotalChapters,
        (SELECT COUNT(*) FROM dbo.ICD10_Sections) AS TotalSections,
        (SELECT COUNT(*) FROM dbo.ICD10_Codes WHERE IsActive = 1) AS TotalCodes,
        (SELECT COUNT(*) FROM dbo.ICD10_Codes WHERE IsLeafNode = 1 AND IsActive = 1) AS LeafCodes,
        (SELECT COUNT(*) FROM dbo.ICD10_Codes WHERE IsBillable = 1 AND IsActive = 1) AS BillableCodes,
        (SELECT COUNT(*) FROM dbo.ICD10_Codes WHERE IsCommon = 1 AND IsActive = 1) AS CommonCodes,
        (SELECT COUNT(*) FROM dbo.ICD10_Codes WHERE IsTranslated = 1 AND IsActive = 1) AS TranslatedCodes,
        (SELECT COUNT(*) FROM dbo.ICD10_Codes WHERE IsTranslated = 0 AND IsActive = 1) AS UntranslatedCodes
    
    -- Coduri per capitol
    SELECT 
        ch.ChapterNumber,
        COALESCE(ch.DescriptionRo, ch.DescriptionEn) AS ChapterDescription,
        COUNT(c.ICD10_ID) AS CodeCount,
        SUM(CASE WHEN c.IsTranslated = 1 THEN 1 ELSE 0 END) AS TranslatedCount
    FROM dbo.ICD10_Chapters ch
    LEFT JOIN dbo.ICD10_Codes c ON ch.ChapterId = c.ChapterId AND c.IsActive = 1
    GROUP BY ch.ChapterId, ch.ChapterNumber, ch.DescriptionRo, ch.DescriptionEn
    ORDER BY ch.ChapterNumber
    
    -- Coduri per categorie
    SELECT 
        COALESCE(Category, 'Necategorizat') AS Category,
        COUNT(*) AS CodeCount,
        SUM(CASE WHEN IsTranslated = 1 THEN 1 ELSE 0 END) AS TranslatedCount
    FROM dbo.ICD10_Codes
    WHERE IsActive = 1
    GROUP BY Category
    ORDER BY CodeCount DESC
END
GO

PRINT '? Stored procedure sp_ICD10_GetStatistics creat.'

PRINT ''
PRINT '=========================================='
PRINT '? VIEWS & STORED PROCEDURES CREATE!'
PRINT '=========================================='
PRINT ''
PRINT 'Views:'
PRINT '  ?? vw_ICD10_CodesComplete    - Toate datele'
PRINT '  ?? vw_ICD10_CommonCodes      - Coduri frecvente'
PRINT '  ?? vw_ICD10_UntranslatedCodes - F?r? traducere'
PRINT ''
PRINT 'Stored Procedures:'
PRINT '  ?? sp_ICD10_Search           - C?utare'
PRINT '  ?? sp_ICD10_GetByCode        - Detalii cod'
PRINT '  ?? sp_ICD10_GetHierarchy     - Ierarhie'
PRINT '  ?? sp_ICD10_GetChildren      - Copii'
PRINT '  ?? sp_ICD10_GetCommonCodes   - Coduri comune'
PRINT '  ?? sp_ICD10_UpdateTranslation - Actualizare traducere'
PRINT '  ?? sp_ICD10_GetStatistics    - Statistici'
PRINT ''
GO
