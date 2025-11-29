-- ============================================================================
-- View: VW_Programari_Calendar
-- Database: ValyanMed
-- Descriere: View optimizat pentru calendar view (Syncfusion Scheduler)
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VW_Programari_Calendar]'))
BEGIN
DROP VIEW [dbo].[VW_Programari_Calendar];
    PRINT '? View existenta stearsa (va fi recreata).';
END
GO

CREATE VIEW [dbo].[VW_Programari_Calendar]
AS
SELECT 
    -- ID-uri
    p.ProgramareID,
    p.PacientID,
    p.DoctorID,
    -- Data si ora (pentru calendar)
    p.DataProgramare,
    p.OraInceput,
    p.OraSfarsit,
    -- Start DateTime (pentru Syncfusion Scheduler)
  CAST(CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraInceput AS VARCHAR) AS DATETIME2) AS StartTime,
    -- End DateTime (pentru Syncfusion Scheduler)
    CAST(CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraSfarsit AS VARCHAR) AS DATETIME2) AS EndTime,
    -- Detalii
    p.Status,
    p.TipProgramare,
  p.Observatii,
  -- Subject pentru calendar (format: "Pacient - Tip programare")
    pac.Nume + ' ' + pac.Prenume + ' - ' + ISNULL(p.TipProgramare, 'Consultatie') AS Subject,
    -- Description pentru calendar
'Pacient: ' + pac.Nume + ' ' + pac.Prenume + CHAR(13) + CHAR(10) +
     'Telefon: ' + ISNULL(pac.Telefon, 'N/A') + CHAR(13) + CHAR(10) +
        'Tip: ' + ISNULL(p.TipProgramare, 'Consultatie') + CHAR(13) + CHAR(10) +
   'Status: ' + p.Status AS Description,
    -- Pacient (pentru tooltips)
    pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
    pac.Telefon AS PacientTelefon,
  -- Doctor (pentru filtrare si grupare)
    doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
    doc.Specializare AS DoctorSpecializare,
 -- Culoare pentru calendar (bazat pe status)
  CASE p.Status
    WHEN 'Programata' THEN '#3788d8'  -- Albastru
     WHEN 'Confirmata' THEN '#02a499'  -- Verde
 WHEN 'In desfasurare' THEN '#fdb849'  -- Portocaliu
 WHEN 'Finalizata' THEN '#6c757d'  -- Gri
   WHEN 'Anulata' THEN '#e63757'  -- Rosu
        ELSE '#3788d8'
    END AS CategoryColor,
    -- IsAllDay flag (pentru calendar)
    CAST(0 AS BIT) AS IsAllDay,
    -- IsReadonly (programari trecute sau anulate)
  CASE 
  WHEN p.Status IN ('Anulata', 'Finalizata') THEN CAST(1 AS BIT)
        WHEN p.DataProgramare < CAST(GETDATE() AS DATE) THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END AS IsReadOnly
FROM Programari p
INNER JOIN Pacienti pac ON p.PacientID = pac.Id
INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID;
GO

PRINT '? VW_Programari_Calendar creata cu succes';
PRINT '  Coloane optimizate pentru Syncfusion Scheduler';
PRINT '  Include: StartTime, EndTime, Subject, Description, CategoryColor, IsAllDay, IsReadOnly';
GO
