-- =============================================
-- FIX URGENT: sp_Pacienti_GetAll - NULL Parameter Handling
-- Database: ValyanMed
-- Issue: SP returneaza 0 records cand @Activ si @Asigurat sunt NULL
-- Root Cause: Filtrul se aplica chiar cand parametrul este NULL
-- Fix: Verifica IS NULL inainte de a aplica filtrul
-- Date: 2025-01-06
-- =============================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'FIX: sp_Pacienti_GetAll - NULL Handling';
PRINT '========================================';
PRINT '';

-- Drop existing SP if exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetAll')
BEGIN
    DROP PROCEDURE sp_Pacienti_GetAll;
    PRINT '? Stored Procedure existent sters';
END
GO

-- Create fixed SP
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
    
    -- Logging pentru debugging
    PRINT '========== sp_Pacienti_GetAll EXECUTION ==========';
    PRINT 'Parameters:';
    PRINT '  @PageNumber = ' + CAST(@PageNumber AS VARCHAR(10));
    PRINT '  @PageSize = ' + CAST(@PageSize AS VARCHAR(10));
    PRINT '  @SearchText = ' + ISNULL(@SearchText, 'NULL');
    PRINT '  @Judet = ' + ISNULL(@Judet, 'NULL');
    PRINT '  @Asigurat = ' + ISNULL(CAST(@Asigurat AS VARCHAR(5)), 'NULL');
    PRINT '  @Activ = ' + ISNULL(CAST(@Activ AS VARCHAR(5)), 'NULL');
    PRINT '  @SortColumn = ' + @SortColumn;
    PRINT '  @SortDirection = ' + @SortDirection;
    PRINT '';
    
    -- Validate sort direction
    IF @SortDirection NOT IN ('ASC', 'DESC')
        SET @SortDirection = 'ASC';
    
    -- Whitelist for sort columns (prevent SQL injection)
    IF @SortColumn NOT IN ('Nume', 'Prenume', 'CNP', 'Data_Nasterii', 'Cod_Pacient', 'Judet', 'Localitate', 'Telefon', 'Email')
        SET @SortColumn = 'Nume';
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- ? FIX: Query cu NULL handling corect
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
        AND (@Activ IS NULL OR p.Activ = @Activ)  -- ? FIX: NULL handling
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
    
    -- Return total count (optional second result set)
    SELECT COUNT(*) AS TotalCount
    FROM Pacienti p
    WHERE 1=1
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
    
    PRINT '========== sp_Pacienti_GetAll EXECUTION END ==========';
    PRINT '';
END
GO

PRINT '';
PRINT '? sp_Pacienti_GetAll recreat cu fix pentru NULL handling';
PRINT '';

-- =============================================
-- TEST CASES pentru verificare fix
-- =============================================

PRINT '========================================';
PRINT 'TEST CASES - Verificare Fix';
PRINT '========================================';
PRINT '';

-- Test 1: Fara filtre (toate null) - trebuie sa returneze TOATE records
PRINT 'TEST 1: Fara filtre (@Activ=NULL, @Asigurat=NULL)';
EXEC sp_Pacienti_GetAll 
    @PageNumber = 1, 
    @PageSize = 10,
    @SearchText = NULL,
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = NULL,
    @SortColumn = 'Nume',
    @SortDirection = 'ASC';
PRINT '';

-- Test 2: Doar @Activ = 1
PRINT 'TEST 2: Doar pacienti activi (@Activ=1)';
EXEC sp_Pacienti_GetAll 
    @PageNumber = 1, 
    @PageSize = 10,
    @SearchText = NULL,
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = 1,
    @SortColumn = 'Nume',
    @SortDirection = 'ASC';
PRINT '';

-- Test 3: Doar @Asigurat = 1
PRINT 'TEST 3: Doar pacienti asigurati (@Asigurat=1)';
EXEC sp_Pacienti_GetAll 
    @PageNumber = 1, 
    @PageSize = 10,
    @SearchText = NULL,
    @Judet = NULL,
    @Asigurat = 1,
    @Activ = NULL,
    @SortColumn = 'Nume',
    @SortDirection = 'ASC';
PRINT '';

-- Test 4: Ambele filtre (@Activ=1, @Asigurat=1)
PRINT 'TEST 4: Pacienti activi si asigurati (@Activ=1, @Asigurat=1)';
EXEC sp_Pacienti_GetAll 
    @PageNumber = 1, 
    @PageSize = 10,
    @SearchText = NULL,
    @Judet = NULL,
    @Asigurat = 1,
    @Activ = 1,
    @SortColumn = 'Nume',
    @SortDirection = 'ASC';
PRINT '';

-- Test 5: Search text
PRINT 'TEST 5: Cautare dupa nume';
EXEC sp_Pacienti_GetAll 
    @PageNumber = 1, 
    @PageSize = 10,
    @SearchText = 'Iancu',
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = NULL,
    @SortColumn = 'Nume',
    @SortDirection = 'ASC';
PRINT '';

PRINT '========================================';
PRINT 'FIX COMPLET!';
PRINT '========================================';
PRINT '';
PRINT 'INSTRUCTIUNI:';
PRINT '1. Ruleaza acest script in SQL Server Management Studio (SSMS)';
PRINT '2. Verifica test cases-urile de mai sus';
PRINT '3. Daca TEST 1 returneaza records, FIX-ul functioneaza!';
PRINT '4. Restart aplicatia Blazor';
PRINT '5. Acceseaza /pacienti/administrare';
PRINT '6. Ar trebui sa vezi toti pacientii!';
PRINT '';
GO
