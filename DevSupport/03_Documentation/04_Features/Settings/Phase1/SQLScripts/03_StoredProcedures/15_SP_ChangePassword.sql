-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Stored Procedure pentru schimbare parol? (cu valid?ri)
-- Dependen?e: Tabele Utilizatori, Setari_Sistem, Audit_Log, Function FN_IsPasswordInHistory
-- =============================================

PRINT '=== CREARE SP_ChangePassword ===';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_ChangePassword]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_ChangePassword];
    PRINT '? Procedure existent? ?tears? (va fi recreat?).';
END
GO

CREATE PROCEDURE [dbo].[SP_ChangePassword]
    @UtilizatorID UNIQUEIDENTIFIER,
    @NewPasswordHash NVARCHAR(MAX),
    @ModificatDe NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verific? dac? parola este �n istoric
    DECLARE @IsInHistory BIT;
      DECLARE @PasswordHistoryEnabled BIT;
        
     SELECT @PasswordHistoryEnabled = CAST([Valoare] AS BIT)
        FROM [dbo].[Setari_Sistem] 
     WHERE [Categorie] = 'Autentificare' AND [Cheie] = 'PasswordHistoryEnabled';
      
        IF @PasswordHistoryEnabled = 1
  BEGIN
   SET @IsInHistory = dbo.FN_IsPasswordInHistory(@UtilizatorID, @NewPasswordHash);
      
            IF @IsInHistory = 1
 BEGIN
     RAISERROR('Parola a fost folosit? recent. Alege?i o parol? diferit?.', 16, 1);
        RETURN;
      END
        END
     
     -- Calculeaz? data expirare parol?
  DECLARE @PasswordExpirationDays INT;
        DECLARE @NewExpirationDate DATETIME2 = NULL;
      
 SELECT @PasswordExpirationDays = CAST([Valoare] AS INT)
    FROM [dbo].[Setari_Sistem] 
    WHERE [Categorie] = 'Autentificare' AND [Cheie] = 'PasswordExpirationDays';
        
        IF @PasswordExpirationDays > 0
        BEGIN
  SET @NewExpirationDate = DATEADD(DAY, @PasswordExpirationDays, GETDATE());
        END
  
        -- Update parol? (trigger va popula PasswordHistory automat)
        UPDATE [dbo].[Utilizatori]
     SET [PasswordHash] = @NewPasswordHash,
 [LastPasswordChangedDate] = GETDATE(),
  [PasswordExpirationDate] = @NewExpirationDate,
    [MustChangePasswordOnNextLogin] = 0
   WHERE [UtilizatorID] = @UtilizatorID;
   
   -- Audit
        INSERT INTO [dbo].[Audit_Log] ([UtilizatorID], [Actiune], [Entitate], [StatusActiune])
        VALUES (@UtilizatorID, 'PasswordChanged', 'Utilizatori', 'Success');
        
     COMMIT TRANSACTION;
        
SELECT 1 AS Success, 'Parol? schimbat? cu succes.' AS Message;
  END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
      
  INSERT INTO [dbo].[Audit_Log] ([UtilizatorID], [Actiune], [StatusActiune], [DetaliiEroare])
        VALUES (@UtilizatorID, 'PasswordChanged', 'Failed', ERROR_MESSAGE());
  
   THROW;
    END CATCH
END;
GO

PRINT '? SP_ChangePassword creat? cu succes.';
PRINT '? Parametri: @UtilizatorID, @NewPasswordHash, @ModificatDe';
PRINT '? Features: Validare Istoric + Auto-expirare + Trigger auto-populate PasswordHistory';
GO
