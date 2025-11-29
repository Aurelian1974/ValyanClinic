-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Stored Procedure pentru update activitate sesiune (heartbeat)
-- Dependen?e: Tabele UserSessions, Setari_Sistem
-- =============================================

PRINT '=== CREARE SP_UpdateSessionActivity ===';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_UpdateSessionActivity]') AND type in (N'P', N'PC'))
BEGIN
  DROP PROCEDURE [dbo].[SP_UpdateSessionActivity];
    PRINT '? Procedure existent? ?tears? (va fi recreat?).';
END
GO

CREATE PROCEDURE [dbo].[SP_UpdateSessionActivity]
    @SessionToken NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TimeoutMinutes INT;
    
    SELECT @TimeoutMinutes = CAST([Valoare] AS INT)
    FROM [dbo].[Setari_Sistem] 
    WHERE [Categorie] = 'Autentificare' AND [Cheie] = 'SessionTimeoutMinutes';
    
    UPDATE [dbo].[UserSessions]
    SET [DataUltimaActivitate] = GETDATE(),
        [DataExpirare] = DATEADD(MINUTE, @TimeoutMinutes, GETDATE())
    WHERE [SessionToken] = @SessionToken
      AND [EsteActiva] = 1
AND [DataExpirare] > GETDATE();
    
-- Return number of rows affected (1 = session updated, 0 = session expired/invalid)
    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

PRINT '? SP_UpdateSessionActivity creat? cu succes.';
PRINT '? Parametri: @SessionToken';
PRINT '? Return: RowsAffected (1 = success, 0 = session invalid/expired)';
GO
