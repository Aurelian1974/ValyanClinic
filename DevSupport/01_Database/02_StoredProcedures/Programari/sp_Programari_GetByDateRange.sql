-- ============================================================================
-- Stored Procedure: sp_Programari_GetByDateRange
-- Database: ValyanMed
-- Descriere: ? OPTIMIZED - Obtine toate programarile pentru un interval de date (ex: saptamana)
--            PERFORMANCE: Inlocuieste multiple apeluri sp_Programari_GetByDate (7 queries ? 1 query)
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetByDateRange')
    DROP PROCEDURE sp_Programari_GetByDateRange
GO

CREATE PROCEDURE sp_Programari_GetByDateRange
    @DataStart DATE,
    @DataEnd DATE,
    @DoctorID UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- ? SINGLE QUERY pentru intervalul de date
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
        p.CreatDe,
        p.DataUltimeiModificari,
  p.ModificatDe,
        -- Detalii Pacient
        pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
        pac.Telefon AS PacientTelefon,
     pac.Email AS PacientEmail,
    pac.CNP AS PacientCNP,
        -- Detalii Doctor
        doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
  doc.Specializare AS DoctorSpecializare,
        doc.Telefon AS DoctorTelefon,
   -- Utilizator creat
        creator.Nume + ' ' + creator.Prenume AS CreatDeNumeComplet
    FROM Programari p
    INNER JOIN Pacienti pac ON p.PacientID = pac.Id
    INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
  LEFT JOIN PersonalMedical creator ON p.CreatDe = creator.PersonalID
    WHERE p.DataProgramare BETWEEN @DataStart AND @DataEnd
        AND (@DoctorID IS NULL OR p.DoctorID = @DoctorID)
    ORDER BY p.DataProgramare ASC, p.OraInceput ASC, doc.Nume ASC;
END
GO

PRINT '? sp_Programari_GetByDateRange creat cu succes (OPTIMIZED for week loading)';
GO
