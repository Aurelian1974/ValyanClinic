-- ========================================
-- Stored Procedure: sp_Ocupatii_ISCO08_Delete
-- Database: ValyanMed
-- Created: 10/08/2025 17:27:33
-- Modified: 10/08/2025 17:27:33
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

CREATE   PROCEDURE sp_Ocupatii_ISCO08_Delete
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM dbo.Ocupatii_ISCO08 WHERE [Id] = @Id)
    BEGIN
        SELECT 'EROARE: ID-ul nu exista' AS [Mesaj];
        RETURN -1;
    END
    
    DELETE FROM dbo.Ocupatii_ISCO08 WHERE [Id] = @Id;
    
    SELECT 'Inregistrare stearsa cu succes' AS [Mesaj];
END
GO
