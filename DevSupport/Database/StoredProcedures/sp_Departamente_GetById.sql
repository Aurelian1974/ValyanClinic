-- ========================================
-- Stored Procedure: sp_Departamente_GetById
-- Database: ValyanMed
-- Created: 10/13/2025 14:54:02
-- Modified: 10/13/2025 14:54:02
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


CREATE PROCEDURE dbo.sp_Departamente_GetById
    @IdDepartament UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        d.IdDepartament,
        d.IdTipDepartament,
        d.DenumireDepartament,
        d.DescriereDepartament,
        td.DenumireTipDepartament
    FROM Departamente d
    LEFT JOIN TipDepartament td ON d.IdTipDepartament = td.IdTipDepartament
    WHERE d.IdDepartament = @IdDepartament;
END

GO
