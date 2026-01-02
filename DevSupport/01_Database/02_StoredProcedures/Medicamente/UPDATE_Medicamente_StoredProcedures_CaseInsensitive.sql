-- =============================================
-- UPDATE: Medicamente Stored Procedures - Case Insensitive Search + All Columns
-- Data: 2026-01-02
-- Descriere: 
--   1. Case-insensitive search cu UPPER()
--   2. Returnează toate coloanele din tabel (20 coloane ANM + audit)
--   3. Corectează numele coloanelor: CodCIM, DenumireComerciala (nu CodANM, NumeComercial)
-- =============================================

USE [ValyanMed]
GO

-- ==================== SP: Medicamente_Search (UPDATED) ====================
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
    DECLARE @SearchTermUpper NVARCHAR(100) = UPPER(@SearchTerm);
    
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
        [Activ]
    FROM [dbo].[Medicamente_Nomenclator]
    WHERE [Activ] = 1
      AND (
          UPPER([DenumireComerciala]) LIKE @SearchTermUpper + '%'
          OR UPPER([DenumireComerciala]) LIKE '%' + @SearchTermUpper + '%'
          OR UPPER([DCI]) LIKE @SearchTermUpper + '%'
          OR UPPER([CodCIM]) LIKE @SearchTermUpper + '%'
      )
    ORDER BY 
        CASE WHEN UPPER([DenumireComerciala]) LIKE @SearchTermUpper + '%' THEN 0 ELSE 1 END,
        [DenumireComerciala]
END
GO

PRINT '✅ Medicamente_Search updated - Case-insensitive + All columns'
GO

-- ==================== SP: Medicamente_GetByCod (UPDATED) ====================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Medicamente_GetByCod')
    DROP PROCEDURE [dbo].[Medicamente_GetByCod]
GO

CREATE PROCEDURE [dbo].[Medicamente_GetByCod]
    @CodCIM NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
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
        [Activ]
    FROM [dbo].[Medicamente_Nomenclator]
    WHERE [CodCIM] = @CodCIM AND [Activ] = 1
END
GO

PRINT '✅ Medicamente_GetByCod updated - All columns'
GO

-- ==================== VERIFICARE ====================
PRINT ''
PRINT '========================================='
PRINT 'TEST: Căutare case-insensitive'
PRINT '========================================='
PRINT ''

-- Test 1: Lowercase
PRINT 'Test 1: Search "paracetamol" (lowercase)'
EXEC [dbo].[Medicamente_Search] @SearchTerm = 'paracetamol', @MaxResults = 5
GO

-- Test 2: Uppercase
PRINT 'Test 2: Search "PARACETAMOL" (uppercase)'
EXEC [dbo].[Medicamente_Search] @SearchTerm = 'PARACETAMOL', @MaxResults = 5
GO

-- Test 3: MixedCase
PRINT 'Test 3: Search "ParaCetaMol" (mixed)'
EXEC [dbo].[Medicamente_Search] @SearchTerm = 'ParaCetaMol', @MaxResults = 5
GO

PRINT ''
PRINT '✅ Update complet! Stored procedures actualizate cu:'
PRINT '   - Case-insensitive search (UPPER)'
PRINT '   - Toate coloanele (24 total: 20 ANM + 4 audit)'
PRINT '   - Nume coloane corecte: CodCIM, DenumireComerciala'
