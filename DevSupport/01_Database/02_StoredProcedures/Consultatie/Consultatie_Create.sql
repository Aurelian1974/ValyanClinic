/*
==============================================================================
STORED PROCEDURE: Consultatie_Create
==============================================================================
Description: Creates a new consultatie (MASTER record only)
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)

Usage:
    EXEC Consultatie_Create 
        @ProgramareID = NULL,
        @PacientID = '...',
        @MedicID = '...',
        @DataConsultatie = '2026-01-02',
        @OraConsultatie = '10:30',
        @TipConsultatie = 'Prima consultatie',
        @CreatDe = '...'
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_Create')
    DROP PROCEDURE [dbo].[Consultatie_Create]
GO

CREATE PROCEDURE [dbo].[Consultatie_Create]
    @ProgramareID UNIQUEIDENTIFIER = NULL,
    @PacientID UNIQUEIDENTIFIER,
    @MedicID UNIQUEIDENTIFIER,
    @DataConsultatie DATETIME,
    @OraConsultatie TIME,
    @TipConsultatie NVARCHAR(50) = 'Prima consultatie',
    @CreatDe UNIQUEIDENTIFIER,
    @ConsultatieID UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Generate new ID
        SET @ConsultatieID = NEWID();
        
        -- Insert MASTER record
        INSERT INTO [dbo].[Consultatii]
        (
            [ConsultatieID],
            [ProgramareID],
            [PacientID],
            [MedicID],
            [DataConsultatie],
            [OraConsultatie],
            [TipConsultatie],
            [Status],
            [DurataMinute],
            [DataCreare],
            [CreatDe]
        )
        VALUES
        (
            @ConsultatieID,
            @ProgramareID,
            @PacientID,
            @MedicID,
            @DataConsultatie,
            @OraConsultatie,
            @TipConsultatie,
            'In desfasurare',
            0,
            GETDATE(),
            @CreatDe
        );
        
        COMMIT TRANSACTION;
        
        -- Return the created record
        SELECT 
            [ConsultatieID],
            [ProgramareID],
            [PacientID],
            [MedicID],
            [DataConsultatie],
            [OraConsultatie],
            [TipConsultatie],
            [Status],
            [DataFinalizare],
            [DurataMinute],
            [DataCreare],
            [CreatDe],
            [DataUltimeiModificari],
            [ModificatDe]
        FROM [dbo].[Consultatii]
        WHERE [ConsultatieID] = @ConsultatieID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
