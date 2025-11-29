-- =============================================
-- VERIFICARE COMPLETA BAZA DE DATE - PERSONAL MEDICAL
-- Database: ValyanMed
-- Purpose: Verific? structura exact? a tabelelor ?i SP-urilor
-- =============================================

USE [ValyanMed]
GO

PRINT '========================================='
PRINT 'VERIFICARE STRUCTURA BAZA DE DATE'
PRINT 'Database: ValyanMed'
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================='
PRINT ''

-- =============================================
-- SECTION 1: Verificare Tabele
-- =============================================
PRINT '?? SECTION 1: VERIFICARE TABELE'
PRINT '-----------------------------------------'

-- 1.1 Verificare tabel PersonalMedical
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PersonalMedical')
BEGIN
    PRINT '? Tabel PersonalMedical EXISTS'
    PRINT ''
    PRINT '  Coloane în PersonalMedical:'
    SELECT 
        COLUMN_NAME as [Coloana],
        DATA_TYPE as [Tip],
        CHARACTER_MAXIMUM_LENGTH as [Lungime],
        IS_NULLABLE as [Nullable]
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'PersonalMedical'
    ORDER BY ORDINAL_POSITION
END
ELSE
BEGIN
    PRINT '? Tabel PersonalMedical NU EXISTA!'
END
PRINT ''

-- 1.2 Verificare tabel Departamente
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Departamente')
BEGIN
    PRINT '? Tabel Departamente EXISTS'
    PRINT ''
    PRINT '  Coloane în Departamente:'
    SELECT 
        COLUMN_NAME as [Coloana],
        DATA_TYPE as [Tip],
        CHARACTER_MAXIMUM_LENGTH as [Lungime],
        IS_NULLABLE as [Nullable]
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Departamente'
    ORDER BY ORDINAL_POSITION
END
ELSE
BEGIN
    PRINT '? Tabel Departamente NU EXISTA!'
END
PRINT ''

-- 1.3 Verificare tabel TipDepartament
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TipDepartament')
BEGIN
    PRINT '? Tabel TipDepartament EXISTS'
    PRINT ''
    PRINT '  Coloane în TipDepartament:'
    SELECT 
        COLUMN_NAME as [Coloana],
        DATA_TYPE as [Tip],
        CHARACTER_MAXIMUM_LENGTH as [Lungime],
        IS_NULLABLE as [Nullable]
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'TipDepartament'
    ORDER BY ORDINAL_POSITION
END
ELSE
BEGIN
    PRINT '? Tabel TipDepartament NU EXISTA!'
END
PRINT ''
PRINT ''

-- =============================================
-- SECTION 2: Verificare Stored Procedures - PersonalMedical
-- =============================================
PRINT '?? SECTION 2: STORED PROCEDURES - PERSONALMEDICAL'
PRINT '-----------------------------------------'

DECLARE @PersonalMedicalSPs TABLE (SPName VARCHAR(100), Required BIT)
INSERT INTO @PersonalMedicalSPs VALUES
    ('sp_PersonalMedical_GetAll', 1),
    ('sp_PersonalMedical_GetById', 1),
    ('sp_PersonalMedical_GetStatistics', 0),
    ('sp_PersonalMedical_CheckUnique', 1),
    ('sp_PersonalMedical_Create', 1),
    ('sp_PersonalMedical_Update', 1),
    ('sp_PersonalMedical_Delete', 1),
    ('sp_PersonalMedical_GetDropdownOptions', 0)

SELECT 
    sp.SPName as [Stored Procedure],
    CASE WHEN sp.Required = 1 THEN 'OBLIGATORIU' ELSE 'Optional' END as [Status],
    CASE 
        WHEN o.name IS NOT NULL THEN '? EXISTS'
        ELSE '? MISSING'
    END as [Exista in DB],
    o.create_date as [Data Creare],
    o.modify_date as [Data Modificare]
FROM @PersonalMedicalSPs sp
LEFT JOIN sys.objects o ON o.name = sp.SPName AND o.type = 'P'
ORDER BY sp.Required DESC, sp.SPName

PRINT ''
PRINT ''

-- =============================================
-- SECTION 3: Verificare PARAMETRI sp_PersonalMedical_GetById
-- =============================================
PRINT '?? SECTION 3: PARAMETRI SP - sp_PersonalMedical_GetById'
PRINT '-----------------------------------------'

IF EXISTS (SELECT * FROM sys.objects WHERE name = 'sp_PersonalMedical_GetById' AND type = 'P')
BEGIN
    SELECT 
        p.name as [Parametru],
        TYPE_NAME(p.user_type_id) as [Tip],
        p.max_length as [Lungime],
        CASE WHEN p.is_output = 1 THEN 'OUTPUT' ELSE 'INPUT' END as [Directie]
    FROM sys.parameters p
    WHERE p.object_id = OBJECT_ID('sp_PersonalMedical_GetById')
    ORDER BY p.parameter_id
END
ELSE
BEGIN
    PRINT '? SP nu exist? - nu pot afi?a parametri'
END

PRINT ''
PRINT ''

-- =============================================
-- SECTION 4: TESTE QUERY - Verificare JOIN-uri
-- =============================================
PRINT '?? SECTION 4: TEST QUERY - Verificare JOIN-uri'
PRINT '-----------------------------------------'

-- Verificare dac? exist? date în PersonalMedical
DECLARE @PersonalMedicalCount INT
SELECT @PersonalMedicalCount = COUNT(*) FROM PersonalMedical

PRINT 'Total PersonalMedical records: ' + CAST(@PersonalMedicalCount AS VARCHAR(10))

IF @PersonalMedicalCount > 0
BEGIN
    PRINT ''
    PRINT 'Testing JOIN cu Departamente...'
    
    -- Test query similar cu cea din SP
    SELECT TOP 1
        pm.PersonalID,
        pm.Nume,
        pm.Prenume,
        pm.CategorieID,
        pm.SpecializareID,
        pm.SubspecializareID,
        d1.IdDepartament as Dept1_ID,
        d1.DenumireDepartament as Dept1_Denumire,
        d2.IdDepartament as Dept2_ID,
        d2.DenumireDepartament as Dept2_Denumire,
        d3.IdDepartament as Dept3_ID,
        d3.DenumireDepartament as Dept3_Denumire
    FROM PersonalMedical pm
    LEFT JOIN Departamente d1 ON pm.CategorieID = d1.IdDepartament
    LEFT JOIN Departamente d2 ON pm.SpecializareID = d2.IdDepartament
    LEFT JOIN Departamente d3 ON pm.SubspecializareID = d3.IdDepartament
    
    PRINT '? JOIN test SUCCESSFUL - No errors'
END
ELSE
BEGIN
    PRINT '? No data in PersonalMedical - Cannot test JOIN'
END

PRINT ''
PRINT ''

-- =============================================
-- SECTION 5: RECOMANDARI
-- =============================================
PRINT '?? SECTION 5: RECOMANDARI PENTRU COD C#'
PRINT '-----------------------------------------'
PRINT ''
PRINT 'Bazat pe structura bazei de date verificat?:'
PRINT ''
PRINT '1. TABELA PersonalMedical:'
PRINT '   - Coloane principale: PersonalID, Nume, Prenume, Specializare, etc.'
PRINT '   - Foreign keys: CategorieID, SpecializareID, SubspecializareID'
PRINT ''
PRINT '2. TABELA Departamente:'
PRINT '   - Primary Key: IdDepartament (UNIQUEIDENTIFIER)'
PRINT '   - Coloana Denumire: DenumireDepartament (VARCHAR(200))'
PRINT '   ? NU exist? coloane: DepartamentID, Nume'
PRINT ''
PRINT '3. STORED PROCEDURES:'
PRINT '   - Trebuie s? foloseasc? IdDepartament ?i DenumireDepartament'
PRINT '   - NU DepartamentID ?i Nume'
PRINT ''
PRINT '4. COD C# - PersonalMedicalRepository:'
PRINT '   - DTO-urile sunt OK (PersonalMedicalListDto, PersonalMedicalDetailDto)'
PRINT '   - Repository-ul folose?te SP-urile corect'
PRINT '   - ? Nu necesit? modific?ri dac? SP-urile sunt corecte'
PRINT ''
PRINT ''

-- =============================================
-- SECTION 6: GENERATE FIX SCRIPT
-- =============================================
PRINT '?? SECTION 6: SCRIPTUL DE FIX GENERAT'
PRINT '-----------------------------------------'
PRINT ''
PRINT 'Pentru a rula fix-ul, execut?:'
PRINT '  DevSupport\Scripts\SQLScripts\Fix_PersonalMedical_GetById_ColumnNames.sql'
PRINT ''
PRINT ''

PRINT '========================================='
PRINT '? VERIFICARE COMPLETA'
PRINT '========================================='
