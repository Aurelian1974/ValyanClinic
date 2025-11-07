-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: View pentru parole care expiră în următoarele 7 zile
-- Dependențe: Tabel Utilizatori
-- =============================================

PRINT '=== CREARE VIEW VW_PasswordExpirations_Next7Days ===';

IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VW_PasswordExpirations_Next7Days]'))
BEGIN
    DROP VIEW [dbo].[VW_PasswordExpirations_Next7Days];
    PRINT '✓ View existentă ștearsă (va fi recreată).';
END
GO

CREATE VIEW [dbo].[VW_PasswordExpirations_Next7Days]
AS
SELECT 
    [UtilizatorID],
    [Username],
    [Email],
    [LastPasswordChangedDate],
    [PasswordExpirationDate],
    DATEDIFF(DAY, GETDATE(), [PasswordExpirationDate]) AS DaysUntilExpiration,
    DATEDIFF(DAY, [LastPasswordChangedDate], GETDATE()) AS DaysSinceLastChange
FROM [dbo].[Utilizatori]
WHERE [PasswordExpirationDate] IS NOT NULL
  AND [PasswordExpirationDate] BETWEEN GETDATE() AND DATEADD(DAY, 7, GETDATE())
  AND [EsteActiv] = 1;
GO

PRINT '✓ VW_PasswordExpirations_Next7Days creată cu succes.';
PRINT '✓ Coloane: UtilizatorID, UserName, Email, LastPasswordChangedDate, PasswordExpirationDate, DaysUntilExpiration, DaysSinceLastChange';
PRINT '✓ Filtru: Parole care expiră în următoarele 7 zile';
GO

-- Test view
/*
SELECT * FROM VW_PasswordExpirations_Next7Days ORDER BY DaysUntilExpiration ASC;
*/
GO
