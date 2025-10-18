-- ========================================
-- Stored Procedure: sp_Ocupatii_ISCO08_Update
-- Database: ValyanMed
-- Created: 10/08/2025 17:27:33
-- Modified: 10/08/2025 17:27:33
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

CREATE   PROCEDURE sp_Ocupatii_ISCO08_Update
    @Id UNIQUEIDENTIFIER,
    @DenumireOcupatie NVARCHAR(500),
    @EsteActiv BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM dbo.Ocupatii_ISCO08 WHERE [Id] = @Id)
    BEGIN
        SELECT 'EROARE: ID-ul nu exista' AS [Mesaj];
        RETURN -1;
    END
    
    UPDATE dbo.Ocupatii_ISCO08
    SET 
        [Denumire_Ocupatie] = @DenumireOcupatie,
        [Este_Activ] = @EsteActiv,
        [Data_Ultimei_Modificari] = GETDATE()
    WHERE [Id] = @Id;
    
    SELECT 
        [Id], [Cod_ISCO], [Denumire_Ocupatie], [Este_Activ]
    FROM dbo.Ocupatii_ISCO08 
    WHERE [Id] = @Id;
END
GO
