-- ========================================
-- Tabel: RezultateTeste
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:43
-- Coloane: 11
-- Primary Keys: 1
-- Foreign Keys: 3
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.RezultateTeste', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.RezultateTeste
    PRINT 'Tabel RezultateTeste sters.'
END
GO

-- Create table
CREATE TABLE dbo.RezultateTeste (    [RezultatID] UNIQUEIDENTIFIER  NOT NULL,
    [ComandaID] UNIQUEIDENTIFIER  NOT NULL,
    [Rezultat] NVARCHAR(500)  NULL,
    [ValoareNumerica] DECIMAL(10,3)  NULL,
    [IntervalReferinta] NVARCHAR(100)  NULL,
    [Status] NVARCHAR(50)  NULL,
    [DataRezultat] DATETIME2  NULL,
    [EfectuatDe] UNIQUEIDENTIFIER  NULL,
    [RevizuitDe] UNIQUEIDENTIFIER  NULL,
    [DataRevizuire] DATETIME2  NULL,
    [Observatii] NVARCHAR(1000)  NULL,
    ,CONSTRAINT [PK_RezultateTeste] PRIMARY KEY ([RezultatID])
)
GO

-- Foreign Keys
ALTER TABLE dbo.RezultateTeste
ADD CONSTRAINT [FK__Rezultate__Coman__64CCF2AE] FOREIGN KEY ([ComandaID]) 
    REFERENCES dbo.[ComenziTeste] ([ComandaID])
GO
ALTER TABLE dbo.RezultateTeste
ADD CONSTRAINT [FK__Rezultate__Efect__65C116E7] FOREIGN KEY ([EfectuatDe]) 
    REFERENCES dbo.[PersonalMedical] ([PersonalID])
GO
ALTER TABLE dbo.RezultateTeste
ADD CONSTRAINT [FK__Rezultate__Reviz__66B53B20] FOREIGN KEY ([RevizuitDe]) 
    REFERENCES dbo.[PersonalMedical] ([PersonalID])
GO
PRINT 'Tabel RezultateTeste creat cu succes cu 11 coloane.'
