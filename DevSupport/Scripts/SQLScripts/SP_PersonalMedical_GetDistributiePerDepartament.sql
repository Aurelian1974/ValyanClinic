-- =============================================
-- SP pentru obinerea distribuiei personalului medical pe departamente
-- Folosit pentru statisticile din AdministrarePersonalMedical
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetDistributiePerDepartament]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ISNULL(pm.Departament, 'Nedefinit') as Categorie,
        COUNT(*) as Numar
    FROM PersonalMedical pm
    WHERE pm.EsteActiv = 1  -- Doar personalul activ
    GROUP BY pm.Departament
    ORDER BY Numar DESC, Categorie ASC;
END;