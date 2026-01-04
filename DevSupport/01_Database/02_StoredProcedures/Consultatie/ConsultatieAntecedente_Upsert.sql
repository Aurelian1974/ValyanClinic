/*
==============================================================================
STORED PROCEDURE: ConsultatieAntecedente_Upsert
==============================================================================
Description: Insert or Update antecedente for a consultatie (Simplified)
Author: System
Date: 2026-01-04
Version: 3.0 (Simplified Structure - Only IstoricMedicalPersonal & IstoricFamilial)
==============================================================================
*/

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieAntecedente_Upsert')
    DROP PROCEDURE [dbo].[ConsultatieAntecedente_Upsert]
GO

CREATE PROCEDURE [dbo].[ConsultatieAntecedente_Upsert]
    @ConsultatieID UNIQUEIDENTIFIER,
    @IstoricMedicalPersonal NVARCHAR(MAX) = NULL,
    @IstoricFamilial NVARCHAR(MAX) = NULL,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM [dbo].[ConsultatieAntecedente] WHERE [ConsultatieID] = @ConsultatieID)
        BEGIN
            UPDATE [dbo].[ConsultatieAntecedente]
            SET 
                [IstoricMedicalPersonal] = @IstoricMedicalPersonal,
                [IstoricFamilial] = @IstoricFamilial,
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @ModificatDe
            WHERE [ConsultatieID] = @ConsultatieID;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[ConsultatieAntecedente]
            (
                [Id], [ConsultatieID],
                [IstoricMedicalPersonal], [IstoricFamilial],
                [DataCreare], [CreatDe]
            )
            VALUES
            (
                NEWID(), @ConsultatieID,
                @IstoricMedicalPersonal, @IstoricFamilial,
                GETDATE(), @ModificatDe
            );
        END
        
        UPDATE [dbo].[Consultatii]
        SET [DataUltimeiModificari] = GETDATE(), [ModificatDe] = @ModificatDe
        WHERE [ConsultatieID] = @ConsultatieID;
        
        COMMIT TRANSACTION;
        
        SELECT * FROM [dbo].[ConsultatieAntecedente] WHERE [ConsultatieID] = @ConsultatieID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
