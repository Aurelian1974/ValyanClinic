-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: View pentru sesiuni active (monitoring)
-- Dependențe: Tabele UserSessions, Utilizatori
-- =============================================

PRINT '=== CREARE VIEW VW_ActiveSessions ===';

IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VW_ActiveSessions]'))
BEGIN
    DROP VIEW [dbo].[VW_ActiveSessions];
    PRINT '✓ View existentă ștearsă (va fi recreată).';
END
GO

CREATE VIEW [dbo].[VW_ActiveSessions]
AS
SELECT 
    s.[SessionID],
    s.[UtilizatorID],
    u.[Username],
    u.[Email],
    s.[AdresaIP],
    s.[Dispozitiv],
    s.[DataCreare],
  s.[DataUltimaActivitate],
    s.[DataExpirare],
    DATEDIFF(MINUTE, s.[DataUltimaActivitate], GETDATE()) AS MinuteInactive,
    DATEDIFF(MINUTE, GETDATE(), s.[DataExpirare]) AS MinuteUntilExpiration
FROM [dbo].[UserSessions] s
INNER JOIN [dbo].[Utilizatori] u ON s.[UtilizatorID] = u.[UtilizatorID]
WHERE s.[EsteActiva] = 1
  AND s.[DataExpirare] > GETDATE();
GO

PRINT '✓ VW_ActiveSessions creată cu succes.';
PRINT '✓ Coloane: SessionID, UtilizatorID, UserName, Email, AdresaIP, Dispozitiv, DataCreare, DataUltimaActivitate, DataExpirare, MinuteInactive, MinuteUntilExpiration';
GO

-- Test view
/*
SELECT TOP 10 * FROM VW_ActiveSessions ORDER BY DataUltimaActivitate DESC;
*/
GO
