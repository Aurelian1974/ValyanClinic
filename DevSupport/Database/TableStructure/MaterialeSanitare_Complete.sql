-- ========================================
-- Tabel: MaterialeSanitare
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:42
-- Coloane: 10
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 2
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.MaterialeSanitare', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.MaterialeSanitare
    PRINT 'Tabel MaterialeSanitare sters.'
END
GO

-- Create table
CREATE TABLE dbo.MaterialeSanitare (    [Id] INT IDENTITY(1,1) NOT NULL,
    [Guid] UNIQUEIDENTIFIER  NULL,
    [Denumire] NVARCHAR(255)  NOT NULL,
    [Categorie] NVARCHAR(100)  NULL,
    [Specificatii] NVARCHAR(MAX)  NULL,
    [UnitateaMasura] NVARCHAR(50)  NULL,
    [Sterile] BIT  NULL,
    [UniUzinta] BIT  NULL,
    [DataCreare] DATETIME2  NULL,
    [DataModificare] DATETIME2  NULL,
    ,CONSTRAINT [PK_MaterialeSanitare] PRIMARY KEY ([Id])
)
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IX_MaterialeSanitare_Guid] 
ON dbo.MaterialeSanitare (Guid ASC)
GO
CREATE NONCLUSTERED INDEX [IX_MaterialeSanitare_Categorie] 
ON dbo.MaterialeSanitare (Categorie ASC)
GO
PRINT 'Tabel MaterialeSanitare creat cu succes cu 10 coloane.'
