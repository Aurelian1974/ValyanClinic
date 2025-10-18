-- ========================================
-- Tabel: Audit_Persoana
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:44
-- Coloane: 16
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Audit_Persoana', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Audit_Persoana
    PRINT 'Tabel Audit_Persoana sters.'
END
GO

-- Create table
CREATE TABLE dbo.Audit_Persoana (    [AuditId] INT IDENTITY(1,1) NOT NULL,
    [PersoanaId] INT  NULL,
    [Actiune] NVARCHAR(50)  NULL,
    [Nume] NVARCHAR(100)  NULL,
    [Prenume] NVARCHAR(100)  NULL,
    [Judet] NVARCHAR(100)  NULL,
    [Localitate] NVARCHAR(100)  NULL,
    [Strada] NVARCHAR(150)  NULL,
    [NumarStrada] NVARCHAR(50)  NULL,
    [CodPostal] NVARCHAR(20)  NULL,
    [PozitieOrganizatie] NVARCHAR(100)  NULL,
    [DataNasterii] DATE  NULL,
    [DataCreare] DATETIME  NULL,
    [DataModificare] DATETIME  NULL,
    [DataAudit] DATETIME  NULL,
    [UserAudit] NVARCHAR(100)  NULL,
    ,CONSTRAINT [PK_Audit_Persoana] PRIMARY KEY ([AuditId])
)
GO
PRINT 'Tabel Audit_Persoana creat cu succes cu 16 coloane.'
