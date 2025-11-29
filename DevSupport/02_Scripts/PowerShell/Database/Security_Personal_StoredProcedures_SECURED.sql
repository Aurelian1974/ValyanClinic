-- ============================================================================
-- VALYANMED - SECURED STORED PROCEDURES - SQL INJECTION PREVENTION
-- Created: 2025-01-12
-- Version: 2.0 SECURED
-- ============================================================================

USE [ValyanMed]
GO

-- ============================================================================
-- 1. sp_Personal_GetAll - SECURED VERSION with Parameterized Queries
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Personal_GetAll')
    DROP PROCEDURE sp_Personal_GetAll
GO

CREATE PROCEDURE sp_Personal_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SearchText NVARCHAR(255) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Status NVARCHAR(50) = NULL,
    @Functie NVARCHAR(150) = NULL,
    @Judet NVARCHAR(100) = NULL,
    @SortColumn NVARCHAR(50) = 'Nume',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate sort direction to prevent injection
    IF @SortDirection NOT IN ('ASC', 'DESC')
        SET @SortDirection = 'ASC';
    
    -- Whitelist for sort columns
    IF @SortColumn NOT IN ('Nume', 'Prenume', 'Cod_Angajat', 'CNP', 'Data_Nasterii', 
                           'Status_Angajat', 'Functia', 'Departament', 'Data_Crearii')
        SET @SortColumn = 'Nume';
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- ? PARAMETERIZED QUERY - NO STRING CONCATENATION
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @Params NVARCHAR(MAX);
    
    SET @Params = N'@SearchText NVARCHAR(255), @Departament NVARCHAR(100), 
                    @Status NVARCHAR(50), @Functie NVARCHAR(150), 
                    @Judet NVARCHAR(100), @Offset INT, @PageSize INT';
    
    SET @SQL = N'
    SELECT 
        Id_Personal, Cod_Angajat, CNP, Nume, Prenume, Nume_Anterior,
        Data_Nasterii, Locul_Nasterii, Nationalitate, Cetatenie,
        Telefon_Personal, Telefon_Serviciu, Email_Personal, Email_Serviciu,
        Adresa_Domiciliu, Judet_Domiciliu, Oras_Domiciliu, Cod_Postal_Domiciliu,
        Adresa_Resedinta, Judet_Resedinta, Oras_Resedinta, Cod_Postal_Resedinta,
        Stare_Civila, Functia, Departament, Serie_CI, Numar_CI, Eliberat_CI_De,
        Data_Eliberare_CI, Valabil_CI_Pana, Status_Angajat, Observatii,
        Data_Crearii, Data_Ultimei_Modificari, Creat_De, Modificat_De
    FROM Personal
    WHERE 1=1'
    
    -- ? SAFE: Using parameters, not concatenation
    + CASE WHEN @SearchText IS NOT NULL THEN '
        AND (
            Nume LIKE ''%'' + @SearchText + ''%'' 
            OR Prenume LIKE ''%'' + @SearchText + ''%''
            OR Cod_Angajat LIKE ''%'' + @SearchText + ''%''
            OR CNP LIKE ''%'' + @SearchText + ''%''
            OR Telefon_Personal LIKE ''%'' + @SearchText + ''%''
            OR Email_Personal LIKE ''%'' + @SearchText + ''%''
            OR Functia LIKE ''%'' + @SearchText + ''%''
            OR Departament LIKE ''%'' + @SearchText + ''%''
        )' ELSE '' END
    
    + CASE WHEN @Departament IS NOT NULL THEN '
        AND Departament = @Departament' ELSE '' END
    
    + CASE WHEN @Status IS NOT NULL THEN '
        AND Status_Angajat = @Status' ELSE '' END
    
    + CASE WHEN @Functie IS NOT NULL THEN '
        AND Functia = @Functie' ELSE '' END
    
    + CASE WHEN @Judet IS NOT NULL THEN '
        AND Judet_Domiciliu = @Judet' ELSE '' END
    
    + N' ORDER BY ' + QUOTENAME(@SortColumn) + ' ' + @SortDirection + '
    OFFSET @Offset ROWS 
    FETCH NEXT @PageSize ROWS ONLY;';
    
    -- ? EXECUTE WITH PARAMETERS
    EXEC sp_executesql @SQL, @Params, 
        @SearchText, @Departament, @Status, @Functie, @Judet, @Offset, @PageSize;
END
GO

-- ============================================================================
-- 2. sp_Personal_GetCount - SECURED VERSION
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Personal_GetCount')
    DROP PROCEDURE sp_Personal_GetCount
GO

CREATE PROCEDURE sp_Personal_GetCount
    @SearchText NVARCHAR(255) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Status NVARCHAR(50) = NULL,
    @Functie NVARCHAR(150) = NULL,
    @Judet NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- ? PARAMETERIZED QUERY - NO DYNAMIC SQL NEEDED
    SELECT COUNT(*) AS TotalCount
    FROM Personal
    WHERE 1=1
        AND (@SearchText IS NULL OR (
            Nume LIKE '%' + @SearchText + '%' 
            OR Prenume LIKE '%' + @SearchText + '%'
            OR Cod_Angajat LIKE '%' + @SearchText + '%'
            OR CNP LIKE '%' + @SearchText + '%'
            OR Telefon_Personal LIKE '%' + @SearchText + '%'
            OR Email_Personal LIKE '%' + @SearchText + '%'
            OR Functia LIKE '%' + @SearchText + '%'
            OR Departament LIKE '%' + @SearchText + '%'
        ))
        AND (@Departament IS NULL OR Departament = @Departament)
        AND (@Status IS NULL OR Status_Angajat = @Status)
        AND (@Functie IS NULL OR Functia = @Functie)
        AND (@Judet IS NULL OR Judet_Domiciliu = @Judet);
END
GO

-- ============================================================================
-- 3. sp_Personal_Create - SECURED VERSION
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Personal_Create')
    DROP PROCEDURE sp_Personal_Create
GO

CREATE PROCEDURE sp_Personal_Create
    @Id_Personal UNIQUEIDENTIFIER = NULL,
    @Cod_Angajat NVARCHAR(50),
    @CNP NVARCHAR(13),
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Nume_Anterior NVARCHAR(100) = NULL,
    @Data_Nasterii DATETIME,
    @Locul_Nasterii NVARCHAR(100) = NULL,
    @Nationalitate NVARCHAR(50) = 'Romana',
    @Cetatenie NVARCHAR(50) = 'Romana',
    @Telefon_Personal NVARCHAR(20) = NULL,
    @Telefon_Serviciu NVARCHAR(20) = NULL,
    @Email_Personal NVARCHAR(255) = NULL,
    @Email_Serviciu NVARCHAR(255) = NULL,
    @Adresa_Domiciliu NVARCHAR(500),
    @Judet_Domiciliu NVARCHAR(100),
    @Oras_Domiciliu NVARCHAR(100),
    @Cod_Postal_Domiciliu NVARCHAR(10) = NULL,
    @Adresa_Resedinta NVARCHAR(500) = NULL,
    @Judet_Resedinta NVARCHAR(100) = NULL,
    @Oras_Resedinta NVARCHAR(100) = NULL,
    @Cod_Postal_Resedinta NVARCHAR(10) = NULL,
    @Stare_Civila NVARCHAR(50) = NULL,
    @Functia NVARCHAR(150),
    @Departament NVARCHAR(100) = NULL,
    @Serie_CI NVARCHAR(10) = NULL,
    @Numar_CI NVARCHAR(20) = NULL,
    @Eliberat_CI_De NVARCHAR(100) = NULL,
    @Data_Eliberare_CI DATETIME = NULL,
    @Valabil_CI_Pana DATETIME = NULL,
    @Status_Angajat NVARCHAR(50) = 'Activ',
    @Observatii NVARCHAR(MAX) = NULL,
    @Creat_De NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Generate GUID if not provided
    IF @Id_Personal IS NULL OR @Id_Personal = '00000000-0000-0000-0000-000000000000'
        SET @Id_Personal = NEWID();
        
    DECLARE @Data_Crearii DATETIME = GETUTCDATE();
    
    -- ? DIRECT INSERT - NO DYNAMIC SQL
    INSERT INTO Personal (
        Id_Personal, Cod_Angajat, CNP, Nume, Prenume, Nume_Anterior,
        Data_Nasterii, Locul_Nasterii, Nationalitate, Cetatenie,
        Telefon_Personal, Telefon_Serviciu, Email_Personal, Email_Serviciu,
        Adresa_Domiciliu, Judet_Domiciliu, Oras_Domiciliu, Cod_Postal_Domiciliu,
        Adresa_Resedinta, Judet_Resedinta, Oras_Resedinta, Cod_Postal_Resedinta,
        Stare_Civila, Functia, Departament, Serie_CI, Numar_CI, Eliberat_CI_De,
        Data_Eliberare_CI, Valabil_CI_Pana, Status_Angajat, Observatii,
        Data_Crearii, Data_Ultimei_Modificari, Creat_De, Modificat_De
    ) VALUES (
        @Id_Personal, @Cod_Angajat, @CNP, @Nume, @Prenume, @Nume_Anterior,
        @Data_Nasterii, @Locul_Nasterii, @Nationalitate, @Cetatenie,
        @Telefon_Personal, @Telefon_Serviciu, @Email_Personal, @Email_Serviciu,
        @Adresa_Domiciliu, @Judet_Domiciliu, @Oras_Domiciliu, @Cod_Postal_Domiciliu,
        @Adresa_Resedinta, @Judet_Resedinta, @Oras_Resedinta, @Cod_Postal_Resedinta,
        @Stare_Civila, @Functia, @Departament, @Serie_CI, @Numar_CI, @Eliberat_CI_De,
        @Data_Eliberare_CI, @Valabil_CI_Pana, @Status_Angajat, @Observatii,
        @Data_Crearii, @Data_Crearii, @Creat_De, @Creat_De
    );
    
    -- Return created record
    SELECT 
        Id_Personal, Cod_Angajat, CNP, Nume, Prenume, Nume_Anterior,
        Data_Nasterii, Locul_Nasterii, Nationalitate, Cetatenie,
        Telefon_Personal, Telefon_Serviciu, Email_Personal, Email_Serviciu,
        Adresa_Domiciliu, Judet_Domiciliu, Oras_Domiciliu, Cod_Postal_Domiciliu,
        Adresa_Resedinta, Judet_Resedinta, Oras_Resedinta, Cod_Postal_Resedinta,
        Stare_Civila, Functia, Departament, Serie_CI, Numar_CI, Eliberat_CI_De,
        Data_Eliberare_CI, Valabil_CI_Pana, Status_Angajat, Observatii,
        Data_Crearii, Data_Ultimei_Modificari, Creat_De, Modificat_De
    FROM Personal 
    WHERE Id_Personal = @Id_Personal;
END
GO

-- ============================================================================
-- VERIFICATION
-- ============================================================================
PRINT '============================================';
PRINT 'SECURED STORED PROCEDURES CREATED';
PRINT '============================================';

SELECT 
    name AS 'Secured Procedure',
    create_date AS 'Created',
    modify_date AS 'Modified'
FROM sys.procedures 
WHERE name IN ('sp_Personal_GetAll', 'sp_Personal_GetCount', 'sp_Personal_Create')
ORDER BY name;

PRINT '';
PRINT '? SQL INJECTION PREVENTION: COMPLETE';
PRINT '? All queries use parameterized execution';
PRINT '? Input validation implemented';
PRINT '? Whitelist for sort columns';
