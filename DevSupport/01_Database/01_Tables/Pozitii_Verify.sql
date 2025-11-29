-- ========================================
-- Script de Verificare - Tabela Pozitii
-- Database: ValyanMed
-- Descriere: Script pentru testarea si verificarea instalarii corecte
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'VERIFICARE INSTALARE TABELA POZITII';
PRINT '========================================';
PRINT '';

-- ============================================================================
-- 1. Verificare existenta tabel
-- ============================================================================
PRINT '1. Verificare existenta tabel Pozitii...';
IF OBJECT_ID('dbo.Pozitii', 'U') IS NOT NULL
BEGIN
    PRINT '   ? Tabelul Pozitii EXISTA';
    
    -- Afisare structura tabel
    SELECT 
        COLUMN_NAME as 'Coloana',
        DATA_TYPE as 'Tip Date',
        CHARACTER_MAXIMUM_LENGTH as 'Lungime Max',
        IS_NULLABLE as 'NULL?',
        COLUMN_DEFAULT as 'Valoare Default'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Pozitii'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT '   ? EROARE: Tabelul Pozitii NU EXISTA!';
END
PRINT '';

-- ============================================================================
-- 2. Verificare constrangeri
-- ============================================================================
PRINT '2. Verificare constrangeri...';
SELECT 
    name as 'Constrangere',
    type_desc as 'Tip',
    CASE 
        WHEN type = 'PK' THEN 'Primary Key'
        WHEN type = 'UQ' THEN 'Unique'
        WHEN type = 'C' THEN 'Check'
        WHEN type = 'D' THEN 'Default'
        ELSE type_desc
    END as 'Descriere'
FROM sys.objects
WHERE parent_object_id = OBJECT_ID('dbo.Pozitii')
    AND type IN ('PK', 'UQ', 'C', 'D')
ORDER BY type, name;
PRINT '';

-- ============================================================================
-- 3. Verificare indexuri
-- ============================================================================
PRINT '3. Verificare indexuri...';
SELECT 
    i.name as 'Index',
    i.type_desc as 'Tip',
    COL_NAME(ic.object_id, ic.column_id) as 'Coloana',
    i.is_unique as 'Unic?'
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('dbo.Pozitii')
    AND i.type > 0
ORDER BY i.name, ic.key_ordinal;
PRINT '';

-- ============================================================================
-- 4. Verificare trigger-e
-- ============================================================================
PRINT '4. Verificare trigger-e...';
SELECT 
    name as 'Trigger',
    is_disabled as 'Dezactivat?',
    create_date as 'Data Creare',
    modify_date as 'Data Modificare'
FROM sys.triggers
WHERE parent_id = OBJECT_ID('dbo.Pozitii')
ORDER BY name;
PRINT '';

-- ============================================================================
-- 5. Verificare date populare
-- ============================================================================
PRINT '5. Verificare date populare...';
DECLARE @NumarPozitii INT;
SELECT @NumarPozitii = COUNT(*) FROM Pozitii;

IF @NumarPozitii >= 20
BEGIN
    PRINT '   ? Date populare: ' + CAST(@NumarPozitii AS VARCHAR) + ' pozitii gasite';
    
    -- Afisare primele 5 pozitii
    PRINT '   Primele 5 pozitii:';
    SELECT TOP 5
        Id,
        Denumire,
        Este_Activ as 'Activ?',
        Data_Crearii
    FROM Pozitii
    ORDER BY Denumire;
END
ELSE
BEGIN
    PRINT '   ? ATENTIE: Doar ' + CAST(@NumarPozitii AS VARCHAR) + ' pozitii gasite (se astepta 20)';
END
PRINT '';

-- ============================================================================
-- 6. Verificare stored procedures
-- ============================================================================
PRINT '6. Verificare stored procedures...';
DECLARE @NumarSP INT;
SELECT @NumarSP = COUNT(*) 
FROM sys.procedures 
WHERE name LIKE 'sp_Pozitii_%';

IF @NumarSP >= 11
BEGIN
    PRINT '   ? Stored Procedures: ' + CAST(@NumarSP AS VARCHAR) + ' SP-uri gasite';
    
    SELECT 
        name as 'Stored Procedure',
        create_date as 'Data Creare'
    FROM sys.procedures 
    WHERE name LIKE 'sp_Pozitii_%'
    ORDER BY name;
END
ELSE
BEGIN
    PRINT '   ? ATENTIE: Doar ' + CAST(@NumarSP AS VARCHAR) + ' SP-uri gasite (se astepta 11)';
END
PRINT '';

-- ============================================================================
-- 7. Test functional - GetAll
-- ============================================================================
PRINT '7. Test functional - sp_Pozitii_GetAll...';
BEGIN TRY
    EXEC sp_Pozitii_GetAll @PageNumber = 1, @PageSize = 5;
    PRINT '   ? sp_Pozitii_GetAll functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE sp_Pozitii_GetAll: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- 8. Test functional - GetCount
-- ============================================================================
PRINT '8. Test functional - sp_Pozitii_GetCount...';
BEGIN TRY
    EXEC sp_Pozitii_GetCount;
    PRINT '   ? sp_Pozitii_GetCount functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE sp_Pozitii_GetCount: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- 9. Test functional - GetDropdownOptions
-- ============================================================================
PRINT '9. Test functional - sp_Pozitii_GetDropdownOptions...';
BEGIN TRY
    EXEC sp_Pozitii_GetDropdownOptions @EsteActiv = 1;
    PRINT '   ? sp_Pozitii_GetDropdownOptions functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE sp_Pozitii_GetDropdownOptions: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- 10. Test functional - CheckUnique
-- ============================================================================
PRINT '10. Test functional - sp_Pozitii_CheckUnique...';
BEGIN TRY
    EXEC sp_Pozitii_CheckUnique @Denumire = N'Medic primar';
    PRINT '   ? sp_Pozitii_CheckUnique functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE sp_Pozitii_CheckUnique: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- 11. Test functional - GetStatistics
-- ============================================================================
PRINT '11. Test functional - sp_Pozitii_GetStatistics...';
BEGIN TRY
    EXEC sp_Pozitii_GetStatistics;
    PRINT '   ? sp_Pozitii_GetStatistics functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE sp_Pozitii_GetStatistics: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- SUMAR FINAL
-- ============================================================================
PRINT '========================================';
PRINT 'SUMAR VERIFICARE';
PRINT '========================================';

DECLARE @TabelExista BIT = CASE WHEN OBJECT_ID('dbo.Pozitii', 'U') IS NOT NULL THEN 1 ELSE 0 END;
DECLARE @DatePopulate BIT = CASE WHEN EXISTS(SELECT 1 FROM Pozitii) THEN 1 ELSE 0 END;
DECLARE @SPExista BIT = CASE WHEN EXISTS(SELECT 1 FROM sys.procedures WHERE name LIKE 'sp_Pozitii_%') THEN 1 ELSE 0 END;

PRINT 'Tabel creat: ' + CASE WHEN @TabelExista = 1 THEN '? DA' ELSE '? NU' END;
PRINT 'Date populate: ' + CASE WHEN @DatePopulate = 1 THEN '? DA' ELSE '? NU' END;
PRINT 'Stored Procedures: ' + CASE WHEN @SPExista = 1 THEN '? DA' ELSE '? NU' END;
PRINT '';

IF @TabelExista = 1 AND @DatePopulate = 1 AND @SPExista = 1
BEGIN
    PRINT '??? INSTALARE COMPLETA SI FUNCTIONALA ???';
END
ELSE
BEGIN
    PRINT '??? INSTALARE INCOMPLETA - Verificati erorile de mai sus ???';
END

PRINT '========================================';
GO
