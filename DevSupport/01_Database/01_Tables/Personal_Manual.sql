-- ========================================
-- Tabel: Personal
-- Database: ValyanMed
-- Generat: 2025-10-08 16:28:32
-- ========================================

USE [ValyanMed]
GO

IF OBJECT_ID('dbo.Personal', 'U') IS NOT NULL
    DROP TABLE dbo.Personal
GO

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
    [Modificat_De] NVARCHAR(50)  NULL
)
GO

PRINT 'Tabel Personal creat cu succes.'
