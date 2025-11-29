-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Stored Procedure pentru creare sesiune utilizator
-- Dependen?e: Tabele UserSessions, Setari_Sistem, Audit_Log
-- =============================================

PRINT '=== CREARE SP_CreateUserSession ===';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_CreateUserSession]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_CreateUserSession];
    PRINT '? Procedure existent? ?tears? (va fi recreat?).';
END
GO

CREATE PROCEDURE [dbo].[SP_CreateUserSession]
  @UtilizatorID UNIQUEIDENTIFIER,
  @AdresaIP NVARCHAR(50),
    @UserAgent NVARCHAR(500),
    @Dispozitiv NVARCHAR(200),
    @SessionToken NVARCHAR(500) OUTPUT,
    @SessionID UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
 
        DECLARE @TimeoutMinutes INT;
        
        SELECT @TimeoutMinutes = CAST([Valoare] AS INT)
   FROM [dbo].[Setari_Sistem] 
        WHERE [Categorie] = 'Autentificare' AND [Cheie] = 'SessionTimeoutMinutes';
        
        SET @SessionID = NEWID();
        SET @SessionToken = CONVERT(NVARCHAR(500), NEWID()) + '_' + CONVERT(NVARCHAR(500), NEWID());
   
        INSERT INTO [dbo].[UserSessions] (
            [SessionID], [UtilizatorID], [SessionToken], [AdresaIP], 
          [UserAgent], [Dispozitiv], [DataExpirare]
        )
        VALUES (
            @SessionID, @UtilizatorID, @SessionToken, @AdresaIP,
            @UserAgent, @Dispozitiv, DATEADD(MINUTE, @TimeoutMinutes, GETDATE())
        );
 
        -- Audit
        INSERT INTO [dbo].[Audit_Log] ([UtilizatorID], [Actiune], [AdresaIP], [StatusActiune])
        VALUES (@UtilizatorID, 'SessionCreated', @AdresaIP, 'Success');
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

PRINT '? SP_CreateUserSession creat? cu succes.';
PRINT '? Parametri: @UtilizatorID, @AdresaIP, @UserAgent, @Dispozitiv, @SessionToken (OUT), @SessionID (OUT)';
GO
