-- =============================================
-- TEST RAPID: Verificare salvare Pozitie
-- Debug problema de persisten?? in PersonalMedical
-- =============================================

USE [ValyanMed]
GO

PRINT '=============================================='
PRINT 'TEST RAPID: Verificare PersonalMedical Pozitie'
PRINT '=============================================='
PRINT ''

-- =============================================
-- 1. Verificare structura tabel PersonalMedical
-- =============================================
PRINT '1. Verificare structura tabel PersonalMedical:'
PRINT '----------------------------------------------'

SELECT 
    COLUMN_NAME as 'Nume Coloana',
 DATA_TYPE as 'Tip Date',
    IS_NULLABLE as 'Nullable',
    CHARACTER_MAXIMUM_LENGTH as 'Lungime Max'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PersonalMedical' 
AND COLUMN_NAME IN ('PersonalID', 'Pozitie', 'PozitieID')
ORDER BY ORDINAL_POSITION

PRINT ''

-- =============================================
-- 2. Verificare date existente
-- =============================================
PRINT '2. Date curente PersonalMedical:'
PRINT '--------------------------------'

SELECT 
    PersonalID,
    Nume,
    Prenume,
    Pozitie,
 PozitieID,
    DataCreare
FROM PersonalMedical
ORDER BY DataCreare DESC

PRINT ''

-- =============================================
-- 3. Test UPDATE direct
-- =============================================
PRINT '3. Test UPDATE direct pe primul record:'
PRINT '---------------------------------------'

-- Preiau primul record pentru test
DECLARE @TestPersonalID UNIQUEIDENTIFIER
DECLARE @TestNume NVARCHAR(100)
DECLARE @TestPrenume NVARCHAR(100)

SELECT TOP 1 
    @TestPersonalID = PersonalID,
    @TestNume = Nume,
    @TestPrenume = Prenume
FROM PersonalMedical
ORDER BY DataCreare DESC

IF @TestPersonalID IS NOT NULL
BEGIN
    PRINT 'Test PersonalID: ' + CAST(@TestPersonalID AS NVARCHAR(50))
    PRINT 'Test Nume: ' + @TestNume + ' ' + @TestPrenume
    
    -- Afisare valori inainte de update
  PRINT ''
    PRINT 'INAINTE de UPDATE:'
    SELECT PersonalID, Nume, Prenume, Pozitie, PozitieID 
    FROM PersonalMedical 
    WHERE PersonalID = @TestPersonalID
    
    -- Update direct cu noua pozitie
    DECLARE @NouaPozitie NVARCHAR(50) = 'TEST POZITIE DIRECTA'
    DECLARE @NouaPozitieID UNIQUEIDENTIFIER = '97864932-F4AB-F011-BB6C-20235109A3A2' -- ID-ul din log-urile tale
    
  PRINT ''
    PRINT 'Execut UPDATE cu:'
    PRINT '  Pozitie: ' + @NouaPozitie  
    PRINT '  PozitieID: ' + CAST(@NouaPozitieID AS NVARCHAR(50))
    
    UPDATE PersonalMedical 
    SET 
     Pozitie = @NouaPozitie,
        PozitieID = @NouaPozitieID
    WHERE PersonalID = @TestPersonalID
    
    PRINT 'Rows affected: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
  
    -- Afisare valori dupa update
    PRINT ''
    PRINT 'DUPA UPDATE:'
    SELECT PersonalID, Nume, Prenume, Pozitie, PozitieID 
    FROM PersonalMedical 
    WHERE PersonalID = @TestPersonalID
    
END
ELSE
BEGIN
    PRINT 'NU EXISTA DATE IN PersonalMedical pentru test!'
END

PRINT ''

-- =============================================
-- 4. Test cu Stored Procedure
-- =============================================
PRINT '4. Test cu Stored Procedure:'
PRINT '----------------------------'

IF @TestPersonalID IS NOT NULL
BEGIN
    DECLARE @TestPozitie2 NVARCHAR(50) = 'TEST POZITIE SP'
    DECLARE @TestPozitieID2 UNIQUEIDENTIFIER = '97864932-F4AB-F011-BB6C-20235109A3A2'
    
    PRINT 'Test cu sp_PersonalMedical_Update'
    PRINT 'Parametri:'
    PRINT 'PersonalID: ' + CAST(@TestPersonalID AS NVARCHAR(50))
    PRINT '  Nume: ' + @TestNume
    PRINT '  Prenume: ' + @TestPrenume  
    PRINT '  Pozitie: ' + @TestPozitie2
    PRINT '  PozitieID: nu este parametru in SP!'
    
    -- IMPORTANT: Verifica daca SP accepta PozitieID
    SELECT 
        p.name AS ParameterName,
        t.name AS DataType
    FROM sys.parameters p
    INNER JOIN sys.types t ON p.user_type_id = t.user_type_id
  WHERE object_id = OBJECT_ID('sp_PersonalMedical_Update')
    AND p.name LIKE '%Pozitie%'
    ORDER BY p.parameter_id
    
    -- Executa SP doar cu parametrii disponibili
    IF EXISTS (SELECT * FROM sys.parameters WHERE object_id = OBJECT_ID('sp_PersonalMedical_Update'))
    BEGIN
        EXEC sp_PersonalMedical_Update
    @PersonalID = @TestPersonalID,
 @Nume = @TestNume,
         @Prenume = @TestPrenume,
            @Pozitie = @TestPozitie2,
 @EsteActiv = 1
     
   PRINT ''
        PRINT 'REZULTAT dupa SP:'
        SELECT PersonalID, Nume, Prenume, Pozitie, PozitieID 
   FROM PersonalMedical 
WHERE PersonalID = @TestPersonalID
    END
    ELSE
 BEGIN
        PRINT 'SP sp_PersonalMedical_Update nu exista!'
    END
END

PRINT ''

-- =============================================
-- 5. Verificare finala si diagnosticare
-- =============================================
PRINT '5. Diagnosticare problema:'
PRINT '-------------------------'

-- Verifica daca coloana PozitieID exista si poate fi actualizata
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
 WHERE TABLE_NAME = 'PersonalMedical' AND COLUMN_NAME = 'PozitieID')
BEGIN
    PRINT '[OK] Coloana PozitieID exista in tabel'
    
    -- Verifica constraint-uri pe PozitieID
    SELECT 
        'CONSTRAINT: ' + cc.CONSTRAINT_NAME + ' TYPE: ' + cc.CONSTRAINT_TYPE as Info
    FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu
    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS cc ON ccu.CONSTRAINT_NAME = cc.CONSTRAINT_NAME
    WHERE ccu.TABLE_NAME = 'PersonalMedical' 
    AND ccu.COLUMN_NAME = 'PozitieID'
    
    IF @@ROWCOUNT = 0
        PRINT '[INFO] Nu exista constraint-uri pe PozitieID'
        
END
ELSE
BEGIN
    PRINT '[PROBLEMA] Coloana PozitieID NU EXISTA in tabel!'
    PRINT 'SOLUTIE: Adaugati coloana PozitieID in tabel'
 PRINT 'ALTER TABLE PersonalMedical ADD PozitieID UNIQUEIDENTIFIER NULL'
END

-- Verifica parametrii SP
PRINT ''
PRINT 'Parametrii sp_PersonalMedical_Update:'
SELECT 
    p.name AS ParameterName,
    t.name AS DataType,
    p.max_length AS MaxLength
FROM sys.parameters p
INNER JOIN sys.types t ON p.user_type_id = t.user_type_id
WHERE object_id = OBJECT_ID('sp_PersonalMedical_Update')
ORDER BY p.parameter_id

PRINT ''
PRINT '=============================================='
PRINT 'TEST COMPLET!'
PRINT '=============================================='
PRINT ''
PRINT 'PROBLEME POSIBILE IDENTIFICATE:'
PRINT '1. Coloana PozitieID nu exista in tabel'
PRINT '2. SP nu accepta parametrul PozitieID'  
PRINT '3. Trigger sau constraint blocheaza update-ul'
PRINT '4. Aplicatia nu trimite PozitieID catre SP'
PRINT ''
PRINT 'VERIFICATI REZULTATELE DE MAI SUS!'
PRINT '=============================================='