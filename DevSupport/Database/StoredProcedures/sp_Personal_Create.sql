-- ========================================
-- Stored Procedure: sp_Personal_Create
-- Database: ValyanMed
-- Created: 10/12/2025 09:05:43
-- Modified: 10/12/2025 09:05:43
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO


CREATE PROCEDURE sp_Personal_Create
    @Id_Personal UNIQUEIDENTIFIER = NULL,
    @Cod_Angajat NVARCHAR(50),
    @CNP NVARCHAR(13),
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Nume_Anterior NVARCHAR(100) = NULL,
    @Data_Nasterii DATETIME,
    @Locul_Nasterii NVARCHAR(100) = NULL,
    @Nationalitate NVARCHAR(50) = 'Romana',
    @Cetatenie NVARCHAR(50) = 'Romana',
    @Telefon_Personal NVARCHAR(20) = NULL,
    @Telefon_Serviciu NVARCHAR(20) = NULL,
    @Email_Personal NVARCHAR(255) = NULL,
    @Email_Serviciu NVARCHAR(255) = NULL,
    @Adresa_Domiciliu NVARCHAR(500),
    @Judet_Domiciliu NVARCHAR(100),
    @Oras_Domiciliu NVARCHAR(100),
    @Cod_Postal_Domiciliu NVARCHAR(10) = NULL,
    @Adresa_Resedinta NVARCHAR(500) = NULL,
    @Judet_Resedinta NVARCHAR(100) = NULL,
    @Oras_Resedinta NVARCHAR(100) = NULL,
    @Cod_Postal_Resedinta NVARCHAR(10) = NULL,
    @Stare_Civila NVARCHAR(50) = NULL,
    @Functia NVARCHAR(150),
    @Departament NVARCHAR(100) = NULL,
    @Serie_CI NVARCHAR(10) = NULL,
    @Numar_CI NVARCHAR(20) = NULL,
    @Eliberat_CI_De NVARCHAR(100) = NULL,
    @Data_Eliberare_CI DATETIME = NULL,
    @Valabil_CI_Pana DATETIME = NULL,
    @Status_Angajat NVARCHAR(50) = 'Activ',
    @Observatii NVARCHAR(MAX) = NULL,
    @Creat_De NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Generate GUID if not provided
    IF @Id_Personal IS NULL OR @Id_Personal = '00000000-0000-0000-0000-000000000000'
        SET @Id_Personal = NEWID();
        
    DECLARE @Data_Crearii DATETIME = GETUTCDATE();
    
    -- ✅ DIRECT INSERT - NO DYNAMIC SQL
    INSERT INTO Personal (
        Id_Personal, Cod_Angajat, CNP, Nume, Prenume, Nume_Anterior,
        Data_Nasterii, Locul_Nasterii, Nationalitate, Cetatenie,
        Telefon_Personal, Telefon_Serviciu, Email_Personal, Email_Serviciu,
        Adresa_Domiciliu, Judet_Domiciliu, Oras_Domiciliu, Cod_Postal_Domiciliu,
        Adresa_Resedinta, Judet_Resedinta, Oras_Resedinta, Cod_Postal_Resedinta,
        Stare_Civila, Functia, Departament, Serie_CI, Numar_CI, Eliberat_CI_De,
        Data_Eliberare_CI, Valabil_CI_Pana, Status_Angajat, Observatii,
        Data_Crearii, Data_Ultimei_Modificari, Creat_De, Modificat_De
    ) VALUES (
        @Id_Personal, @Cod_Angajat, @CNP, @Nume, @Prenume, @Nume_Anterior,
        @Data_Nasterii, @Locul_Nasterii, @Nationalitate, @Cetatenie,
        @Telefon_Personal, @Telefon_Serviciu, @Email_Personal, @Email_Serviciu,
        @Adresa_Domiciliu, @Judet_Domiciliu, @Oras_Domiciliu, @Cod_Postal_Domiciliu,
        @Adresa_Resedinta, @Judet_Resedinta, @Oras_Resedinta, @Cod_Postal_Resedinta,
        @Stare_Civila, @Functia, @Departament, @Serie_CI, @Numar_CI, @Eliberat_CI_De,
        @Data_Eliberare_CI, @Valabil_CI_Pana, @Status_Angajat, @Observatii,
        @Data_Crearii, @Data_Crearii, @Creat_De, @Creat_De
    );
    
    -- Return created record
    SELECT 
        Id_Personal, Cod_Angajat, CNP, Nume, Prenume, Nume_Anterior,
        Data_Nasterii, Locul_Nasterii, Nationalitate, Cetatenie,
        Telefon_Personal, Telefon_Serviciu, Email_Personal, Email_Serviciu,
        Adresa_Domiciliu, Judet_Domiciliu, Oras_Domiciliu, Cod_Postal_Domiciliu,
        Adresa_Resedinta, Judet_Resedinta, Oras_Resedinta, Cod_Postal_Resedinta,
        Stare_Civila, Functia, Departament, Serie_CI, Numar_CI, Eliberat_CI_De,
        Data_Eliberare_CI, Valabil_CI_Pana, Status_Angajat, Observatii,
        Data_Crearii, Data_Ultimei_Modificari, Creat_De, Modificat_De
    FROM Personal 
    WHERE Id_Personal = @Id_Personal;
END

GO
