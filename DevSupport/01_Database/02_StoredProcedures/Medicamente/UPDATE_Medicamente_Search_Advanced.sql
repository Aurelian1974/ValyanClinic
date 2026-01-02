-- =============================================
-- UPDATE: Medicamente_Search - Advanced Multi-Word Filtering
-- Data: 2026-01-02
-- Descriere: 
--   Căutare inteligentă cu filtrare pe multiple cuvinte
--   Exemplu: "sortis 10mg" -> caută Sortis cu concentrație 10mg
--   Exemplu: "paracetamol 500 compr" -> caută Paracetamol 500mg comprimate
-- =============================================

USE [ValyanMed]
GO

-- ==================== SP: Medicamente_Search (ADVANCED) ====================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Medicamente_Search')
    DROP PROCEDURE [dbo].[Medicamente_Search]
GO

CREATE PROCEDURE [dbo].[Medicamente_Search]
    @SearchTerm NVARCHAR(100),
    @MaxResults INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Case-insensitive search cu UPPER pentru siguranță
    DECLARE @SearchTermUpper NVARCHAR(100) = UPPER(LTRIM(RTRIM(@SearchTerm)));
    
    -- Split search term în cuvinte pentru căutare inteligentă
    -- Ex: "sortis 10mg" -> caută "sortis" în denumire și "10mg" în concentrație
    DECLARE @Words TABLE (Word NVARCHAR(50));
    
    -- Simple word split (funcționează pentru majoritatea cazurilor)
    INSERT INTO @Words (Word)
    SELECT UPPER(LTRIM(RTRIM(value))) 
    FROM STRING_SPLIT(@SearchTermUpper, ' ')
    WHERE LTRIM(RTRIM(value)) <> '';
    
    -- Numără cuvinte pentru prioritizare
    DECLARE @WordCount INT = (SELECT COUNT(*) FROM @Words);
    
    SELECT TOP (@MaxResults)
        [Id],
        [CodCIM],
        [DenumireComerciala],
        [DCI],
        [FormaFarmaceutica],
        [Concentratie],
        [FirmaTaraProducatoareAPP],
        [FirmaTaraDetinatoareAPP],
        [CodATC],
        [ActiuneTerapeutica],
        [Prescriptie],
        [NrDataAmbalajAPP],
        [Ambalaj],
        [VolumAmbalaj],
        [ValabilitateAmbalaj],
        [Bulina],
        [Diez],
        [Stea],
        [Triunghi],
        [Dreptunghi],
        [DataActualizare],
        [DataImport],
        [DataUltimaActualizare],
        [Activ],
        -- Scoring pentru prioritizare rezultate
        (
            -- Match exact în denumire (prioritate maximă)
            CASE WHEN UPPER([DenumireComerciala]) LIKE @SearchTermUpper + '%' THEN 1000 ELSE 0 END +
            CASE WHEN UPPER([DenumireComerciala]) = @SearchTermUpper THEN 500 ELSE 0 END +
            
            -- Match în DCI
            CASE WHEN UPPER([DCI]) LIKE @SearchTermUpper + '%' THEN 100 ELSE 0 END +
            
            -- Match multi-cuvânt (ex: "sortis 10mg")
            CASE WHEN @WordCount > 1 THEN
                (SELECT COUNT(*) * 200 FROM @Words w 
                 WHERE UPPER([DenumireComerciala]) LIKE '%' + w.Word + '%' 
                    OR UPPER([Concentratie]) LIKE '%' + w.Word + '%'
                    OR UPPER([FormaFarmaceutica]) LIKE '%' + w.Word + '%')
            ELSE 0 END +
            
            -- Match parțial în denumire
            CASE WHEN UPPER([DenumireComerciala]) LIKE '%' + @SearchTermUpper + '%' THEN 50 ELSE 0 END
        ) AS Score
    FROM [dbo].[Medicamente_Nomenclator]
    WHERE [Activ] = 1
      AND (
          -- Căutare de bază
          UPPER([DenumireComerciala]) LIKE '%' + @SearchTermUpper + '%'
          OR UPPER([DCI]) LIKE '%' + @SearchTermUpper + '%'
          OR UPPER([CodCIM]) LIKE '%' + @SearchTermUpper + '%'
          
          -- Căutare avansată multi-cuvânt
          OR (
              @WordCount > 1 
              AND NOT EXISTS (
                  SELECT 1 FROM @Words w
                  WHERE UPPER([DenumireComerciala]) NOT LIKE '%' + w.Word + '%'
                    AND UPPER([Concentratie]) NOT LIKE '%' + w.Word + '%'
                    AND UPPER([FormaFarmaceutica]) NOT LIKE '%' + w.Word + '%'
                    AND UPPER([DCI]) NOT LIKE '%' + w.Word + '%'
              )
          )
      )
    ORDER BY 
        Score DESC,
        [DenumireComerciala]
END
GO

PRINT '✅ Medicamente_Search updated - Advanced multi-word filtering'
GO

-- ==================== TESTE ====================
PRINT ''
PRINT '========================================='
PRINT 'TEST: Căutare avansată cu filtrare'
PRINT '========================================='
PRINT ''

-- Test 1: Un cuvânt (sortis) - toate variantele
PRINT 'Test 1: "sortis" (toate variantele)'
EXEC [dbo].[Medicamente_Search] @SearchTerm = 'sortis', @MaxResults = 10
GO

PRINT ''
PRINT '----------------------------------------'
PRINT ''

-- Test 2: Două cuvinte (sortis 10) - doar 10mg
PRINT 'Test 2: "sortis 10" (doar 10mg)'
EXEC [dbo].[Medicamente_Search] @SearchTerm = 'sortis 10', @MaxResults = 10
GO

PRINT ''
PRINT '----------------------------------------'
PRINT ''

-- Test 3: Trei cuvinte cu mg
PRINT 'Test 3: "sortis 10 mg"'
EXEC [dbo].[Medicamente_Search] @SearchTerm = 'sortis 10 mg', @MaxResults = 10
GO

PRINT ''
PRINT '----------------------------------------'
PRINT ''

-- Test 4: Cu formă farmaceutică
PRINT 'Test 4: "paracetamol 500 compr"'
EXEC [dbo].[Medicamente_Search] @SearchTerm = 'paracetamol 500 compr', @MaxResults = 10
GO

PRINT ''
PRINT '✅ Update complet! Stored procedure cu:'
PRINT '   - Filtrare multi-cuvânt inteligentă'
PRINT '   - Scoring pentru prioritizare rezultate'
PRINT '   - Căutare în: Denumire, DCI, Concentrație, Formă farmaceutică'
PRINT '   - Exemplu: "sortis 10mg" -> doar Sortis 10mg (nu toate cele 100)'
