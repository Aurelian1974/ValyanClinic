-- ============================================================================
-- FIX: sp_Personal_GetAll - Cautare Globala Extinsa
-- Data: 2025-01-XX
-- Scop: Adauga toate campurile relevante in cautarea globala
-- ============================================================================

USE [ValyanMed]
GO

-- Drop existing procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Personal_GetAll')
    DROP PROCEDURE sp_Personal_GetAll
GO

-- Recreate with extended global search
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
    
    -- ========================================================================
    -- GLOBAL SEARCH - Cauta in toate campurile relevante
    -- ========================================================================
    IF @SearchText IS NOT NULL AND LEN(@SearchText) > 0
    BEGIN
        SET @WhereClause = @WhereClause + 
            ' AND (
                Nume LIKE ''%' + @SearchText + '%'' 
                OR Prenume LIKE ''%' + @SearchText + '%'' 
                OR Cod_Angajat LIKE ''%' + @SearchText + '%''
                OR CNP LIKE ''%' + @SearchText + '%''
                OR Telefon_Personal LIKE ''%' + @SearchText + '%''
                OR Telefon_Serviciu LIKE ''%' + @SearchText + '%''
                OR Email_Personal LIKE ''%' + @SearchText + '%''
                OR Email_Serviciu LIKE ''%' + @SearchText + '%''
                OR Functia LIKE ''%' + @SearchText + '%''
                OR Departament LIKE ''%' + @SearchText + '%''
            ) ';
    END
    
    -- ========================================================================
    -- SPECIFIC FILTERS
    -- ========================================================================
    IF @Departament IS NOT NULL AND LEN(@Departament) > 0
    BEGIN
        SET @WhereClause = @WhereClause + ' AND Departament = ''' + @Departament + ''' ';
    END
    
    IF @Status IS NOT NULL AND LEN(@Status) > 0
    BEGIN
        SET @WhereClause = @WhereClause + ' AND Status_Angajat = ''' + @Status + ''' ';
    END
    
    -- ========================================================================
    -- QUERY PENTRU DATE PAGINATA
    -- ========================================================================
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
    
    -- ========================================================================
    -- QUERY PENTRU TOTAL COUNT
    -- ========================================================================
    SET @CountSQL = 'SELECT COUNT(*) FROM Personal ' + @WhereClause;
    
    -- ========================================================================
    -- EXECUTE QUERIES
    -- ========================================================================
    EXEC sp_executesql @SQL;
    EXEC sp_executesql @CountSQL;
END
GO

-- ============================================================================
-- VERIFICARE
-- ============================================================================
PRINT '✅ Stored procedure sp_Personal_GetAll actualizat cu succes!';
PRINT '';
PRINT '🔍 Cautarea globala acum include campurile:';
PRINT '   - Nume';
PRINT '   - Prenume';
PRINT '   - Cod_Angajat';
PRINT '   - CNP';
PRINT '   - Telefon_Personal';
PRINT '   - Telefon_Serviciu';
PRINT '   - Email_Personal';
PRINT '   - Email_Serviciu';
PRINT '   - Functia';
PRINT '   - Departament';
PRINT '';

-- Test cautare globala
PRINT '🧪 Testare cautare globala:';
DECLARE @TestPageNumber INT = 1;
DECLARE @TestPageSize INT = 10;
DECLARE @TestSearch NVARCHAR(255) = 'Sorin'; -- schimba cu un search text relevant

EXEC sp_Personal_GetAll 
    @PageNumber = @TestPageNumber,
    @PageSize = @TestPageSize,
    @SearchText = @TestSearch;
    
PRINT '';
PRINT '✅ Test executat cu succes!';
