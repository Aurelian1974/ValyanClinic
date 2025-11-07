-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Tabel istoric parole utilizatori
-- Dependențe: Tabel Utilizatori
-- =============================================

PRINT '=== CREARE TABEL PasswordHistory ===';

-- Verificare dependențe
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND type in (N'U'))
BEGIN
    RAISERROR('EROARE: Tabelul Utilizatori nu există! Verifică schema database.', 16, 1);
    RETURN;
END
GO

-- Creare tabel
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PasswordHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PasswordHistory] (
        [PasswordHistoryID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [UtilizatorID] UNIQUEIDENTIFIER NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
  [DataCrearii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
  
        CONSTRAINT [PK_PasswordHistory] PRIMARY KEY CLUSTERED ([PasswordHistoryID] ASC),
     CONSTRAINT [FK_PasswordHistory_Utilizator] 
          FOREIGN KEY ([UtilizatorID]) REFERENCES [dbo].[Utilizatori]([UtilizatorID]) ON DELETE CASCADE
    );
    
    -- Index pentru performanță
    CREATE NONCLUSTERED INDEX [IX_PasswordHistory_UtilizatorID_DataCrearii] 
    ON [dbo].[PasswordHistory] ([UtilizatorID] ASC, [DataCrearii] DESC);
    
    PRINT '✓ Tabel PasswordHistory creat cu succes.';
    PRINT '✓ Primary Key: PK_PasswordHistory';
    PRINT '✓ Foreign Key: FK_PasswordHistory_Utilizator (CASCADE DELETE)';
    PRINT '✓ Index: IX_PasswordHistory_UtilizatorID_DataCrearii';
END
ELSE
BEGIN
    PRINT '! Tabelul PasswordHistory există deja. Skip creation.';
END
GO

-- Verificare finală
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PasswordHistory]') AND type in (N'U'))
BEGIN
    PRINT '=== VERIFICARE FINALĂ ===';
    PRINT '✓ Tabelul PasswordHistory este gata de utilizare.';
    PRINT '✓ Trigger pentru auto-populate va fi creat în scriptul următor.';
END
GO
