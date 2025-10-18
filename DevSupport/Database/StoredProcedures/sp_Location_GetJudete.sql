-- ========================================
-- Stored Procedure: sp_Location_GetJudete
-- Database: ValyanMed
-- Created: 10/03/2025 14:28:26
-- Modified: 10/03/2025 14:28:26
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


    CREATE PROCEDURE [dbo].[sp_Location_GetJudete]
    AS
    BEGIN
        SET NOCOUNT ON;
        EXEC sp_Judete_GetAll;
    END
    
GO
