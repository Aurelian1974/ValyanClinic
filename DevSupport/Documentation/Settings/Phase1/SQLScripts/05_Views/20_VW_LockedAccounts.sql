-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: View pentru conturi lockuite (monitoring)
-- Dependențe: Tabel Utilizatori
-- =============================================

PRINT '=== CREARE VIEW VW_LockedAccounts ===';

IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VW_LockedAccounts]'))
BEGIN
    DROP VIEW [dbo].[VW_LockedAccounts];
    PRINT '✓ View existentă ștearsă (va fi recreată).';
END
GO

CREATE VIEW [dbo].[VW_LockedAccounts]
AS
SELECT 
    [UtilizatorID],
    [Username],
    [Email],
    [AccessFailedCount],
    [LockoutEndDateUtc],
    DATEDIFF(MINUTE, GETDATE(), [LockoutEndDateUtc]) AS MinuteRemaining,
    [LastLoginIP],
    [LastLoginDate]
FROM [dbo].[Utilizatori]
WHERE [LockoutEnabled] = 1
  AND [LockoutEndDateUtc] IS NOT NULL
  AND [LockoutEndDateUtc] > GETUTCDATE();
GO

PRINT '✓ VW_LockedAccounts creată cu succes.';
PRINT '✓ Coloane: UtilizatorID, UserName, Email, AccessFailedCount, LockoutEndDateUtc, MinuteRemaining, LastLoginIP, LastLoginDate';
GO

-- Test view
/*
SELECT * FROM VW_LockedAccounts ORDER BY MinuteRemaining DESC;
*/
GO
