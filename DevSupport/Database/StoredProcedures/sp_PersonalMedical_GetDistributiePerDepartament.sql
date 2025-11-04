-- ========================================
-- Stored Procedure: sp_PersonalMedical_GetDistributiePerDepartament
-- Database: ValyanMed
-- Created: 09/23/2025 09:30:33
-- Modified: 09/23/2025 09:30:33
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

-- =============================================
-- SP pentru obținerea distribuției personalului medical pe departamente
-- Folosit pentru statisticile din AdministrarePersonalMedical
-- =============================================
CREATE   PROCEDURE [dbo].[sp_PersonalMedical_GetDistributiePerDepartament]
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
GO
