-- ========================================
-- Tabel: SemneVitale
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:46
-- Coloane: 13
-- Primary Keys: 1
-- Foreign Keys: 3
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.SemneVitale', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.SemneVitale
    PRINT 'Tabel SemneVitale sters.'
END
GO

-- Create table
CREATE TABLE dbo.SemneVitale (    [SemneVitaleID] UNIQUEIDENTIFIER  NOT NULL,
    [ProgramareID] UNIQUEIDENTIFIER  NOT NULL,
    [TensiuneArterialaMax] INT  NULL,
    [TensiuneArterialaMin] INT  NULL,
    [FrecariaCardiaca] INT  NULL,
    [Temperatura] DECIMAL(4,1)  NULL,
    [Greutate] DECIMAL(5,2)  NULL,
    [Inaltime] INT  NULL,
    [FrecariaRespiratorie] INT  NULL,
    [SaturatieOxigen] DECIMAL(5,2)  NULL,
    [MasuratDe] UNIQUEIDENTIFIER  NULL,
    [DataMasurare] DATETIME2  NULL,
    [PacientID] UNIQUEIDENTIFIER  NOT NULL,
    ,CONSTRAINT [PK_SemneVitale] PRIMARY KEY ([SemneVitaleID])
)
GO

-- Foreign Keys
ALTER TABLE dbo.SemneVitale
ADD CONSTRAINT [FK__SemneVita__Progr__4924D839] FOREIGN KEY ([ProgramareID]) 
    REFERENCES dbo.[Programari] ([ProgramareID])
GO
ALTER TABLE dbo.SemneVitale
ADD CONSTRAINT [FK__SemneVita__Masur__4A18FC72] FOREIGN KEY ([MasuratDe]) 
    REFERENCES dbo.[PersonalMedical] ([PersonalID])
GO
ALTER TABLE dbo.SemneVitale
ADD CONSTRAINT [FK__SemneVita__Pacie__1D114BD1] FOREIGN KEY ([PacientID]) 
    REFERENCES dbo.[Pacienti] ([PacientID])
GO
PRINT 'Tabel SemneVitale creat cu succes cu 13 coloane.'
