-- ========================================
-- Tabel: Departamente
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:44
-- Coloane: 4
-- Primary Keys: 1
-- Foreign Keys: 1
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Departamente', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Departamente
    PRINT 'Tabel Departamente sters.'
END
GO

-- Create table
CREATE TABLE dbo.Departamente (    [IdDepartament] UNIQUEIDENTIFIER  NOT NULL,
    [IdTipDepartament] UNIQUEIDENTIFIER  NULL,
    [DenumireDepartament] VARCHAR(200)  NOT NULL,
    [DescriereDepartament] VARCHAR(500)  NULL,
    ,CONSTRAINT [PK_Departamente] PRIMARY KEY ([IdDepartament])
)
GO

-- Foreign Keys
ALTER TABLE dbo.Departamente
ADD CONSTRAINT [FK_Departamente_TipDepartament] FOREIGN KEY ([IdTipDepartament]) 
    REFERENCES dbo.[TipDepartament] ([IdTipDepartament]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT 'Tabel Departamente creat cu succes cu 4 coloane.'
