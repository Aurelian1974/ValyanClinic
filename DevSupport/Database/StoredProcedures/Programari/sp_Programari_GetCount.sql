-- ============================================================================
-- Stored Procedure: sp_Programari_GetCount
-- Database: ValyanMed
-- Descriere: Obtinere numar total programari pentru paginare
-- Creat: 2025-01-15
-- Versiune: 1.1 (Modified: 2025-01-XX)
-- Modificari: 
--   - FilterDataStart default: prima zi a lunii curente (instead of azi)
--   - Cautare case-insensitive folosind COLLATE Latin1_General_CI_AI
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetCount')
    DROP PROCEDURE sp_Programari_GetCount
GO

CREATE PROCEDURE sp_Programari_GetCount
    @SearchText NVARCHAR(255) = NULL,
    @DoctorID UNIQUEIDENTIFIER = NULL,
    @PacientID UNIQUEIDENTIFIER = NULL,
    @DataStart DATE = NULL,
    @DataEnd DATE = NULL,
    @Status NVARCHAR(50) = NULL,
    @TipProgramare NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- ? MODIFICARE 1: Setare interval implicit - prima zi a lunii curente
    IF @DataStart IS NULL
      SET @DataStart = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    
    -- ? MODIFICARE 2: Data end - ultima zi a lunii curente
    IF @DataEnd IS NULL
        SET @DataEnd = EOMONTH(@DataStart);
    
    SELECT COUNT(*) AS TotalCount
    FROM Programari p
 INNER JOIN Pacienti pac ON p.PacientID = pac.Id
    INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
 WHERE 1=1
        AND p.DataProgramare BETWEEN @DataStart AND @DataEnd
        AND (@DoctorID IS NULL OR p.DoctorID = @DoctorID)
        AND (@PacientID IS NULL OR p.PacientID = @PacientID)
        AND (@Status IS NULL OR p.Status = @Status)
        AND (@TipProgramare IS NULL OR p.TipProgramare = @TipProgramare)
        -- ? MODIFICARE 3: Search global CASE-INSENSITIVE cu COLLATE
        AND (
      @SearchText IS NULL OR
       pac.Nume COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%' OR
  pac.Prenume COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%' OR
        pac.CNP LIKE '%' + @SearchText + '%' OR
 doc.Nume COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%' OR
         doc.Prenume COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%' OR
   p.Observatii COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%'
      );
END
GO

PRINT '? sp_Programari_GetCount creat cu succes (v1.1 - case-insensitive search + prima zi luna curenta)';
GO
