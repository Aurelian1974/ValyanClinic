/*
==============================================================================
STORED PROCEDURE: ConsultatieExamenObiectiv_Upsert
==============================================================================
Description: Insert or Update examen obiectiv for a consultatie
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieExamenObiectiv_Upsert')
    DROP PROCEDURE [dbo].[ConsultatieExamenObiectiv_Upsert]
GO

CREATE PROCEDURE [dbo].[ConsultatieExamenObiectiv_Upsert]
    @ConsultatieID UNIQUEIDENTIFIER,
    @StareGenerala NVARCHAR(MAX) = NULL,
    @Constitutie NVARCHAR(500) = NULL,
    @Atitudine NVARCHAR(500) = NULL,
    @Facies NVARCHAR(500) = NULL,
    @Tegumente NVARCHAR(MAX) = NULL,
    @Mucoase NVARCHAR(MAX) = NULL,
    @GangliniLimfatici NVARCHAR(MAX) = NULL,
    @Edeme NVARCHAR(MAX) = NULL,
    @Greutate DECIMAL(6,2) = NULL,
    @Inaltime DECIMAL(6,2) = NULL,
    @IMC DECIMAL(5,2) = NULL,
    @Temperatura DECIMAL(4,2) = NULL,
    @TensiuneArteriala NVARCHAR(50) = NULL,
    @Puls INT = NULL,
    @FreccventaRespiratorie INT = NULL,
    @SaturatieO2 INT = NULL,
    @Glicemie DECIMAL(6,2) = NULL,
    @ExamenCardiovascular NVARCHAR(MAX) = NULL,
    @ExamenRespiratoriu NVARCHAR(MAX) = NULL,
    @ExamenDigestiv NVARCHAR(MAX) = NULL,
    @ExamenUrinar NVARCHAR(MAX) = NULL,
    @ExamenNervos NVARCHAR(MAX) = NULL,
    @ExamenLocomotor NVARCHAR(MAX) = NULL,
    @ExamenEndocrin NVARCHAR(MAX) = NULL,
    @ExamenORL NVARCHAR(MAX) = NULL,
    @ExamenOftalmologic NVARCHAR(MAX) = NULL,
    @ExamenDermatologic NVARCHAR(MAX) = NULL,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM [dbo].[ConsultatieExamenObiectiv] WHERE [ConsultatieID] = @ConsultatieID)
        BEGIN
            UPDATE [dbo].[ConsultatieExamenObiectiv]
            SET 
                [StareGenerala] = @StareGenerala,
                [Constitutie] = @Constitutie,
                [Atitudine] = @Atitudine,
                [Facies] = @Facies,
                [Tegumente] = @Tegumente,
                [Mucoase] = @Mucoase,
                [GangliniLimfatici] = @GangliniLimfatici,
                [Edeme] = @Edeme,
                [Greutate] = @Greutate,
                [Inaltime] = @Inaltime,
                [IMC] = @IMC,
                [Temperatura] = @Temperatura,
                [TensiuneArteriala] = @TensiuneArteriala,
                [Puls] = @Puls,
                [FreccventaRespiratorie] = @FreccventaRespiratorie,
                [SaturatieO2] = @SaturatieO2,
                [Glicemie] = @Glicemie,
                [ExamenCardiovascular] = @ExamenCardiovascular,
                [ExamenRespiratoriu] = @ExamenRespiratoriu,
                [ExamenDigestiv] = @ExamenDigestiv,
                [ExamenUrinar] = @ExamenUrinar,
                [ExamenNervos] = @ExamenNervos,
                [ExamenLocomotor] = @ExamenLocomotor,
                [ExamenEndocrin] = @ExamenEndocrin,
                [ExamenORL] = @ExamenORL,
                [ExamenOftalmologic] = @ExamenOftalmologic,
                [ExamenDermatologic] = @ExamenDermatologic,
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @ModificatDe
            WHERE [ConsultatieID] = @ConsultatieID;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[ConsultatieExamenObiectiv]
            (
                [Id], [ConsultatieID],
                [StareGenerala], [Constitutie], [Atitudine], [Facies],
                [Tegumente], [Mucoase], [GangliniLimfatici], [Edeme],
                [Greutate], [Inaltime], [IMC], [Temperatura], [TensiuneArteriala],
                [Puls], [FreccventaRespiratorie], [SaturatieO2], [Glicemie],
                [ExamenCardiovascular], [ExamenRespiratoriu], [ExamenDigestiv],
                [ExamenUrinar], [ExamenNervos], [ExamenLocomotor], [ExamenEndocrin],
                [ExamenORL], [ExamenOftalmologic], [ExamenDermatologic],
                [DataCreare], [CreatDe]
            )
            VALUES
            (
                NEWID(), @ConsultatieID,
                @StareGenerala, @Constitutie, @Atitudine, @Facies,
                @Tegumente, @Mucoase, @GangliniLimfatici, @Edeme,
                @Greutate, @Inaltime, @IMC, @Temperatura, @TensiuneArteriala,
                @Puls, @FreccventaRespiratorie, @SaturatieO2, @Glicemie,
                @ExamenCardiovascular, @ExamenRespiratoriu, @ExamenDigestiv,
                @ExamenUrinar, @ExamenNervos, @ExamenLocomotor, @ExamenEndocrin,
                @ExamenORL, @ExamenOftalmologic, @ExamenDermatologic,
                GETDATE(), @ModificatDe
            );
        END
        
        UPDATE [dbo].[Consultatii]
        SET [DataUltimeiModificari] = GETDATE(), [ModificatDe] = @ModificatDe
        WHERE [ConsultatieID] = @ConsultatieID;
        
        COMMIT TRANSACTION;
        
        SELECT * FROM [dbo].[ConsultatieExamenObiectiv] WHERE [ConsultatieID] = @ConsultatieID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
