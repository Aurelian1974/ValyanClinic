-- ========================================
-- Tabel: DispozitiveMedicale
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:41
-- Coloane: 13
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 3
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.DispozitiveMedicale', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.DispozitiveMedicale
    PRINT 'Tabel DispozitiveMedicale sters.'
END
GO

-- Create table
CREATE TABLE dbo.DispozitiveMedicale (    [Id] INT IDENTITY(1,1) NOT NULL,
    [Guid] UNIQUEIDENTIFIER  NULL,
    [Denumire] NVARCHAR(255)  NOT NULL,
    [Categorie] NVARCHAR(100)  NULL,
    [ClasaRisc] NVARCHAR(10)  NULL,
    [Producator] NVARCHAR(255)  NULL,
    [ModelTip] NVARCHAR(100)  NULL,
    [NumarSerie] NVARCHAR(100)  NULL,
    [CertificareCE] BIT  NULL,
    [DataExpirare] DATE  NULL,
    [Specificatii] NVARCHAR(MAX)  NULL,
    [DataCreare] DATETIME2  NULL,
    [DataModificare] DATETIME2  NULL,
    ,CONSTRAINT [PK_DispozitiveMedicale] PRIMARY KEY ([Id])
)
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IX_DispozitiveMedicale_ClasaRisc] 
ON dbo.DispozitiveMedicale (ClasaRisc ASC)
GO
CREATE NONCLUSTERED INDEX [IX_DispozitiveMedicale_Categorie] 
ON dbo.DispozitiveMedicale (Categorie ASC)
GO
CREATE NONCLUSTERED INDEX [IX_DispozitiveMedicale_Guid] 
ON dbo.DispozitiveMedicale (Guid ASC)
GO
PRINT 'Tabel DispozitiveMedicale creat cu succes cu 13 coloane.'
