/*
==============================================================================
TABLE & STORED PROCEDURES: ConsultatieAnalizeMedicaleRecomandate
==============================================================================
Description: Analize medicale recomandate în timpul consultației
             Tabelă separată de ConsultatieAnalizeMedicale (pentru import)
Author: System
Date: 2026-01-05
Version: 1.0
==============================================================================
*/

USE [ValyanMed]
GO

-- ============================================================================
-- DROP existing objects if they exist
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieAnalizeMedicaleRecomandate_Create')
    DROP PROCEDURE [dbo].[ConsultatieAnalizeMedicaleRecomandate_Create]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieAnalizeMedicaleRecomandate_GetByConsultatieId')
    DROP PROCEDURE [dbo].[ConsultatieAnalizeMedicaleRecomandate_GetByConsultatieId]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieAnalizeMedicaleRecomandate_Delete')
    DROP PROCEDURE [dbo].[ConsultatieAnalizeMedicaleRecomandate_Delete]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieAnalizeMedicaleRecomandate_Update')
    DROP PROCEDURE [dbo].[ConsultatieAnalizeMedicaleRecomandate_Update]
GO

-- ============================================================================
-- CREATE TABLE: ConsultatieAnalizeMedicaleRecomandate
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConsultatieAnalizeMedicaleRecomandate]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ConsultatieAnalizeMedicaleRecomandate]
    (
        -- PRIMARY KEY
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        
        -- FOREIGN KEYS
        [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
        [AnalizaNomenclatorID] UNIQUEIDENTIFIER NULL, -- Link către nomenclatorul de analize
        
        -- DETALII ANALIZA
        [NumeAnaliza] NVARCHAR(200) NOT NULL,
        [CodAnaliza] NVARCHAR(50) NULL,
        [TipAnaliza] NVARCHAR(50) NOT NULL DEFAULT 'Laborator', -- Categoria: Laborator, Biochimie, Hematologie, etc.
        
        -- RECOMANDARE
        [DataRecomandare] DATETIME NOT NULL DEFAULT GETDATE(),
        [Prioritate] NVARCHAR(20) NULL, -- 'Normala', 'Urgent', 'Foarte urgent'
        [EsteCito] BIT NOT NULL DEFAULT 0,
        [IndicatiiClinice] NVARCHAR(1000) NULL,
        [ObservatiiMedic] NVARCHAR(1000) NULL,
        
        -- STATUS TRACKING
        [Status] NVARCHAR(30) NOT NULL DEFAULT 'Recomandata', -- 'Recomandata', 'Programata', 'Efectuata', 'Anulata'
        [DataProgramata] DATETIME NULL,
        
        -- AUDIT
        [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatDe] UNIQUEIDENTIFIER NOT NULL,
        [DataModificare] DATETIME NULL,
        [ModificatDe] UNIQUEIDENTIFIER NULL,
        
        CONSTRAINT [PK_ConsultatieAnalizeMedicaleRecomandate] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ConsultatieAnalizeMedicaleRecomandate_Consultatie] 
            FOREIGN KEY ([ConsultatieID]) REFERENCES [dbo].[Consultatii]([ConsultatieID]) ON DELETE CASCADE
    )
    
    -- Create indexes for performance
    CREATE NONCLUSTERED INDEX [IX_ConsultatieAnalizeMedicaleRecomandate_ConsultatieID] 
        ON [dbo].[ConsultatieAnalizeMedicaleRecomandate] ([ConsultatieID])
    
    CREATE NONCLUSTERED INDEX [IX_ConsultatieAnalizeMedicaleRecomandate_AnalizaNomenclatorID] 
        ON [dbo].[ConsultatieAnalizeMedicaleRecomandate] ([AnalizaNomenclatorID])
    
    PRINT 'Table ConsultatieAnalizeMedicaleRecomandate created successfully.'
END
ELSE
BEGIN
    PRINT 'Table ConsultatieAnalizeMedicaleRecomandate already exists.'
END
GO

-- ============================================================================
-- STORED PROCEDURE: Create
-- ============================================================================
CREATE PROCEDURE [dbo].[ConsultatieAnalizeMedicaleRecomandate_Create]
    @ConsultatieID UNIQUEIDENTIFIER,
    @AnalizaNomenclatorID UNIQUEIDENTIFIER = NULL,
    @NumeAnaliza NVARCHAR(200),
    @CodAnaliza NVARCHAR(50) = NULL,
    @TipAnaliza NVARCHAR(50) = 'Laborator',
    @Prioritate NVARCHAR(20) = NULL,
    @EsteCito BIT = 0,
    @IndicatiiClinice NVARCHAR(1000) = NULL,
    @ObservatiiMedic NVARCHAR(1000) = NULL,
    @CreatDe UNIQUEIDENTIFIER,
    @Id UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @Id = NEWID();
    
    INSERT INTO [dbo].[ConsultatieAnalizeMedicaleRecomandate]
    (
        [Id],
        [ConsultatieID],
        [AnalizaNomenclatorID],
        [NumeAnaliza],
        [CodAnaliza],
        [TipAnaliza],
        [DataRecomandare],
        [Prioritate],
        [EsteCito],
        [IndicatiiClinice],
        [ObservatiiMedic],
        [Status],
        [DataCreare],
        [CreatDe]
    )
    VALUES
    (
        @Id,
        @ConsultatieID,
        @AnalizaNomenclatorID,
        @NumeAnaliza,
        @CodAnaliza,
        @TipAnaliza,
        GETDATE(),
        @Prioritate,
        @EsteCito,
        @IndicatiiClinice,
        @ObservatiiMedic,
        'Recomandata',
        GETDATE(),
        @CreatDe
    );
    
    SELECT @Id AS Id;
END
GO

-- ============================================================================
-- STORED PROCEDURE: GetByConsultatieId
-- ============================================================================
CREATE PROCEDURE [dbo].[ConsultatieAnalizeMedicaleRecomandate_GetByConsultatieId]
    @ConsultatieID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.[Id],
        r.[ConsultatieID],
        r.[AnalizaNomenclatorID],
        r.[NumeAnaliza],
        r.[CodAnaliza],
        r.[TipAnaliza],
        r.[DataRecomandare],
        r.[Prioritate],
        r.[EsteCito],
        r.[IndicatiiClinice],
        r.[ObservatiiMedic],
        r.[Status],
        r.[DataProgramata],
        r.[DataCreare],
        r.[CreatDe]
    FROM [dbo].[ConsultatieAnalizeMedicaleRecomandate] r
    WHERE r.[ConsultatieID] = @ConsultatieID
    ORDER BY r.[DataRecomandare] DESC;
END
GO

-- ============================================================================
-- STORED PROCEDURE: Delete
-- ============================================================================
CREATE PROCEDURE [dbo].[ConsultatieAnalizeMedicaleRecomandate_Delete]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [dbo].[ConsultatieAnalizeMedicaleRecomandate]
    WHERE [Id] = @Id;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================================
-- STORED PROCEDURE: Update
-- ============================================================================
CREATE PROCEDURE [dbo].[ConsultatieAnalizeMedicaleRecomandate_Update]
    @Id UNIQUEIDENTIFIER,
    @Prioritate NVARCHAR(20) = NULL,
    @EsteCito BIT = 0,
    @IndicatiiClinice NVARCHAR(1000) = NULL,
    @ObservatiiMedic NVARCHAR(1000) = NULL,
    @Status NVARCHAR(30) = NULL,
    @DataProgramata DATETIME = NULL,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[ConsultatieAnalizeMedicaleRecomandate]
    SET 
        [Prioritate] = ISNULL(@Prioritate, [Prioritate]),
        [EsteCito] = @EsteCito,
        [IndicatiiClinice] = @IndicatiiClinice,
        [ObservatiiMedic] = @ObservatiiMedic,
        [Status] = ISNULL(@Status, [Status]),
        [DataProgramata] = @DataProgramata,
        [DataModificare] = GETDATE(),
        [ModificatDe] = @ModificatDe
    WHERE [Id] = @Id;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT 'All stored procedures created successfully.'
GO
