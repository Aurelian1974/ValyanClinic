-- ========================================
-- Stored Procedure: sp_ICD10_Search
-- Database: ValyanMed
-- Descriere: Cautare rapida coduri ICD-10 pentru autocomplete
-- UPDATED: Permite SearchTerm gol pentru a returna toate codurile
-- FIXED: Nume tabel corect ICD10_Codes (NU ICD10)
-- ========================================

USE [ValyanMed]
GO

IF OBJECT_ID('dbo.sp_ICD10_Search', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_Search
GO

CREATE PROCEDURE dbo.sp_ICD10_Search
    @SearchTerm NVARCHAR(200) = NULL,  -- ? FIXED: Permite NULL/gol
    @Category NVARCHAR(50) = NULL,
    @OnlyCommon BIT = 0,
    @OnlyLeafNodes BIT = 1,
    @MaxResults INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    -- ? REMOVED: Validare SearchTerm gol - acum este permis!
    -- Sanitize input dac? exist?
    IF @SearchTerm IS NOT NULL
    BEGIN
        SET @SearchTerm = LTRIM(RTRIM(@SearchTerm))
        IF LEN(@SearchTerm) = 0
            SET @SearchTerm = NULL  -- Treat empty string as NULL
    END
    
    -- Query cu prioritizare:
    -- 1. Coduri comune (IsCommon=1) apar primele
    -- 2. Match exact pe Code
    -- 3. Match pe ShortDescription
    -- 4. Match pe SearchTerms
    SELECT TOP (@MaxResults)
        ICD10_ID,
        Code,
        Category,
        ShortDescription,
        LongDescription,
        IsCommon,
        Severity,
        -- Calculare scor relevanta
        CASE
            -- Dac? nu avem SearchTerm, folosim scor default
            WHEN @SearchTerm IS NULL THEN 100
            -- Dac? avem SearchTerm, calcul?m relevance
            WHEN Code LIKE @SearchTerm + '%' THEN 100          -- Match exact pe cod
            WHEN Code LIKE '%' + @SearchTerm + '%' THEN 90     -- Match partial pe cod
            WHEN ShortDescription LIKE @SearchTerm + '%' THEN 80
            WHEN ShortDescription LIKE '%' + @SearchTerm + '%' THEN 70
            WHEN SearchTerms LIKE '%' + @SearchTerm + '%' THEN 60
            ELSE 50
        END AS RelevanceScore
    FROM 
        dbo.ICD10_Codes  -- ? FIXED: Nume tabel corect!
    WHERE
        (@OnlyLeafNodes = 0 OR IsLeafNode = 1)  -- Filtru leaf nodes
        AND (
            @SearchTerm IS NULL  -- ? Dac? SearchTerm e NULL, returneaz? toate
            OR Code LIKE '%' + @SearchTerm + '%'
            OR ShortDescription LIKE '%' + @SearchTerm + '%'
            OR LongDescription LIKE '%' + @SearchTerm + '%'
            OR SearchTerms LIKE '%' + @SearchTerm + '%'
        )
        AND (@Category IS NULL OR Category = @Category)
        AND (@OnlyCommon = 0 OR IsCommon = 1)
    ORDER BY
        IsCommon DESC,           -- Coduri comune primele
        RelevanceScore DESC,     -- Apoi cele mai relevante
        Code ASC                 -- Apoi alfabetic
END
GO

PRINT '? Stored Procedure sp_ICD10_Search creat cu succes cu nume tabel corect!'
GO
