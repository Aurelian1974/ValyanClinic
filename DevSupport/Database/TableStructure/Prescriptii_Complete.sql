-- ========================================
-- Tabel: Prescriptii
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:43
-- Coloane: 11
-- Primary Keys: 1
-- Foreign Keys: 3
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Prescriptii', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Prescriptii
    PRINT 'Tabel Prescriptii sters.'
END
GO

-- Create table
CREATE TABLE dbo.Prescriptii (    [PrescriptieID] UNIQUEIDENTIFIER  NOT NULL,
    [ConsultatieID] UNIQUEIDENTIFIER  NOT NULL,
    [MedicamentID] UNIQUEIDENTIFIER  NOT NULL,
    [Doza] NVARCHAR(100)  NULL,
    [Frecventa] NVARCHAR(100)  NULL,
    [Durata] NVARCHAR(100)  NULL,
    [Cantitate] INT  NULL,
    [Reinnoire] INT  NULL,
    [Instructiuni] NVARCHAR(500)  NULL,
    [DataPrescriptie] DATETIME2  NULL,
    [PrescrisDe] UNIQUEIDENTIFIER  NOT NULL,
    ,CONSTRAINT [PK_Prescriptii] PRIMARY KEY ([PrescriptieID])
)
GO

-- Foreign Keys
ALTER TABLE dbo.Prescriptii
ADD CONSTRAINT [FK__Prescript__Consu__75035A77] FOREIGN KEY ([ConsultatieID]) 
    REFERENCES dbo.[Consultatii] ([ConsultatieID])
GO
ALTER TABLE dbo.Prescriptii
ADD CONSTRAINT [FK__Prescript__Medic__75F77EB0] FOREIGN KEY ([MedicamentID]) 
    REFERENCES dbo.[MedicamenteNoi] ([MedicamentID])
GO
ALTER TABLE dbo.Prescriptii
ADD CONSTRAINT [FK__Prescript__Presc__76EBA2E9] FOREIGN KEY ([PrescrisDe]) 
    REFERENCES dbo.[PersonalMedical] ([PersonalID])
GO
PRINT 'Tabel Prescriptii creat cu succes cu 11 coloane.'
