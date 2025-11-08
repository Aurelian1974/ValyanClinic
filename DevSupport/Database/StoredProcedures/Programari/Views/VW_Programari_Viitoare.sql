-- ============================================================================
-- View: VW_Programari_Viitoare
-- Database: ValyanMed
-- Descriere: View pentru programarile viitoare (urmatoarele 7 zile)
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VW_Programari_Viitoare]'))
BEGIN
    DROP VIEW [dbo].[VW_Programari_Viitoare];
    PRINT '? View existenta stearsa (va fi recreata).';
END
GO

CREATE VIEW [dbo].[VW_Programari_Viitoare]
AS
SELECT 
    p.ProgramareID,
    p.DataProgramare,
    p.OraInceput,
    p.OraSfarsit,
    p.Status,
    p.TipProgramare,
    -- Pacient
    p.PacientID,
    pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
    pac.Telefon AS PacientTelefon,
    pac.Email AS PacientEmail,
    -- Doctor
  p.DoctorID,
    doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
doc.Specializare AS DoctorSpecializare,
  -- Computed
  DATEDIFF(DAY, CAST(GETDATE() AS DATE), p.DataProgramare) AS ZilePanaProgramare,
    DATEDIFF(HOUR, GETDATE(), 
        CAST(CAST(p.DataProgramare AS VARCHAR) + ' ' + CAST(p.OraInceput AS VARCHAR) AS DATETIME)
    ) AS OrePanaProgramare
FROM Programari p
INNER JOIN Pacienti pac ON p.PacientID = pac.Id
INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
WHERE p.DataProgramare BETWEEN CAST(GETDATE() AS DATE) AND DATEADD(DAY, 7, CAST(GETDATE() AS DATE))
  AND p.Status NOT IN ('Anulata', 'Finalizata');
GO

PRINT '? VW_Programari_Viitoare creata cu succes';
PRINT '  Filtru: Urmatoarele 7 zile, status activ';
GO
