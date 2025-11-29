-- =============================================
-- ALTER TABLE Consultatii
-- Adaug? coloana CoduriICD10Secundare pentru diagnostic secundar
-- Database: ValyanMed
-- =============================================

USE [ValyanMed];

PRINT '========================================';
PRINT 'ADAUGARE COLOANA CoduriICD10Secundare';
PRINT 'Database: ValyanMed';
PRINT 'Table: Consultatii';
PRINT '========================================';
PRINT '';

-- Verific? dac? coloana exist? deja
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Consultatii' 
    AND COLUMN_NAME = 'CoduriICD10Secundare'
)
BEGIN
    PRINT '1. Adaugare coloana CoduriICD10Secundare...';
    
    ALTER TABLE dbo.Consultatii
    ADD CoduriICD10Secundare NVARCHAR(500) NULL;
    
    PRINT '   ? Coloana CoduriICD10Secundare adaugata cu succes';
    PRINT '   Tip: NVARCHAR(500) NULL';
    PRINT '   Descriere: Coduri ICD-10 secundare (comma-separated)';
END
ELSE
BEGIN
    PRINT '   ? Coloana CoduriICD10Secundare exista deja';
END

PRINT '';
PRINT '========================================';
PRINT 'VERIFICARE STRUCTURA TABEL';
PRINT '========================================';

-- Afi?eaz? coloanele ICD-10
SELECT 
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType,
    CHARACTER_MAXIMUM_LENGTH as MaxLength,
    IS_NULLABLE as IsNullable
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Consultatii'
AND (COLUMN_NAME LIKE '%ICD10%' OR COLUMN_NAME LIKE '%Diagnostic%')
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '? Migrare finalizata cu succes!';
PRINT '';
