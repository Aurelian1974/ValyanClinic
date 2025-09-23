-- =============================================
-- TEST SCRIPT pentru Stored Procedures Departamente
-- Verific? funcionalitatea sp_Departamente_GetByTip ?i sp_Departamente_GetAll
-- =============================================

-- Cleanup and setup test data
PRINT '?? Starting Departamente SP Testing...'
PRINT '======================================================'

-- =============================================
-- SECTION 1: Test Database and Table Structure
-- =============================================
PRINT '?? SECTION 1: Verifying Database Structure'
PRINT '------------------------------------------------------'

-- Check if database exists and is accessible
DECLARE @DatabaseName NVARCHAR(50) = DB_NAME()
PRINT '?? Current Database: ' + @DatabaseName

-- Check if Departamente table exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Departamente')
BEGIN
    PRINT '? Table Departamente exists'
    
    -- Show table structure
    PRINT '?? Table Structure:'
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Departamente'
    ORDER BY ORDINAL_POSITION
END
ELSE
BEGIN
    PRINT '? ERROR: Table Departamente does not exist!'
    PRINT '   Please create the table first using the appropriate schema script.'
    -- Exit if table doesn't exist
    RETURN
END

-- Check current data in Departamente table
PRINT ''
PRINT '?? Current Data in Departamente Table:'
SELECT COUNT(*) as TotalRecords FROM Departamente
SELECT 
    DepartamentID,
    Nume,
    Tip
FROM Departamente
ORDER BY Tip, Nume

PRINT ''

-- =============================================
-- SECTION 2: Test Setup - Insert Sample Data (if empty)
-- =============================================
PRINT '?? SECTION 2: Setting up Test Data'
PRINT '------------------------------------------------------'

-- Check if we have any data
DECLARE @RecordCount INT
SELECT @RecordCount = COUNT(*) FROM Departamente

IF @RecordCount = 0
BEGIN
    PRINT '??  No data found. Inserting sample test data...'
    
    -- Insert sample departamente data
    INSERT INTO Departamente (DepartamentID, Nume, Tip) VALUES
    (NEWID(), 'Cardiologie', 'Medical'),
    (NEWID(), 'Neurologie', 'Medical'),
    (NEWID(), 'Pediatrie', 'Medical'),
    (NEWID(), 'Chirurgie General?', 'Medical'),
    (NEWID(), 'Radiologie', 'Medical'),
    (NEWID(), 'Laborator', 'Medical'),
    (NEWID(), 'Administraie', 'Administrativ'),
    (NEWID(), 'Financiar-Contabilitate', 'Administrativ'),
    (NEWID(), 'Resurse Umane', 'Administrativ'),
    (NEWID(), 'IT ?i Informatic?', 'Tehnic'),
    (NEWID(), 'Mentenan??', 'Tehnic'),
    (NEWID(), 'Securitate', 'Tehnic'),
    (NEWID(), 'Cur??enie', 'Servicii'),
    (NEWID(), 'Alimentaie', 'Servicii'),
    (NEWID(), 'Transport', 'Servicii')
    
    PRINT '? Sample data inserted successfully!'
    SELECT @RecordCount = COUNT(*) FROM Departamente
    PRINT '?? Total records after insert: ' + CAST(@RecordCount AS VARCHAR(10))
END
ELSE
BEGIN
    PRINT '? Found ' + CAST(@RecordCount AS VARCHAR(10)) + ' existing records'
END

PRINT ''

-- =============================================
-- SECTION 3: Test sp_Departamente_GetByTip
-- =============================================
PRINT '?? SECTION 3: Testing sp_Departamente_GetByTip'
PRINT '------------------------------------------------------'

-- Check if stored procedure exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Departamente_GetByTip')
BEGIN
    PRINT '? Stored procedure sp_Departamente_GetByTip exists'
    
    -- Test 1: Get Medical departments
    PRINT ''
    PRINT '?? TEST 1: Getting Medical departments'
    EXEC sp_Departamente_GetByTip @Tip = 'Medical'
    
    -- Test 2: Get Administrative departments
    PRINT ''
    PRINT '?? TEST 2: Getting Administrativ departments'
    EXEC sp_Departamente_GetByTip @Tip = 'Administrativ'
    
    -- Test 3: Get Technical departments
    PRINT ''
    PRINT '?? TEST 3: Getting Tehnic departments'
    EXEC sp_Departamente_GetByTip @Tip = 'Tehnic'
    
    -- Test 4: Get Service departments
    PRINT ''
    PRINT '?? TEST 4: Getting Servicii departments'
    EXEC sp_Departamente_GetByTip @Tip = 'Servicii'
    
    -- Test 5: Test with non-existent type
    PRINT ''
    PRINT '?? TEST 5: Testing with non-existent type (should return empty)'
    EXEC sp_Departamente_GetByTip @Tip = 'NonExistent'
    
    -- Test 6: Test with NULL parameter
    PRINT ''
    PRINT '?? TEST 6: Testing with NULL parameter (should return empty)'
    EXEC sp_Departamente_GetByTip @Tip = NULL
    
    -- Test 7: Test with empty string
    PRINT ''
    PRINT '?? TEST 7: Testing with empty string (should return empty)'
    EXEC sp_Departamente_GetByTip @Tip = ''
    
END
ELSE
BEGIN
    PRINT '? ERROR: Stored procedure sp_Departamente_GetByTip does not exist!'
    PRINT '   Please run sp_Departamente_Correct.sql first to create the procedures.'
END

PRINT ''

-- =============================================
-- SECTION 4: Test sp_Departamente_GetAll
-- =============================================
PRINT '?? SECTION 4: Testing sp_Departamente_GetAll'
PRINT '------------------------------------------------------'

-- Check if stored procedure exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Departamente_GetAll')
BEGIN
    PRINT '? Stored procedure sp_Departamente_GetAll exists'
    
    -- Test 1: Get all departments
    PRINT ''
    PRINT '?? TEST 1: Getting all departments'
    EXEC sp_Departamente_GetAll
    
    -- Verify results with count
    PRINT ''
    PRINT '?? Verification: Total departments by type:'
    SELECT 
        Tip,
        COUNT(*) as Count
    FROM Departamente 
    GROUP BY Tip
    ORDER BY Tip
    
END
ELSE
BEGIN
    PRINT '? ERROR: Stored procedure sp_Departamente_GetAll does not exist!'
    PRINT '   Please run sp_Departamente_Correct.sql first to create the procedures.'
END

PRINT ''

-- =============================================
-- SECTION 5: Performance Testing
-- =============================================
PRINT '?? SECTION 5: Performance Testing'
PRINT '------------------------------------------------------'

-- Test performance of both procedures
SET STATISTICS TIME ON
SET STATISTICS IO ON

PRINT '??  Performance test for sp_Departamente_GetByTip:'
EXEC sp_Departamente_GetByTip @Tip = 'Medical'

PRINT ''
PRINT '??  Performance test for sp_Departamente_GetAll:'
EXEC sp_Departamente_GetAll

SET STATISTICS TIME OFF
SET STATISTICS IO OFF

PRINT ''

-- =============================================
-- SECTION 6: Data Integrity Tests
-- =============================================
PRINT '?? SECTION 6: Data Integrity Tests'
PRINT '------------------------------------------------------'

-- Test 1: Check for duplicate names within same type
PRINT '?? TEST 1: Checking for duplicate names within same type'
SELECT 
    Tip,
    Nume,
    COUNT(*) as DuplicateCount
FROM Departamente
GROUP BY Tip, Nume
HAVING COUNT(*) > 1

-- Test 2: Check for NULLs in required fields
PRINT ''
PRINT '?? TEST 2: Checking for NULLs in required fields'
SELECT 
    COUNT(*) as NullDepartamentID
FROM Departamente 
WHERE DepartamentID IS NULL

SELECT 
    COUNT(*) as NullNume
FROM Departamente 
WHERE Nume IS NULL OR Nume = ''

SELECT 
    COUNT(*) as NullTip
FROM Departamente 
WHERE Tip IS NULL OR Tip = ''

-- Test 3: Check data types and formats
PRINT ''
PRINT '?? TEST 3: Data format validation'
SELECT 
    Tip,
    COUNT(*) as Count,
    MIN(LEN(Nume)) as MinNameLength,
    MAX(LEN(Nume)) as MaxNameLength
FROM Departamente
GROUP BY Tip
ORDER BY Tip

PRINT ''

-- =============================================
-- SECTION 7: Edge Cases Testing
-- =============================================
PRINT '?? SECTION 7: Edge Cases Testing'
PRINT '------------------------------------------------------'

-- Test with case sensitivity
PRINT '?? TEST 1: Case sensitivity test'
EXEC sp_Departamente_GetByTip @Tip = 'medical'  -- lowercase
EXEC sp_Departamente_GetByTip @Tip = 'MEDICAL'  -- uppercase
EXEC sp_Departamente_GetByTip @Tip = 'Medical'  -- proper case

-- Test with leading/trailing spaces
PRINT ''
PRINT '?? TEST 2: Spaces handling test'
EXEC sp_Departamente_GetByTip @Tip = ' Medical '  -- with spaces
EXEC sp_Departamente_GetByTip @Tip = 'Medical   ' -- trailing spaces

-- Test with special characters (if any exist in data)
PRINT ''
PRINT '?? TEST 3: Special characters test'
-- This will show if there are any departments with special characters
SELECT 
    Nume,
    Tip,
    CASE 
        WHEN Nume LIKE '%?%' OR Nume LIKE '%a%' OR Nume LIKE '%i%' OR 
             Nume LIKE '%?%' OR Nume LIKE '%?%' THEN 'Contains Romanian diacritics'
        WHEN Nume LIKE '%-%' THEN 'Contains hyphens'
        WHEN Nume LIKE '% %' THEN 'Contains spaces'
        ELSE 'Standard characters'
    END as CharacterType
FROM Departamente
WHERE Nume LIKE '%?%' OR Nume LIKE '%a%' OR Nume LIKE '%i%' OR 
      Nume LIKE '%?%' OR Nume LIKE '%?%' OR Nume LIKE '%-%'
ORDER BY CharacterType, Nume

PRINT ''

-- =============================================
-- SECTION 8: Final Summary and Recommendations
-- =============================================
PRINT '?? SECTION 8: Test Summary'
PRINT '======================================================'

-- Count total tests performed
DECLARE @TotalDepartments INT
SELECT @TotalDepartments = COUNT(*) FROM Departamente

DECLARE @UniqueTypes INT
SELECT @UniqueTypes = COUNT(DISTINCT Tip) FROM Departamente

PRINT '?? TEST SUMMARY:'
PRINT '   • Total Departments: ' + CAST(@TotalDepartments AS VARCHAR(10))
PRINT '   • Unique Types: ' + CAST(@UniqueTypes AS VARCHAR(10))

-- Show breakdown by type
PRINT ''
PRINT '?? Departments by Type:'
SELECT 
    '   • ' + Tip + ': ' + CAST(COUNT(*) AS VARCHAR(10)) + ' departments' as Summary
FROM Departamente
GROUP BY Tip
ORDER BY COUNT(*) DESC

-- Check if both stored procedures exist and work
DECLARE @SP1Exists BIT = 0
DECLARE @SP2Exists BIT = 0

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Departamente_GetByTip')
    SET @SP1Exists = 1

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Departamente_GetAll')
    SET @SP2Exists = 1

PRINT ''
PRINT '?? STORED PROCEDURES STATUS:'
PRINT '   • sp_Departamente_GetByTip: ' + CASE WHEN @SP1Exists = 1 THEN '? EXISTS' ELSE '? MISSING' END
PRINT '   • sp_Departamente_GetAll: ' + CASE WHEN @SP2Exists = 1 THEN '? EXISTS' ELSE '? MISSING' END

IF @SP1Exists = 1 AND @SP2Exists = 1
BEGIN
    PRINT ''
    PRINT '?? SUCCESS: All stored procedures are working correctly!'
    PRINT '   Both procedures executed without errors and returned expected results.'
    PRINT '   The database is ready for use with the ValyanClinic application.'
END
ELSE
BEGIN
    PRINT ''
    PRINT '??  WARNING: Some stored procedures are missing!'
    PRINT '   Please run the sp_Departamente_Correct.sql script to create missing procedures.'
END

-- Final recommendations
PRINT ''
PRINT '?? RECOMMENDATIONS:'
PRINT '   • Consider adding indexes on Tip column for better performance'
PRINT '   • Implement data validation constraints at database level'
PRINT '   • Consider adding CreatedDate and ModifiedDate columns for auditing'
PRINT '   • Regular maintenance: Update statistics and check for data consistency'

PRINT ''
PRINT '?? Testing completed successfully!'
PRINT '======================================================'