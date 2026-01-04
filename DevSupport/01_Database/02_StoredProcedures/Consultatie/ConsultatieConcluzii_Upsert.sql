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
    -- Scrisoare Medicala Anexa 43
    @EsteAfectiuneOncologica BIT = 0,
    @DetaliiAfectiuneOncologica NVARCHAR(500) = NULL,
    @AreIndicatieInternare BIT = 0,
    @TermenInternare NVARCHAR(100) = NULL,
    @SaEliberatPrescriptie BIT = NULL,
    @SeriePrescriptie NVARCHAR(50) = NULL,
    @SaEliberatConcediuMedical BIT = NULL,
    @SerieConcediuMedical NVARCHAR(50) = NULL,
    @SaEliberatIngrijiriDomiciliu BIT = NULL,
    @SaEliberatDispozitiveMedicale BIT = NULL,
    @TransmiterePrinEmail BIT = 0,
    @EmailTransmitere NVARCHAR(100) = NULL,
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
                [EsteAfectiuneOncologica] = @EsteAfectiuneOncologica,
                [DetaliiAfectiuneOncologica] = @DetaliiAfectiuneOncologica,
                [AreIndicatieInternare] = @AreIndicatieInternare,
                [TermenInternare] = @TermenInternare,
                [SaEliberatPrescriptie] = @SaEliberatPrescriptie,
                [SeriePrescriptie] = @SeriePrescriptie,
                [SaEliberatConcediuMedical] = @SaEliberatConcediuMedical,
                [SerieConcediuMedical] = @SerieConcediuMedical,
                [SaEliberatIngrijiriDomiciliu] = @SaEliberatIngrijiriDomiciliu,
                [SaEliberatDispozitiveMedicale] = @SaEliberatDispozitiveMedicale,
                [TransmiterePrinEmail] = @TransmiterePrinEmail,
                [EmailTransmitere] = @EmailTransmitere,
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @ModificatDe
            WHERE [ConsultatieID] = @ConsultatieID;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[ConsultatieConcluzii]
            (
                [Id], [ConsultatieID],
                [Prognostic], [Concluzie], [ObservatiiMedic], [NotePacient], [DocumenteAtatate],
                [EsteAfectiuneOncologica], [DetaliiAfectiuneOncologica],
                [AreIndicatieInternare], [TermenInternare],
                [SaEliberatPrescriptie], [SeriePrescriptie],
                [SaEliberatConcediuMedical], [SerieConcediuMedical],
                [SaEliberatIngrijiriDomiciliu], [SaEliberatDispozitiveMedicale],
                [TransmiterePrinEmail], [EmailTransmitere],
                [DataCreare], [CreatDe]
            )
            VALUES
            (
                NEWID(), @ConsultatieID,
                @Prognostic, @Concluzie, @ObservatiiMedic, @NotePacient, @DocumenteAtatate,
                @EsteAfectiuneOncologica, @DetaliiAfectiuneOncologica,
                @AreIndicatieInternare, @TermenInternare,
                @SaEliberatPrescriptie, @SeriePrescriptie,
                @SaEliberatConcediuMedical, @SerieConcediuMedical,
                @SaEliberatIngrijiriDomiciliu, @SaEliberatDispozitiveMedicale,
                @TransmiterePrinEmail, @EmailTransmitere,
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
