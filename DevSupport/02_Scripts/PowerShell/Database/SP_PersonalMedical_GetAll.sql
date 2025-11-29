-- =============================================
-- SP pentru obtinerea listei de personal medical cu filtrare ?i paginare
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetAll]
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SearchText NVARCHAR(255) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = NULL,
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
        SET @WhereClause = @WhereClause + ' AND (pm.Nume LIKE ''%' + @SearchText + '%'' OR pm.Prenume LIKE ''%' + @SearchText + '%'' OR pm.Email LIKE ''%' + @SearchText + '%'' OR pm.Telefon LIKE ''%' + @SearchText + '%'' OR pm.NumarLicenta LIKE ''%' + @SearchText + '%'') ';
    END
    
    IF @Departament IS NOT NULL AND @Departament != ''
    BEGIN
        SET @WhereClause = @WhereClause + ' AND pm.Departament = ''' + @Departament + ''' ';
    END
    
    IF @Pozitie IS NOT NULL AND @Pozitie != ''
    BEGIN
        SET @WhereClause = @WhereClause + ' AND pm.Pozitie = ''' + @Pozitie + ''' ';
    END
    
    IF @EsteActiv IS NOT NULL
    BEGIN
        SET @WhereClause = @WhereClause + ' AND pm.EsteActiv = ' + CAST(@EsteActiv AS NVARCHAR(1)) + ' ';
    END
    
    -- Query principal cu JOIN-uri pentru lookup-uri
    SET @SQL = '
    SELECT 
        pm.PersonalID,
        pm.Nume,
        pm.Prenume,
        pm.Specializare,
        pm.NumarLicenta,
        pm.Telefon,
        pm.Email,
        pm.Departament,
        pm.Pozitie,
        pm.EsteActiv,
        pm.DataCreare,
        pm.CategorieID,
        pm.SpecializareID,
        pm.SubspecializareID,
        d1.Nume AS CategorieName,
        d2.Nume AS SpecializareName,
        d3.Nume AS SubspecializareName
    FROM PersonalMedical pm
    LEFT JOIN Departamente d1 ON pm.CategorieID = d1.DepartamentID
    LEFT JOIN Departamente d2 ON pm.SpecializareID = d2.DepartamentID  
    LEFT JOIN Departamente d3 ON pm.SubspecializareID = d3.DepartamentID
    ' + @WhereClause + '
    ORDER BY pm.' + @SortColumn + ' ' + @SortDirection + '
    OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + ' ROWS
    FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + ' ROWS ONLY';
    
    -- Query pentru total count
    SET @CountSQL = 'SELECT COUNT(*) as TotalCount FROM PersonalMedical pm ' + @WhereClause;
    
    -- Executare queries
    EXEC sp_executesql @SQL;
    EXEC sp_executesql @CountSQL;
END;