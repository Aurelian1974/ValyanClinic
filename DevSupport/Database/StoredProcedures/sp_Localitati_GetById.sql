-- ========================================
-- Stored Procedure: sp_Localitati_GetById
-- Database: ValyanMed
-- Created: 09/17/2025 08:42:02
-- Modified: 09/17/2025 09:22:10
-- Generat: 2025-10-08 16:36:44
-- ========================================

USE [ValyanMed]
GO


-- SP pentru obtinerea unei localitati dupa ID
CREATE   PROCEDURE [dbo].[sp_Localitati_GetById]
    @IdOras INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdOras,
        LocalitateGuid,
        IdJudet,
        Nume,
        Siruta,
        IdTipLocalitate,
        CodLocalitate
    FROM Localitate
    WHERE IdOras = @IdOras;
END;


GO
