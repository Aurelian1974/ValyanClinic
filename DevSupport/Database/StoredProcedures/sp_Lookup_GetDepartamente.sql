-- ========================================
-- Stored Procedure: sp_Lookup_GetDepartamente
-- Database: ValyanMed
-- Created: 10/03/2025 14:28:26
-- Modified: 10/03/2025 14:28:26
-- Generat: 2025-10-08 16:36:44
-- ========================================

USE [ValyanMed]
GO


    CREATE PROCEDURE [dbo].[sp_Lookup_GetDepartamente]
    AS
    BEGIN
        SET NOCOUNT ON;
        EXEC sp_Departamente_GetAll;
    END
    
GO
