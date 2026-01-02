/*
==============================================================================
STORED PROCEDURE: Consultatie_Finalize
==============================================================================
Description: Finalizes a consultatie (marks as completed)
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_Finalize')
    DROP PROCEDURE [dbo].[Consultatie_Finalize]
GO

CREATE PROCEDURE [dbo].[Consultatie_Finalize]
    @ConsultatieID UNIQUEIDENTIFIER,
    @DurataMinute INT = 0,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        UPDATE [dbo].[Consultatii]
        SET 
            [Status] = 'Finalizata',
            [DataFinalizare] = GETDATE(),
            [DurataMinute] = @DurataMinute,
            [DataUltimeiModificari] = GETDATE(),
            [ModificatDe] = @ModificatDe
        WHERE [ConsultatieID] = @ConsultatieID;
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Consultatie not found', 16, 1);
            RETURN;
        END
        
        COMMIT TRANSACTION;
        
        SELECT * FROM [dbo].[Consultatii] WHERE [ConsultatieID] = @ConsultatieID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
