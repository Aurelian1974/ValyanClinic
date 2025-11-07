-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Stored Procedure pentru cur??are sesiuni expirate (SQL Agent Job)
-- Dependen?e: Tabele UserSessions, Audit_Log
-- =============================================

PRINT '=== CREARE SP_CleanupExpiredSessions ===';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_CleanupExpiredSessions]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_CleanupExpiredSessions];
    PRINT '? Procedure existent? ?tears? (va fi recreat?).';
END
GO

CREATE PROCEDURE [dbo].[SP_CleanupExpiredSessions]
AS
BEGIN
    SET NOCOUNT ON;
    
  DELETE FROM [dbo].[UserSessions]
    WHERE [DataExpirare] < GETUTCDATE() OR [EsteActiva] = 0;
  
  -- Log cleanup
    DECLARE @DeletedCount INT = @@ROWCOUNT;
    
    IF @DeletedCount > 0
    BEGIN
        INSERT INTO [dbo].[Audit_Log] ([Actiune], [Entitate], [StatusActiune], [DetaliiEroare])
        VALUES ('SessionCleanup', 'UserSessions', 'Success', 
   'Deleted ' + CAST(@DeletedCount AS NVARCHAR) + ' expired sessions');
    END
    
    SELECT @DeletedCount AS DeletedSessions;
END;
GO

PRINT '? SP_CleanupExpiredSessions creat? cu succes.';
PRINT '? Parametri: Niciun';
PRINT '? Return: DeletedSessions count';
PRINT '! RECOMANDARE: Configureaz? SQL Server Agent Job s? ruleze la fiecare 15 minute.';
GO
