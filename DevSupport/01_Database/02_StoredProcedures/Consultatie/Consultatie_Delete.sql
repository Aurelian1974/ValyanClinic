/*
==============================================================================
STORED PROCEDURE: Consultatie_Delete
==============================================================================
Description: Deletes a consultatie (CASCADE will delete all related details)
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_Delete')
    DROP PROCEDURE [dbo].[Consultatie_Delete]
GO

CREATE PROCEDURE [dbo].[Consultatie_Delete]
    @ConsultatieID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- CASCADE DELETE will automatically remove all related records
        DELETE FROM [dbo].[Consultatii]
        WHERE [ConsultatieID] = @ConsultatieID;
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Consultatie not found', 16, 1);
            RETURN;
        END
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
