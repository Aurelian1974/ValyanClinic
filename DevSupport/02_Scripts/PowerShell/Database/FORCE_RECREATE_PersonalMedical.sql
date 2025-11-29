-- =============================================
-- FORCE RECREATE: PersonalMedical Complete
-- PersonalMedical is INDEPENDENT (no FK to Personal)
-- Database: ValyanMed
-- =============================================

USE [ValyanMed]
GO

PRINT '=============================================='
PRINT 'FORCE RECREATE: PersonalMedical'
PRINT 'Independent table (NO FK to Personal)'
PRINT '=============================================='
PRINT ''

-- =============================================
-- STEP 1: DROP ALL FOREIGN KEYS FIRST
-- =============================================
PRINT 'STEP 1: Dropping Foreign Keys...'

DECLARE @SQL NVARCHAR(MAX) = '';

-- Drop FKs FROM other tables that reference PersonalMedical
SELECT @SQL = @SQL + 'ALTER TABLE ' + OBJECT_SCHEMA_NAME(parent_object_id) + '.' + OBJECT_NAME(parent_object_id) + 
              ' DROP CONSTRAINT ' + name + ';' + CHAR(13)
FROM sys.foreign_keys
WHERE referenced_object_id = OBJECT_ID('PersonalMedical');

-- Drop FKs FROM PersonalMedical
SELECT @SQL = @SQL + 'ALTER TABLE PersonalMedical DROP CONSTRAINT ' + name + ';' + CHAR(13)
FROM sys.foreign_keys
WHERE parent_object_id = OBJECT_ID('PersonalMedical');

IF LEN(@SQL) > 0
BEGIN
    EXEC sp_executesql @SQL;
    PRINT '[OK] Foreign Keys dropped'
END
ELSE
    PRINT '[INFO] No Foreign Keys to drop'

-- =============================================
-- STEP 2: DROP SPs
-- =============================================
PRINT 'STEP 2: Dropping SPs...'

DROP PROCEDURE IF EXISTS sp_PersonalMedical_GetAll;
DROP PROCEDURE IF EXISTS sp_PersonalMedical_GetById;
DROP PROCEDURE IF EXISTS sp_PersonalMedical_Create;
DROP PROCEDURE IF EXISTS sp_PersonalMedical_Update;
DROP PROCEDURE IF EXISTS sp_PersonalMedical_Delete;
DROP PROCEDURE IF EXISTS sp_PersonalMedical_CheckUnique;

PRINT '[OK] SPs dropped'

-- =============================================
-- STEP 3: DROP TABLE
-- =============================================
PRINT 'STEP 3: Dropping table...'

DROP TABLE IF EXISTS PersonalMedical;

PRINT '[OK] Table dropped'
PRINT ''

-- =============================================
-- STEP 4: CREATE TABLE (INDEPENDENT)
-- =============================================
PRINT 'STEP 4: Creating PersonalMedical table...'

CREATE TABLE PersonalMedical (
    -- Primary Key - NOT FK, just GUID
    PersonalID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    
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
    
    -- Foreign Keys (References to lookup tables ONLY)
    CategorieID UNIQUEIDENTIFIER NULL,
    SpecializareID UNIQUEIDENTIFIER NULL,
    SubspecializareID UNIQUEIDENTIFIER NULL,
    
    -- Audit
    DataCreare DATETIME2 NULL DEFAULT GETDATE()
);

PRINT '[OK] Table created (independent)'

-- =============================================
-- STEP 5: ADD FOREIGN KEYS (ONLY to lookups)
-- =============================================
PRINT 'STEP 5: Adding Foreign Keys (lookup tables only)...'

-- NO FK to Personal - PersonalMedical is independent!

-- FK to Departamente
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Departamente')
BEGIN
    ALTER TABLE PersonalMedical
    ADD CONSTRAINT FK_PersonalMedical_Categorie 
    FOREIGN KEY (CategorieID) REFERENCES Departamente(IdDepartament) 
    ON DELETE SET NULL;
    PRINT '[OK] FK to Departamente added'
END
ELSE
    PRINT '[WARN] Departamente table not found - FK skipped'

-- FK to Specializari
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Specializari')
BEGIN
    ALTER TABLE PersonalMedical
    ADD CONSTRAINT FK_PersonalMedical_Specializare 
    FOREIGN KEY (SpecializareID) REFERENCES Specializari(Id) 
    ON DELETE SET NULL;
    
    ALTER TABLE PersonalMedical
    ADD CONSTRAINT FK_PersonalMedical_Subspecializare 
    FOREIGN KEY (SubspecializareID) REFERENCES Specializari(Id) 
    ON DELETE NO ACTION;
    
    PRINT '[OK] FK to Specializari added'
END
ELSE
    PRINT '[WARN] Specializari table not found - FK skipped'

-- =============================================
-- STEP 6: ADD CONSTRAINTS & INDEXES
-- =============================================
PRINT 'STEP 6: Adding Constraints & Indexes...'

ALTER TABLE PersonalMedical
ADD CONSTRAINT UQ_PersonalMedical_NumarLicenta UNIQUE (NumarLicenta);

ALTER TABLE PersonalMedical
ADD CONSTRAINT UQ_PersonalMedical_Email UNIQUE (Email);

CREATE INDEX IX_PersonalMedical_Nume ON PersonalMedical(Nume);
CREATE INDEX IX_PersonalMedical_EsteActiv ON PersonalMedical(EsteActiv);
CREATE INDEX IX_PersonalMedical_SpecializareID ON PersonalMedical(SpecializareID);
CREATE INDEX IX_PersonalMedical_CategorieID ON PersonalMedical(CategorieID);

PRINT '[OK] Constraints & Indexes added'
PRINT ''

-- =============================================
-- STEP 7: CREATE STORED PROCEDURES
-- =============================================
PRINT 'STEP 7: Creating Stored Procedures...'
GO

-- SP: GetAll
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
    
    -- Main query
    SELECT 
        PersonalID, Nume, Prenume, Specializare, NumarLicenta,
        Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare
    FROM PersonalMedical
    WHERE 
        (@SearchText IS NULL OR Nume LIKE '%' + @SearchText + '%' OR Prenume LIKE '%' + @SearchText + '%')
        AND (@Departament IS NULL OR Departament = @Departament)
        AND (@Pozitie IS NULL OR Pozitie = @Pozitie)
        AND (@EsteActiv IS NULL OR EsteActiv = @EsteActiv)
    ORDER BY 
        CASE WHEN @SortColumn = 'Nume' AND @SortDirection = 'ASC' THEN Nume END ASC,
        CASE WHEN @SortColumn = 'Nume' AND @SortDirection = 'DESC' THEN Nume END DESC,
        CASE WHEN @SortColumn = 'Prenume' AND @SortDirection = 'ASC' THEN Prenume END ASC,
        CASE WHEN @SortColumn = 'Prenume' AND @SortDirection = 'DESC' THEN Prenume END DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
    
    -- Count
    SELECT COUNT(*) AS TotalCount
    FROM PersonalMedical
    WHERE 
        (@SearchText IS NULL OR Nume LIKE '%' + @SearchText + '%' OR Prenume LIKE '%' + @SearchText + '%')
        AND (@Departament IS NULL OR Departament = @Departament)
        AND (@Pozitie IS NULL OR Pozitie = @Pozitie)
        AND (@EsteActiv IS NULL OR EsteActiv = @EsteActiv);
END;
GO

PRINT '[OK] sp_PersonalMedical_GetAll'

-- SP: GetById
GO
CREATE PROCEDURE sp_PersonalMedical_GetById
    @PersonalID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pm.PersonalID, pm.Nume, pm.Prenume, pm.Specializare, pm.NumarLicenta,
        pm.Telefon, pm.Email, pm.Departament, pm.Pozitie, pm.EsteActiv, pm.DataCreare,
        pm.CategorieID, pm.SpecializareID, pm.SubspecializareID,
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

PRINT '[OK] sp_PersonalMedical_GetById'

-- SP: Create
GO
CREATE PROCEDURE sp_PersonalMedical_Create
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
    DECLARE @PersonalID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO PersonalMedical (
        PersonalID, Nume, Prenume, Specializare, NumarLicenta,
        Telefon, Email, Departament, Pozitie, EsteActiv,
        CategorieID, SpecializareID, SubspecializareID, DataCreare
    ) VALUES (
        @PersonalID, @Nume, @Prenume, @Specializare, @NumarLicenta,
        @Telefon, @Email, @Departament, @Pozitie, @EsteActiv,
        @CategorieID, @SpecializareID, @SubspecializareID, GETDATE()
    );
    
    EXEC sp_PersonalMedical_GetById @PersonalID;
END;
GO

PRINT '[OK] sp_PersonalMedical_Create'

-- SP: Update
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
        Nume = @Nume, Prenume = @Prenume, Specializare = @Specializare,
        NumarLicenta = @NumarLicenta, Telefon = @Telefon, Email = @Email,
        Departament = @Departament, Pozitie = @Pozitie, EsteActiv = @EsteActiv,
        CategorieID = @CategorieID, SpecializareID = @SpecializareID,
        SubspecializareID = @SubspecializareID
    WHERE PersonalID = @PersonalID;
    
    EXEC sp_PersonalMedical_GetById @PersonalID;
END;
GO

PRINT '[OK] sp_PersonalMedical_Update'

-- SP: Delete
GO
CREATE PROCEDURE sp_PersonalMedical_Delete
    @PersonalID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PersonalMedical SET EsteActiv = 0 WHERE PersonalID = @PersonalID;
    SELECT 1 AS Success;
END;
GO

PRINT '[OK] sp_PersonalMedical_Delete'

-- SP: CheckUnique
GO
CREATE PROCEDURE sp_PersonalMedical_CheckUnique
    @PersonalID UNIQUEIDENTIFIER = NULL,
    @Email NVARCHAR(100) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CASE WHEN EXISTS (SELECT 1 FROM PersonalMedical WHERE Email = @Email AND (@PersonalID IS NULL OR PersonalID <> @PersonalID)) THEN 1 ELSE 0 END AS EmailExists,
        CASE WHEN EXISTS (SELECT 1 FROM PersonalMedical WHERE NumarLicenta = @NumarLicenta AND (@PersonalID IS NULL OR PersonalID <> @PersonalID)) THEN 1 ELSE 0 END AS NumarLicentaExists;
END;
GO

PRINT '[OK] sp_PersonalMedical_CheckUnique'
PRINT ''

-- =============================================
-- FINAL VERIFICATION
-- =============================================
PRINT '=============================================='
PRINT 'VERIFICATION'
PRINT '=============================================='

SELECT 'PersonalMedical' AS [Table], COUNT(*) AS [Columns]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PersonalMedical';

SELECT name AS [ForeignKey], OBJECT_NAME(referenced_object_id) AS [ReferencedTable]
FROM sys.foreign_keys
WHERE parent_object_id = OBJECT_ID('PersonalMedical');

SELECT name AS [StoredProcedure]
FROM sys.objects
WHERE type = 'P' AND name LIKE 'sp_PersonalMedical_%';

PRINT ''
PRINT '=============================================='
PRINT 'RECREATION COMPLETE!'
PRINT '=============================================='
PRINT 'Table: PersonalMedical (INDEPENDENT)'
PRINT 'Foreign Keys: 3 (Departamente, 2x Specializari)'
PRINT 'NO FK to Personal - PersonalMedical is standalone'
PRINT 'Stored Procedures: 6'
PRINT 'Indexes: 4'
PRINT ''
PRINT 'READY TO USE! Restart Blazor app.'
PRINT '=============================================='
