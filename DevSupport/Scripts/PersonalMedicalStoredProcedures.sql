-- =============================================
-- Stored Procedures pentru tabela PersonalMedical
-- CRUD operations cu relaii ctre tabela Departamente
-- =============================================

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

-- =============================================
-- SP pentru obtinerea statisticilor personal medical
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Total Personal Medical' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-md' as IconClass,
        '#007bff' as ColorClass
    FROM PersonalMedical
    
    UNION ALL
    
    SELECT 
        'Personal Activ' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-check' as IconClass,
        '#28a745' as ColorClass
    FROM PersonalMedical 
    WHERE EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Personal Inactiv' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-times' as IconClass,
        '#dc3545' as ColorClass
    FROM PersonalMedical 
    WHERE EsteActiv = 0
    
    UNION ALL
    
    SELECT 
        'Doctori' as StatisticName,
        COUNT(*) as Value,
        'fas fa-stethoscope' as IconClass,
        '#17a2b8' as ColorClass
    FROM PersonalMedical 
    WHERE Pozitie LIKE '%Doctor%' AND EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Asistenti Medicali' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-nurse' as IconClass,
        '#6f42c1' as ColorClass
    FROM PersonalMedical 
    WHERE Pozitie LIKE '%Asistent%' AND EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Tehnicieni' as StatisticName,
        COUNT(*) as Value,
        'fas fa-microscope' as IconClass,
        '#fd7e14' as ColorClass
    FROM PersonalMedical 
    WHERE Pozitie LIKE '%Tehnician%' AND EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Receptioneri' as StatisticName,
        COUNT(*) as Value,
        'fas fa-desktop' as IconClass,
        '#6c757d' as ColorClass
    FROM PersonalMedical 
    WHERE Pozitie LIKE '%Receptioner%' AND EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Cu Licenta' as StatisticName,
        COUNT(*) as Value,
        'fas fa-certificate' as IconClass,
        '#e83e8c' as ColorClass
    FROM PersonalMedical 
    WHERE NumarLicenta IS NOT NULL AND NumarLicenta != '' AND EsteActiv = 1;
END;

-- =============================================
-- SP pentru obinerea distribuiei personalului medical pe departamente
-- Folosit pentru statisticile din AdministrarePersonalMedical
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetDistributiePerDepartament]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ISNULL(pm.Departament, 'Nedefinit') as Categorie,
        COUNT(*) as Numar
    FROM PersonalMedical pm
    WHERE pm.EsteActiv = 1  -- Doar personalul activ
    GROUP BY pm.Departament
    ORDER BY Numar DESC, Categorie ASC;
END;

-- =============================================
-- SP pentru obinerea distribuiei personalului medical pe specializri
-- Folosit pentru statisticile din AdministrarePersonalMedical
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetDistributiePerSpecializare]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CASE 
            WHEN pm.Specializare IS NULL OR pm.Specializare = '' THEN 'Nespecializat'
            ELSE pm.Specializare
        END as Categorie,
        COUNT(*) as Numar
    FROM PersonalMedical pm
    WHERE pm.EsteActiv = 1  -- Doar personalul activ
    GROUP BY pm.Specializare
    ORDER BY Numar DESC, Categorie ASC;
END;

-- =============================================
-- SP pentru obtinerea unui personal medical dupa ID
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetById]
    @PersonalID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
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
    WHERE pm.PersonalID = @PersonalID;
END;

-- =============================================
-- SP pentru verificarea unit??ii Email ?i NumarLicenta
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_CheckUnique]
    @Email VARCHAR(100) = NULL,
    @NumarLicenta VARCHAR(50) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CASE WHEN EXISTS (
            SELECT 1 FROM PersonalMedical 
            WHERE Email = @Email 
            AND (@ExcludeId IS NULL OR PersonalID != @ExcludeId)
        ) THEN 1 ELSE 0 END as Email_Exists,
        
        CASE WHEN EXISTS (
            SELECT 1 FROM PersonalMedical 
            WHERE NumarLicenta = @NumarLicenta 
            AND (@ExcludeId IS NULL OR PersonalID != @ExcludeId)
        ) THEN 1 ELSE 0 END as NumarLicenta_Exists;
END;

-- =============================================
-- SP pentru crearea unui nou personal medical
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_Create]
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Specializare NVARCHAR(100) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @Telefon NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = 1,
    @CategorieID UNIQUEIDENTIFIER = NULL,
    @SpecializareID UNIQUEIDENTIFIER = NULL,
    @SubspecializareID UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        
        -- Verificare unicitate Email ?i NumarLicenta
        IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM PersonalMedical WHERE Email = @Email)
        BEGIN
            THROW 50001, 'Email-ul exista deja in baza de date.', 1;
        END
        
        IF @NumarLicenta IS NOT NULL AND EXISTS (SELECT 1 FROM PersonalMedical WHERE NumarLicenta = @NumarLicenta)
        BEGIN
            THROW 50002, 'Numarul de licenta exista deja in baza de date.', 1;
        END
        
        INSERT INTO PersonalMedical (
            PersonalID, Nume, Prenume, Specializare, NumarLicenta,
            Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare,
            CategorieID, SpecializareID, SubspecializareID
        ) VALUES (
            @NewId, @Nume, @Prenume, @Specializare, @NumarLicenta,
            @Telefon, @Email, @Departament, @Pozitie, @EsteActiv, @CurrentDate,
            @CategorieID, @SpecializareID, @SubspecializareID
        );
        
        COMMIT TRANSACTION;
        
        -- Returnare personal medical creat cu lookup-uri
        EXEC sp_PersonalMedical_GetById @NewId;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

-- =============================================
-- SP pentru actualizarea unui personal medical
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_Update]
    @PersonalID UNIQUEIDENTIFIER,
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Specializare NVARCHAR(100) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @Telefon NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = 1,
    @CategorieID UNIQUEIDENTIFIER = NULL,
    @SpecializareID UNIQUEIDENTIFIER = NULL,
    @SubspecializareID UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE PersonalID = @PersonalID)
        BEGIN
            THROW 50003, 'Personalul medical nu a fost gasit.', 1;
        END
        
        -- Verificare unicitate Email ?i NumarLicenta (exclude ID-ul curent)
        IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM PersonalMedical WHERE Email = @Email AND PersonalID != @PersonalID)
        BEGIN
            THROW 50001, 'Email-ul exista deja in baza de date.', 1;
        END
        
        IF @NumarLicenta IS NOT NULL AND EXISTS (SELECT 1 FROM PersonalMedical WHERE NumarLicenta = @NumarLicenta AND PersonalID != @PersonalID)
        BEGIN
            THROW 50002, 'Numarul de licenta exista deja in baza de date.', 1;
        END
        
        UPDATE PersonalMedical SET
            Nume = @Nume,
            Prenume = @Prenume,
            Specializare = @Specializare,
            NumarLicenta = @NumarLicenta,
            Telefon = @Telefon,
            Email = @Email,
            Departament = @Departament,
            Pozitie = @Pozitie,
            EsteActiv = @EsteActiv,
            CategorieID = @CategorieID,
            SpecializareID = @SpecializareID,
            SubspecializareID = @SubspecializareID
        WHERE PersonalID = @PersonalID;
        
        COMMIT TRANSACTION;
        
        -- Returnare personal medical actualizat cu lookup-uri
        EXEC sp_PersonalMedical_GetById @PersonalID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

-- =============================================
-- SP pentru stergerea unui personal medical (soft delete)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_Delete]
    @PersonalID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE PersonalID = @PersonalID)
        BEGIN
            THROW 50003, 'Personalul medical nu a fost gasit.', 1;
        END
        
        -- Soft delete - setare EsteActiv pe false
        UPDATE PersonalMedical SET
            EsteActiv = 0
        WHERE PersonalID = @PersonalID;
        
        COMMIT TRANSACTION;
        
        SELECT 1 as Success;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

-- =============================================
-- SP pentru obtinerea optiunilor pentru dropdown-uri
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetDropdownOptions]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Departamente distincte
    SELECT DISTINCT 
        Departament as Value,
        Departament as Text
    FROM PersonalMedical 
    WHERE Departament IS NOT NULL 
    ORDER BY Departament;
    
    -- Pozitii distincte
    SELECT DISTINCT 
        Pozitie as Value,
        Pozitie as Text
    FROM PersonalMedical 
    WHERE Pozitie IS NOT NULL 
    ORDER BY Pozitie;
    
    -- Specializari distincte
    SELECT DISTINCT 
        Specializare as Value,
        Specializare as Text
    FROM PersonalMedical 
    WHERE Specializare IS NOT NULL 
    ORDER BY Specializare;
END;

-- =============================================
-- SP pentru obtinerea departamentelor din tabela Departamente
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Departamente_GetByTip]
    @Tip NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        DepartamentID,
        Nume,
        Tip
    FROM Departamente 
    WHERE Tip = @Tip
    ORDER BY Nume;
END;