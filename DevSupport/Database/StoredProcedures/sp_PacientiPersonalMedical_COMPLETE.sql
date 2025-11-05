-- =============================================
-- Stored Procedures pentru Pacienti_PersonalMedical
-- Table: Many-to-Many între Pacienti ?i PersonalMedical
-- Database: ValyanMed
-- FIXED: Column names match actual table structure
-- =============================================

USE [ValyanMed]
GO

PRINT '=============================================='
PRINT 'Creating SPs for Pacienti_PersonalMedical'
PRINT 'FIXED: Column names corrected'
PRINT '=============================================='
PRINT ''

-- =============================================
-- SP 1: GetDoctoriByPacient - Ob?ine doctorii unui pacient
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_GetDoctoriByPacient')
    DROP PROCEDURE sp_PacientiPersonalMedical_GetDoctoriByPacient
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_GetDoctoriByPacient
    @PacientID UNIQUEIDENTIFIER,
    @ApenumereActivi BIT = 1  -- 1 = doar activi, 0 = to?i
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        -- Rela?ie info (CORRECTED column names)
        ppm.Id AS RelatieID,  -- FIXED: Id not RelatieID
     ppm.PacientID,
        ppm.PersonalMedicalID,
     ppm.TipRelatie,
        ppm.DataAsocierii,
        ppm.DataDezactivarii,
        ppm.EsteActiv,
        ppm.Motiv,
        ppm.Observatii,
        ppm.Creat_De AS CreatDe,  -- FIXED: Creat_De not CreatDe
        ppm.Data_Crearii AS DataCreare,  -- FIXED: Data_Crearii not DataCreare
        
        -- Doctor info
        pm.Nume AS DoctorNume,
        pm.Prenume AS DoctorPrenume,
        (pm.Nume + ' ' + pm.Prenume) AS DoctorNumeComplet,
    pm.Specializare AS DoctorSpecializare,
    pm.NumarLicenta AS DoctorNumarLicenta,
        pm.Telefon AS DoctorTelefon,
      pm.Email AS DoctorEmail,
 pm.Departament AS DoctorDepartament,
  pm.Pozitie AS DoctorPozitie,
 
        -- Calculated fields
        DATEDIFF(DAY, ppm.DataAsocierii, GETDATE()) AS ZileDeAsociere,
        CASE 
       WHEN ppm.DataDezactivarii IS NOT NULL 
     THEN DATEDIFF(DAY, ppm.DataAsocierii, ppm.DataDezactivarii)
      ELSE DATEDIFF(DAY, ppm.DataAsocierii, GETDATE())
        END AS ZileTotale
    
    FROM Pacienti_PersonalMedical ppm
  INNER JOIN PersonalMedical pm ON ppm.PersonalMedicalID = pm.PersonalID
    WHERE ppm.PacientID = @PacientID
      AND (@ApenumereActivi = 0 OR ppm.EsteActiv = 1)
    ORDER BY 
   ppm.EsteActiv DESC,  -- Activi first
        ppm.DataAsocierii DESC;  -- Newest first
END;
GO

PRINT '[OK] sp_PacientiPersonalMedical_GetDoctoriByPacient - FIXED'

-- =============================================
-- SP 2: GetPacientiByDoctor - Ob?ine pacien?ii unui doctor
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_GetPacientiByDoctor')
    DROP PROCEDURE sp_PacientiPersonalMedical_GetPacientiByDoctor
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_GetPacientiByDoctor
    @PersonalMedicalID UNIQUEIDENTIFIER,
    @ApenumereActivi BIT = 1  -- 1 = doar activi, 0 = to?i
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
   -- Rela?ie info (CORRECTED)
        ppm.Id AS RelatieID,-- FIXED
        ppm.PacientID,
        ppm.PersonalMedicalID,
ppm.TipRelatie,
        ppm.DataAsocierii,
     ppm.DataDezactivarii,
        ppm.EsteActiv,
   ppm.Motiv,
        ppm.Observatii,
        
-- Pacient info
        p.Cod_Pacient AS PacientCodPacient,
        p.Nume AS PacientNume,
        p.Prenume AS PacientPrenume,
     (p.Nume + ' ' + p.Prenume) AS PacientNumeComplet,
        p.CNP AS PacientCNP,
  p.Data_Nasterii AS PacientDataNasterii,
   DATEDIFF(YEAR, p.Data_Nasterii, GETDATE()) AS PacientVarsta,
   p.Sex AS PacientSex,
        p.Telefon AS PacientTelefon,
  p.Email AS PacientEmail,
        p.Activ AS PacientActiv,
     
        -- Calculated fields
        DATEDIFF(DAY, ppm.DataAsocierii, GETDATE()) AS ZileDeAsociere
        
    FROM Pacienti_PersonalMedical ppm
    INNER JOIN Pacienti p ON ppm.PacientID = p.Id
WHERE ppm.PersonalMedicalID = @PersonalMedicalID
   AND (@ApenumereActivi = 0 OR ppm.EsteActiv = 1)
ORDER BY 
        ppm.EsteActiv DESC,
        ppm.DataAsocierii DESC;
END;
GO

PRINT '[OK] sp_PacientiPersonalMedical_GetPacientiByDoctor - FIXED'

-- =============================================
-- SP 3: AddRelatie - Adaug? rela?ie pacient-doctor
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_AddRelatie')
    DROP PROCEDURE sp_PacientiPersonalMedical_AddRelatie
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_AddRelatie
    @PacientID UNIQUEIDENTIFIER,
  @PersonalMedicalID UNIQUEIDENTIFIER,
    @TipRelatie NVARCHAR(50) = NULL,
    @Motiv NVARCHAR(500) = NULL,
    @Observatii NVARCHAR(MAX) = NULL,
  @CreatDe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
    
      -- Verific? dac? exist? deja o rela?ie ACTIV?
IF EXISTS (
            SELECT 1 
       FROM Pacienti_PersonalMedical 
         WHERE PacientID = @PacientID 
        AND PersonalMedicalID = @PersonalMedicalID 
  AND EsteActiv = 1
        )
        BEGIN
         THROW 50001, 'Exist? deja o rela?ie activ? între acest pacient ?i doctor.', 1;
      END
        
 DECLARE @CurrentDate DATETIME2 = GETDATE();
     
    -- FIXED: NU inser?m Id manual - SQL Server îl genereaz? automat cu NEWSEQUENTIALID()
    -- OUTPUT clause pentru a captura Id-ul generat
        DECLARE @OutputTable TABLE (NewId UNIQUEIDENTIFIER);
   
        INSERT INTO Pacienti_PersonalMedical (
  -- Id excluded - auto-generated by SQL Server
            PacientID,
  PersonalMedicalID,
    TipRelatie,
          DataAsocierii,
            EsteActiv,
            Motiv,
  Observatii,
            Creat_De,
            Data_Crearii
   )
      OUTPUT INSERTED.Id INTO @OutputTable(NewId)
        VALUES (
         @PacientID,
  @PersonalMedicalID,
  @TipRelatie,
          @CurrentDate,
            1,  -- EsteActiv = true
            @Motiv,
            @Observatii,
       ISNULL(@CreatDe, SYSTEM_USER),
 @CurrentDate
        );
        
    -- Captureaz? Id-ul generat
   DECLARE @NewRelatieID UNIQUEIDENTIFIER;
     SELECT @NewRelatieID = NewId FROM @OutputTable;
 
     COMMIT TRANSACTION;
        
        -- Return created record
   SELECT 
         ppm.Id AS RelatieID,
            ppm.PacientID,
            ppm.PersonalMedicalID,
     ppm.TipRelatie,
      ppm.DataAsocierii,
     ppm.EsteActiv,
    ppm.Motiv,
   ppm.Observatii,
            pm.Nume + ' ' + pm.Prenume AS DoctorNumeComplet,
            pm.Specializare AS DoctorSpecializare
        FROM Pacienti_PersonalMedical ppm
        INNER JOIN PersonalMedical pm ON ppm.PersonalMedicalID = pm.PersonalID
WHERE ppm.Id = @NewRelatieID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
  
        -- Return detailed error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

PRINT '[OK] sp_PacientiPersonalMedical_AddRelatie - FIXED (auto-generated Id)'

-- =============================================
-- SP 4: RemoveRelatie - Dezactiveaz? rela?ie (soft delete)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_RemoveRelatie')
    DROP PROCEDURE sp_PacientiPersonalMedical_RemoveRelatie
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_RemoveRelatie
    @RelatieID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verific? dac? rela?ia exist? (FIXED: Id not RelatieID)
        IF NOT EXISTS (SELECT 1 FROM Pacienti_PersonalMedical WHERE Id = @RelatieID)
        BEGIN
            THROW 50002, 'Rela?ia specificat? nu exist?.', 1;
        END
 
        -- Soft delete - marcheaz? ca inactiv? (FIXED: Id not RelatieID)
        UPDATE Pacienti_PersonalMedical 
 SET EsteActiv = 0,
    DataDezactivarii = GETDATE()
        WHERE Id = @RelatieID;
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success, 'Rela?ia a fost dezactivat? cu succes.' AS Message;
        
    END TRY
  BEGIN CATCH
    IF @@TRANCOUNT > 0
     ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

PRINT '[OK] sp_PacientiPersonalMedical_RemoveRelatie - FIXED'

-- =============================================
-- SP 5: ReactivateRelatie - Reactiveaz? rela?ie dezactivat?
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_ReactivateRelatie')
  DROP PROCEDURE sp_PacientiPersonalMedical_ReactivateRelatie
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_ReactivateRelatie
    @RelatieID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
 BEGIN TRANSACTION;
        
        -- Verific? dac? rela?ia exist? (FIXED: Id not RelatieID)
        IF NOT EXISTS (SELECT 1 FROM Pacienti_PersonalMedical WHERE Id = @RelatieID)
        BEGIN
            THROW 50002, 'Rela?ia specificat? nu exist?.', 1;
     END
        
        -- Reactiveaz? rela?ia (FIXED: Id not RelatieID)
        UPDATE Pacienti_PersonalMedical 
        SET EsteActiv = 1,
  DataDezactivarii = NULL
        WHERE Id = @RelatieID;
        
      COMMIT TRANSACTION;
   
        SELECT 1 AS Success, 'Rela?ia a fost reactivat? cu succes.' AS Message;
     
    END TRY
    BEGIN CATCH
    IF @@TRANCOUNT > 0
  ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

PRINT '[OK] sp_PacientiPersonalMedical_ReactivateRelatie - FIXED'

-- =============================================
-- SP 6: GetStatistics - Statistici rela?ii pacient-doctor
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_GetStatistics')
    DROP PROCEDURE sp_PacientiPersonalMedical_GetStatistics
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_GetStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
      'Total Rela?ii' AS Categorie,
      COUNT(*) AS Numar,
        SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END) AS Active,
  SUM(CASE WHEN EsteActiv = 0 THEN 1 ELSE 0 END) AS Inactive
    FROM Pacienti_PersonalMedical
    
    UNION ALL
  
    SELECT 
        'Pacienti cu Doctori',
    COUNT(DISTINCT PacientID),
    COUNT(DISTINCT CASE WHEN EsteActiv = 1 THEN PacientID END),
     0
    FROM Pacienti_PersonalMedical
    
    UNION ALL
    
    SELECT 
        'Doctori cu Pacienti',
  COUNT(DISTINCT PersonalMedicalID),
  COUNT(DISTINCT CASE WHEN EsteActiv = 1 THEN PersonalMedicalID END),
        0
    FROM Pacienti_PersonalMedical;
END;
GO

PRINT '[OK] sp_PacientiPersonalMedical_GetStatistics'

-- =============================================
-- VERIFICATION
-- =============================================
PRINT ''
PRINT '=============================================='
PRINT 'VERIFICATION'
PRINT '=============================================='

DECLARE @SPCount INT = (
 SELECT COUNT(*) 
    FROM sys.objects 
    WHERE type = 'P' 
    AND name LIKE 'sp_PacientiPersonalMedical_%'
)

PRINT '[OK] Created ' + CAST(@SPCount AS VARCHAR(10)) + ' stored procedures'

SELECT 
    name AS [Stored Procedure],
    create_date AS [Created],
    modify_date AS [Modified]
FROM sys.procedures 
WHERE name LIKE 'sp_PacientiPersonalMedical_%'
ORDER BY name;

PRINT ''
PRINT '=============================================='
PRINT 'COMPLETE - ALL FIXED!'
PRINT '=============================================='
PRINT 'Stored Procedures:'
PRINT '  1. sp_PacientiPersonalMedical_GetDoctoriByPacient'
PRINT '  2. sp_PacientiPersonalMedical_GetPacientiByDoctor'
PRINT '  3. sp_PacientiPersonalMedical_AddRelatie'
PRINT '  4. sp_PacientiPersonalMedical_RemoveRelatie'
PRINT '  5. sp_PacientiPersonalMedical_ReactivateRelatie'
PRINT '  6. sp_PacientiPersonalMedical_GetStatistics'
PRINT ''
PRINT 'Column name fixes applied:'
PRINT '  - RelatieID -> Id'
PRINT '  - CreatDe -> Creat_De'
PRINT '  - DataCreare -> Data_Crearii'
PRINT ''
PRINT 'Ready to use! Test with:'
PRINT '  EXEC sp_PacientiPersonalMedical_GetDoctoriByPacient @PacientID = ''...guid...'', @ApenumereActivi = 0'
PRINT '=============================================='
