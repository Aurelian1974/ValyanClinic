-- ========================================
-- Tabel: Ocupatii_ISCO08
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:45
-- Coloane: 19
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 7
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Ocupatii_ISCO08', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Ocupatii_ISCO08
    PRINT 'Tabel Ocupatii_ISCO08 sters.'
END
GO

-- Create table
CREATE TABLE dbo.Ocupatii_ISCO08 (    [Id] UNIQUEIDENTIFIER  NOT NULL,
    [Cod_ISCO] NVARCHAR(10)  NOT NULL,
    [Denumire_Ocupatie] NVARCHAR(500)  NOT NULL,
    [Denumire_Ocupatie_EN] NVARCHAR(500)  NULL,
    [Nivel_Ierarhic] TINYINT  NOT NULL,
    [Cod_Parinte] NVARCHAR(10)  NULL,
    [Grupa_Majora] NVARCHAR(10)  NULL,
    [Grupa_Majora_Denumire] NVARCHAR(300)  NULL,
    [Subgrupa] NVARCHAR(10)  NULL,
    [Subgrupa_Denumire] NVARCHAR(300)  NULL,
    [Grupa_Minora] NVARCHAR(10)  NULL,
    [Grupa_Minora_Denumire] NVARCHAR(300)  NULL,
    [Descriere] NVARCHAR(MAX)  NULL,
    [Observatii] NVARCHAR(1000)  NULL,
    [Este_Activ] BIT  NOT NULL,
    [Data_Crearii] DATETIME2  NOT NULL,
    [Data_Ultimei_Modificari] DATETIME2  NOT NULL,
    [Creat_De] NVARCHAR(100)  NULL,
    [Modificat_De] NVARCHAR(100)  NULL,
    ,CONSTRAINT [PK_Ocupatii_ISCO08] PRIMARY KEY ([Id])
)
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Grupa_Majora] 
ON dbo.Ocupatii_ISCO08 (Grupa_Majora ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Activ] 
ON dbo.Ocupatii_ISCO08 (Este_Activ ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Nivel_Ierarhic] 
ON dbo.Ocupatii_ISCO08 (Nivel_Ierarhic ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Parinte] 
ON dbo.Ocupatii_ISCO08 (Cod_Parinte ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Cod_ISCO] 
ON dbo.Ocupatii_ISCO08 (Cod_ISCO ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Denumire] 
ON dbo.Ocupatii_ISCO08 (Denumire_Ocupatie ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UK_Ocupatii_ISCO08_Cod] 
ON dbo.Ocupatii_ISCO08 (Cod_ISCO ASC)
GO
PRINT 'Tabel Ocupatii_ISCO08 creat cu succes cu 19 coloane.'
