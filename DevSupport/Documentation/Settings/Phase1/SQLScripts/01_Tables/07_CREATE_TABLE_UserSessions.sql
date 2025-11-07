-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Tabel sesiuni utilizatori (tracking sesiuni active)
-- Dependen?e: Tabel Utilizatori
-- =============================================

PRINT '=== CREARE TABEL UserSessions ===';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND type in (N'U'))
BEGIN
    RAISERROR('EROARE: Tabelul Utilizatori nu exist?!', 16, 1);
  RETURN;
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserSessions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserSessions] (
        [SessionID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [UtilizatorID] UNIQUEIDENTIFIER NOT NULL,
   [SessionToken] NVARCHAR(500) NOT NULL,
[AdresaIP] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
  [Dispozitiv] NVARCHAR(200) NULL,
   [DataCreare] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
     [DataUltimaActivitate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        [DataExpirare] DATETIME2(7) NOT NULL,
        [EsteActiva] BIT NOT NULL DEFAULT 1,
     
   CONSTRAINT [PK_UserSessions] PRIMARY KEY CLUSTERED ([SessionID] ASC),
        CONSTRAINT [UQ_UserSessions_SessionToken] UNIQUE NONCLUSTERED ([SessionToken]),
      CONSTRAINT [FK_UserSessions_Utilizator] 
   FOREIGN KEY ([UtilizatorID]) REFERENCES [dbo].[Utilizatori]([UtilizatorID]) ON DELETE CASCADE
    );
    
    -- Indexuri pentru performan??
    CREATE NONCLUSTERED INDEX [IX_UserSessions_UtilizatorID] 
    ON [dbo].[UserSessions] ([UtilizatorID] ASC);
    
    CREATE NONCLUSTERED INDEX [IX_UserSessions_DataExpirare] 
    ON [dbo].[UserSessions] ([DataExpirare] ASC);
    
    PRINT '? Tabel UserSessions creat cu succes.';
    PRINT '? Primary Key: PK_UserSessions';
    PRINT '? Unique Constraint: UQ_UserSessions_SessionToken';
    PRINT '? Foreign Key: FK_UserSessions_Utilizator (CASCADE DELETE)';
    PRINT '? 2 indexuri create pentru performan??.';
END
ELSE
BEGIN
    PRINT '! Tabelul UserSessions exist? deja. Skip creation.';
END
GO

PRINT '=== VERIFICARE FINAL? ===';
PRINT '? Tabelul UserSessions este gata de utilizare.';
PRINT '? SP_CleanupExpiredSessions va fi creat în scripturile urm?toare.';
GO
