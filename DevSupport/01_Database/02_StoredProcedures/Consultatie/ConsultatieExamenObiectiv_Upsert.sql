/*
==============================================================================
STORED PROCEDURE: ConsultatieExamenObiectiv_Upsert
==============================================================================
Description: Insert or Update examen obiectiv for a consultatie
Author: System
Date: 2026-01-02
Version: 3.0 (Simplified - Removed legacy exam columns)
==============================================================================
*/

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieExamenObiectiv_Upsert')
    DROP PROCEDURE [dbo].[ConsultatieExamenObiectiv_Upsert]
GO

CREATE PROCEDURE [dbo].[ConsultatieExamenObiectiv_Upsert]
    @ConsultatieID UNIQUEIDENTIFIER,
    @StareGenerala NVARCHAR(MAX) = NULL,
    @Tegumente NVARCHAR(MAX) = NULL,
    @Mucoase NVARCHAR(MAX) = NULL,
    @GanglioniLimfatici NVARCHAR(MAX) = NULL,
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
    @ExamenObiectivDetaliat NVARCHAR(MAX) = NULL,
    @AlteObservatiiClinice NVARCHAR(MAX) = NULL,
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
                [Tegumente] = @Tegumente,
                [Mucoase] = @Mucoase,
                [GanglioniLimfatici] = @GanglioniLimfatici,
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
                [ExamenObiectivDetaliat] = @ExamenObiectivDetaliat,
                [AlteObservatiiClinice] = @AlteObservatiiClinice,
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @ModificatDe
            WHERE [ConsultatieID] = @ConsultatieID;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[ConsultatieExamenObiectiv]
            (
                [Id], [ConsultatieID],
                [StareGenerala], [Tegumente], [Mucoase], [GanglioniLimfatici], [Edeme],
                [Greutate], [Inaltime], [IMC], [Temperatura], [TensiuneArteriala],
                [Puls], [FreccventaRespiratorie], [SaturatieO2], [Glicemie],
                [ExamenObiectivDetaliat], [AlteObservatiiClinice],
                [DataCreare], [CreatDe]
            )
            VALUES
            (
                NEWID(), @ConsultatieID,
                @StareGenerala, @Tegumente, @Mucoase, @GanglioniLimfatici, @Edeme,
                @Greutate, @Inaltime, @IMC, @Temperatura, @TensiuneArteriala,
                @Puls, @FreccventaRespiratorie, @SaturatieO2, @Glicemie,
                @ExamenObiectivDetaliat, @AlteObservatiiClinice,
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
