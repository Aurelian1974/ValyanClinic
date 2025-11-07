-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Function verificare cont lockuit
-- Dependen?e: Tabel Utilizatori
-- =============================================

PRINT '=== CREARE FUNCTION FN_IsAccountLocked ===';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FN_IsAccountLocked]') AND type in (N'FN', N'IF', N'TF'))
BEGIN
  DROP FUNCTION [dbo].[FN_IsAccountLocked];
    PRINT '? Function existent? ?tears? (va fi recreat?).';
END
GO

CREATE FUNCTION [dbo].[FN_IsAccountLocked]
(
    @UtilizatorID UNIQUEIDENTIFIER
)
RETURNS BIT
AS
BEGIN
DECLARE @IsLocked BIT = 0;
    
    -- Check if account is locked and lockout hasn't expired
    IF EXISTS (
        SELECT 1 
        FROM [dbo].[Utilizatori]
        WHERE [UtilizatorID] = @UtilizatorID
          AND [LockoutEnabled] = 1
 AND [LockoutEndDateUtc] IS NOT NULL
   AND [LockoutEndDateUtc] > GETUTCDATE()
  )
    BEGIN
SET @IsLocked = 1;
    END
    
    RETURN @IsLocked;
END;
GO

PRINT '? Function FN_IsAccountLocked creat? cu succes.';
PRINT '? Parametri: @UtilizatorID';
PRINT '? Return: BIT (1 = cont lockuit, 0 = cont activ)';
GO
