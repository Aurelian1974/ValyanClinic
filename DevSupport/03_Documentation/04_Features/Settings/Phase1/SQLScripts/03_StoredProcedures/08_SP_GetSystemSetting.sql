-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Stored Procedure pentru ob?inere setare sistem
-- Dependen?e: Tabel Setari_Sistem
-- =============================================

PRINT '=== CREARE SP_GetSystemSetting ===';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetSystemSetting]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_GetSystemSetting];
    PRINT '? Procedure existent? ?tears? (va fi recreat?).';
END
GO

CREATE PROCEDURE [dbo].[SP_GetSystemSetting]
    @Categorie NVARCHAR(100),
    @Cheie NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [SetareID],
        [Categorie],
        [Cheie],
   [Valoare],
        [TipDate],
        [Descriere],
   [ValoareDefault],
        [EsteEditabil],
     [DataCrearii],
     [DataModificarii],
        [ModificatDe]
    FROM [dbo].[Setari_Sistem]
    WHERE [Categorie] = @Categorie AND [Cheie] = @Cheie;
END;
GO

PRINT '? SP_GetSystemSetting creat? cu succes.';
PRINT '? Parametri: @Categorie, @Cheie';
PRINT '? Return: Recordset cu detalii setare';
GO

-- Test (op?ional)
/*
EXEC SP_GetSystemSetting 
    @Categorie = 'Autentificare', 
    @Cheie = 'PasswordMinLength';
*/
GO
