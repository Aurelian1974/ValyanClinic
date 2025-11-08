-- ============================================================================
-- Stored Procedure: sp_Programari_GetAll
-- Database: ValyanMed
-- Descriere: Obtinere lista paginata programari cu filtrare complexa
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetAll')
    DROP PROCEDURE sp_Programari_GetAll
GO

CREATE PROCEDURE sp_Programari_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SearchText NVARCHAR(255) = NULL,
    @DoctorID UNIQUEIDENTIFIER = NULL,
    @PacientID UNIQUEIDENTIFIER = NULL,
    @DataStart DATE = NULL,
    @DataEnd DATE = NULL,
    @Status NVARCHAR(50) = NULL,
    @TipProgramare NVARCHAR(100) = NULL,
    @SortColumn NVARCHAR(50) = 'DataProgramare',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate sort direction
    IF @SortDirection NOT IN ('ASC', 'DESC')
        SET @SortDirection = 'ASC';
    
    -- Whitelist for sort columns
    IF @SortColumn NOT IN ('DataProgramare', 'OraInceput', 'Status', 'PacientNume', 'DoctorNume', 'DataCreare')
        SET @SortColumn = 'DataProgramare';
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
  
    -- Setare interval implicit daca nu e specificat (urmatoarele 30 zile)
    IF @DataStart IS NULL
     SET @DataStart = CAST(GETDATE() AS DATE);
    
    IF @DataEnd IS NULL
        SET @DataEnd = DATEADD(DAY, 30, @DataStart);
    
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
        -- Join cu Pacienti pentru nume complet
        pac.Nume + ' ' + pac.Prenume AS PacientNumeComplet,
 pac.Telefon AS PacientTelefon,
     pac.Email AS PacientEmail,
        pac.CNP AS PacientCNP,
        -- Join cu PersonalMedical pentru doctor
     doc.Nume + ' ' + doc.Prenume AS DoctorNumeComplet,
        doc.Specializare AS DoctorSpecializare,
        doc.Telefon AS DoctorTelefon,
        -- Creat de (PersonalMedical)
        cre.Nume + ' ' + cre.Prenume AS CreatDeNumeComplet
    FROM Programari p
    INNER JOIN Pacienti pac ON p.PacientID = pac.Id
    INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
 LEFT JOIN PersonalMedical cre ON p.CreatDe = cre.PersonalID
WHERE 1=1
     -- Filtrare dupa data (obligatorie)
        AND p.DataProgramare BETWEEN @DataStart AND @DataEnd
        -- Filtrare dupa doctor
        AND (@DoctorID IS NULL OR p.DoctorID = @DoctorID)
        -- Filtrare dupa pacient
        AND (@PacientID IS NULL OR p.PacientID = @PacientID)
        -- Filtrare dupa status
        AND (@Status IS NULL OR p.Status = @Status)
        -- Filtrare dupa tip programare
  AND (@TipProgramare IS NULL OR p.TipProgramare = @TipProgramare)
 -- Search global (pacient, doctor, observatii)
        AND (
            @SearchText IS NULL OR
        pac.Nume LIKE '%' + @SearchText + '%' OR
  pac.Prenume LIKE '%' + @SearchText + '%' OR
            pac.CNP LIKE '%' + @SearchText + '%' OR
            doc.Nume LIKE '%' + @SearchText + '%' OR
     doc.Prenume LIKE '%' + @SearchText + '%' OR
       p.Observatii LIKE '%' + @SearchText + '%'
  )
    ORDER BY 
        CASE WHEN @SortColumn = 'DataProgramare' AND @SortDirection = 'ASC' 
 THEN p.DataProgramare END ASC,
 CASE WHEN @SortColumn = 'DataProgramare' AND @SortDirection = 'DESC' 
       THEN p.DataProgramare END DESC,
   CASE WHEN @SortColumn = 'OraInceput' AND @SortDirection = 'ASC' 
      THEN p.OraInceput END ASC,
        CASE WHEN @SortColumn = 'OraInceput' AND @SortDirection = 'DESC' 
    THEN p.OraInceput END DESC,
   CASE WHEN @SortColumn = 'Status' AND @SortDirection = 'ASC' 
  THEN p.Status END ASC,
        CASE WHEN @SortColumn = 'Status' AND @SortDirection = 'DESC' 
            THEN p.Status END DESC,
     CASE WHEN @SortColumn = 'PacientNume' AND @SortDirection = 'ASC' 
            THEN pac.Nume END ASC,
        CASE WHEN @SortColumn = 'PacientNume' AND @SortDirection = 'DESC' 
   THEN pac.Nume END DESC,
        CASE WHEN @SortColumn = 'DoctorNume' AND @SortDirection = 'ASC' 
       THEN doc.Nume END ASC,
        CASE WHEN @SortColumn = 'DoctorNume' AND @SortDirection = 'DESC' 
         THEN doc.Nume END DESC,
        CASE WHEN @SortColumn = 'DataCreare' AND @SortDirection = 'ASC' 
            THEN p.DataCreare END ASC,
     CASE WHEN @SortColumn = 'DataCreare' AND @SortDirection = 'DESC' 
            THEN p.DataCreare END DESC,
      -- Default sorting
        p.DataProgramare ASC,
 p.OraInceput ASC
    OFFSET @Offset ROWS 
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT '? sp_Programari_GetAll creat cu succes';
GO
