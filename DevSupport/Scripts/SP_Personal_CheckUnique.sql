-- SP pentru verificarea unicit??ii CNP ?i Cod_Angajat
CREATE PROCEDURE [dbo].[sp_Personal_CheckUnique]
    @CNP VARCHAR(13) = NULL,
    @Cod_Angajat VARCHAR(20) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CASE WHEN EXISTS (
            SELECT 1 FROM Personal 
            WHERE CNP = @CNP 
            AND (@ExcludeId IS NULL OR Id_Personal != @ExcludeId)
        ) THEN 1 ELSE 0 END as CNP_Exists,
        
        CASE WHEN EXISTS (
            SELECT 1 FROM Personal 
            WHERE Cod_Angajat = @Cod_Angajat 
            AND (@ExcludeId IS NULL OR Id_Personal != @ExcludeId)
        ) THEN 1 ELSE 0 END as CodAngajat_Exists;
END;