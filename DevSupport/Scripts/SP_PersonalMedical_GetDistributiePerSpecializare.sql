-- =============================================
-- SP pentru ob?inerea distribu?iei personalului medical pe specializ?ri
-- Folosit pentru statisticile din AdministrarePersonalMedical
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetDistributiePerSpecializare]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CASE 
            WHEN pm.Specializare IS NULL OR pm.Specializare = '' THEN 'Nespecializat'
            ELSE pm.Specializare
        END as Categorie,
        COUNT(*) as Numar
    FROM PersonalMedical pm
    WHERE pm.EsteActiv = 1  -- Doar personalul activ
    GROUP BY pm.Specializare
    ORDER BY Numar DESC, Categorie ASC;
END;