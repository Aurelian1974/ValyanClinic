-- =============================================
-- SP pentru ob?inerea statisticilor personal medical
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Total Personal Medical' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-md' as IconClass,
        'stat-blue' as ColorClass
    FROM PersonalMedical
    
    UNION ALL
    
    SELECT 
        'Personal Activ' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-check' as IconClass,
        'stat-green' as ColorClass
    FROM PersonalMedical 
    WHERE EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Personal Inactiv' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-times' as IconClass,
        'stat-red' as ColorClass
    FROM PersonalMedical 
    WHERE EsteActiv = 0
    
    UNION ALL
    
    SELECT 
        'Doctori' as StatisticName,
        COUNT(*) as Value,
        'fas fa-stethoscope' as IconClass,
        'stat-info' as ColorClass
    FROM PersonalMedical 
    WHERE Pozitie LIKE '%Doctor%' AND EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Asistenti Medicali' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-nurse' as IconClass,
        'stat-purple' as ColorClass
    FROM PersonalMedical 
    WHERE Pozitie LIKE '%Asistent%' AND EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Tehnicieni' as StatisticName,
        COUNT(*) as Value,
        'fas fa-microscope' as IconClass,
        'stat-orange' as ColorClass
    FROM PersonalMedical 
    WHERE Pozitie LIKE '%Tehnician%' AND EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Receptioneri' as StatisticName,
        COUNT(*) as Value,
        'fas fa-desktop' as IconClass,
        'stat-gray' as ColorClass
    FROM PersonalMedical 
    WHERE Pozitie LIKE '%Receptioner%' AND EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'Cu Licenta' as StatisticName,
        COUNT(*) as Value,
        'fas fa-certificate' as IconClass,
        'stat-pink' as ColorClass
    FROM PersonalMedical 
    WHERE NumarLicenta IS NOT NULL AND NumarLicenta != '' AND EsteActiv = 1;
END;