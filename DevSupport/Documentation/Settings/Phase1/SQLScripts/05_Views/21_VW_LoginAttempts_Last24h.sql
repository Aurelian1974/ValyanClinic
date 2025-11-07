-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: View pentru tentative login (ultimele 24h)
-- Dependențe: Tabel Audit_Log
-- =============================================

PRINT '=== CREARE VIEW VW_LoginAttempts_Last24h ===';

IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VW_LoginAttempts_Last24h]'))
BEGIN
    DROP VIEW [dbo].[VW_LoginAttempts_Last24h];
    PRINT '✓ View existentă ștearsă (va fi recreată).';
END
GO

CREATE VIEW [dbo].[VW_LoginAttempts_Last24h]
AS
SELECT 
 [AuditID],
    [UtilizatorID],
    [UserName],
  [AdresaIP],
  [Actiune],
    [StatusActiune],
    [DataActiune],
    [UserAgent],
    [DetaliiEroare]
FROM [dbo].[Audit_Log]
WHERE [Actiune] IN ('Login', 'LoginFailed')
  AND [DataActiune] >= DATEADD(HOUR, -24, GETDATE());
GO

PRINT '✓ VW_LoginAttempts_Last24h creată cu succes.';
PRINT '✓ Coloane: AuditID, UtilizatorID, UserName, AdresaIP, Actiune, StatusActiune, DataActiune, UserAgent, DetaliiEroare';
PRINT '✓ Filtru: Ultimele 24 ore';
GO

-- Test view
/*
SELECT * FROM VW_LoginAttempts_Last24h ORDER BY DataActiune DESC;
SELECT StatusActiune, COUNT(*) AS Total FROM VW_LoginAttempts_Last24h GROUP BY StatusActiune;
*/
GO
