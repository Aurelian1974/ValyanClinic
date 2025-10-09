-- ========================================
-- Tabel: PersonalMedical_Backup_Migration
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:43
-- Coloane: 11
-- Primary Keys: 0
-- Foreign Keys: 0
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.PersonalMedical_Backup_Migration', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.PersonalMedical_Backup_Migration
    PRINT 'Tabel PersonalMedical_Backup_Migration sters.'
END
GO

-- Create table
CREATE TABLE dbo.PersonalMedical_Backup_Migration (    [PersonalID] UNIQUEIDENTIFIER  NOT NULL,
    [Nume] NVARCHAR(100)  NOT NULL,
    [Prenume] NVARCHAR(100)  NOT NULL,
    [Specializare] NVARCHAR(100)  NULL,
    [NumarLicenta] NVARCHAR(50)  NULL,
    [Telefon] NVARCHAR(20)  NULL,
    [Email] NVARCHAR(100)  NULL,
    [Departament] NVARCHAR(100)  NULL,
    [Pozitie] NVARCHAR(50)  NULL,
    [EsteActiv] BIT  NULL,
    [DataCreare] DATETIME2  NULL
)
GO
PRINT 'Tabel PersonalMedical_Backup_Migration creat cu succes cu 11 coloane.'
