/*
==============================================================================
UPDATED: sp_Consultatie_SaveDraft
Author: AI Agent
Date: 2026-01-02
Description: Updated to work with normalized structure (14 master columns only)
Changes: Removed all denormalized parameters - SP now saves only master table
==============================================================================
*/

USE [ValyanMed]
GO

PRINT 'Updating sp_Consultatie_SaveDraft for normalized structure...'

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Consultatie_SaveDraft')
    DROP PROCEDURE sp_Consultatie_SaveDraft;
GO

CREATE PROCEDURE sp_Consultatie_SaveDraft
    @ConsultatieID UNIQUEIDENTIFIER OUTPUT,
    @ProgramareID UNIQUEIDENTIFIER,
    @PacientID UNIQUEIDENTIFIER,
    @MedicID UNIQUEIDENTIFIER,
    @DataConsultatie DATE = NULL,
    @OraConsultatie TIME = NULL,
    @TipConsultatie NVARCHAR(50) = 'Prima consultatie',
    @DurataMinute INT = 0,
    @CreatDeSauModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verificare daca consultatia exista deja
        IF EXISTS (SELECT 1 FROM Consultatii WHERE ConsultatieID = @ConsultatieID)
        BEGIN
            -- UPDATE draft existent (doar master fields)
            UPDATE Consultatii
            SET 
                DataUltimeiModificari = GETDATE(),
                ModificatDe = @CreatDeSauModificatDe,
                [Status] = 'In desfasurare',
                DurataMinute = ISNULL(@DurataMinute, DurataMinute)
            WHERE ConsultatieID = @ConsultatieID;
        END
        ELSE
        BEGIN
            -- INSERT draft nou (doar master fields)
            IF @ConsultatieID IS NULL OR @ConsultatieID = '00000000-0000-0000-0000-000000000000'
                SET @ConsultatieID = NEWID();
            
            IF @DataConsultatie IS NULL
                SET @DataConsultatie = CAST(GETDATE() AS DATE);
            
            IF @OraConsultatie IS NULL
                SET @OraConsultatie = CAST(GETDATE() AS TIME);
            
            INSERT INTO Consultatii (
                ConsultatieID, ProgramareID, PacientID, MedicID,
                DataConsultatie, OraConsultatie, TipConsultatie,
                [Status], DurataMinute,
                DataCreare, CreatDe
            )
            VALUES (
                @ConsultatieID, @ProgramareID, @PacientID, @MedicID,
                @DataConsultatie, @OraConsultatie, @TipConsultatie,
                'In desfasurare', @DurataMinute,
                GETDATE(), @CreatDeSauModificatDe
            );
        END
        
        -- Return ConsultatieID
        SELECT @ConsultatieID AS ConsultatieID, @@ROWCOUNT AS RowsAffected;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT '✓ sp_Consultatie_SaveDraft updated successfully for normalized structure'
GO
