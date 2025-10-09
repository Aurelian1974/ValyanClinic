-- ========================================
-- Stored Procedure: sp_Personal_Delete
-- Database: ValyanMed
-- Created: 09/14/2025 16:56:11
-- Modified: 09/14/2025 16:56:11
-- Generat: 2025-10-08 16:36:44
-- ========================================

USE [ValyanMed]
GO


CREATE PROCEDURE sp_Personal_Delete
    @Id_Personal UNIQUEIDENTIFIER,
    @Modificat_De NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Data_Ultimei_Modificari DATETIME = GETUTCDATE();
    
    UPDATE Personal SET
        Status_Angajat = 'Inactiv',
        Data_Ultimei_Modificari = @Data_Ultimei_Modificari,
        Modificat_De = @Modificat_De
    WHERE Id_Personal = @Id_Personal;
    
    -- Returneaza 1 pentru success, 0 pentru failure
    SELECT CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END AS Result;
END

GO
