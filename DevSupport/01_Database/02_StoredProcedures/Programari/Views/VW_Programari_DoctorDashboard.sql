-- ============================================================================
-- View: VW_Programari_DoctorDashboard
-- Database: ValyanMed
-- Descriere: View complet pentru dashboard doctor (toate datele necesare)
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VW_Programari_DoctorDashboard]'))
BEGIN
    DROP VIEW [dbo].[VW_Programari_DoctorDashboard];
    PRINT '? View existenta stearsa (va fi recreata).';
END
GO

CREATE VIEW [dbo].[VW_Programari_DoctorDashboard]
AS
SELECT 
  -- IDs
    p.ProgramareID,
    p.PacientID,
    p.DoctorID,
    -- Data si ora
  p.DataProgramare,
    p.OraInceput,
 p.OraSfarsit,
    -- Status
    p.Status,
    p.TipProgramare,
    p.Observatii,
  -- Doctor info
    doc.Nume AS DoctorNume,
    doc.Prenume AS DoctorPrenume,
    doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
    doc.Specializare AS DoctorSpecializare,
    doc.Departament AS DoctorDepartament,
    doc.Telefon AS DoctorTelefon,
    -- Pacient info
    pac.Nume AS PacientNume,
    pac.Prenume AS PacientPrenume,
    pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
    pac.CNP AS PacientCNP,
    pac.Telefon AS PacientTelefon,
    pac.Email AS PacientEmail,
    pac.Data_Nasterii AS PacientDataNasterii,
    DATEDIFF(YEAR, pac.Data_Nasterii, GETDATE()) AS PacientVarsta,
  pac.Alergii AS PacientAlergii,
    pac.Boli_Cronice AS PacientBoliCronice,
    -- Computed fields pentru dashboard
    DATEDIFF(MINUTE, p.OraInceput, p.OraSfarsit) AS DurataMinute,
    -- Flags pentru status
    CASE WHEN p.DataProgramare = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END AS EstePentruAstazi,
  CASE 
        WHEN p.DataProgramare = CAST(GETDATE() AS DATE)
    AND CAST(GETDATE() AS TIME) BETWEEN p.OraInceput AND p.OraSfarsit
        THEN 1 ELSE 0 
    END AS EsteInDesfasurare,
    CASE 
     WHEN p.DataProgramare > CAST(GETDATE() AS DATE) THEN 1
        WHEN p.DataProgramare = CAST(GETDATE() AS DATE) AND p.OraInceput > CAST(GETDATE() AS TIME) THEN 1
 ELSE 0 
 END AS EsteViitoare,
    CASE WHEN p.DataProgramare < CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END AS EsteTrecuta,
-- Timp pana la programare (pentru sorting si notificari)
    CASE 
     WHEN p.DataProgramare >= CAST(GETDATE() AS DATE)
        THEN DATEDIFF(MINUTE, GETDATE(), 
            CAST(CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraInceput AS VARCHAR) AS DATETIME2))
        ELSE NULL
    END AS MinutePanaLaStart,
    -- Culoare pentru badge status
    CASE p.Status
     WHEN 'Programata' THEN 'badge-primary'
   WHEN 'Confirmata' THEN 'badge-success'
      WHEN 'In desfasurare' THEN 'badge-warning'
   WHEN 'Finalizata' THEN 'badge-secondary'
        WHEN 'Anulata' THEN 'badge-danger'
  ELSE 'badge-light'
    END AS StatusBadgeClass,
    -- Priority (pentru sorting: programari astazi si iminente = HIGH)
CASE 
     WHEN p.DataProgramare = CAST(GETDATE() AS DATE) 
          AND DATEDIFF(MINUTE, GETDATE(), 
        CAST(CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraInceput AS VARCHAR) AS DATETIME2)) <= 30
  THEN 'HIGH'
     WHEN p.DataProgramare = CAST(GETDATE() AS DATE) THEN 'MEDIUM'
        WHEN p.DataProgramare > CAST(GETDATE() AS DATE) 
           AND p.DataProgramare <= DATEADD(DAY, 1, CAST(GETDATE() AS DATE)) 
  THEN 'MEDIUM'
        ELSE 'LOW'
    END AS Priority
FROM Programari p
INNER JOIN Pacienti pac ON p.PacientID = pac.Id
INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
WHERE p.DataProgramare >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))  -- Ultimele 7 zile + viitor
  AND p.Status NOT IN ('Anulata');  -- Exclude anulate din dashboard
GO

PRINT '? VW_Programari_DoctorDashboard creata cu succes';
PRINT '  View complet pentru dashboard doctor cu toate campurile necesare';
PRINT '  Include: status flags, computed fields, priority, badge classes';
GO
