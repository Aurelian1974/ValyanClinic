-- =============================================
-- Migration: 012_Cleanup_Diagnostic_Legacy_Columns.sql
-- Descriere: Eliminare coloane legacy din ConsultatieDiagnostic
-- Data: 2026-01-04
-- =============================================

USE [ValyanMed];
GO

PRINT '=== MIGRATION 012: Cleanup Diagnostic Legacy Columns ===';
PRINT '';

-- =============================================
-- STEP 1: Eliminate DiagnosticDiferential and DiagnosticEtiologic
-- Keep only DiagnosticPozitiv as fallback for legacy data
-- =============================================

PRINT '1. Checking columns to drop...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'ConsultatieDiagnostic' AND COLUMN_NAME = 'DiagnosticDiferential')
BEGIN
    ALTER TABLE [dbo].[ConsultatieDiagnostic] DROP COLUMN [DiagnosticDiferential];
    PRINT '   ✓ Dropped DiagnosticDiferential column';
END
ELSE
    PRINT '   - DiagnosticDiferential already dropped';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'ConsultatieDiagnostic' AND COLUMN_NAME = 'DiagnosticEtiologic')
BEGIN
    ALTER TABLE [dbo].[ConsultatieDiagnostic] DROP COLUMN [DiagnosticEtiologic];
    PRINT '   ✓ Dropped DiagnosticEtiologic column';
END
ELSE
    PRINT '   - DiagnosticEtiologic already dropped';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'ConsultatieDiagnostic' AND COLUMN_NAME = 'CoduriICD10Secundare')
BEGIN
    ALTER TABLE [dbo].[ConsultatieDiagnostic] DROP COLUMN [CoduriICD10Secundare];
    PRINT '   ✓ Dropped CoduriICD10Secundare column (moved to ConsultatieDiagnosticSecundar)';
END
ELSE
    PRINT '   - CoduriICD10Secundare already dropped';

PRINT '';

-- =============================================
-- STEP 2: Update stored procedure
-- =============================================

PRINT '2. Updating ConsultatieDiagnostic_Upsert stored procedure...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ConsultatieDiagnostic_Upsert]
    @ConsultatieID UNIQUEIDENTIFIER,
    -- Diagnostic Principal
    @CodICD10Principal NVARCHAR(20) = NULL,
    @NumeDiagnosticPrincipal NVARCHAR(500) = NULL,
    @DescriereDetaliataPrincipal NVARCHAR(MAX) = NULL,
    -- Legacy fields (kept for backwards compatibility)
    @DiagnosticPozitiv NVARCHAR(MAX) = NULL,
    @CoduriICD10 NVARCHAR(MAX) = NULL,
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
                [CodICD10Principal] = @CodICD10Principal,
                [NumeDiagnosticPrincipal] = @NumeDiagnosticPrincipal,
                [DescriereDetaliataPrincipal] = @DescriereDetaliataPrincipal,
                [DiagnosticPozitiv] = @DiagnosticPozitiv,
                [CoduriICD10] = @CoduriICD10,
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @ModificatDe
            WHERE [ConsultatieID] = @ConsultatieID;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[ConsultatieDiagnostic]
            (
                [Id], [ConsultatieID],
                [CodICD10Principal], [NumeDiagnosticPrincipal], [DescriereDetaliataPrincipal],
                [DiagnosticPozitiv], [CoduriICD10],
                [DataCreare], [CreatDe]
            )
            VALUES
            (
                NEWID(), @ConsultatieID,
                @CodICD10Principal, @NumeDiagnosticPrincipal, @DescriereDetaliataPrincipal,
                @DiagnosticPozitiv, @CoduriICD10,
                GETDATE(), @ModificatDe
            );
        END

        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT '   ✓ Updated ConsultatieDiagnostic_Upsert stored procedure';
PRINT '';
PRINT '=== MIGRATION 012 COMPLETE ===';
GO
