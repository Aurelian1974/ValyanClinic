-- ============================================================================
-- Stored Procedure: sp_Programari_GetDoctorStatistics
-- Database: ValyanMed
-- Descriere: Statistici detaliate pentru dashboard doctor
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetDoctorStatistics')
    DROP PROCEDURE sp_Programari_GetDoctorStatistics
GO

CREATE PROCEDURE sp_Programari_GetDoctorStatistics
    @DoctorID UNIQUEIDENTIFIER,
    @DataStart DATE = NULL,
    @DataEnd DATE = NULL
AS
BEGIN
 SET NOCOUNT ON;
  
    -- Default: ultima luna
    IF @DataStart IS NULL
        SET @DataStart = DATEADD(MONTH, -1, CAST(GETDATE() AS DATE));

    IF @DataEnd IS NULL
        SET @DataEnd = CAST(GETDATE() AS DATE);
    
    -- Returneaza un singur result set cu toate statisticile
    SELECT 
        -- Programari astazi
        (SELECT COUNT(*) 
 FROM Programari 
WHERE DoctorID = @DoctorID 
         AND DataProgramare = CAST(GETDATE() AS DATE)
           AND Status NOT IN ('Anulata')) AS ProgramariAstazi,
        
  -- Programari urmatoarele 7 zile
      (SELECT COUNT(*) 
         FROM Programari 
         WHERE DoctorID = @DoctorID 
      AND DataProgramare BETWEEN CAST(GETDATE() AS DATE) AND DATEADD(DAY, 7, CAST(GETDATE() AS DATE))
           AND Status NOT IN ('Anulata', 'Finalizata')) AS ProgramariUrmatoarele7Zile,
      
 -- Total programari in perioada
        (SELECT COUNT(*) 
      FROM Programari 
     WHERE DoctorID = @DoctorID 
       AND DataProgramare BETWEEN @DataStart AND @DataEnd) AS TotalProgramariPerioda,
     
        -- Programari finalizate
     (SELECT COUNT(*) 
         FROM Programari 
         WHERE DoctorID = @DoctorID 
      AND DataProgramare BETWEEN @DataStart AND @DataEnd
           AND Status = 'Finalizata') AS ProgramariFinalizate,
        
        -- Programari anulate
    (SELECT COUNT(*) 
  FROM Programari 
         WHERE DoctorID = @DoctorID 
           AND DataProgramare BETWEEN @DataStart AND @DataEnd
           AND Status = 'Anulata') AS ProgramariAnulate,
        
        -- Rata de prezenta (%)
        CASE 
    WHEN (SELECT COUNT(*) FROM Programari WHERE DoctorID = @DoctorID 
           AND DataProgramare BETWEEN @DataStart AND @DataEnd
          AND DataProgramare < CAST(GETDATE() AS DATE)) > 0
          THEN CAST((SELECT COUNT(*) FROM Programari WHERE DoctorID = @DoctorID 
         AND DataProgramare BETWEEN @DataStart AND @DataEnd
      AND Status = 'Finalizata') AS FLOAT) * 100 / 
        (SELECT COUNT(*) FROM Programari WHERE DoctorID = @DoctorID 
      AND DataProgramare BETWEEN @DataStart AND @DataEnd
    AND DataProgramare < CAST(GETDATE() AS DATE))
     ELSE 0
        END AS RataPrezenta,
  
        -- Numar pacienti unici
     (SELECT COUNT(DISTINCT PacientID) 
   FROM Programari 
         WHERE DoctorID = @DoctorID 
           AND DataProgramare BETWEEN @DataStart AND @DataEnd) AS PacientiUnici,
        
        -- Media programari pe zi
   CASE 
        WHEN DATEDIFF(DAY, @DataStart, @DataEnd) > 0
         THEN CAST((SELECT COUNT(*) FROM Programari WHERE DoctorID = @DoctorID 
 AND DataProgramare BETWEEN @DataStart AND @DataEnd) AS FLOAT) / 
       DATEDIFF(DAY, @DataStart, @DataEnd)
      ELSE 0
   END AS MediaProgramariPeZi,
        
 -- Durata medie programare (minute)
        (SELECT AVG(DATEDIFF(MINUTE, OraInceput, OraSfarsit))
         FROM Programari 
         WHERE DoctorID = @DoctorID 
           AND DataProgramare BETWEEN @DataStart AND @DataEnd) AS DurataMedieProgramareMinute,
        
        -- Tip programare cel mai frecvent
  (SELECT TOP 1 TipProgramare
   FROM Programari 
   WHERE DoctorID = @DoctorID 
    AND DataProgramare BETWEEN @DataStart AND @DataEnd
      AND TipProgramare IS NOT NULL
         GROUP BY TipProgramare
    ORDER BY COUNT(*) DESC) AS TipProgramareCelMaiFrecvent;
END
GO

PRINT '? sp_Programari_GetDoctorStatistics creat cu succes';
PRINT '  Returneaza KPI-uri complete pentru dashboard doctor';
GO
