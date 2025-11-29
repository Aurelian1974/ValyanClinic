-- ============================================================================
-- Function: fn_Programari_CalculateDurataTotala
-- Database: ValyanMed
-- Descriere: Calculeaza durata totala programari pentru un doctor/perioada
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'FN' AND name = 'fn_Programari_CalculateDurataTotala')
    DROP FUNCTION fn_Programari_CalculateDurataTotala
GO

CREATE FUNCTION fn_Programari_CalculateDurataTotala
(
    @DoctorID UNIQUEIDENTIFIER,
    @DataStart DATE,
    @DataEnd DATE
)
RETURNS INT
AS
BEGIN
    DECLARE @TotalMinute INT;
    
    SELECT @TotalMinute = SUM(DATEDIFF(MINUTE, OraInceput, OraSfarsit))
FROM Programari
    WHERE DoctorID = @DoctorID
      AND DataProgramare BETWEEN @DataStart AND @DataEnd
  AND Status NOT IN ('Anulata');
  
 RETURN ISNULL(@TotalMinute, 0);
END
GO

PRINT '? fn_Programari_CalculateDurataTotala creata cu succes';
PRINT '  Functie pentru calcul durata totala programari';
GO
