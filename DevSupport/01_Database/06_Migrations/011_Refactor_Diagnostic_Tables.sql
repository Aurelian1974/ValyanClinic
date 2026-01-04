/*
==============================================================================
MIGRATION: 011_Refactor_Diagnostic_Tables
==============================================================================
Description: Restructurează tabelele de diagnostic pentru Scrisoare Medicală:
             - ConsultatieDiagnostic: simplificat pentru diagnostic principal
             - ConsultatieDiagnosticSecundar: NOU pentru diagnostice secundare (1:N)
             
Format afișare Scrisoare Medicală:
  Diagnostic principal: I10 - Hipertensiune arterială esențială (primară)
  Diagnostice secundare:
    1. E11.9 - Diabet zaharat tip 2 fără complicații
    2. E78.0 - Hipercolesterolemie pură

Author: AI Agent
Date: 2026-01-04
Database: ValyanMed
==============================================================================
*/

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'Migration: Refactor Diagnostic Tables';
PRINT 'Date: 2026-01-04';
PRINT '========================================';
PRINT '';

-- =============================================
-- 1. Add new columns to ConsultatieDiagnostic for Principal
-- =============================================
PRINT '1. Adding new columns to ConsultatieDiagnostic...';

-- Cod ICD10 Principal (singur)
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ConsultatieDiagnostic' 
    AND COLUMN_NAME = 'CodICD10Principal'
)
BEGIN
    ALTER TABLE [dbo].[ConsultatieDiagnostic]
    ADD [CodICD10Principal] NVARCHAR(20) NULL;
    PRINT '   ✓ Coloana CodICD10Principal adăugată';
END
ELSE
BEGIN
    PRINT '   → Coloana CodICD10Principal există deja';
END

-- Nume diagnostic principal (din catalog ICD10)
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ConsultatieDiagnostic' 
    AND COLUMN_NAME = 'NumeDiagnosticPrincipal'
)
BEGIN
    ALTER TABLE [dbo].[ConsultatieDiagnostic]
    ADD [NumeDiagnosticPrincipal] NVARCHAR(500) NULL;
    PRINT '   ✓ Coloana NumeDiagnosticPrincipal adăugată';
END
ELSE
BEGIN
    PRINT '   → Coloana NumeDiagnosticPrincipal există deja';
END

-- Descriere detaliată diagnostic principal (HTML din RTE)
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ConsultatieDiagnostic' 
    AND COLUMN_NAME = 'DescriereDetaliataPrincipal'
)
BEGIN
    ALTER TABLE [dbo].[ConsultatieDiagnostic]
    ADD [DescriereDetaliataPrincipal] NVARCHAR(MAX) NULL;
    PRINT '   ✓ Coloana DescriereDetaliataPrincipal adăugată';
END
ELSE
BEGIN
    PRINT '   → Coloana DescriereDetaliataPrincipal există deja';
END
GO

-- =============================================
-- 2. Create table ConsultatieDiagnosticSecundar
-- =============================================
PRINT '';
PRINT '2. Creating table ConsultatieDiagnosticSecundar...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConsultatieDiagnosticSecundar]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ConsultatieDiagnosticSecundar]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [ConsultatieID] UNIQUEIDENTIFIER NOT NULL,
        [OrdineAfisare] INT NOT NULL DEFAULT 1,
        
        -- Cod ICD10 pentru acest diagnostic secundar
        [CodICD10] NVARCHAR(20) NULL,
        [NumeDiagnostic] NVARCHAR(500) NULL,
        
        -- Descriere detaliată (HTML din RTE)
        [Descriere] NVARCHAR(MAX) NULL,
        
        -- Audit
        [DataCreare] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatDe] UNIQUEIDENTIFIER NOT NULL,
        [DataUltimeiModificari] DATETIME NULL,
        [ModificatDe] UNIQUEIDENTIFIER NULL,
        
        CONSTRAINT [PK_ConsultatieDiagnosticSecundar] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ConsultatieDiagnosticSecundar_Consultatie] 
            FOREIGN KEY ([ConsultatieID]) REFERENCES [dbo].[Consultatii]([ConsultatieID]) ON DELETE CASCADE
    );
    
    -- Index pentru lookup rapid
    CREATE NONCLUSTERED INDEX [IX_ConsultatieDiagnosticSecundar_ConsultatieID] 
        ON [dbo].[ConsultatieDiagnosticSecundar]([ConsultatieID])
        INCLUDE ([OrdineAfisare], [CodICD10], [NumeDiagnostic]);
    
    PRINT '   ✓ Tabelul ConsultatieDiagnosticSecundar creat cu succes';
END
ELSE
BEGIN
    PRINT '   → Tabelul ConsultatieDiagnosticSecundar există deja';
END
GO

-- =============================================
-- 3. Create stored procedure for Upsert Diagnostic Principal
-- =============================================
PRINT '';
PRINT '3. Creating/updating stored procedure ConsultatieDiagnostic_Upsert...';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieDiagnostic_Upsert')
    DROP PROCEDURE [dbo].[ConsultatieDiagnostic_Upsert]
GO

CREATE PROCEDURE [dbo].[ConsultatieDiagnostic_Upsert]
    @ConsultatieID UNIQUEIDENTIFIER,
    -- Diagnostic Principal (NOU)
    @CodICD10Principal NVARCHAR(20) = NULL,
    @NumeDiagnosticPrincipal NVARCHAR(500) = NULL,
    @DescriereDetaliataPrincipal NVARCHAR(MAX) = NULL,
    -- Legacy fields (backwards compatibility)
    @DiagnosticPozitiv NVARCHAR(MAX) = NULL,
    @DiagnosticDiferential NVARCHAR(MAX) = NULL,
    @DiagnosticEtiologic NVARCHAR(MAX) = NULL,
    @CoduriICD10 NVARCHAR(MAX) = NULL,
    @CoduriICD10Secundare NVARCHAR(MAX) = NULL,
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
                [CodICD10Principal], [NumeDiagnosticPrincipal], [DescriereDetaliataPrincipal],
                [DiagnosticPozitiv], [DiagnosticDiferential], [DiagnosticEtiologic],
                [CoduriICD10], [CoduriICD10Secundare],
                [DataCreare], [CreatDe]
            )
            VALUES
            (
                NEWID(), @ConsultatieID,
                @CodICD10Principal, @NumeDiagnosticPrincipal, @DescriereDetaliataPrincipal,
                @DiagnosticPozitiv, @DiagnosticDiferential, @DiagnosticEtiologic,
                @CoduriICD10, @CoduriICD10Secundare,
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

PRINT '   ✓ ConsultatieDiagnostic_Upsert creat cu succes';

-- =============================================
-- 4. Create stored procedure for Sync Diagnostice Secundare
-- =============================================
PRINT '';
PRINT '4. Creating stored procedure ConsultatieDiagnosticSecundar_Sync...';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieDiagnosticSecundar_Sync')
    DROP PROCEDURE [dbo].[ConsultatieDiagnosticSecundar_Sync]
GO

CREATE PROCEDURE [dbo].[ConsultatieDiagnosticSecundar_Sync]
    @ConsultatieID UNIQUEIDENTIFIER,
    @DiagnosticeJSON NVARCHAR(MAX),  -- JSON array: [{ordine, codICD10, numeDiagnostic, descriere}]
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- 1. Șterge toate diagnosticele secundare existente pentru această consultație
        DELETE FROM [dbo].[ConsultatieDiagnosticSecundar]
        WHERE [ConsultatieID] = @ConsultatieID;
        
        -- 2. Inserează noile diagnostice din JSON
        IF @DiagnosticeJSON IS NOT NULL AND LEN(@DiagnosticeJSON) > 2
        BEGIN
            INSERT INTO [dbo].[ConsultatieDiagnosticSecundar]
            (
                [Id], [ConsultatieID], [OrdineAfisare],
                [CodICD10], [NumeDiagnostic], [Descriere],
                [DataCreare], [CreatDe]
            )
            SELECT 
                NEWID(),
                @ConsultatieID,
                JSON_VALUE(j.value, '$.ordine'),
                JSON_VALUE(j.value, '$.codICD10'),
                JSON_VALUE(j.value, '$.numeDiagnostic'),
                JSON_VALUE(j.value, '$.descriere'),
                GETDATE(),
                @ModificatDe
            FROM OPENJSON(@DiagnosticeJSON) AS j;
        END
        
        COMMIT TRANSACTION;
        
        -- Return count of inserted records
        SELECT @@ROWCOUNT AS InsertedCount;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT '   ✓ ConsultatieDiagnosticSecundar_Sync creat cu succes';

-- =============================================
-- 5. Create stored procedure for Get Diagnostice Secundare
-- =============================================
PRINT '';
PRINT '5. Creating stored procedure ConsultatieDiagnosticSecundar_GetByConsultatie...';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieDiagnosticSecundar_GetByConsultatie')
    DROP PROCEDURE [dbo].[ConsultatieDiagnosticSecundar_GetByConsultatie]
GO

CREATE PROCEDURE [dbo].[ConsultatieDiagnosticSecundar_GetByConsultatie]
    @ConsultatieID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [ConsultatieID],
        [OrdineAfisare],
        [CodICD10],
        [NumeDiagnostic],
        [Descriere],
        [DataCreare],
        [CreatDe],
        [DataUltimeiModificari],
        [ModificatDe]
    FROM [dbo].[ConsultatieDiagnosticSecundar]
    WHERE [ConsultatieID] = @ConsultatieID
    ORDER BY [OrdineAfisare] ASC;
END
GO

PRINT '   ✓ ConsultatieDiagnosticSecundar_GetByConsultatie creat cu succes';

-- =============================================
-- Migration Complete
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'Migration completă!';
PRINT '';
PRINT 'Structură nouă:';
PRINT '- ConsultatieDiagnostic: CodICD10Principal, NumeDiagnosticPrincipal, DescriereDetaliataPrincipal';
PRINT '- ConsultatieDiagnosticSecundar: OrdineAfisare, CodICD10, NumeDiagnostic, Descriere';
PRINT '';
PRINT 'Format Scrisoare Medicală:';
PRINT '  Diagnostic principal: I10 - Hipertensiune arterială';
PRINT '  Diagnostice secundare:';
PRINT '    1. E11.9 - Diabet zaharat tip 2';
PRINT '    2. E78.0 - Hipercolesterolemie';
PRINT '========================================';
GO
