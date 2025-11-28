-- Script SQL pentru verificare utilizator "pop"
USE ValyanMed;
GO

PRINT '=== VERIFICARE UTILIZATOR "pop" ===';
PRINT '';

-- 1. Verific? dac? utilizatorul exist?
IF EXISTS (SELECT 1 FROM Utilizatori WHERE Username = 'pop')
BEGIN
    PRINT '? Utilizatorul "pop" EXIST? în baza de date';
    PRINT '';
    
    -- Afi?eaz? detalii utilizator
    SELECT 
        UtilizatorID,
        Username,
        Email,
        Rol,
        EsteActiv,
        NumarIncercariEsuate,
        DataBlocare,
        LEFT(PasswordHash, 60) AS PasswordHashPreview,
        DataUltimeiModificari,
        ModificatDe
    FROM Utilizatori 
    WHERE Username = 'pop';
    
    PRINT '';
    PRINT '=== AC?IUNI RECOMANDATE ===';
    PRINT '';
    PRINT '1. Verific? c? PasswordHash este BCrypt (începe cu $2a$, $2b$ sau $2y$)';
    PRINT '2. Dac? nu, regenereaz? hash-ul cu:';
    PRINT '   cd DevSupport\Scripts\PowerShellScripts';
    PRINT '   .\Create-User.ps1 -Username "pop" -Password "Pop123!@#" -UpdateExisting';
    PRINT '';
END
ELSE
BEGIN
    PRINT '? Utilizatorul "pop" NU EXIST? în baza de date!';
    PRINT '';
    PRINT 'CREEAZ? utilizatorul cu:';
    PRINT '  cd DevSupport\Scripts\PowerShellScripts';
    PRINT '  .\Create-User.ps1 -Username "pop" -Password "Pop123!@#" -Email "pop@clinic.ro" -Rol "Doctor"';
    PRINT '';
END

PRINT '';
PRINT '=== FINALIZAT ===';
GO
