-- =============================================
-- Script: Verificare Complet? - Func?ionalitate Reactivare
-- Database: ValyanMed
-- Descriere: Verific? toate componentele necesare pentru reactivarea rela?iilor doctor-pacient
-- =============================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'VERIFICARE COMPLET?';
PRINT 'Func?ionalitate: Reactivare Rela?ie Doctor-Pacient';
PRINT 'Database: ValyanMed';
PRINT 'Data: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

DECLARE @Verificari TABLE (
  Test VARCHAR(100), 
    Status VARCHAR(10),
    Detalii NVARCHAR(255)
);

-- ============================================================================
-- STEP 1: Verificare Tabel Pacienti_PersonalMedical
-- ============================================================================
PRINT 'STEP 1: Verificare tabel Pacienti_PersonalMedical...';
PRINT '';

IF OBJECT_ID('dbo.Pacienti_PersonalMedical', 'U') IS NOT NULL
BEGIN
    INSERT INTO @Verificari VALUES ('Tabel Pacienti_PersonalMedical', '? OK', 'Exist?');

    -- Verificare coloane necesare
    DECLARE @ColoaneNecesare TABLE (Coloana VARCHAR(50));
    INSERT INTO @ColoaneNecesare VALUES 
        ('Id'), ('PacientID'), ('PersonalMedicalID'), 
        ('EsteActiv'), ('DataDezactivarii'), ('Observatii'), 
        ('Motiv'), ('Data_Ultimei_Modificari'), ('Modificat_De');
    
  DECLARE @ColoaneGasite INT;
    SELECT @ColoaneGasite = COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS c
    INNER JOIN @ColoaneNecesare n ON c.COLUMN_NAME = n.Coloana
    WHERE c.TABLE_NAME = 'Pacienti_PersonalMedical';
    
    IF @ColoaneGasite = 9
   INSERT INTO @Verificari VALUES ('Coloane necesare', '? OK', 'Toate coloanele exist? (9/9)');
    ELSE
        INSERT INTO @Verificari VALUES ('Coloane necesare', '? EROARE', 'Lipsesc coloane (' + CAST(@ColoaneGasite AS VARCHAR) + '/9)');
END
ELSE
BEGIN
    INSERT INTO @Verificari VALUES ('Tabel Pacienti_PersonalMedical', '? EROARE', 'NU exist?');
  INSERT INTO @Verificari VALUES ('Coloane necesare', '? SKIP', 'Tabelul lipse?te');
END

-- ============================================================================
-- STEP 2: Verificare Stored Procedure
-- ============================================================================
PRINT 'STEP 2: Verificare stored procedure...';
PRINT '';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_ActivateRelatie')
BEGIN
    INSERT INTO @Verificari VALUES ('SP ActivateRelatie', '? OK', 'Exist?');
    
    -- Verificare parametri SP
    DECLARE @NumarParametri INT;
    SELECT @NumarParametri = COUNT(*)
    FROM sys.parameters
 WHERE object_id = OBJECT_ID('sp_PacientiPersonalMedical_ActivateRelatie')
    AND name IN ('@RelatieID', '@Observatii', '@Motiv', '@ModificatDe');
    
    IF @NumarParametri = 4
        INSERT INTO @Verificari VALUES ('Parametri SP', '? OK', 'To?i parametrii exist? (4/4)');
    ELSE
        INSERT INTO @Verificari VALUES ('Parametri SP', '? EROARE', 'Parametri lips? (' + CAST(@NumarParametri AS VARCHAR) + '/4)');
END
ELSE
BEGIN
    INSERT INTO @Verificari VALUES ('SP ActivateRelatie', '? EROARE', 'NU exist?');
    INSERT INTO @Verificari VALUES ('Parametri SP', '? SKIP', 'SP lipse?te');
END

-- ============================================================================
-- STEP 3: Verificare Date Test (op?ional)
-- ============================================================================
PRINT 'STEP 3: Verificare date test...';
PRINT '';

IF OBJECT_ID('dbo.Pacienti_PersonalMedical', 'U') IS NOT NULL
BEGIN
    DECLARE @TotalRelatii INT, @RelatiiActive INT, @RelatiiInactive INT;
    
    SELECT @TotalRelatii = COUNT(*) FROM Pacienti_PersonalMedical;
 SELECT @RelatiiActive = COUNT(*) FROM Pacienti_PersonalMedical WHERE EsteActiv = 1;
    SELECT @RelatiiInactive = COUNT(*) FROM Pacienti_PersonalMedical WHERE EsteActiv = 0;
    
    INSERT INTO @Verificari VALUES 
        ('Date Test', '? INFO', 'Total: ' + CAST(@TotalRelatii AS VARCHAR) + ' | Active: ' + CAST(@RelatiiActive AS VARCHAR) + ' | Inactive: ' + CAST(@RelatiiInactive AS VARCHAR));
    
    IF @RelatiiInactive > 0
        INSERT INTO @Verificari VALUES ('Rela?ii Inactive', '? INFO', 'Exist? ' + CAST(@RelatiiInactive AS VARCHAR) + ' rela?ii inactive pentru testare');
    ELSE
        INSERT INTO @Verificari VALUES ('Rela?ii Inactive', '? WARN', 'Nu exist? rela?ii inactive pentru testare');
END
ELSE
    INSERT INTO @Verificari VALUES ('Date Test', '? SKIP', 'Tabelul lipse?te');

-- ============================================================================
-- STEP 4: Verificare Indexuri (op?ional)
-- ============================================================================
PRINT 'STEP 4: Verificare indexuri...';
PRINT '';

IF OBJECT_ID('dbo.Pacienti_PersonalMedical', 'U') IS NOT NULL
BEGIN
    DECLARE @NumarIndexuri INT;
    SELECT @NumarIndexuri = COUNT(*)
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('Pacienti_PersonalMedical')
    AND type > 0; -- Exclude heap
    
    IF @NumarIndexuri >= 4
        INSERT INTO @Verificari VALUES ('Indexuri', '? OK', 'Exist? ' + CAST(@NumarIndexuri AS VARCHAR) + ' indexuri');
    ELSE
        INSERT INTO @Verificari VALUES ('Indexuri', '? WARN', 'Prea pu?ine indexuri (' + CAST(@NumarIndexuri AS VARCHAR) + ')');
END
ELSE
    INSERT INTO @Verificari VALUES ('Indexuri', '? SKIP', 'Tabelul lipse?te');

-- ============================================================================
-- AFI?ARE REZULTATE
-- ============================================================================
PRINT '';
PRINT '========================================';
PRINT 'REZULTATE VERIFICARE';
PRINT '========================================';
PRINT '';

SELECT 
    Test,
    Status,
    Detalii
FROM @Verificari
ORDER BY
    CASE 
      WHEN Status LIKE '%OK%' THEN 1
        WHEN Status LIKE '%INFO%' THEN 2
        WHEN Status LIKE '%WARN%' THEN 3
        WHEN Status LIKE '%EROARE%' THEN 4
        ELSE 5
    END;

-- ============================================================================
-- VERIFICARE FINAL?
-- ============================================================================
PRINT '';
PRINT '========================================';

DECLARE @Erori INT, @Warnings INT;
SELECT @Erori = COUNT(*) FROM @Verificari WHERE Status LIKE '%EROARE%';
SELECT @Warnings = COUNT(*) FROM @Verificari WHERE Status LIKE '%WARN%';

IF @Erori = 0 AND @Warnings = 0
BEGIN
    PRINT '??? TOATE VERIFIC?RILE AU TRECUT CU SUCCES! ???';
    PRINT '';
    PRINT '?? FUNC?IONALITATEA ESTE GATA DE UTILIZARE!';
    PRINT '';
    PRINT 'Urm?torii pa?i:';
    PRINT '1. Rebuild Blazor app (dotnet build)';
    PRINT '2. Restart application';
    PRINT '3. Testare în UI (Pacienti ? Edit ? Tab Doctori)';
END
ELSE IF @Erori = 0 AND @Warnings > 0
BEGIN
  PRINT '??  VERIFIC?RI CU AVERTISMENTE ??';
    PRINT '';
    PRINT 'Avertismente: ' + CAST(@Warnings AS VARCHAR);
    PRINT '';
    PRINT 'Func?ionalitatea poate func?iona, dar verifica?i avertismentele de mai sus.';
END
ELSE
BEGIN
    PRINT '? VERIFIC?RI CU ERORI ?';
    PRINT '';
    PRINT 'Erori: ' + CAST(@Erori AS VARCHAR);
    IF @Warnings > 0
        PRINT 'Avertismente: ' + CAST(@Warnings AS VARCHAR);
    PRINT '';
    PRINT '?? AC?IUNI NECESARE:';
    PRINT '1. Verifica?i erorile de mai sus';
  PRINT '2. Rula?i scripturile de creare lips?:';
    PRINT '   - DevSupport/Database/TableStructure/PacientiPersonalMedical_CreateTable.sql';
    PRINT '   - DevSupport/Database/StoredProcedures/sp_PacientiPersonalMedical_ActivateRelatie.sql';
    PRINT '3. Re-rula?i acest script de verificare';
END

PRINT '========================================';
PRINT '';

-- ============================================================================
-- TEST OPTIONAL (Decomenteaz? pentru a rula)
-- ============================================================================

/*
-- Test rapid al SP cu date fictive
DECLARE @TestRelatieID UNIQUEIDENTIFIER = (
    SELECT TOP 1 Id 
    FROM Pacienti_PersonalMedical 
    WHERE EsteActiv = 0
);

IF @TestRelatieID IS NOT NULL
BEGIN
    PRINT '';
    PRINT '========================================';
    PRINT 'TEST OPTIONAL: Reactivare Rela?ie';
    PRINT '========================================';
    PRINT 'RelatieID: ' + CAST(@TestRelatieID AS VARCHAR(36));
    PRINT '';
    
    EXEC sp_PacientiPersonalMedical_ActivateRelatie
        @RelatieID = @TestRelatieID,
   @Observatii = 'Test automat reactivare',
        @Motiv = 'Verificare func?ionalitate',
        @ModificatDe = 'Script Verificare';
    
    PRINT '';
    PRINT '? Test executat cu succes!';
    PRINT '========================================';
END
ELSE
BEGIN
    PRINT '';
    PRINT '?? Nu exist? rela?ii inactive pentru test';
END
*/

-- ============================================================================
-- AFI?ARE INFO DETALII (pentru debugging)
-- ============================================================================

PRINT '';
PRINT '========================================';
PRINT 'INFORMA?II SUPLIMENTARE';
PRINT '========================================';
PRINT '';

-- Structura tabelului
IF OBJECT_ID('dbo.Pacienti_PersonalMedical', 'U') IS NOT NULL
BEGIN
    PRINT 'Structura tabelului Pacienti_PersonalMedical:';
    PRINT '';
    
    SELECT 
        COLUMN_NAME AS Coloana,
  DATA_TYPE AS Tip,
        CHARACTER_MAXIMUM_LENGTH AS Lungime,
        IS_NULLABLE AS Nullable
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Pacienti_PersonalMedical'
    ORDER BY ORDINAL_POSITION;
END

PRINT '';
PRINT '? Script de verificare finalizat!';
PRINT '?? Pentru detalii complete, consulta?i: DevSupport/Database/TableStructure/Pacienti_PersonalMedical_README.md';
PRINT '';
GO
