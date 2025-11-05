-- =============================================
-- FIX DEFINITIV: sp_PersonalMedical_Update cu PozitieID
-- Adauga suport pentru parametrul PozitieID
-- =============================================

USE [ValyanMed]
GO

PRINT '=============================================='
PRINT 'FIX DEFINITIV: sp_PersonalMedical_Update'
PRINT 'Adaugare parametru PozitieID'
PRINT '=============================================='
PRINT ''

-- Verifica daca SP exista si afiseaza parametrii actuali
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_Update')
BEGIN
    PRINT '[INFO] sp_PersonalMedical_Update exista'
    
    PRINT 'Parametrii actuali:'
    SELECT 
 p.name AS ParameterName,
        t.name AS DataType,
        p.max_length AS MaxLength,
        p.is_output AS IsOutput
    FROM sys.parameters p
    INNER JOIN sys.types t ON p.user_type_id = t.user_type_id
    WHERE object_id = OBJECT_ID('sp_PersonalMedical_Update')
    ORDER BY p.parameter_id
    
    -- Verifica daca PozitieID exista deja
    IF EXISTS (SELECT * FROM sys.parameters WHERE object_id = OBJECT_ID('sp_PersonalMedical_Update') AND name = '@PozitieID')
    BEGIN
        PRINT '[OK] Parametrul @PozitieID exista deja!'
    END
    ELSE
    BEGIN
   PRINT '[PROBLEMA] Parametrul @PozitieID NU EXISTA - va fi adaugat'
        DROP PROCEDURE sp_PersonalMedical_Update
    END
END
ELSE
BEGIN
    PRINT '[WARNING] sp_PersonalMedical_Update nu exista - va fi creat'
END

PRINT ''
GO

-- =============================================
-- Recreaza SP cu parametrul PozitieID
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
    @PozitieID UNIQUEIDENTIFIER = NULL,     -- ? ADAUGAT: Parametru PozitieID
    @SpecializareID UNIQUEIDENTIFIER = NULL,
    @SubspecializareID UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Log parametri (doar pentru debug, de eliminat in productie)
    IF @Pozitie IS NOT NULL OR @PozitieID IS NOT NULL
    BEGIN
        PRINT 'DEBUG sp_PersonalMedical_Update:'
        PRINT '  PersonalID: ' + CAST(@PersonalID AS NVARCHAR(50))
        PRINT '  Pozitie text: ' + ISNULL(@Pozitie, 'NULL')
        PRINT '  PozitieID: ' + ISNULL(CAST(@PozitieID AS NVARCHAR(50)), 'NULL')
    END
    
    -- Verificare existenta
    IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE PersonalID = @PersonalID)
    BEGIN
        PRINT 'ERROR: PersonalMedical cu ID ' + CAST(@PersonalID AS NVARCHAR(50)) + ' nu exista'
   RETURN -1
    END
    
    -- Update complet cu toate campurile
    BEGIN TRY
        UPDATE PersonalMedical SET
          Nume = @Nume,
  Prenume = @Prenume,
            Specializare = @Specializare,
  NumarLicenta = @NumarLicenta,
            Telefon = @Telefon,
            Email = @Email,
         Departament = @Departament,
    Pozitie = @Pozitie,       -- ? Text field
EsteActiv = @EsteActiv,
 CategorieID = @CategorieID,
            PozitieID = @PozitieID,      -- ? FK field - ACUM SUPORTAT!
      SpecializareID = @SpecializareID,
        SubspecializareID = @SubspecializareID
        WHERE PersonalID = @PersonalID;
        
        -- Verifica success
        IF @@ROWCOUNT = 0
        BEGIN
            PRINT 'WARNING: Nicio inregistrare nu a fost actualizata'
        END
    ELSE
  BEGIN
       PRINT 'SUCCESS: Inregistrarea a fost actualizata cu PozitieID'
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
        CategorieID, PozitieID, SpecializareID, SubspecializareID  -- ? Include PozitieID in result
    FROM PersonalMedical 
    WHERE PersonalID = @PersonalID;
END;
GO

PRINT '[OK] sp_PersonalMedical_Update recreat cu suport PozitieID'
PRINT ''

-- =============================================
-- Update si sp_PersonalMedical_Create 
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_Create')
BEGIN
    DROP PROCEDURE sp_PersonalMedical_Create
END
GO

CREATE PROCEDURE sp_PersonalMedical_Create
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
    @PozitieID UNIQUEIDENTIFIER = NULL,     -- ? ADAUGAT: Parametru PozitieID
    @SpecializareID UNIQUEIDENTIFIER = NULL,
    @SubspecializareID UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NewPersonalID UNIQUEIDENTIFIER = NEWID();
    DECLARE @CurrentDate DATETIME2 = GETDATE();
  
    INSERT INTO PersonalMedical (
        PersonalID, Nume, Prenume, Specializare, NumarLicenta,
        Telefon, Email, Departament, Pozitie, EsteActiv,
        CategorieID, PozitieID, SpecializareID, SubspecializareID, DataCreare
    ) VALUES (
        @NewPersonalID, @Nume, @Prenume, @Specializare, @NumarLicenta,
        @Telefon, @Email, @Departament, @Pozitie, @EsteActiv,
        @CategorieID, @PozitieID, @SpecializareID, @SubspecializareID, @CurrentDate
    );
    
    -- Returnare record creat
    SELECT 
 PersonalID, Nume, Prenume, Specializare, NumarLicenta,
        Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare,
     CategorieID, PozitieID, SpecializareID, SubspecializareID
    FROM PersonalMedical 
    WHERE PersonalID = @NewPersonalID;
END;
GO

PRINT '[OK] sp_PersonalMedical_Create recreat cu suport PozitieID'
PRINT ''

-- =============================================
-- Verificare finala
-- =============================================
PRINT '=============================================='
PRINT 'VERIFICARE FINALA'
PRINT '=============================================='

-- Lista parametri pentru UPDATE
PRINT 'Parametrii sp_PersonalMedical_Update:'
SELECT 
  p.name AS ParameterName,
    t.name AS DataType,
    CASE WHEN p.name = '@PozitieID' THEN '? ADAUGAT' ELSE '' END AS Status
FROM sys.parameters p
INNER JOIN sys.types t ON p.user_type_id = t.user_type_id
WHERE object_id = OBJECT_ID('sp_PersonalMedical_Update')
ORDER BY p.parameter_id

-- Verifica daca PozitieID este acum inclus
IF EXISTS (SELECT * FROM sys.parameters WHERE object_id = OBJECT_ID('sp_PersonalMedical_Update') AND name = '@PozitieID')
BEGIN
  PRINT ''
    PRINT '? SUCCESS: Parametrul @PozitieID a fost adaugat!'
    PRINT 'sp_PersonalMedical_Update acum accepta PozitieID'
END
ELSE
BEGIN
    PRINT ''
    PRINT '? FAILED: Parametrul @PozitieID inca lipseste!'
END

PRINT ''
PRINT '=============================================='
PRINT 'FIX COMPLET APLICAT!'
PRINT '=============================================='
PRINT ''
PRINT 'PASII URMATORI:'
PRINT '1. ? Stored Procedures actualizate cu PozitieID'
PRINT '2. ? PersonalMedicalRepository.cs actualizat cu PozitieID'
PRINT '3. ?? Restartati aplicatia Blazor'
PRINT '4. ?? Testati modificarea unei pozitii'
PRINT '5. ?? Verificati log-urile pentru confirmare'
PRINT ''
PRINT 'Acum PozitieID ar trebui sa se salveze corect!'
PRINT '=============================================='