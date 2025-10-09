-- ========================================
-- Stored Procedure: sp_Judete_GetByCod
-- Database: ValyanMed
-- Created: 09/17/2025 08:41:52
-- Modified: 09/17/2025 09:22:01
-- Generat: 2025-10-08 16:36:44
-- ========================================

USE [ValyanMed]
GO


-- SP pentru obtinerea unui judet dupa cod
CREATE   PROCEDURE [dbo].[sp_Judete_GetByCod]
    @CodJudet NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdJudet,
        JudetGuid,
        CodJudet,
        Nume,
        Siruta,
        CodAuto,
        Ordine
    FROM Judet
    WHERE CodJudet = @CodJudet;
END;


GO
