-- ========================================
-- Test Stored Procedures pentru Pacienti
-- Database: ValyanMed
-- Descriere: Script pentru testarea procedurilor stocate Pacienti
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================'
PRINT 'TEST STORED PROCEDURES PACIENTI'
PRINT '========================================'
PRINT ''

-- Variabile pentru testare
DECLARE @TestId UNIQUEIDENTIFIER
DECLARE @TestCodPacient NVARCHAR(20)
DECLARE @TestCNP NVARCHAR(13) = '1800515123456'

-- ============================================================================
-- TEST 1: sp_Pacienti_GenerateNextCodPacient
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 1: sp_Pacienti_GenerateNextCodPacient'
PRINT '----------------------------------------'

BEGIN TRY
    EXEC sp_Pacienti_GenerateNextCodPacient
    PRINT '? Procedura executata cu succes'
END TRY
BEGIN CATCH
    PRINT '? EROARE: ' + ERROR_MESSAGE()
END CATCH
PRINT ''

-- ============================================================================
-- TEST 2: sp_Pacienti_Create
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 2: sp_Pacienti_Create'
PRINT '----------------------------------------'

BEGIN TRY
    -- Creare pacient de test
    CREATE TABLE #TempPacient (
        Id UNIQUEIDENTIFIER,
        Cod_Pacient NVARCHAR(20),
        CNP NVARCHAR(13),
        Nume NVARCHAR(100),
        Prenume NVARCHAR(100)
    )
    
    INSERT INTO #TempPacient
    EXEC sp_Pacienti_Create
        @Nume = 'Test',
        @Prenume = 'Pacient',
        @Data_Nasterii = '1980-05-15',
        @Sex = 'M',
        @CNP = @TestCNP,
        @Telefon = '0721234567',
        @Email = 'test.pacient@email.com',
        @Judet = 'Bucuresti',
        @Localitate = 'Bucuresti',
        @Adresa = 'Str. Test nr. 123',
        @Cod_Postal = '010101',
        @Asigurat = 1,
        @Casa_Asigurari = 'CNAS',
        @Nr_Card_Sanatate = '123456789012',
        @Alergii = 'Penicilina',
        @Boli_Cronice = 'Hipertensiune',
        @Medic_Familie = 'Dr. Pop Ion',
        @Persoana_Contact = 'Maria Popescu',
        @Telefon_Urgenta = '0722345678',
        @Relatie_Contact = 'Sotie',
        @Observatii = 'Pacient de test',
        @CreatDe = 'TestScript'
    
    SELECT @TestId = Id, @TestCodPacient = Cod_Pacient FROM #TempPacient
    
    PRINT '? Pacient creat cu succes'
    PRINT '  ID: ' + CAST(@TestId AS NVARCHAR(36))
    PRINT '  Cod Pacient: ' + @TestCodPacient
    PRINT '  CNP: ' + @TestCNP
    
    DROP TABLE #TempPacient
END TRY
BEGIN CATCH
    PRINT '? EROARE: ' + ERROR_MESSAGE()
    IF OBJECT_ID('tempdb..#TempPacient') IS NOT NULL
        DROP TABLE #TempPacient
END CATCH
PRINT ''

-- ============================================================================
-- TEST 3: sp_Pacienti_GetById
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 3: sp_Pacienti_GetById'
PRINT '----------------------------------------'

IF @TestId IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC sp_Pacienti_GetById @Id = @TestId
        PRINT '? Procedura executata cu succes'
    END TRY
    BEGIN CATCH
        PRINT '? EROARE: ' + ERROR_MESSAGE()
    END CATCH
END
ELSE
BEGIN
    PRINT '? Test omis - pacient nu a fost creat'
END
PRINT ''

-- ============================================================================
-- TEST 4: sp_Pacienti_GetByCodPacient
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 4: sp_Pacienti_GetByCodPacient'
PRINT '----------------------------------------'

IF @TestCodPacient IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC sp_Pacienti_GetByCodPacient @CodPacient = @TestCodPacient
        PRINT '? Procedura executata cu succes'
    END TRY
    BEGIN CATCH
        PRINT '? EROARE: ' + ERROR_MESSAGE()
    END CATCH
END
ELSE
BEGIN
    PRINT '? Test omis - pacient nu a fost creat'
END
PRINT ''

-- ============================================================================
-- TEST 5: sp_Pacienti_GetByCNP
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 5: sp_Pacienti_GetByCNP'
PRINT '----------------------------------------'

BEGIN TRY
    EXEC sp_Pacienti_GetByCNP @CNP = @TestCNP
    PRINT '? Procedura executata cu succes'
END TRY
BEGIN CATCH
    PRINT '? EROARE: ' + ERROR_MESSAGE()
END CATCH
PRINT ''

-- ============================================================================
-- TEST 6: sp_Pacienti_CheckUnique
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 6: sp_Pacienti_CheckUnique'
PRINT '----------------------------------------'

BEGIN TRY
    -- Test CNP existent
    EXEC sp_Pacienti_CheckUnique 
        @CNP = @TestCNP,
        @Cod_Pacient = NULL,
        @ExcludeId = NULL
    
    PRINT '? Test CNP existent'
    
    -- Test CNP nou
    EXEC sp_Pacienti_CheckUnique 
        @CNP = '9999999999999',
        @Cod_Pacient = NULL,
        @ExcludeId = NULL
    
    PRINT '? Test CNP nou'
    
    -- Test exclude current ID
    IF @TestId IS NOT NULL
    BEGIN
        EXEC sp_Pacienti_CheckUnique 
            @CNP = @TestCNP,
            @Cod_Pacient = NULL,
            @ExcludeId = @TestId
        
        PRINT '? Test exclude ID curent'
    END
    
    PRINT '? Procedura executata cu succes'
END TRY
BEGIN CATCH
    PRINT '? EROARE: ' + ERROR_MESSAGE()
END CATCH
PRINT ''

-- ============================================================================
-- TEST 7: sp_Pacienti_Update
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 7: sp_Pacienti_Update'
PRINT '----------------------------------------'

IF @TestId IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC sp_Pacienti_Update
            @Id = @TestId,
            @CNP = @TestCNP,
            @Nume = 'Test',
            @Prenume = 'Pacient Modificat',
            @Data_Nasterii = '1980-05-15',
            @Sex = 'M',
            @Telefon = '0721234567',
            @Telefon_Secundar = '0723456789',
            @Email = 'test.modificat@email.com',
            @Judet = 'Bucuresti',
            @Localitate = 'Bucuresti',
            @Adresa = 'Str. Test Modificat nr. 456',
            @Cod_Postal = '010101',
            @Asigurat = 1,
            @Casa_Asigurari = 'CNAS',
            @Nr_Card_Sanatate = '123456789012',
            @Alergii = 'Penicilina, Polen',
            @Boli_Cronice = 'Hipertensiune, Diabet',
            @Medic_Familie = 'Dr. Pop Ion',
            @Persoana_Contact = 'Maria Popescu',
            @Telefon_Urgenta = '0722345678',
            @Relatie_Contact = 'Sotie',
            @Activ = 1,
            @Observatii = 'Pacient de test - modificat',
            @ModificatDe = 'TestScript'
        
        PRINT '? Pacient actualizat cu succes'
    END TRY
    BEGIN CATCH
        PRINT '? EROARE: ' + ERROR_MESSAGE()
    END CATCH
END
ELSE
BEGIN
    PRINT '? Test omis - pacient nu a fost creat'
END
PRINT ''

-- ============================================================================
-- TEST 8: sp_Pacienti_UpdateUltimaVizita
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 8: sp_Pacienti_UpdateUltimaVizita'
PRINT '----------------------------------------'

IF @TestId IS NOT NULL
BEGIN
    BEGIN TRY
        -- Primul update
        EXEC sp_Pacienti_UpdateUltimaVizita
            @Id = @TestId,
            @DataVizita = '2025-01-20',
            @ModificatDe = 'TestScript'
        
        PRINT '? Prima vizita adaugata'
        
        -- Al doilea update
        EXEC sp_Pacienti_UpdateUltimaVizita
            @Id = @TestId,
            @DataVizita = '2025-01-23',
            @ModificatDe = 'TestScript'
        
        PRINT '? A doua vizita adaugata'
        
        -- Verificare numar vizite
        DECLARE @NrVizite INT
        SELECT @NrVizite = Nr_Total_Vizite FROM Pacienti WHERE Id = @TestId
        PRINT '? Numar total vizite: ' + CAST(@NrVizite AS VARCHAR(10))
    END TRY
    BEGIN CATCH
        PRINT '? EROARE: ' + ERROR_MESSAGE()
    END CATCH
END
ELSE
BEGIN
    PRINT '? Test omis - pacient nu a fost creat'
END
PRINT ''

-- ============================================================================
-- TEST 9: sp_Pacienti_GetAll cu filtrare
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 9: sp_Pacienti_GetAll'
PRINT '----------------------------------------'

BEGIN TRY
    -- Fara filtre
    PRINT 'Test 9.1: Fara filtre'
    EXEC sp_Pacienti_GetAll
        @PageNumber = 1,
        @PageSize = 10,
        @SortColumn = 'Nume',
        @SortDirection = 'ASC'
    PRINT '? Success'
    
    -- Cu search text
    PRINT 'Test 9.2: Cu search text'
    EXEC sp_Pacienti_GetAll
        @PageNumber = 1,
        @PageSize = 10,
        @SearchText = 'Test',
        @SortColumn = 'Nume',
        @SortDirection = 'ASC'
    PRINT '? Success'
    
    -- Cu filtru asigurat
    PRINT 'Test 9.3: Filtru asigurat'
    EXEC sp_Pacienti_GetAll
        @PageNumber = 1,
        @PageSize = 10,
        @Asigurat = 1,
        @SortColumn = 'Nume',
        @SortDirection = 'ASC'
    PRINT '? Success'
    
    -- Cu filtru activ
    PRINT 'Test 9.4: Filtru activ'
    EXEC sp_Pacienti_GetAll
        @PageNumber = 1,
        @PageSize = 10,
        @Activ = 1,
        @SortColumn = 'Data_Inregistrare',
        @SortDirection = 'DESC'
    PRINT '? Success'
END TRY
BEGIN CATCH
    PRINT '? EROARE: ' + ERROR_MESSAGE()
END CATCH
PRINT ''

-- ============================================================================
-- TEST 10: sp_Pacienti_GetCount
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 10: sp_Pacienti_GetCount'
PRINT '----------------------------------------'

BEGIN TRY
    DECLARE @TotalCount INT
    
    CREATE TABLE #TempCount (TotalCount INT)
    
    INSERT INTO #TempCount
    EXEC sp_Pacienti_GetCount
        @SearchText = NULL,
        @Judet = NULL,
        @Asigurat = NULL,
        @Activ = 1
    
    SELECT @TotalCount = TotalCount FROM #TempCount
    
    PRINT '? Total pacienti activi: ' + CAST(@TotalCount AS VARCHAR(10))
    
    DROP TABLE #TempCount
END TRY
BEGIN CATCH
    PRINT '? EROARE: ' + ERROR_MESSAGE()
    IF OBJECT_ID('tempdb..#TempCount') IS NOT NULL
        DROP TABLE #TempCount
END CATCH
PRINT ''

-- ============================================================================
-- TEST 11: sp_Pacienti_GetStatistics
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 11: sp_Pacienti_GetStatistics'
PRINT '----------------------------------------'

BEGIN TRY
    EXEC sp_Pacienti_GetStatistics
    PRINT '? Procedura executata cu succes'
END TRY
BEGIN CATCH
    PRINT '? EROARE: ' + ERROR_MESSAGE()
END CATCH
PRINT ''

-- ============================================================================
-- TEST 12: sp_Pacienti_GetJudete
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 12: sp_Pacienti_GetJudete'
PRINT '----------------------------------------'

BEGIN TRY
    EXEC sp_Pacienti_GetJudete
    PRINT '? Procedura executata cu succes'
END TRY
BEGIN CATCH
    PRINT '? EROARE: ' + ERROR_MESSAGE()
END CATCH
PRINT ''

-- ============================================================================
-- TEST 13: sp_Pacienti_GetDropdownOptions
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 13: sp_Pacienti_GetDropdownOptions'
PRINT '----------------------------------------'

BEGIN TRY
    EXEC sp_Pacienti_GetDropdownOptions @Activ = 1
    PRINT '? Procedura executata cu succes'
END TRY
BEGIN CATCH
    PRINT '? EROARE: ' + ERROR_MESSAGE()
END CATCH
PRINT ''

-- ============================================================================
-- TEST 14: sp_Pacienti_GetBirthdays
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 14: sp_Pacienti_GetBirthdays'
PRINT '----------------------------------------'

BEGIN TRY
    EXEC sp_Pacienti_GetBirthdays
        @StartDate = '2025-01-01',
        @EndDate = '2025-12-31'
    PRINT '? Procedura executata cu succes'
END TRY
BEGIN CATCH
    PRINT '? EROARE: ' + ERROR_MESSAGE()
END CATCH
PRINT ''

-- ============================================================================
-- TEST 15: sp_Pacienti_Delete (Soft Delete)
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 15: sp_Pacienti_Delete (Soft Delete)'
PRINT '----------------------------------------'

IF @TestId IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC sp_Pacienti_Delete
            @Id = @TestId,
            @ModificatDe = 'TestScript'
        
        PRINT '? Pacient dezactivat cu succes'
        
        -- Verificare status
        DECLARE @Activ BIT
        SELECT @Activ = Activ FROM Pacienti WHERE Id = @TestId
        
        IF @Activ = 0
            PRINT '? Verificare: Pacient marcat ca inactiv'
        ELSE
            PRINT '? EROARE: Pacient inca activ'
    END TRY
    BEGIN CATCH
        PRINT '? EROARE: ' + ERROR_MESSAGE()
    END CATCH
END
ELSE
BEGIN
    PRINT '? Test omis - pacient nu a fost creat'
END
PRINT ''

-- ============================================================================
-- TEST 16: sp_Pacienti_HardDelete (Stergere Fizica)
-- ============================================================================
PRINT '----------------------------------------'
PRINT 'TEST 16: sp_Pacienti_HardDelete (Cleanup)'
PRINT '----------------------------------------'

IF @TestId IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC sp_Pacienti_HardDelete @Id = @TestId
        
        PRINT '? Pacient sters fizic cu succes'
        
        -- Verificare stergere
        IF NOT EXISTS (SELECT 1 FROM Pacienti WHERE Id = @TestId)
            PRINT '? Verificare: Pacient sters complet din baza de date'
        ELSE
            PRINT '? EROARE: Pacient inca exista in baza de date'
    END TRY
    BEGIN CATCH
        PRINT '? EROARE: ' + ERROR_MESSAGE()
    END CATCH
END
ELSE
BEGIN
    PRINT '? Test omis - pacient nu a fost creat'
END
PRINT ''

-- ============================================================================
-- SUMAR TESTE
-- ============================================================================
PRINT '========================================'
PRINT 'SUMAR TESTE COMPLETE'
PRINT '========================================'
PRINT ''
PRINT 'Toate testele au fost executate.'
PRINT 'Verificati output-ul pentru detalii despre fiecare test.'
PRINT ''
PRINT 'Proceduri testate:'
PRINT '  1. sp_Pacienti_GenerateNextCodPacient'
PRINT '  2. sp_Pacienti_Create'
PRINT '  3. sp_Pacienti_GetById'
PRINT '  4. sp_Pacienti_GetByCodPacient'
PRINT '  5. sp_Pacienti_GetByCNP'
PRINT '  6. sp_Pacienti_CheckUnique'
PRINT '  7. sp_Pacienti_Update'
PRINT '  8. sp_Pacienti_UpdateUltimaVizita'
PRINT '  9. sp_Pacienti_GetAll'
PRINT ' 10. sp_Pacienti_GetCount'
PRINT ' 11. sp_Pacienti_GetStatistics'
PRINT ' 12. sp_Pacienti_GetJudete'
PRINT ' 13. sp_Pacienti_GetDropdownOptions'
PRINT ' 14. sp_Pacienti_GetBirthdays'
PRINT ' 15. sp_Pacienti_Delete'
PRINT ' 16. sp_Pacienti_HardDelete'
PRINT ''
PRINT '========================================'
GO
