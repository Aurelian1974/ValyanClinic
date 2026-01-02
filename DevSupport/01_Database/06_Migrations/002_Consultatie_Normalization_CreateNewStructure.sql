/*
==============================================================================
MIGRATION: Consultatie Normalization - Phase 2: Create New Normalized Structure
==============================================================================
Description: Create new normalized tables for Consultatie management
Author: System
Date: 2026-01-02
Version: 1.0

New Structure:
- Consultatii (MASTER - core data only)
- ConsultatieMotivePrezentare (1:1)
- ConsultatieAntecedente (1:1)
- ConsultatieExamenObiectiv (1:1)
- ConsultatieInvestigatii (1:1)
- ConsultatieAnalizeMedicale (1:N)
- ConsultatieAnalizaDetalii (1:N per Analiza)
- ConsultatieDiagnostic (1:1)
- ConsultatieTratament (1:1)
- ConsultatieConcluzii (1:1)
==============================================================================
*/

USE [ValyanClinicDB]
GO

PRINT '========================================='
PRINT 'Starting Consultatie Normalization Phase 2'
PRINT 'Creating normalized table structure...'
PRINT '========================================='
PRINT ''

-- ==================== MASTER TABLE: Consultatii ====================
PRINT 'Creating table: Consultatii (MASTER)'
GO

CREATE TABLE [dbo].[Consultatii]
(
    [ConsultatieID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Keys
    [ProgramareID] UNIQUEIDENTIFIER NULL,
    [PacientID] UNIQUEIDENTIFIER NOT NULL,
    [MedicID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Date Consultatie
    [DataConsultatie] DATETIME NOT NULL,
    [OraConsultatie] TIME NOT NULL,
    [TipConsultatie] NVARCHAR(50) NOT NULL DEFAULT 'Prima consultatie',
    
    -- Status & Workflow
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'In desfasurare',
    [DataFinalizare] DATETIME NULL,
    [DurataMinute] INT NOT NULL DEFAULT 0,
    
    -- Audit
    [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatDe] UNIQUEIDENTIFIER NOT NULL,
    [DataUltimeiModificari] DATETIME NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT [PK_Consultatii] PRIMARY KEY CLUSTERED ([ConsultatieID] ASC)
)
GO

PRINT 'Table Consultatii created successfully!'
PRINT ''

-- ==================== ConsultatieMotivePrezentare (1:1) ====================
PRINT 'Creating table: ConsultatieMotivePrezentare'
GO

CREATE TABLE [dbo].[ConsultatieMotivePrezentare]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Motive Prezentare
    [MotivPrezentare] NVARCHAR(MAX) NULL,
    [IstoricBoalaActuala] NVARCHAR(MAX) NULL,
    
    -- Audit
    [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatDe] UNIQUEIDENTIFIER NOT NULL,
    [DataUltimeiModificari] DATETIME NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT [PK_ConsultatieMotivePrezentare] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ConsultatieMotivePrezentare_Consultatie] 
        FOREIGN KEY ([ConsultatieID]) REFERENCES [dbo].[Consultatii]([ConsultatieID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ConsultatieMotivePrezentare_ConsultatieID] UNIQUE ([ConsultatieID])
)
GO

CREATE NONCLUSTERED INDEX [IX_ConsultatieMotivePrezentare_ConsultatieID] 
ON [dbo].[ConsultatieMotivePrezentare]([ConsultatieID])
GO

PRINT 'Table ConsultatieMotivePrezentare created successfully!'
PRINT ''

-- ==================== ConsultatieAntecedente (1:1) ====================
PRINT 'Creating table: ConsultatieAntecedente'
GO

CREATE TABLE [dbo].[ConsultatieAntecedente]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Antecedente Heredo-Colaterale (AHC)
    [AHC_Mama] NVARCHAR(MAX) NULL,
    [AHC_Tata] NVARCHAR(MAX) NULL,
    [AHC_Frati] NVARCHAR(MAX) NULL,
    [AHC_Bunici] NVARCHAR(MAX) NULL,
    [AHC_Altele] NVARCHAR(MAX) NULL,
    
    -- Antecedente Fiziologice (AF)
    [AF_Nastere] NVARCHAR(MAX) NULL,
    [AF_Dezvoltare] NVARCHAR(MAX) NULL,
    [AF_Menstruatie] NVARCHAR(MAX) NULL,
    [AF_Sarcini] NVARCHAR(MAX) NULL,
    [AF_Alaptare] NVARCHAR(MAX) NULL,
    
    -- Antecedente Personale Patologice (APP)
    [APP_BoliCopilarieAdolescenta] NVARCHAR(MAX) NULL,
    [APP_BoliAdult] NVARCHAR(MAX) NULL,
    [APP_Interventii] NVARCHAR(MAX) NULL,
    [APP_Traumatisme] NVARCHAR(MAX) NULL,
    [APP_Transfuzii] NVARCHAR(MAX) NULL,
    [APP_Alergii] NVARCHAR(MAX) NULL,
    [APP_Medicatie] NVARCHAR(MAX) NULL,
    
    -- Conditii Socio-Economice
    [Profesie] NVARCHAR(500) NULL,
    [ConditiiLocuinta] NVARCHAR(MAX) NULL,
    [ConditiiMunca] NVARCHAR(MAX) NULL,
    [ObiceiuriAlimentare] NVARCHAR(MAX) NULL,
    [Toxice] NVARCHAR(MAX) NULL,
    
    -- Audit
    [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatDe] UNIQUEIDENTIFIER NOT NULL,
    [DataUltimeiModificari] DATETIME NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT [PK_ConsultatieAntecedente] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ConsultatieAntecedente_Consultatie] 
        FOREIGN KEY ([ConsultatieID]) REFERENCES [dbo].[Consultatii]([ConsultatieID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ConsultatieAntecedente_ConsultatieID] UNIQUE ([ConsultatieID])
)
GO

CREATE NONCLUSTERED INDEX [IX_ConsultatieAntecedente_ConsultatieID] 
ON [dbo].[ConsultatieAntecedente]([ConsultatieID])
GO

PRINT 'Table ConsultatieAntecedente created successfully!'
PRINT ''

-- ==================== ConsultatieExamenObiectiv (1:1) ====================
PRINT 'Creating table: ConsultatieExamenObiectiv'
GO

CREATE TABLE [dbo].[ConsultatieExamenObiectiv]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Examen General
    [StareGenerala] NVARCHAR(MAX) NULL,
    [Constitutie] NVARCHAR(500) NULL,
    [Atitudine] NVARCHAR(500) NULL,
    [Facies] NVARCHAR(500) NULL,
    [Tegumente] NVARCHAR(MAX) NULL,
    [Mucoase] NVARCHAR(MAX) NULL,
    [GangliniLimfatici] NVARCHAR(MAX) NULL,
    [Edeme] NVARCHAR(MAX) NULL,
    
    -- Semne Vitale
    [Greutate] DECIMAL(6,2) NULL,
    [Inaltime] DECIMAL(6,2) NULL,
    [IMC] DECIMAL(5,2) NULL,
    [Temperatura] DECIMAL(4,2) NULL,
    [TensiuneArteriala] NVARCHAR(50) NULL,
    [Puls] INT NULL,
    [FreccventaRespiratorie] INT NULL,
    [SaturatieO2] INT NULL,
    [Glicemie] DECIMAL(6,2) NULL,
    
    -- Examen pe Aparate/Sisteme
    [ExamenCardiovascular] NVARCHAR(MAX) NULL,
    [ExamenRespiratoriu] NVARCHAR(MAX) NULL,
    [ExamenDigestiv] NVARCHAR(MAX) NULL,
    [ExamenUrinar] NVARCHAR(MAX) NULL,
    [ExamenNervos] NVARCHAR(MAX) NULL,
    [ExamenLocomotor] NVARCHAR(MAX) NULL,
    [ExamenEndocrin] NVARCHAR(MAX) NULL,
    [ExamenORL] NVARCHAR(MAX) NULL,
    [ExamenOftalmologic] NVARCHAR(MAX) NULL,
    [ExamenDermatologic] NVARCHAR(MAX) NULL,
    
    -- Audit
    [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatDe] UNIQUEIDENTIFIER NOT NULL,
    [DataUltimeiModificari] DATETIME NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT [PK_ConsultatieExamenObiectiv] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ConsultatieExamenObiectiv_Consultatie] 
        FOREIGN KEY ([ConsultatieID]) REFERENCES [dbo].[Consultatii]([ConsultatieID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ConsultatieExamenObiectiv_ConsultatieID] UNIQUE ([ConsultatieID])
)
GO

CREATE NONCLUSTERED INDEX [IX_ConsultatieExamenObiectiv_ConsultatieID] 
ON [dbo].[ConsultatieExamenObiectiv]([ConsultatieID])
GO

PRINT 'Table ConsultatieExamenObiectiv created successfully!'
PRINT ''

-- ==================== ConsultatieInvestigatii (1:1) ====================
PRINT 'Creating table: ConsultatieInvestigatii'
GO

CREATE TABLE [dbo].[ConsultatieInvestigatii]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Investigatii Efectuate
    [InvestigatiiLaborator] NVARCHAR(MAX) NULL,
    [InvestigatiiImagistice] NVARCHAR(MAX) NULL,
    [InvestigatiiEKG] NVARCHAR(MAX) NULL,
    [AlteInvestigatii] NVARCHAR(MAX) NULL,
    
    -- Audit
    [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatDe] UNIQUEIDENTIFIER NOT NULL,
    [DataUltimeiModificari] DATETIME NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT [PK_ConsultatieInvestigatii] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ConsultatieInvestigatii_Consultatie] 
        FOREIGN KEY ([ConsultatieID]) REFERENCES [dbo].[Consultatii]([ConsultatieID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ConsultatieInvestigatii_ConsultatieID] UNIQUE ([ConsultatieID])
)
GO

CREATE NONCLUSTERED INDEX [IX_ConsultatieInvestigatii_ConsultatieID] 
ON [dbo].[ConsultatieInvestigatii]([ConsultatieID])
GO

PRINT 'Table ConsultatieInvestigatii created successfully!'
PRINT ''

-- ==================== ConsultatieAnalizeMedicale (1:N) ====================
PRINT 'Creating table: ConsultatieAnalizeMedicale'
GO

CREATE TABLE [dbo].[ConsultatieAnalizeMedicale]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Tipul Analizei
    [TipAnaliza] NVARCHAR(50) NOT NULL,
    [NumeAnaliza] NVARCHAR(200) NOT NULL,
    [CodAnaliza] NVARCHAR(50) NULL,
    
    -- Status
    [StatusAnaliza] NVARCHAR(50) NOT NULL DEFAULT 'Recomandata',
    
    -- Date Programare/Efectuare
    [DataRecomandare] DATETIME NOT NULL,
    [DataProgramata] DATETIME NULL,
    [DataEfectuare] DATETIME NULL,
    [LocEfectuare] NVARCHAR(200) NULL,
    
    -- Prioritate si Urgenta
    [Prioritate] NVARCHAR(20) NULL,
    [EsteCito] BIT NOT NULL DEFAULT 0,
    
    -- Indicatii Clinice
    [IndicatiiClinice] NVARCHAR(1000) NULL,
    [ObservatiiRecomandare] NVARCHAR(1000) NULL,
    
    -- Rezultate
    [AreRezultate] BIT NOT NULL DEFAULT 0,
    [DataRezultate] DATETIME NULL,
    [ValoareRezultat] NVARCHAR(500) NULL,
    [UnitatiMasura] NVARCHAR(50) NULL,
    [ValoareNormalaMin] DECIMAL(18,3) NULL,
    [ValoareNormalaMax] DECIMAL(18,3) NULL,
    [EsteInAfaraLimitelor] BIT NOT NULL DEFAULT 0,
    
    -- Interpretare Medicala
    [InterpretareMedic] NVARCHAR(2000) NULL,
    [ConclusiiAnaliza] NVARCHAR(2000) NULL,
    
    -- Documente Atasate
    [CaleFisierRezultat] NVARCHAR(500) NULL,
    [TipFisier] NVARCHAR(50) NULL,
    [DimensiuneFisier] BIGINT NULL,
    
    -- Costuri
    [Pret] DECIMAL(10,2) NULL,
    [Decontat] BIT NOT NULL DEFAULT 0,
    
    -- Link catre Alte Entitati
    [LaboratorID] UNIQUEIDENTIFIER NULL,
    [MedicInterpretareID] UNIQUEIDENTIFIER NULL,
    
    -- Audit
    [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatDe] UNIQUEIDENTIFIER NOT NULL,
    [DataUltimeiModificari] DATETIME NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT [PK_ConsultatieAnalizeMedicale] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ConsultatieAnalizeMedicale_Consultatie] 
        FOREIGN KEY ([ConsultatieID]) REFERENCES [dbo].[Consultatii]([ConsultatieID]) ON DELETE CASCADE
)
GO

CREATE NONCLUSTERED INDEX [IX_ConsultatieAnalizeMedicale_ConsultatieID] 
ON [dbo].[ConsultatieAnalizeMedicale]([ConsultatieID])
GO

CREATE NONCLUSTERED INDEX [IX_ConsultatieAnalizeMedicale_StatusAnaliza] 
ON [dbo].[ConsultatieAnalizeMedicale]([StatusAnaliza])
GO

CREATE NONCLUSTERED INDEX [IX_ConsultatieAnalizeMedicale_DataProgramata] 
ON [dbo].[ConsultatieAnalizeMedicale]([DataProgramata])
GO

PRINT 'Table ConsultatieAnalizeMedicale created successfully!'
PRINT ''

-- ==================== ConsultatieAnalizaDetalii (1:N per Analiza) ====================
PRINT 'Creating table: ConsultatieAnalizaDetalii'
GO

CREATE TABLE [dbo].[ConsultatieAnalizaDetalii]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [AnalizaMedicalaID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Parametru Individual
    [NumeParametru] NVARCHAR(200) NOT NULL,
    [CodParametru] NVARCHAR(50) NULL,
    
    -- Valoare
    [Valoare] NVARCHAR(200) NOT NULL,
    [UnitatiMasura] NVARCHAR(50) NULL,
    [TipValoare] NVARCHAR(20) NULL,
    
    -- Limite Normale
    [ValoareNormalaMin] DECIMAL(18,3) NULL,
    [ValoareNormalaMax] DECIMAL(18,3) NULL,
    [ValoareNormalaText] NVARCHAR(200) NULL,
    
    -- Status
    [EsteAnormal] BIT NOT NULL DEFAULT 0,
    [NivelGravitate] NVARCHAR(20) NULL,
    
    -- Interpretare
    [Observatii] NVARCHAR(500) NULL,
    
    -- Audit
    [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_ConsultatieAnalizaDetalii] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ConsultatieAnalizaDetalii_Analiza] 
        FOREIGN KEY ([AnalizaMedicalaID]) REFERENCES [dbo].[ConsultatieAnalizeMedicale]([Id]) ON DELETE CASCADE
)
GO

CREATE NONCLUSTERED INDEX [IX_ConsultatieAnalizaDetalii_AnalizaMedicalaID] 
ON [dbo].[ConsultatieAnalizaDetalii]([AnalizaMedicalaID])
GO

PRINT 'Table ConsultatieAnalizaDetalii created successfully!'
PRINT ''

-- ==================== ConsultatieDiagnostic (1:1) ====================
PRINT 'Creating table: ConsultatieDiagnostic'
GO

CREATE TABLE [dbo].[ConsultatieDiagnostic]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Diagnostic
    [DiagnosticPozitiv] NVARCHAR(MAX) NULL,
    [DiagnosticDiferential] NVARCHAR(MAX) NULL,
    [DiagnosticEtiologic] NVARCHAR(MAX) NULL,
    [CoduriICD10] NVARCHAR(200) NULL,
    [CoduriICD10Secundare] NVARCHAR(500) NULL,
    
    -- Audit
    [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatDe] UNIQUEIDENTIFIER NOT NULL,
    [DataUltimeiModificari] DATETIME NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT [PK_ConsultatieDiagnostic] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ConsultatieDiagnostic_Consultatie] 
        FOREIGN KEY ([ConsultatieID]) REFERENCES [dbo].[Consultatii]([ConsultatieID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ConsultatieDiagnostic_ConsultatieID] UNIQUE ([ConsultatieID])
)
GO

CREATE NONCLUSTERED INDEX [IX_ConsultatieDiagnostic_ConsultatieID] 
ON [dbo].[ConsultatieDiagnostic]([ConsultatieID])
GO

PRINT 'Table ConsultatieDiagnostic created successfully!'
PRINT ''

-- ==================== ConsultatieTratament (1:1) ====================
PRINT 'Creating table: ConsultatieTratament'
GO

CREATE TABLE [dbo].[ConsultatieTratament]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Tratament
    [TratamentMedicamentos] NVARCHAR(MAX) NULL,
    [TratamentNemedicamentos] NVARCHAR(MAX) NULL,
    [RecomandariDietetice] NVARCHAR(MAX) NULL,
    [RecomandariRegimViata] NVARCHAR(MAX) NULL,
    
    -- Recomandari
    [InvestigatiiRecomandate] NVARCHAR(MAX) NULL,
    [ConsulturiSpecialitate] NVARCHAR(MAX) NULL,
    [DataUrmatoareiProgramari] NVARCHAR(200) NULL,
    [RecomandariSupraveghere] NVARCHAR(MAX) NULL,
    
    -- Audit
    [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatDe] UNIQUEIDENTIFIER NOT NULL,
    [DataUltimeiModificari] DATETIME NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT [PK_ConsultatieTratament] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ConsultatieTratament_Consultatie] 
        FOREIGN KEY ([ConsultatieID]) REFERENCES [dbo].[Consultatii]([ConsultatieID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ConsultatieTratament_ConsultatieID] UNIQUE ([ConsultatieID])
)
GO

CREATE NONCLUSTERED INDEX [IX_ConsultatieTratament_ConsultatieID] 
ON [dbo].[ConsultatieTratament]([ConsultatieID])
GO

PRINT 'Table ConsultatieTratament created successfully!'
PRINT ''

-- ==================== ConsultatieConcluzii (1:1) ====================
PRINT 'Creating table: ConsultatieConcluzii'
GO

CREATE TABLE [dbo].[ConsultatieConcluzii]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Prognostic
    [Prognostic] NVARCHAR(50) NULL,
    
    -- Concluzie
    [Concluzie] NVARCHAR(MAX) NULL,
    
    -- Observatii Suplimentare
    [ObservatiiMedic] NVARCHAR(MAX) NULL,
    [NotePacient] NVARCHAR(MAX) NULL,
    
    -- Documente Anexate
    [DocumenteAtatate] NVARCHAR(MAX) NULL,
    
    -- Audit
    [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatDe] UNIQUEIDENTIFIER NOT NULL,
    [DataUltimeiModificari] DATETIME NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT [PK_ConsultatieConcluzii] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ConsultatieConcluzii_Consultatie] 
        FOREIGN KEY ([ConsultatieID]) REFERENCES [dbo].[Consultatii]([ConsultatieID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ConsultatieConcluzii_ConsultatieID] UNIQUE ([ConsultatieID])
)
GO

CREATE NONCLUSTERED INDEX [IX_ConsultatieConcluzii_ConsultatieID] 
ON [dbo].[ConsultatieConcluzii]([ConsultatieID])
GO

PRINT 'Table ConsultatieConcluzii created successfully!'
PRINT ''

-- ==================== Additional Indexes on MASTER Table ====================
PRINT 'Creating indexes on Consultatii master table...'
GO

CREATE NONCLUSTERED INDEX [IX_Consultatii_PacientID] 
ON [dbo].[Consultatii]([PacientID])
GO

CREATE NONCLUSTERED INDEX [IX_Consultatii_MedicID] 
ON [dbo].[Consultatii]([MedicID])
GO

CREATE NONCLUSTERED INDEX [IX_Consultatii_DataConsultatie] 
ON [dbo].[Consultatii]([DataConsultatie])
GO

CREATE NONCLUSTERED INDEX [IX_Consultatii_Status] 
ON [dbo].[Consultatii]([Status])
GO

PRINT 'Indexes created successfully!'
PRINT ''

PRINT '========================================='
PRINT 'Phase 2 Complete!'
PRINT 'Normalized structure created successfully.'
PRINT ''
PRINT 'Tables created:'
PRINT '  - Consultatii (MASTER)'
PRINT '  - ConsultatieMotivePrezentare (1:1)'
PRINT '  - ConsultatieAntecedente (1:1)'
PRINT '  - ConsultatieExamenObiectiv (1:1)'
PRINT '  - ConsultatieInvestigatii (1:1)'
PRINT '  - ConsultatieAnalizeMedicale (1:N)'
PRINT '  - ConsultatieAnalizaDetalii (1:N)'
PRINT '  - ConsultatieDiagnostic (1:1)'
PRINT '  - ConsultatieTratament (1:1)'
PRINT '  - ConsultatieConcluzii (1:1)'
PRINT ''
PRINT 'Ready for stored procedures creation.'
PRINT '========================================='
GO
