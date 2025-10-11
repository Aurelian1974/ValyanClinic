-- ========================================
-- Tabel: TriajPacienti
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:44
-- Coloane: 7
-- Primary Keys: 1
-- Foreign Keys: 2
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.TriajPacienti', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.TriajPacienti
    PRINT 'Tabel TriajPacienti sters.'
END
GO

-- Create table
CREATE TABLE dbo.TriajPacienti (    [TriajID] UNIQUEIDENTIFIER  NOT NULL,
    [ProgramareID] UNIQUEIDENTIFIER  NOT NULL,
    [NivelTriaj] INT  NULL,
    [PlangereaPrincipala] NVARCHAR(1000)  NULL,
    [AsistentTriajID] UNIQUEIDENTIFIER  NULL,
    [DataTriaj] DATETIME2  NULL,
    [Observatii] NVARCHAR(1000)  NULL,
    ,CONSTRAINT [PK_TriajPacienti] PRIMARY KEY ([TriajID])
)
GO

-- Foreign Keys
ALTER TABLE dbo.TriajPacienti
ADD CONSTRAINT [FK__TriajPaci__Progr__436BFEE3] FOREIGN KEY ([ProgramareID]) 
    REFERENCES dbo.[Programari] ([ProgramareID])
GO
ALTER TABLE dbo.TriajPacienti
ADD CONSTRAINT [FK__TriajPaci__Asist__4460231C] FOREIGN KEY ([AsistentTriajID]) 
    REFERENCES dbo.[PersonalMedical] ([PersonalID])
GO
PRINT 'Tabel TriajPacienti creat cu succes cu 7 coloane.'
