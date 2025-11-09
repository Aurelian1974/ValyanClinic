-- ============================================================================
-- Stored Procedure: sp_Programari_GetDoctorStatistics_v2
-- Database: ValyanMed
-- Descriere: Statistici programari pentru un doctor specific - Format NOU
-- Creat: 2025-01-XX
-- Versiune: 2.0 (ÎNLOCUIE?TE sp_Programari_GetDoctorStatistics)
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetDoctorStatistics_v2')
    DROP PROCEDURE sp_Programari_GetDoctorStatistics_v2
GO

CREATE PROCEDURE sp_Programari_GetDoctorStatistics_v2
    @DoctorID UNIQUEIDENTIFIER,
    @DataStart DATE = NULL,
    @DataEnd DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
 
    -- ? Setare interval implicit - prima zi ?i ultima zi a lunii curente
    IF @DataStart IS NULL
 SET @DataStart = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    
    IF @DataEnd IS NULL
        SET @DataEnd = EOMONTH(@DataStart);
    
    -- ==================== STATISTICI DOCTOR - UN SINGUR RESULT SET ====================
  
    SELECT 
        -- Total ?i contoare pe status
        COUNT(*) AS TotalProgramari,
  SUM(CASE WHEN Status = 'Programata' THEN 1 ELSE 0 END) AS Programate,
   SUM(CASE WHEN Status = 'Confirmata' THEN 1 ELSE 0 END) AS Confirmate,
 SUM(CASE WHEN Status = 'CheckedIn' THEN 1 ELSE 0 END) AS CheckedIn,
        SUM(CASE WHEN Status = 'InConsultatie' THEN 1 ELSE 0 END) AS InConsultatie,
     SUM(CASE WHEN Status = 'Finalizata' THEN 1 ELSE 0 END) AS Finalizate,
        SUM(CASE WHEN Status = 'Anulata' THEN 1 ELSE 0 END) AS Anulate,
 SUM(CASE WHEN Status = 'NoShow' THEN 1 ELSE 0 END) AS NoShow,
        
      -- Contoare pe tip programare
        SUM(CASE WHEN TipProgramare = 'ConsultatieInitiala' THEN 1 ELSE 0 END) AS ConsultatiiInitiale,
        SUM(CASE WHEN TipProgramare = 'ControlPeriodic' THEN 1 ELSE 0 END) AS ControalePeriodice,
        SUM(CASE WHEN TipProgramare = 'Consultatie' THEN 1 ELSE 0 END) AS Consultatii,
 SUM(CASE WHEN TipProgramare = 'Investigatie' THEN 1 ELSE 0 END) AS Investigatii,
 SUM(CASE WHEN TipProgramare = 'Procedura' THEN 1 ELSE 0 END) AS Proceduri,
        SUM(CASE WHEN TipProgramare = 'Urgenta' THEN 1 ELSE 0 END) AS Urgente,
        SUM(CASE WHEN TipProgramare = 'Telemedicina' THEN 1 ELSE 0 END) AS Telemedicina,
        SUM(CASE WHEN TipProgramare = 'LaDomiciliu' THEN 1 ELSE 0 END) AS LaDomiciliu,
        
        -- Statistici avansate (pentru un doctor, MediciActivi = 1)
     1 AS MediciActivi,
        COUNT(DISTINCT p.PacientID) AS PacientiUnici,
        
     -- Durata medie (în minute)
 AVG(DATEDIFF(MINUTE, p.OraInceput, p.OraSfarsit)) AS DurataMedieMinute
        
    FROM Programari p
    WHERE p.DoctorID = @DoctorID
        AND p.DataProgramare BETWEEN @DataStart AND @DataEnd;
    
END
GO

PRINT '? sp_Programari_GetDoctorStatistics_v2 creat cu succes (format pentru ProgramareStatisticsDto)';
GO

-- ============================================================================
-- TEST SCRIPT
-- ============================================================================

PRINT '';
PRINT '?? TEST: Ob?inere statistici pentru un doctor (trebuie s? existe date în DB)';
PRINT '';

-- Ob?ine primul doctor activ din PersonalMedical
DECLARE @TestDoctorID UNIQUEIDENTIFIER;
SELECT TOP 1 @TestDoctorID = PersonalID 
FROM PersonalMedical 
WHERE EsteActiv = 1
ORDER BY Nume, Prenume;

IF @TestDoctorID IS NOT NULL
BEGIN
    DECLARE @TestDataStart DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    DECLARE @TestDataEnd DATE = EOMONTH(@TestDataStart);
    
    DECLARE @DoctorNume NVARCHAR(200);
    SELECT @DoctorNume = Nume + ' ' + Prenume 
    FROM PersonalMedical 
    WHERE PersonalID = @TestDoctorID;
    
    PRINT 'Doctor test: ' + @DoctorNume + ' (' + CAST(@TestDoctorID AS NVARCHAR(36)) + ')';
    PRINT 'Perioada test: ' + CONVERT(VARCHAR(10), @TestDataStart, 120) + ' - ' + CONVERT(VARCHAR(10), @TestDataEnd, 120);
    PRINT '';
    
    EXEC sp_Programari_GetDoctorStatistics_v2 
        @DoctorID = @TestDoctorID,
     @DataStart = @TestDataStart,
        @DataEnd = @TestDataEnd;
    
    PRINT '';
    PRINT '? Test executat cu succes!';
END
ELSE
BEGIN
PRINT '?? ATEN?IE: Nu exist? medici activi în PersonalMedical pentru test';
END
