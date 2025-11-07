-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Function verificare parol? în istoric
-- Dependen?e: Tabele PasswordHistory, Setari_Sistem
-- =============================================

PRINT '=== CREARE FUNCTION FN_IsPasswordInHistory ===';

-- ?tergere function existent?
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FN_IsPasswordInHistory]') AND type in (N'FN', N'IF', N'TF'))
BEGIN
    DROP FUNCTION [dbo].[FN_IsPasswordInHistory];
    PRINT '? Function existent? ?tears? (va fi recreat?).';
END
GO

-- Creare function
CREATE FUNCTION [dbo].[FN_IsPasswordInHistory]
(
    @UtilizatorID UNIQUEIDENTIFIER,
    @NewPasswordHash NVARCHAR(MAX)
)
RETURNS BIT
AS
BEGIN
    DECLARE @IsInHistory BIT = 0;
    DECLARE @PasswordHistoryCount INT;
    
    -- Get history count from settings
    SELECT @PasswordHistoryCount = CAST([Valoare] AS INT)
    FROM [dbo].[Setari_Sistem] 
    WHERE [Categorie] = 'Autentificare' AND [Cheie] = 'PasswordHistoryCount';
    
    -- Check if password exists in history (last N passwords)
   IF EXISTS (
     SELECT TOP (@PasswordHistoryCount) 1 
   FROM [dbo].[PasswordHistory]
      WHERE [UtilizatorID] = @UtilizatorID
          AND [PasswordHash] = @NewPasswordHash
    ORDER BY [DataCrearii] DESC
  )
    BEGIN
        SET @IsInHistory = 1;
    END
    
    RETURN @IsInHistory;
END;
GO

PRINT '? Function FN_IsPasswordInHistory creat? cu succes.';
PRINT '? Parametri: @UtilizatorID, @NewPasswordHash';
PRINT '? Return: BIT (1 = parol? în istoric, 0 = parol? nou?)';
GO

-- Test function (op?ional)
/*
DECLARE @TestResult BIT;
EXEC @TestResult = dbo.FN_IsPasswordInHistory 
    @UtilizatorID = 'user-guid-here', 
    @NewPasswordHash = 'test-hash';
SELECT @TestResult AS IsInHistory;
*/
GO
