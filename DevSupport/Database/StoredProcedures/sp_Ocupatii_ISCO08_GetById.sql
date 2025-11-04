-- ========================================
-- Stored Procedure: sp_Ocupatii_ISCO08_GetById
-- Database: ValyanMed
-- Created: 10/08/2025 17:19:55
-- Modified: 10/08/2025 17:27:03
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

CREATE   PROCEDURE sp_Ocupatii_ISCO08_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.[Id],
        o.[Cod_ISCO],
        o.[Denumire_Ocupatie],
        o.[Denumire_Ocupatie_EN],
        o.[Nivel_Ierarhic],
        o.[Cod_Parinte],
        o.[Grupa_Majora],
        o.[Grupa_Majora_Denumire],
        o.[Subgrupa],
        o.[Subgrupa_Denumire],
        o.[Grupa_Minora],
        o.[Grupa_Minora_Denumire],
        o.[Descriere],
        o.[Este_Activ],
        o.[Data_Crearii],
        o.[Data_Ultimei_Modificari],
        -- Helper pentru UI
        UPPER(LEFT(REPLACE(CAST(o.[Id] AS NVARCHAR(36)), '-', ''), 8)) AS [IdScurt]
    FROM dbo.Ocupatii_ISCO08 o
    WHERE o.[Id] = @Id;
END
GO
