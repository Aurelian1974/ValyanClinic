-- =============================================
-- Stored Procedures Suplimentare pentru Personal
-- Doar daca lipsesc in DB
-- =============================================

-- Verificare si creare sp_Personal_GetCount
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Personal_GetCount')
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_Personal_GetCount]
        @SearchText NVARCHAR(255) = NULL,
        @Departament NVARCHAR(100) = NULL,
        @Status NVARCHAR(50) = NULL
    AS
    BEGIN
        SET NOCOUNT ON;
        
        DECLARE @WhereClause NVARCHAR(MAX) = '' WHERE 1=1 '';
        
        IF @SearchText IS NOT NULL AND @SearchText != ''''
        BEGIN
            SET @WhereClause = @WhereClause + '' AND (Nume LIKE N''''%'' + @SearchText + ''%'''' 
                                                     OR Prenume LIKE N''''%'' + @SearchText + ''%'''' 
                                                     OR Email_Personal LIKE N''''%'' + @SearchText + ''%'''' 
                                                     OR Telefon_Personal LIKE N''''%'' + @SearchText + ''%'''') '';
        END
        
        IF @Departament IS NOT NULL AND @Departament != ''''
        BEGIN
            SET @WhereClause = @WhereClause + '' AND Departament = N'''''' + @Departament + '''''' '';
        END
        
        IF @Status IS NOT NULL AND @Status != ''''
        BEGIN
            SET @WhereClause = @WhereClause + '' AND Status_Angajat = N'''''' + @Status + '''''' '';
        END
        
        DECLARE @SQL NVARCHAR(MAX) = ''SELECT COUNT(*) as TotalCount FROM Personal '' + @WhereClause;
        
        EXEC sp_executesql @SQL;
    END
    ')
    PRINT '? sp_Personal_GetCount creat cu succes'
END
ELSE
BEGIN
    PRINT '? sp_Personal_GetCount exista deja'
END
GO

-- =============================================
-- Stored Procedures pentru Location (Judete/Localitati)
-- Wrapper-e pentru compatibilitate cu codul nou
-- =============================================

-- sp_Location_GetJudete (wrapper peste sp_Judete_GetAll)
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Location_GetJudete')
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_Location_GetJudete]
    AS
    BEGIN
        SET NOCOUNT ON;
        EXEC sp_Judete_GetAll;
    END
    ')
    PRINT '? sp_Location_GetJudete creat ca wrapper'
END
GO

-- sp_Location_GetLocalitatiByJudetId (wrapper)
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Location_GetLocalitatiByJudetId')
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_Location_GetLocalitatiByJudetId]
        @JudetId INT
    AS
    BEGIN
        SET NOCOUNT ON;
        EXEC sp_Localitati_GetByJudetId @JudetId;
    END
    ')
    PRINT '? sp_Location_GetLocalitatiByJudetId creat ca wrapper'
END
GO

-- sp_Location_GetJudetNameById
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Location_GetJudetNameById')
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_Location_GetJudetNameById]
        @JudetId INT
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT Nume 
        FROM Judete 
        WHERE Id = @JudetId;
    END
    ')
    PRINT '? sp_Location_GetJudetNameById creat'
END
GO

-- sp_Location_GetLocalitateNameById
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Location_GetLocalitateNameById')
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_Location_GetLocalitateNameById]
        @LocalitateId INT
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT Nume 
        FROM Localitati 
        WHERE Id = @LocalitateId;
    END
    ')
    PRINT '? sp_Location_GetLocalitateNameById creat'
END
GO

-- =============================================
-- Stored Procedures pentru Lookup (Departamente)
-- =============================================

-- sp_Lookup_GetDepartamente (wrapper)
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Lookup_GetDepartamente')
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_Lookup_GetDepartamente]
    AS
    BEGIN
        SET NOCOUNT ON;
        EXEC sp_Departamente_GetAll;
    END
    ')
    PRINT '? sp_Lookup_GetDepartamente creat ca wrapper'
END
GO

-- sp_Lookup_GetDepartamentNameById
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Lookup_GetDepartamentNameById')
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_Lookup_GetDepartamentNameById]
        @DepartamentId INT
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT Nume 
        FROM Departamente 
        WHERE Id = @DepartamentId;
    END
    ')
    PRINT '? sp_Lookup_GetDepartamentNameById creat'
END
GO

-- =============================================
-- Verificare finala
-- =============================================
PRINT ''
PRINT '========================================='
PRINT 'VERIFICARE STORED PROCEDURES'
PRINT '========================================='

SELECT 
    name as ProcedureName,
    create_date as CreateDate
FROM sys.procedures 
WHERE name LIKE 'sp_Personal%' 
   OR name LIKE 'sp_Location%'
   OR name LIKE 'sp_Lookup%'
ORDER BY name

PRINT ''
PRINT '? Stored procedures actualizate cu succes!'
