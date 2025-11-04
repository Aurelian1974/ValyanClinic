-- ========================================
-- Stored Procedure: sp_PersonalMedical_GetById
-- Database: ValyanMed
-- FIXED: Corectate numele coloanelor din Departamente
-- ========================================

USE [ValyanMed]
GO

-- =============================================
-- SP pentru obținerea unui personal medical după ID
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetById]
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
        -- FIXED: Folosește numele corecte de coloane
        d1.DenumireDepartament AS CategorieName,
        d2.DenumireDepartament AS SpecializareName,
        d3.DenumireDepartament AS SubspecializareName
    FROM PersonalMedical pm
    -- FIXED: Folosește IdDepartament în loc de DepartamentID
    LEFT JOIN Departamente d1 ON pm.CategorieID = d1.IdDepartament
    LEFT JOIN Departamente d2 ON pm.SpecializareID = d2.IdDepartament
    LEFT JOIN Departamente d3 ON pm.SubspecializareID = d3.IdDepartament
    WHERE pm.PersonalID = @PersonalID;
END;
GO
