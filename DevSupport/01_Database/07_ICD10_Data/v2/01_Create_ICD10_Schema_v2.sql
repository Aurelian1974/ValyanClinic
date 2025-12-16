-- ========================================
-- ICD-10 Database Schema - Versiune Normalizat?
-- Database: ValyanMed
-- Versiune: 2.0 (Cu suport multilingv RO/EN)
-- Data: 2025-01-15
-- ========================================

USE [ValyanMed]
GO

PRINT '=========================================='
PRINT '  ICD-10 DATABASE SCHEMA v2.0'
PRINT '  Suport multilingv (RO principal + EN)'
PRINT '=========================================='
PRINT ''

-- ==========================================
-- DROP EXISTING TABLES (în ordine invers? pentru FK)
-- ==========================================
PRINT '??? ?tergere tabele existente...'

IF OBJECT_ID('dbo.ICD10_CodingInstructions', 'U') IS NOT NULL
    DROP TABLE dbo.ICD10_CodingInstructions
IF OBJECT_ID('dbo.ICD10_Exclusions', 'U') IS NOT NULL
    DROP TABLE dbo.ICD10_Exclusions
IF OBJECT_ID('dbo.ICD10_InclusionTerms', 'U') IS NOT NULL
    DROP TABLE dbo.ICD10_InclusionTerms
IF OBJECT_ID('dbo.ICD10_Notes', 'U') IS NOT NULL
    DROP TABLE dbo.ICD10_Notes
IF OBJECT_ID('dbo.ICD10_Codes', 'U') IS NOT NULL
    DROP TABLE dbo.ICD10_Codes
IF OBJECT_ID('dbo.ICD10_Sections', 'U') IS NOT NULL
    DROP TABLE dbo.ICD10_Sections
IF OBJECT_ID('dbo.ICD10_Chapters', 'U') IS NOT NULL
    DROP TABLE dbo.ICD10_Chapters

PRINT '? Tabele vechi ?terse.'
PRINT ''

-- ==========================================
-- TABEL 1: ICD10_Chapters (Capitole)
-- ==========================================
PRINT '?? Creare tabel ICD10_Chapters...'

CREATE TABLE dbo.ICD10_Chapters (
    -- Primary Key
    ChapterId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    
    -- Identificare
    ChapterNumber INT NOT NULL,                           -- 1, 2, 3... 22
    CodeRangeStart NVARCHAR(10) NOT NULL,                -- A00, C00, etc.
    CodeRangeEnd NVARCHAR(10) NOT NULL,                  -- B99, D49, etc.
    
    -- Descrieri (RO = principal, EN = original din XML)
    DescriptionRo NVARCHAR(500) NULL,                    -- Descriere în român? (pentru UI)
    DescriptionEn NVARCHAR(500) NOT NULL,                -- Descriere în englez? (din XML)
    
    -- Metadata
    Version NVARCHAR(10) NOT NULL DEFAULT '2026',
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Audit
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT PK_ICD10_Chapters PRIMARY KEY (ChapterId),
    CONSTRAINT UQ_ICD10_Chapters_Number UNIQUE (ChapterNumber)
)
GO

PRINT '? Tabel ICD10_Chapters creat.'

-- ==========================================
-- TABEL 2: ICD10_Sections (Sec?iuni/Blocuri)
-- ==========================================
PRINT '?? Creare tabel ICD10_Sections...'

CREATE TABLE dbo.ICD10_Sections (
    -- Primary Key
    SectionId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Key
    ChapterId UNIQUEIDENTIFIER NOT NULL,
    
    -- Identificare
    SectionCode NVARCHAR(20) NOT NULL,                   -- A00-A09, C00-C14, etc.
    CodeRangeStart NVARCHAR(10) NOT NULL,
    CodeRangeEnd NVARCHAR(10) NOT NULL,
    
    -- Descrieri
    DescriptionRo NVARCHAR(500) NULL,                    -- Român? (principal)
    DescriptionEn NVARCHAR(500) NOT NULL,                -- Englez? (din XML)
    
    -- Metadata
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Audit
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT PK_ICD10_Sections PRIMARY KEY (SectionId),
    CONSTRAINT FK_ICD10_Sections_Chapter FOREIGN KEY (ChapterId) 
        REFERENCES dbo.ICD10_Chapters(ChapterId) ON DELETE CASCADE,
    CONSTRAINT UQ_ICD10_Sections_Code UNIQUE (SectionCode)
)
GO

PRINT '? Tabel ICD10_Sections creat.'

-- ==========================================
-- TABEL 3: ICD10_Codes (Coduri - Tabel Principal)
-- ==========================================
PRINT '?? Creare tabel ICD10_Codes...'

CREATE TABLE dbo.ICD10_Codes (
    -- Primary Key
    ICD10_ID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Keys
    ChapterId UNIQUEIDENTIFIER NOT NULL,
    SectionId UNIQUEIDENTIFIER NULL,                     -- Poate fi NULL pentru categorii mari
    ParentId UNIQUEIDENTIFIER NULL,                      -- Self-reference pentru ierarhie
    
    -- ==================== IDENTIFICARE ====================
    Code NVARCHAR(10) NOT NULL,                          -- Codul ICD-10 (ex: A00.0, I21.01)
    FullCode NVARCHAR(15) NOT NULL,                      -- Pentru sortare (ex: A00.0)
    
    -- ==================== DESCRIERI (RO = Principal) ====================
    ShortDescriptionRo NVARCHAR(250) NULL,               -- Descriere scurt? în român? (UI)
    LongDescriptionRo NVARCHAR(1000) NULL,               -- Descriere detaliat? în român?
    
    -- ==================== DESCRIERI (EN = Original din XML) ====================
    ShortDescriptionEn NVARCHAR(250) NOT NULL,           -- Descriere scurt? în englez?
    LongDescriptionEn NVARCHAR(1000) NULL,               -- Descriere detaliat? în englez?
    
    -- ==================== IERARHIE ====================
    ParentCode NVARCHAR(10) NULL,                        -- Codul p?rinte (ex: A00 pentru A00.0)
    HierarchyLevel INT NOT NULL DEFAULT 0,               -- 0=Category, 1=Subcategory, 2=Code, etc.
    IsLeafNode BIT NOT NULL DEFAULT 1,                   -- True = cod final utilizabil
    IsBillable BIT NOT NULL DEFAULT 0,                   -- True = cod facturabil (valid pentru diagnostic)
    
    -- ==================== CLASIFICARE ====================
    Category NVARCHAR(50) NULL,                          -- Categorie medical? (Cardiovascular, etc.)
    IsCommon BIT NOT NULL DEFAULT 0,                     -- Cod frecvent utilizat
    Severity NVARCHAR(20) NULL,                          -- Mild, Moderate, Severe, Critical
    
    -- ==================== C?UTARE ====================
    SearchTermsRo NVARCHAR(MAX) NULL,                    -- Keywords român? pentru c?utare
    SearchTermsEn NVARCHAR(MAX) NULL,                    -- Keywords englez? pentru c?utare
    
    -- ==================== STATUS ====================
    IsTranslated BIT NOT NULL DEFAULT 0,                 -- True = are traducere în român?
    TranslatedAt DATETIME2 NULL,                         -- Când a fost tradus
    TranslatedBy NVARCHAR(100) NULL,                     -- Cine a tradus
    
    -- ==================== METADATA ====================
    Version NVARCHAR(10) NOT NULL DEFAULT '2026',
    SourceFile NVARCHAR(100) NULL,                       -- Fi?ierul surs? XML
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- ==================== AUDIT ====================
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT PK_ICD10_Codes PRIMARY KEY (ICD10_ID),
    CONSTRAINT FK_ICD10_Codes_Chapter FOREIGN KEY (ChapterId) 
        REFERENCES dbo.ICD10_Chapters(ChapterId),
    CONSTRAINT FK_ICD10_Codes_Section FOREIGN KEY (SectionId) 
        REFERENCES dbo.ICD10_Sections(SectionId),
    CONSTRAINT FK_ICD10_Codes_Parent FOREIGN KEY (ParentId) 
        REFERENCES dbo.ICD10_Codes(ICD10_ID),
    CONSTRAINT UQ_ICD10_Codes_Code UNIQUE (Code)
)
GO

PRINT '? Tabel ICD10_Codes creat.'

-- ==========================================
-- TABEL 4: ICD10_InclusionTerms (Termeni Inclu?i)
-- ==========================================
PRINT '?? Creare tabel ICD10_InclusionTerms...'

CREATE TABLE dbo.ICD10_InclusionTerms (
    -- Primary Key
    InclusionId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Key
    ICD10_ID UNIQUEIDENTIFIER NOT NULL,
    
    -- Date
    TermType NVARCHAR(20) NOT NULL,                      -- 'includes' sau 'inclusionTerm'
    TermTextEn NVARCHAR(1000) NOT NULL,                  -- Text original în englez?
    TermTextRo NVARCHAR(1000) NULL,                      -- Traducere în român?
    SortOrder INT NOT NULL DEFAULT 0,
    
    -- Audit
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    
    -- Constraints
    CONSTRAINT PK_ICD10_InclusionTerms PRIMARY KEY (InclusionId),
    CONSTRAINT FK_ICD10_InclusionTerms_Code FOREIGN KEY (ICD10_ID) 
        REFERENCES dbo.ICD10_Codes(ICD10_ID) ON DELETE CASCADE
)
GO

PRINT '? Tabel ICD10_InclusionTerms creat.'

-- ==========================================
-- TABEL 5: ICD10_Exclusions (Excluderi)
-- ==========================================
PRINT '?? Creare tabel ICD10_Exclusions...'

CREATE TABLE dbo.ICD10_Exclusions (
    -- Primary Key
    ExclusionId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Key
    ICD10_ID UNIQUEIDENTIFIER NOT NULL,
    
    -- Date
    ExclusionType NVARCHAR(10) NOT NULL,                 -- 'excludes1' sau 'excludes2'
    NoteTextEn NVARCHAR(1000) NOT NULL,                  -- Text original în englez?
    NoteTextRo NVARCHAR(1000) NULL,                      -- Traducere în român?
    ReferencedCode NVARCHAR(20) NULL,                    -- Codul referen?iat (dac? exist?)
    SortOrder INT NOT NULL DEFAULT 0,
    
    -- Audit
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    
    -- Constraints
    CONSTRAINT PK_ICD10_Exclusions PRIMARY KEY (ExclusionId),
    CONSTRAINT FK_ICD10_Exclusions_Code FOREIGN KEY (ICD10_ID) 
        REFERENCES dbo.ICD10_Codes(ICD10_ID) ON DELETE CASCADE,
    CONSTRAINT CK_ICD10_Exclusions_Type CHECK (ExclusionType IN ('excludes1', 'excludes2'))
)
GO

PRINT '? Tabel ICD10_Exclusions creat.'

-- ==========================================
-- TABEL 6: ICD10_CodingInstructions (Instruc?iuni Codificare)
-- ==========================================
PRINT '?? Creare tabel ICD10_CodingInstructions...'

CREATE TABLE dbo.ICD10_CodingInstructions (
    -- Primary Key
    InstructionId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Key
    ICD10_ID UNIQUEIDENTIFIER NOT NULL,
    
    -- Date
    InstructionType NVARCHAR(25) NOT NULL,               -- 'codeFirst', 'useAdditionalCode', 'codeAlso'
    InstructionTextEn NVARCHAR(1000) NOT NULL,           -- Text original în englez?
    InstructionTextRo NVARCHAR(1000) NULL,               -- Traducere în român?
    ReferencedCode NVARCHAR(20) NULL,                    -- Codul referen?iat
    SortOrder INT NOT NULL DEFAULT 0,
    
    -- Audit
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    
    -- Constraints
    CONSTRAINT PK_ICD10_CodingInstructions PRIMARY KEY (InstructionId),
    CONSTRAINT FK_ICD10_CodingInstructions_Code FOREIGN KEY (ICD10_ID) 
        REFERENCES dbo.ICD10_Codes(ICD10_ID) ON DELETE CASCADE,
    CONSTRAINT CK_ICD10_CodingInstructions_Type CHECK (
        InstructionType IN ('codeFirst', 'useAdditionalCode', 'codeAlso', 'codeFirstNote')
    )
)
GO

PRINT '? Tabel ICD10_CodingInstructions creat.'

-- ==========================================
-- TABEL 7: ICD10_Notes (Note Generale)
-- ==========================================
PRINT '?? Creare tabel ICD10_Notes...'

CREATE TABLE dbo.ICD10_Notes (
    -- Primary Key
    NoteId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Key
    ICD10_ID UNIQUEIDENTIFIER NOT NULL,
    
    -- Date
    NoteType NVARCHAR(20) NOT NULL DEFAULT 'general',    -- 'general', 'clinical', 'coding'
    NoteTextEn NVARCHAR(2000) NOT NULL,                  -- Text original în englez?
    NoteTextRo NVARCHAR(2000) NULL,                      -- Traducere în român?
    SortOrder INT NOT NULL DEFAULT 0,
    
    -- Audit
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    
    -- Constraints
    CONSTRAINT PK_ICD10_Notes PRIMARY KEY (NoteId),
    CONSTRAINT FK_ICD10_Notes_Code FOREIGN KEY (ICD10_ID) 
        REFERENCES dbo.ICD10_Codes(ICD10_ID) ON DELETE CASCADE
)
GO

PRINT '? Tabel ICD10_Notes creat.'

-- ==========================================
-- INDEXURI PENTRU PERFORMAN??
-- ==========================================
PRINT ''
PRINT '?? Creare indexuri...'

-- Indexuri pe ICD10_Codes
CREATE NONCLUSTERED INDEX IX_ICD10_Codes_Code 
    ON dbo.ICD10_Codes (Code)
GO

CREATE NONCLUSTERED INDEX IX_ICD10_Codes_ParentCode 
    ON dbo.ICD10_Codes (ParentCode) 
    WHERE ParentCode IS NOT NULL
GO

CREATE NONCLUSTERED INDEX IX_ICD10_Codes_ParentId 
    ON dbo.ICD10_Codes (ParentId) 
    WHERE ParentId IS NOT NULL
GO

CREATE NONCLUSTERED INDEX IX_ICD10_Codes_Chapter 
    ON dbo.ICD10_Codes (ChapterId)
GO

CREATE NONCLUSTERED INDEX IX_ICD10_Codes_Section 
    ON dbo.ICD10_Codes (SectionId) 
    WHERE SectionId IS NOT NULL
GO

CREATE NONCLUSTERED INDEX IX_ICD10_Codes_Category 
    ON dbo.ICD10_Codes (Category) 
    WHERE Category IS NOT NULL
GO

CREATE NONCLUSTERED INDEX IX_ICD10_Codes_IsCommon 
    ON dbo.ICD10_Codes (IsCommon DESC, Code)
    WHERE IsLeafNode = 1 AND IsActive = 1
GO

CREATE NONCLUSTERED INDEX IX_ICD10_Codes_IsTranslated 
    ON dbo.ICD10_Codes (IsTranslated)
    WHERE IsActive = 1
GO

CREATE NONCLUSTERED INDEX IX_ICD10_Codes_Hierarchy 
    ON dbo.ICD10_Codes (HierarchyLevel, FullCode)
GO

-- Indexuri pe tabele secundare
CREATE NONCLUSTERED INDEX IX_ICD10_InclusionTerms_Code 
    ON dbo.ICD10_InclusionTerms (ICD10_ID)
GO

CREATE NONCLUSTERED INDEX IX_ICD10_Exclusions_Code 
    ON dbo.ICD10_Exclusions (ICD10_ID)
GO

CREATE NONCLUSTERED INDEX IX_ICD10_CodingInstructions_Code 
    ON dbo.ICD10_CodingInstructions (ICD10_ID)
GO

CREATE NONCLUSTERED INDEX IX_ICD10_Notes_Code 
    ON dbo.ICD10_Notes (ICD10_ID)
GO

PRINT '? Indexuri create.'

-- ==========================================
-- FULL-TEXT SEARCH (pentru c?utare avansat?)
-- ==========================================
PRINT ''
PRINT '?? Configurare Full-Text Search...'

-- Verific? ?i creeaz? catalog Full-Text
IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'ICD10_FullText_Catalog')
BEGIN
    CREATE FULLTEXT CATALOG ICD10_FullText_Catalog AS DEFAULT
    PRINT '? Full-text catalog creat.'
END
ELSE
BEGIN
    PRINT '?? Full-text catalog exist? deja.'
END
GO

-- Full-Text Index pe ICD10_Codes
IF NOT EXISTS (
    SELECT * FROM sys.fulltext_indexes 
    WHERE object_id = OBJECT_ID('dbo.ICD10_Codes')
)
BEGIN
    CREATE FULLTEXT INDEX ON dbo.ICD10_Codes
    (
        Code LANGUAGE 1033,
        ShortDescriptionRo LANGUAGE 1048,     -- Romanian
        ShortDescriptionEn LANGUAGE 1033,     -- English
        LongDescriptionRo LANGUAGE 1048,
        LongDescriptionEn LANGUAGE 1033,
        SearchTermsRo LANGUAGE 1048,
        SearchTermsEn LANGUAGE 1033
    )
    KEY INDEX PK_ICD10_Codes
    ON ICD10_FullText_Catalog
    WITH CHANGE_TRACKING AUTO
    
    PRINT '? Full-text index pe ICD10_Codes creat.'
END
GO

PRINT ''
PRINT '=========================================='
PRINT '? SCHEMA ICD-10 CREAT? CU SUCCES!'
PRINT '=========================================='
PRINT ''
PRINT 'Tabele create:'
PRINT '  ?? ICD10_Chapters       - Capitole ICD-10'
PRINT '  ?? ICD10_Sections       - Sec?iuni/Blocuri'
PRINT '  ?? ICD10_Codes          - Coduri (PRINCIPAL)'
PRINT '  ?? ICD10_InclusionTerms - Termeni inclu?i'
PRINT '  ?? ICD10_Exclusions     - Excluderi'
PRINT '  ?? ICD10_CodingInstructions - Instruc?iuni codificare'
PRINT '  ?? ICD10_Notes          - Note generale'
PRINT ''
PRINT 'Caracteristici:'
PRINT '  ? Primary Keys: UNIQUEIDENTIFIER cu NEWSEQUENTIALID()'
PRINT '  ? Suport multilingv: Român? (principal) + Englez?'
PRINT '  ? Ierarhie complet? cu self-reference'
PRINT '  ? Full-Text Search activat'
PRINT '  ? Indexuri optimizate pentru c?utare'
PRINT ''
GO
