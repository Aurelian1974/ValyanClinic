-- =============================================
-- FIX: Update Admin Password to match UI
-- Database: ValyanMed
-- Issue: Password hash mismatch
-- =============================================

USE [ValyanMed]
GO

PRINT '============================================'
PRINT 'FIX ADMIN PASSWORD HASH'
PRINT '============================================'
PRINT ''

-- =============================================
-- IMPORTANT: Acest hash este generat folosind BCrypt
-- pentru parola: admin123!@#
-- =============================================

DECLARE @Username NVARCHAR(100) = 'Admin'
DECLARE @NewPasswordHash NVARCHAR(512)
DECLARE @NewSalt NVARCHAR(512)

-- BCrypt hash pentru parola "admin123!@#"
-- Generat cu BCrypt.Net.BCrypt.HashPassword("admin123!@#", workFactor: 12)
-- IMPORTANT: Ruleaz? în C# pentru a genera hash-ul corect:
-- var hash = BCrypt.Net.BCrypt.HashPassword("admin123!@#", 12);

-- Exemplu hash (TREBUIE ÎNLOCUIT cu hash-ul generat din C#):
SET @NewPasswordHash = '$2a$12$YourActualHashHere'  -- PLACEHOLDER
SET @NewSalt = ''  -- BCrypt nu folose?te salt separat

PRINT 'Verificare utilizator Admin...'

IF EXISTS (SELECT 1 FROM Utilizatori WHERE Username = @Username)
BEGIN
    PRINT '? Utilizator g?sit: ' + @Username
    PRINT ''
    PRINT 'ATEN?IE: Nu poti rula acest script direct!'
    PRINT ''
    PRINT 'Pa?i pentru a fixa parola:'
    PRINT '1. Deschide C# Interactive Window în Visual Studio'
    PRINT '2. Adaug? referin?a: #r "BCrypt.Net-Next"'
    PRINT '3. Folose?te: using BCrypt.Net;'
  PRINT '4. Genereaz? hash: var hash = BCrypt.HashPassword("admin123!@#", 12);'
    PRINT '5. Copiaz? hash-ul generat'
    PRINT '6. Înlocuie?te @NewPasswordHash din acest script'
    PRINT '7. Ruleaz? UPDATE-ul de mai jos'
    PRINT ''
    
    -- UNCOMMENT dup? ce ai hash-ul corect din C#:
    /*
    UPDATE Utilizatori
    SET 
        PasswordHash = @NewPasswordHash,
      Salt = @NewSalt,
   Data_Ultimei_Modificari = GETDATE(),
        Modificat_De = 'System_PasswordFix'
    WHERE Username = @Username;
    
 IF @@ROWCOUNT > 0
    BEGIN
        PRINT '? Parola actualizat? cu succes!'
        PRINT ''
        PRINT 'Verificare final?:'
        
   SELECT 
          UtilizatorID,
            Username,
            Email,
            Rol,
    EsteActiv,
         LEFT(PasswordHash, 20) + '...' AS PasswordHashPreview,
            Data_Ultimei_Modificari
        FROM Utilizatori
        WHERE Username = @Username;
      
    PRINT ''
        PRINT 'Test login:'
        PRINT '  Username: Admin'
        PRINT '  Password: admin123!@#'
    END
    ELSE
 BEGIN
        PRINT '? Eroare la actualizarea parolei!'
    END
    */
    
    PRINT ''
    PRINT 'Status: SCRIPT MANUAL - Vezi instruc?iunile de mai sus'
END
ELSE
BEGIN
    PRINT '? EROARE: Utilizatorul Admin nu exist?!'
  PRINT ''
    PRINT 'Verific? dac? tabela Utilizatori exist? ?i con?ine date:'

    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Utilizatori')
    BEGIN
     PRINT '? Tabela Utilizatori exist?'
        
        DECLARE @UserCount INT
    SELECT @UserCount = COUNT(*) FROM Utilizatori
        
        PRINT '? Total utilizatori: ' + CAST(@UserCount AS NVARCHAR(10))
   
        IF @UserCount > 0
    BEGIN
       PRINT ''
      PRINT 'Utilizatori existen?i:'
     SELECT Username, Email, Rol, EsteActiv FROM Utilizatori
   END
    END
    ELSE
    BEGIN
        PRINT '? Tabela Utilizatori NU exist?!'
        PRINT '   Ruleaz? mai întâi scriptul de creare tabele'
    END
END

GO

PRINT ''
PRINT '============================================'
PRINT 'ALTERNATIV?: Folose?te C# Code pentru fix'
PRINT '============================================'
PRINT ''
PRINT 'Cod C# pentru a genera hash ?i a actualiza direct:'
PRINT ''
PRINT '// În C# (console app sau unit test):'
PRINT 'using BCrypt.Net;'
PRINT 'using Microsoft.Data.SqlClient;'
PRINT ''
PRINT 'var password = "admin123!@#";'
PRINT 'var hash = BCrypt.HashPassword(password, 12);'
PRINT ''
PRINT 'using (var conn = new SqlConnection("your-connection-string"))'
PRINT '{'
PRINT '  conn.Open();'
PRINT '    var cmd = new SqlCommand('
PRINT '        "UPDATE Utilizatori SET PasswordHash = @Hash WHERE Username = ''Admin''", '
PRINT '        conn);'
PRINT '    cmd.Parameters.AddWithValue("@Hash", hash);'
PRINT '    cmd.ExecuteNonQuery();'
PRINT '}'
PRINT ''
PRINT '============================================'

