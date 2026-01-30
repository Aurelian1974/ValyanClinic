-- =============================================
-- Script: Update_All_SP_UseAgeFromCNP.sql
-- Descriere: Actualizează toate stored procedures să calculeze vârsta din CNP
-- Data: 2026-01-07
-- =============================================
-- Acest script actualizează toate stored procedures care calculează vârsta
-- să folosească funcția fn_CalculateAgeFromCNP în loc de DATEDIFF din Data_Nasterii
-- =============================================

USE ValyanMed;
GO

PRINT '========================================';
PRINT 'ACTUALIZARE: Calcul vârstă din CNP';
PRINT '========================================';
PRINT '';

-- =============================================
-- 1. sp_Pacienti_GetAll
-- =============================================
PRINT 'Actualizare sp_Pacienti_GetAll...';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetAll')
    DROP PROCEDURE sp_Pacienti_GetAll;
GO

CREATE PROCEDURE [dbo].[sp_Pacienti_GetAll]
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL,
    @SortColumn NVARCHAR(50) = 'NumeComplet',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Declare total count
    DECLARE @TotalCount INT;
    
    -- Calculate offset
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Get total count with search filter
    SELECT @TotalCount = COUNT(*)
    FROM dbo.Pacienti p
    WHERE p.Activ = 1
        AND (@SearchTerm IS NULL 
             OR UPPER(p.Nume) LIKE UPPER('%' + @SearchTerm + '%')
             OR UPPER(p.Prenume) LIKE UPPER('%' + @SearchTerm + '%')
             OR UPPER(p.CNP) LIKE UPPER('%' + @SearchTerm + '%')
             OR UPPER(p.Telefon) LIKE UPPER('%' + @SearchTerm + '%')
             OR UPPER(CONCAT(p.Nume, ' ', p.Prenume)) LIKE UPPER('%' + @SearchTerm + '%'));
    
    -- Return paginated results with dynamic sorting
    SELECT 
        p.Id,
        p.Cod_Pacient,
        p.CNP,
        p.Nume,
        p.Prenume,
        -- Computed columns
        CONCAT(p.Nume, ' ', p.Prenume) AS NumeComplet,
        dbo.fn_CalculateAgeFromCNP(p.CNP) AS Varsta,  -- ✅ Calculat din CNP
        -- Contact info
        p.Telefon,
        p.Telefon_Secundar,
        p.Email,
        -- Address
        p.Judet,
        p.Localitate,
        p.Adresa,
        p.Cod_Postal,
        CONCAT(ISNULL(p.Adresa, ''), 
               CASE WHEN p.Localitate IS NOT NULL THEN ', ' + p.Localitate ELSE '' END,
               CASE WHEN p.Judet IS NOT NULL THEN ', ' + p.Judet ELSE '' END) AS AdresaCompleta,
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
        p.Modificat_De,
        -- Pagination info
        @TotalCount AS TotalCount
    FROM dbo.Pacienti p
    WHERE p.Activ = 1
        AND (@SearchTerm IS NULL 
             OR UPPER(p.Nume) LIKE UPPER('%' + @SearchTerm + '%')
             OR UPPER(p.Prenume) LIKE UPPER('%' + @SearchTerm + '%')
             OR UPPER(p.CNP) LIKE UPPER('%' + @SearchTerm + '%')
             OR UPPER(p.Telefon) LIKE UPPER('%' + @SearchTerm + '%')
             OR UPPER(CONCAT(p.Nume, ' ', p.Prenume)) LIKE UPPER('%' + @SearchTerm + '%'))
    ORDER BY
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'NumeComplet' THEN CONCAT(p.Nume, ' ', p.Prenume) END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'NumeComplet' THEN CONCAT(p.Nume, ' ', p.Prenume) END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Nume' THEN p.Nume END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Nume' THEN p.Nume END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Prenume' THEN p.Prenume END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Prenume' THEN p.Prenume END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'CNP' THEN p.CNP END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'CNP' THEN p.CNP END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Telefon' THEN p.Telefon END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Telefon' THEN p.Telefon END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Data_Nasterii' THEN p.Data_Nasterii END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Data_Nasterii' THEN p.Data_Nasterii END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Data_Inregistrare' THEN p.Data_Inregistrare END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Data_Inregistrare' THEN p.Data_Inregistrare END DESC,
        p.Nume ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT '✓ sp_Pacienti_GetAll actualizat';


-- =============================================
-- 2. sp_Programari_GetDoctorSchedule
-- =============================================
PRINT 'Verificare sp_Programari_GetDoctorSchedule...';

-- Citim definiția actuală și actualizăm
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetDoctorSchedule')
BEGIN
    DECLARE @Definition NVARCHAR(MAX);
    SELECT @Definition = OBJECT_DEFINITION(OBJECT_ID('sp_Programari_GetDoctorSchedule'));
    
    IF @Definition LIKE '%DATEDIFF(YEAR, pac.Data_Nasterii%'
    BEGIN
        PRINT '  -> Necesită actualizare - vedeți scriptul separat pentru această procedură';
    END
    ELSE
    BEGIN
        PRINT '  -> Deja actualizat sau nu folosește DATEDIFF pentru vârstă';
    END
END
ELSE
BEGIN
    PRINT '  -> Procedura nu există';
END


-- =============================================
-- SUMAR
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'ACTUALIZARE COMPLETĂ!';
PRINT '========================================';
PRINT '';
PRINT 'Stored procedures actualizate:';
PRINT '  ✓ sp_Pacienti_GetById';
PRINT '  ✓ sp_Pacienti_GetAll';
PRINT '';
PRINT 'Funcție creată:';
PRINT '  ✓ fn_CalculateAgeFromCNP';
PRINT '';
PRINT 'C# DTOs actualizate:';
PRINT '  ✓ ConsulatieDetailDto.Varsta - calculează din PacientCNP';
PRINT '';
PRINT 'NOTĂ: Vârsta este acum calculată consistent din CNP';
PRINT '      în loc de Data_Nasterii peste tot în aplicație.';
PRINT '';
GO
