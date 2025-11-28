-- ========================================
-- INVESTIGARE UTILIZATOR "pop"
-- ========================================
USE ValyanMed;
GO

PRINT '=== 1. VERIFICARE EXISTEN?? UTILIZATOR ===';
PRINT '';

-- Verific? dac? utilizatorul exist?
DECLARE @UserExists BIT = 0;
IF EXISTS (SELECT 1 FROM Utilizatori WHERE Username = 'pop')
    SET @UserExists = 1;

IF @UserExists = 1
BEGIN
    PRINT '? Utilizatorul "pop" EXIST?';
    PRINT '';
    
    -- Afi?eaz? TOATE detaliile
    PRINT '=== 2. DETALII UTILIZATOR ===';
    SELECT 
        UtilizatorID,
        PersonalMedicalID,
        Username,
        Email,
        Rol,
        EsteActiv,
        NumarIncercariEsuate,
        DataBlocare,
        DataUltimaAutentificare,
        PasswordHash,  -- HASH COMPLET
        Salt,
        TokenResetareParola,
        DataExpirareToken,
        DataCrearii,
        CreatDe,
        DataUltimeiModificari,
        ModificatDe
    FROM Utilizatori 
    WHERE Username = 'pop';
    
    PRINT '';
    PRINT '=== 3. ANALIZA HASH ===';
    
    DECLARE @Hash NVARCHAR(256);
    SELECT @Hash = PasswordHash FROM Utilizatori WHERE Username = 'pop';
    
    PRINT 'Hash Length: ' + CAST(LEN(@Hash) AS VARCHAR);
    PRINT 'Hash Prefix: ' + LEFT(@Hash, 10);
    
    -- Verific? tipul de hash
    IF LEFT(@Hash, 4) = '$2a$' OR LEFT(@Hash, 4) = '$2b$' OR LEFT(@Hash, 4) = '$2y$'
    BEGIN
        PRINT '? Hash Type: BCrypt (CORRECT)';
        PRINT 'Work Factor: ' + SUBSTRING(@Hash, 5, 2);
    END
    ELSE IF LEN(@Hash) = 64
    BEGIN
        PRINT '? Hash Type: SHA256 (INCORRECT - needs BCrypt)';
    END
    ELSE IF LEN(@Hash) = 32
    BEGIN
        PRINT '? Hash Type: MD5 (VERY INSECURE - needs BCrypt)';
    END
    ELSE IF @Hash LIKE 'HASH_%'
    BEGIN
        PRINT '? Hash Type: PLACEHOLDER (INVALID - needs real hash)';
    END
    ELSE
    BEGIN
        PRINT '? Hash Type: UNKNOWN (length: ' + CAST(LEN(@Hash) AS VARCHAR) + ')';
    END
    
    PRINT '';
    PRINT '=== 4. STATUS CONT ===';
    
    DECLARE @EsteActiv BIT, @DataBlocare DATETIME2, @NumarIncercari INT;
    SELECT 
        @EsteActiv = EsteActiv,
        @DataBlocare = DataBlocare,
        @NumarIncercari = NumarIncercariEsuate
    FROM Utilizatori 
    WHERE Username = 'pop';
    
    IF @EsteActiv = 1
        PRINT '? Cont ACTIV';
    ELSE
        PRINT '? Cont INACTIV';
        
    IF @DataBlocare IS NULL
        PRINT '? Cont DEBLOCAT';
    ELSE
        PRINT '? Cont BLOCAT din ' + CONVERT(VARCHAR, @DataBlocare, 120);
        
    PRINT 'Încerc?ri e?uate: ' + CAST(@NumarIncercari AS VARCHAR) + '/5';
    
    PRINT '';
    PRINT '=== 5. VERIFICARE PersonalMedical ===';
    
    DECLARE @PersonalMedicalID UNIQUEIDENTIFIER;
    SELECT @PersonalMedicalID = PersonalMedicalID FROM Utilizatori WHERE Username = 'pop';
    
    IF @PersonalMedicalID IS NOT NULL
    BEGIN
        IF EXISTS (SELECT 1 FROM PersonalMedical WHERE PersonalID = @PersonalMedicalID)
        BEGIN
            PRINT '? PersonalMedical asociat EXIST?';
            SELECT 
                PersonalID,
                Nume,
                Prenume,
                Nume + ' ' + Prenume AS NumeComplet,
                Specializare,
                Departament,
                Pozitie,
                Email,
                Telefon,
                EsteActiv
            FROM PersonalMedical 
            WHERE PersonalID = @PersonalMedicalID;
        END
        ELSE
        BEGIN
            PRINT '? PersonalMedical cu ID ' + CAST(@PersonalMedicalID AS VARCHAR(36)) + ' NU EXIST?!';
        END
    END
    ELSE
    BEGIN
        PRINT '? PersonalMedicalID este NULL';
    END
END
ELSE
BEGIN
    PRINT '? Utilizatorul "pop" NU EXIST? în baza de date!';
    PRINT '';
    PRINT 'C?utare utilizatori similari...';
    SELECT Username, Email, Rol, EsteActiv
    FROM Utilizatori
    WHERE Username LIKE '%pop%' OR Email LIKE '%pop%';
END

PRINT '';
PRINT '=== 6. ULTIMELE 5 MODIFIC?RI PE UTILIZATORI ===';
SELECT TOP 5
    Username,
    ModificatDe,
    DataUltimeiModificari,
    EsteActiv,
    NumarIncercariEsuate
FROM Utilizatori
ORDER BY DataUltimeiModificari DESC;

PRINT '';
PRINT '=== INVESTIGARE COMPLET? ===';
GO
