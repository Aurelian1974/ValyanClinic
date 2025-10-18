-- ========================================
-- Tabel: Medicament
-- Database: ValyanMed
-- Generat: 2025-10-18 08:40:45
-- Coloane: 28
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 8
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Medicament', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Medicament
    PRINT 'Tabel Medicament sters.'
END
GO

-- Create table
CREATE TABLE dbo.Medicament (    [MedicamentID] INT IDENTITY(1,1) NOT NULL,
    [MedicamentGUID] UNIQUEIDENTIFIER  NOT NULL,
    [Nume] NVARCHAR(200)  NOT NULL,
    [DenumireComunaInternationala] NVARCHAR(200)  NOT NULL,
    [Concentratie] NVARCHAR(100)  NOT NULL,
    [FormaFarmaceutica] NVARCHAR(100)  NOT NULL,
    [Producator] NVARCHAR(200)  NOT NULL,
    [CodATC] VARCHAR(20)  NOT NULL,
    [Status] VARCHAR(50)  NOT NULL,
    [DataInregistrare] DATETIME2  NOT NULL,
    [NumarAutorizatie] VARCHAR(50)  NULL,
    [DataAutorizatie] DATE  NULL,
    [DataExpirare] DATE  NOT NULL,
    [Ambalaj] NVARCHAR(200)  NULL,
    [Prospect] NTEXT  NULL,
    [Contraindicatii] NTEXT  NULL,
    [Interactiuni] NTEXT  NULL,
    [Pret] DECIMAL(10,2)  NULL,
    [PretProducator] DECIMAL(10,2)  NULL,
    [TVA] DECIMAL(5,2)  NULL,
    [Compensat] BIT  NULL,
    [PrescriptieMedicala] BIT  NULL,
    [Stoc] INT  NOT NULL,
    [StocSiguranta] INT  NOT NULL,
    [DataActualizare] DATETIME2  NULL,
    [UtilizatorActualizare] NVARCHAR(100)  NULL,
    [Observatii] NTEXT  NULL,
    [Activ] BIT  NOT NULL,
    ,CONSTRAINT [PK_Medicament] PRIMARY KEY ([MedicamentID])
)
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IX_Medicament_DenumireGenerica] 
ON dbo.Medicament (DenumireComunaInternationala ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Medicament_Producator] 
ON dbo.Medicament (Producator ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Medicament_CodATC] 
ON dbo.Medicament (CodATC ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ__Medicame__275350AA66DCF67A] 
ON dbo.Medicament (MedicamentGUID ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Medicament_Activ] 
ON dbo.Medicament (Activ ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Medicament_Nume] 
ON dbo.Medicament (Nume ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Medicament_DataInregistrare] 
ON dbo.Medicament (DataInregistrare ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Medicament_Status] 
ON dbo.Medicament (Status ASC)
GO
PRINT 'Tabel Medicament creat cu succes cu 28 coloane.'
