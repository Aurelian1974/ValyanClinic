/*
==============================================================================
STORED PROCEDURE: ConsultatieConcluzii_Upsert
==============================================================================
Description: Insert or Update concluzii for a consultatie
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieConcluzii_Upsert')
    DROP PROCEDURE [dbo].[ConsultatieConcluzii_Upsert]
GO

CREATE PROCEDURE [dbo].[ConsultatieConcluzii_Upsert]
    @ConsultatieID UNIQUEIDENTIFIER,
    @Prognostic NVARCHAR(50) = NULL,
    @Concluzie NVARCHAR(MAX) = NULL,
    @ObservatiiMedic NVARCHAR(MAX) = NULL,
    @NotePacient NVARCHAR(MAX) = NULL,
    @DocumenteAtatate NVARCHAR(MAX) = NULL,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM [dbo].[ConsultatieConcluzii] WHERE [ConsultatieID] = @ConsultatieID)
        BEGIN
            UPDATE [dbo].[ConsultatieConcluzii]
            SET 
                [Prognostic] = @Prognostic,
                [Concluzie] = @Concluzie,
                [ObservatiiMedic] = @ObservatiiMedic,
                [NotePacient] = @NotePacient,
                [DocumenteAtatate] = @DocumenteAtatate,
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @ModificatDe
            WHERE [ConsultatieID] = @ConsultatieID;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[ConsultatieConcluzii]
            (
                [Id], [ConsultatieID],
                [Prognostic], [Concluzie], [ObservatiiMedic], 
                [NotePacient], [DocumenteAtatate],
                [DataCreare], [CreatDe]
            )
            VALUES
            (
                NEWID(), @ConsultatieID,
                @Prognostic, @Concluzie, @ObservatiiMedic,
                @NotePacient, @DocumenteAtatate,
                GETDATE(), @ModificatDe
            );
        END
        
        UPDATE [dbo].[Consultatii]
        SET [DataUltimeiModificari] = GETDATE(), [ModificatDe] = @ModificatDe
        WHERE [ConsultatieID] = @ConsultatieID;
        
        COMMIT TRANSACTION;
        
        SELECT * FROM [dbo].[ConsultatieConcluzii] WHERE [ConsultatieID] = @ConsultatieID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
