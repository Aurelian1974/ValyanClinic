-- ========================================
-- Tabel: TipLocalitate
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:46
-- Coloane: 3
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.TipLocalitate', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.TipLocalitate
    PRINT 'Tabel TipLocalitate sters.'
END
GO

-- Create table
CREATE TABLE dbo.TipLocalitate (    [IdTipLocalitate] INT IDENTITY(1,1) NOT NULL,
    [CodTipLocalitate] VARCHAR(10)  NULL,
    [DenumireTipLocalitate] VARCHAR(100)  NULL,
    ,CONSTRAINT [PK_TipLocalitate] PRIMARY KEY ([IdTipLocalitate])
)
GO
PRINT 'Tabel TipLocalitate creat cu succes cu 3 coloane.'
