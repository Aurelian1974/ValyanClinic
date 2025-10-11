-- ========================================
-- Tabel: IstoricMedical
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:41
-- Coloane: 9
-- Primary Keys: 1
-- Foreign Keys: 2
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.IstoricMedical', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.IstoricMedical
    PRINT 'Tabel IstoricMedical sters.'
END
GO

-- Create table
CREATE TABLE dbo.IstoricMedical (    [IstoricID] UNIQUEIDENTIFIER  NOT NULL,
    [PacientID] UNIQUEIDENTIFIER  NOT NULL,
    [Afectiune] NVARCHAR(200)  NOT NULL,
    [DataDiagnostic] DATE  NULL,
    [Status] NVARCHAR(50)  NULL,
    [Severitate] NVARCHAR(50)  NULL,
    [Observatii] NVARCHAR(1000)  NULL,
    [InregistratDe] UNIQUEIDENTIFIER  NULL,
    [DataInregistrare] DATETIME2  NULL,
    ,CONSTRAINT [PK_IstoricMedical] PRIMARY KEY ([IstoricID])
)
GO

-- Foreign Keys
ALTER TABLE dbo.IstoricMedical
ADD CONSTRAINT [FK__IstoricMe__Pacie__4EDDB18F] FOREIGN KEY ([PacientID]) 
    REFERENCES dbo.[Pacienti] ([PacientID])
GO
ALTER TABLE dbo.IstoricMedical
ADD CONSTRAINT [FK__IstoricMe__Inreg__4FD1D5C8] FOREIGN KEY ([InregistratDe]) 
    REFERENCES dbo.[PersonalMedical] ([PersonalID])
GO
PRINT 'Tabel IstoricMedical creat cu succes cu 9 coloane.'
