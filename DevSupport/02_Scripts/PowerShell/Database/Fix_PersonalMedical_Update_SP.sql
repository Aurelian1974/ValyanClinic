-- =============================================
-- FIX: sp_PersonalMedical_Update - Verificare si corec?ie
-- Problema: Pozitia nu se salveaza corect
-- =============================================

USE [ValyanMed]
GO

PRINT '=============================================='
PRINT 'FIX: sp_PersonalMedical_Update'
PRINT 'Verificare si corectare problema Pozitia'
PRINT '=============================================='
PRINT ''

-- Verifica daca SP exista
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_Update')
BEGIN
    PRINT '[INFO] sp_PersonalMedical_Update exista - va fi recreat'
    DROP PROCEDURE sp_PersonalMedical_Update
END
ELSE
BEGIN
    PRINT '[WARNING] sp_PersonalMedical_Update nu exista - va fi creat'
END

PRINT ''
GO

-- =============================================
-- RECREARE SP cu debug logging
-- =============================================
CREATE PROCEDURE sp_PersonalMedical_Update
    @PersonalID UNIQUEIDENTIFIER,
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Specializare NVARCHAR(100) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @Telefon NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = 1,
    @CategorieID UNIQUEIDENTIFIER = NULL,
    @SpecializareID UNIQUEIDENTIFIER = NULL,
    @SubspecializareID UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- DEBUG: Print incoming parameters
    PRINT 'DEBUG sp_PersonalMedical_Update called with:'
    PRINT '  PersonalID: ' + CAST(@PersonalID AS NVARCHAR(50))
    PRINT '  Pozitie: ' + ISNULL(@Pozitie, 'NULL')
    PRINT '  Nume: ' + ISNULL(@Nume, 'NULL')
    PRINT '  Prenume: ' + ISNULL(@Prenume, 'NULL')
    
    -- Verificare existenta
    IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE PersonalID = @PersonalID)
    BEGIN
        PRINT 'ERROR: PersonalMedical cu ID ' + CAST(@PersonalID AS NVARCHAR(50)) + ' nu exista'
        RETURN -1
    END
    
    -- Print existing values
    DECLARE @ExistingPozitie NVARCHAR(50)
    SELECT @ExistingPozitie = Pozitie FROM PersonalMedical WHERE PersonalID = @PersonalID
    PRINT 'Existing Pozitie: ' + ISNULL(@ExistingPozitie, 'NULL')
    
    -- Update cu validare
    BEGIN TRY
        UPDATE PersonalMedical SET
            Nume = @Nume,
            Prenume = @Prenume,
            Specializare = @Specializare,
            NumarLicenta = @NumarLicenta,
            Telefon = @Telefon,
            Email = @Email,
            Departament = @Departament,
            Pozitie = @Pozitie,
            EsteActiv = @EsteActiv,
            CategorieID = @CategorieID,
            SpecializareID = @SpecializareID,
            SubspecializareID = @SubspecializareID
        WHERE PersonalID = @PersonalID;
        
        -- Verifica daca update-ul a avut efect
        IF @@ROWCOUNT = 0
        BEGIN
            PRINT 'WARNING: Nicio inregistrare nu a fost actualizata'
        END
        ELSE
        BEGIN
            PRINT 'SUCCESS: Inregistrarea a fost actualizata'
            
            -- Verifica noua valoare
            DECLARE @NewPozitie NVARCHAR(50)
            SELECT @NewPozitie = Pozitie FROM PersonalMedical WHERE PersonalID = @PersonalID
            PRINT 'New Pozitie: ' + ISNULL(@NewPozitie, 'NULL')
        END
        
    END TRY
    BEGIN CATCH
        PRINT 'ERROR in UPDATE: ' + ERROR_MESSAGE()
        RETURN -1
    END CATCH
    
    -- Returnare record actualizat
    SELECT 
        PersonalID, Nume, Prenume, Specializare, NumarLicenta,
        Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare,
        CategorieID, SpecializareID, SubspecializareID
    FROM PersonalMedical 
    WHERE PersonalID = @PersonalID;
END;
GO

PRINT '[OK] sp_PersonalMedical_Update recreat cu debug logging'
PRINT ''

-- =============================================
-- TEST SCRIPT pentru verificare
-- =============================================
PRINT 'Test script pentru verificare functionalitate:'
PRINT ''
PRINT '-- Verifica daca exista inregistrari in PersonalMedical'
PRINT 'SELECT COUNT(*) as TotalRecords FROM PersonalMedical;'
PRINT ''
PRINT '-- Verifica primele 3 inregistrari'
PRINT 'SELECT TOP 3 PersonalID, Nume, Prenume, Pozitie FROM PersonalMedical;'
PRINT ''
PRINT '-- Test UPDATE pentru prima inregistrare (modifica doar pentru test):'
PRINT 'DECLARE @TestID UNIQUEIDENTIFIER = (SELECT TOP 1 PersonalID FROM PersonalMedical);'
PRINT 'EXEC sp_PersonalMedical_Update'
PRINT '    @PersonalID = @TestID,'
PRINT '    @Nume = ''Test Nume'','
PRINT '    @Prenume = ''Test Prenume'','
PRINT '    @Pozitie = ''Test Pozitie'','
PRINT '    @EsteActiv = 1;'
PRINT ''

PRINT '=============================================='
PRINT 'FIX COMPLET!'
PRINT '=============================================='
PRINT ''
PRINT 'URMATOARELE PASI:'
PRINT '1. Rulati testele de mai sus pentru a verifica SP'
PRINT '2. Restartati aplicatia Blazor'
PRINT '3. Testati modificarea unei inregistrari PersonalMedical'
PRINT '4. Verificati log-urile in consola pentru valorile trimise'
PRINT ''
PRINT 'Daca problema persista, verificati:'
PRINT '- Tabela Pozitii are inregistrari active'
PRINT '- Relatia FK intre PersonalMedical si Pozitii'
PRINT '- Valorile PozitieID trimise din frontend'
PRINT ''
PRINT '=============================================='