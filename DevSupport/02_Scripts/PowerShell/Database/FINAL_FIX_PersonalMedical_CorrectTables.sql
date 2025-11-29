-- =============================================
-- FINAL CORRECTED FIX: Personal Medical
-- USES: Departamente for CategorieID, Specializari for SpecializareID/SubspecializareID
-- Database: ValyanMed
-- =============================================

USE [ValyanMed]
GO

PRINT ''
PRINT '================================================'
PRINT ' FINAL FIX: PersonalMedical - Correct Tables'
PRINT '================================================'
PRINT ''

-- Drop old SP
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_GetById')
BEGIN
    DROP PROCEDURE [dbo].[sp_PersonalMedical_GetById]
    PRINT 'Step 1/2: Old SP dropped'
END
GO

-- Create CORRECTED SP
CREATE PROCEDURE [dbo].[sp_PersonalMedical_GetById]
    @PersonalID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pm.PersonalID,
        pm.Nume,
        pm.Prenume,
        pm.Specializare,
        pm.NumarLicenta,
        pm.Telefon,
        pm.Email,
        pm.Departament,
        pm.Pozitie,
        pm.EsteActiv,
        pm.DataCreare,
        pm.CategorieID,
        pm.SpecializareID,
        pm.SubspecializareID,
        -- CORRECTED: Use Departamente for Categorie
        d1.DenumireDepartament AS CategorieName,
        -- CORRECTED: Use Specializari for Specializare
        s1.Denumire AS SpecializareName,
        -- CORRECTED: Use Specializari for Subspecializare
        s2.Denumire AS SubspecializareName
    FROM PersonalMedical pm
    LEFT JOIN Departamente d1 ON pm.CategorieID = d1.IdDepartament
    LEFT JOIN Specializari s1 ON pm.SpecializareID = s1.Id
    LEFT JOIN Specializari s2 ON pm.SubspecializareID = s2.Id
    WHERE pm.PersonalID = @PersonalID;
END;
GO

PRINT 'Step 2/2: CORRECTED SP created'
PRINT ''

-- Test the SP
DECLARE @TestID UNIQUEIDENTIFIER = (SELECT TOP 1 PersonalID FROM PersonalMedical WHERE SpecializareID IS NOT NULL)
IF @TestID IS NOT NULL
BEGIN
    PRINT 'Testing SP with PersonalID: ' + CAST(@TestID AS VARCHAR(36))
    EXEC sp_PersonalMedical_GetById @PersonalID = @TestID
END

PRINT ''
PRINT '================================================'
PRINT ' FIX COMPLETE - SP uses correct tables now:'
PRINT '   - CategorieID   -> Departamente'
PRINT '   - SpecializareID -> Specializari'
PRINT '   - SubspecializareID -> Specializari'
PRINT '================================================'
