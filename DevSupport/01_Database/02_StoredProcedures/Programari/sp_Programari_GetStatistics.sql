-- ============================================================================
-- Stored Procedure: sp_Programari_GetStatistics
-- Database: ValyanMed
-- Descriere: Statistici programari pentru dashboard
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_GetStatistics')
    DROP PROCEDURE sp_Programari_GetStatistics
GO

CREATE PROCEDURE sp_Programari_GetStatistics
    @DataStart DATE = NULL,
    @DataEnd DATE = NULL
AS
BEGIN
  SET NOCOUNT ON;
    
    -- Setare interval implicit (ultimele 30 zile)
    IF @DataStart IS NULL
        SET @DataStart = DATEADD(DAY, -30, CAST(GETDATE() AS DATE));
    
    IF @DataEnd IS NULL
        SET @DataEnd = CAST(GETDATE() AS DATE);
    
    -- Statistici generale
    SELECT 
        'Total Programari' AS Categorie,
  COUNT(*) AS Numar,
        SUM(CASE WHEN Status = 'Programata' THEN 1 ELSE 0 END) AS Programate,
    SUM(CASE WHEN Status = 'Confirmata' THEN 1 ELSE 0 END) AS Confirmate,
        SUM(CASE WHEN Status = 'Finalizata' THEN 1 ELSE 0 END) AS Finalizate,
   SUM(CASE WHEN Status = 'Anulata' THEN 1 ELSE 0 END) AS Anulate
    FROM Programari
    WHERE DataProgramare BETWEEN @DataStart AND @DataEnd
    
    UNION ALL
    
    -- Programari astazi
    SELECT 
  'Programari Astazi' AS Categorie,
     COUNT(*) AS Numar,
        SUM(CASE WHEN Status = 'Programata' THEN 1 ELSE 0 END) AS Programate,
  SUM(CASE WHEN Status = 'Confirmata' THEN 1 ELSE 0 END) AS Confirmate,
     SUM(CASE WHEN Status = 'Finalizata' THEN 1 ELSE 0 END) AS Finalizate,
    SUM(CASE WHEN Status = 'Anulata' THEN 1 ELSE 0 END) AS Anulate
    FROM Programari
    WHERE DataProgramare = CAST(GETDATE() AS DATE)
 
  UNION ALL
    
    -- Programari viitoare
    SELECT 
        'Programari Viitoare' AS Categorie,
    COUNT(*) AS Numar,
SUM(CASE WHEN Status = 'Programata' THEN 1 ELSE 0 END) AS Programate,
        SUM(CASE WHEN Status = 'Confirmata' THEN 1 ELSE 0 END) AS Confirmate,
   0 AS Finalizate,  -- Nu pot fi finalizate (sunt in viitor)
        0 AS Anulate
    FROM Programari
    WHERE DataProgramare > CAST(GETDATE() AS DATE)
        AND Status NOT IN ('Anulata', 'Finalizata')
    
    UNION ALL

    -- Programari pe tip
    SELECT 
        'Tip: ' + TipProgramare AS Categorie,
      COUNT(*) AS Numar,
      0 AS Programate,
     0 AS Confirmate,
        0 AS Finalizate,
        0 AS Anulate
    FROM Programari
    WHERE DataProgramare BETWEEN @DataStart AND @DataEnd
        AND TipProgramare IS NOT NULL
    GROUP BY TipProgramare;
END
GO

PRINT '? sp_Programari_GetStatistics creat cu succes';
GO
