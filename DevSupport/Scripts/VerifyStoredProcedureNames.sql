-- =============================================
-- VERIFICARE CONCORDANTA STORED PROCEDURES BAZA DE DATE vs COD
-- Verific? dac? SP-urile din baza de date au aceea?i denumire cu cele folosite în cod
-- =============================================

PRINT '?? VERIFICARE STORED PROCEDURES - BAZA DE DATE vs COD'
PRINT '======================================================'

-- =============================================
-- SECTION 1: Lista SP-urilor din baza de date
-- =============================================
PRINT ''
PRINT '?? SECTION 1: Stored Procedures existente în baza de date'
PRINT '------------------------------------------------------'

SELECT 
    name as StoredProcedureName,
    type_desc as Type,
    create_date as CreateDate,
    modify_date as LastModified
FROM sys.objects 
WHERE type = 'P' 
    AND name LIKE 'sp_%'
ORDER BY name;

-- =============================================
-- SECTION 2: SP-urile folosite în COD (PersonalRepository)
-- =============================================
PRINT ''
PRINT '?? SECTION 2: Verificare SP-uri Personal (PersonalRepository.cs)'
PRINT '------------------------------------------------------'

-- Lista SP-urilor g?site în PersonalRepository.cs:
DECLARE @PersonalSPs TABLE (SPName VARCHAR(100), UsedInMethod VARCHAR(100))
INSERT INTO @PersonalSPs VALUES
    ('sp_Personal_GetAll', 'GetAllAsync'),
    ('sp_Personal_GetById', 'GetByIdAsync'),
    ('sp_Personal_Create', 'CreateAsync'),
    ('sp_Personal_Update', 'UpdateAsync'),
    ('sp_Personal_Delete', 'DeleteAsync'),
    ('sp_Personal_CheckUnique', 'CheckUniqueAsync'),
    ('sp_Personal_GetStatistics', 'GetStatisticsAsync')

-- Verificare dac? exist? în baza de date
SELECT 
    p.SPName,
    p.UsedInMethod,
    CASE 
        WHEN s.name IS NOT NULL THEN '? EXIST?'
        ELSE '? LIPSE?TE'
    END as StatusInDB,
    s.create_date as CreateDate
FROM @PersonalSPs p
LEFT JOIN sys.objects s ON s.name = p.SPName AND s.type = 'P'
ORDER BY p.SPName;

-- =============================================
-- SECTION 3: SP-urile folosite în COD (PersonalMedicalStoredProcedures.sql)
-- =============================================
PRINT ''
PRINT '?? SECTION 3: Verificare SP-uri PersonalMedical'
PRINT '------------------------------------------------------'

DECLARE @PersonalMedicalSPs TABLE (SPName VARCHAR(100), Purpose VARCHAR(200))
INSERT INTO @PersonalMedicalSPs VALUES
    ('sp_PersonalMedical_GetAll', 'Obtinerea listei de personal medical cu filtrare ?i paginare'),
    ('sp_PersonalMedical_GetStatistics', 'Obtinerea statisticilor personal medical'),
    ('sp_PersonalMedical_GetById', 'Obtinerea unui personal medical dupa ID'),
    ('sp_PersonalMedical_CheckUnique', 'Verificarea unicitatii Email ?i NumarLicenta'),
    ('sp_PersonalMedical_Create', 'Crearea unui nou personal medical'),
    ('sp_PersonalMedical_Update', 'Actualizarea unui personal medical'),
    ('sp_PersonalMedical_Delete', 'Stergerea unui personal medical (soft delete)'),
    ('sp_PersonalMedical_GetDropdownOptions', 'Obtinerea optiunilor pentru dropdown-uri')

-- Verificare PersonalMedical SPs
SELECT 
    pm.SPName,
    pm.Purpose,
    CASE 
        WHEN s.name IS NOT NULL THEN '? EXIST?'
        ELSE '? LIPSE?TE'
    END as StatusInDB,
    s.create_date as CreateDate
FROM @PersonalMedicalSPs pm
LEFT JOIN sys.objects s ON s.name = pm.SPName AND s.type = 'P'
ORDER BY pm.SPName;

-- =============================================
-- SECTION 4: SP-urile pentru Departamente
-- =============================================
PRINT ''
PRINT '?? SECTION 4: Verificare SP-uri Departamente'
PRINT '------------------------------------------------------'

DECLARE @DepartamenteSPs TABLE (SPName VARCHAR(100), Purpose VARCHAR(200))
INSERT INTO @DepartamenteSPs VALUES
    ('sp_Departamente_GetAll', 'Obtinerea tuturor departamentelor'),
    ('sp_Departamente_GetByTip', 'Obtinerea departamentelor dupa tip'),
    ('sp_Departamente_GetById', 'Obtinerea unui departament dupa ID'),
    ('sp_Departamente_Create', 'Crearea unui nou departament'),
    ('sp_Departamente_Update', 'Actualizarea unui departament'),
    ('sp_Departamente_Delete', 'Stergerea unui departament')

-- Verificare Departamente SPs
SELECT 
    d.SPName,
    d.Purpose,
    CASE 
        WHEN s.name IS NOT NULL THEN '? EXIST?'
        ELSE '? LIPSE?TE'
    END as StatusInDB,
    s.create_date as CreateDate
FROM @DepartamenteSPs d
LEFT JOIN sys.objects s ON s.name = d.SPName AND s.type = 'P'
ORDER BY d.SPName;

-- =============================================
-- SECTION 5: SP-urile pentru Jude?e ?i Localit??i (LocationRepository)
-- =============================================
PRINT ''
PRINT '?? SECTION 5: Verificare SP-uri Location (Jude?e ?i Localit??i)'
PRINT '------------------------------------------------------'

DECLARE @LocationSPs TABLE (SPName VARCHAR(100), Purpose VARCHAR(200))
INSERT INTO @LocationSPs VALUES
    ('sp_Judete_GetAll', 'Obtinerea tuturor judeteor'),
    ('sp_Judete_GetOrderedByName', 'Obtinerea judetelor ordonate dupa nume'),
    ('sp_Judete_GetById', 'Obtinerea unui judet dupa ID'),
    ('sp_Judete_GetByCod', 'Obtinerea unui judet dupa cod'),
    ('sp_Localitati_GetAll', 'Obtinerea tuturor localitatilor'),
    ('sp_Localitati_GetById', 'Obtinerea unei localitati dupa ID'),
    ('sp_Localitati_GetByJudetId', 'Obtinerea localitatilor dupa judet ID'),
    ('sp_Localitati_GetByJudetIdOrdered', 'Obtinerea localitatilor ordonate dupa judet ID')

-- Verificare Location SPs
SELECT 
    l.SPName,
    l.Purpose,
    CASE 
        WHEN s.name IS NOT NULL THEN '? EXIST?'
        ELSE '? LIPSE?TE'
    END as StatusInDB,
    s.create_date as CreateDate
FROM @LocationSPs l
LEFT JOIN sys.objects s ON s.name = l.SPName AND s.type = 'P'
ORDER BY l.SPName;

-- =============================================
-- SECTION 6: SUMAR GENERAL
-- =============================================
PRINT ''
PRINT '?? SECTION 6: SUMAR VERIFICARE'
PRINT '======================================================'

-- Count total SPs în baza de date
DECLARE @TotalSPsInDB INT
SELECT @TotalSPsInDB = COUNT(*) FROM sys.objects WHERE type = 'P' AND name LIKE 'sp_%'

-- Count SPs definite în cod
DECLARE @TotalSPsInCode INT = 23  -- Total din toate categoriile de mai sus

PRINT '?? STATISTICI:'
PRINT '   • Total SP-uri în baza de date: ' + CAST(@TotalSPsInDB AS VARCHAR(10))
PRINT '   • Total SP-uri definite în cod: ' + CAST(@TotalSPsInCode AS VARCHAR(10))

-- Verificare SPs extra în baza de date (care nu sunt în cod)
PRINT ''
PRINT '??  SP-uri din baza de date care NU sunt folosite în cod:'
PRINT '------------------------------------------------------'

SELECT s.name as UnusedStoredProcedure
FROM sys.objects s
WHERE s.type = 'P' 
    AND s.name LIKE 'sp_%'
    AND s.name NOT IN (
        -- Personal SPs
        'sp_Personal_GetAll', 'sp_Personal_GetById', 'sp_Personal_Create', 
        'sp_Personal_Update', 'sp_Personal_Delete', 'sp_Personal_CheckUnique', 
        'sp_Personal_GetStatistics',
        -- PersonalMedical SPs  
        'sp_PersonalMedical_GetAll', 'sp_PersonalMedical_GetStatistics',
        'sp_PersonalMedical_GetById', 'sp_PersonalMedical_CheckUnique',
        'sp_PersonalMedical_Create', 'sp_PersonalMedical_Update', 
        'sp_PersonalMedical_Delete', 'sp_PersonalMedical_GetDropdownOptions',
        -- Departamente SPs
        'sp_Departamente_GetAll', 'sp_Departamente_GetByTip', 
        'sp_Departamente_GetById', 'sp_Departamente_Create',
        'sp_Departamente_Update', 'sp_Departamente_Delete',
        -- Location SPs
        'sp_Judete_GetAll', 'sp_Judete_GetOrderedByName', 'sp_Judete_GetById', 
        'sp_Judete_GetByCod', 'sp_Localitati_GetAll', 'sp_Localitati_GetById',
        'sp_Localitati_GetByJudetId', 'sp_Localitati_GetByJudetIdOrdered'
    )
ORDER BY s.name;

-- =============================================
-- SECTION 7: RECOMAND?RI
-- =============================================
PRINT ''
PRINT '?? RECOMAND?RI:'
PRINT '------------------------------------------------------'
PRINT '1. Verifica?i SP-urile marcate cu ? LIPSE?TE ?i crea?i-le folosind scripturile din DevSupport/Scripts/'
PRINT '2. SP-urile nefolosite pot fi ?terse sau documentate ca fiind pentru uz viitor'
PRINT '3. Asigura?i-v? c? toate SP-urile au acelea?i nume în cod ?i în baza de date'
PRINT '4. Testa?i fiecare SP folosind scripturile de test din DevSupport/Scripts/'
PRINT '5. Documenta?i orice modific?ri în fi?ierele de documenta?ie'
PRINT ''
PRINT '?? Pentru a rula testele complete, executa?i:'
PRINT '   • sp_Departamente_Test.sql - pentru testare Departamente'
PRINT '   • Test-PersonalMedicalStoredProcedures.ps1 - pentru testare PersonalMedical'
PRINT '   • SP_Personal_GetAll.sql ?i alte scripturi individuale'
PRINT ''
PRINT '? VERIFICARE COMPLET?!'
PRINT '======================================================'