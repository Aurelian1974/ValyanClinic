-- ========================================
-- Tabel: PersonalMedical
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:45
-- Coloane: 14
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 6
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.PersonalMedical', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.PersonalMedical
    PRINT 'Tabel PersonalMedical sters.'
END
GO

-- Create table
CREATE TABLE dbo.PersonalMedical (    [PersonalID] UNIQUEIDENTIFIER  NOT NULL,
    [Nume] NVARCHAR(100)  NOT NULL,
    [Prenume] NVARCHAR(100)  NOT NULL,
    [Specializare] NVARCHAR(100)  NULL,
    [NumarLicenta] NVARCHAR(50)  NULL,
    [Telefon] NVARCHAR(20)  NULL,
    [Email] NVARCHAR(100)  NULL,
    [Departament] NVARCHAR(100)  NULL,
    [Pozitie] NVARCHAR(50)  NULL,
    [EsteActiv] BIT  NULL,
    [DataCreare] DATETIME2  NULL,
    [CategorieID] UNIQUEIDENTIFIER  NULL,
    [SpecializareID] UNIQUEIDENTIFIER  NULL,
    [SubspecializareID] UNIQUEIDENTIFIER  NULL,
    ,CONSTRAINT [PK_PersonalMedical] PRIMARY KEY ([PersonalID])
)
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IX_PersonalMedical_Ierarhie_Activ] 
ON dbo.PersonalMedical (CategorieID ASC, SpecializareID ASC, EsteActiv ASC)
GO
CREATE NONCLUSTERED INDEX [IX_PersonalMedical_NumarLicenta] 
ON dbo.PersonalMedical (NumarLicenta ASC)
GO
CREATE NONCLUSTERED INDEX [IX_PersonalMedical_SubspecializareID] 
ON dbo.PersonalMedical (SubspecializareID ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ__Personal__0D51B6659A3E2294] 
ON dbo.PersonalMedical (NumarLicenta ASC)
GO
CREATE NONCLUSTERED INDEX [IX_PersonalMedical_SpecializareID] 
ON dbo.PersonalMedical (SpecializareID ASC)
GO
CREATE NONCLUSTERED INDEX [IX_PersonalMedical_CategorieID] 
ON dbo.PersonalMedical (CategorieID ASC)
GO
PRINT 'Tabel PersonalMedical creat cu succes cu 14 coloane.'
