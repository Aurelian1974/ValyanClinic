-- SP pentru obtinerea listei de personal cu filtrare si paginare
CREATE PROCEDURE [dbo].[sp_Personal_GetAll]
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
    
    -- Construire WHERE clause dinamic
    IF @SearchText IS NOT NULL AND @SearchText != ''
    BEGIN
        SET @WhereClause = @WhereClause + ' AND (Nume LIKE ''%' + @SearchText + '%'' OR Prenume LIKE ''%' + @SearchText + '%'' OR Email_Personal LIKE ''%' + @SearchText + '%'' OR Telefon_Personal LIKE ''%' + @SearchText + '%'') ';
    END
    
    IF @Departament IS NOT NULL AND @Departament != ''
    BEGIN
        SET @WhereClause = @WhereClause + ' AND Departament = ''' + @Departament + ''' ';
    END
    
    IF @Status IS NOT NULL AND @Status != ''
    BEGIN
        SET @WhereClause = @WhereClause + ' AND Status_Angajat = ''' + @Status + ''' ';
    END
    
    -- Query principal cu paginare
    SET @SQL = '
    SELECT 
        Id_Personal,
        Cod_Angajat,
        CNP,
        Nume,
        Prenume,
        Nume_Anterior,
        Data_Nasterii,
        Locul_Nasterii,
        Nationalitate,
        Cetatenie,
        Telefon_Personal,
        Telefon_Serviciu,
        Email_Personal,
        Email_Serviciu,
        Adresa_Domiciliu,
        Judet_Domiciliu,
        Oras_Domiciliu,
        Cod_Postal_Domiciliu,
        Adresa_Resedinta,
        Judet_Resedinta,
        Oras_Resedinta,
        Cod_Postal_Resedinta,
        Stare_Civila,
        Functia,
        Departament,
        Serie_CI,
        Numar_CI,
        Eliberat_CI_De,
        Data_Eliberare_CI,
        Valabil_CI_Pana,
        Status_Angajat,
        Observatii,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Personal ' + @WhereClause + '
    ORDER BY ' + @SortColumn + ' ' + @SortDirection + '
    OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + ' ROWS
    FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + ' ROWS ONLY';
    
    -- Query pentru total count
    SET @CountSQL = 'SELECT COUNT(*) as TotalCount FROM Personal ' + @WhereClause;
    
    -- Executare queries
    EXEC sp_executesql @SQL;
    EXEC sp_executesql @CountSQL;
END;