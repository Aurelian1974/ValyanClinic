-- ============================================================================
-- Stored Procedure: sp_Programari_GetById
-- Database: ValyanMed
-- Descriere: Obtinere detalii complete programare dupa ID
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetById')
    DROP PROCEDURE sp_Programari_GetById
GO

CREATE PROCEDURE sp_Programari_GetById
    @ProgramareID UNIQUEIDENTIFIER
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
p.CreatDe,
        p.DataUltimeiModificari,
        p.ModificatDe,
      -- Detalii Pacient
        pac.Nume AS PacientNume,
        pac.Prenume AS PacientPrenume,
        pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
      pac.CNP AS PacientCNP,
        pac.Telefon AS PacientTelefon,
  pac.Email AS PacientEmail,
        pac.Data_Nasterii AS PacientDataNasterii,
        -- Detalii Doctor
        doc.Nume AS DoctorNume,
        doc.Prenume AS DoctorPrenume,
  doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
   doc.Specializare AS DoctorSpecializare,
    doc.Telefon AS DoctorTelefon,
        doc.Email AS DoctorEmail,
        doc.Departament AS DoctorDepartament,
        -- Creat de
    cre.Nume + ' ' + cre.Prenume AS CreatDeNumeComplet,
        -- Modificat de
        mod.Nume + ' ' + mod.Prenume AS ModificatDeNumeComplet
    FROM Programari p
    INNER JOIN Pacienti pac ON p.PacientID = pac.Id
    INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
    LEFT JOIN PersonalMedical cre ON p.CreatDe = cre.PersonalID
    LEFT JOIN PersonalMedical mod ON p.ModificatDe = mod.PersonalID
    WHERE p.ProgramareID = @ProgramareID;
END
GO

PRINT '? sp_Programari_GetById creat cu succes';
GO
