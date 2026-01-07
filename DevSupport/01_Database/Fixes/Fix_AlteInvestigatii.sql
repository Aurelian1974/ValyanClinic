/*
==============================================================================
FIX SCRIPT: AlteInvestigatii - Verificare și Reparare
==============================================================================
Description: Verifică și repară coloana AlteInvestigatii în tabelul 
             ConsultatieInvestigatii și actualizează stored procedures
Author: System
Date: 2026-01-07
==============================================================================
INSTRUCȚIUNI: Rulează acest script în SSMS pe baza de date ValyanClinicDB
==============================================================================
*/

USE [ValyanClinicDB]
GO

PRINT '========== VERIFICARE ȘI REPARARE AlteInvestigatii =========='
PRINT ''

-- ============================================================================
-- PASUL 1: Verifică dacă coloana există în tabel
-- ============================================================================
PRINT '1. Verificare coloană AlteInvestigatii în tabel...'

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ConsultatieInvestigatii' 
    AND COLUMN_NAME = 'AlteInvestigatii'
)
BEGIN
    PRINT '   -> Coloana NU există. Se adaugă...'
    ALTER TABLE [dbo].[ConsultatieInvestigatii]
    ADD [AlteInvestigatii] NVARCHAR(MAX) NULL;
    PRINT '   -> Coloana AlteInvestigatii a fost adăugată!'
END
ELSE
BEGIN
    PRINT '   -> Coloana AlteInvestigatii există deja. OK!'
END
GO

-- ============================================================================
-- PASUL 2: Actualizează ConsultatieInvestigatii_Upsert
-- ============================================================================
PRINT ''
PRINT '2. Actualizare stored procedure ConsultatieInvestigatii_Upsert...'

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
        
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
        RETURN -1;
    END CATCH
END
GO

PRINT '   -> ConsultatieInvestigatii_Upsert actualizat!'
GO

-- ============================================================================
-- PASUL 3: Verificare finală
-- ============================================================================
PRINT ''
PRINT '3. Verificare finală...'
PRINT ''

-- Verifică coloana
SELECT 
    'Coloană în tabel' AS Verificare,
    TABLE_NAME AS Tabel,
    COLUMN_NAME AS Coloana,
    DATA_TYPE AS TipDate,
    'OK' AS Status
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ConsultatieInvestigatii' 
AND COLUMN_NAME = 'AlteInvestigatii';

-- Verifică SP
SELECT 
    'Stored Procedure' AS Verificare,
    name AS NumeSP,
    CASE 
        WHEN OBJECT_DEFINITION(object_id) LIKE '%@AlteInvestigatii%' THEN 'OK - Conține @AlteInvestigatii'
        ELSE 'EROARE - Lipsește @AlteInvestigatii'
    END AS Status
FROM sys.procedures 
WHERE name = 'ConsultatieInvestigatii_Upsert';

PRINT ''
PRINT '========== SCRIPT FINALIZAT =========='
PRINT 'Repornește aplicația pentru a testa!'
GO
