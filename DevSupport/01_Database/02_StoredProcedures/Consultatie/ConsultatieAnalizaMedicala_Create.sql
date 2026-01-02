/*
==============================================================================
STORED PROCEDURE: ConsultatieAnalizaMedicala_Create
==============================================================================
Description: Create a new medical analysis for a consultatie
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieAnalizaMedicala_Create')
    DROP PROCEDURE [dbo].[ConsultatieAnalizaMedicala_Create]
GO

CREATE PROCEDURE [dbo].[ConsultatieAnalizaMedicala_Create]
    @ConsultatieID UNIQUEIDENTIFIER,
    @TipAnaliza NVARCHAR(50),
    @NumeAnaliza NVARCHAR(200),
    @CodAnaliza NVARCHAR(50) = NULL,
    @StatusAnaliza NVARCHAR(50) = 'Recomandata',
    @DataRecomandare DATETIME = NULL,
    @Prioritate NVARCHAR(20) = NULL,
    @EsteCito BIT = 0,
    @IndicatiiClinice NVARCHAR(1000) = NULL,
    @ObservatiiRecomandare NVARCHAR(1000) = NULL,
    @CreatDe UNIQUEIDENTIFIER,
    @AnalizaID UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        SET @AnalizaID = NEWID();
        
        IF @DataRecomandare IS NULL
            SET @DataRecomandare = GETDATE();
        
        INSERT INTO [dbo].[ConsultatieAnalizeMedicale]
        (
            [Id],
            [ConsultatieID],
            [TipAnaliza],
            [NumeAnaliza],
            [CodAnaliza],
            [StatusAnaliza],
            [DataRecomandare],
            [Prioritate],
            [EsteCito],
            [IndicatiiClinice],
            [ObservatiiRecomandare],
            [AreRezultate],
            [EsteInAfaraLimitelor],
            [Decontat],
            [DataCreare],
            [CreatDe]
        )
        VALUES
        (
            @AnalizaID,
            @ConsultatieID,
            @TipAnaliza,
            @NumeAnaliza,
            @CodAnaliza,
            @StatusAnaliza,
            @DataRecomandare,
            @Prioritate,
            @EsteCito,
            @IndicatiiClinice,
            @ObservatiiRecomandare,
            0,
            0,
            0,
            GETDATE(),
            @CreatDe
        );
        
        UPDATE [dbo].[Consultatii]
        SET [DataUltimeiModificari] = GETDATE(), [ModificatDe] = @CreatDe
        WHERE [ConsultatieID] = @ConsultatieID;
        
        COMMIT TRANSACTION;
        
        SELECT * FROM [dbo].[ConsultatieAnalizeMedicale] WHERE [Id] = @AnalizaID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
