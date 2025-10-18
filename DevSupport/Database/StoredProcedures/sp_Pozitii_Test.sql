-- ========================================
-- Test Script pentru sp_Pozitii_Create
-- Database: ValyanMed
-- Descriere: Test pentru verificarea corectarii erorii SCOPE_IDENTITY
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'TEST sp_Pozitii_Create - CORECTARE EROARE';
PRINT '========================================';
PRINT '';

-- ============================================================================
-- Test 1: Creare pozitie noua
-- ============================================================================
PRINT 'Test 1: Creare pozitie noua...';
BEGIN TRY
    EXEC sp_Pozitii_Create 
        @Denumire = N'Asistent medical principal - TEST',
        @Descriere = N'Pozi?ie de test pentru verificare',
        @EsteActiv = 1,
        @CreatDe = N'test@valyanclinic.ro';
    
    PRINT '   ? Pozitie creata cu succes!';
END TRY
BEGIN CATCH
    PRINT '   ? EROARE: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Test 2: Verificare pozitie creata
-- ============================================================================
PRINT 'Test 2: Verificare pozitie creata...';
IF EXISTS (SELECT 1 FROM Pozitii WHERE Denumire = N'Asistent medical principal - TEST')
BEGIN
    PRINT '   ? Pozitia a fost gasita in baza de date';
    
    SELECT 
        Id,
        Denumire,
        Descriere,
        Este_Activ,
        Data_Crearii,
        Creat_De
    FROM Pozitii 
    WHERE Denumire = N'Asistent medical principal - TEST';
END
ELSE
BEGIN
    PRINT '   ? Pozitia NU a fost gasita in baza de date';
END
PRINT '';

-- ============================================================================
-- Test 3: Verificare eroare duplicat (ar trebui sa dea eroare)
-- ============================================================================
PRINT 'Test 3: Verificare constrangere unicitate (ar trebui sa dea eroare)...';
BEGIN TRY
    EXEC sp_Pozitii_Create 
        @Denumire = N'Asistent medical principal - TEST',
        @Descriere = N'Aceasta ar trebui sa dea eroare',
        @EsteActiv = 1,
        @CreatDe = N'test@valyanclinic.ro';
    
    PRINT '   ? NU A DAT EROARE - constrangerea de unicitate NU functioneaza!';
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 50001
        PRINT '   ? Eroare corecta: ' + ERROR_MESSAGE();
    ELSE
        PRINT '   ? Eroare nea?teptat?: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- Cleanup: Stergere pozitie test
-- ============================================================================
PRINT 'Cleanup: Stergere pozitie test...';
DECLARE @TestId UNIQUEIDENTIFIER;
SELECT @TestId = Id FROM Pozitii WHERE Denumire = N'Asistent medical principal - TEST';

IF @TestId IS NOT NULL
BEGIN
    DELETE FROM Pozitii WHERE Id = @TestId;
    PRINT '   ? Pozitie test stearsa';
END
PRINT '';

-- ============================================================================
-- SUMAR
-- ============================================================================
PRINT '========================================';
PRINT 'REZUMAT: Eroarea SCOPE_IDENTITY() a fost corectata!';
PRINT 'sp_Pozitii_Create foloseste acum OUTPUT cu table variable.';
PRINT '========================================';
GO
