/*
==============================================================================
STORED PROCEDURE: ConsultatieAntecedente_Upsert
==============================================================================
Description: Insert or Update antecedente for a consultatie
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieAntecedente_Upsert')
    DROP PROCEDURE [dbo].[ConsultatieAntecedente_Upsert]
GO

CREATE PROCEDURE [dbo].[ConsultatieAntecedente_Upsert]
    @ConsultatieID UNIQUEIDENTIFIER,
    @AHC_Mama NVARCHAR(MAX) = NULL,
    @AHC_Tata NVARCHAR(MAX) = NULL,
    @AHC_Frati NVARCHAR(MAX) = NULL,
    @AHC_Bunici NVARCHAR(MAX) = NULL,
    @AHC_Altele NVARCHAR(MAX) = NULL,
    @AF_Nastere NVARCHAR(MAX) = NULL,
    @AF_Dezvoltare NVARCHAR(MAX) = NULL,
    @AF_Menstruatie NVARCHAR(MAX) = NULL,
    @AF_Sarcini NVARCHAR(MAX) = NULL,
    @AF_Alaptare NVARCHAR(MAX) = NULL,
    @APP_BoliCopilarieAdolescenta NVARCHAR(MAX) = NULL,
    @APP_BoliAdult NVARCHAR(MAX) = NULL,
    @APP_Interventii NVARCHAR(MAX) = NULL,
    @APP_Traumatisme NVARCHAR(MAX) = NULL,
    @APP_Transfuzii NVARCHAR(MAX) = NULL,
    @APP_Alergii NVARCHAR(MAX) = NULL,
    @APP_Medicatie NVARCHAR(MAX) = NULL,
    @Profesie NVARCHAR(500) = NULL,
    @ConditiiLocuinta NVARCHAR(MAX) = NULL,
    @ConditiiMunca NVARCHAR(MAX) = NULL,
    @ObiceiuriAlimentare NVARCHAR(MAX) = NULL,
    @Toxice NVARCHAR(MAX) = NULL,
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
                [AHC_Mama] = @AHC_Mama,
                [AHC_Tata] = @AHC_Tata,
                [AHC_Frati] = @AHC_Frati,
                [AHC_Bunici] = @AHC_Bunici,
                [AHC_Altele] = @AHC_Altele,
                [AF_Nastere] = @AF_Nastere,
                [AF_Dezvoltare] = @AF_Dezvoltare,
                [AF_Menstruatie] = @AF_Menstruatie,
                [AF_Sarcini] = @AF_Sarcini,
                [AF_Alaptare] = @AF_Alaptare,
                [APP_BoliCopilarieAdolescenta] = @APP_BoliCopilarieAdolescenta,
                [APP_BoliAdult] = @APP_BoliAdult,
                [APP_Interventii] = @APP_Interventii,
                [APP_Traumatisme] = @APP_Traumatisme,
                [APP_Transfuzii] = @APP_Transfuzii,
                [APP_Alergii] = @APP_Alergii,
                [APP_Medicatie] = @APP_Medicatie,
                [Profesie] = @Profesie,
                [ConditiiLocuinta] = @ConditiiLocuinta,
                [ConditiiMunca] = @ConditiiMunca,
                [ObiceiuriAlimentare] = @ObiceiuriAlimentare,
                [Toxice] = @Toxice,
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @ModificatDe
            WHERE [ConsultatieID] = @ConsultatieID;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[ConsultatieAntecedente]
            (
                [Id], [ConsultatieID],
                [AHC_Mama], [AHC_Tata], [AHC_Frati], [AHC_Bunici], [AHC_Altele],
                [AF_Nastere], [AF_Dezvoltare], [AF_Menstruatie], [AF_Sarcini], [AF_Alaptare],
                [APP_BoliCopilarieAdolescenta], [APP_BoliAdult], [APP_Interventii], 
                [APP_Traumatisme], [APP_Transfuzii], [APP_Alergii], [APP_Medicatie],
                [Profesie], [ConditiiLocuinta], [ConditiiMunca], [ObiceiuriAlimentare], [Toxice],
                [DataCreare], [CreatDe]
            )
            VALUES
            (
                NEWID(), @ConsultatieID,
                @AHC_Mama, @AHC_Tata, @AHC_Frati, @AHC_Bunici, @AHC_Altele,
                @AF_Nastere, @AF_Dezvoltare, @AF_Menstruatie, @AF_Sarcini, @AF_Alaptare,
                @APP_BoliCopilarieAdolescenta, @APP_BoliAdult, @APP_Interventii,
                @APP_Traumatisme, @APP_Transfuzii, @APP_Alergii, @APP_Medicatie,
                @Profesie, @ConditiiLocuinta, @ConditiiMunca, @ObiceiuriAlimentare, @Toxice,
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
