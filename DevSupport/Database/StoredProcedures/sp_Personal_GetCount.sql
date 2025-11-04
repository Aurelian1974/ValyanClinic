-- ========================================
-- Stored Procedure: sp_Personal_GetCount
-- Database: ValyanMed
-- Created: 10/12/2025 09:05:43
-- Modified: 10/12/2025 09:05:43
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


CREATE PROCEDURE sp_Personal_GetCount
    @SearchText NVARCHAR(255) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Status NVARCHAR(50) = NULL,
    @Functie NVARCHAR(150) = NULL,
    @Judet NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- ✅ PARAMETERIZED QUERY - NO DYNAMIC SQL NEEDED
    SELECT COUNT(*) AS TotalCount
    FROM Personal
    WHERE 1=1
        AND (@SearchText IS NULL OR (
            Nume LIKE '%' + @SearchText + '%' 
            OR Prenume LIKE '%' + @SearchText + '%'
            OR Cod_Angajat LIKE '%' + @SearchText + '%'
            OR CNP LIKE '%' + @SearchText + '%'
            OR Telefon_Personal LIKE '%' + @SearchText + '%'
            OR Email_Personal LIKE '%' + @SearchText + '%'
            OR Functia LIKE '%' + @SearchText + '%'
            OR Departament LIKE '%' + @SearchText + '%'
        ))
        AND (@Departament IS NULL OR Departament = @Departament)
        AND (@Status IS NULL OR Status_Angajat = @Status)
        AND (@Functie IS NULL OR Functia = @Functie)
        AND (@Judet IS NULL OR Judet_Domiciliu = @Judet);
END

GO
