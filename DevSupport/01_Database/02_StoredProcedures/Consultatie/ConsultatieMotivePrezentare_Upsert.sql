/*
==============================================================================
STORED PROCEDURE: ConsultatieMotivePrezentare_Upsert
==============================================================================
Description: Insert or Update motive prezentare for a consultatie
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)

Usage:
    EXEC ConsultatieMotivePrezentare_Upsert
        @ConsultatieID = '...',
        @MotivPrezentare = '...',
        @IstoricBoalaActuala = '...',
        @ModificatDe = '...'
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieMotivePrezentare_Upsert')
    DROP PROCEDURE [dbo].[ConsultatieMotivePrezentare_Upsert]
GO

CREATE PROCEDURE [dbo].[ConsultatieMotivePrezentare_Upsert]
    @ConsultatieID UNIQUEIDENTIFIER,
    @MotivPrezentare NVARCHAR(MAX) = NULL,
    @IstoricBoalaActuala NVARCHAR(MAX) = NULL,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if record exists
        IF EXISTS (SELECT 1 FROM [dbo].[ConsultatieMotivePrezentare] WHERE [ConsultatieID] = @ConsultatieID)
        BEGIN
            -- UPDATE
            UPDATE [dbo].[ConsultatieMotivePrezentare]
            SET 
                [MotivPrezentare] = @MotivPrezentare,
                [IstoricBoalaActuala] = @IstoricBoalaActuala,
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @ModificatDe
            WHERE [ConsultatieID] = @ConsultatieID;
        END
        ELSE
        BEGIN
            -- INSERT
            INSERT INTO [dbo].[ConsultatieMotivePrezentare]
            (
                [Id],
                [ConsultatieID],
                [MotivPrezentare],
                [IstoricBoalaActuala],
                [DataCreare],
                [CreatDe]
            )
            VALUES
            (
                NEWID(),
                @ConsultatieID,
                @MotivPrezentare,
                @IstoricBoalaActuala,
                GETDATE(),
                @ModificatDe
            );
        END
        
        -- Update master record timestamp
        UPDATE [dbo].[Consultatii]
        SET 
            [DataUltimeiModificari] = GETDATE(),
            [ModificatDe] = @ModificatDe
        WHERE [ConsultatieID] = @ConsultatieID;
        
        COMMIT TRANSACTION;
        
        -- Return updated record
        SELECT 
            [Id],
            [ConsultatieID],
            [MotivPrezentare],
            [IstoricBoalaActuala],
            [DataCreare],
            [CreatDe],
            [DataUltimeiModificari],
            [ModificatDe]
        FROM [dbo].[ConsultatieMotivePrezentare]
        WHERE [ConsultatieID] = @ConsultatieID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
