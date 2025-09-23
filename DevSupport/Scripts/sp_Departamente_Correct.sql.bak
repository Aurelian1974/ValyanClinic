-- =============================================
-- SP pentru ob?inerea departamentelor din tabela Departamente
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Departamente_GetByTip]
    @Tip NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        DepartamentID,
        Nume,
        Tip
    FROM Departamente 
    WHERE Tip = @Tip
    ORDER BY Nume;
END;
GO

-- =============================================
-- SP pentru ob?inerea tuturor departamentelor
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Departamente_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        DepartamentID,
        Nume,
        Tip
    FROM Departamente 
    ORDER BY Nume;
END;
GO