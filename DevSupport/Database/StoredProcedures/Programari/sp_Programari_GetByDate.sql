-- ============================================================================
-- Stored Procedure: sp_Programari_GetByDate
-- Database: ValyanMed
-- Descriere: Obtinere toate programarile pentru o data specifica
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetByDate')
    DROP PROCEDURE sp_Programari_GetByDate
GO

CREATE PROCEDURE sp_Programari_GetByDate
    @DataProgramare DATE,
    @DoctorID UNIQUEIDENTIFIER = NULL,
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
        -- Detalii Pacient
        pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
        pac.Telefon AS PacientTelefon,
        -- Detalii Doctor
    doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
     doc.Specializare AS DoctorSpecializare,
  doc.Telefon AS DoctorTelefon,
 doc.Email AS DoctorEmail  -- ? NEW - pentru trimitere email-uri
    FROM Programari p
    INNER JOIN Pacienti pac ON p.PacientID = pac.Id
    INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
 WHERE p.DataProgramare = @DataProgramare
        AND (@DoctorID IS NULL OR p.DoctorID = @DoctorID)
        AND (@Status IS NULL OR p.Status = @Status)
    ORDER BY p.OraInceput ASC, doc.Nume ASC;
END
GO

PRINT '? sp_Programari_GetByDate creat cu succes';
GO
