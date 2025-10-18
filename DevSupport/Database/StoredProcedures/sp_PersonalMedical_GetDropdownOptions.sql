-- ========================================
-- Stored Procedure: sp_PersonalMedical_GetDropdownOptions
-- Database: ValyanMed
-- Created: 09/22/2025 20:34:43
-- Modified: 09/22/2025 20:34:43
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

-- =============================================
-- SP pentru ob?inerea op?iunilor pentru dropdown-uri
-- =============================================
CREATE   PROCEDURE [dbo].[sp_PersonalMedical_GetDropdownOptions]
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
    
    -- Pozi?ii distincte
    SELECT DISTINCT 
        Pozitie as Value,
        Pozitie as Text
    FROM PersonalMedical 
    WHERE Pozitie IS NOT NULL 
    ORDER BY Pozitie;
    
    -- Specializ?ri distincte
    SELECT DISTINCT 
        Specializare as Value,
        Specializare as Text
    FROM PersonalMedical 
    WHERE Specializare IS NOT NULL 
    ORDER BY Specializare;
END;
GO
