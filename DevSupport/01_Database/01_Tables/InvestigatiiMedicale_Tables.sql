/*
==============================================================================
TABELE PENTRU INVESTIGAȚII MEDICALE
==============================================================================
Description: Nomenclatoare și tabele pentru investigații imagistice, 
             explorări funcționale și endoscopii (recomandate + efectuate)
Author: AI Agent
Date: 2026-01-06
Version: 1.0
==============================================================================
*/

USE [ValyanMed]
GO

-- ============================================================================
-- SECȚIUNEA 1: NOMENCLATOARE (CATALOAGE)
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 1.1. Nomenclator Investigații Imagistice
-- ----------------------------------------------------------------------------
PRINT 'Creating NomenclatorInvestigatiiImagistice...'

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NomenclatorInvestigatiiImagistice')
BEGIN
    CREATE TABLE [dbo].[NomenclatorInvestigatiiImagistice] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [Cod] NVARCHAR(20) NOT NULL,
        [Denumire] NVARCHAR(200) NOT NULL,
        [Descriere] NVARCHAR(500) NULL,
        [Categorie] NVARCHAR(100) NULL, -- Radiografie, Ecografie, CT, RMN, Angiografie, Scintigrafie, etc.
        [RequiresContrast] BIT NOT NULL DEFAULT 0,
        [PreparationInstructions] NVARCHAR(MAX) NULL,
        [EstimatedDuration] INT NULL, -- durata în minute
        [IsActive] BIT NOT NULL DEFAULT 1,
        [OrdineAfisare] INT NOT NULL DEFAULT 0,
        [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatDe] UNIQUEIDENTIFIER NULL,
        [DataUltimeiModificari] DATETIME NULL,
        [ModificatDe] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_NomenclatorInvestigatiiImagistice] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_NomenclatorInvestigatiiImagistice_Cod] UNIQUE ([Cod])
    );
    PRINT '✓ NomenclatorInvestigatiiImagistice created'
END
ELSE
    PRINT '→ NomenclatorInvestigatiiImagistice already exists'
GO

-- ----------------------------------------------------------------------------
-- 1.2. Nomenclator Explorări Funcționale
-- ----------------------------------------------------------------------------
PRINT 'Creating NomenclatorExplorariFunc...'

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NomenclatorExplorariFunc')
BEGIN
    CREATE TABLE [dbo].[NomenclatorExplorariFunc] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [Cod] NVARCHAR(20) NOT NULL,
        [Denumire] NVARCHAR(200) NOT NULL,
        [Descriere] NVARCHAR(500) NULL,
        [Categorie] NVARCHAR(100) NULL, -- Cardiologie, Pneumologie, Neurologie, ORL, Gastroenterologie, Urologie
        [PreparationInstructions] NVARCHAR(MAX) NULL,
        [EstimatedDuration] INT NULL, -- durata în minute
        [IsActive] BIT NOT NULL DEFAULT 1,
        [OrdineAfisare] INT NOT NULL DEFAULT 0,
        [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatDe] UNIQUEIDENTIFIER NULL,
        [DataUltimeiModificari] DATETIME NULL,
        [ModificatDe] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_NomenclatorExplorariFunc] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_NomenclatorExplorariFunc_Cod] UNIQUE ([Cod])
    );
    PRINT '✓ NomenclatorExplorariFunc created'
END
ELSE
    PRINT '→ NomenclatorExplorariFunc already exists'
GO

-- ----------------------------------------------------------------------------
-- 1.3. Nomenclator Endoscopii
-- ----------------------------------------------------------------------------
PRINT 'Creating NomenclatorEndoscopii...'

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NomenclatorEndoscopii')
BEGIN
    CREATE TABLE [dbo].[NomenclatorEndoscopii] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [Cod] NVARCHAR(20) NOT NULL,
        [Denumire] NVARCHAR(200) NOT NULL,
        [Descriere] NVARCHAR(500) NULL,
        [Categorie] NVARCHAR(100) NULL, -- Digestiv, Respirator, Urologic, Ginecologic, Articular, Chirurgical
        [RequiresSedation] BIT NOT NULL DEFAULT 0,
        [PreparationInstructions] NVARCHAR(MAX) NULL,
        [EstimatedDuration] INT NULL, -- durata în minute
        [IsActive] BIT NOT NULL DEFAULT 1,
        [OrdineAfisare] INT NOT NULL DEFAULT 0,
        [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatDe] UNIQUEIDENTIFIER NULL,
        [DataUltimeiModificari] DATETIME NULL,
        [ModificatDe] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_NomenclatorEndoscopii] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_NomenclatorEndoscopii_Cod] UNIQUE ([Cod])
    );
    PRINT '✓ NomenclatorEndoscopii created'
END
ELSE
    PRINT '→ NomenclatorEndoscopii already exists'
GO

-- ============================================================================
-- SECȚIUNEA 2: INVESTIGAȚII RECOMANDATE LA CONSULTAȚIE
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 2.1. Investigații Imagistice Recomandate
-- ----------------------------------------------------------------------------
PRINT 'Creating ConsultatieInvestigatieImagisticaRecomandata...'

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConsultatieInvestigatieImagisticaRecomandata')
BEGIN
    CREATE TABLE [dbo].[ConsultatieInvestigatieImagisticaRecomandata] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
        [InvestigatieID] UNIQUEIDENTIFIER NOT NULL, -- FK to NomenclatorInvestigatiiImagistice
        [RegiuneAnatomica] NVARCHAR(200) NULL,
        [IndicatiiClinice] NVARCHAR(MAX) NULL,
        [Prioritate] NVARCHAR(50) NULL, -- Normal, Urgent, Cito
        [EsteCito] BIT NOT NULL DEFAULT 0,
        [TermenRecomandat] DATE NULL,
        [Observatii] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Recomandata', -- Recomandata, Programata, Efectuata, Anulata
        [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatDe] UNIQUEIDENTIFIER NOT NULL,
        [DataUltimeiModificari] DATETIME NULL,
        [ModificatDe] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_ConsultatieInvestigatieImagisticaRecomandata] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ConsultatieInvestigatieImagisticaRecomandata_Consultatie] FOREIGN KEY ([ConsultatieID]) 
            REFERENCES [dbo].[Consultatii] ([ConsultatieID]) ON DELETE CASCADE,
        CONSTRAINT [FK_ConsultatieInvestigatieImagisticaRecomandata_Investigatie] FOREIGN KEY ([InvestigatieID]) 
            REFERENCES [dbo].[NomenclatorInvestigatiiImagistice] ([Id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_ConsultatieInvestigatieImagisticaRecomandata_ConsultatieID] 
        ON [dbo].[ConsultatieInvestigatieImagisticaRecomandata] ([ConsultatieID]);
    
    PRINT '✓ ConsultatieInvestigatieImagisticaRecomandata created'
END
ELSE
    PRINT '→ ConsultatieInvestigatieImagisticaRecomandata already exists'
GO

-- ----------------------------------------------------------------------------
-- 2.2. Explorări Funcționale Recomandate
-- ----------------------------------------------------------------------------
PRINT 'Creating ConsultatieExplorareRecomandata...'

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConsultatieExplorareRecomandata')
BEGIN
    CREATE TABLE [dbo].[ConsultatieExplorareRecomandata] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
        [ExplorareID] UNIQUEIDENTIFIER NOT NULL, -- FK to NomenclatorExplorariFunc
        [IndicatiiClinice] NVARCHAR(MAX) NULL,
        [Prioritate] NVARCHAR(50) NULL, -- Normal, Urgent, Cito
        [EsteCito] BIT NOT NULL DEFAULT 0,
        [TermenRecomandat] DATE NULL,
        [Observatii] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Recomandata', -- Recomandata, Programata, Efectuata, Anulata
        [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatDe] UNIQUEIDENTIFIER NOT NULL,
        [DataUltimeiModificari] DATETIME NULL,
        [ModificatDe] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_ConsultatieExplorareRecomandata] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ConsultatieExplorareRecomandata_Consultatie] FOREIGN KEY ([ConsultatieID]) 
            REFERENCES [dbo].[Consultatii] ([ConsultatieID]) ON DELETE CASCADE,
        CONSTRAINT [FK_ConsultatieExplorareRecomandata_Explorare] FOREIGN KEY ([ExplorareID]) 
            REFERENCES [dbo].[NomenclatorExplorariFunc] ([Id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_ConsultatieExplorareRecomandata_ConsultatieID] 
        ON [dbo].[ConsultatieExplorareRecomandata] ([ConsultatieID]);
    
    PRINT '✓ ConsultatieExplorareRecomandata created'
END
ELSE
    PRINT '→ ConsultatieExplorareRecomandata already exists'
GO

-- ----------------------------------------------------------------------------
-- 2.3. Endoscopii Recomandate
-- ----------------------------------------------------------------------------
PRINT 'Creating ConsultatieEndoscopieRecomandata...'

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConsultatieEndoscopieRecomandata')
BEGIN
    CREATE TABLE [dbo].[ConsultatieEndoscopieRecomandata] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
        [EndoscopieID] UNIQUEIDENTIFIER NOT NULL, -- FK to NomenclatorEndoscopii
        [IndicatiiClinice] NVARCHAR(MAX) NULL,
        [Prioritate] NVARCHAR(50) NULL, -- Normal, Urgent, Cito
        [EsteCito] BIT NOT NULL DEFAULT 0,
        [TermenRecomandat] DATE NULL,
        [Observatii] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Recomandata', -- Recomandata, Programata, Efectuata, Anulata
        [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatDe] UNIQUEIDENTIFIER NOT NULL,
        [DataUltimeiModificari] DATETIME NULL,
        [ModificatDe] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_ConsultatieEndoscopieRecomandata] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ConsultatieEndoscopieRecomandata_Consultatie] FOREIGN KEY ([ConsultatieID]) 
            REFERENCES [dbo].[Consultatii] ([ConsultatieID]) ON DELETE CASCADE,
        CONSTRAINT [FK_ConsultatieEndoscopieRecomandata_Endoscopie] FOREIGN KEY ([EndoscopieID]) 
            REFERENCES [dbo].[NomenclatorEndoscopii] ([Id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_ConsultatieEndoscopieRecomandata_ConsultatieID] 
        ON [dbo].[ConsultatieEndoscopieRecomandata] ([ConsultatieID]);
    
    PRINT '✓ ConsultatieEndoscopieRecomandata created'
END
ELSE
    PRINT '→ ConsultatieEndoscopieRecomandata already exists'
GO

-- ============================================================================
-- SECȚIUNEA 3: INVESTIGAȚII EFECTUATE (CU REZULTATE)
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 3.1. Investigații Imagistice Efectuate
-- ----------------------------------------------------------------------------
PRINT 'Creating ConsultatieInvestigatieImagisticaEfectuata...'

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConsultatieInvestigatieImagisticaEfectuata')
BEGIN
    CREATE TABLE [dbo].[ConsultatieInvestigatieImagisticaEfectuata] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
        [PacientID] UNIQUEIDENTIFIER NOT NULL,
        [InvestigatieID] UNIQUEIDENTIFIER NULL, -- FK to NomenclatorInvestigatiiImagistice (poate fi NULL pentru investigații custom)
        [RecomandareID] UNIQUEIDENTIFIER NULL, -- FK to ConsultatieInvestigatieImagisticaRecomandata (legătura opțională)
        [NumeInvestigatie] NVARCHAR(200) NOT NULL, -- denumirea (din nomenclator sau custom)
        [RegiuneAnatomica] NVARCHAR(200) NULL,
        [DataEfectuare] DATE NOT NULL,
        [Laborator] NVARCHAR(200) NULL, -- unde s-a efectuat
        [MedicRadiolog] NVARCHAR(200) NULL,
        [Rezultat] NVARCHAR(MAX) NULL, -- descrierea rezultatului
        [Concluzie] NVARCHAR(MAX) NULL,
        [EsteAnormal] BIT NOT NULL DEFAULT 0,
        [Observatii] NVARCHAR(MAX) NULL,
        [CaleDocument] NVARCHAR(500) NULL, -- path către documentul scanat/PDF
        [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatDe] UNIQUEIDENTIFIER NOT NULL,
        [DataUltimeiModificari] DATETIME NULL,
        [ModificatDe] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_ConsultatieInvestigatieImagisticaEfectuata] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ConsultatieInvestigatieImagisticaEfectuata_Consultatie] FOREIGN KEY ([ConsultatieID]) 
            REFERENCES [dbo].[Consultatii] ([ConsultatieID]) ON DELETE CASCADE,
        CONSTRAINT [FK_ConsultatieInvestigatieImagisticaEfectuata_Pacient] FOREIGN KEY ([PacientID]) 
            REFERENCES [dbo].[Pacienti] ([Id]),
        CONSTRAINT [FK_ConsultatieInvestigatieImagisticaEfectuata_Investigatie] FOREIGN KEY ([InvestigatieID]) 
            REFERENCES [dbo].[NomenclatorInvestigatiiImagistice] ([Id]),
        CONSTRAINT [FK_ConsultatieInvestigatieImagisticaEfectuata_Recomandare] FOREIGN KEY ([RecomandareID]) 
            REFERENCES [dbo].[ConsultatieInvestigatieImagisticaRecomandata] ([Id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_ConsultatieInvestigatieImagisticaEfectuata_ConsultatieID] 
        ON [dbo].[ConsultatieInvestigatieImagisticaEfectuata] ([ConsultatieID]);
    CREATE NONCLUSTERED INDEX [IX_ConsultatieInvestigatieImagisticaEfectuata_PacientID] 
        ON [dbo].[ConsultatieInvestigatieImagisticaEfectuata] ([PacientID]);
    CREATE NONCLUSTERED INDEX [IX_ConsultatieInvestigatieImagisticaEfectuata_DataEfectuare] 
        ON [dbo].[ConsultatieInvestigatieImagisticaEfectuata] ([DataEfectuare] DESC);
    
    PRINT '✓ ConsultatieInvestigatieImagisticaEfectuata created'
END
ELSE
    PRINT '→ ConsultatieInvestigatieImagisticaEfectuata already exists'
GO

-- ----------------------------------------------------------------------------
-- 3.2. Explorări Funcționale Efectuate
-- ----------------------------------------------------------------------------
PRINT 'Creating ConsultatieExplorareEfectuata...'

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConsultatieExplorareEfectuata')
BEGIN
    CREATE TABLE [dbo].[ConsultatieExplorareEfectuata] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
        [PacientID] UNIQUEIDENTIFIER NOT NULL,
        [ExplorareID] UNIQUEIDENTIFIER NULL, -- FK to NomenclatorExplorariFunc
        [RecomandareID] UNIQUEIDENTIFIER NULL, -- FK to ConsultatieExplorareRecomandata
        [NumeExplorare] NVARCHAR(200) NOT NULL,
        [DataEfectuare] DATE NOT NULL,
        [Laborator] NVARCHAR(200) NULL,
        [MedicExecutant] NVARCHAR(200) NULL,
        [Rezultat] NVARCHAR(MAX) NULL,
        [ParametriMasurati] NVARCHAR(MAX) NULL, -- JSON cu parametrii măsurați (ex: FEV1, FVC, etc.)
        [Interpretare] NVARCHAR(MAX) NULL,
        [Concluzie] NVARCHAR(MAX) NULL,
        [EsteAnormal] BIT NOT NULL DEFAULT 0,
        [Observatii] NVARCHAR(MAX) NULL,
        [CaleDocument] NVARCHAR(500) NULL,
        [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatDe] UNIQUEIDENTIFIER NOT NULL,
        [DataUltimeiModificari] DATETIME NULL,
        [ModificatDe] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_ConsultatieExplorareEfectuata] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ConsultatieExplorareEfectuata_Consultatie] FOREIGN KEY ([ConsultatieID]) 
            REFERENCES [dbo].[Consultatii] ([ConsultatieID]) ON DELETE CASCADE,
        CONSTRAINT [FK_ConsultatieExplorareEfectuata_Pacient] FOREIGN KEY ([PacientID]) 
            REFERENCES [dbo].[Pacienti] ([Id]),
        CONSTRAINT [FK_ConsultatieExplorareEfectuata_Explorare] FOREIGN KEY ([ExplorareID]) 
            REFERENCES [dbo].[NomenclatorExplorariFunc] ([Id]),
        CONSTRAINT [FK_ConsultatieExplorareEfectuata_Recomandare] FOREIGN KEY ([RecomandareID]) 
            REFERENCES [dbo].[ConsultatieExplorareRecomandata] ([Id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_ConsultatieExplorareEfectuata_ConsultatieID] 
        ON [dbo].[ConsultatieExplorareEfectuata] ([ConsultatieID]);
    CREATE NONCLUSTERED INDEX [IX_ConsultatieExplorareEfectuata_PacientID] 
        ON [dbo].[ConsultatieExplorareEfectuata] ([PacientID]);
    CREATE NONCLUSTERED INDEX [IX_ConsultatieExplorareEfectuata_DataEfectuare] 
        ON [dbo].[ConsultatieExplorareEfectuata] ([DataEfectuare] DESC);
    
    PRINT '✓ ConsultatieExplorareEfectuata created'
END
ELSE
    PRINT '→ ConsultatieExplorareEfectuata already exists'
GO

-- ----------------------------------------------------------------------------
-- 3.3. Endoscopii Efectuate
-- ----------------------------------------------------------------------------
PRINT 'Creating ConsultatieEndoscopieEfectuata...'

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConsultatieEndoscopieEfectuata')
BEGIN
    CREATE TABLE [dbo].[ConsultatieEndoscopieEfectuata] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
        [PacientID] UNIQUEIDENTIFIER NOT NULL,
        [EndoscopieID] UNIQUEIDENTIFIER NULL, -- FK to NomenclatorEndoscopii
        [RecomandareID] UNIQUEIDENTIFIER NULL, -- FK to ConsultatieEndoscopieRecomandata
        [NumeEndoscopie] NVARCHAR(200) NOT NULL,
        [DataEfectuare] DATE NOT NULL,
        [Laborator] NVARCHAR(200) NULL,
        [MedicExecutant] NVARCHAR(200) NULL,
        [TipAnestezie] NVARCHAR(100) NULL, -- Fără, Locală, Sedare, Generală
        [DescriereProcedurii] NVARCHAR(MAX) NULL,
        [Rezultat] NVARCHAR(MAX) NULL,
        [BiopsiePrelevata] BIT NOT NULL DEFAULT 0,
        [LocBiopsie] NVARCHAR(200) NULL,
        [Concluzie] NVARCHAR(MAX) NULL,
        [Complicatii] NVARCHAR(MAX) NULL,
        [EsteAnormal] BIT NOT NULL DEFAULT 0,
        [Observatii] NVARCHAR(MAX) NULL,
        [CaleDocument] NVARCHAR(500) NULL,
        [CaleImagini] NVARCHAR(500) NULL, -- folder cu imagini de la procedură
        [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatDe] UNIQUEIDENTIFIER NOT NULL,
        [DataUltimeiModificari] DATETIME NULL,
        [ModificatDe] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_ConsultatieEndoscopieEfectuata] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ConsultatieEndoscopieEfectuata_Consultatie] FOREIGN KEY ([ConsultatieID]) 
            REFERENCES [dbo].[Consultatii] ([ConsultatieID]) ON DELETE CASCADE,
        CONSTRAINT [FK_ConsultatieEndoscopieEfectuata_Pacient] FOREIGN KEY ([PacientID]) 
            REFERENCES [dbo].[Pacienti] ([Id]),
        CONSTRAINT [FK_ConsultatieEndoscopieEfectuata_Endoscopie] FOREIGN KEY ([EndoscopieID]) 
            REFERENCES [dbo].[NomenclatorEndoscopii] ([Id]),
        CONSTRAINT [FK_ConsultatieEndoscopieEfectuata_Recomandare] FOREIGN KEY ([RecomandareID]) 
            REFERENCES [dbo].[ConsultatieEndoscopieRecomandata] ([Id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_ConsultatieEndoscopieEfectuata_ConsultatieID] 
        ON [dbo].[ConsultatieEndoscopieEfectuata] ([ConsultatieID]);
    CREATE NONCLUSTERED INDEX [IX_ConsultatieEndoscopieEfectuata_PacientID] 
        ON [dbo].[ConsultatieEndoscopieEfectuata] ([PacientID]);
    CREATE NONCLUSTERED INDEX [IX_ConsultatieEndoscopieEfectuata_DataEfectuare] 
        ON [dbo].[ConsultatieEndoscopieEfectuata] ([DataEfectuare] DESC);
    
    PRINT '✓ ConsultatieEndoscopieEfectuata created'
END
ELSE
    PRINT '→ ConsultatieEndoscopieEfectuata already exists'
GO

PRINT ''
PRINT '============================================================================'
PRINT 'TABELE CREATE PENTRU INVESTIGAȚII MEDICALE:'
PRINT '============================================================================'
PRINT '  NOMENCLATOARE:'
PRINT '    - NomenclatorInvestigatiiImagistice'
PRINT '    - NomenclatorExplorariFunc'
PRINT '    - NomenclatorEndoscopii'
PRINT ''
PRINT '  RECOMANDATE:'
PRINT '    - ConsultatieInvestigatieImagisticaRecomandata'
PRINT '    - ConsultatieExplorareRecomandata'
PRINT '    - ConsultatieEndoscopieRecomandata'
PRINT ''
PRINT '  EFECTUATE:'
PRINT '    - ConsultatieInvestigatieImagisticaEfectuata'
PRINT '    - ConsultatieExplorareEfectuata'
PRINT '    - ConsultatieEndoscopieEfectuata'
PRINT '============================================================================'
GO
