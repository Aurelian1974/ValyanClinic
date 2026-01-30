-- =============================================
-- Funcție: fn_CalculateAgeFromCNP
-- Descriere: Calculează vârsta din CNP-ul românesc
-- Data: 2026-01-07
-- =============================================
-- Această funcție extrage data nașterii din CNP și calculează vârsta
-- CNP format: S AA LL ZZ JJ NNN C
--   S = cifra sex (1-9)
--   AA = anul nașterii (2 cifre)
--   LL = luna nașterii (01-12)
--   ZZ = ziua nașterii (01-31)
--   JJ = codul județului
--   NNN = număr ordine
--   C = cifră de control
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fn_CalculateAgeFromCNP]') AND type IN (N'FN', N'IF', N'TF', N'FS', N'FT'))
    DROP FUNCTION [dbo].[fn_CalculateAgeFromCNP];
GO

CREATE FUNCTION [dbo].[fn_CalculateAgeFromCNP]
(
    @CNP NVARCHAR(13)
)
RETURNS INT
AS
BEGIN
    DECLARE @Age INT = 0;
    DECLARE @SexDigit INT;
    DECLARE @Year INT;
    DECLARE @Month INT;
    DECLARE @Day INT;
    DECLARE @Century INT;
    DECLARE @BirthYear INT;
    DECLARE @BirthDate DATE;

    -- Validare CNP
    IF @CNP IS NULL OR LEN(@CNP) != 13 OR @CNP LIKE '%[^0-9]%'
        RETURN 0;

    -- Extragere componente
    SET @SexDigit = CAST(SUBSTRING(@CNP, 1, 1) AS INT);
    SET @Year = CAST(SUBSTRING(@CNP, 2, 2) AS INT);
    SET @Month = CAST(SUBSTRING(@CNP, 4, 2) AS INT);
    SET @Day = CAST(SUBSTRING(@CNP, 6, 2) AS INT);

    -- Validare componente
    IF @Month < 1 OR @Month > 12
        RETURN 0;
    IF @Day < 1 OR @Day > 31
        RETURN 0;

    -- Determinare secol pe baza cifrei de sex
    SET @Century = CASE @SexDigit
        WHEN 1 THEN 1900  -- Bărbat născut 1900-1999
        WHEN 2 THEN 1900  -- Femeie născută 1900-1999
        WHEN 3 THEN 1800  -- Bărbat născut 1800-1899
        WHEN 4 THEN 1800  -- Femeie născută 1800-1899
        WHEN 5 THEN 2000  -- Bărbat născut 2000-2099
        WHEN 6 THEN 2000  -- Femeie născută 2000-2099
        WHEN 7 THEN 2000  -- Rezident străin bărbat
        WHEN 8 THEN 2000  -- Rezident străin femeie
        WHEN 9 THEN 1900  -- Cetățean străin
        ELSE 2000
    END;

    SET @BirthYear = @Century + @Year;

    -- Validare an rezultat (între 1900 și anul curent + 1)
    IF @BirthYear < 1800 OR @BirthYear > YEAR(GETDATE()) + 1
        RETURN 0;

    -- Validare zi pentru luna respectivă (simplificată)
    IF @Day > 31 OR (@Month IN (4, 6, 9, 11) AND @Day > 30) OR (@Month = 2 AND @Day > 29)
        RETURN 0;

    -- Construire data nașterii
    SET @BirthDate = DATEFROMPARTS(@BirthYear, @Month, @Day);

    -- Calcul vârstă
    SET @Age = DATEDIFF(YEAR, @BirthDate, GETDATE());
    
    -- Ajustare dacă ziua de naștere nu a trecut încă anul acesta
    IF DATEADD(YEAR, @Age, @BirthDate) > GETDATE()
        SET @Age = @Age - 1;

    RETURN @Age;
END
GO

-- Test funcție
PRINT '✓ Funcție fn_CalculateAgeFromCNP creată cu succes';
PRINT '';
PRINT 'Test funcție:';
PRINT '  CNP 1740301080012 (născut 01.03.1974): ' + CAST(dbo.fn_CalculateAgeFromCNP('1740301080012') AS VARCHAR);
PRINT '  CNP 2851015123456 (născut 15.10.1985): ' + CAST(dbo.fn_CalculateAgeFromCNP('2851015123456') AS VARCHAR);
PRINT '  CNP 5100520123456 (născut 20.05.2010): ' + CAST(dbo.fn_CalculateAgeFromCNP('5100520123456') AS VARCHAR);
GO
