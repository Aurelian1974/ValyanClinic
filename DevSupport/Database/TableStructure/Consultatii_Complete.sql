-- ========================================
-- Tabel: Consultatii
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:40
-- Coloane: 9
-- Primary Keys: 1
-- Foreign Keys: 1
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Consultatii', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Consultatii
    PRINT 'Tabel Consultatii sters.'
END
GO

-- Create table
CREATE TABLE dbo.Consultatii (    [ConsultatieID] UNIQUEIDENTIFIER  NOT NULL,
    [ProgramareID] UNIQUEIDENTIFIER  NOT NULL,
    [PlangereaPrincipala] NVARCHAR(1000)  NULL,
    [IstoricBoalaActuala] NVARCHAR(2000)  NULL,
    [ExamenFizic] NVARCHAR(2000)  NULL,
    [Evaluare] NVARCHAR(1000)  NULL,
    [Plan] NVARCHAR(1000)  NULL,
    [DataConsultatie] DATETIME2  NULL,
    [Durata] INT  NULL,
    ,CONSTRAINT [PK_Consultatii] PRIMARY KEY ([ConsultatieID])
)
GO

-- Foreign Keys
ALTER TABLE dbo.Consultatii
ADD CONSTRAINT [FK__Consultat__Progr__54968AE5] FOREIGN KEY ([ProgramareID]) 
    REFERENCES dbo.[Programari] ([ProgramareID])
GO
PRINT 'Tabel Consultatii creat cu succes cu 9 coloane.'
