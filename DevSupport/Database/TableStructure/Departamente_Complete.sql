-- ========================================
-- Tabel: Departamente
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:41
-- Coloane: 3
-- Primary Keys: 1
-- Foreign Keys: 0
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
CREATE TABLE dbo.Departamente (    [DepartamentID] UNIQUEIDENTIFIER  NOT NULL,
    [Nume] NVARCHAR(200)  NOT NULL,
    [Tip] NVARCHAR(50)  NOT NULL,
    ,CONSTRAINT [PK_Departamente] PRIMARY KEY ([DepartamentID])
)
GO
PRINT 'Tabel Departamente creat cu succes cu 3 coloane.'
