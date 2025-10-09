-- ========================================
-- Stored Procedure: sp_Personal_GetCount
-- Database: ValyanMed
-- Created: 10/08/2025 15:35:56
-- Modified: 10/08/2025 15:35:56
-- Generat: 2025-10-08 16:36:44
-- ========================================

USE [ValyanMed]
GO


CREATE PROCEDURE sp_Personal_GetCount
    @SearchText NVARCHAR(255) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Status NVARCHAR(50) = NULL,
    @Functie NVARCHAR(150) = NULL,
    @Judet NVARCHAR(100) = NULL           -- ✨ NOU PARAMETRU
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @WhereClause NVARCHAR(MAX) = ' WHERE 1=1 ';
    DECLARE @CountSQL NVARCHAR(MAX);
    
    -- Global search
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
    
    -- Specific filters
    IF @Departament IS NOT NULL AND LEN(@Departament) > 0
    BEGIN
        SET @WhereClause = @WhereClause + ' AND Departament = ''' + @Departament + ''' ';
    END
    
    IF @Status IS NOT NULL AND LEN(@Status) > 0
    BEGIN
        SET @WhereClause = @WhereClause + ' AND Status_Angajat = ''' + @Status + ''' ';
    END
    
    IF @Functie IS NOT NULL AND LEN(@Functie) > 0
    BEGIN
        SET @WhereClause = @WhereClause + ' AND Functia = ''' + @Functie + ''' ';
    END
    
    -- ✨ NOU FILTRU JUDET
    IF @Judet IS NOT NULL AND LEN(@Judet) > 0
    BEGIN
        SET @WhereClause = @WhereClause + ' AND Judet_Domiciliu = ''' + @Judet + ''' ';
    END
    
    SET @CountSQL = 'SELECT COUNT(*) FROM Personal ' + @WhereClause;
    
    EXEC sp_executesql @CountSQL;
END

GO
