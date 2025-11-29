-- ========================================
-- Tabel: Programari
-- Database: ValyanMed
-- Versiune: 2.0 - ACTUALIZAT cu corecții arhitecturale
-- Data: 2025-01-15
-- Modificari:
--   - NEWSEQUENTIALID() pentru PK (performance)
--   - Data si ora separate (DATE + TIME + TIME)
--   - FK către Pacienti.Id (nu PacientID)
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists (include FK constraints)
IF OBJECT_ID('dbo.Programari', 'U') IS NOT NULL
BEGIN
    -- Drop FK constraints first
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Programari_Pacienti')
      ALTER TABLE dbo.Programari DROP CONSTRAINT FK_Programari_Pacienti;
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Programari_Doctor')
        ALTER TABLE dbo.Programari DROP CONSTRAINT FK_Programari_Doctor;
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Programari_CreatDe')
ALTER TABLE dbo.Programari DROP CONSTRAINT FK_Programari_CreatDe;
    
    -- Now drop table
    DROP TABLE dbo.Programari;
    PRINT 'Tabel Programari sters (inclusiv FK constraints).';
END
GO

-- Create table with corrected structure
CREATE TABLE dbo.Programari (
    -- Primary Key cu NEWSEQUENTIALID pentru performance
    [ProgramareID] UNIQUEIDENTIFIER NOT NULL 
      CONSTRAINT PK_Programari PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Keys (CORECTATE!)
    [PacientID] UNIQUEIDENTIFIER NOT NULL,  -- FK către Pacienti.Id (NU PacientID!)
    [DoctorID] UNIQUEIDENTIFIER NOT NULL,   -- FK către PersonalMedical.PersonalID
    
    -- Data și Ora SEPARATE (Opțiunea B - mai flexibil)
    [DataProgramare] DATE NOT NULL,
    [OraInceput] TIME NOT NULL,
    [OraSfarsit] TIME NOT NULL,
  
    -- Detalii programare
    [TipProgramare] NVARCHAR(100) NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Programata',
    [Observatii] NVARCHAR(1000) NULL,
    
    -- Audit fields
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [CreatDe] UNIQUEIDENTIFIER NOT NULL,
    [DataUltimeiModificari] DATETIME2 NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL
)
GO

-- Foreign Key Constraints (CORECTATE!)
ALTER TABLE dbo.Programari
ADD CONSTRAINT FK_Programari_Pacienti FOREIGN KEY (PacientID) 
    REFERENCES dbo.Pacienti(Id)  -- CORECTAT: Id nu PacientID!
GO

ALTER TABLE dbo.Programari
ADD CONSTRAINT FK_Programari_Doctor FOREIGN KEY (DoctorID) 
    REFERENCES dbo.PersonalMedical(PersonalID)
GO

ALTER TABLE dbo.Programari
ADD CONSTRAINT FK_Programari_CreatDe FOREIGN KEY (CreatDe) 
 REFERENCES dbo.PersonalMedical(PersonalID)
GO

-- Check Constraint pentru validare oră
ALTER TABLE dbo.Programari
ADD CONSTRAINT CK_Programari_OraValida CHECK (OraInceput < OraSfarsit)
GO

-- Indexes pentru performance
CREATE NONCLUSTERED INDEX IX_Programari_Data_Doctor 
    ON dbo.Programari (DataProgramare ASC, DoctorID ASC)
GO

CREATE NONCLUSTERED INDEX IX_Programari_Pacient_Status 
    ON dbo.Programari (PacientID ASC, Status ASC)
GO

-- Index suplimentar pentru queries pe data+ora
CREATE NONCLUSTERED INDEX IX_Programari_DataOraInceput
    ON dbo.Programari (DataProgramare ASC, OraInceput ASC)
GO

PRINT '';
PRINT '========================================';
PRINT 'Tabel Programari creat cu succes!';
PRINT '========================================';
PRINT '';
PRINT 'Structura:';
PRINT '  - Primary Key: NEWSEQUENTIALID() pentru performance';
PRINT '  - Data si ora: SEPARATE (DATE + TIME + TIME)';
PRINT '  - Foreign Keys: 3 (Pacienti.Id, Doctor, CreatDe)';
PRINT '  - Indexes: 3 (Data+Doctor, Pacient+Status, Data+Ora)';
PRINT '  - Check Constraint: OraInceput < OraSfarsit';
PRINT '';
PRINT 'Urmatorul pas: Creaza stored procedures (sp_Programari.sql)';
PRINT '========================================';
GO
