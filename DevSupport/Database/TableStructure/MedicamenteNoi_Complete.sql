-- ========================================
-- Tabel: MedicamenteNoi
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:42
-- Coloane: 7
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.MedicamenteNoi', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.MedicamenteNoi
    PRINT 'Tabel MedicamenteNoi sters.'
END
GO

-- Create table
CREATE TABLE dbo.MedicamenteNoi (    [MedicamentID] UNIQUEIDENTIFIER  NOT NULL,
    [NumeMedicament] NVARCHAR(200)  NOT NULL,
    [NumeGeneric] NVARCHAR(200)  NULL,
    [Concentratie] NVARCHAR(50)  NULL,
    [Forma] NVARCHAR(50)  NULL,
    [Producator] NVARCHAR(100)  NULL,
    [EsteActiv] BIT  NULL,
    ,CONSTRAINT [PK_MedicamenteNoi] PRIMARY KEY ([MedicamentID])
)
GO
PRINT 'Tabel MedicamenteNoi creat cu succes cu 7 coloane.'
