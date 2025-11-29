-- ============================================================================
-- Stored Procedure: sp_Programari_Create
-- Database: ValyanMed
-- Descriere: Creare programare noua cu validari complete
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_Create')
    DROP PROCEDURE sp_Programari_Create
GO

CREATE PROCEDURE sp_Programari_Create
 @PacientID UNIQUEIDENTIFIER,
    @DoctorID UNIQUEIDENTIFIER,
 @DataProgramare DATE,
    @OraInceput TIME,
    @OraSfarsit TIME,
    @TipProgramare NVARCHAR(100) = 'Consultatie Generala',
    @Status NVARCHAR(50) = 'Programata',
@Observatii NVARCHAR(1000) = NULL,
 @CreatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
     BEGIN TRANSACTION;
        
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
     
        -- Validare: Data programare >= azi
  IF @DataProgramare < CAST(GETDATE() AS DATE)
        BEGIN
            THROW 50003, 'Data programarii nu poate fi in trecut.', 1;
        END
    
        -- Validare: Ora inceput < Ora sfarsit
   IF @OraInceput >= @OraSfarsit
    BEGIN
   THROW 50004, 'Ora de inceput trebuie sa fie mai mica decat ora de sfarsit.', 1;
        END
        
        -- Validare: Verificare conflict orar (acelasi doctor, aceeasi perioada)
        IF EXISTS (
            SELECT 1 
 FROM Programari 
   WHERE DoctorID = @DoctorID
     AND DataProgramare = @DataProgramare
           AND Status NOT IN ('Anulata', 'Finalizata')
 AND (
    -- Overlap: noua programare incepe in timpul alteia
          (@OraInceput >= OraInceput AND @OraInceput < OraSfarsit)
       OR
     -- Overlap: noua programare se termina in timpul alteia
      (@OraSfarsit > OraInceput AND @OraSfarsit <= OraSfarsit)
                  OR
 -- Overlap: noua programare cuprinde complet alta programare
       (@OraInceput <= OraInceput AND @OraSfarsit >= OraSfarsit)
              )
        )
        BEGIN
       THROW 50005, 'Doctorul are deja o programare in acest interval orar.', 1;
  END
        
        DECLARE @NewID UNIQUEIDENTIFIER;
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        DECLARE @OutputTable TABLE (ProgramareID UNIQUEIDENTIFIER);
        
        -- Insert cu OUTPUT pentru a captura ID-ul generat (NEWSEQUENTIALID)
        INSERT INTO Programari (
  PacientID,
            DoctorID,
            DataProgramare,
         OraInceput,
      OraSfarsit,
     TipProgramare,
    Status,
      Observatii,
    DataCreare,
    CreatDe
        )
        OUTPUT INSERTED.ProgramareID INTO @OutputTable(ProgramareID)
        VALUES (
     @PacientID,
      @DoctorID,
      @DataProgramare,
      @OraInceput,
            @OraSfarsit,
      @TipProgramare,
     @Status,
     @Observatii,
 @CurrentDate,
       @CreatDe
 );
        
        -- Preluare ID din table variable
        SELECT @NewID = ProgramareID FROM @OutputTable;
        
        COMMIT TRANSACTION;
        
      -- Returnare programare creata (cu join-uri pentru detalii complete)
    EXEC sp_Programari_GetById @NewID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
     THROW;
    END CATCH
END
GO

PRINT '? sp_Programari_Create creat cu succes';
GO
