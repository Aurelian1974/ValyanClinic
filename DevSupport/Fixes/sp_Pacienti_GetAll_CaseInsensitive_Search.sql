-- =============================================
-- FIX: sp_Pacienti_GetAll - Case-Insensitive Search
-- Database: ValyanMed
-- Issue: Search is case-sensitive (LIKE operator)
-- Fix: Convert both search text and column values to UPPER() for comparison
-- Date: 2025-01-02
-- Author: GitHub Copilot
-- =============================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'FIX: sp_Pacienti_GetAll - Case-Insensitive Search';
PRINT '========================================';
PRINT '';

-- Drop existing SP if exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetAll')
BEGIN
    DROP PROCEDURE sp_Pacienti_GetAll;
    PRINT '✓ Stored Procedure existent sters';
END
GO

-- Create updated SP with case-insensitive search
CREATE PROCEDURE [dbo].[sp_Pacienti_GetAll]
    @PageNumber INT = 1,
    @PageSize INT = 25,
    @SearchText NVARCHAR(255) = NULL,
    @Judet NVARCHAR(50) = NULL,
    @Asigurat BIT = NULL,
    @Activ BIT = NULL,
    @SortColumn NVARCHAR(50) = 'Nume',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate sort direction
    IF @SortDirection NOT IN ('ASC', 'DESC')
        SET @SortDirection = 'ASC';
    
    -- Whitelist for sort columns (prevent SQL injection)
    IF @SortColumn NOT IN ('Nume', 'Prenume', 'CNP', 'Data_Nasterii', 'Cod_Pacient', 'Judet', 'Localitate', 'Telefon', 'Email')
        SET @SortColumn = 'Nume';
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- ✅ CASE-INSENSITIVE: Convert search text to uppercase
    DECLARE @SearchTextUpper NVARCHAR(255) = UPPER(@SearchText);
    
    SELECT 
        p.Id,
        p.Cod_Pacient,
        p.CNP,
        p.Nume,
        p.Prenume,
        -- Computed columns
        CONCAT(p.Nume, ' ', p.Prenume) AS NumeComplet,
        DATEDIFF(YEAR, p.Data_Nasterii, GETDATE()) AS Varsta,
        -- Contact info
        p.Telefon,
        p.Telefon_Secundar,
        p.Email,
        -- Address
        p.Judet,
        p.Localitate,
        p.Adresa,
        p.Cod_Postal,
        CONCAT(p.Adresa, ', ', p.Localitate, ', ', p.Judet) AS AdresaCompleta,
        -- Personal info
        p.Data_Nasterii,
        p.Sex,
        -- Insurance
        p.Asigurat,
        p.CNP_Asigurat,
        p.Nr_Card_Sanatate,
        p.Casa_Asigurari,
        -- Medical
        p.Alergii,
        p.Boli_Cronice,
        p.Medic_Familie,
        -- Emergency contact
        p.Persoana_Contact,
        p.Telefon_Urgenta,
        p.Relatie_Contact,
        -- Administrative
        p.Data_Inregistrare,
        p.Ultima_Vizita,
        p.Nr_Total_Vizite,
        p.Activ,
        p.Observatii,
        -- Audit
        p.Data_Crearii,
        p.Data_Ultimei_Modificari,
        p.Creat_De,
        p.Modificat_De
    FROM Pacienti p
    WHERE 1=1
        -- ✅ CASE-INSENSITIVE: Use UPPER() for comparison
        AND (@SearchText IS NULL OR 
             UPPER(p.Nume) LIKE '%' + @SearchTextUpper + '%' OR
             UPPER(p.Prenume) LIKE '%' + @SearchTextUpper + '%' OR
             UPPER(p.CNP) LIKE '%' + @SearchTextUpper + '%' OR
             UPPER(p.Telefon) LIKE '%' + @SearchTextUpper + '%' OR
             UPPER(p.Email) LIKE '%' + @SearchTextUpper + '%' OR
             UPPER(p.Cod_Pacient) LIKE '%' + @SearchTextUpper + '%')
        AND (@Judet IS NULL OR p.Judet = @Judet)
        AND (@Asigurat IS NULL OR p.Asigurat = @Asigurat)
        AND (@Activ IS NULL OR p.Activ = @Activ)
    ORDER BY 
        CASE WHEN @SortColumn = 'Nume' AND @SortDirection = 'ASC' THEN p.Nume END ASC,
        CASE WHEN @SortColumn = 'Nume' AND @SortDirection = 'DESC' THEN p.Nume END DESC,
        CASE WHEN @SortColumn = 'Prenume' AND @SortDirection = 'ASC' THEN p.Prenume END ASC,
        CASE WHEN @SortColumn = 'Prenume' AND @SortDirection = 'DESC' THEN p.Prenume END DESC,
        CASE WHEN @SortColumn = 'CNP' AND @SortDirection = 'ASC' THEN p.CNP END ASC,
        CASE WHEN @SortColumn = 'CNP' AND @SortDirection = 'DESC' THEN p.CNP END DESC,
        CASE WHEN @SortColumn = 'Data_Nasterii' AND @SortDirection = 'ASC' THEN p.Data_Nasterii END ASC,
        CASE WHEN @SortColumn = 'Data_Nasterii' AND @SortDirection = 'DESC' THEN p.Data_Nasterii END DESC,
        CASE WHEN @SortColumn = 'Cod_Pacient' AND @SortDirection = 'ASC' THEN p.Cod_Pacient END ASC,
        CASE WHEN @SortColumn = 'Cod_Pacient' AND @SortDirection = 'DESC' THEN p.Cod_Pacient END DESC,
        CASE WHEN @SortColumn = 'Judet' AND @SortDirection = 'ASC' THEN p.Judet END ASC,
        CASE WHEN @SortColumn = 'Judet' AND @SortDirection = 'DESC' THEN p.Judet END DESC,
        p.Nume ASC  -- Default fallback
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT '';
PRINT '✓ sp_Pacienti_GetAll actualizat cu cautare case-insensitive';
PRINT '';

-- =============================================
-- Update sp_Pacienti_GetCount pentru consistenta
-- =============================================

PRINT '========================================';
PRINT 'FIX: sp_Pacienti_GetCount - Case-Insensitive Search';
PRINT '========================================';
PRINT '';

-- Drop existing SP if exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetCount')
BEGIN
    DROP PROCEDURE sp_Pacienti_GetCount;
    PRINT '✓ Stored Procedure existent sters';
END
GO

CREATE PROCEDURE [dbo].[sp_Pacienti_GetCount]
    @SearchText NVARCHAR(255) = NULL,
    @Judet NVARCHAR(50) = NULL,
    @Asigurat BIT = NULL,
    @Activ BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- ✅ CASE-INSENSITIVE: Convert search text to uppercase
    DECLARE @SearchTextUpper NVARCHAR(255) = UPPER(@SearchText);
    
    SELECT COUNT(*) AS TotalCount
    FROM Pacienti p
    WHERE 1=1
        -- ✅ CASE-INSENSITIVE: Use UPPER() for comparison
        AND (@SearchText IS NULL OR 
             UPPER(p.Nume) LIKE '%' + @SearchTextUpper + '%' OR
             UPPER(p.Prenume) LIKE '%' + @SearchTextUpper + '%' OR
             UPPER(p.CNP) LIKE '%' + @SearchTextUpper + '%' OR
             UPPER(p.Telefon) LIKE '%' + @SearchTextUpper + '%' OR
             UPPER(p.Email) LIKE '%' + @SearchTextUpper + '%' OR
             UPPER(p.Cod_Pacient) LIKE '%' + @SearchTextUpper + '%')
        AND (@Judet IS NULL OR p.Judet = @Judet)
        AND (@Asigurat IS NULL OR p.Asigurat = @Asigurat)
        AND (@Activ IS NULL OR p.Activ = @Activ);
END
GO

PRINT '';
PRINT '✓ sp_Pacienti_GetCount actualizat cu cautare case-insensitive';
PRINT '';

-- =============================================
-- TEST CASES pentru verificare fix
-- =============================================

PRINT '========================================';
PRINT 'TEST CASES - Verificare Case-Insensitive Search';
PRINT '========================================';
PRINT '';

-- Test 1: Search cu lowercase
PRINT 'TEST 1: Cautare cu lowercase - "vasile"';
EXEC sp_Pacienti_GetAll 
    @PageNumber = 1, 
    @PageSize = 10,
    @SearchText = 'vasile',
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = NULL,
    @SortColumn = 'Nume',
    @SortDirection = 'ASC';
PRINT '';

-- Test 2: Search cu UPPERCASE
PRINT 'TEST 2: Cautare cu UPPERCASE - "VASILE"';
EXEC sp_Pacienti_GetAll 
    @PageNumber = 1, 
    @PageSize = 10,
    @SearchText = 'VASILE',
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = NULL,
    @SortColumn = 'Nume',
    @SortDirection = 'ASC';
PRINT '';

-- Test 3: Search cu MixedCase
PRINT 'TEST 3: Cautare cu MixedCase - "VaSiLe"';
EXEC sp_Pacienti_GetAll 
    @PageNumber = 1, 
    @PageSize = 10,
    @SearchText = 'VaSiLe',
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = NULL,
    @SortColumn = 'Nume',
    @SortDirection = 'ASC';
PRINT '';

-- Test 4: Verificare count
PRINT 'TEST 4: Verificare count cu lowercase';
EXEC sp_Pacienti_GetCount 
    @SearchText = 'vasile',
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = NULL;
PRINT '';

PRINT '========================================';
PRINT 'FIX COMPLET! Toate cele 3 teste ar trebui sa returneze ACELEASI rezultate';
PRINT '========================================';
PRINT '';
