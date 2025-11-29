-- =============================================
-- VERIFICARE DATE ICD-10 în Database
-- =============================================

USE [ValyanMed];

PRINT '========================================';
PRINT 'VERIFICARE TABELA ICD10';
PRINT '========================================';
PRINT '';

-- 1. Total coduri
SELECT 'Total Coduri ICD-10' as Check_Type, COUNT(*) as Count
FROM dbo.ICD10;

PRINT '';

-- 2. Coduri COMUNE (IsCommon = 1)
SELECT 'Coduri Comune (Favorite)' as Check_Type, COUNT(*) as Count
FROM dbo.ICD10
WHERE IsCommon = 1;

PRINT '';

-- 3. Coduri LEAF NODES (f?r? copii)
SELECT 'Coduri Leaf Nodes' as Check_Type, COUNT(*) as Count
FROM dbo.ICD10
WHERE IsLeafNode = 1;

PRINT '';

-- 4. Sample coduri pentru verificare
SELECT TOP 10 
    Code,
    ShortDescription,
    Category,
    IsCommon,
    IsLeafNode,
    Severity
FROM dbo.ICD10
ORDER BY IsCommon DESC, Code;

PRINT '';
PRINT '========================================';
PRINT 'VERIFICARE FINALIZAT?';
PRINT '========================================';
