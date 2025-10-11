-- ========================================
-- Stored Procedure: sp_Location_GetLocalitateNameById
-- Database: ValyanMed
-- Created: 10/03/2025 14:28:26
-- Modified: 10/03/2025 14:28:26
-- Generat: 2025-10-08 16:36:44
-- ========================================

USE [ValyanMed]
GO


    CREATE PROCEDURE [dbo].[sp_Location_GetLocalitateNameById]
        @LocalitateId INT
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT Nume 
        FROM Localitati 
        WHERE Id = @LocalitateId;
    END
    
GO
