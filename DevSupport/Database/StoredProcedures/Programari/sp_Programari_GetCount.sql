-- ============================================================================
-- Stored Procedure: sp_Programari_GetCount
-- Database: ValyanMed
-- Descriere: Obtinere numar total programari pentru paginare
-- Creat: 2025-01-15
-- Versiune: 1.0
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
    
    -- Setare interval implicit daca nu e specificat
    IF @DataStart IS NULL
   SET @DataStart = CAST(GETDATE() AS DATE);
    
    IF @DataEnd IS NULL
   SET @DataEnd = DATEADD(DAY, 30, @DataStart);
    
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
     AND (
        @SearchText IS NULL OR
    pac.Nume LIKE '%' + @SearchText + '%' OR
            pac.Prenume LIKE '%' + @SearchText + '%' OR
            pac.CNP LIKE '%' + @SearchText + '%' OR
      doc.Nume LIKE '%' + @SearchText + '%' OR
       doc.Prenume LIKE '%' + @SearchText + '%' OR
     p.Observatii LIKE '%' + @SearchText + '%'
        );
END
GO

PRINT '? sp_Programari_GetCount creat cu succes';
GO
