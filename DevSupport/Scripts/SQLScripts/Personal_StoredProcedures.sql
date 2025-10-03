-- ============================================================================
-- VALYANMED - STORED PROCEDURES PENTRU TABELA PERSONAL
-- Creat: Septembrie 2025
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

-- ============================================================================
-- 1. sp_Personal_GetAll - Obtinere lista paginata cu filtrare
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
    @SortColumn NVARCHAR(50) = 'Nume',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @CountSQL NVARCHAR(MAX);
    DECLARE @WhereClause NVARCHAR(MAX) = ' WHERE 1=1 ';
    
    -- Construieste WHERE clause dinamic
    IF @SearchText IS NOT NULL AND LEN(@SearchText) > 0
    BEGIN
        SET @WhereClause = @WhereClause + 
            ' AND (Nume LIKE ''%' + @SearchText + '%'' 
               OR Prenume LIKE ''%' + @SearchText + '%'' 
               OR Email_Personal LIKE ''%' + @SearchText + '%'') ';
    END
    
    IF @Departament IS NOT NULL AND LEN(@Departament) > 0
    BEGIN
        SET @WhereClause = @WhereClause + ' AND Departament = ''' + @Departament + ''' ';
    END
    
    IF @Status IS NOT NULL AND LEN(@Status) > 0
    BEGIN
        SET @WhereClause = @WhereClause + ' AND Status_Angajat = ''' + @Status + ''' ';
    END
    
    -- Query pentru data
    SET @SQL = '
    SELECT 
        Id_Personal, Cod_Angajat, CNP, Nume, Prenume, Nume_Anterior,
        Data_Nasterii, Locul_Nasterii, Nationalitate, Cetatenie,
        Telefon_Personal, Telefon_Serviciu, Email_Personal, Email_Serviciu,
        Adresa_Domiciliu, Judet_Domiciliu, Oras_Domiciliu, Cod_Postal_Domiciliu,
        Adresa_Resedinta, Judet_Resedinta, Oras_Resedinta, Cod_Postal_Resedinta,
        Stare_Civila, Functia, Departament, Serie_CI, Numar_CI, Eliberat_CI_De,
        Data_Eliberare_CI, Valabil_CI_Pana, Status_Angajat, Observatii,
        Data_Crearii, Data_Ultimei_Modificari, Creat_De, Modificat_De
    FROM Personal ' + 
    @WhereClause + '
    ORDER BY ' + @SortColumn + ' ' + @SortDirection + '
    OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + ' ROWS 
    FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + ' ROWS ONLY;';
    
    -- Query pentru count
    SET @CountSQL = 'SELECT COUNT(*) FROM Personal ' + @WhereClause;
    
    -- Execute data query
    EXEC sp_executesql @SQL;
    
    -- Execute count query
    EXEC sp_executesql @CountSQL;
END
GO

-- ============================================================================
-- 2. sp_Personal_GetById - Obtinere personal dupa ID
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Personal_GetById')
    DROP PROCEDURE sp_Personal_GetById
GO

CREATE PROCEDURE sp_Personal_GetById
    @Id_Personal UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
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
-- 3. sp_Personal_Create - Crearea unui nou personal
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
    @Nationalitate NVARCHAR(50) = 'Roman?',
    @Cetatenie NVARCHAR(50) = 'Roman?',
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
    
    -- Genereaza nou GUID daca nu e furnizat
    IF @Id_Personal IS NULL
        SET @Id_Personal = NEWID();
        
    DECLARE @Data_Crearii DATETIME = GETUTCDATE();
    
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
    
    -- Returneaza recordul creat
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
-- 4. sp_Personal_Update - Actualizarea unui personal existent
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Personal_Update')
    DROP PROCEDURE sp_Personal_Update
GO

CREATE PROCEDURE sp_Personal_Update
    @Id_Personal UNIQUEIDENTIFIER,
    @Cod_Angajat NVARCHAR(50),
    @CNP NVARCHAR(13),
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Nume_Anterior NVARCHAR(100) = NULL,
    @Data_Nasterii DATETIME,
    @Locul_Nasterii NVARCHAR(100) = NULL,
    @Nationalitate NVARCHAR(50),
    @Cetatenie NVARCHAR(50),
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
    @Status_Angajat NVARCHAR(50),
    @Observatii NVARCHAR(MAX) = NULL,
    @Modificat_De NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Data_Ultimei_Modificari DATETIME = GETUTCDATE();
    
    UPDATE Personal SET
        Cod_Angajat = @Cod_Angajat,
        CNP = @CNP,
        Nume = @Nume,
        Prenume = @Prenume,
        Nume_Anterior = @Nume_Anterior,
        Data_Nasterii = @Data_Nasterii,
        Locul_Nasterii = @Locul_Nasterii,
        Nationalitate = @Nationalitate,
        Cetatenie = @Cetatenie,
        Telefon_Personal = @Telefon_Personal,
        Telefon_Serviciu = @Telefon_Serviciu,
        Email_Personal = @Email_Personal,
        Email_Serviciu = @Email_Serviciu,
        Adresa_Domiciliu = @Adresa_Domiciliu,
        Judet_Domiciliu = @Judet_Domiciliu,
        Oras_Domiciliu = @Oras_Domiciliu,
        Cod_Postal_Domiciliu = @Cod_Postal_Domiciliu,
        Adresa_Resedinta = @Adresa_Resedinta,
        Judet_Resedinta = @Judet_Resedinta,
        Oras_Resedinta = @Oras_Resedinta,
        Cod_Postal_Resedinta = @Cod_Postal_Resedinta,
        Stare_Civila = @Stare_Civila,
        Functia = @Functia,
        Departament = @Departament,
        Serie_CI = @Serie_CI,
        Numar_CI = @Numar_CI,
        Eliberat_CI_De = @Eliberat_CI_De,
        Data_Eliberare_CI = @Data_Eliberare_CI,
        Valabil_CI_Pana = @Valabil_CI_Pana,
        Status_Angajat = @Status_Angajat,
        Observatii = @Observatii,
        Data_Ultimei_Modificari = @Data_Ultimei_Modificari,
        Modificat_De = @Modificat_De
    WHERE Id_Personal = @Id_Personal;
    
    -- Returneaza recordul actualizat
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
-- 5. sp_Personal_Delete - Soft delete pentru personal
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Personal_Delete')
    DROP PROCEDURE sp_Personal_Delete
GO

CREATE PROCEDURE sp_Personal_Delete
    @Id_Personal UNIQUEIDENTIFIER,
    @Modificat_De NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Data_Ultimei_Modificari DATETIME = GETUTCDATE();
    
    UPDATE Personal SET
        Status_Angajat = 'Inactiv',
        Data_Ultimei_Modificari = @Data_Ultimei_Modificari,
        Modificat_De = @Modificat_De
    WHERE Id_Personal = @Id_Personal;
    
    -- Returneaza 1 pentru success, 0 pentru failure
    SELECT CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END AS Result;
END
GO

-- ============================================================================
-- 6. sp_Personal_CheckUnique - Verificare unicitate CNP si Cod Angajat
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Personal_CheckUnique')
    DROP PROCEDURE sp_Personal_CheckUnique
GO

CREATE PROCEDURE sp_Personal_CheckUnique
    @CNP NVARCHAR(13),
    @Cod_Angajat NVARCHAR(50),
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CNP_Exists BIT = 0;
    DECLARE @CodAngajat_Exists BIT = 0;
    
    -- Check CNP
    IF EXISTS (
        SELECT 1 FROM Personal 
        WHERE CNP = @CNP 
        AND (@ExcludeId IS NULL OR Id_Personal <> @ExcludeId)
    )
        SET @CNP_Exists = 1;
    
    -- Check Cod Angajat
    IF EXISTS (
        SELECT 1 FROM Personal 
        WHERE Cod_Angajat = @Cod_Angajat 
        AND (@ExcludeId IS NULL OR Id_Personal <> @ExcludeId)
    )
        SET @CodAngajat_Exists = 1;
    
    SELECT @CNP_Exists AS CNP_Exists, @CodAngajat_Exists AS CodAngajat_Exists;
END
GO

-- ============================================================================
-- 7. sp_Personal_GetStatistics - Statistici pentru dashboard
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Personal_GetStatistics')
    DROP PROCEDURE sp_Personal_GetStatistics
GO

CREATE PROCEDURE sp_Personal_GetStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Total Personal' AS StatisticName,
        COUNT(*) AS Value,
        'fas fa-users' AS IconClass,
        'stat-blue' AS ColorClass
    FROM Personal
    
    UNION ALL
    
    SELECT 
        'Personal Activ' AS StatisticName,
        COUNT(*) AS Value,
        'fas fa-user-check' AS IconClass,
        'stat-green' AS ColorClass
    FROM Personal 
    WHERE Status_Angajat = 'Activ'
    
    UNION ALL
    
    SELECT 
        'Personal Inactiv' AS StatisticName,
        COUNT(*) AS Value,
        'fas fa-user-times' AS IconClass,
        'stat-red' AS ColorClass
    FROM Personal 
    WHERE Status_Angajat = 'Inactiv';
END
GO

-- ============================================================================
-- VERIFICARE CREARE PROCEDURI
-- ============================================================================
PRINT 'Verificare proceduri create:';
SELECT 
    name AS 'Procedura Creata',
    create_date AS 'Data Creare'
FROM sys.procedures 
WHERE name LIKE 'sp_Personal_%'
ORDER BY name;

PRINT 'Script executat cu succes! Toate procedurile stocate pentru Personal au fost create.';