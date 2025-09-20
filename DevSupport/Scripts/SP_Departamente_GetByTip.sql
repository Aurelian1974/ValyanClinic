-- =============================================
-- SP pentru obtinerea departamentelor din tabela Departamente
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