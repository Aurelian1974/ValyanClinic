-- ========================================
-- Stored Procedure: sp_PersonalMedical_GetDistributiePerSpecializare
-- Database: ValyanMed
-- Created: 09/23/2025 09:30:24
-- Modified: 09/23/2025 09:30:24
-- Generat: 2025-10-08 16:36:44
-- ========================================

USE [ValyanMed]
GO

-- =============================================
-- SP pentru obținerea distribuției personalului medical pe specializări
-- Folosit pentru statisticile din AdministrarePersonalMedical
-- =============================================
CREATE   PROCEDURE [dbo].[sp_PersonalMedical_GetDistributiePerSpecializare]
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
GO
