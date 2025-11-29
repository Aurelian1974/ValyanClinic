-- ============================================================================
-- Stored Procedure: sp_Programari_GetUpcoming
-- Database: ValyanMed
-- Descriere: Programari iminente (urmatoarele ore) pentru notificari
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetUpcoming')
    DROP PROCEDURE sp_Programari_GetUpcoming
GO

CREATE PROCEDURE sp_Programari_GetUpcoming
    @DoctorID UNIQUEIDENTIFIER = NULL,
    @OreInainte INT = 2  -- Notifica cu X ore inainte
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentDateTime DATETIME2 = GETDATE();
    DECLARE @ThresholdDateTime DATETIME2 = DATEADD(HOUR, @OreInainte, @CurrentDateTime);
    
    SELECT 
p.ProgramareID,
 p.DoctorID,
        p.PacientID,
  p.DataProgramare,
        p.OraInceput,
        p.OraSfarsit,
     p.Status,
 p.TipProgramare,
        -- DateTime complet pentru start
  CAST(CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraInceput AS VARCHAR) AS DATETIME2) AS StartDateTime,
  -- Pacient
        pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
  pac.Telefon AS PacientTelefon,
 pac.Email AS PacientEmail,
        -- Doctor
  doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
   doc.Telefon AS DoctorTelefon,
doc.Departament AS DoctorDepartament,
   -- Minute pana la programare
  DATEDIFF(MINUTE, @CurrentDateTime, 
   CAST(CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraInceput AS VARCHAR) AS DATETIME2)
        ) AS MinutePanaLaProgramare,
        -- Ore pana la programare (decimal)
      CAST(DATEDIFF(MINUTE, @CurrentDateTime, 
CAST(CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraInceput AS VARCHAR) AS DATETIME2)
        ) AS FLOAT) / 60 AS OrePanaLaProgramare
    FROM Programari p
    INNER JOIN Pacienti pac ON p.PacientID = pac.Id
  INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
    WHERE (@DoctorID IS NULL OR p.DoctorID = @DoctorID)
      AND p.Status IN ('Programata', 'Confirmata')
  AND CAST(CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraInceput AS VARCHAR) AS DATETIME2) 
    BETWEEN @CurrentDateTime AND @ThresholdDateTime
  ORDER BY p.DataProgramare ASC, p.OraInceput ASC;
END
GO

PRINT '? sp_Programari_GetUpcoming creat cu succes';
PRINT '  Pentru notificari si alerte programari iminente';
GO
