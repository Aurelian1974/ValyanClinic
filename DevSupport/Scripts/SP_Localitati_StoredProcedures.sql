-- SP pentru obtinerea tuturor localitatilor
CREATE OR ALTER PROCEDURE [dbo].[sp_Localitati_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdOras,
        LocalitateGuid,
        IdJudet,
        Nume,
        Siruta,
        IdTipLocalitate,
        CodLocalitate
    FROM Localitate
    ORDER BY Nume ASC;
END;

GO

-- SP pentru obtinerea unei localitati dupa ID
CREATE OR ALTER PROCEDURE [dbo].[sp_Localitati_GetById]
    @IdOras INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdOras,
        LocalitateGuid,
        IdJudet,
        Nume,
        Siruta,
        IdTipLocalitate,
        CodLocalitate
    FROM Localitate
    WHERE IdOras = @IdOras;
END;

GO

-- SP pentru obtinerea localitatilor dintr-un judet
CREATE OR ALTER PROCEDURE [dbo].[sp_Localitati_GetByJudetId]
    @IdJudet INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdOras,
        LocalitateGuid,
        IdJudet,
        Nume,
        Siruta,
        IdTipLocalitate,
        CodLocalitate
    FROM Localitate
    WHERE IdJudet = @IdJudet;
END;

GO

-- SP pentru obtinerea localitatilor dintr-un judet ordonate dupa nume
CREATE OR ALTER PROCEDURE [dbo].[sp_Localitati_GetByJudetIdOrdered]
    @IdJudet INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdOras,
        LocalitateGuid,
        IdJudet,
        Nume,
        Siruta,
        IdTipLocalitate,
        CodLocalitate
    FROM Localitate
    WHERE IdJudet = @IdJudet
    ORDER BY Nume ASC;
END;

GO