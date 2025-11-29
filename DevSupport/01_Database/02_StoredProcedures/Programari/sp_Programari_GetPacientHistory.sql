-- ============================================================================
-- Stored Procedure: sp_Programari_GetPacientHistory
-- Database: ValyanMed
-- Descriere: Istoric complet programari pacient (pentru vizualizare in dashboard doctor)
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetPacientHistory')
    DROP PROCEDURE sp_Programari_GetPacientHistory
GO

CREATE PROCEDURE sp_Programari_GetPacientHistory
  @PacientID UNIQUEIDENTIFIER,
    @DoctorID UNIQUEIDENTIFIER = NULL,  -- Optional: filtrare dupa doctor
    @IncludeAnulate BIT = 0,
    @TopN INT = 10  -- Ultimele N programari
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@TopN)
        p.ProgramareID,
 p.DataProgramare,
  p.OraInceput,
p.OraSfarsit,
p.Status,
        p.TipProgramare,
   p.Observatii,
  p.DataCreare,
   -- Doctor
        p.DoctorID,
   doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
  doc.Specializare AS DoctorSpecializare,
      -- Consultatii asociate (daca exista)
     (SELECT COUNT(*) FROM Consultatii c WHERE c.ProgramareID = p.ProgramareID) AS AreConsultatie,
        (SELECT TOP 1 c.PlangereaPrincipala FROM Consultatii c WHERE c.ProgramareID = p.ProgramareID) AS ConsultatiePlangere,
        (SELECT TOP 1 c.Evaluare FROM Consultatii c WHERE c.ProgramareID = p.ProgramareID) AS ConsultatieEvaluare,
        -- Zile de la programare (pentru sorting)
        DATEDIFF(DAY, p.DataProgramare, CAST(GETDATE() AS DATE)) AS ZileDeLaProgramare
    FROM Programari p
  INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
    WHERE p.PacientID = @PacientID
      AND (@DoctorID IS NULL OR p.DoctorID = @DoctorID)
      AND (@IncludeAnulate = 1 OR p.Status != 'Anulata')
    ORDER BY p.DataProgramare DESC, p.OraInceput DESC;
END
GO

PRINT '? sp_Programari_GetPacientHistory creat cu succes';
PRINT '  Istoric programari pacient cu consultatii asociate';
GO
