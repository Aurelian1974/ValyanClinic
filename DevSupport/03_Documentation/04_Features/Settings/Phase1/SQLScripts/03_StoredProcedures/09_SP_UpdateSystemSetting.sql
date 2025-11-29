-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.1 (✅ ADĂUGAT suport pentru Descriere)
-- Descriere: Stored Procedure pentru actualizare setare sistem (cu audit)
-- Dependențe: Tabele Setari_Sistem, Audit_Log
-- =============================================

PRINT '=== CREARE SP_UpdateSystemSetting ===';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_UpdateSystemSetting]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_UpdateSystemSetting];
    PRINT '✓ Procedure existentă ștearsă (va fi recreată).';
END
GO

CREATE PROCEDURE [dbo].[SP_UpdateSystemSetting]
    @Categorie NVARCHAR(100),
    @Cheie NVARCHAR(200),
    @Valoare NVARCHAR(MAX),
    @Descriere NVARCHAR(500) = NULL, -- ✅ ADĂUGAT (optional)
    @ModificatDe NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
 BEGIN TRANSACTION;
     
        DECLARE @ValoareVeche NVARCHAR(MAX);
  DECLARE @DescriereVeche NVARCHAR(500); -- ✅ ADĂUGAT
        DECLARE @EsteEditabil BIT;
        
     -- Verifică dacă setarea este editabilă
        SELECT @ValoareVeche = [Valoare], 
               @DescriereVeche = [Descriere], -- ✅ ADĂUGAT
         @EsteEditabil = [EsteEditabil]
        FROM [dbo].[Setari_Sistem]
      WHERE [Categorie] = @Categorie AND [Cheie] = @Cheie;
    
        IF @EsteEditabil = 0
        BEGIN
            RAISERROR('Setarea nu poate fi modificată (EsteEditabil = 0)', 16, 1);
    RETURN;
  END
        
        -- Update setare (cu Descriere dacă este furnizată)
     UPDATE [dbo].[Setari_Sistem]
        SET [Valoare] = @Valoare,
   [Descriere] = ISNULL(@Descriere, [Descriere]), -- ✅ UPDATE doar dacă e furnizată
       [DataModificarii] = GETDATE(),
      [ModificatDe] = @ModificatDe
 WHERE [Categorie] = @Categorie AND [Cheie] = @Cheie;
   
        -- Audit log (✅ include și schimbarea descripției)
INSERT INTO [dbo].[Audit_Log] (
          [Actiune], 
        [Entitate], 
            [EntitateID], 
            [ValoareVeche], 
            [ValoareNoua],
 [StatusActiune]
        )
        VALUES (
     'UpdateSetting', 
       'Setari_Sistem', 
            @Categorie + '.' + @Cheie, 
  @ValoareVeche + ' | ' + ISNULL(@DescriereVeche, ''), -- ✅ Include descriere veche
            @Valoare + ' | ' + ISNULL(@Descriere, ISNULL(@DescriereVeche, '')), -- ✅ Include descriere nouă
     'Success'
        );
   
        COMMIT TRANSACTION;
     
        -- Return updated setting
    SELECT 
    [Categorie],
            [Cheie],
            [Valoare],
            [Descriere],
            [TipDate],
      [ValoareDefault],
            [EsteEditabil],
     [DataCrearii],
          [DataModificarii],
       [ModificatDe]
        FROM [dbo].[Setari_Sistem]
   WHERE [Categorie] = @Categorie AND [Cheie] = @Cheie;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
        INSERT INTO [dbo].[Audit_Log] ([Actiune], [Entitate], [StatusActiune], [DetaliiEroare])
        VALUES ('UpdateSetting', 'Setari_Sistem', 'Failed', ERROR_MESSAGE());
        
   THROW;
 END CATCH
END;
GO

PRINT '✓ SP_UpdateSystemSetting creată cu succes.';
PRINT '✓ Parametri: @Categorie, @Cheie, @Valoare, @Descriere (optional), @ModificatDe';
PRINT '✓ Features: Validare EsteEditabil + Audit Log + UPDATE Descriere';
GO
