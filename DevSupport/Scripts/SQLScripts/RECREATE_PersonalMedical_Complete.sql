-- =============================================
-- RECREATE COMPLETE: PersonalMedical Tables & SPs
-- Based on C# Domain Models
-- Database: ValyanMed
-- =============================================

USE [ValyanMed]
GO

PRINT '=============================================='
PRINT 'RECREATING PersonalMedical - COMPLETE SETUP'
PRINT 'Based on C# Domain Models'
PRINT '=============================================='
PRINT ''

-- =============================================
-- STEP 1: DROP EXISTING OBJECTS
-- =============================================
PRINT 'STEP 1: Dropping existing objects...'
PRINT '----------------------------------------------'

-- Drop SPs
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_GetAll')
    DROP PROCEDURE sp_PersonalMedical_GetAll
    
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_GetById')
    DROP PROCEDURE sp_PersonalMedical_GetById
    
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_Create')
    DROP PROCEDURE sp_PersonalMedical_Create
    
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_Update')
    DROP PROCEDURE sp_PersonalMedical_Update
    
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_Delete')
    DROP PROCEDURE sp_PersonalMedical_Delete
    
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_CheckUnique')
    DROP PROCEDURE sp_PersonalMedical_CheckUnique

PRINT 'Old SPs dropped'

-- Drop Table
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PersonalMedical')
    DROP TABLE PersonalMedical

PRINT 'Old table dropped'
PRINT ''

-- =============================================
-- STEP 2: CREATE TABLE PersonalMedical
-- =============================================
PRINT 'STEP 2: Creating PersonalMedical table...'
PRINT '----------------------------------------------'

CREATE TABLE PersonalMedical (
    -- Primary Key
    PersonalID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    
    -- Basic Info
    Nume NVARCHAR(100) NOT NULL,
    Prenume NVARCHAR(100) NOT NULL,
    
    -- Medical Info
    Specializare NVARCHAR(100) NULL,
    NumarLicenta NVARCHAR(50) NULL,
    
    -- Contact
    Telefon NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    
    -- Position
    Departament NVARCHAR(100) NULL,
    Pozitie NVARCHAR(50) NULL,
    
    -- Status
    EsteActiv BIT NULL DEFAULT 1,
    
    -- Foreign Keys (References)
    CategorieID UNIQUEIDENTIFIER NULL,
    SpecializareID UNIQUEIDENTIFIER NULL,
    SubspecializareID UNIQUEIDENTIFIER NULL,
    
    -- Audit
    DataCreare DATETIME2 NULL DEFAULT GETDATE(),
    
    -- Constraints
    CONSTRAINT FK_PersonalMedical_Personal FOREIGN KEY (PersonalID) 
        REFERENCES Personal(Id_Personal) ON DELETE CASCADE,
    
    CONSTRAINT FK_PersonalMedical_Categorie FOREIGN KEY (CategorieID) 
        REFERENCES Departamente(IdDepartament) ON DELETE SET NULL,
    
    CONSTRAINT FK_PersonalMedical_Specializare FOREIGN KEY (SpecializareID) 
        REFERENCES Specializari(Id) ON DELETE SET NULL,
    
    CONSTRAINT FK_PersonalMedical_Subspecializare FOREIGN KEY (SubspecializareID) 
        REFERENCES Specializari(Id) ON DELETE NO ACTION,
    
    CONSTRAINT UQ_PersonalMedical_NumarLicenta UNIQUE (NumarLicenta),
    CONSTRAINT UQ_PersonalMedical_Email UNIQUE (Email)
)

PRINT 'PersonalMedical table created with Foreign Keys'

-- Create Indexes
CREATE INDEX IX_PersonalMedical_Nume ON PersonalMedical(Nume)
CREATE INDEX IX_PersonalMedical_EsteActiv ON PersonalMedical(EsteActiv)
CREATE INDEX IX_PersonalMedical_SpecializareID ON PersonalMedical(SpecializareID)
CREATE INDEX IX_PersonalMedical_CategorieID ON PersonalMedical(CategorieID)

PRINT 'Indexes created'
PRINT ''

-- =============================================
-- STEP 3: CREATE STORED PROCEDURES
-- =============================================
PRINT 'STEP 3: Creating Stored Procedures...'
PRINT '----------------------------------------------'

-- SP 1: GetAll with Pagination
GO
CREATE PROCEDURE sp_PersonalMedical_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SearchText NVARCHAR(200) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = NULL,
    @SortColumn NVARCHAR(50) = 'Nume',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Build dynamic WHERE clause
    DECLARE @WhereClause NVARCHAR(MAX) = ' WHERE 1=1 ';
    
    IF @SearchText IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND (pm.Nume LIKE ''%' + @SearchText + '%'' OR pm.Prenume LIKE ''%' + @SearchText + '%'' OR pm.Specializare LIKE ''%' + @SearchText + '%'') ';
    
    IF @Departament IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND pm.Departament = ''' + @Departament + ''' ';
    
    IF @Pozitie IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND pm.Pozitie = ''' + @Pozitie + ''' ';
    
    IF @EsteActiv IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND pm.EsteActiv = ' + CAST(@EsteActiv AS NVARCHAR(1)) + ' ';
    
    -- Build ORDER BY
    DECLARE @OrderBy NVARCHAR(100) = ' ORDER BY ' + @SortColumn + ' ' + @SortDirection;
    
    -- Data Query
    DECLARE @SQL NVARCHAR(MAX) = '
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
        pm.DataCreare
    FROM PersonalMedical pm '
    + @WhereClause 
    + @OrderBy 
    + ' OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + ' ROWS ONLY;';
    
    EXEC sp_executesql @SQL;
    
    -- Count Query
    DECLARE @CountSQL NVARCHAR(MAX) = '
    SELECT COUNT(*) AS TotalCount 
    FROM PersonalMedical pm '
    + @WhereClause;
    
    EXEC sp_executesql @CountSQL;
END;
GO

PRINT 'Created: sp_PersonalMedical_GetAll'

-- SP 2: GetById with JOINs
GO
CREATE PROCEDURE sp_PersonalMedical_GetById
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
        -- JOINs
        d1.DenumireDepartament AS CategorieName,
        s1.Denumire AS SpecializareName,
        s2.Denumire AS SubspecializareName
    FROM PersonalMedical pm
    LEFT JOIN Departamente d1 ON pm.CategorieID = d1.IdDepartament
    LEFT JOIN Specializari s1 ON pm.SpecializareID = s1.Id
    LEFT JOIN Specializari s2 ON pm.SubspecializareID = s2.Id
    WHERE pm.PersonalID = @PersonalID;
END;
GO

PRINT 'Created: sp_PersonalMedical_GetById'

-- SP 3: Create
GO
CREATE PROCEDURE sp_PersonalMedical_Create
    @PersonalID UNIQUEIDENTIFIER OUTPUT,
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
    
    SET @PersonalID = NEWID();
    
    INSERT INTO PersonalMedical (
        PersonalID, Nume, Prenume, Specializare, NumarLicenta,
        Telefon, Email, Departament, Pozitie, EsteActiv,
        CategorieID, SpecializareID, SubspecializareID, DataCreare
    ) VALUES (
        @PersonalID, @Nume, @Prenume, @Specializare, @NumarLicenta,
        @Telefon, @Email, @Departament, @Pozitie, @EsteActiv,
        @CategorieID, @SpecializareID, @SubspecializareID, GETDATE()
    );
    
    -- Return created record
    EXEC sp_PersonalMedical_GetById @PersonalID;
END;
GO

PRINT 'Created: sp_PersonalMedical_Create'

-- SP 4: Update
GO
CREATE PROCEDURE sp_PersonalMedical_Update
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
    
    -- Return updated record
    EXEC sp_PersonalMedical_GetById @PersonalID;
END;
GO

PRINT 'Created: sp_PersonalMedical_Update'

-- SP 5: Delete (Soft Delete)
GO
CREATE PROCEDURE sp_PersonalMedical_Delete
    @PersonalID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE PersonalMedical 
    SET EsteActiv = 0 
    WHERE PersonalID = @PersonalID;
    
    SELECT 1 AS Success;
END;
GO

PRINT 'Created: sp_PersonalMedical_Delete'

-- SP 6: CheckUnique
GO
CREATE PROCEDURE sp_PersonalMedical_CheckUnique
    @PersonalID UNIQUEIDENTIFIER = NULL,
    @Email NVARCHAR(100) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @EmailExists BIT = 0;
    DECLARE @LicentaExists BIT = 0;
    
    IF @Email IS NOT NULL
    BEGIN
        IF EXISTS (SELECT 1 FROM PersonalMedical WHERE Email = @Email AND (@PersonalID IS NULL OR PersonalID <> @PersonalID))
            SET @EmailExists = 1;
    END
    
    IF @NumarLicenta IS NOT NULL
    BEGIN
        IF EXISTS (SELECT 1 FROM PersonalMedical WHERE NumarLicenta = @NumarLicenta AND (@PersonalID IS NULL OR PersonalID <> @PersonalID))
            SET @LicentaExists = 1;
    END
    
    SELECT 
        @EmailExists AS EmailExists,
        @LicentaExists AS NumarLicentaExists;
END;
GO

PRINT 'Created: sp_PersonalMedical_CheckUnique'
PRINT ''

-- =============================================
-- STEP 4: VERIFICATION
-- =============================================
PRINT 'STEP 4: Verification...'
PRINT '----------------------------------------------'

-- Check table
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PersonalMedical')
    PRINT '[OK] PersonalMedical table exists'
ELSE
    PRINT '[ERROR] PersonalMedical table NOT created'

-- Check Foreign Keys
SELECT 
    fk.name AS ForeignKey,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys AS fk
WHERE OBJECT_NAME(fk.parent_object_id) = 'PersonalMedical'

-- Check SPs
DECLARE @SPCount INT = (
    SELECT COUNT(*) 
    FROM sys.objects 
    WHERE type = 'P' 
    AND name LIKE 'sp_PersonalMedical_%'
)

PRINT '[OK] Created ' + CAST(@SPCount AS VARCHAR(10)) + ' stored procedures'

PRINT ''
PRINT '=============================================='
PRINT 'RECREATION COMPLETE!'
PRINT '=============================================='
PRINT ''
PRINT 'Summary:'
PRINT '  - Table: PersonalMedical with 4 Foreign Keys'
PRINT '  - Stored Procedures: 6'
PRINT '  - Indexes: 4'
PRINT ''
PRINT 'Foreign Keys:'
PRINT '  - PersonalID -> Personal(Id_Personal)'
PRINT '  - CategorieID -> Departamente(IdDepartament)'
PRINT '  - SpecializareID -> Specializari(Id)'
PRINT '  - SubspecializareID -> Specializari(Id)'
PRINT ''
PRINT 'Ready to use! Restart your Blazor application.'
PRINT '=============================================='
