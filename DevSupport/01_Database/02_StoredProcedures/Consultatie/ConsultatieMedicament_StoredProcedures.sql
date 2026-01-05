/*
==============================================================================
STORED PROCEDURES: ConsultatieMedicament
==============================================================================
Description: CRUD operations for ConsultatieMedicament table
Author: System
Date: 2026-01-04
Version: 1.0
==============================================================================
*/

USE [ValyanMed]
GO

-- ============================================================================
-- 1. GET BY CONSULTATIE ID
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieMedicament_GetByConsultatieId')
    DROP PROCEDURE [dbo].[ConsultatieMedicament_GetByConsultatieId]
GO

CREATE PROCEDURE [dbo].[ConsultatieMedicament_GetByConsultatieId]
    @ConsultatieID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [ConsultatieID],
        [OrdineAfisare],
        [NumeMedicament],
        [Doza],
        [Frecventa],
        [Durata],
        [Cantitate],
        [Observatii],
        [DataCreare],
        [CreatDe]
    FROM [dbo].[ConsultatieMedicament]
    WHERE [ConsultatieID] = @ConsultatieID
    ORDER BY [OrdineAfisare] ASC;
END
GO

PRINT 'Created: ConsultatieMedicament_GetByConsultatieId';
GO

-- ============================================================================
-- 2. DELETE BY CONSULTATIE ID (delete all medications for a consultatie)
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieMedicament_DeleteByConsultatieId')
    DROP PROCEDURE [dbo].[ConsultatieMedicament_DeleteByConsultatieId]
GO

CREATE PROCEDURE [dbo].[ConsultatieMedicament_DeleteByConsultatieId]
    @ConsultatieID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [dbo].[ConsultatieMedicament]
    WHERE [ConsultatieID] = @ConsultatieID;
    
    SELECT @@ROWCOUNT AS DeletedCount;
END
GO

PRINT 'Created: ConsultatieMedicament_DeleteByConsultatieId';
GO

-- ============================================================================
-- 3. INSERT SINGLE MEDICATION
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieMedicament_Insert')
    DROP PROCEDURE [dbo].[ConsultatieMedicament_Insert]
GO

CREATE PROCEDURE [dbo].[ConsultatieMedicament_Insert]
    @ConsultatieID UNIQUEIDENTIFIER,
    @OrdineAfisare INT,
    @NumeMedicament NVARCHAR(500),
    @Doza NVARCHAR(100) = NULL,
    @Frecventa NVARCHAR(200) = NULL,
    @Durata NVARCHAR(100) = NULL,
    @Cantitate NVARCHAR(100) = NULL,
    @Observatii NVARCHAR(500) = NULL,
    @CreatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [dbo].[ConsultatieMedicament]
    (
        [Id], [ConsultatieID], [OrdineAfisare],
        [NumeMedicament], [Doza], [Frecventa], [Durata], [Cantitate], [Observatii],
        [DataCreare], [CreatDe]
    )
    VALUES
    (
        @NewId, @ConsultatieID, @OrdineAfisare,
        @NumeMedicament, @Doza, @Frecventa, @Durata, @Cantitate, @Observatii,
        GETDATE(), @CreatDe
    );
    
    SELECT @NewId AS Id;
END
GO

PRINT 'Created: ConsultatieMedicament_Insert';
GO

-- ============================================================================
-- 4. BULK REPLACE (MERGE logic - preserves CreatDe/DataCreare for existing)
-- Using TVP (Table-Valued Parameter) for efficiency
-- ============================================================================

-- Drop and recreate TVP type (must drop SP first)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieMedicament_BulkReplace')
    DROP PROCEDURE [dbo].[ConsultatieMedicament_BulkReplace]
GO

IF TYPE_ID('dbo.MedicamentListType') IS NOT NULL
    DROP TYPE [dbo].[MedicamentListType];
GO

CREATE TYPE [dbo].[MedicamentListType] AS TABLE
(
    [Id] UNIQUEIDENTIFIER NULL,  -- NULL = new medication, NOT NULL = existing
    [OrdineAfisare] INT NOT NULL,
    [NumeMedicament] NVARCHAR(500) NOT NULL,
    [Doza] NVARCHAR(100) NULL,
    [Frecventa] NVARCHAR(200) NULL,
    [Durata] NVARCHAR(100) NULL,
    [Cantitate] NVARCHAR(100) NULL,
    [Observatii] NVARCHAR(500) NULL
);
GO

PRINT 'Created: MedicamentListType (TVP) with Id column';
GO

CREATE PROCEDURE [dbo].[ConsultatieMedicament_BulkReplace]
    @ConsultatieID UNIQUEIDENTIFIER,
    @Medicamente MedicamentListType READONLY,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- 1. DELETE medications that are no longer in the list
        DELETE FROM [dbo].[ConsultatieMedicament]
        WHERE [ConsultatieID] = @ConsultatieID
          AND [Id] NOT IN (SELECT [Id] FROM @Medicamente WHERE [Id] IS NOT NULL);
        
        -- 2. UPDATE existing medications ONLY if values changed (preserves DataCreare, CreatDe)
        UPDATE cm
        SET 
            cm.[OrdineAfisare] = m.[OrdineAfisare],
            cm.[NumeMedicament] = m.[NumeMedicament],
            cm.[Doza] = m.[Doza],
            cm.[Frecventa] = m.[Frecventa],
            cm.[Durata] = m.[Durata],
            cm.[Cantitate] = m.[Cantitate],
            cm.[Observatii] = m.[Observatii],
            cm.[DataUltimeiModificari] = GETDATE(),
            cm.[ModificatDe] = @ModificatDe
        FROM [dbo].[ConsultatieMedicament] cm
        INNER JOIN @Medicamente m ON cm.[Id] = m.[Id]
        WHERE cm.[ConsultatieID] = @ConsultatieID
          AND m.[Id] IS NOT NULL
          AND (
              ISNULL(cm.[OrdineAfisare], 0) <> ISNULL(m.[OrdineAfisare], 0)
              OR ISNULL(cm.[NumeMedicament], '') <> ISNULL(m.[NumeMedicament], '')
              OR ISNULL(cm.[Doza], '') <> ISNULL(m.[Doza], '')
              OR ISNULL(cm.[Frecventa], '') <> ISNULL(m.[Frecventa], '')
              OR ISNULL(cm.[Durata], '') <> ISNULL(m.[Durata], '')
              OR ISNULL(cm.[Cantitate], '') <> ISNULL(m.[Cantitate], '')
              OR ISNULL(cm.[Observatii], '') <> ISNULL(m.[Observatii], '')
          );
        
        -- 3. INSERT new medications (Id IS NULL in TVP)
        INSERT INTO [dbo].[ConsultatieMedicament]
        (
            [Id], [ConsultatieID], [OrdineAfisare],
            [NumeMedicament], [Doza], [Frecventa], [Durata], [Cantitate], [Observatii],
            [DataCreare], [CreatDe], [DataUltimeiModificari], [ModificatDe]
        )
        SELECT 
            NEWID(),
            @ConsultatieID,
            [OrdineAfisare],
            [NumeMedicament],
            [Doza],
            [Frecventa],
            [Durata],
            [Cantitate],
            [Observatii],
            GETDATE(),
            @ModificatDe,
            NULL,  -- DataUltimeiModificari = NULL for new records
            NULL   -- ModificatDe = NULL for new records
        FROM @Medicamente
        WHERE [Id] IS NULL
          AND [NumeMedicament] IS NOT NULL 
          AND LEN(LTRIM(RTRIM([NumeMedicament]))) > 0;
        
        -- Update consultatie timestamp
        UPDATE [dbo].[Consultatii]
        SET [DataUltimeiModificari] = GETDATE(), [ModificatDe] = @ModificatDe
        WHERE [ConsultatieID] = @ConsultatieID;
        
        COMMIT TRANSACTION;
        
        SELECT @@ROWCOUNT AS InsertedCount;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Created: ConsultatieMedicament_BulkReplace';
GO

-- ============================================================================
-- VERIFICATION
-- ============================================================================
PRINT '=== All ConsultatieMedicament Stored Procedures Created ===';
SELECT name, type_desc, create_date
FROM sys.objects
WHERE type = 'P' AND name LIKE 'ConsultatieMedicament%'
ORDER BY name;
GO
