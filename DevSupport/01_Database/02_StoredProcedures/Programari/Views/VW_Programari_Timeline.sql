-- ============================================================================
-- View: VW_Programari_Timeline
-- Database: ValyanMed
-- Descriere: View pentru timeline/activity feed (notificari, istoric recent)
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VW_Programari_Timeline]'))
BEGIN
    DROP VIEW [dbo].[VW_Programari_Timeline];
    PRINT '? View existenta stearsa (va fi recreata).';
END
GO

CREATE VIEW [dbo].[VW_Programari_Timeline]
AS
SELECT 
    p.ProgramareID,
  p.DoctorID,
    p.PacientID,
    p.DataProgramare,
    p.OraInceput,
 p.Status,
    p.TipProgramare,
    p.DataCreare,
    p.DataUltimeiModificari,
    -- Info pentru timeline
    CASE 
    WHEN p.DataCreare >= DATEADD(HOUR, -1, GETDATE()) THEN 'Creat acum ' + CAST(DATEDIFF(MINUTE, p.DataCreare, GETDATE()) AS VARCHAR) + ' minute'
  WHEN p.DataCreare >= DATEADD(HOUR, -24, GETDATE()) THEN 'Creat acum ' + CAST(DATEDIFF(HOUR, p.DataCreare, GETDATE()) AS VARCHAR) + ' ore'
   ELSE 'Creat pe ' + FORMAT(p.DataCreare, 'dd.MM.yyyy')
    END AS DataCreareText,
 CASE 
     WHEN p.DataUltimeiModificari IS NOT NULL AND p.DataUltimeiModificari >= DATEADD(HOUR, -1, GETDATE()) 
    THEN 'Modificat acum ' + CAST(DATEDIFF(MINUTE, p.DataUltimeiModificari, GETDATE()) AS VARCHAR) + ' minute'
     WHEN p.DataUltimeiModificari IS NOT NULL AND p.DataUltimeiModificari >= DATEADD(HOUR, -24, GETDATE()) 
       THEN 'Modificat acum ' + CAST(DATEDIFF(HOUR, p.DataUltimeiModificari, GETDATE()) AS VARCHAR) + ' ore'
   WHEN p.DataUltimeiModificari IS NOT NULL
     THEN 'Modificat pe ' + FORMAT(p.DataUltimeiModificari, 'dd.MM.yyyy')
        ELSE NULL
    END AS DataModificareText,
    -- Pacient
    pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
    pac.Telefon AS PacientTelefon,
  -- Doctor
    doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
    -- Creat de
    cre.Nume + ' ' + cre.Prenume AS CreatDeNumeComplet,
    -- Event type pentru UI icons/colors
CASE 
   WHEN p.Status = 'Programata' AND p.DataCreare >= DATEADD(HOUR, -24, GETDATE()) THEN 'new_appointment'
     WHEN p.Status = 'Confirmata' THEN 'confirmed'
    WHEN p.Status = 'Finalizata' THEN 'completed'
     WHEN p.Status = 'Anulata' THEN 'cancelled'
  WHEN p.DataUltimeiModificari >= DATEADD(HOUR, -24, GETDATE()) THEN 'modified'
        ELSE 'standard'
 END AS EventType,
    -- Icon pentru timeline
    CASE 
 WHEN p.Status = 'Programata' THEN 'fa-calendar-plus'
  WHEN p.Status = 'Confirmata' THEN 'fa-calendar-check'
   WHEN p.Status = 'Finalizata' THEN 'fa-check-circle'
    WHEN p.Status = 'Anulata' THEN 'fa-times-circle'
    ELSE 'fa-calendar'
  END AS TimelineIcon,
    -- Culoare pentru timeline
    CASE 
  WHEN p.Status = 'Programata' THEN 'timeline-primary'
        WHEN p.Status = 'Confirmata' THEN 'timeline-success'
   WHEN p.Status = 'Finalizata' THEN 'timeline-secondary'
   WHEN p.Status = 'Anulata' THEN 'timeline-danger'
        ELSE 'timeline-light'
    END AS TimelineColor,
    -- Relevance score (pentru sorting)
    CASE 
        -- Programari noi (24h) = highest priority
  WHEN p.DataCreare >= DATEADD(HOUR, -24, GETDATE()) THEN 100
    -- Modificate recent
    WHEN p.DataUltimeiModificari >= DATEADD(HOUR, -24, GETDATE()) THEN 90
   -- Programari astazi
        WHEN p.DataProgramare = CAST(GETDATE() AS DATE) THEN 80
     -- Programari urmatoare
        WHEN p.DataProgramare > CAST(GETDATE() AS DATE) THEN 70
   ELSE 50
    END AS RelevanceScore
FROM Programari p
INNER JOIN Pacienti pac ON p.PacientID = pac.Id
INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
LEFT JOIN PersonalMedical cre ON p.CreatDe = cre.PersonalID
WHERE p.DataCreare >= DATEADD(DAY, -7, GETDATE())  -- Ultimele 7 zile
   OR p.DataProgramare >= CAST(GETDATE() AS DATE);  -- Sau viitoare
GO

PRINT '? VW_Programari_Timeline creata cu succes';
PRINT '  View pentru activity feed / timeline cu text formatting si icons';
GO
