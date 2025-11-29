-- ========================================
-- Tabel: Judet
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:45
-- Coloane: 7
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Judet', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Judet
    PRINT 'Tabel Judet sters.'
END
GO

-- Create table
CREATE TABLE dbo.Judet (    [IdJudet] INT IDENTITY(1,1) NOT NULL,
    [JudetGuid] UNIQUEIDENTIFIER  NOT NULL,
    [CodJudet] NVARCHAR(10)  NOT NULL,
    [Nume] NVARCHAR(100)  NOT NULL,
    [Siruta] INT  NULL,
    [CodAuto] NVARCHAR(5)  NULL,
    [Ordine] INT  NULL,
    ,CONSTRAINT [PK_Judet] PRIMARY KEY ([IdJudet])
)
GO
PRINT 'Tabel Judet creat cu succes cu 7 coloane.'
