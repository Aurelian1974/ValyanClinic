-- ========================================
-- Tabel: Audit_UtilizatorDetaliat
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:40
-- Coloane: 7
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 0
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Audit_UtilizatorDetaliat', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Audit_UtilizatorDetaliat
    PRINT 'Tabel Audit_UtilizatorDetaliat sters.'
END
GO

-- Create table
CREATE TABLE dbo.Audit_UtilizatorDetaliat (    [AuditDetaliuId] INT IDENTITY(1,1) NOT NULL,
    [UtilizatorId] INT  NULL,
    [Coloana] NVARCHAR(100)  NULL,
    [ValoareVeche] NVARCHAR(MAX)  NULL,
    [ValoareNoua] NVARCHAR(MAX)  NULL,
    [DataAudit] DATETIME  NULL,
    [UserAudit] NVARCHAR(100)  NULL,
    ,CONSTRAINT [PK_Audit_UtilizatorDetaliat] PRIMARY KEY ([AuditDetaliuId])
)
GO
PRINT 'Tabel Audit_UtilizatorDetaliat creat cu succes cu 7 coloane.'
