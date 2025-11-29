-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Extindere tabel Utilizatori + Corectare DEFAULT CONSTRAINTS (GETUTCDATE -> GETDATE)
-- Dependențe: Tabel Utilizatori (existent)
-- =============================================

PRINT '=== EXTINDERE TABEL Utilizatori ===';

-- Verificare tabel există
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND type in (N'U'))
BEGIN
    RAISERROR('EROARE: Tabelul Utilizatori nu există! Verifică schema database.', 16, 1);
    RETURN;
END
GO

BEGIN TRY
    BEGIN TRANSACTION;
  
    PRINT '=== PASUL 1: Adăugare coloane noi (dacă nu există) ===';
    
    -- AccessFailedCount
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND name = 'AccessFailedCount')
    BEGIN
        ALTER TABLE [dbo].[Utilizatori] ADD [AccessFailedCount] INT NOT NULL DEFAULT 0;
      PRINT '✓ Coloana AccessFailedCount adăugată.';
    END
    ELSE
        PRINT '! Coloana AccessFailedCount există deja.';
    
-- LockoutEndDateUtc
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND name = 'LockoutEndDateUtc')
BEGIN
        ALTER TABLE [dbo].[Utilizatori] ADD [LockoutEndDateUtc] DATETIME2(7) NULL;
 PRINT '✓ Coloana LockoutEndDateUtc adăugată.';
    END
    ELSE
      PRINT '! Coloana LockoutEndDateUtc există deja.';
    
    -- LockoutEnabled
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND name = 'LockoutEnabled')
    BEGIN
   ALTER TABLE [dbo].[Utilizatori] ADD [LockoutEnabled] BIT NOT NULL DEFAULT 1;
 PRINT '✓ Coloana LockoutEnabled adăugată.';
    END
    ELSE
        PRINT '! Coloana LockoutEnabled există deja.';
    
    -- LastPasswordChangedDate
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND name = 'LastPasswordChangedDate')
    BEGIN
        ALTER TABLE [dbo].[Utilizatori] ADD [LastPasswordChangedDate] DATETIME2(7) NULL;
        PRINT '✓ Coloana LastPasswordChangedDate adăugată.';
    END
  ELSE
        PRINT '! Coloana LastPasswordChangedDate există deja.';
    
 -- PasswordExpirationDate
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND name = 'PasswordExpirationDate')
    BEGIN
        ALTER TABLE [dbo].[Utilizatori] ADD [PasswordExpirationDate] DATETIME2(7) NULL;
  PRINT '✓ Coloana PasswordExpirationDate adăugată.';
  END
    ELSE
        PRINT '! Coloana PasswordExpirationDate există deja.';
 
    -- MustChangePasswordOnNextLogin
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND name = 'MustChangePasswordOnNextLogin')
    BEGIN
    ALTER TABLE [dbo].[Utilizatori] ADD [MustChangePasswordOnNextLogin] BIT NOT NULL DEFAULT 0;
        PRINT '✓ Coloana MustChangePasswordOnNextLogin adăugată.';
    END
    ELSE
        PRINT '! Coloana MustChangePasswordOnNextLogin există deja.';
    
    -- LastLoginDate
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND name = 'LastLoginDate')
    BEGIN
    ALTER TABLE [dbo].[Utilizatori] ADD [LastLoginDate] DATETIME2(7) NULL;
        PRINT '✓ Coloana LastLoginDate adăugată.';
    END
    ELSE
        PRINT '! Coloana LastLoginDate există deja.';
    
    -- LastLoginIP
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND name = 'LastLoginIP')
    BEGIN
      ALTER TABLE [dbo].[Utilizatori] ADD [LastLoginIP] NVARCHAR(50) NULL;
        PRINT '✓ Coloana LastLoginIP adăugată.';
    END
 ELSE
     PRINT '! Coloana LastLoginIP există deja.';
    
    PRINT '';
    PRINT '=== PASUL 2: Corectare DEFAULT CONSTRAINTS (GETUTCDATE -> GETDATE) ===';
    
    -- Declare variable pentru numele constraint-ului
    DECLARE @ConstraintName NVARCHAR(200);
    DECLARE @DropSQL NVARCHAR(MAX);
    
    -- Corectare DEFAULT pentru DataCrearii (dacă există)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND name = 'DataCrearii')
  BEGIN
        -- Găsește numele constraint-ului DEFAULT pentru DataCrearii
        SELECT @ConstraintName = dc.name
        FROM sys.default_constraints dc
 INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
        WHERE c.object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND c.name = 'DataCrearii';
        
     IF @ConstraintName IS NOT NULL
        BEGIN
 -- Drop constraint vechi
            SET @DropSQL = 'ALTER TABLE [dbo].[Utilizatori] DROP CONSTRAINT [' + @ConstraintName + ']';
            EXEC sp_executesql @DropSQL;
            PRINT '✓ Constraint vechi pentru DataCrearii șters: ' + @ConstraintName;
      
            -- Add constraint nou cu GETDATE()
            ALTER TABLE [dbo].[Utilizatori] ADD CONSTRAINT [DF_Utilizatori_DataCrearii] DEFAULT GETDATE() FOR [DataCrearii];
   PRINT '✓ Constraint nou pentru DataCrearii adăugat cu GETDATE().';
        END
        ELSE
        BEGIN
            -- Adaugă direct constraint dacă nu există
            ALTER TABLE [dbo].[Utilizatori] ADD CONSTRAINT [DF_Utilizatori_DataCrearii] DEFAULT GETDATE() FOR [DataCrearii];
    PRINT '✓ Constraint DataCrearii adăugat cu GETDATE().';
 END
END
    
    -- Corectare DEFAULT pentru DataUltimeiModificari (dacă există)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND name = 'DataUltimeiModificari')
 BEGIN
SET @ConstraintName = NULL;
     
    SELECT @ConstraintName = dc.name
     FROM sys.default_constraints dc
        INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
        WHERE c.object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND c.name = 'DataUltimeiModificari';
        
 IF @ConstraintName IS NOT NULL
        BEGIN
            SET @DropSQL = 'ALTER TABLE [dbo].[Utilizatori] DROP CONSTRAINT [' + @ConstraintName + ']';
     EXEC sp_executesql @DropSQL;
          PRINT '✓ Constraint vechi pentru DataUltimeiModificari șters: ' + @ConstraintName;
          
   ALTER TABLE [dbo].[Utilizatori] ADD CONSTRAINT [DF_Utilizatori_DataUltimeiModificari] DEFAULT GETDATE() FOR [DataUltimeiModificari];
  PRINT '✓ Constraint nou pentru DataUltimeiModificari adăugat cu GETDATE().';
        END
        ELSE
        BEGIN
       ALTER TABLE [dbo].[Utilizatori] ADD CONSTRAINT [DF_Utilizatori_DataUltimeiModificari] DEFAULT GETDATE() FOR [DataUltimeiModificari];
    PRINT '✓ Constraint DataUltimeiModificari adăugat cu GETDATE().';
 END
    END
    
    PRINT '';
    PRINT '=== PASUL 3: Creare INDEX pentru utilizatori lockuiți ===';
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Utilizatori]') AND name = N'IX_Utilizatori_LockoutEndDate')
  BEGIN
   CREATE NONCLUSTERED INDEX [IX_Utilizatori_LockoutEndDate] 
        ON [dbo].[Utilizatori] ([LockoutEndDateUtc] ASC) 
  WHERE [LockoutEndDateUtc] IS NOT NULL;
        PRINT '✓ Index IX_Utilizatori_LockoutEndDate creat.';
    END
    ELSE
 BEGIN
PRINT '! Index IX_Utilizatori_LockoutEndDate există deja.';
    END
    
    COMMIT TRANSACTION;
    
    PRINT '';
    PRINT '=== REZUMAT EXTINDERE TABEL ===';
    PRINT '✓ Tabel Utilizatori extins cu succes.';
    PRINT '✓ Toate coloanele necesare sunt disponibile.';
    PRINT '✓ DEFAULT CONSTRAINTS corectate: GETUTCDATE() -> GETDATE()';
    PRINT '✓ Index pentru performanță creat/verificat.';
    
    -- Afișare structură coloane modificate
    SELECT 
        c.COLUMN_NAME AS [Coloană],
        c.DATA_TYPE AS [Tip Date],
        c.IS_NULLABLE AS [Nullable],
      c.COLUMN_DEFAULT AS [Valoare Default]
    FROM INFORMATION_SCHEMA.COLUMNS c
    WHERE c.TABLE_NAME = 'Utilizatori'
      AND (
   c.COLUMN_NAME IN ('AccessFailedCount', 'LockoutEndDateUtc', 'LockoutEnabled', 
          'LastPasswordChangedDate', 'PasswordExpirationDate', 
   'MustChangePasswordOnNextLogin', 'LastLoginDate', 'LastLoginIP')
          OR c.COLUMN_NAME IN ('DataCrearii', 'DataUltimeiModificari')
   )
    ORDER BY c.ORDINAL_POSITION;

END TRY
BEGIN CATCH
  IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    
 DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT '';
    PRINT '✗ EROARE la extinderea tabelului Utilizatori:';
    PRINT @ErrorMessage;

    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
GO
