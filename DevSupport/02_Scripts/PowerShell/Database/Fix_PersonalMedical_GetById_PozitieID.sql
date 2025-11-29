-- =============================================
-- FIX DEFINITIV: sp_PersonalMedical_GetById cu PozitieID
-- Problema: SP nu returneaz? coloana PozitieID pentru dropdown
-- =============================================

USE [ValyanMed]
GO

PRINT '=============================================='
PRINT 'FIX DEFINITIV: sp_PersonalMedical_GetById'
PRINT 'Adaugare coloana PozitieID in SELECT'
PRINT '=============================================='
PRINT ''

-- Verifica SP existent si afiseaza structura
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_GetById')
BEGIN
    PRINT '[INFO] sp_PersonalMedical_GetById exista - va fi recreat'
    DROP PROCEDURE sp_PersonalMedical_GetById
END
ELSE
BEGIN
    PRINT '[INFO] sp_PersonalMedical_GetById nu exista - va fi creat'
END

PRINT ''
GO

-- =============================================
-- Recreaza SP cu PozitieID inclus in SELECT
-- =============================================
CREATE PROCEDURE sp_PersonalMedical_GetById
    @PersonalID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- DEBUG pentru verificare
    PRINT 'DEBUG sp_PersonalMedical_GetById called with PersonalID: ' + CAST(@PersonalID AS NVARCHAR(50))
    
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
        pm.PozitieID,              -- ? FIX: ADAUGAT PozitieID
  pm.SpecializareID,
        pm.SubspecializareID,
        -- JOINs pentru nume lookup
     d1.DenumireDepartament AS CategorieName,
        s1.Denumire AS SpecializareName,
        s2.Denumire AS SubspecializareName,
        p1.Denumire AS PozitieNume  -- ? FIX: ADAUGAT si nume pozitie din tabela Pozitii
    FROM PersonalMedical pm
    LEFT JOIN Departamente d1 ON pm.CategorieID = d1.IdDepartament
    LEFT JOIN Specializari s1 ON pm.SpecializareID = s1.Id
    LEFT JOIN Specializari s2 ON pm.SubspecializareID = s2.Id
    LEFT JOIN Pozitii p1 ON pm.PozitieID = p1.Id          -- ? FIX: ADAUGAT JOIN cu tabela Pozitii
    WHERE pm.PersonalID = @PersonalID;
    
    -- DEBUG pentru verificare rezultat
 IF @@ROWCOUNT > 0
    BEGIN
        PRINT 'DEBUG: Record g?sit ?i returnat'
    END
    ELSE
 BEGIN
      PRINT 'DEBUG: Niciun record g?sit pentru PersonalID: ' + CAST(@PersonalID AS NVARCHAR(50))
    END
END;
GO

PRINT '[OK] sp_PersonalMedical_GetById recreat cu PozitieID'
PRINT ''

-- =============================================
-- Verificare finala ca PozitieID e inclus
-- =============================================
PRINT '=============================================='
PRINT 'VERIFICARE FINALA'
PRINT '=============================================='

-- Test cu un record existent
PRINT 'Test cu primul record din PersonalMedical:'

DECLARE @TestPersonalID UNIQUEIDENTIFIER
SELECT TOP 1 @TestPersonalID = PersonalID FROM PersonalMedical

IF @TestPersonalID IS NOT NULL
BEGIN
    PRINT 'Testing cu PersonalID: ' + CAST(@TestPersonalID AS NVARCHAR(50))
    
    -- Rulare SP pentru test
    EXEC sp_PersonalMedical_GetById @TestPersonalID
    
    PRINT ''
    PRINT '? VERIFICA?I REZULTATUL DE MAI SUS!'
  PRINT ' - Ar trebui s? vede?i coloana PozitieID în rezultat'
    PRINT '   - Ar trebui s? vede?i coloana PozitieNume dac? PozitieID nu e NULL'
END
ELSE
BEGIN
    PRINT '? Nu exist? date în PersonalMedical pentru test'
    PRINT '   Ad?uga?i mai întâi date pentru a testa'
END

PRINT ''
PRINT '=============================================='
PRINT 'FIX APLICAT - PROBLEMA REZOLVAT?!'
PRINT '=============================================='
PRINT ''
PRINT 'CE A FOST REPARAT:'
PRINT '1. ? sp_PersonalMedical_GetById returneaz? acum PozitieID'
PRINT '2. ? Ad?ugat JOIN cu tabela Pozitii pentru PozitieNume'
PRINT '3. ? C# ApplicationLayer va primi PozitieID'
PRINT '4. ? PersonalMedicalFormModal va pre-selecta dropdown-ul'
PRINT ''
PRINT 'PASII URMATORI:'
PRINT '1. ?? Restarta?i aplica?ia Blazor'
PRINT '2. ?? Testa?i editarea unui record PersonalMedical'
PRINT '3. ?? Verifica?i c? dropdown-ul Pozi?ie este pre-selectat'
PRINT ''
PRINT 'Acum dropdown-ul pentru Pozi?ie ar trebui s? func?ioneze!'
PRINT '=============================================='