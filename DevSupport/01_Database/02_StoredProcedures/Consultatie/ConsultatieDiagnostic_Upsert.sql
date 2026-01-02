/*
==============================================================================
STORED PROCEDURE: ConsultatieDiagnostic_Upsert
==============================================================================
Description: Insert or Update diagnostic for a consultatie
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieDiagnostic_Upsert')
    DROP PROCEDURE [dbo].[ConsultatieDiagnostic_Upsert]
GO

CREATE PROCEDURE [dbo].[ConsultatieDiagnostic_Upsert]
    @ConsultatieID UNIQUEIDENTIFIER,
    @DiagnosticPozitiv NVARCHAR(MAX) = NULL,
    @DiagnosticDiferential NVARCHAR(MAX) = NULL,
    @DiagnosticEtiologic NVARCHAR(MAX) = NULL,
    @CoduriICD10 NVARCHAR(200) = NULL,
    @CoduriICD10Secundare NVARCHAR(500) = NULL,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM [dbo].[ConsultatieDiagnostic] WHERE [ConsultatieID] = @ConsultatieID)
        BEGIN
            UPDATE [dbo].[ConsultatieDiagnostic]
            SET 
                [DiagnosticPozitiv] = @DiagnosticPozitiv,
                [DiagnosticDiferential] = @DiagnosticDiferential,
                [DiagnosticEtiologic] = @DiagnosticEtiologic,
                [CoduriICD10] = @CoduriICD10,
                [CoduriICD10Secundare] = @CoduriICD10Secundare,
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @ModificatDe
            WHERE [ConsultatieID] = @ConsultatieID;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[ConsultatieDiagnostic]
            (
                [Id], [ConsultatieID],
                [DiagnosticPozitiv], [DiagnosticDiferential], [DiagnosticEtiologic],
                [CoduriICD10], [CoduriICD10Secundare],
                [DataCreare], [CreatDe]
            )
            VALUES
            (
                NEWID(), @ConsultatieID,
                @DiagnosticPozitiv, @DiagnosticDiferential, @DiagnosticEtiologic,
                @CoduriICD10, @CoduriICD10Secundare,
                GETDATE(), @ModificatDe
            );
        END
        
        UPDATE [dbo].[Consultatii]
        SET [DataUltimeiModificari] = GETDATE(), [ModificatDe] = @ModificatDe
        WHERE [ConsultatieID] = @ConsultatieID;
        
        COMMIT TRANSACTION;
        
        SELECT * FROM [dbo].[ConsultatieDiagnostic] WHERE [ConsultatieID] = @ConsultatieID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
