-- ========================================
-- Tabel: Pacienti_PersonalMedical (Junction Table)
-- Database: ValyanMed
-- Descriere: Tabela de legatura Many-to-Many intre Pacienti si PersonalMedical
-- Scop: Un pacient poate avea mai multi doctori; Un doctor poate avea mai multi pacienti
-- Generat: 2025-01-23
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'Creare tabel: Pacienti_PersonalMedical';
PRINT 'Junction Table (Many-to-Many)';
PRINT '========================================';
PRINT '';

-- Drop table if exists
IF OBJECT_ID('dbo.Pacienti_PersonalMedical', 'U') IS NOT NULL
BEGIN
 DROP TABLE dbo.Pacienti_PersonalMedical
    PRINT 'Tabel Pacienti_PersonalMedical sters.'
END
GO

-- Create table
CREATE TABLE dbo.Pacienti_PersonalMedical (
    -- Primary Key
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Keys - Relatia Many-to-Many
    [PacientID] UNIQUEIDENTIFIER NOT NULL,
    [PersonalMedicalID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Informatii contextuale despre relatie
    [TipRelatie] NVARCHAR(50) NULL, -- 'MedicPrimar', 'MedicConsultant', 'Specialist', 'MedicDeGarda'
    [DataAsocierii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [DataDezactivarii] DATETIME2(7) NULL,
  [EsteActiv] BIT NOT NULL DEFAULT 1,
    
    -- Metadata suplimentara
    [Observatii] NVARCHAR(MAX) NULL,
    [Motiv] NVARCHAR(500) NULL, -- Motivul asocierii (ex: "Pacient cu probleme cardiace")
  
    -- Audit
    [Data_Crearii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
  [Data_Ultimei_Modificari] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Creat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    [Modificat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    
    -- Constraints
    CONSTRAINT [PK_Pacienti_PersonalMedical] PRIMARY KEY ([Id]),
    
    -- Foreign Key constraints
    CONSTRAINT [FK_PacientiPersonalMedical_Pacient] 
        FOREIGN KEY ([PacientID]) 
  REFERENCES dbo.Pacienti([Id]) 
     ON DELETE CASCADE,
    
    CONSTRAINT [FK_PacientiPersonalMedical_PersonalMedical] 
        FOREIGN KEY ([PersonalMedicalID]) 
 REFERENCES dbo.PersonalMedical([PersonalID]) 
        ON DELETE CASCADE,
    
    -- Previne duplicate (acelasi pacient cu acelasi doctor)
    CONSTRAINT [UQ_Pacient_PersonalMedical] 
    UNIQUE ([PacientID], [PersonalMedicalID]),
    
    -- Check constraints
    CONSTRAINT [CK_TipRelatie] 
        CHECK ([TipRelatie] IN ('MedicPrimar', 'MedicConsultant', 'Specialist', 'MedicDeGarda', 'MedicFamilie', 'AsistentMedical', NULL))
)
GO

PRINT 'Tabel Pacienti_PersonalMedical creat cu succes.';
PRINT '';

-- ========================================
-- INDECSI PENTRU PERFORMANTA
-- ========================================

PRINT 'Creare indecsi pentru performanta...';

-- Index pentru cautare doctori dupa pacient
CREATE NONCLUSTERED INDEX [IX_Pacienti_PersonalMedical_PacientID] 
ON dbo.Pacienti_PersonalMedical ([PacientID] ASC, [EsteActiv] ASC)
INCLUDE ([PersonalMedicalID], [TipRelatie], [DataAsocierii])
GO

-- Index pentru cautare pacienti dupa doctor
CREATE NONCLUSTERED INDEX [IX_Pacienti_PersonalMedical_PersonalMedicalID] 
ON dbo.Pacienti_PersonalMedical ([PersonalMedicalID] ASC, [EsteActiv] ASC)
INCLUDE ([PacientID], [TipRelatie], [DataAsocierii])
GO

-- Index pentru relatii active
CREATE NONCLUSTERED INDEX [IX_Pacienti_PersonalMedical_EsteActiv] 
ON dbo.Pacienti_PersonalMedical ([EsteActiv] ASC)
WHERE [EsteActiv] = 1
GO

-- Index pentru tipul relatiei
CREATE NONCLUSTERED INDEX [IX_Pacienti_PersonalMedical_TipRelatie] 
ON dbo.Pacienti_PersonalMedical ([TipRelatie] ASC, [EsteActiv] ASC)
GO

-- Index pentru data asocierii (istoric)
CREATE NONCLUSTERED INDEX [IX_Pacienti_PersonalMedical_DataAsocierii] 
ON dbo.Pacienti_PersonalMedical ([DataAsocierii] DESC)
GO

PRINT 'Indecsi creati cu succes.';
PRINT '';

-- ========================================
-- TRIGGER PENTRU ACTUALIZARE AUTOMATA
-- ========================================

PRINT 'Creare trigger pentru actualizare timestamp...';

CREATE TRIGGER [TR_Pacienti_PersonalMedical_UpdateTimestamp]
ON dbo.Pacienti_PersonalMedical
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Pacienti_PersonalMedical
    SET [Data_Ultimei_Modificari] = GETDATE(),
      [Modificat_De] = SYSTEM_USER
    FROM dbo.Pacienti_PersonalMedical ppm
    INNER JOIN inserted i ON ppm.[Id] = i.[Id]
END
GO

PRINT 'Trigger creat cu succes.';
PRINT '';

-- ========================================
-- DOCUMENTATIE EXTINSA
-- ========================================

PRINT 'Adaugare documentatie extinsa...';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Tabela de legatura Many-to-Many intre Pacienti si PersonalMedical. Un pacient poate avea mai multi doctori si un doctor poate avea mai multi pacienti.', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti_PersonalMedical'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Identificator unic al relatiei', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti_PersonalMedical',
    @level2type = N'COLUMN', @level2name = N'Id'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Referinta catre pacient (FK catre Pacienti.Id)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti_PersonalMedical',
    @level2type = N'COLUMN', @level2name = N'PacientID'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Referinta catre personal medical (FK catre PersonalMedical.PersonalID)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti_PersonalMedical',
    @level2type = N'COLUMN', @level2name = N'PersonalMedicalID'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Tipul relatiei: MedicPrimar, MedicConsultant, Specialist, MedicDeGarda, MedicFamilie, AsistentMedical', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti_PersonalMedical',
    @level2type = N'COLUMN', @level2name = N'TipRelatie'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Data la care a fost stabilita relatia intre pacient si personalul medical', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti_PersonalMedical',
    @level2type = N'COLUMN', @level2name = N'DataAsocierii'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Data la care relatia a fost dezactivata (null daca este activa)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti_PersonalMedical',
    @level2type = N'COLUMN', @level2name = N'DataDezactivarii'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Indica daca relatia este activa in prezent', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti_PersonalMedical',
    @level2type = N'COLUMN', @level2name = N'EsteActiv'
GO

PRINT 'Documentatie adaugata cu succes.';
PRINT '';

-- ========================================
-- VERIFICARE FINALA
-- ========================================

PRINT '========================================';
PRINT 'VERIFICARE FINALA';
PRINT '========================================';

-- Verificare tabel
IF OBJECT_ID('dbo.Pacienti_PersonalMedical', 'U') IS NOT NULL
    PRINT '? Tabel Pacienti_PersonalMedical: EXISTA'
ELSE
    PRINT '? Tabel Pacienti_PersonalMedical: NU EXISTA'

-- Verificare Foreign Keys
DECLARE @FKCount INT;
SELECT @FKCount = COUNT(*)
FROM sys.foreign_keys
WHERE parent_object_id = OBJECT_ID('Pacienti_PersonalMedical');
PRINT '? Foreign Keys: ' + CAST(@FKCount AS NVARCHAR(10)) + ' (asteptat: 2)';

-- Verificare Indexes
DECLARE @IndexCount INT;
SELECT @IndexCount = COUNT(*)
FROM sys.indexes
WHERE object_id = OBJECT_ID('Pacienti_PersonalMedical')
AND name IS NOT NULL;
PRINT '? Indexes: ' + CAST(@IndexCount AS NVARCHAR(10)) + ' (asteptat: 6 - 1 PK + 5 nonclustered)';

-- Verificare Constraints
DECLARE @ConstraintCount INT;
SELECT @ConstraintCount = COUNT(*)
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('Pacienti_PersonalMedical');
PRINT '? Check Constraints: ' + CAST(@ConstraintCount AS NVARCHAR(10)) + ' (asteptat: 1)';

-- Verificare Trigger
IF OBJECT_ID('TR_Pacienti_PersonalMedical_UpdateTimestamp', 'TR') IS NOT NULL
    PRINT '? Trigger UpdateTimestamp: EXISTA'
ELSE
    PRINT '? Trigger UpdateTimestamp: NU EXISTA'

PRINT '';
PRINT '========================================';
PRINT 'TABEL PACIENTI_PERSONALMEDICAL GATA!';
PRINT '========================================';
PRINT '';
PRINT 'PASUL URMATOR:';
PRINT '1. Ruleaza scriptul pentru Stored Procedures';
PRINT '2. Testeaza cu date mock';
PRINT '3. Integreaza in aplicatia C#';
PRINT '';

GO
