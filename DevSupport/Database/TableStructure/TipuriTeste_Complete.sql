-- ========================================
-- Tabel: TipuriTeste
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:43
-- Coloane: 7
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.TipuriTeste', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.TipuriTeste
    PRINT 'Tabel TipuriTeste sters.'
END
GO

-- Create table
CREATE TABLE dbo.TipuriTeste (    [TipTestID] UNIQUEIDENTIFIER  NOT NULL,
    [NumeTest] NVARCHAR(200)  NOT NULL,
    [Categorie] NVARCHAR(100)  NULL,
    [Departament] NVARCHAR(100)  NULL,
    [IntervalNormal] NVARCHAR(200)  NULL,
    [UnitateaMasura] NVARCHAR(50)  NULL,
    [EsteActiv] BIT  NULL,
    ,CONSTRAINT [PK_TipuriTeste] PRIMARY KEY ([TipTestID])
)
GO
PRINT 'Tabel TipuriTeste creat cu succes cu 7 coloane.'
