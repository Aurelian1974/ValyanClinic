-- =============================================
-- SP pentru obtinerea statisticilor personal medical
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Total Personal Medical' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-md' as IconClass,
        '#007bff' as ColorClass
    FROM PersonalMedical
    
    UNION ALL
    
    SELECT 
        'Personal Activ' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-check' as IconClass,
        '#28a745' as ColorClass
    FROM PersonalMedical 
    WHERE EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Personal Inactiv' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-times' as IconClass,
        '#dc3545' as ColorClass
    FROM PersonalMedical 
    WHERE EsteActiv = 0
    
    UNION ALL
    
    SELECT 
        'Doctori' as StatisticName,
        COUNT(*) as Value,
        'fas fa-stethoscope' as IconClass,
        '#17a2b8' as ColorClass
    FROM PersonalMedical 
    WHERE Pozitie LIKE '%Doctor%' AND EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Asistenti Medicali' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-nurse' as IconClass,
        '#6f42c1' as ColorClass
    FROM PersonalMedical 
    WHERE Pozitie LIKE '%Asistent%' AND EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Tehnicieni' as StatisticName,
        COUNT(*) as Value,
        'fas fa-microscope' as IconClass,
        '#fd7e14' as ColorClass
    FROM PersonalMedical 
    WHERE Pozitie LIKE '%Tehnician%' AND EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Receptioneri' as StatisticName,
        COUNT(*) as Value,
        'fas fa-desktop' as IconClass,
        '#6c757d' as ColorClass
    FROM PersonalMedical 
    WHERE Pozitie LIKE '%Receptioner%' AND EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Cu Licenta' as StatisticName,
        COUNT(*) as Value,
        'fas fa-certificate' as IconClass,
        '#e83e8c' as ColorClass
    FROM PersonalMedical 
    WHERE NumarLicenta IS NOT NULL AND NumarLicenta != '' AND EsteActiv = 1;
END;