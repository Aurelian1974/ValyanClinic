-- ========================================
-- Stored Procedure: SP_GetSystemSettingsByCategory
-- Description: Returneaza toate setarile dintr-o categorie
-- ========================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetSystemSettingsByCategory]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_GetSystemSettingsByCategory]
GO

CREATE PROCEDURE [dbo].[SP_GetSystemSettingsByCategory]
    @Categorie NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        SetareID,
        Categorie,
        Cheie,
        Valoare,
        TipDate,
        Descriere,
        ValoareDefault,
        EsteEditabil,
        DataCrearii,
        DataModificarii,
        ModificatDe
    FROM Setari_Sistem
    WHERE Categorie = @Categorie
    ORDER BY Cheie;
END
GO
