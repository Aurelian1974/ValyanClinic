-- ========================================
-- Tabel: ComenziTeste
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:40
-- Coloane: 8
-- Primary Keys: 1
-- Foreign Keys: 3
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.ComenziTeste', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.ComenziTeste
    PRINT 'Tabel ComenziTeste sters.'
END
GO

-- Create table
CREATE TABLE dbo.ComenziTeste (    [ComandaID] UNIQUEIDENTIFIER  NOT NULL,
    [ConsultatieID] UNIQUEIDENTIFIER  NOT NULL,
    [TipTestID] UNIQUEIDENTIFIER  NOT NULL,
    [DataComanda] DATETIME2  NULL,
    [Status] NVARCHAR(50)  NULL,
    [Prioritate] NVARCHAR(50)  NULL,
    [ComantatDe] UNIQUEIDENTIFIER  NOT NULL,
    [Observatii] NVARCHAR(500)  NULL,
    ,CONSTRAINT [PK_ComenziTeste] PRIMARY KEY ([ComandaID])
)
GO

-- Foreign Keys
ALTER TABLE dbo.ComenziTeste
ADD CONSTRAINT [FK__ComenziTe__Consu__5F141958] FOREIGN KEY ([ConsultatieID]) 
    REFERENCES dbo.[Consultatii] ([ConsultatieID])
GO
ALTER TABLE dbo.ComenziTeste
ADD CONSTRAINT [FK__ComenziTe__TipTe__60083D91] FOREIGN KEY ([TipTestID]) 
    REFERENCES dbo.[TipuriTeste] ([TipTestID])
GO
ALTER TABLE dbo.ComenziTeste
ADD CONSTRAINT [FK__ComenziTe__Coman__60FC61CA] FOREIGN KEY ([ComantatDe]) 
    REFERENCES dbo.[PersonalMedical] ([PersonalID])
GO
PRINT 'Tabel ComenziTeste creat cu succes cu 8 coloane.'
