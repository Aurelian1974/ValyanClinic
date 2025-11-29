-- ========================================
-- Tabel: FormulareConsimtamant
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:45
-- Coloane: 7
-- Primary Keys: 1
-- Foreign Keys: 2
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.FormulareConsimtamant', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.FormulareConsimtamant
    PRINT 'Tabel FormulareConsimtamant sters.'
END
GO

-- Create table
CREATE TABLE dbo.FormulareConsimtamant (    [ConsimtamantID] UNIQUEIDENTIFIER  NOT NULL,
    [PacientID] UNIQUEIDENTIFIER  NOT NULL,
    [TipFormular] NVARCHAR(100)  NOT NULL,
    [ConsimtamantAcordat] BIT  NOT NULL,
    [DataConsimtamant] DATETIME2  NOT NULL,
    [MartorID] UNIQUEIDENTIFIER  NULL,
    [CaleDocument] NVARCHAR(500)  NULL,
    ,CONSTRAINT [PK_FormulareConsimtamant] PRIMARY KEY ([ConsimtamantID])
)
GO

-- Foreign Keys
ALTER TABLE dbo.FormulareConsimtamant
ADD CONSTRAINT [FK__Formulare__Pacie__3CBF0154] FOREIGN KEY ([PacientID]) 
    REFERENCES dbo.[Pacienti] ([PacientID])
GO
ALTER TABLE dbo.FormulareConsimtamant
ADD CONSTRAINT [FK__Formulare__Marto__3DB3258D] FOREIGN KEY ([MartorID]) 
    REFERENCES dbo.[PersonalMedical] ([PersonalID])
GO
PRINT 'Tabel FormulareConsimtamant creat cu succes cu 7 coloane.'
