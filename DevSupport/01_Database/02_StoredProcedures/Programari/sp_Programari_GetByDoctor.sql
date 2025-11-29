-- ============================================================================
-- Stored Procedure: sp_Programari_GetByDoctor
-- Database: ValyanMed
-- Descriere: Obtinere programari pentru un doctor specific (interval de timp)
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetByDoctor')
    DROP PROCEDURE sp_Programari_GetByDoctor
GO

CREATE PROCEDURE sp_Programari_GetByDoctor
    @DoctorID UNIQUEIDENTIFIER,
    @DataStart DATE,
    @DataEnd DATE,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
  p.ProgramareID,
        p.PacientID,
        p.DoctorID,
        p.DataProgramare,
   p.OraInceput,
   p.OraSfarsit,
        p.TipProgramare,
  p.Status,
        p.Observatii,
   p.DataCreare,
        -- Detalii Pacient
        pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
pac.Telefon AS PacientTelefon,
   pac.CNP AS PacientCNP,
  -- Doctor
        doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet
  FROM Programari p
    INNER JOIN Pacienti pac ON p.PacientID = pac.Id
    INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
    WHERE p.DoctorID = @DoctorID
        AND p.DataProgramare BETWEEN @DataStart AND @DataEnd
        AND (@Status IS NULL OR p.Status = @Status)
    ORDER BY p.DataProgramare ASC, p.OraInceput ASC;
END
GO

PRINT '? sp_Programari_GetByDoctor creat cu succes';
GO
