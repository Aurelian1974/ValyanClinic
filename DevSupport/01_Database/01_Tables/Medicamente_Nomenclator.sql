-- =============================================
-- Tabel: Medicamente_Nomenclator
-- Descriere: Import din Nomenclatorul ANM (Agenția Națională a Medicamentului)
-- Sursă: https://nomenclator.anm.ro/files/nomenclator.xlsx
-- Actualizare: Automată (zilnic/săptămânal)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Medicamente_Nomenclator]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Medicamente_Nomenclator]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        
        -- Date principale din nomenclator
        [CodANM] NVARCHAR(20) NOT NULL,              -- Cod unic ANM (ex: W70067001)
        [NumeComercial] NVARCHAR(500) NOT NULL,       -- Nume comercial (ex: ABACAVIR AUROBINDO 300 mg)
        [DCI] NVARCHAR(500) NULL,                     -- Denumire Comună Internațională (ex: ABACAVIRUM)
        [FormaFarmaceutica] NVARCHAR(200) NULL,       -- Forma (ex: COMPR. FILM., SOL. INJ.)
        [CodATC] NVARCHAR(20) NULL,                   -- Cod clasificare ATC (ex: J05AF06)
        [Concentratie] NVARCHAR(200) NULL,            -- Concentrație/Doză
        [Producator] NVARCHAR(500) NULL,              -- Producător
        [TaraProducator] NVARCHAR(100) NULL,          -- Țara producătorului
        
        -- Informații suplimentare (dacă sunt disponibile în Excel)
        [PrescriptieMedicala] BIT NULL DEFAULT 1,     -- Necesită prescripție
        [Compensat] BIT NULL DEFAULT 0,               -- Este pe lista de compensate
        [ProcentCompensare] INT NULL,                 -- Procent compensare (0, 50, 90, 100)
        
        -- Metadate sincronizare
        [DataImport] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [DataUltimaActualizare] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [SursaFisier] NVARCHAR(500) NULL,             -- Numele fișierului sursă
        [Activ] BIT NOT NULL DEFAULT 1,               -- Flag pentru soft-delete
        
        CONSTRAINT [PK_Medicamente_Nomenclator] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Medicamente_CodANM] UNIQUE NONCLUSTERED ([CodANM])
    )
END
GO

-- Index pentru căutare rapidă după nume (autocomplete)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Medicamente_NumeComercial' AND object_id = OBJECT_ID('Medicamente_Nomenclator'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Medicamente_NumeComercial] 
    ON [dbo].[Medicamente_Nomenclator] ([NumeComercial])
    INCLUDE ([DCI], [FormaFarmaceutica], [CodATC])
END
GO

-- Index pentru căutare după DCI (substanță activă)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Medicamente_DCI' AND object_id = OBJECT_ID('Medicamente_Nomenclator'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Medicamente_DCI] 
    ON [dbo].[Medicamente_Nomenclator] ([DCI])
    INCLUDE ([NumeComercial], [FormaFarmaceutica])
END
GO

-- Index pentru căutare după cod ATC (grupare terapeutică)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Medicamente_CodATC' AND object_id = OBJECT_ID('Medicamente_Nomenclator'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Medicamente_CodATC] 
    ON [dbo].[Medicamente_Nomenclator] ([CodATC])
END
GO

-- Full-text index pentru căutare avansată (opțional)
-- Necesită Full-Text Search activat pe server
/*
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Medicamente_Nomenclator'))
BEGIN
    CREATE FULLTEXT CATALOG [MedicamenteCatalog] AS DEFAULT;
    CREATE FULLTEXT INDEX ON [dbo].[Medicamente_Nomenclator]
    (
        [NumeComercial] LANGUAGE 1048,  -- Romanian
        [DCI] LANGUAGE 1048
    )
    KEY INDEX [PK_Medicamente_Nomenclator]
    ON [MedicamenteCatalog];
END
*/
GO

-- Tabel pentru log-ul sincronizărilor
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Medicamente_SyncLog]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Medicamente_SyncLog]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [DataStart] DATETIME2 NOT NULL,
        [DataEnd] DATETIME2 NULL,
        [Status] NVARCHAR(50) NOT NULL,              -- 'InProgress', 'Success', 'Failed'
        [TotalRecords] INT NULL,
        [RecordsAdded] INT NULL,
        [RecordsUpdated] INT NULL,
        [RecordsDeactivated] INT NULL,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [SursaURL] NVARCHAR(500) NULL,
        [NumeFisier] NVARCHAR(200) NULL,
        [DimensiuneFisier] BIGINT NULL,
        
        CONSTRAINT [PK_Medicamente_SyncLog] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

PRINT 'Tabelele Medicamente_Nomenclator și Medicamente_SyncLog au fost create/actualizate.'
GO
