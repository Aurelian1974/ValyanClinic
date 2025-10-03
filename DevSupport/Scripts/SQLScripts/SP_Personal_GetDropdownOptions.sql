-- SP pentru obtinerea optiunilor pentru dropdown-uri
CREATE PROCEDURE [dbo].[sp_Personal_GetDropdownOptions]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Departamente distincte
    SELECT DISTINCT 
        Departament as Value,
        Departament as Text
    FROM Personal 
    WHERE Departament IS NOT NULL 
    ORDER BY Departament;
    
    -- Functii distincte
    SELECT DISTINCT 
        Functia as Value,
        Functia as Text
    FROM Personal 
    WHERE Functia IS NOT NULL 
    ORDER BY Functia;
    
    -- Judete distincte
    SELECT DISTINCT 
        Judet_Domiciliu as Value,
        Judet_Domiciliu as Text
    FROM Personal 
    WHERE Judet_Domiciliu IS NOT NULL 
    ORDER BY Judet_Domiciliu;
END;