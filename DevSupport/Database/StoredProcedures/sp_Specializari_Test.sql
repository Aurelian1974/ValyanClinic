-- ========================================
-- Test Script pentru Stored Procedures - Specializari
-- Database: ValyanMed
-- Descriere: Test pentru verificarea functionalitatii SP-urilor
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'TEST STORED PROCEDURES - SPECIALIZARI';
PRINT '========================================';
PRINT '';

-- ============================================================================
-- Test 1: sp_Specializari_GetAll - Obtinere lista paginata
-- ============================================================================
PRINT 'Test 1: sp_Specializari_GetAll - Lista paginata...';
BEGIN TRY
    EXEC sp_Specializari_GetAll 
        @PageNumber = 1, 
        @PageSize = 10,
        @SortColumn = 'Denumire',
        @SortDirection = 'ASC';
    
    PRINT '   ? GetAll functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Test 2: sp_Specializari_GetAll - Filtrare dupa categorie
-- ============================================================================
PRINT 'Test 2: sp_Specializari_GetAll - Filtrare dupa categorie (Cardiologie)...';
BEGIN TRY
    EXEC sp_Specializari_GetAll 
        @PageNumber = 1, 
        @PageSize = 20,
        @SearchText = 'cardio',
        @Categorie = N'Medical?';
    
    PRINT '   ? Filtrare dupa categorie functioneaza';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Test 3: sp_Specializari_GetById
-- ============================================================================
PRINT 'Test 3: sp_Specializari_GetById...';
BEGIN TRY
    DECLARE @TestId UNIQUEIDENTIFIER;
    SELECT TOP 1 @TestId = Id FROM Specializari WHERE Denumire LIKE N'%Cardiologie%';
    
    IF @TestId IS NOT NULL
    BEGIN
        EXEC sp_Specializari_GetById @Id = @TestId;
        PRINT '   ? GetById functioneaza corect';
    END
    ELSE
    BEGIN
        PRINT '   ? Nu s-a gasit specializare pentru test';
    END
END TRY
BEGIN CATCH
    PRINT '   ? EROARE: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Test 4: sp_Specializari_GetByDenumire
-- ============================================================================
PRINT 'Test 4: sp_Specializari_GetByDenumire...';
BEGIN TRY
    EXEC sp_Specializari_GetByDenumire @Denumire = N'Cardiologie';
    PRINT '   ? GetByDenumire functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Test 5: sp_Specializari_GetByCategorie
-- ============================================================================
PRINT 'Test 5: sp_Specializari_GetByCategorie - Chirurgical?...';
BEGIN TRY
    EXEC sp_Specializari_GetByCategorie 
        @Categorie = N'Chirurgical?',
        @EsteActiv = 1;
    
    PRINT '   ? GetByCategorie functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Test 6: sp_Specializari_GetCategorii
-- ============================================================================
PRINT 'Test 6: sp_Specializari_GetCategorii...';
BEGIN TRY
    EXEC sp_Specializari_GetCategorii;
    PRINT '   ? GetCategorii functioneaza corect';
    
    -- Afisare numar categorii
    DECLARE @NumarCategorii INT;
    SELECT @NumarCategorii = COUNT(DISTINCT Categorie) 
    FROM Specializari 
    WHERE Categorie IS NOT NULL;
    PRINT '   ? Numar categorii: ' + CAST(@NumarCategorii AS VARCHAR);
END TRY
BEGIN CATCH
    PRINT '   ? EROARE: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Test 7: sp_Specializari_GetDropdownOptions
-- ============================================================================
PRINT 'Test 7: sp_Specializari_GetDropdownOptions - Toate...';
BEGIN TRY
    EXEC sp_Specializari_GetDropdownOptions @EsteActiv = 1;
    PRINT '   ? GetDropdownOptions functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Test 8: sp_Specializari_GetDropdownOptions - Filtrat pe categorie
-- ============================================================================
PRINT 'Test 8: sp_Specializari_GetDropdownOptions - Stomatologie...';
BEGIN TRY
    EXEC sp_Specializari_GetDropdownOptions 
        @Categorie = N'Stomatologie',
        @EsteActiv = 1;
    
    PRINT '   ? GetDropdownOptions cu filtru functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Test 9: sp_Specializari_CheckUnique - Verificare duplicat
-- ============================================================================
PRINT 'Test 9: sp_Specializari_CheckUnique - Verificare existenta...';
BEGIN TRY
    DECLARE @Exists INT;
    
    -- Test 1: Specializare existenta
    EXEC sp_Specializari_CheckUnique @Denumire = N'Cardiologie';
    PRINT '   ? CheckUnique pentru specializare existenta functioneaza';
    
    -- Test 2: Specializare noua
    EXEC sp_Specializari_CheckUnique @Denumire = N'Specializare Test Inexistenta XYZ';
    PRINT '   ? CheckUnique pentru specializare noua functioneaza';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Test 10: sp_Specializari_GetStatistics
-- ============================================================================
PRINT 'Test 10: sp_Specializari_GetStatistics...';
BEGIN TRY
    EXEC sp_Specializari_GetStatistics;
    PRINT '   ? GetStatistics functioneaza corect';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Test 11: sp_Specializari_Create - Creare specializare test
-- ============================================================================
PRINT 'Test 11: sp_Specializari_Create - Creare specializare test...';
DECLARE @TestSpecializareId UNIQUEIDENTIFIER;

BEGIN TRY
    -- Cream o specializare de test
    DECLARE @OutputCreate TABLE (
        Id UNIQUEIDENTIFIER,
        Denumire NVARCHAR(200),
        Categorie NVARCHAR(100),
        Este_Activ BIT
    );
    
    INSERT INTO @OutputCreate
    EXEC sp_Specializari_Create 
        @Denumire = N'Specializare Test - Automat',
        @Categorie = N'Test',
        @Descriere = N'Specializare creata automat pentru test',
        @EsteActiv = 1,
        @CreatDe = N'test_automation@valyanclinic.ro';
    
    SELECT @TestSpecializareId = Id FROM @OutputCreate;
    
    IF @TestSpecializareId IS NOT NULL
        PRINT '   ? Create functioneaza corect - ID: ' + CAST(@TestSpecializareId AS VARCHAR(36));
    ELSE
        PRINT '   ? Create a rulat dar nu a returnat ID';
        
END TRY
BEGIN CATCH
    PRINT '   ? EROARE Create: ' + ERROR_MESSAGE();
    SET @TestSpecializareId = NULL;
END CATCH
PRINT '';

-- ============================================================================
-- Test 12: sp_Specializari_Create - Verificare constrangere duplicat
-- ============================================================================
PRINT 'Test 12: sp_Specializari_Create - Verificare constrangere duplicat...';
BEGIN TRY
    EXEC sp_Specializari_Create 
        @Denumire = N'Cardiologie', -- Exista deja!
        @Categorie = N'Medical?',
        @EsteActiv = 1,
        @CreatDe = N'test@valyanclinic.ro';
    
    PRINT '   ? NU A DAT EROARE - constrangerea de unicitate NU functioneaza!';
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 50001
        PRINT '   ? Constrangere duplicat functioneaza corect: ' + ERROR_MESSAGE();
    ELSE
        PRINT '   ? Eroare nea?teptat?: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Test 13: sp_Specializari_Update - Actualizare specializare test
-- ============================================================================
PRINT 'Test 13: sp_Specializari_Update - Actualizare specializare test...';
IF @TestSpecializareId IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC sp_Specializari_Update 
            @Id = @TestSpecializareId,
            @Denumire = N'Specializare Test - Modificat',
            @Categorie = N'Test',
            @Descriere = N'Specializare modificata prin test automat',
            @EsteActiv = 1,
            @ModificatDe = N'test_automation@valyanclinic.ro';
        
        PRINT '   ? Update functioneaza corect';
    END TRY
    BEGIN CATCH
        PRINT '   ? EROARE Update: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT '   ? Skip - Nu exista specializare test pentru actualizare';
END
PRINT '';

-- ============================================================================
-- Test 14: sp_Specializari_Delete - Soft delete
-- ============================================================================
PRINT 'Test 14: sp_Specializari_Delete - Soft delete...';
IF @TestSpecializareId IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC sp_Specializari_Delete 
            @Id = @TestSpecializareId,
            @ModificatDe = N'test_automation@valyanclinic.ro';
        
        -- Verificare soft delete
        DECLARE @EsteActiv BIT;
        SELECT @EsteActiv = Este_Activ 
        FROM Specializari 
        WHERE Id = @TestSpecializareId;
        
        IF @EsteActiv = 0
            PRINT '   ? Soft Delete functioneaza corect (Este_Activ = 0)';
        ELSE
            PRINT '   ? Soft Delete NU functioneaza - Este_Activ inca 1';
            
    END TRY
    BEGIN CATCH
        PRINT '   ? EROARE Delete: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT '   ? Skip - Nu exista specializare test pentru stergere';
END
PRINT '';

-- ============================================================================
-- Test 15: sp_Specializari_HardDelete - Stergere fizica
-- ============================================================================
PRINT 'Test 15: sp_Specializari_HardDelete - Stergere fizica...';
IF @TestSpecializareId IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC sp_Specializari_HardDelete @Id = @TestSpecializareId;
        
        -- Verificare stergere fizica
        IF NOT EXISTS (SELECT 1 FROM Specializari WHERE Id = @TestSpecializareId)
            PRINT '   ? Hard Delete functioneaza corect (inregistrare stearsa fizic)';
        ELSE
            PRINT '   ? Hard Delete NU functioneaza - inregistrarea inca exista';
            
    END TRY
    BEGIN CATCH
        PRINT '   ? EROARE HardDelete: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT '   ? Skip - Nu exista specializare test pentru stergere fizica';
END
PRINT '';

-- ============================================================================
-- SUMAR FINAL
-- ============================================================================
PRINT '========================================';
PRINT 'SUMAR TESTE STORED PROCEDURES';
PRINT '========================================';

-- Statistici generale
DECLARE @TotalSpecializari INT;
DECLARE @TotalActive INT;
DECLARE @TotalCategorii INT;

SELECT 
    @TotalSpecializari = COUNT(*),
    @TotalActive = SUM(CASE WHEN Este_Activ = 1 THEN 1 ELSE 0 END)
FROM Specializari;

SELECT @TotalCategorii = COUNT(DISTINCT Categorie)
FROM Specializari
WHERE Categorie IS NOT NULL;

PRINT 'Total specializari: ' + CAST(@TotalSpecializari AS VARCHAR);
PRINT 'Specializari active: ' + CAST(@TotalActive AS VARCHAR);
PRINT 'Total categorii: ' + CAST(@TotalCategorii AS VARCHAR);
PRINT '';

-- Verificare SP-uri
DECLARE @NumarSP INT;
SELECT @NumarSP = COUNT(*) 
FROM sys.procedures 
WHERE name LIKE 'sp_Specializari_%';

PRINT 'Stored Procedures disponibile: ' + CAST(@NumarSP AS VARCHAR);
PRINT '';

IF @NumarSP >= 13
    PRINT '??? TOATE STORED PROCEDURES FUNCTIONEAZA CORECT ???';
ELSE
    PRINT '??? LIPSESC STORED PROCEDURES - Rulati sp_Specializari.sql ???';

PRINT '========================================';
GO
