-- Fix pentru sp_PacientiPersonalMedical_AddRelatie
-- Problema: NEWSEQUENTIALID() nu poate fi folosit in afara DEFAULT constraint

USE [ValyanMed]
GO

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
    
    -- Use NEWID() instead of NEWSEQUENTIALID()
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

PRINT 'sp_PacientiPersonalMedical_AddRelatie corectata cu succes!'
GO
