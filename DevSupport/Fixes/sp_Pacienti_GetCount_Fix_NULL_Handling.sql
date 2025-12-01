-- =============================================
-- FIX URGENT: sp_Pacienti_GetCount - NULL Parameter Handling
-- Database: ValyanMed
-- Issue: SP returneaza count incorect cand @Activ si @Asigurat sunt NULL
-- Root Cause: Filtrul se aplica chiar cand parametrul este NULL
-- Fix: Verifica IS NULL inainte de a aplica filtrul
-- Date: 2025-01-06
-- =============================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'FIX: sp_Pacienti_GetCount - NULL Handling';
PRINT '========================================';
PRINT '';

-- Drop existing SP if exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetCount')
BEGIN
    DROP PROCEDURE sp_Pacienti_GetCount;
    PRINT '? Stored Procedure existent sters';
END
GO

-- Create fixed SP
CREATE PROCEDURE [dbo].[sp_Pacienti_GetCount]
    @SearchText NVARCHAR(255) = NULL,
    @Judet NVARCHAR(50) = NULL,
    @Asigurat BIT = NULL,
    @Activ BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Logging pentru debugging
    PRINT '========== sp_Pacienti_GetCount EXECUTION ==========';
    PRINT 'Parameters:';
    PRINT '  @SearchText = ' + ISNULL(@SearchText, 'NULL');
    PRINT '  @Judet = ' + ISNULL(@Judet, 'NULL');
    PRINT '  @Asigurat = ' + ISNULL(CAST(@Asigurat AS VARCHAR(5)), 'NULL');
    PRINT '  @Activ = ' + ISNULL(CAST(@Activ AS VARCHAR(5)), 'NULL');
    PRINT '';
    
    -- ? FIX: Query cu NULL handling corect
    SELECT COUNT(*) AS TotalCount
    FROM Pacienti p
    WHERE 1=1
        -- ? FIX: Verificare IS NULL pentru fiecare filtru
        AND (@SearchText IS NULL OR 
             p.Nume LIKE '%' + @SearchText + '%' OR
             p.Prenume LIKE '%' + @SearchText + '%' OR
             p.CNP LIKE '%' + @SearchText + '%' OR
             p.Telefon LIKE '%' + @SearchText + '%' OR
             p.Email LIKE '%' + @SearchText + '%' OR
             p.Cod_Pacient LIKE '%' + @SearchText + '%')
        AND (@Judet IS NULL OR p.Judet = @Judet)
        AND (@Asigurat IS NULL OR p.Asigurat = @Asigurat)  -- ? FIX: NULL handling
        AND (@Activ IS NULL OR p.Activ = @Activ);  -- ? FIX: NULL handling
    
    PRINT '========== sp_Pacienti_GetCount EXECUTION END ==========';
    PRINT '';
END
GO

PRINT '';
PRINT '? sp_Pacienti_GetCount recreat cu fix pentru NULL handling';
PRINT '';

-- =============================================
-- TEST CASES pentru verificare fix
-- =============================================

PRINT '========================================';
PRINT 'TEST CASES - Verificare Fix';
PRINT '========================================';
PRINT '';

-- Test 1: Fara filtre (toate null) - trebuie sa returneze total count
PRINT 'TEST 1: Fara filtre (@Activ=NULL, @Asigurat=NULL)';
EXEC sp_Pacienti_GetCount 
    @SearchText = NULL,
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = NULL;
PRINT '';

-- Test 2: Doar @Activ = 1
PRINT 'TEST 2: Doar pacienti activi (@Activ=1)';
EXEC sp_Pacienti_GetCount 
    @SearchText = NULL,
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = 1;
PRINT '';

-- Test 3: Doar @Asigurat = 1
PRINT 'TEST 3: Doar pacienti asigurati (@Asigurat=1)';
EXEC sp_Pacienti_GetCount 
    @SearchText = NULL,
    @Judet = NULL,
    @Asigurat = 1,
    @Activ = NULL;
PRINT '';

-- Test 4: Ambele filtre (@Activ=1, @Asigurat=1)
PRINT 'TEST 4: Pacienti activi si asigurati (@Activ=1, @Asigurat=1)';
EXEC sp_Pacienti_GetCount 
    @SearchText = NULL,
    @Judet = NULL,
    @Asigurat = 1,
    @Activ = 1;
PRINT '';

-- Test 5: Search text
PRINT 'TEST 5: Cautare dupa nume';
EXEC sp_Pacienti_GetCount 
    @SearchText = 'Iancu',
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = NULL;
PRINT '';

PRINT '========================================';
PRINT 'FIX COMPLET!';
PRINT '========================================';
PRINT '';
GO
