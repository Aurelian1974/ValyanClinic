-- ========================================
-- Tabel: Programari
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:46
-- Coloane: 10
-- Primary Keys: 1
-- Foreign Keys: 3
-- Indexes: 2
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Programari', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Programari
    PRINT 'Tabel Programari sters.'
END
GO

-- Create table
CREATE TABLE dbo.Programari (    [ProgramareID] UNIQUEIDENTIFIER  NOT NULL,
    [PacientID] UNIQUEIDENTIFIER  NOT NULL,
    [DoctorID] UNIQUEIDENTIFIER  NOT NULL,
    [DataProgramare] DATETIME2  NOT NULL,
    [TipProgramare] NVARCHAR(100)  NULL,
    [Status] NVARCHAR(50)  NULL,
    [Observatii] NVARCHAR(1000)  NULL,
    [DataCreare] DATETIME2  NULL,
    [CreatDe] UNIQUEIDENTIFIER  NULL,
    [DataUltimeiModificari] DATETIME2  NULL,
    ,CONSTRAINT [PK_Programari] PRIMARY KEY ([ProgramareID])
)
GO

-- Foreign Keys
ALTER TABLE dbo.Programari
ADD CONSTRAINT [FK__Programar__Pacie__370627FE] FOREIGN KEY ([PacientID]) 
    REFERENCES dbo.[Pacienti] ([PacientID])
GO
ALTER TABLE dbo.Programari
ADD CONSTRAINT [FK__Programar__Docto__37FA4C37] FOREIGN KEY ([DoctorID]) 
    REFERENCES dbo.[PersonalMedical] ([PersonalID])
GO
ALTER TABLE dbo.Programari
ADD CONSTRAINT [FK__Programar__Creat__38EE7070] FOREIGN KEY ([CreatDe]) 
    REFERENCES dbo.[PersonalMedical] ([PersonalID])
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IX_Programari_Data_Doctor] 
ON dbo.Programari (DataProgramare ASC, DoctorID ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Programari_Pacient_Status] 
ON dbo.Programari (PacientID ASC, Status ASC)
GO
PRINT 'Tabel Programari creat cu succes cu 10 coloane.'
