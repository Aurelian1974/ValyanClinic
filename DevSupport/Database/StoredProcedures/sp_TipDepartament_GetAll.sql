-- ========================================
-- Stored Procedure: sp_TipDepartament_GetAll
-- Database: ValyanMed
-- Created: 10/13/2025 14:54:02
-- Modified: 10/13/2025 14:54:02
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


CREATE PROCEDURE dbo.sp_TipDepartament_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdTipDepartament,
        DenumireTipDepartament
    FROM TipDepartament
    ORDER BY DenumireTipDepartament;
END

GO
