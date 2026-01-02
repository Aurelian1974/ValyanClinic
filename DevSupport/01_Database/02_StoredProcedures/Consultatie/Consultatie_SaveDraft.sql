/*
==============================================================================
STORED PROCEDURE: Consultatie_SaveDraft
==============================================================================
Description: Save or update a draft consultation (master record only)
Author: System
Date: 2026-01-02
Version: 1.0 (Normalized Structure)
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_SaveDraft')
    DROP PROCEDURE [dbo].[Consultatie_SaveDraft]
GO

CREATE PROCEDURE [dbo].[Consultatie_SaveDraft]
    @ConsultatieID UNIQUEIDENTIFIER OUTPUT,
    @ProgramareID UNIQUEIDENTIFIER = NULL,
    @PacientID UNIQUEIDENTIFIER,
    @MedicID UNIQUEIDENTIFIER,
    @DataConsultatie DATE,
    @OraConsultatie TIME,
    @TipConsultatie NVARCHAR(50),
    @CreatDeSauModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if consultation exists
        IF EXISTS (SELECT 1 FROM [dbo].[Consultatii] WHERE [ConsultatieID] = @ConsultatieID)
        BEGIN
            -- Update existing draft
            UPDATE [dbo].[Consultatii]
            SET 
                [ProgramareID] = @ProgramareID,
                [PacientID] = @PacientID,
                [MedicID] = @MedicID,
                [DataConsultatie] = @DataConsultatie,
                [OraConsultatie] = @OraConsultatie,
                [TipConsultatie] = @TipConsultatie,
                [Status] = 'Draft',
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @CreatDeSauModificatDe
            WHERE [ConsultatieID] = @ConsultatieID
        END
        ELSE
        BEGIN
            -- Create new draft
            IF @ConsultatieID = '00000000-0000-0000-0000-000000000000'
                SET @ConsultatieID = NEWID()
            
            INSERT INTO [dbo].[Consultatii] (
                [ConsultatieID],
                [ProgramareID],
                [PacientID],
                [MedicID],
                [DataConsultatie],
                [OraConsultatie],
                [TipConsultatie],
                [Status],
                [DataCreare],
                [CreatDe]
            )
            VALUES (
                @ConsultatieID,
                @ProgramareID,
                @PacientID,
                @MedicID,
                @DataConsultatie,
                @OraConsultatie,
                @TipConsultatie,
                'Draft',
                GETDATE(),
                @CreatDeSauModificatDe
            )
        END
        
        COMMIT TRANSACTION;
        
        -- Return the ConsultatieID
        SELECT @ConsultatieID AS ConsultatieID
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO
