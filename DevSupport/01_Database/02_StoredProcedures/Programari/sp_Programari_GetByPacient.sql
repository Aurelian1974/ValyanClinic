-- ============================================================================
-- Stored Procedure: sp_Programari_GetByPacient
-- Database: ValyanMed
-- Descriere: Obtinere istoricul programarilor unui pacient
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetByPacient')
    DROP PROCEDURE sp_Programari_GetByPacient
GO

CREATE PROCEDURE sp_Programari_GetByPacient
    @PacientID UNIQUEIDENTIFIER,
    @DataStart DATE = NULL,
    @DataEnd DATE = NULL,
    @Status NVARCHAR(50) = NULL,
    @IncludePast BIT = 1  -- Include programari trecute
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Daca nu e specificat interval, ia ultimele 6 luni + urmatoarele 6 luni
IF @DataStart IS NULL
        SET @DataStart = DATEADD(MONTH, -6, CAST(GETDATE() AS DATE));
    
    IF @DataEnd IS NULL
    SET @DataEnd = DATEADD(MONTH, 6, CAST(GETDATE() AS DATE));
    
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
        -- Detalii Doctor
        doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
        doc.Specializare AS DoctorSpecializare,
   doc.Telefon AS DoctorTelefon,
        doc.Departament AS DoctorDepartament,
        -- Pacient
  pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet
    FROM Programari p
    INNER JOIN Pacienti pac ON p.PacientID = pac.Id
    INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
    WHERE p.PacientID = @PacientID
      AND p.DataProgramare BETWEEN @DataStart AND @DataEnd
   AND (@Status IS NULL OR p.Status = @Status)
   AND (@IncludePast = 1 OR p.DataProgramare >= CAST(GETDATE() AS DATE))
    ORDER BY p.DataProgramare DESC, p.OraInceput DESC;
END
GO

PRINT '? sp_Programari_GetByPacient creat cu succes';
GO
