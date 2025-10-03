-- SP pentru obtinerea tuturor judetelor
CREATE OR ALTER PROCEDURE [dbo].[sp_Judete_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdJudet,
        JudetGuid,
        CodJudet,
        Nume,
        Siruta,
        CodAuto,
        Ordine
    FROM Judet
    ORDER BY Ordine ASC, Nume ASC;
END;

GO

-- SP pentru obtinerea judetelor ordonate dupa nume
CREATE OR ALTER PROCEDURE [dbo].[sp_Judete_GetOrderedByName]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdJudet,
        JudetGuid,
        CodJudet,
        Nume,
        Siruta,
        CodAuto,
        Ordine
    FROM Judet
    ORDER BY Nume ASC;
END;

GO

-- SP pentru obtinerea unui judet dupa ID
CREATE OR ALTER PROCEDURE [dbo].[sp_Judete_GetById]
    @IdJudet INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdJudet,
        JudetGuid,
        CodJudet,
        Nume,
        Siruta,
        CodAuto,
        Ordine
    FROM Judet
    WHERE IdJudet = @IdJudet;
END;

GO

-- SP pentru obtinerea unui judet dupa cod
CREATE OR ALTER PROCEDURE [dbo].[sp_Judete_GetByCod]
    @CodJudet NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdJudet,
        JudetGuid,
        CodJudet,
        Nume,
        Siruta,
        CodAuto,
        Ordine
    FROM Judet
    WHERE CodJudet = @CodJudet;
END;

GO