-- ========================================
-- Tabel: Audit_Utilizator
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:44
-- Coloane: 12
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Audit_Utilizator', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Audit_Utilizator
    PRINT 'Tabel Audit_Utilizator sters.'
END
GO

-- Create table
CREATE TABLE dbo.Audit_Utilizator (    [AuditId] INT IDENTITY(1,1) NOT NULL,
    [UtilizatorId] INT  NULL,
    [Actiune] NVARCHAR(50)  NULL,
    [PersoanaId] INT  NULL,
    [NumeUtilizator] NVARCHAR(100)  NULL,
    [ParolaHash] NVARCHAR(512)  NULL,
    [Email] NVARCHAR(150)  NULL,
    [Telefon] NVARCHAR(50)  NULL,
    [DataCreare] DATETIME  NULL,
    [DataModificare] DATETIME  NULL,
    [DataAudit] DATETIME  NULL,
    [UserAudit] NVARCHAR(100)  NULL,
    ,CONSTRAINT [PK_Audit_Utilizator] PRIMARY KEY ([AuditId])
)
GO
PRINT 'Tabel Audit_Utilizator creat cu succes cu 12 coloane.'
