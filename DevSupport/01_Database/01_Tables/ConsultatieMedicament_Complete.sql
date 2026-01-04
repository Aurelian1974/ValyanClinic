/*
==============================================================================
TABLE: ConsultatieMedicament
==============================================================================
Description: Tabelă pentru stocarea medicamentelor prescrise într-o consultație
             Relație 1:N cu Consultatii (o consultație poate avea multe medicamente)
Author: System
Date: 2026-01-04
Version: 1.0
==============================================================================
*/

USE [ValyanMed]
GO

-- ============================================================================
-- DROP TABLE IF EXISTS
-- ============================================================================
IF OBJECT_ID('dbo.ConsultatieMedicament', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[ConsultatieMedicament];
    PRINT 'Dropped existing ConsultatieMedicament table';
END
GO

-- ============================================================================
-- CREATE TABLE
-- ============================================================================
CREATE TABLE [dbo].[ConsultatieMedicament]
(
    -- Primary Key
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Key to Consultatie
    [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Ordinea afișării în listă
    [OrdineAfisare] INT NOT NULL DEFAULT 0,
    
    -- Detalii medicament
    [NumeMedicament] NVARCHAR(500) NOT NULL,
    [Doza] NVARCHAR(100) NULL,
    [Frecventa] NVARCHAR(200) NULL,
    [Durata] NVARCHAR(100) NULL,
    [Cantitate] NVARCHAR(100) NULL,
    [Observatii] NVARCHAR(500) NULL,
    
    -- Audit
    [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatDe] UNIQUEIDENTIFIER NOT NULL,
    [DataUltimeiModificari] DATETIME NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL,
    
    -- Constraints
    CONSTRAINT [PK_ConsultatieMedicament] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ConsultatieMedicament_Consultatie] FOREIGN KEY ([ConsultatieID])
        REFERENCES [dbo].[Consultatii] ([ConsultatieID]) ON DELETE CASCADE
);
GO

-- ============================================================================
-- INDEXES
-- ============================================================================
CREATE NONCLUSTERED INDEX [IX_ConsultatieMedicament_ConsultatieID]
ON [dbo].[ConsultatieMedicament] ([ConsultatieID] ASC)
INCLUDE ([OrdineAfisare], [NumeMedicament], [Doza], [Frecventa], [Durata]);
GO

-- ============================================================================
-- VERIFICATION
-- ============================================================================
PRINT '=== ConsultatieMedicament Table Created Successfully ===';
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.ConsultatieMedicament')
ORDER BY c.column_id;
GO
