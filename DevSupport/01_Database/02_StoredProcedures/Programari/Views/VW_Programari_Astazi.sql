-- ============================================================================
-- View: VW_Programari_Astazi
-- Database: ValyanMed
-- Descriere: View pentru programarile de astazi (monitoring)
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VW_Programari_Astazi]'))
BEGIN
    DROP VIEW [dbo].[VW_Programari_Astazi];
    PRINT '? View existenta stearsa (va fi recreata).';
END
GO

CREATE VIEW [dbo].[VW_Programari_Astazi]
AS
SELECT 
    p.ProgramareID,
    p.DataProgramare,
    p.OraInceput,
    p.OraSfarsit,
    p.Status,
    p.TipProgramare,
    p.Observatii,
    -- Pacient
    p.PacientID,
  pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
    pac.Telefon AS PacientTelefon,
    pac.CNP AS PacientCNP,
    -- Doctor
    p.DoctorID,
    doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
  doc.Specializare AS DoctorSpecializare,
    doc.Telefon AS DoctorTelefon,
 doc.Departament AS DoctorDepartament,
    -- Computed
    DATEDIFF(MINUTE, GETDATE(), 
        CAST(CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraInceput AS VARCHAR) AS DATETIME)
    ) AS MinutePanaLaProgramare,
    CASE 
     WHEN p.Status = 'Anulata' THEN 'Anulata'
   WHEN CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraSfarsit AS VARCHAR) < CONVERT(VARCHAR, GETDATE(), 120) 
            THEN 'Trecuta'
  WHEN CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraInceput AS VARCHAR) <= CONVERT(VARCHAR, GETDATE(), 120)
     AND CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraSfarsit AS VARCHAR) >= CONVERT(VARCHAR, GETDATE(), 120)
          THEN 'In desfasurare'
     ELSE 'Viitoare'
 END AS StareActuala
FROM Programari p
INNER JOIN Pacienti pac ON p.PacientID = pac.Id
INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
WHERE p.DataProgramare = CAST(GETDATE() AS DATE);
GO

PRINT '? VW_Programari_Astazi creata cu succes';
PRINT '  Coloane: ProgramareID, Data, Ora, Status, Pacient, Doctor, StareActuala, MinutePanaLaProgramare';
GO
