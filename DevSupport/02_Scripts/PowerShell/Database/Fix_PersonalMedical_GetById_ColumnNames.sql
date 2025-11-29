-- =============================================
-- FIX URGENT: sp_PersonalMedical_GetById
-- Problema: Query folosea nume gre?ite de coloane (DepartamentID, Nume)
-- Fix: Actualizat cu nume corecte (IdDepartament, DenumireDepartament)
-- =============================================

USE [ValyanMed]
GO

PRINT '========================================='
PRINT 'FIX sp_PersonalMedical_GetById'
PRINT '========================================='
PRINT ''

-- Drop procedura existent? (dac? exist?)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_GetById')
BEGIN
    DROP PROCEDURE [dbo].[sp_PersonalMedical_GetById]
    PRINT '? Procedura veche ?tears?'
END
GO

-- Creeaz? procedura corectat?
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
        -- FIXED: Folose?te numele corecte de coloane
        d1.DenumireDepartament AS CategorieName,
        d2.DenumireDepartament AS SpecializareName,
        d3.DenumireDepartament AS SubspecializareName
    FROM PersonalMedical pm
    -- FIXED: Folose?te IdDepartament în loc de DepartamentID
    LEFT JOIN Departamente d1 ON pm.CategorieID = d1.IdDepartament
    LEFT JOIN Departamente d2 ON pm.SpecializareID = d2.IdDepartament
    LEFT JOIN Departamente d3 ON pm.SubspecializareID = d3.IdDepartament
    WHERE pm.PersonalID = @PersonalID;
END;
GO

PRINT '? Procedura sp_PersonalMedical_GetById recreat? cu succes'
PRINT ''

-- Verificare
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_GetById')
    PRINT '? SUCCES: sp_PersonalMedical_GetById exist? în baza de date'
ELSE
    PRINT '? EROARE: sp_PersonalMedical_GetById NU a fost creat?'

PRINT ''
PRINT '========================================='
PRINT 'FIX COMPLET!'
PRINT '========================================='
PRINT ''
PRINT 'NEXT STEPS:'
PRINT '1. Testeaz? pagina Personal Medical din aplica?ie'
PRINT '2. Verific? c? nu mai apar erori "Invalid column name"'
PRINT '3. Verific? c? datele se încarc? corect'
PRINT ''
