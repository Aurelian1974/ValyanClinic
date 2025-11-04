-- ========================================
-- Stored Procedure: sp_Personal_GetStatistics
-- Database: ValyanMed
-- Created: 09/14/2025 16:56:11
-- Modified: 09/14/2025 16:56:11
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


CREATE PROCEDURE sp_Personal_GetStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Total Personal' AS StatisticName,
        COUNT(*) AS Value,
        'fas fa-users' AS IconClass,
        'stat-blue' AS ColorClass
    FROM Personal
    
    UNION ALL
    
    SELECT 
        'Personal Activ' AS StatisticName,
        COUNT(*) AS Value,
        'fas fa-user-check' AS IconClass,
        'stat-green' AS ColorClass
    FROM Personal 
    WHERE Status_Angajat = 'Activ'
    
    UNION ALL
    
    SELECT 
        'Personal Inactiv' AS StatisticName,
        COUNT(*) AS Value,
        'fas fa-user-times' AS IconClass,
        'stat-red' AS ColorClass
    FROM Personal 
    WHERE Status_Angajat = 'Inactiv';
END

GO
