-- =============================================
-- DIAGNOSTIC FINAL: Test Direct sp_Pacienti_GetAll
-- Verificare EXACTĂ ce returnează SP-ul
-- =============================================

USE [ValyanMed]
GO

PRINT '╔════════════════════════════════════════════════════════════════╗';
PRINT '║  DIAGNOSTIC FINAL: Test Direct sp_Pacienti_GetAll              ║';
PRINT '╚════════════════════════════════════════════════════════════════╝';
PRINT '';

-- =============================================
-- 1. Verificare date în tabela Pacienti
-- =============================================
PRINT '1. Verificare date în tabela Pacienti:';
PRINT '────────────────────────────────────────────────────────────────';

SELECT COUNT(*) AS 'Total Pacienti in DB' FROM Pacienti;
SELECT COUNT(*) AS 'Pacienti Activi' FROM Pacienti WHERE Activ = 1;
SELECT COUNT(*) AS 'Pacienti Inactivi' FROM Pacienti WHERE Activ = 0;

PRINT '';

-- Afișare primii 3 pacienți
SELECT TOP 3 
    Id,
    Cod_Pacient,
    Nume,
    Prenume,
    CONCAT(Nume, ' ', Prenume) AS NumeComplet,
    Activ,
    Asigurat,
    Data_Crearii
FROM Pacienti
ORDER BY Nume;

PRINT '';
PRINT '════════════════════════════════════════════════════════════════';

-- =============================================
-- 2. Test sp_Pacienti_GetAll cu parametri NULL
-- =============================================
PRINT '';
PRINT '2. Test sp_Pacienti_GetAll cu TOȚI parametrii NULL:';
PRINT '────────────────────────────────────────────────────────────────';
PRINT 'Parametri:';
PRINT '  @PageNumber   = 1';
PRINT '  @PageSize     = 25';
PRINT '  @SearchText   = NULL';
PRINT '  @Judet        = NULL';
PRINT '  @Asigurat     = NULL';
PRINT '  @Activ        = NULL';
PRINT '  @SortColumn   = ''Nume''';
PRINT '  @SortDirection= ''ASC''';
PRINT '';

EXEC sp_Pacienti_GetAll
    @PageNumber = 1,
    @PageSize = 25,
    @SearchText = NULL,
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = NULL,
    @SortColumn = 'Nume',
    @SortDirection = 'ASC';

PRINT '';
PRINT '════════════════════════════════════════════════════════════════';

-- =============================================
-- 3. Test sp_Pacienti_GetAll cu @Activ = 1 explicit
-- =============================================
PRINT '';
PRINT '3. Test sp_Pacienti_GetAll cu @Activ = 1 (explicit):';
PRINT '────────────────────────────────────────────────────────────────';

EXEC sp_Pacienti_GetAll
    @PageNumber = 1,
    @PageSize = 25,
    @SearchText = NULL,
    @Judet = NULL,
    @Asigurat = NULL,
    @Activ = 1,
    @SortColumn = 'Nume',
    @SortDirection = 'ASC';

PRINT '';
PRINT '════════════════════════════════════════════════════════════════';

-- =============================================
-- 4. Verificare structură SP
-- =============================================
PRINT '';
PRINT '4. Verificare WHERE clause în sp_Pacienti_GetAll:';
PRINT '────────────────────────────────────────────────────────────────';

DECLARE @SPDefinition NVARCHAR(MAX);
SELECT @SPDefinition = OBJECT_DEFINITION(OBJECT_ID('dbo.sp_Pacienti_GetAll'));

-- Căutare WHERE clause pentru @Activ
IF @SPDefinition LIKE '%WHERE%Activ%=%@Activ%'
BEGIN
    PRINT '❌ PROBLEMATIC: WHERE Activ = @Activ (returnează 0 când @Activ este NULL)';
END
ELSE IF @SPDefinition LIKE '%@Activ%IS NULL%OR%Activ%=%@Activ%'
BEGIN
    PRINT '✅ CORRECT: WHERE (@Activ IS NULL OR Activ = @Activ)';
END
ELSE
BEGIN
    PRINT '⚠️  NU găsit pattern cunoscut pentru @Activ';
END

-- Căutare WHERE clause pentru @Asigurat
IF @SPDefinition LIKE '%WHERE%Asigurat%=%@Asigurat%'
BEGIN
    PRINT '❌ PROBLEMATIC: WHERE Asigurat = @Asigurat';
END
ELSE IF @SPDefinition LIKE '%@Asigurat%IS NULL%OR%Asigurat%=%@Asigurat%'
BEGIN
    PRINT '✅ CORRECT: WHERE (@Asigurat IS NULL OR Asigurat = @Asigurat)';
END
ELSE
BEGIN
    PRINT '⚠️  NU găsit pattern cunoscut pentru @Asigurat';
END

PRINT '';
PRINT '════════════════════════════════════════════════════════════════';

-- =============================================
-- 5. Test manual WHERE clause
-- =============================================
PRINT '';
PRINT '5. Test manual WHERE clause (simulare SP logic):';
PRINT '────────────────────────────────────────────────────────────────';

DECLARE @Activ BIT = NULL;
DECLARE @Asigurat BIT = NULL;

PRINT 'Parametri: @Activ = NULL, @Asigurat = NULL';
PRINT '';

-- Test GREȘIT (cum era înainte)
PRINT 'A) WHERE Activ = @Activ AND Asigurat = @Asigurat (GREȘIT):';
SELECT COUNT(*) AS 'Records Returned'
FROM Pacienti
WHERE Activ = @Activ AND Asigurat = @Asigurat;

PRINT '';

-- Test CORECT (cum ar trebui să fie)
PRINT 'B) WHERE (@Activ IS NULL OR Activ = @Activ) AND (@Asigurat IS NULL OR Asigurat = @Asigurat) (CORECT):';
SELECT COUNT(*) AS 'Records Returned'
FROM Pacienti
WHERE (@Activ IS NULL OR Activ = @Activ)
  AND (@Asigurat IS NULL OR Asigurat = @Asigurat);

PRINT '';
PRINT '════════════════════════════════════════════════════════════════';
PRINT '';
PRINT '╔════════════════════════════════════════════════════════════════╗';
PRINT '║  CONCLUZIE:                                                     ║';
PRINT '╠════════════════════════════════════════════════════════════════╣';
PRINT '║  Dacă Test 2 returnează 0 records dar tabela are date:         ║';
PRINT '║  → SP-ul ARE BUG de NULL handling                               ║';
PRINT '║  → Aplică: MASTER_FIX_AdministrarePacienti_NULL_Handling.sql   ║';
PRINT '║                                                                  ║';
PRINT '║  Dacă Test 2 returnează records:                                ║';
PRINT '║  → SP-ul funcționează CORECT                                    ║';
PRINT '║  → Problema este în C# (Dapper mapping sau connection)         ║';
PRINT '╚════════════════════════════════════════════════════════════════╝';

sp_helptext 'sp_Pacienti_GetAll';
GO
