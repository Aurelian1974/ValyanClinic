-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Trigger pentru auto-populate PasswordHistory + cleanup
-- Dependințe: Tabele Utilizatori, PasswordHistory, Setari_Sistem
-- =============================================

PRINT '=== CREARE TRIGGER TR_Utilizatori_PasswordChange ===';

-- Verificare dependențe
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PasswordHistory]') AND type in (N'U'))
BEGIN
    RAISERROR('EROARE: Tabelul PasswordHistory nu există!', 16, 1);
    RETURN;
END
GO

-- Ștergere trigger existent (dacă există)
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_Utilizatori_PasswordChange]'))
BEGIN
    DROP TRIGGER [dbo].[TR_Utilizatori_PasswordChange];
    PRINT '✓ Trigger existent șters (va fi recreat).';
END
GO

-- Creare trigger
CREATE TRIGGER [dbo].[TR_Utilizatori_PasswordChange]
ON [dbo].[Utilizatori]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Adaugă în istoric doar dacă parola s-a schimbat
    INSERT INTO [dbo].[PasswordHistory] ([UtilizatorID], [PasswordHash], [DataCrearii])
    SELECT 
        i.[UtilizatorID],
        i.[PasswordHash],
        GETDATE()
    FROM inserted i
    INNER JOIN deleted d ON i.[UtilizatorID] = d.[UtilizatorID]
    WHERE i.[PasswordHash] <> d.[PasswordHash];
    
    -- Curățare istoric vechi (păstrează doar ultimele N parole)
    DECLARE @PasswordHistoryCount INT;
    
    SELECT @PasswordHistoryCount = CAST([Valoare] AS INT) 
    FROM [dbo].[Setari_Sistem] 
    WHERE [Categorie] = 'Autentificare' AND [Cheie] = 'PasswordHistoryCount';
    
    IF @PasswordHistoryCount > 0
    BEGIN
   ;WITH CTE_OldPasswords AS (
   SELECT 
            [PasswordHistoryID],
       ROW_NUMBER() OVER (PARTITION BY [UtilizatorID] ORDER BY [DataCrearii] DESC) AS RowNum
       FROM [dbo].[PasswordHistory]
 )
 DELETE FROM CTE_OldPasswords WHERE RowNum > @PasswordHistoryCount;
    END
END;
GO

PRINT '✓ Trigger TR_Utilizatori_PasswordChange creat cu succes.';
PRINT '✓ Trigger activat pe: UPDATE Utilizatori';
PRINT '✓ Funcționalități: Auto-insert în PasswordHistory + Cleanup istoric vechi';
GO

-- Test trigger (opțional - comentează în production)
-- PRINT '=== TEST TRIGGER ===';
-- Creează un utilizator test și modifică parola pentru a verifica trigger-ul
GO
