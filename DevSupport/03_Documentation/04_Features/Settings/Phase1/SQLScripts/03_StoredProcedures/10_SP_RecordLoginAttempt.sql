-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Stored Procedure pentru �nregistrare tentativ? login (cu lockout logic)
-- Dependen?e: Tabele Utilizatori, Audit_Log, Setari_Sistem
-- =============================================

PRINT '=== CREARE SP_RecordLoginAttempt ===';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_RecordLoginAttempt]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_RecordLoginAttempt];
    PRINT '? Procedure existent? ?tears? (va fi recreat?).';
END
GO

CREATE PROCEDURE [dbo].[SP_RecordLoginAttempt]
    @UserName NVARCHAR(256),
    @AdresaIP NVARCHAR(50),
    @UserAgent NVARCHAR(500),
    @Success BIT,
    @UtilizatorID UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
      BEGIN TRANSACTION;
    
   DECLARE @StatusActiune NVARCHAR(50) = CASE WHEN @Success = 1 THEN 'Success' ELSE 'Failed' END;
        DECLARE @Actiune NVARCHAR(200) = CASE WHEN @Success = 1 THEN 'Login' ELSE 'LoginFailed' END;
     
        -- �nregistreaz? �n audit log
   INSERT INTO [dbo].[Audit_Log] ([UtilizatorID], [UserName], [Actiune], [AdresaIP], [UserAgent], [StatusActiune])
  VALUES (@UtilizatorID, @UserName, @Actiune, @AdresaIP, @UserAgent, @StatusActiune);
        
        IF @Success = 1
 BEGIN
            -- Update last login info + reset failed attempts
     UPDATE [dbo].[Utilizatori]
  SET [LastLoginDate] = GETDATE(),
       [LastLoginIP] = @AdresaIP,
   [AccessFailedCount] = 0
      WHERE [UtilizatorID] = @UtilizatorID;
        END
        ELSE
   BEGIN
   -- Increment failed attempts + lockout dac? e cazul
            DECLARE @MaxFailedAttempts INT;
    DECLARE @LockoutDurationMinutes INT;
        DECLARE @LockoutEnabled BIT;
            
   SELECT @LockoutEnabled = CAST([Valoare] AS BIT)
 FROM [dbo].[Setari_Sistem] WHERE [Categorie] = 'Autentificare' AND [Cheie] = 'LockoutEnabled';
    
         IF @LockoutEnabled = 1 AND @UtilizatorID IS NOT NULL
       BEGIN
     SELECT @MaxFailedAttempts = CAST([Valoare] AS INT)
                FROM [dbo].[Setari_Sistem] WHERE [Categorie] = 'Autentificare' AND [Cheie] = 'LockoutMaxFailedAttempts';
       
                SELECT @LockoutDurationMinutes = CAST([Valoare] AS INT)
    FROM [dbo].[Setari_Sistem] WHERE [Categorie] = 'Autentificare' AND [Cheie] = 'LockoutDurationMinutes';
            
  -- Update failed count ?i lockout
  UPDATE [dbo].[Utilizatori]
           SET [AccessFailedCount] = [AccessFailedCount] + 1,
   [LockoutEndDateUtc] = CASE 
      WHEN [AccessFailedCount] + 1 >= @MaxFailedAttempts 
 THEN DATEADD(MINUTE, @LockoutDurationMinutes, GETDATE())
        ELSE [LockoutEndDateUtc]
            END
  WHERE [UtilizatorID] = @UtilizatorID;
            END
END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
 IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
  END CATCH
END;
GO

PRINT '? SP_RecordLoginAttempt creat? cu succes.';
PRINT '? Parametri: @UserName, @AdresaIP, @UserAgent, @Success, @UtilizatorID (OUTPUT)';
PRINT '? Features: Audit Log + Lockout Logic + Reset Failed Attempts';
GO
