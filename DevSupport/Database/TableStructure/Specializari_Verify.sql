-- ========================================
-- Script de Verificare - Tabela Specializari
-- Database: ValyanMed
-- Descriere: Script pentru testarea si verificarea instalarii corecte
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'VERIFICARE INSTALARE TABELA SPECIALIZARI';
PRINT '========================================';
PRINT '';

-- ============================================================================
-- 1. Verificare existenta tabel
-- ============================================================================
PRINT '1. Verificare existenta tabel Specializari...';
IF OBJECT_ID('dbo.Specializari', 'U') IS NOT NULL
BEGIN
    PRINT '   ? Tabelul Specializari EXISTA';
    
    -- Afisare structura tabel
    SELECT 
        COLUMN_NAME as 'Coloana',
        DATA_TYPE as 'Tip Date',
        CHARACTER_MAXIMUM_LENGTH as 'Lungime Max',
        IS_NULLABLE as 'NULL?',
        COLUMN_DEFAULT as 'Valoare Default'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Specializari'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT '   ? EROARE: Tabelul Specializari NU EXISTA!';
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
WHERE parent_object_id = OBJECT_ID('dbo.Specializari')
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
WHERE i.object_id = OBJECT_ID('dbo.Specializari')
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
WHERE parent_id = OBJECT_ID('dbo.Specializari')
ORDER BY name;
PRINT '';

-- ============================================================================
-- 5. Verificare date populare
-- ============================================================================
PRINT '5. Verificare date populare...';
DECLARE @NumarSpecializari INT;
SELECT @NumarSpecializari = COUNT(*) FROM Specializari;

IF @NumarSpecializari >= 66
BEGIN
    PRINT '   ? Date populare: ' + CAST(@NumarSpecializari AS VARCHAR) + ' specializari gasite';
    
    -- Statistici pe categorii
    PRINT '   Detalii pe categorii:';
    SELECT 
        Categorie,
        COUNT(*) AS Numar,
        SUM(CASE WHEN Este_Activ = 1 THEN 1 ELSE 0 END) AS Active
    FROM Specializari
    GROUP BY Categorie
    ORDER BY Categorie;
END
ELSE
BEGIN
    PRINT '   ? ATENTIE: Doar ' + CAST(@NumarSpecializari AS VARCHAR) + ' specializari gasite (se astepta 66)';
END
PRINT '';

-- ============================================================================
-- 6. Verificare stored procedures
-- ============================================================================
PRINT '6. Verificare stored procedures...';
DECLARE @NumarSP INT;
SELECT @NumarSP = COUNT(*) 
FROM sys.procedures 
WHERE name LIKE 'sp_Specializari_%';

IF @NumarSP >= 4
BEGIN
    PRINT '   ? Stored Procedures: ' + CAST(@NumarSP AS VARCHAR) + ' SP-uri gasite';
    
    SELECT 
        name as 'Stored Procedure',
        create_date as 'Data Creare'
    FROM sys.procedures 
    WHERE name LIKE 'sp_Specializari_%'
    ORDER BY name;
END
ELSE
BEGIN
    PRINT '   ? ATENTIE: Doar ' + CAST(@NumarSP AS VARCHAR) + ' SP-uri gasite (se astepta minim 4)';
    PRINT '   ? Pentru toate SP-urile, rulati: sp_Specializari.sql';
END
PRINT '';

-- ============================================================================
-- 7. Test functional - GetAll
-- ============================================================================
PRINT '7. Test functional - sp_Specializari_GetAll...';
BEGIN TRY
    EXEC sp_Specializari_GetAll @PageNumber = 1, @PageSize = 5;
    PRINT '   ? sp_Specializari_GetAll functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE sp_Specializari_GetAll: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- 8. Test functional - GetById (folosind prima inregistrare)
-- ============================================================================
PRINT '8. Test functional - sp_Specializari_GetById...';
BEGIN TRY
    DECLARE @TestId UNIQUEIDENTIFIER;
    SELECT TOP 1 @TestId = Id FROM Specializari;
    
    IF @TestId IS NOT NULL
    BEGIN
        EXEC sp_Specializari_GetById @Id = @TestId;
        PRINT '   ? sp_Specializari_GetById functioneaza corect';
    END
    ELSE
    BEGIN
        PRINT '   ? Nu exista date pentru test';
    END
END TRY
BEGIN CATCH
    PRINT '   ? EROARE sp_Specializari_GetById: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- 9. Test functional - GetDropdownOptions
-- ============================================================================
PRINT '9. Test functional - sp_Specializari_GetDropdownOptions...';
BEGIN TRY
    EXEC sp_Specializari_GetDropdownOptions @EsteActiv = 1;
    PRINT '   ? sp_Specializari_GetDropdownOptions functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE sp_Specializari_GetDropdownOptions: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- 10. Test functional - GetCategorii
-- ============================================================================
PRINT '10. Test functional - sp_Specializari_GetCategorii...';
BEGIN TRY
    EXEC sp_Specializari_GetCategorii;
    PRINT '   ? sp_Specializari_GetCategorii functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE sp_Specializari_GetCategorii: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- 11. Verificare integritate date
-- ============================================================================
PRINT '11. Verificare integritate date...';

-- Verificare duplicate
DECLARE @Duplicate INT;
SELECT @Duplicate = COUNT(*) - COUNT(DISTINCT Denumire) FROM Specializari;
IF @Duplicate = 0
    PRINT '   ? Nu exista duplicate';
ELSE
    PRINT '   ? ATENTIE: ' + CAST(@Duplicate AS VARCHAR) + ' duplicate gasite!';

-- Verificare categorii null
DECLARE @NullCategories INT;
SELECT @NullCategories = COUNT(*) FROM Specializari WHERE Categorie IS NULL;
IF @NullCategories = 0
    PRINT '   ? Toate specializarile au categorie';
ELSE
    PRINT '   ? ' + CAST(@NullCategories AS VARCHAR) + ' specializari fara categorie';

-- Verificare specializari active
DECLARE @Active INT, @Inactive INT;
SELECT 
    @Active = SUM(CASE WHEN Este_Activ = 1 THEN 1 ELSE 0 END),
    @Inactive = SUM(CASE WHEN Este_Activ = 0 THEN 1 ELSE 0 END)
FROM Specializari;
PRINT '   ? Specializari active: ' + CAST(@Active AS VARCHAR);
PRINT '   ? Specializari inactive: ' + CAST(@Inactive AS VARCHAR);

PRINT '';

-- ============================================================================
-- 12. Sample Data - Afisare exemple
-- ============================================================================
PRINT '12. Sample data - Primele 5 specializari pe fiecare categorie...';
PRINT '';

;WITH RankedSpecializari AS (
    SELECT 
        Denumire,
        Categorie,
        Este_Activ,
        ROW_NUMBER() OVER (PARTITION BY Categorie ORDER BY Denumire) AS RowNum
    FROM Specializari
)
SELECT 
    Denumire,
    Categorie,
    Este_Activ as 'Activ?'
FROM RankedSpecializari
WHERE RowNum <= 5
ORDER BY Categorie, Denumire;

PRINT '';

-- ============================================================================
-- SUMAR FINAL
-- ============================================================================
PRINT '========================================';
PRINT 'SUMAR VERIFICARE';
PRINT '========================================';

DECLARE @TabelExista BIT = CASE WHEN OBJECT_ID('dbo.Specializari', 'U') IS NOT NULL THEN 1 ELSE 0 END;
DECLARE @DatePopulate BIT = CASE WHEN EXISTS(SELECT 1 FROM Specializari) THEN 1 ELSE 0 END;
DECLARE @SPExista BIT = CASE WHEN EXISTS(SELECT 1 FROM sys.procedures WHERE name LIKE 'sp_Specializari_%') THEN 1 ELSE 0 END;
DECLARE @DateCorecte BIT = CASE WHEN (SELECT COUNT(*) FROM Specializari) >= 66 THEN 1 ELSE 0 END;

PRINT 'Tabel creat: ' + CASE WHEN @TabelExista = 1 THEN '? DA' ELSE '? NU' END;
PRINT 'Date populate: ' + CASE WHEN @DatePopulate = 1 THEN '? DA' ELSE '? NU' END;
PRINT 'Date complete (66+): ' + CASE WHEN @DateCorecte = 1 THEN '? DA' ELSE '? NU' END;
PRINT 'Stored Procedures: ' + CASE WHEN @SPExista = 1 THEN '? DA' ELSE '? NU' END;
PRINT '';

IF @TabelExista = 1 AND @DatePopulate = 1 AND @SPExista = 1 AND @DateCorecte = 1
BEGIN
    PRINT '??? INSTALARE COMPLETA SI FUNCTIONALA ???';
    PRINT '';
    PRINT 'Total specializari: ' + CAST((SELECT COUNT(*) FROM Specializari) AS VARCHAR);
    PRINT 'Categorii disponibile: ' + CAST((SELECT COUNT(DISTINCT Categorie) FROM Specializari WHERE Categorie IS NOT NULL) AS VARCHAR);
END
ELSE
BEGIN
    PRINT '??? INSTALARE INCOMPLETA - Verificati erorile de mai sus ???';
END

PRINT '========================================';
GO
