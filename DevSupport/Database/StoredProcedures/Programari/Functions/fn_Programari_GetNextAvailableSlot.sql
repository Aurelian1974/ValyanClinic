-- ============================================================================
-- Function: fn_Programari_GetNextAvailableSlot
-- Database: ValyanMed
-- Descriere: Gaseste urmatorul slot disponibil pentru un doctor
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'FN' AND name = 'fn_Programari_GetNextAvailableSlot')
    DROP FUNCTION fn_Programari_GetNextAvailableSlot
GO

CREATE FUNCTION fn_Programari_GetNextAvailableSlot
(
    @DoctorID UNIQUEIDENTIFIER,
    @DataStart DATE,
    @DurataDorita INT = 30  -- Minute
)
RETURNS TABLE
AS
RETURN
(
    -- Returneaza primele 5 sloturi disponibile
    WITH TimeSlots AS (
  SELECT TOP 100
        DATEADD(MINUTE, (ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1) * 15, 
       CAST(CAST(@DataStart AS VARCHAR) + ' 08:00:00' AS DATETIME2)) AS SlotStart
        FROM sys.objects
),
    AvailableSlots AS (
        SELECT 
   SlotStart,
       DATEADD(MINUTE, @DurataDorita, SlotStart) AS SlotEnd,
   CAST(SlotStart AS DATE) AS Data,
   CAST(SlotStart AS TIME) AS OraInceput,
   CAST(DATEADD(MINUTE, @DurataDorita, SlotStart) AS TIME) AS OraSfarsit
        FROM TimeSlots
  WHERE CAST(SlotStart AS TIME) BETWEEN '08:00' AND '18:00'  -- Program 8-18
       AND NOT EXISTS (
       -- Verifica daca exista conflict cu programari existente
    SELECT 1 
      FROM Programari p
      WHERE p.DoctorID = @DoctorID
     AND p.DataProgramare = CAST(SlotStart AS DATE)
    AND p.Status NOT IN ('Anulata')
    AND (
        (CAST(SlotStart AS TIME) >= p.OraInceput AND CAST(SlotStart AS TIME) < p.OraSfarsit)
     OR
     (CAST(DATEADD(MINUTE, @DurataDorita, SlotStart) AS TIME) > p.OraInceput 
          AND CAST(DATEADD(MINUTE, @DurataDorita, SlotStart) AS TIME) <= p.OraSfarsit)
       OR
       (CAST(SlotStart AS TIME) <= p.OraInceput 
    AND CAST(DATEADD(MINUTE, @DurataDorita, SlotStart) AS TIME) >= p.OraSfarsit)
            )
   )
    )
    SELECT TOP 5
   Data,
        OraInceput,
   OraSfarsit,
        @DurataDorita AS DurataMinute
FROM AvailableSlots
    WHERE Data >= @DataStart
    ORDER BY Data ASC, OraInceput ASC
);
GO

PRINT '? fn_Programari_GetNextAvailableSlot creata cu succes';
PRINT '  Functie pentru gasire slot disponibil (pentru UI suggestions)';
GO
