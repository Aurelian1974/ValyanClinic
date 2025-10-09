-- ========================================
-- Tabel: DepartamenteIerarhie
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:41
-- Coloane: 3
-- Primary Keys: 2
-- Foreign Keys: 2
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.DepartamenteIerarhie', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.DepartamenteIerarhie
    PRINT 'Tabel DepartamenteIerarhie sters.'
END
GO

-- Create table
CREATE TABLE dbo.DepartamenteIerarhie (    [AncestorID] UNIQUEIDENTIFIER  NOT NULL,
    [DescendantID] UNIQUEIDENTIFIER  NOT NULL,
    [Nivel] INT  NOT NULL,
    ,CONSTRAINT [PK_DepartamenteIerarhie] PRIMARY KEY ([AncestorID], [DescendantID])
)
GO

-- Foreign Keys
ALTER TABLE dbo.DepartamenteIerarhie
ADD CONSTRAINT [FK_DepartamenteIerarhie_Ancestor] FOREIGN KEY ([AncestorID]) 
    REFERENCES dbo.[Departamente] ([DepartamentID])
GO
ALTER TABLE dbo.DepartamenteIerarhie
ADD CONSTRAINT [FK_DepartamenteIerarhie_Descendant] FOREIGN KEY ([DescendantID]) 
    REFERENCES dbo.[Departamente] ([DepartamentID])
GO
PRINT 'Tabel DepartamenteIerarhie creat cu succes cu 3 coloane.'
