-- ========================================
-- Stored Procedure: sp_Location_GetJudetNameById
-- Database: ValyanMed
-- Created: 10/03/2025 14:28:26
-- Modified: 10/03/2025 14:28:26
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


    CREATE PROCEDURE [dbo].[sp_Location_GetJudetNameById]
        @JudetId INT
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT Nume 
        FROM Judete 
        WHERE Id = @JudetId;
    END
    
GO
