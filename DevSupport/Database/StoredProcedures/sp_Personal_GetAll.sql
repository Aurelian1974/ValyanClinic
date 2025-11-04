-- ========================================
-- Stored Procedure: sp_Personal_GetAll
-- Database: ValyanMed
-- Created: 10/12/2025 09:05:43
-- Modified: 10/12/2025 09:05:43
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
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
    
    -- ✅ PARAMETERIZED QUERY - NO STRING CONCATENATION
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
    
    -- ✅ SAFE: Using parameters, not concatenation
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
    
    -- ✅ EXECUTE WITH PARAMETERS
    EXEC sp_executesql @SQL, @Params, 
        @SearchText, @Departament, @Status, @Functie, @Judet, @Offset, @PageSize;
END

GO
