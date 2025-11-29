-- ========================================
-- FIX: sp_Utilizatori_GetById - Add Pozitie column
-- Database: ValyanMed
-- Problema: SP nu returneaz? coloana Pozitie din PersonalMedical
-- Solu?ie: Adaug? pm.Pozitie în SELECT
-- Date: 2025-01-XX
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'FIX: sp_Utilizatori_GetById - Add Pozitie';
PRINT '========================================';
PRINT '';

-- Drop existing SP
IF OBJECT_ID('sp_Utilizatori_GetById', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE sp_Utilizatori_GetById
    PRINT '? Dropped old sp_Utilizatori_GetById'
END
GO

-- Create FIXED SP with Pozitie column
CREATE PROCEDURE sp_Utilizatori_GetById
  @UtilizatorID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
   -- Utilizator columns
        u.UtilizatorID,
        u.PersonalMedicalID,
        u.Username,
        u.Email,
        u.PasswordHash,
        u.Salt,
        u.Rol,
        u.EsteActiv,
    u.DataCreare,
u.DataUltimaAutentificare,
        u.NumarIncercariEsuate,
        u.DataBlocare,
    u.TokenResetareParola,
      u.DataExpirareToken,
        u.CreatDe,
        u.DataCrearii,
        u.ModificatDe,
        u.DataUltimeiModificari,
        
      -- PersonalMedical columns
        pm.Nume,
        pm.Prenume,
        pm.Nume + ' ' + pm.Prenume AS NumeComplet,
        pm.Specializare,
        pm.Departament,
        pm.Pozitie,  -- ? FIX: ADDED THIS COLUMN
        pm.Telefon,
 pm.Email AS EmailPersonalMedical
    FROM Utilizatori u
    INNER JOIN PersonalMedical pm ON u.PersonalMedicalID = pm.PersonalID
    WHERE u.UtilizatorID = @UtilizatorID;
END
GO

PRINT '? Created FIXED sp_Utilizatori_GetById with Pozitie column';
PRINT '';

-- Verification
PRINT '========================================';
PRINT 'VERIFICATION';
PRINT '========================================';

IF OBJECT_ID('sp_Utilizatori_GetById', 'P') IS NOT NULL
  PRINT '? sp_Utilizatori_GetById exists'
ELSE
    PRINT '? ERROR: sp_Utilizatori_GetById NOT created'

PRINT '';
PRINT 'TEST QUERY:';
PRINT 'EXEC sp_Utilizatori_GetById @UtilizatorID = ''[YOUR-GUID-HERE]''';
PRINT '';
PRINT 'Expected columns:';
PRINT '  - All Utilizator fields';
PRINT '  - Nume, Prenume, NumeComplet';
PRINT '  - Specializare, Departament';
PRINT '  - Pozitie  ? NEW COLUMN';
PRINT '  - Telefon, EmailPersonalMedical';
PRINT '';
PRINT '========================================';
PRINT 'FIX COMPLETE!';
PRINT '========================================';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Test the stored procedure with a real UtilizatorID';
PRINT '2. Restart your Blazor application';
PRINT '3. Open Detalii Utilizator modal';
PRINT '4. Check that Personal Medical tab shows Pozitie field';
PRINT '';
