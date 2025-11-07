-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Date inițiale setări autentificare (17 setări)
-- Dependențe: 01_CREATE_TABLE_Setari_Sistem.sql
-- =============================================

-- Verificare tabel există
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Setari_Sistem]') AND type in (N'U'))
BEGIN
    RAISERROR('EROARE: Tabelul Setari_Sistem nu există! Rulează mai întâi scriptul 01_CREATE_TABLE_Setari_Sistem.sql', 16, 1);
    RETURN;
END
GO

PRINT '=== INSERARE DATE INIȚIALE Setari_Sistem ===';

-- Ștergere date existente (doar pentru development - comentează în production!)
-- DELETE FROM [dbo].[Setari_Sistem] WHERE [Categorie] = 'Autentificare';

-- Inserare date inițiale
BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO [dbo].[Setari_Sistem] 
        ([Categorie], [Cheie], [Valoare], [TipDate], [Descriere], [ValoareDefault], [EsteEditabil])
    VALUES 
    -- ===== POLITICI PAROLE =====
    ('Autentificare', 'PasswordMinLength', '8', 'int', 
     'Lungime minimă parolă (caractere)', '8', 1),
  
    ('Autentificare', 'PasswordRequireDigit', 'true', 'bool', 
     'Parolă necesită cel puțin o cifră (0-9)', 'true', 1),
    
    ('Autentificare', 'PasswordRequireUppercase', 'true', 'bool', 
     'Parolă necesită cel puțin o literă mare (A-Z)', 'true', 1),
    
    ('Autentificare', 'PasswordRequireLowercase', 'true', 'bool', 
     'Parolă necesită cel puțin o literă mică (a-z)', 'true', 1),
    
    ('Autentificare', 'PasswordRequireNonAlphanumeric', 'true', 'bool', 
     'Parolă necesită cel puțin un caracter special (!@#$%)', 'true', 1),
    
    ('Autentificare', 'PasswordExpirationDays', '90', 'int', 
     'Zile până la expirarea parolei (0 = fără expirare)', '90', 1),

    -- ===== TIMEOUT SESIUNE =====
    ('Autentificare', 'SessionTimeoutMinutes', '30', 'int', 
     'Timeout sesiune după inactivitate (minute)', '30', 1),
    
    ('Autentificare', 'SessionAbsoluteTimeoutHours', '8', 'int', 
   'Timeout absolut sesiune indiferent de activitate (ore)', '8', 1),

    -- ===== LOCKOUT CONT =====
    ('Autentificare', 'LockoutMaxFailedAttempts', '5', 'int', 
     'Număr maxim încercări eșuate înainte de lockout', '5', 1),
    
    ('Autentificare', 'LockoutDurationMinutes', '15', 'int', 
     'Durata lockout cont după încercări eșuate (minute)', '15', 1),
    
    ('Autentificare', 'LockoutEnabled', 'true', 'bool', 
     'Activare funcționalitate lockout automat', 'true', 1),

    -- ===== PAROLĂ IMPLICITĂ =====
    ('Autentificare', 'DefaultPasswordForNewUsers', 'Valyan@2025', 'string', 
     'Parolă implicită pentru conturi noi (va fi forțată schimbarea)', 'Valyan@2025', 0),
    
    ('Autentificare', 'ForcePasswordChangeOnFirstLogin', 'true', 'bool', 
     'Forțează schimbare parolă la prima autentificare', 'true', 1),

    -- ===== ISTORIC PAROLE =====
    ('Autentificare', 'PasswordHistoryCount', '5', 'int', 
     'Număr parole stocate în istoric (nu pot fi refolosite)', '5', 1),
    
    ('Autentificare', 'PasswordHistoryEnabled', 'true', 'bool', 
     'Activare verificare istoric parole', 'true', 1),

    -- ===== AUDIT LOG =====
    ('Autentificare', 'AuditLogEnabled', 'true', 'bool', 
     'Activare audit log pentru acțiuni utilizatori', 'true', 0),
    
    ('Autentificare', 'AuditLogRetentionDays', '365', 'int', 
     'Zile păstrare înregistrări audit log', '365', 1);

    COMMIT TRANSACTION;

    PRINT '✓ 17 setări inițiale inserate cu succes în categoria "Autentificare".';
    
 -- Afișare setări inserate
    SELECT 
        [Cheie],
        [Valoare],
        [TipDate],
        [Descriere]
    FROM [dbo].[Setari_Sistem]
    WHERE [Categorie] = 'Autentificare'
    ORDER BY [Cheie];

END TRY
BEGIN CATCH
 IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT '✗ EROARE la inserarea datelor inițiale:';
    PRINT @ErrorMessage;
    
RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
GO

PRINT '=== FINALIZARE INSERARE DATE ===';
PRINT '✓ Scriptul s-a executat cu succes.';
GO
