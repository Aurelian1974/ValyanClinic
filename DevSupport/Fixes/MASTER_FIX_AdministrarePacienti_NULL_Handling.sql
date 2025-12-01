-- =============================================
-- MASTER FIX SCRIPT: AdministrarePacienti NULL Handling
-- Database: ValyanMed
-- Issue: Pagina /pacienti/administrare returneaza 0 pacienti
-- Root Cause: sp_Pacienti_GetAll si sp_Pacienti_GetCount nu trateaza corect NULL parameters
-- Solution: Recreare ambele SP-uri cu NULL handling corect
-- Date: 2025-01-06
-- =============================================

USE [ValyanMed]
GO

PRINT '??????????????????????????????????????????????????????????????????????';
PRINT '?   MASTER FIX: AdministrarePacienti NULL Handling                   ?';
PRINT '?   Fix pentru problema: "Nu s-au gasit pacienti"                   ?';
PRINT '??????????????????????????????????????????????????????????????????????';
PRINT '';
PRINT 'Problema identificata:';
PRINT '  - Pagina /pacienti/administrare returneaza 0 pacienti';
PRINT '  - Log: "Repository returned 0 items, Total=0"';
PRINT '  - Root Cause: @Activ=NULL si @Asigurat=NULL filtreaza toate records';
PRINT '';
PRINT 'Fix aplicat:';
PRINT '  - sp_Pacienti_GetAll: Verificare IS NULL inainte de filtrare';
PRINT '  - sp_Pacienti_GetCount: Verificare IS NULL inainte de num?rare';
PRINT '';
PRINT '????????????????????????????????????????????????????????????????????';
PRINT '';

-- =============================================
-- 1. FIX: sp_Pacienti_GetAll
-- =============================================

PRINT '1/2 - Recreare sp_Pacienti_GetAll...';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetAll')
    DROP PROCEDURE sp_Pacienti_GetAll;

GO

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
    
    -- Validate parameters
    IF @SortDirection NOT IN ('ASC', 'DESC')
        SET @SortDirection = 'ASC';
    
    IF @SortColumn NOT IN ('Nume', 'Prenume', 'CNP', 'Data_Nasterii', 'Cod_Pacient', 'Judet', 'Localitate', 'Telefon', 'Email')
        SET @SortColumn = 'Nume';
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Main query with NULL handling
    SELECT 
        p.Id,
        p.Cod_Pacient,
        p.CNP,
        p.Nume,
        p.Prenume,
        CONCAT(p.Nume, ' ', p.Prenume) AS NumeComplet,
        DATEDIFF(YEAR, p.Data_Nasterii, GETDATE()) AS Varsta,
        p.Telefon,
        p.Telefon_Secundar,
        p.Email,
        p.Judet,
        p.Localitate,
        p.Adresa,
        p.Cod_Postal,
        CONCAT(p.Adresa, ', ', p.Localitate, ', ', p.Judet) AS AdresaCompleta,
        p.Data_Nasterii,
        p.Sex,
        p.Asigurat,
        p.CNP_Asigurat,
        p.Nr_Card_Sanatate,
        p.Casa_Asigurari,
        p.Alergii,
        p.Boli_Cronice,
        p.Medic_Familie,
        p.Persoana_Contact,
        p.Telefon_Urgenta,
        p.Relatie_Contact,
        p.Data_Inregistrare,
        p.Ultima_Vizita,
        p.Nr_Total_Vizite,
        p.Activ,
        p.Observatii,
        p.Data_Crearii,
        p.Data_Ultimei_Modificari,
        p.Creat_De,
        p.Modificat_De
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
        AND (@Asigurat IS NULL OR p.Asigurat = @Asigurat)
        AND (@Activ IS NULL OR p.Activ = @Activ)
    ORDER BY 
        CASE WHEN @SortColumn = 'Nume' AND @SortDirection = 'ASC' THEN p.Nume END ASC,
        CASE WHEN @SortColumn = 'Nume' AND @SortDirection = 'DESC' THEN p.Nume END DESC,
        CASE WHEN @SortColumn = 'Prenume' AND @SortDirection = 'ASC' THEN p.Prenume END ASC,
        CASE WHEN @SortColumn = 'Prenume' AND @SortDirection = 'DESC' THEN p.Prenume END DESC,
        p.Nume ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT '  ? sp_Pacienti_GetAll recreat cu succes';
PRINT '';

-- =============================================
-- 2. FIX: sp_Pacienti_GetCount
-- =============================================

PRINT '2/2 - Recreare sp_Pacienti_GetCount...';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetCount')
    DROP PROCEDURE sp_Pacienti_GetCount;

GO

CREATE PROCEDURE [dbo].[sp_Pacienti_GetCount]
    @SearchText NVARCHAR(255) = NULL,
    @Judet NVARCHAR(50) = NULL,
    @Asigurat BIT = NULL,
    @Activ BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
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
        AND (@Asigurat IS NULL OR p.Asigurat = @Asigurat)
        AND (@Activ IS NULL OR p.Activ = @Activ);
END
GO

PRINT '  ? sp_Pacienti_GetCount recreat cu succes';
PRINT '';

-- =============================================
-- 3. VERIFICATION TESTS
-- =============================================

PRINT '????????????????????????????????????????????????????????????????????';
PRINT 'VERIFICARE FIX - Test Cases';
PRINT '????????????????????????????????????????????????????????????????????';
PRINT '';

-- Test 1: Fara filtre (scenario real din aplicatie)
PRINT '? TEST 1: Fara filtre (@Activ=NULL, @Asigurat=NULL) - SCENARIUL REAL';
PRINT '  Ar trebui sa returneze TOATE records din baza de date';
PRINT '';

DECLARE @TestCount INT;

EXEC sp_Pacienti_GetCount 
    @SearchText = NULL,
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = NULL;

SELECT @TestCount = COUNT(*) FROM Pacienti;
PRINT '  Total pacienti in baza de date: ' + CAST(@TestCount AS VARCHAR(10));

PRINT '  Executare sp_Pacienti_GetAll (primele 5 records):';
EXEC sp_Pacienti_GetAll 
    @PageNumber = 1, 
    @PageSize = 5,
    @SearchText = NULL,
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = NULL,
    @SortColumn = 'Nume',
    @SortDirection = 'ASC';
PRINT '';

-- Test 2: Cu filtre active
PRINT '? TEST 2: Doar pacienti activi (@Activ=1)';
EXEC sp_Pacienti_GetCount @Activ = 1;
PRINT '';

PRINT '? TEST 3: Doar pacienti asigurati (@Asigurat=1)';
EXEC sp_Pacienti_GetCount @Asigurat = 1;
PRINT '';

PRINT '? TEST 4: Pacienti activi si asigurati (@Activ=1, @Asigurat=1)';
EXEC sp_Pacienti_GetCount @Activ = 1, @Asigurat = 1;
PRINT '';

-- =============================================
-- 4. FINAL SUMMARY
-- =============================================

PRINT '????????????????????????????????????????????????????????????????????';
PRINT '??? FIX APLICAT CU SUCCES! ???';
PRINT '????????????????????????????????????????????????????????????????????';
PRINT '';
PRINT 'Ce s-a reparat:';
PRINT '  1. sp_Pacienti_GetAll - Acum trateaza corect @Activ=NULL si @Asigurat=NULL';
PRINT '  2. sp_Pacienti_GetCount - Acum returneaza count corect cu filtre NULL';
PRINT '';
PRINT 'Urmatorii pasi:';
PRINT '  1. Verifica test cases-urile de mai sus';
PRINT '  2. Restart aplicatia Blazor (CTRL+C apoi dotnet run)';
PRINT '  3. Acceseaza: https://localhost:7164/pacienti/administrare';
PRINT '  4. Ar trebui sa vezi toti pacientii din baza de date!';
PRINT '';
PRINT 'Daca problema persista:';
PRINT '  - Verifica log-urile aplicatiei pentru erori noi';
PRINT '  - Verifica ca Repository apeleaza corect SP-ul';
PRINT '  - Contacteaza echipa de dezvoltare';
PRINT '';
PRINT '????????????????????????????????????????????????????????????????????';
GO
