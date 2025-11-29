-- ========================================
-- Tabel: TipDepartament
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:46
-- Coloane: 2
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.TipDepartament', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.TipDepartament
    PRINT 'Tabel TipDepartament sters.'
END
GO

-- Create table
CREATE TABLE dbo.TipDepartament (    [IdTipDepartament] UNIQUEIDENTIFIER  NOT NULL,
    [DenumireTipDepartament] VARCHAR(100)  NULL,
    ,CONSTRAINT [PK_TipDepartament] PRIMARY KEY ([IdTipDepartament])
)
GO
PRINT 'Tabel TipDepartament creat cu succes cu 2 coloane.'
