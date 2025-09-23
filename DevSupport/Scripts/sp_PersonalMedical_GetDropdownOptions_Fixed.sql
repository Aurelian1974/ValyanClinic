-- =============================================
-- SP pentru obinerea opiunilor pentru dropdown-uri
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetDropdownOptions]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Departamente distincte
    SELECT DISTINCT 
        Departament as Value,
        Departament as Text
    FROM PersonalMedical 
    WHERE Departament IS NOT NULL 
    ORDER BY Departament;
    
    -- Poziii distincte
    SELECT DISTINCT 
        Pozitie as Value,
        Pozitie as Text
    FROM PersonalMedical 
    WHERE Pozitie IS NOT NULL 
    ORDER BY Pozitie;
    
    -- Specializri distincte
    SELECT DISTINCT 
        Specializare as Value,
        Specializare as Text
    FROM PersonalMedical 
    WHERE Specializare IS NOT NULL 
    ORDER BY Specializare;
END;