-- =============================================
-- SP pentru obinerea unui personal medical dup? ID
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
        d1.Nume AS CategorieName,
        d2.Nume AS SpecializareName,
        d3.Nume AS SubspecializareName
    FROM PersonalMedical pm
    LEFT JOIN Departamente d1 ON pm.CategorieID = d1.DepartamentID
    LEFT JOIN Departamente d2 ON pm.SpecializareID = d2.DepartamentID
    LEFT JOIN Departamente d3 ON pm.SubspecializareID = d3.DepartamentID
    WHERE pm.PersonalID = @PersonalID;
END;