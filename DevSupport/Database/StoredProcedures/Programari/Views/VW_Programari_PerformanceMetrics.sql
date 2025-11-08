-- ============================================================================
-- View: VW_Programari_PerformanceMetrics
-- Database: ValyanMed
-- Descriere: Metrici performance doctori (pentru rapoarte si analytics)
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VW_Programari_PerformanceMetrics]'))
BEGIN
    DROP VIEW [dbo].[VW_Programari_PerformanceMetrics];
    PRINT '✓ View existenta stearsa (va fi recreata).';
END
GO

CREATE VIEW [dbo].[VW_Programari_PerformanceMetrics]
AS
SELECT 
    -- Doctor info
    doc.PersonalID AS DoctorID,
    doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
    doc.Specializare,
 doc.Departament,
    -- Metrici ultima luna
  (SELECT COUNT(*) 
     FROM Programari p2 
 WHERE p2.DoctorID = doc.PersonalID 
       AND p2.DataProgramare >= DATEADD(MONTH, -1, CAST(GETDATE() AS DATE))
    AND p2.DataProgramare <= CAST(GETDATE() AS DATE)
    ) AS ProgramariUltimaLuna,
    (SELECT COUNT(*) 
     FROM Programari p2 
   WHERE p2.DoctorID = doc.PersonalID 
  AND p2.DataProgramare >= DATEADD(MONTH, -1, CAST(GETDATE() AS DATE))
    AND p2.DataProgramare <= CAST(GETDATE() AS DATE)
     AND p2.Status = 'Finalizata'
    ) AS ProgramariFinalizateUltimaLuna,
    (SELECT COUNT(*) 
     FROM Programari p2 
 WHERE p2.DoctorID = doc.PersonalID 
AND p2.DataProgramare >= DATEADD(MONTH, -1, CAST(GETDATE() AS DATE))
       AND p2.DataProgramare <= CAST(GETDATE() AS DATE)
       AND p2.Status = 'Anulata'
    ) AS ProgramariAnulateUltimaLuna,
    -- Rata finalizare (%)
    CASE 
   WHEN (SELECT COUNT(*) FROM Programari p2 WHERE p2.DoctorID = doc.PersonalID 
       AND p2.DataProgramare >= DATEADD(MONTH, -1, CAST(GETDATE() AS DATE))
         AND p2.DataProgramare < CAST(GETDATE() AS DATE)) > 0
  THEN 
 CAST((SELECT COUNT(*) FROM Programari p2 WHERE p2.DoctorID = doc.PersonalID 
           AND p2.DataProgramare >= DATEADD(MONTH, -1, CAST(GETDATE() AS DATE))
           AND p2.DataProgramare < CAST(GETDATE() AS DATE)
   AND p2.Status = 'Finalizata') AS FLOAT) * 100 /
            (SELECT COUNT(*) FROM Programari p2 WHERE p2.DoctorID = doc.PersonalID 
      AND p2.DataProgramare >= DATEADD(MONTH, -1, CAST(GETDATE() AS DATE))
      AND p2.DataProgramare < CAST(GETDATE() AS DATE))
    ELSE 0
    END AS RataFinalizare,
    -- Pacienti unici ultima luna
    (SELECT COUNT(DISTINCT p2.PacientID)
 FROM Programari p2 
     WHERE p2.DoctorID = doc.PersonalID 
    AND p2.DataProgramare >= DATEADD(MONTH, -1, CAST(GETDATE() AS DATE))
       AND p2.DataProgramare <= CAST(GETDATE() AS DATE)
    ) AS PacientiUniciUltimaLuna,
    -- Durata medie programare (minute)
    (SELECT AVG(DATEDIFF(MINUTE, p2.OraInceput, p2.OraSfarsit))
     FROM Programari p2 
     WHERE p2.DoctorID = doc.PersonalID 
       AND p2.DataProgramare >= DATEADD(MONTH, -1, CAST(GETDATE() AS DATE))
         AND p2.DataProgramare <= CAST(GETDATE() AS DATE)
    ) AS DurataMedieProgramareMinute,
    -- Programari astazi
    (SELECT COUNT(*) 
     FROM Programari p2 
     WHERE p2.DoctorID = doc.PersonalID 
  AND p2.DataProgramare = CAST(GETDATE() AS DATE)
      AND p2.Status NOT IN ('Anulata')
    ) AS ProgramariAstazi,
-- Programari viitoare (urmatoarele 7 zile)
    (SELECT COUNT(*) 
     FROM Programari p2 
     WHERE p2.DoctorID = doc.PersonalID 
AND p2.DataProgramare BETWEEN CAST(GETDATE() AS DATE) AND DATEADD(DAY, 7, CAST(GETDATE() AS DATE))
           AND p2.Status NOT IN ('Anulata', 'Finalizata')
    ) AS ProgramariUrmatoarele7Zile
FROM PersonalMedical doc
WHERE doc.EsteActiv = 1
  AND doc.Pozitie LIKE 'Medic%';  -- Presupunand ca exista acest camp
GO

PRINT '✓ VW_Programari_PerformanceMetrics creata cu succes';
PRINT '  Metrici performance pentru fiecare doctor activ';
PRINT '  Include: rate finalizare, pacienti unici, durata medie, programari viitoare';
GO
