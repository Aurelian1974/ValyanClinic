-- ========================================
-- Tabel: Personal
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:42
-- Coloane: 36
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 9
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Personal', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Personal
    PRINT 'Tabel Personal sters.'
END
GO

-- Create table
CREATE TABLE dbo.Personal (    [Id_Personal] UNIQUEIDENTIFIER  NOT NULL,
    [Cod_Angajat] VARCHAR(20)  NOT NULL,
    [CNP] VARCHAR(13)  NOT NULL,
    [Nume] NVARCHAR(100)  NOT NULL,
    [Prenume] NVARCHAR(100)  NOT NULL,
    [Nume_Anterior] NVARCHAR(100)  NULL,
    [Data_Nasterii] DATE  NOT NULL,
    [Locul_Nasterii] NVARCHAR(200)  NULL,
    [Nationalitate] NVARCHAR(50)  NULL,
    [Cetatenie] NVARCHAR(50)  NULL,
    [Telefon_Personal] VARCHAR(20)  NULL,
    [Telefon_Serviciu] VARCHAR(20)  NULL,
    [Email_Personal] VARCHAR(100)  NULL,
    [Email_Serviciu] VARCHAR(100)  NULL,
    [Adresa_Domiciliu] NVARCHAR(MAX)  NOT NULL,
    [Judet_Domiciliu] NVARCHAR(50)  NOT NULL,
    [Oras_Domiciliu] NVARCHAR(100)  NOT NULL,
    [Cod_Postal_Domiciliu] VARCHAR(10)  NULL,
    [Adresa_Resedinta] NVARCHAR(MAX)  NULL,
    [Judet_Resedinta] NVARCHAR(50)  NULL,
    [Oras_Resedinta] NVARCHAR(100)  NULL,
    [Cod_Postal_Resedinta] VARCHAR(10)  NULL,
    [Stare_Civila] NVARCHAR(100)  NULL,
    [Functia] NVARCHAR(100)  NOT NULL,
    [Departament] NVARCHAR(100)  NULL,
    [Serie_CI] VARCHAR(10)  NULL,
    [Numar_CI] VARCHAR(20)  NULL,
    [Eliberat_CI_De] NVARCHAR(100)  NULL,
    [Data_Eliberare_CI] DATE  NULL,
    [Valabil_CI_Pana] DATE  NULL,
    [Status_Angajat] NVARCHAR(50)  NULL,
    [Observatii] NVARCHAR(MAX)  NULL,
    [Data_Crearii] DATETIME2  NOT NULL,
    [Data_Ultimei_Modificari] DATETIME2  NOT NULL,
    [Creat_De] NVARCHAR(50)  NULL,
    [Modificat_De] NVARCHAR(50)  NULL,
    ,CONSTRAINT [PK_Personal] PRIMARY KEY ([Id_Personal])
)
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IX_Personal_Functia] 
ON dbo.Personal (Functia ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Personal_Status] 
ON dbo.Personal (Status_Angajat ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Personal_Data_Crearii] 
ON dbo.Personal (Data_Crearii ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ__Personal__C1FF677D763B026B] 
ON dbo.Personal (CNP ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Personal_Departament] 
ON dbo.Personal (Departament ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Personal_CNP] 
ON dbo.Personal (CNP ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Personal_Nume] 
ON dbo.Personal (Nume ASC, Prenume ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ__Personal__00477B3B2155E92F] 
ON dbo.Personal (Cod_Angajat ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Personal_Cod_Angajat] 
ON dbo.Personal (Cod_Angajat ASC)
GO
PRINT 'Tabel Personal creat cu succes cu 36 coloane.'
