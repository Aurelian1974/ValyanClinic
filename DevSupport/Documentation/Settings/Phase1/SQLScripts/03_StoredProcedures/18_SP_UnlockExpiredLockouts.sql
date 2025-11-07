-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Stored Procedure pentru unlock automat conturi expirate (SQL Agent Job)
-- Dependen?e: Tabele Utilizatori, Audit_Log
-- =============================================

PRINT '=== CREARE SP_UnlockExpiredLockouts ===';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_UnlockExpiredLockouts]') AND type in (N'P', N'PC'))
BEGIN
 DROP PROCEDURE [dbo].[SP_UnlockExpiredLockouts];
    PRINT '? Procedure existent? ?tears? (va fi recreat?).';
END
GO

CREATE PROCEDURE [dbo].[SP_UnlockExpiredLockouts]
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Utilizatori]
    SET [LockoutEndDateUtc] = NULL,
        [AccessFailedCount] = 0
    WHERE [LockoutEndDateUtc] IS NOT NULL
      AND [LockoutEndDateUtc] < GETDATE();
 
    -- Log
    DECLARE @UnlockedCount INT = @@ROWCOUNT;
    
  IF @UnlockedCount > 0
    BEGIN
   INSERT INTO [dbo].[Audit_Log] ([Actiune], [StatusActiune], [DetaliiEroare])
        VALUES ('AutoUnlock', 'Success', 'Unlocked ' + CAST(@UnlockedCount AS NVARCHAR) + ' accounts');
    END
 
    SELECT @UnlockedCount AS UnlockedAccounts;
END;
GO

PRINT '? SP_UnlockExpiredLockouts creat? cu succes.';
PRINT '? Parametri: Niciun';
PRINT '? Return: UnlockedAccounts count';
PRINT '! RECOMANDARE: Configureaz? SQL Server Agent Job s? ruleze la fiecare 5 minute.';
GO
