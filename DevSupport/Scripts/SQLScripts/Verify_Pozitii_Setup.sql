-- =============================================
-- VERIFICARE: Tabela Pozitii si rela?ia cu PersonalMedical
-- Pentru debugging problema cu salvarea pozitiei
-- =============================================

USE [ValyanMed]
GO

PRINT '=============================================='
PRINT 'VERIFICARE SETUP POZITII'
PRINT '=============================================='
PRINT ''

-- =============================================
-- 1. Verificare tabel Pozitii
-- =============================================
PRINT '1. Verificare tabel Pozitii:'
PRINT '------------------------------'

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Pozitii')
BEGIN
    PRINT '[OK] Tabela Pozitii exista'
    
    -- Verifica coloanele
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        CHARACTER_MAXIMUM_LENGTH
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Pozitii'
    ORDER BY ORDINAL_POSITION
    
    -- Verifica numarul de inregistrari
    DECLARE @TotalPozitii INT = (SELECT COUNT(*) FROM Pozitii)
    DECLARE @PozitiiActive INT = (SELECT COUNT(*) FROM Pozitii WHERE Este_Activ = 1)
    
    PRINT ''
    PRINT 'Statistici Pozitii:'
    PRINT '  Total: ' + CAST(@TotalPozitii AS VARCHAR(10))
    PRINT '  Active: ' + CAST(@PozitiiActive AS VARCHAR(10))
    
    IF @PozitiiActive = 0
    BEGIN
      PRINT '[WARNING] Nu exista pozitii active!'
    END
    
    -- Lista pozitii active
    PRINT ''
    PRINT 'Pozitii active (primele 10):'
    SELECT TOP 10 
        Id,
        Denumire,
        Este_Activ,
        Data_Crearii
    FROM Pozitii 
    WHERE Este_Activ = 1
    ORDER BY Denumire
    
END
ELSE
BEGIN
    PRINT '[ERROR] Tabela Pozitii nu exista!'
    PRINT 'Executati scriptul pentru crearea tabelei Pozitii'
END

PRINT ''

-- =============================================
-- 2. Verificare tabel PersonalMedical
-- =============================================
PRINT '2. Verificare tabel PersonalMedical:'
PRINT '------------------------------------'

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PersonalMedical')
BEGIN
    PRINT '[OK] Tabela PersonalMedical exista'
    
    -- Verifica coloanele relevante
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
   CHARACTER_MAXIMUM_LENGTH
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PersonalMedical' 
    AND COLUMN_NAME IN ('PersonalID', 'Pozitie', 'PozitieID')
    ORDER BY ORDINAL_POSITION
    
    -- Verifica daca coloana PozitieID exista
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PersonalMedical' AND COLUMN_NAME = 'PozitieID')
    BEGIN
   PRINT '[OK] Coloana PozitieID exista'
    END
    ELSE
    BEGIN
        PRINT '[WARNING] Coloana PozitieID nu exista - doar campul text Pozitie'
    END
    
    -- Statistici PersonalMedical
    DECLARE @TotalPersonal INT = (SELECT COUNT(*) FROM PersonalMedical)
    DECLARE @PersonalCuPozitie INT = (SELECT COUNT(*) FROM PersonalMedical WHERE Pozitie IS NOT NULL AND Pozitie != '')
    
  PRINT ''
    PRINT 'Statistici PersonalMedical:'
    PRINT '  Total: ' + CAST(@TotalPersonal AS VARCHAR(10))
    PRINT '  Cu Pozitie: ' + CAST(@PersonalCuPozitie AS VARCHAR(10))
    
    -- Pozitii distincte folosite
    PRINT ''
    PRINT 'Pozitii distincte folosite in PersonalMedical:'
    SELECT 
        Pozitie,
   COUNT(*) as Numar
    FROM PersonalMedical 
    WHERE Pozitie IS NOT NULL AND Pozitie != ''
    GROUP BY Pozitie
    ORDER BY COUNT(*) DESC
    
END
ELSE
BEGIN
    PRINT '[ERROR] Tabela PersonalMedical nu exista!'
END

PRINT ''

-- =============================================
-- 3. Verificare Foreign Key intre tabele
-- =============================================
PRINT '3. Verificare Foreign Key:'
PRINT '-------------------------'

SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS ParentTable,
    pc.name AS ParentColumn,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    rc.name AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns pc ON fkc.parent_object_id = pc.object_id AND fkc.parent_column_id = pc.column_id
INNER JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
WHERE (OBJECT_NAME(fk.parent_object_id) = 'PersonalMedical' AND pc.name LIKE '%Pozitie%')
   OR (OBJECT_NAME(fk.referenced_object_id) = 'Pozitii')

IF @@ROWCOUNT = 0
BEGIN
    PRINT '[INFO] Nu exista Foreign Key intre PersonalMedical si Pozitii'
    PRINT 'Se foloseste doar campul text Pozitie'
END

PRINT ''

-- =============================================
-- 4. Verificare Stored Procedures
-- =============================================
PRINT '4. Verificare Stored Procedures:'
PRINT '-------------------------------'

-- Lista SP-uri PersonalMedical
SELECT 
    name AS ProcedureName,
    create_date AS CreateDate,
    modify_date AS ModifyDate
FROM sys.procedures 
WHERE name LIKE '%PersonalMedical%'
ORDER BY name

-- Verifica daca sp_PersonalMedical_Update exista
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_PersonalMedical_Update')
BEGIN
    PRINT '[OK] sp_PersonalMedical_Update exista'
END
ELSE
BEGIN
    PRINT '[ERROR] sp_PersonalMedical_Update nu exista!'
END

PRINT ''

-- =============================================
-- 5. Test de compatibilitate
-- =============================================
PRINT '5. Test de compatibilitate:'
PRINT '---------------------------'

PRINT 'Pentru a testa functionalitatea, executati:'
PRINT ''
PRINT '-- Test 1: Verifica daca se poate face update'
PRINT 'IF EXISTS (SELECT TOP 1 * FROM PersonalMedical)'
PRINT 'BEGIN'
PRINT '    DECLARE @TestID UNIQUEIDENTIFIER = (SELECT TOP 1 PersonalID FROM PersonalMedical);'
PRINT '    SELECT PersonalID, Nume, Prenume, Pozitie FROM PersonalMedical WHERE PersonalID = @TestID;'
PRINT 'END'
PRINT ''
PRINT '-- Test 2: Verifica parametrii SP'
PRINT 'SELECT'
PRINT '    p.name AS ParameterName,'
PRINT '    t.name AS DataType,'
PRINT '  p.max_length AS MaxLength'
PRINT 'FROM sys.parameters p'
PRINT 'INNER JOIN sys.types t ON p.user_type_id = t.user_type_id'
PRINT 'WHERE object_id = OBJECT_ID(''sp_PersonalMedical_Update'')'
PRINT 'ORDER BY p.parameter_id;'

PRINT ''
PRINT '=============================================='
PRINT 'VERIFICARE COMPLETA!'
PRINT '=============================================='
PRINT ''
PRINT 'Urmatoarele pasi pentru rezolvare:'
PRINT '1. Daca Pozitii nu are inregistrari active, populati tabela'
PRINT '2. Daca PozitieID nu exista, adaugati coloana si FK'
PRINT '3. Verificati log-urile din aplicatie pentru valorile trimise'
PRINT '4. Rulati Fix_PersonalMedical_Update_SP.sql pentru debug'
PRINT ''
PRINT '=============================================='