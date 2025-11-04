-- ========================================
-- Stored Procedure: sp_Ocupatii_ISCO08_Create
-- Database: ValyanMed
-- Created: 10/08/2025 17:27:33
-- Modified: 10/08/2025 17:27:33
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

CREATE   PROCEDURE sp_Ocupatii_ISCO08_Create
    @CodISCO NVARCHAR(10),
    @DenumireOcupatie NVARCHAR(500),
    @NivelIerarhic TINYINT,
    @CodParinte NVARCHAR(10) = NULL,
    @GrupaMajora NVARCHAR(10) = NULL,
    @GrupaMajoraDenumire NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM dbo.Ocupatii_ISCO08 WHERE [Cod_ISCO] = @CodISCO)
    BEGIN
        SELECT 'EROARE: Codul ISCO exista deja' AS [Mesaj];
        RETURN -1;
    END
    
    INSERT INTO dbo.Ocupatii_ISCO08 (
        [Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic],
        [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire]
    ) VALUES (
        @CodISCO, @DenumireOcupatie, @NivelIerarhic,
        @CodParinte, @GrupaMajora, @GrupaMajoraDenumire
    );
    
    SELECT 
        [Id], [Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic]
    FROM dbo.Ocupatii_ISCO08 
    WHERE [Cod_ISCO] = @CodISCO;
END
GO
