-- ============================================================================
-- Stored Procedure: sp_Programari_GetDoctorSchedule
-- Database: ValyanMed
-- Descriere: Obtinere program complet doctor pentru o perioada (pentru dashboard)
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetDoctorSchedule')
    DROP PROCEDURE sp_Programari_GetDoctorSchedule
GO

CREATE PROCEDURE sp_Programari_GetDoctorSchedule
    @DoctorID UNIQUEIDENTIFIER,
  @DataStart DATE = NULL,
    @DataEnd DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Default: urmatoarele 7 zile
    IF @DataStart IS NULL
        SET @DataStart = CAST(GETDATE() AS DATE);
    
    IF @DataEnd IS NULL
        SET @DataEnd = DATEADD(DAY, 7, @DataStart);
    
    SELECT 
        p.ProgramareID,
        p.DataProgramare,
        p.OraInceput,
        p.OraSfarsit,
        p.Status,
   p.TipProgramare,
p.Observatii,
 -- Durata in minute
        DATEDIFF(MINUTE, p.OraInceput, p.OraSfarsit) AS DurataMinute,
     -- Pacient
      p.PacientID,
        pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
     pac.Telefon AS PacientTelefon,
        pac.Email AS PacientEmail,
        pac.CNP AS PacientCNP,
        pac.Data_Nasterii AS PacientDataNasterii,
        dbo.fn_CalculateAgeFromCNP(pac.CNP) AS PacientVarsta,  -- ✅ Calculat din CNP
 -- Status flags pentru dashboard
      CASE 
   WHEN p.DataProgramare = CAST(GETDATE() AS DATE) THEN 1
         ELSE 0
        END AS EstePentruAstazi,
        CASE 
   WHEN p.DataProgramare = CAST(GETDATE() AS DATE)
                AND CAST(GETDATE() AS TIME) BETWEEN p.OraInceput AND p.OraSfarsit
     THEN 1
          ELSE 0
        END AS EsteInDesfasurare,
        CASE 
        WHEN p.DataProgramare = CAST(GETDATE() AS DATE)
     AND p.OraInceput > CAST(GETDATE() AS TIME)
        THEN DATEDIFF(MINUTE, CAST(GETDATE() AS TIME), p.OraInceput)
            ELSE NULL
        END AS MinutePanaLaStart,
        -- Ultima consultatii pentru acest pacient (daca exista)
        (
            SELECT TOP 1 c.DataConsultatie
            FROM Consultatii c
            INNER JOIN Programari pr ON c.ProgramareID = pr.ProgramareID
      WHERE pr.PacientID = p.PacientID
    AND pr.DoctorID = @DoctorID
          AND c.DataConsultatie < p.DataProgramare
            ORDER BY c.DataConsultatie DESC
        ) AS UltimaConsultatieData
    FROM Programari p
    INNER JOIN Pacienti pac ON p.PacientID = pac.Id
    WHERE p.DoctorID = @DoctorID
        AND p.DataProgramare BETWEEN @DataStart AND @DataEnd
        AND p.Status NOT IN ('Anulata')
    ORDER BY p.DataProgramare ASC, p.OraInceput ASC;
END
GO

PRINT '? sp_Programari_GetDoctorSchedule creat cu succes';
PRINT '  Returneaza program complet doctor cu detalii pacient si status flags';
GO
