-- ========================================
-- Stored Procedure: sp_Departamente_Delete
-- Database: ValyanMed
-- Created: 10/13/2025 14:54:02
-- Modified: 10/13/2025 14:54:02
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


CREATE PROCEDURE dbo.sp_Departamente_Delete
    @IdDepartament UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM Departamente
    WHERE IdDepartament = @IdDepartament;
    
    SELECT 1 AS Success;
END

GO
