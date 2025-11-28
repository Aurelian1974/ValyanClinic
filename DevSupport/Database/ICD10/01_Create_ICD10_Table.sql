-- ========================================
-- Tabel: ICD10_Codes
-- Database: ValyanMed
-- Descriere: Clasificarea Internationala a Bolilor (Editia 10)
-- Versiune: ICD-10 OMS (Romanian)
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.ICD10_Codes', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.ICD10_Codes
    PRINT '? Tabel ICD10_Codes sters.'
END
GO

-- Create table
CREATE TABLE dbo.ICD10_Codes (
    ICD10_ID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),  -- ? Changed to UNIQUEIDENTIFIER
    Code NVARCHAR(10) NOT NULL,                    -- ex: I20.0, E11.9
    FullCode NVARCHAR(20) NOT NULL,                -- ex: I20.0 (pentru sortare)
    Category NVARCHAR(50) NOT NULL,                -- ex: Cardiovascular, Endocrin
    ShortDescription NVARCHAR(200) NOT NULL,       -- Descriere scurta (RO)
    LongDescription NVARCHAR(1000) NULL,           -- Descriere detaliata (RO)
    EnglishDescription NVARCHAR(500) NULL,         -- Descriere EN (pentru referinta)
    ParentCode NVARCHAR(10) NULL,                  -- Codul parinte (ex: I20 pentru I20.0)
    IsLeafNode BIT NOT NULL DEFAULT 1,             -- 1 = cod final (se poate folosi), 0 = categorie
    IsCommon BIT NOT NULL DEFAULT 0,               -- 1 = diagnostic comun (pentru autocomplete prioritar)
    Severity NVARCHAR(20) NULL,                    -- Mild, Moderate, Severe, Critical
    SearchTerms NVARCHAR(MAX) NULL,                -- Keywords pentru cautare (RO + sinonime)
    Notes NVARCHAR(MAX) NULL,                      -- Note medicale
    DataCreare DATETIME NOT NULL DEFAULT GETDATE(),
    DataModificare DATETIME NULL,
    
    CONSTRAINT PK_ICD10_Codes PRIMARY KEY (ICD10_ID),
    CONSTRAINT UQ_ICD10_Code UNIQUE (Code)
)
GO

-- Indexes pentru performanta
CREATE NONCLUSTERED INDEX IX_ICD10_Code_Search 
    ON dbo.ICD10_Codes (Code, ShortDescription)
GO

CREATE NONCLUSTERED INDEX IX_ICD10_Category 
    ON dbo.ICD10_Codes (Category, IsCommon DESC)
GO

CREATE NONCLUSTERED INDEX IX_ICD10_IsCommon 
    ON dbo.ICD10_Codes (IsCommon DESC, Code)
    WHERE IsLeafNode = 1
GO

-- Full-text index pentru cautare avansata
IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'ICD10_Catalog')
BEGIN
    CREATE FULLTEXT CATALOG ICD10_Catalog AS DEFAULT
    PRINT '? Full-text catalog ICD10_Catalog creat.'
END
GO

CREATE FULLTEXT INDEX ON dbo.ICD10_Codes
(
    ShortDescription LANGUAGE 1048,  -- Romanian
    LongDescription LANGUAGE 1048,
    SearchTerms LANGUAGE 1048
)
KEY INDEX PK_ICD10_Codes
GO

PRINT '? Tabel ICD10_Codes creat cu succes!'
PRINT '   - Primary Key: ICD10_ID (UNIQUEIDENTIFIER)'
PRINT '   - Unique Index: Code'
PRINT '   - Full-text Search: Enabled'
GO
