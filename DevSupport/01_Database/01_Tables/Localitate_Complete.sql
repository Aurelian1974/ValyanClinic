-- ========================================
-- Tabel: Localitate
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:45
-- Coloane: 7
-- Primary Keys: 1
-- Foreign Keys: 2
-- Indexes: 4
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Localitate', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Localitate
    PRINT 'Tabel Localitate sters.'
END
GO

-- Create table
CREATE TABLE dbo.Localitate (    [IdOras] INT IDENTITY(1,1) NOT NULL,
    [LocalitateGuid] UNIQUEIDENTIFIER  NOT NULL,
    [IdJudet] INT  NOT NULL,
    [Nume] NVARCHAR(100)  NOT NULL,
    [Siruta] INT  NOT NULL,
    [IdTipLocalitate] INT  NULL,
    [CodLocalitate] VARCHAR(10)  NOT NULL,
    ,CONSTRAINT [PK_Localitate] PRIMARY KEY ([IdOras])
)
GO

-- Foreign Keys
ALTER TABLE dbo.Localitate
ADD CONSTRAINT [FK__Localitat__IdJud__06CD04F7] FOREIGN KEY ([IdJudet]) 
    REFERENCES dbo.[Judet] ([IdJudet])
GO
ALTER TABLE dbo.Localitate
ADD CONSTRAINT [FK_Localitate_TipLocalitate] FOREIGN KEY ([IdTipLocalitate]) 
    REFERENCES dbo.[TipLocalitate] ([IdTipLocalitate])
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IX_Localitate_IdJudet] 
ON dbo.Localitate (IdJudet ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Localitate_Nume] 
ON dbo.Localitate (Nume ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Oras_Nume_Judet] 
ON dbo.Localitate (IdJudet ASC, Nume ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Localitate_LocalitateGuid] 
ON dbo.Localitate (LocalitateGuid ASC)
GO
PRINT 'Tabel Localitate creat cu succes cu 7 coloane.'
