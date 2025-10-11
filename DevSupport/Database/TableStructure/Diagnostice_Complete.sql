-- ========================================
-- Tabel: Diagnostice
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:41
-- Coloane: 8
-- Primary Keys: 1
-- Foreign Keys: 1
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Diagnostice', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Diagnostice
    PRINT 'Tabel Diagnostice sters.'
END
GO

-- Create table
CREATE TABLE dbo.Diagnostice (    [DiagnosticID] UNIQUEIDENTIFIER  NOT NULL,
    [ConsultatieID] UNIQUEIDENTIFIER  NOT NULL,
    [CodICD] NVARCHAR(10)  NULL,
    [DescriereaDiagnosticului] NVARCHAR(500)  NOT NULL,
    [TipDiagnostic] NVARCHAR(50)  NULL,
    [Severitate] NVARCHAR(50)  NULL,
    [Status] NVARCHAR(50)  NULL,
    [DataDiagnostic] DATETIME2  NULL,
    ,CONSTRAINT [PK_Diagnostice] PRIMARY KEY ([DiagnosticID])
)
GO

-- Foreign Keys
ALTER TABLE dbo.Diagnostice
ADD CONSTRAINT [FK__Diagnosti__Consu__6B79F03D] FOREIGN KEY ([ConsultatieID]) 
    REFERENCES dbo.[Consultatii] ([ConsultatieID])
GO
PRINT 'Tabel Diagnostice creat cu succes cu 8 coloane.'
