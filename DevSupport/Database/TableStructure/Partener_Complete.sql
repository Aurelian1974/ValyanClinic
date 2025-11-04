-- ========================================
-- Tabel: Partener
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:45
-- Coloane: 13
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 6
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Partener', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Partener
    PRINT 'Tabel Partener sters.'
END
GO

-- Create table
CREATE TABLE dbo.Partener (    [PartenerId] INT IDENTITY(1,1) NOT NULL,
    [PartenerGuid] UNIQUEIDENTIFIER  NOT NULL,
    [CodIntern] NVARCHAR(50)  NOT NULL,
    [Denumire] NVARCHAR(200)  NOT NULL,
    [CodFiscal] NVARCHAR(50)  NULL,
    [Judet] NVARCHAR(100)  NULL,
    [Localitate] NVARCHAR(100)  NULL,
    [Adresa] NVARCHAR(500)  NULL,
    [DataCreare] DATETIME2  NOT NULL,
    [DataActualizare] DATETIME2  NOT NULL,
    [UtilizatorCreare] NVARCHAR(100)  NULL,
    [UtilizatorActualizare] NVARCHAR(100)  NULL,
    [Activ] BIT  NOT NULL,
    ,CONSTRAINT [PK_Partener] PRIMARY KEY ([PartenerId])
)
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IX_Partener_CodFiscal] 
ON dbo.Partener (CodFiscal ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Partener_Activ] 
ON dbo.Partener (Activ ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Partener_Judet] 
ON dbo.Partener (Judet ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Partener_Denumire] 
ON dbo.Partener (Denumire ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Partener_Guid] 
ON dbo.Partener (PartenerGuid ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Partener_CodIntern] 
ON dbo.Partener (CodIntern ASC)
GO
PRINT 'Tabel Partener creat cu succes cu 13 coloane.'
