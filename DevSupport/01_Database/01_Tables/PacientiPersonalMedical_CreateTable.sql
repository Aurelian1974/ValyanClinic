-- ========================================
-- Script: Creare Tabel PacientiPersonalMedical
-- Database: ValyanMed
-- Descriere: Tabel pentru rela?iaMany-to-Many dintre Pacienti ?i PersonalMedical
-- ========================================

USE [ValyanMed]
GO

-- ============================================================================
-- STEP 1: Drop table if exists (pentru re-creare în caz de probleme)
-- ============================================================================
IF OBJECT_ID('dbo.PacientiPersonalMedical', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.PacientiPersonalMedical
    PRINT '? Tabel PacientiPersonalMedical existent ?ters.'
END
GO

-- ============================================================================
-- STEP 2: Creare Tabel PacientiPersonalMedical
-- ============================================================================
CREATE TABLE dbo.PacientiPersonalMedical (
    -- Primary Key
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Keys
[PacientID] UNIQUEIDENTIFIER NOT NULL,
    [PersonalMedicalID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Rela?ie Details
    [TipRelatie] NVARCHAR(100) NULL,  -- Ex: "Medic Primar", "Medic Consultant", etc.
    [DataAsocierii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [DataDezactivarii] DATETIME2(7) NULL,
    
    -- Status
    [EsteActiv] BIT NOT NULL DEFAULT 1,
    
 -- Noti?e
    [Observatii] NVARCHAR(MAX) NULL,
    [Motiv] NVARCHAR(500) NULL,  -- Motiv asociere/dezactivare
    
    -- Audit Fields
    [Data_Crearii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Data_Ultimei_Modificari] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Creat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
 [Modificat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    
    -- Constraints
    CONSTRAINT [PK_PacientiPersonalMedical] PRIMARY KEY ([Id]),
    
    -- Foreign Key Constraints (deocamdat? f?r? FK pentru a nu crea dependen?e)
    -- CONSTRAINT [FK_PacientiPersonalMedical_Pacient] FOREIGN KEY ([PacientID]) REFERENCES [dbo].[Pacienti]([Id]),
    -- CONSTRAINT [FK_PacientiPersonalMedical_PersonalMedical] FOREIGN KEY ([PersonalMedicalID]) REFERENCES [dbo].[PersonalMedical]([PersonalID]),
    
    -- Unique Constraint (un pacient poate avea acela?i doctor doar o dat? activ)
    CONSTRAINT [UK_PacientiPersonalMedical_Activ] UNIQUE ([PacientID], [PersonalMedicalID], [EsteActiv])
)
GO

PRINT '? Tabel PacientiPersonalMedical creat cu succes.';
GO

-- ============================================================================
-- STEP 3: Creare Indexuri pentru performan??
-- ============================================================================
CREATE NONCLUSTERED INDEX [IX_PacientiPersonalMedical_PacientID] 
ON dbo.PacientiPersonalMedical ([PacientID] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_PacientiPersonalMedical_PersonalMedicalID] 
ON dbo.PacientiPersonalMedical ([PersonalMedicalID] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_PacientiPersonalMedical_EsteActiv] 
ON dbo.PacientiPersonalMedical ([EsteActiv] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_PacientiPersonalMedical_DataAsocierii] 
ON dbo.PacientiPersonalMedical ([DataAsocierii] DESC)
GO

PRINT '? Indexuri create cu succes.';
GO

-- ============================================================================
-- STEP 4: Trigger pentru actualizarea automat? a datei de modificare
-- ============================================================================
CREATE TRIGGER [TR_PacientiPersonalMedical_UpdateTimestamp]
ON dbo.PacientiPersonalMedical
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.PacientiPersonalMedical
    SET [Data_Ultimei_Modificari] = GETDATE(),
        [Modificat_De] = SYSTEM_USER
    FROM dbo.PacientiPersonalMedical ppm
 INNER JOIN inserted i ON ppm.[Id] = i.[Id]
END
GO

PRINT '? Trigger creat cu succes.';
GO

-- ============================================================================
-- STEP 5: Extended Properties (Documenta?ie)
-- ============================================================================
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Tabel pentru rela?ia Many-to-Many dintre Pacienti ?i PersonalMedical (doctori). Stocheaz? asocierile ?i istoric.', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'PacientiPersonalMedical'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Identificator unic pentru rela?ie', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'PacientiPersonalMedical',
    @level2type = N'COLUMN', @level2name = N'Id'
GO

EXEC sp_addextendedproperty 
  @name = N'MS_Description', 
    @value = N'ID-ul pacientului (FK c?tre Pacienti)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
  @level1type = N'TABLE', @level1name = N'PacientiPersonalMedical',
    @level2type = N'COLUMN', @level2name = N'PacientID'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'ID-ul doctorului (FK c?tre PersonalMedical)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'PacientiPersonalMedical',
    @level2type = N'COLUMN', @level2name = N'PersonalMedicalID'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Tipul rela?iei: Medic Primar, Medic Consultant, etc.', 
  @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'PacientiPersonalMedical',
    @level2type = N'COLUMN', @level2name = N'TipRelatie'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Data când a fost creat? asocierea dintre pacient ?i doctor', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'PacientiPersonalMedical',
  @level2type = N'COLUMN', @level2name = N'DataAsocierii'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Data dezactiv?rii rela?iei (NULL = înc? activ?)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'PacientiPersonalMedical',
    @level2type = N'COLUMN', @level2name = N'DataDezactivarii'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Indicator dac? rela?ia este activ? (1) sau inactiv? (0)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'PacientiPersonalMedical',
    @level2type = N'COLUMN', @level2name = N'EsteActiv'
GO

PRINT '? Extended properties ad?ugate cu succes.';
GO

-- ============================================================================
-- STEP 6: Verificare final?
-- ============================================================================
PRINT '';
PRINT '========================================';
PRINT 'VERIFICARE FINAL?';
PRINT '========================================';

DECLARE @Verificari TABLE (Test VARCHAR(100), Status VARCHAR(10));

-- Verificare tabel
INSERT INTO @Verificari
SELECT 'Tabel PacientiPersonalMedical', 
       CASE WHEN OBJECT_ID('dbo.PacientiPersonalMedical', 'U') IS NOT NULL THEN '? OK' ELSE '? EROARE' END;

-- Verificare indexuri
INSERT INTO @Verificari
SELECT 'Indexuri', 
       CASE WHEN (SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.PacientiPersonalMedical') AND type > 0) >= 4 THEN '? OK' ELSE '? EROARE' END;

-- Verificare trigger
INSERT INTO @Verificari
SELECT 'Trigger UpdateTimestamp', 
       CASE WHEN EXISTS (SELECT 1 FROM sys.triggers WHERE parent_id = OBJECT_ID('dbo.PacientiPersonalMedical')) THEN '? OK' ELSE '? EROARE' END;

-- Verificare constraints
INSERT INTO @Verificari
SELECT 'Constraints', 
       CASE WHEN (SELECT COUNT(*) FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('dbo.PacientiPersonalMedical')) >= 1 THEN '? OK' ELSE '? EROARE' END;

-- Afi?are rezultate
SELECT * FROM @Verificari;

PRINT '';
IF NOT EXISTS (SELECT 1 FROM @Verificari WHERE Status LIKE '%EROARE%')
BEGIN
    PRINT '??? INSTALARE COMPLET? ?I FUNC?IONAL? ???';
END
ELSE
BEGIN
    PRINT '? INSTALARE CU ERORI - Verifica?i mai sus ?';
END
PRINT '========================================';
PRINT '';

-- ============================================================================
-- STEP 7: Info despre structura tabelului
-- ============================================================================
PRINT 'Structura tabelului PacientiPersonalMedical:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PacientiPersonalMedical'
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT '? Script finalizat cu succes!';
PRINT '?? Urm?torul pas: Rula?i scriptul sp_PacientiPersonalMedical_ActivateRelatie.sql';
GO
