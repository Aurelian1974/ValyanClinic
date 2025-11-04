-- ========================================
-- Stored Procedure: sp_Personal_CheckUnique
-- Database: ValyanMed
-- Created: 09/14/2025 16:56:11
-- Modified: 09/14/2025 16:56:11
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


CREATE PROCEDURE sp_Personal_CheckUnique
    @CNP NVARCHAR(13),
    @Cod_Angajat NVARCHAR(50),
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CNP_Exists BIT = 0;
    DECLARE @CodAngajat_Exists BIT = 0;
    
    -- Check CNP
    IF EXISTS (
        SELECT 1 FROM Personal 
        WHERE CNP = @CNP 
        AND (@ExcludeId IS NULL OR Id_Personal <> @ExcludeId)
    )
        SET @CNP_Exists = 1;
    
    -- Check Cod Angajat
    IF EXISTS (
        SELECT 1 FROM Personal 
        WHERE Cod_Angajat = @Cod_Angajat 
        AND (@ExcludeId IS NULL OR Id_Personal <> @ExcludeId)
    )
        SET @CodAngajat_Exists = 1;
    
    SELECT @CNP_Exists AS CNP_Exists, @CodAngajat_Exists AS CodAngajat_Exists;
END

GO
