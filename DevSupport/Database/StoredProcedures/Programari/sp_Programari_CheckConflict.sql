-- ============================================================================
-- Stored Procedure: sp_Programari_CheckConflict
-- Database: ValyanMed
-- Descriere: Verificare conflict orar pentru doctor (CRITICAL pentru validare)
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_CheckConflict')
    DROP PROCEDURE sp_Programari_CheckConflict
GO

CREATE PROCEDURE sp_Programari_CheckConflict
  @DoctorID UNIQUEIDENTIFIER,
  @DataProgramare DATE,
    @OraInceput TIME,
    @OraSfarsit TIME,
    @ExcludeProgramareID UNIQUEIDENTIFIER = NULL  -- Pentru UPDATE (exclude programarea curenta)
AS
BEGIN
    SET NOCOUNT ON;
  
 -- Returneaza programarile conflictuale (daca exista)
    SELECT 
 p.ProgramareID,
     p.DataProgramare,
     p.OraInceput,
        p.OraSfarsit,
  p.Status,
        p.TipProgramare,
  -- Detalii Pacient conflictual
     pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
  pac.Telefon AS PacientTelefon,
  -- Detalii Doctor
     doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
        -- Flag pentru tip conflict
  CASE 
       WHEN @OraInceput >= p.OraInceput AND @OraInceput < p.OraSfarsit 
      THEN 'Ora inceput se suprapune'
       WHEN @OraSfarsit > p.OraInceput AND @OraSfarsit <= p.OraSfarsit 
       THEN 'Ora sfarsit se suprapune'
      WHEN @OraInceput <= p.OraInceput AND @OraSfarsit >= p.OraSfarsit 
     THEN 'Suprapunere completa'
       ELSE 'Conflict necunoscut'
        END AS TipConflict
    FROM Programari p
    INNER JOIN Pacienti pac ON p.PacientID = pac.Id
    INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
 WHERE p.DoctorID = @DoctorID
    AND p.DataProgramare = @DataProgramare
      AND p.Status NOT IN ('Anulata', 'Finalizata')  -- Ignore programari anulate/finalizate
        AND (@ExcludeProgramareID IS NULL OR p.ProgramareID != @ExcludeProgramareID)
     AND (
    -- Check overlap conditions
      (@OraInceput >= p.OraInceput AND @OraInceput < p.OraSfarsit)
  OR
       (@OraSfarsit > p.OraInceput AND @OraSfarsit <= p.OraSfarsit)
            OR
            (@OraInceput <= p.OraInceput AND @OraSfarsit >= p.OraSfarsit)
  );
    
 -- Daca nu returneaza randuri = NO CONFLICT (perfect pentru UI validation)
END
GO

PRINT '? sp_Programari_CheckConflict creat cu succes';
GO
