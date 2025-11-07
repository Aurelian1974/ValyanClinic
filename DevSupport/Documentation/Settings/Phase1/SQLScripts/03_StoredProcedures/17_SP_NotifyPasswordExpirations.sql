-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Stored Procedure pentru notificări expirare parole (SQL Agent Job)
-- Dependențe: Tabele Utilizatori, Audit_Log
-- =============================================

PRINT '=== CREARE SP_NotifyPasswordExpirations ===';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_NotifyPasswordExpirations]') AND type in (N'P', N'PC'))
BEGIN
 DROP PROCEDURE [dbo].[SP_NotifyPasswordExpirations];
    PRINT '✓ Procedure existentă ștearsă (va fi recreată).';
END
GO

CREATE PROCEDURE [dbo].[SP_NotifyPasswordExpirations]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Selectează utilizatori cu parole care expiră în 7 zile
    SELECT 
        [UtilizatorID],
        [Username],
        [Email],
     DATEDIFF(DAY, GETDATE(), [PasswordExpirationDate]) AS DaysUntilExpiration
    INTO #UsersToNotify
    FROM [dbo].[Utilizatori]
  WHERE [PasswordExpirationDate] BETWEEN GETDATE() AND DATEADD(DAY, 7, GETDATE())
      AND [EsteActiv] = 1;
    
    DECLARE @NotifyCount INT = @@ROWCOUNT;
    
    -- Log notificări (integrarea cu sistem notificări email/SMS se va face în .NET)
  IF @NotifyCount > 0
 BEGIN
        INSERT INTO [dbo].[Audit_Log] ([Actiune], [StatusActiune], [DetaliiEroare])
        SELECT 
        'PasswordExpirationNotification',
   'Pending',
 'User: ' + [Username] + ', Days remaining: ' + CAST(DaysUntilExpiration AS NVARCHAR)
        FROM #UsersToNotify;
    END
    
    -- Return lista utilizatori pentru procesare în .NET
    SELECT * FROM #UsersToNotify ORDER BY DaysUntilExpiration ASC;
    
    DROP TABLE #UsersToNotify;
END;
GO

PRINT '✓ SP_NotifyPasswordExpirations creată cu succes.';
PRINT '✓ Parametri: Niciun';
PRINT '✓ Return: Lista utilizatori cu parole care expiră în 7 zile';
PRINT '! RECOMANDARE: Configurează SQL Server Agent Job să ruleze zilnic la 08:00.';
GO
