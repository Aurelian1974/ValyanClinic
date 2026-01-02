/*
==============================================================================
STORED PROCEDURE: ConsultatieTratament_Upsert
==============================================================================
Description: Insert or Update tratament for a consultatie
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieTratament_Upsert')
    DROP PROCEDURE [dbo].[ConsultatieTratament_Upsert]
GO

CREATE PROCEDURE [dbo].[ConsultatieTratament_Upsert]
    @ConsultatieID UNIQUEIDENTIFIER,
    @TratamentMedicamentos NVARCHAR(MAX) = NULL,
    @TratamentNemedicamentos NVARCHAR(MAX) = NULL,
    @RecomandariDietetice NVARCHAR(MAX) = NULL,
    @RecomandariRegimViata NVARCHAR(MAX) = NULL,
    @InvestigatiiRecomandate NVARCHAR(MAX) = NULL,
    @ConsulturiSpecialitate NVARCHAR(MAX) = NULL,
    @DataUrmatoareiProgramari NVARCHAR(200) = NULL,
    @RecomandariSupraveghere NVARCHAR(MAX) = NULL,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM [dbo].[ConsultatieTratament] WHERE [ConsultatieID] = @ConsultatieID)
        BEGIN
            UPDATE [dbo].[ConsultatieTratament]
            SET 
                [TratamentMedicamentos] = @TratamentMedicamentos,
                [TratamentNemedicamentos] = @TratamentNemedicamentos,
                [RecomandariDietetice] = @RecomandariDietetice,
                [RecomandariRegimViata] = @RecomandariRegimViata,
                [InvestigatiiRecomandate] = @InvestigatiiRecomandate,
                [ConsulturiSpecialitate] = @ConsulturiSpecialitate,
                [DataUrmatoareiProgramari] = @DataUrmatoareiProgramari,
                [RecomandariSupraveghere] = @RecomandariSupraveghere,
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @ModificatDe
            WHERE [ConsultatieID] = @ConsultatieID;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[ConsultatieTratament]
            (
                [Id], [ConsultatieID],
                [TratamentMedicamentos], [TratamentNemedicamentos],
                [RecomandariDietetice], [RecomandariRegimViata],
                [InvestigatiiRecomandate], [ConsulturiSpecialitate],
                [DataUrmatoareiProgramari], [RecomandariSupraveghere],
                [DataCreare], [CreatDe]
            )
            VALUES
            (
                NEWID(), @ConsultatieID,
                @TratamentMedicamentos, @TratamentNemedicamentos,
                @RecomandariDietetice, @RecomandariRegimViata,
                @InvestigatiiRecomandate, @ConsulturiSpecialitate,
                @DataUrmatoareiProgramari, @RecomandariSupraveghere,
                GETDATE(), @ModificatDe
            );
        END
        
        UPDATE [dbo].[Consultatii]
        SET [DataUltimeiModificari] = GETDATE(), [ModificatDe] = @ModificatDe
        WHERE [ConsultatieID] = @ConsultatieID;
        
        COMMIT TRANSACTION;
        
        SELECT * FROM [dbo].[ConsultatieTratament] WHERE [ConsultatieID] = @ConsultatieID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
