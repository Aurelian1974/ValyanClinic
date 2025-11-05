-- ========================================
-- Stored Procedures: Pacienti_PersonalMedical
-- Database: ValyanMed
-- Descriere: Proceduri pentru gestionarea relatiei Many-to-Many
-- Generat: 2025-01-23
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'Creare Stored Procedures';
PRINT 'Pacienti_PersonalMedical';
PRINT '========================================';
PRINT '';

-- ========================================
-- 1. sp_PacientiPersonalMedical_GetDoctoriByPacient
-- Returneaza toti doctorii unui pacient
-- ========================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_GetDoctoriByPacient')
    DROP PROCEDURE sp_PacientiPersonalMedical_GetDoctoriByPacient
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_GetDoctoriByPacient
    @PacientID UNIQUEIDENTIFIER,
    @ApenumereActivi BIT = 1 -- 1 = doar activi, 0 = toti, NULL = toti
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ppm.Id AS RelatieID,
     ppm.PacientID,
        ppm.PersonalMedicalID,
  ppm.TipRelatie,
        ppm.DataAsocierii,
      ppm.DataDezactivarii,
        ppm.EsteActiv,
 ppm.Observatii,
        ppm.Motiv,
   ppm.Data_Crearii,
   ppm.Data_Ultimei_Modificari,
      ppm.Creat_De,
        ppm.Modificat_De,
        -- Date Personal Medical
        pm.Nume AS DoctorNume,
   pm.Prenume AS DoctorPrenume,
      pm.Specializare AS DoctorSpecializare,
        pm.NumarLicenta AS DoctorNumarLicenta,
        pm.Telefon AS DoctorTelefon,
        pm.Email AS DoctorEmail,
    pm.Departament AS DoctorDepartament,
        pm.Pozitie AS DoctorPozitie,
        -- Computed
        (pm.Nume + ' ' + pm.Prenume) AS DoctorNumeComplet,
  DATEDIFF(DAY, ppm.DataAsocierii, GETDATE()) AS ZileDeAsociere
    FROM Pacienti_PersonalMedical ppm
    INNER JOIN PersonalMedical pm ON ppm.PersonalMedicalID = pm.PersonalID
    WHERE ppm.PacientID = @PacientID
        AND (@ApenumereActivi IS NULL OR ppm.EsteActiv = @ApenumereActivi)
    ORDER BY 
     ppm.EsteActiv DESC,
        ppm.DataAsocierii DESC;
END
GO

PRINT '? sp_PacientiPersonalMedical_GetDoctoriByPacient creat';

-- ========================================
-- 2. sp_PacientiPersonalMedical_GetPacientiByDoctor
-- Returneaza toti pacientii unui doctor
-- ========================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_GetPacientiByDoctor')
    DROP PROCEDURE sp_PacientiPersonalMedical_GetPacientiByDoctor
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_GetPacientiByDoctor
    @PersonalMedicalID UNIQUEIDENTIFIER,
    @ApenumereActivi BIT = 1,
    @TipRelatie NVARCHAR(50) = NULL
AS
BEGIN
  SET NOCOUNT ON;
    
    SELECT 
        ppm.Id AS RelatieID,
        ppm.PacientID,
        ppm.PersonalMedicalID,
        ppm.TipRelatie,
        ppm.DataAsocierii,
        ppm.DataDezactivarii,
      ppm.EsteActiv,
        ppm.Observatii,
        ppm.Motiv,
        ppm.Data_Crearii,
      ppm.Data_Ultimei_Modificari,
        -- Date Pacient
        p.Cod_Pacient AS PacientCod,
        p.Nume AS PacientNume,
     p.Prenume AS PacientPrenume,
        p.CNP AS PacientCNP,
        p.Data_Nasterii AS PacientDataNasterii,
    p.Sex AS PacientSex,
        p.Telefon AS PacientTelefon,
        p.Email AS PacientEmail,
        p.Judet AS PacientJudet,
 p.Localitate AS PacientLocalitate,
-- Computed
        (p.Nume + ' ' + p.Prenume) AS PacientNumeComplet,
        DATEDIFF(YEAR, p.Data_Nasterii, GETDATE()) - 
          CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, p.Data_Nasterii, GETDATE()), p.Data_Nasterii) > GETDATE() 
        THEN 1 ELSE 0 END AS PacientVarsta,
        DATEDIFF(DAY, ppm.DataAsocierii, GETDATE()) AS ZileDeAsociere
    FROM Pacienti_PersonalMedical ppm
    INNER JOIN Pacienti p ON ppm.PacientID = p.Id
    WHERE ppm.PersonalMedicalID = @PersonalMedicalID
        AND (@ApenumereActivi IS NULL OR ppm.EsteActiv = @ApenumereActivi)
        AND (@TipRelatie IS NULL OR ppm.TipRelatie = @TipRelatie)
    ORDER BY 
      ppm.EsteActiv DESC,
        p.Nume ASC,
        p.Prenume ASC;
END
GO

PRINT '? sp_PacientiPersonalMedical_GetPacientiByDoctor creat';

-- ========================================
-- 3. sp_PacientiPersonalMedical_AddRelatie
-- Adauga o relatie noua pacient-doctor
-- ========================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_AddRelatie')
    DROP PROCEDURE sp_PacientiPersonalMedical_AddRelatie
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_AddRelatie
    @PacientID UNIQUEIDENTIFIER,
    @PersonalMedicalID UNIQUEIDENTIFIER,
@TipRelatie NVARCHAR(50) = NULL,
@Observatii NVARCHAR(MAX) = NULL,
    @Motiv NVARCHAR(500) = NULL,
    @CreatDe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificare existenta pacient
    IF NOT EXISTS (SELECT 1 FROM Pacienti WHERE Id = @PacientID)
 BEGIN
     RAISERROR('Pacientul cu ID-ul specificat nu exista.', 16, 1);
      RETURN;
    END
    
    -- Verificare existenta personal medical
    IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE PersonalID = @PersonalMedicalID)
    BEGIN
        RAISERROR('Personalul medical cu ID-ul specificat nu exista.', 16, 1);
    RETURN;
    END
    
    -- Verificare relatie existenta activa
    IF EXISTS (
  SELECT 1 FROM Pacienti_PersonalMedical 
        WHERE PacientID = @PacientID 
   AND PersonalMedicalID = @PersonalMedicalID 
        AND EsteActiv = 1
    )
    BEGIN
        RAISERROR('Exista deja o relatie activa intre acest pacient si personalul medical.', 16, 1);
        RETURN;
  END
    
    -- Use NEWID() instead of NEWSEQUENTIALID() - NEWSEQUENTIALID() only works in DEFAULT constraints
    DECLARE @NewID UNIQUEIDENTIFIER = NEWID();
    
    -- Inserare relatie
    INSERT INTO Pacienti_PersonalMedical (
  Id,
    PacientID,
      PersonalMedicalID,
  TipRelatie,
  DataAsocierii,
        EsteActiv,
  Observatii,
        Motiv,
     Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    ) VALUES (
        @NewID,
     @PacientID,
        @PersonalMedicalID,
     @TipRelatie,
        GETDATE(),
      1,
     @Observatii,
        @Motiv,
   GETDATE(),
        GETDATE(),
  ISNULL(@CreatDe, SYSTEM_USER),
        ISNULL(@CreatDe, SYSTEM_USER)
    );
    
    -- Returnare relatia creata
    SELECT 
        ppm.*,
        (p.Nume + ' ' + p.Prenume) AS PacientNumeComplet,
        (pm.Nume + ' ' + pm.Prenume) AS DoctorNumeComplet
  FROM Pacienti_PersonalMedical ppm
    INNER JOIN Pacienti p ON ppm.PacientID = p.Id
    INNER JOIN PersonalMedical pm ON ppm.PersonalMedicalID = pm.PersonalID
    WHERE ppm.Id = @NewID;
END
GO

PRINT '? sp_PacientiPersonalMedical_AddRelatie creat';

-- ========================================
-- 4. sp_PacientiPersonalMedical_RemoveRelatie
-- Dezactiveaza (soft delete) o relatie
-- ========================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_RemoveRelatie')
    DROP PROCEDURE sp_PacientiPersonalMedical_RemoveRelatie
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_RemoveRelatie
    @RelatieID UNIQUEIDENTIFIER = NULL,
    @PacientID UNIQUEIDENTIFIER = NULL,
    @PersonalMedicalID UNIQUEIDENTIFIER = NULL,
    @ModificatDe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Trebuie sa fie specificat fie RelatieID, fie ambele PacientID si PersonalMedicalID
    IF @RelatieID IS NULL AND (@PacientID IS NULL OR @PersonalMedicalID IS NULL)
    BEGIN
        RAISERROR('Trebuie specificat fie RelatieID, fie PacientID si PersonalMedicalID.', 16, 1);
        RETURN;
    END
    
    -- Update relatie
    UPDATE Pacienti_PersonalMedical
  SET 
        EsteActiv = 0,
    DataDezactivarii = GETDATE(),
        Data_Ultimei_Modificari = GETDATE(),
        Modificat_De = ISNULL(@ModificatDe, SYSTEM_USER)
    WHERE 
        (@RelatieID IS NOT NULL AND Id = @RelatieID)
        OR
        (@RelatieID IS NULL AND PacientID = @PacientID AND PersonalMedicalID = @PersonalMedicalID AND EsteActiv = 1);
    
    IF @@ROWCOUNT = 0
    BEGIN
   RAISERROR('Relatia specificata nu a fost gasita sau este deja inactiva.', 16, 1);
        RETURN;
    END
    
    SELECT 1 AS Success, 'Relatie dezactivata cu succes.' AS Message;
END
GO

PRINT '? sp_PacientiPersonalMedical_RemoveRelatie creat';

-- ========================================
-- 5. sp_PacientiPersonalMedical_ReactiveazaRelatie
-- Reactiveaza o relatie inactiva
-- ========================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_ReactiveazaRelatie')
    DROP PROCEDURE sp_PacientiPersonalMedical_ReactiveazaRelatie
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_ReactiveazaRelatie
    @RelatieID UNIQUEIDENTIFIER,
    @ModificatDe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificare existenta relatie
    IF NOT EXISTS (SELECT 1 FROM Pacienti_PersonalMedical WHERE Id = @RelatieID)
 BEGIN
      RAISERROR('Relatia specificata nu exista.', 16, 1);
    RETURN;
    END
    
    -- Update relatie
    UPDATE Pacienti_PersonalMedical
    SET 
        EsteActiv = 1,
     DataDezactivarii = NULL,
        Data_Ultimei_Modificari = GETDATE(),
        Modificat_De = ISNULL(@ModificatDe, SYSTEM_USER)
    WHERE Id = @RelatieID;
    
    SELECT 1 AS Success, 'Relatie reactivata cu succes.' AS Message;
END
GO

PRINT '? sp_PacientiPersonalMedical_ReactiveazaRelatie creat';

-- ========================================
-- 6. sp_PacientiPersonalMedical_UpdateRelatie
-- Actualizeaza detaliile unei relatii
-- ========================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_UpdateRelatie')
    DROP PROCEDURE sp_PacientiPersonalMedical_UpdateRelatie
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_UpdateRelatie
    @RelatieID UNIQUEIDENTIFIER,
    @TipRelatie NVARCHAR(50) = NULL,
    @Observatii NVARCHAR(MAX) = NULL,
    @Motiv NVARCHAR(500) = NULL,
    @ModificatDe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificare existenta relatie
    IF NOT EXISTS (SELECT 1 FROM Pacienti_PersonalMedical WHERE Id = @RelatieID)
    BEGIN
        RAISERROR('Relatia specificata nu exista.', 16, 1);
  RETURN;
    END
    
    -- Update relatie
    UPDATE Pacienti_PersonalMedical
    SET 
        TipRelatie = ISNULL(@TipRelatie, TipRelatie),
      Observatii = ISNULL(@Observatii, Observatii),
        Motiv = ISNULL(@Motiv, Motiv),
        Data_Ultimei_Modificari = GETDATE(),
        Modificat_De = ISNULL(@ModificatDe, SYSTEM_USER)
    WHERE Id = @RelatieID;
    
    -- Returnare relatia actualizata
    SELECT 
        ppm.*,
        (p.Nume + ' ' + p.Prenume) AS PacientNumeComplet,
     (pm.Nume + ' ' + pm.Prenume) AS DoctorNumeComplet
    FROM Pacienti_PersonalMedical ppm
 INNER JOIN Pacienti p ON ppm.PacientID = p.Id
    INNER JOIN PersonalMedical pm ON ppm.PersonalMedicalID = pm.PersonalID
    WHERE ppm.Id = @RelatieID;
END
GO

PRINT '? sp_PacientiPersonalMedical_UpdateRelatie creat';

-- ========================================
-- 7. sp_PacientiPersonalMedical_GetStatistici
-- Returneaza statistici despre relatii
-- ========================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_GetStatistici')
    DROP PROCEDURE sp_PacientiPersonalMedical_GetStatistici
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_GetStatistici
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        -- Statistici generale
        COUNT(*) AS TotalRelatii,
  COUNT(CASE WHEN EsteActiv = 1 THEN 1 END) AS RelatiiActive,
        COUNT(CASE WHEN EsteActiv = 0 THEN 1 END) AS RelatiiInactive,
        
        -- Statistici doctori
        COUNT(DISTINCT PersonalMedicalID) AS TotalDoctori,
        COUNT(DISTINCT CASE WHEN EsteActiv = 1 THEN PersonalMedicalID END) AS DoctoriActivi,
        
        -- Statistici pacienti
        COUNT(DISTINCT PacientID) AS TotalPacienti,
   COUNT(DISTINCT CASE WHEN EsteActiv = 1 THEN PacientID END) AS PacientiActivi,
     
        -- Medii
        AVG(CASE WHEN EsteActiv = 1 THEN DATEDIFF(DAY, DataAsocierii, GETDATE()) END) AS MediuZileAsociere
    FROM Pacienti_PersonalMedical;
    
    -- Top 5 doctori cu cei mai multi pacienti
    SELECT TOP 5
pm.PersonalID,
        pm.Nume + ' ' + pm.Prenume AS DoctorNumeComplet,
      pm.Specializare,
   COUNT(*) AS NumarPacienti
    FROM Pacienti_PersonalMedical ppm
    INNER JOIN PersonalMedical pm ON ppm.PersonalMedicalID = pm.PersonalID
    WHERE ppm.EsteActiv = 1
    GROUP BY pm.PersonalID, pm.Nume, pm.Prenume, pm.Specializare
    ORDER BY NumarPacienti DESC;
    
    -- Distributie pe tip relatie
    SELECT 
        ISNULL(TipRelatie, 'Nespecificat') AS TipRelatie,
        COUNT(*) AS Numar,
        COUNT(CASE WHEN EsteActiv = 1 THEN 1 END) AS NumarActiv
 FROM Pacienti_PersonalMedical
 GROUP BY TipRelatie
    ORDER BY Numar DESC;
END
GO

PRINT '? sp_PacientiPersonalMedical_GetStatistici creat';

-- ========================================
-- 8. sp_PacientiPersonalMedical_GetRelatieById
-- Returneaza detalii complete pentru o relatie
-- ========================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_GetRelatieById')
    DROP PROCEDURE sp_PacientiPersonalMedical_GetRelatieById
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_GetRelatieById
  @RelatieID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ppm.*,
        -- Date Pacient
        p.Cod_Pacient,
    p.Nume AS PacientNume,
        p.Prenume AS PacientPrenume,
        (p.Nume + ' ' + p.Prenume) AS PacientNumeComplet,
p.CNP AS PacientCNP,
        p.Data_Nasterii AS PacientDataNasterii,
        p.Telefon AS PacientTelefon,
        p.Email AS PacientEmail,
   -- Date Doctor
        pm.Nume AS DoctorNume,
pm.Prenume AS DoctorPrenume,
        (pm.Nume + ' ' + pm.Prenume) AS DoctorNumeComplet,
     pm.Specializare AS DoctorSpecializare,
        pm.NumarLicenta AS DoctorNumarLicenta,
        pm.Telefon AS DoctorTelefon,
   pm.Email AS DoctorEmail,
        -- Computed
        DATEDIFF(DAY, ppm.DataAsocierii, GETDATE()) AS ZileDeAsociere
    FROM Pacienti_PersonalMedical ppm
    INNER JOIN Pacienti p ON ppm.PacientID = p.Id
    INNER JOIN PersonalMedical pm ON ppm.PersonalMedicalID = pm.PersonalID
    WHERE ppm.Id = @RelatieID;
END
GO

PRINT '? sp_PacientiPersonalMedical_GetRelatieById creat';

PRINT '';
PRINT '========================================';
PRINT 'STORED PROCEDURES GATA!';
PRINT '========================================';
PRINT '';
PRINT 'Proceduri create: 8';
PRINT '1. GetDoctoriByPacient';
PRINT '2. GetPacientiByDoctor';
PRINT '3. AddRelatie';
PRINT '4. RemoveRelatie (soft delete)';
PRINT '5. ReactiveazaRelatie';
PRINT '6. UpdateRelatie';
PRINT '7. GetStatistici';
PRINT '8. GetRelatieById';
PRINT '';
PRINT 'Gata pentru utilizare!';
PRINT '';

GO
