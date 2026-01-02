/*
==============================================================================
STORED PROCEDURE: ConsultatieInvestigatii_Upsert
==============================================================================
Description: Insert or Update investigatii for a consultatie
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieInvestigatii_Upsert')
    DROP PROCEDURE [dbo].[ConsultatieInvestigatii_Upsert]
GO

CREATE PROCEDURE [dbo].[ConsultatieInvestigatii_Upsert]
    @ConsultatieID UNIQUEIDENTIFIER,
    @InvestigatiiLaborator NVARCHAR(MAX) = NULL,
    @InvestigatiiImagistice NVARCHAR(MAX) = NULL,
    @InvestigatiiEKG NVARCHAR(MAX) = NULL,
    @AlteInvestigatii NVARCHAR(MAX) = NULL,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM [dbo].[ConsultatieInvestigatii] WHERE [ConsultatieID] = @ConsultatieID)
        BEGIN
            UPDATE [dbo].[ConsultatieInvestigatii]
            SET 
                [InvestigatiiLaborator] = @InvestigatiiLaborator,
                [InvestigatiiImagistice] = @InvestigatiiImagistice,
                [InvestigatiiEKG] = @InvestigatiiEKG,
                [AlteInvestigatii] = @AlteInvestigatii,
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @ModificatDe
            WHERE [ConsultatieID] = @ConsultatieID;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[ConsultatieInvestigatii]
            (
                [Id],
                [ConsultatieID],
                [InvestigatiiLaborator],
                [InvestigatiiImagistice],
                [InvestigatiiEKG],
                [AlteInvestigatii],
                [DataCreare],
                [CreatDe]
            )
            VALUES
            (
                NEWID(),
                @ConsultatieID,
                @InvestigatiiLaborator,
                @InvestigatiiImagistice,
                @InvestigatiiEKG,
                @AlteInvestigatii,
                GETDATE(),
                @ModificatDe
            );
        END
        
        COMMIT TRANSACTION;
        
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        RETURN -1;
    END CATCH
END
GO

PRINT 'Stored Procedure ConsultatieInvestigatii_Upsert created successfully';
GO
