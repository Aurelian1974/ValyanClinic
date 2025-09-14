CREATE PROCEDURE [dbo].[sp_Personal_GetStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Total Personal' as StatisticName,
        COUNT(*) as Value,
        'fas fa-users' as IconClass,
        '#007bff' as ColorClass
    FROM Personal
    
    UNION ALL
    
    SELECT 
        'Personal Activ' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-check' as IconClass,
        '#28a745' as ColorClass
    FROM Personal 
    WHERE Status_Angajat = 'Activ'
    
    UNION ALL
    
    SELECT 
        'Personal Inactiv' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-times' as IconClass,
        '#dc3545' as ColorClass
    FROM Personal 
    WHERE Status_Angajat = 'Inactiv';
END;