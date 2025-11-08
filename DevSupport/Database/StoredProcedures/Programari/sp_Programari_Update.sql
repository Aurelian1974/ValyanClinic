-- ============================================================================
-- Stored Procedure: sp_Programari_Update
-- Database: ValyanMed
-- Descriere: Actualizare programare existenta cu validari
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_Update')
    DROP PROCEDURE sp_Programari_Update
GO

CREATE PROCEDURE sp_Programari_Update
    @ProgramareID UNIQUEIDENTIFIER,
    @PacientID UNIQUEIDENTIFIER,
    @DoctorID UNIQUEIDENTIFIER,
    @DataProgramare DATE,
    @OraInceput TIME,
    @OraSfarsit TIME,
  @TipProgramare NVARCHAR(100),
    @Status NVARCHAR(50),
    @Observatii NVARCHAR(1000) = NULL,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
 
    -- Validare: Programarea exista
  IF NOT EXISTS (SELECT 1 FROM Programari WHERE ProgramareID = @ProgramareID)
        BEGIN
        THROW 50006, 'Programarea specificata nu exista.', 1;
        END
        
        -- Validare: Pacient exista si este activ
        IF NOT EXISTS (SELECT 1 FROM Pacienti WHERE Id = @PacientID AND Activ = 1)
        BEGIN
        THROW 50001, 'Pacientul nu exista sau nu este activ.', 1;
    END
     
        -- Validare: Doctor exista si este activ
      IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE PersonalID = @DoctorID AND EsteActiv = 1)
        BEGIN
            THROW 50002, 'Doctorul nu exista sau nu este activ.', 1;
        END
        
      -- Validare: Ora inceput < Ora sfarsit
    IF @OraInceput >= @OraSfarsit
     BEGIN
      THROW 50004, 'Ora de inceput trebuie sa fie mai mica decat ora de sfarsit.', 1;
        END
        
        -- Validare: Verificare conflict orar (exclude programarea curenta)
        IF EXISTS (
            SELECT 1 
    FROM Programari 
            WHERE DoctorID = @DoctorID
    AND DataProgramare = @DataProgramare
      AND ProgramareID != @ProgramareID  -- EXCLUDE programarea curenta!
     AND Status NOT IN ('Anulata', 'Finalizata')
              AND (
                  (@OraInceput >= OraInceput AND @OraInceput < OraSfarsit)
                  OR
          (@OraSfarsit > OraInceput AND @OraSfarsit <= OraSfarsit)
     OR
    (@OraInceput <= OraInceput AND @OraSfarsit >= OraSfarsit)
        )
        )
        BEGIN
          THROW 50005, 'Doctorul are deja o programare in acest interval orar.', 1;
    END
        
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        
        UPDATE Programari SET
PacientID = @PacientID,
DoctorID = @DoctorID,
DataProgramare = @DataProgramare,
   OraInceput = @OraInceput,
    OraSfarsit = @OraSfarsit,
       TipProgramare = @TipProgramare,
    Status = @Status,
        Observatii = @Observatii,
            DataUltimeiModificari = @CurrentDate,
            ModificatDe = @ModificatDe
        WHERE ProgramareID = @ProgramareID;
        
        COMMIT TRANSACTION;
        
        -- Returnare programare actualizata
        EXEC sp_Programari_GetById @ProgramareID;
 
    END TRY
    BEGIN CATCH
   IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;
 THROW;
    END CATCH
END
GO

PRINT '? sp_Programari_Update creat cu succes';
GO
