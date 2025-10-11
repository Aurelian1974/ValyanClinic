-- ========================================
-- Tabel: Pacienti
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:42
-- Coloane: 19
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 3
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Pacienti', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Pacienti
    PRINT 'Tabel Pacienti sters.'
END
GO

-- Create table
CREATE TABLE dbo.Pacienti (    [PacientID] UNIQUEIDENTIFIER  NOT NULL,
    [Nume] NVARCHAR(100)  NOT NULL,
    [Prenume] NVARCHAR(100)  NOT NULL,
    [CNP] NVARCHAR(13)  NULL,
    [DataNasterii] DATE  NOT NULL,
    [Gen] NVARCHAR(20)  NULL,
    [Telefon] NVARCHAR(20)  NULL,
    [Email] NVARCHAR(100)  NULL,
    [Adresa] NVARCHAR(500)  NULL,
    [Oras] NVARCHAR(100)  NULL,
    [Judet] NVARCHAR(100)  NULL,
    [CodPostal] NVARCHAR(10)  NULL,
    [NumeContactUrgenta] NVARCHAR(200)  NULL,
    [TelefonContactUrgenta] NVARCHAR(20)  NULL,
    [FurnizorAsigurare] NVARCHAR(100)  NULL,
    [NumarAsigurare] NVARCHAR(50)  NULL,
    [DataCreare] DATETIME2  NULL,
    [DataUltimeiModificari] DATETIME2  NULL,
    [EsteActiv] BIT  NULL,
    ,CONSTRAINT [PK_Pacienti] PRIMARY KEY ([PacientID])
)
GO

-- Indexes
CREATE UNIQUE NONCLUSTERED INDEX [UQ__Pacienti__C1FF677D015D1673] 
ON dbo.Pacienti (CNP ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Pacienti_NumeComplet] 
ON dbo.Pacienti (Nume ASC, Prenume ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Pacienti_CNP] 
ON dbo.Pacienti (CNP ASC)
GO
PRINT 'Tabel Pacienti creat cu succes cu 19 coloane.'
