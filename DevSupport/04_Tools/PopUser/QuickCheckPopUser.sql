-- Quick check pentru utilizatorul "pop"
USE ValyanMed;
GO

PRINT '=== VERIFICARE RAPID? UTILIZATOR "pop" ===';
PRINT '';

IF EXISTS (SELECT 1 FROM Utilizatori WHERE Username = 'pop')
BEGIN
    SELECT 
        'UTILIZATOR G?SIT' AS Status,
        Username,
        Email,
        Rol,
        EsteActiv,
        NumarIncercariEsuate,
        CASE 
            WHEN DataBlocare IS NOT NULL THEN 'BLOCAT din ' + CONVERT(VARCHAR, DataBlocare, 120)
            ELSE 'DEBLOCAT'
        END AS StatusBlocare,
        LEFT(PasswordHash, 10) AS HashPrefix,
        LEN(PasswordHash) AS HashLength,
        CASE 
            WHEN LEFT(PasswordHash, 4) IN ('$2a$', '$2b$', '$2y$') THEN 'BCrypt ?'
            WHEN PasswordHash LIKE 'HASH_%' THEN 'PLACEHOLDER ?'
            WHEN LEN(PasswordHash) = 64 THEN 'SHA256 ?'
            WHEN LEN(PasswordHash) = 32 THEN 'MD5 ?'
            ELSE 'UNKNOWN ?'
        END AS HashType,
        PasswordHash AS FullHash
    FROM Utilizatori 
    WHERE Username = 'pop';
END
ELSE
BEGIN
    PRINT '? UTILIZATORUL "pop" NU EXIST?!';
END

PRINT '';
PRINT '=== FINALIZAT ===';
GO
