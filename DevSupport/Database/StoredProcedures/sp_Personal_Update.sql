-- ========================================
-- Stored Procedure: sp_Personal_Update
-- Database: ValyanMed
-- Created: 09/16/2025 08:50:17
-- Modified: 09/16/2025 08:50:17
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

-- SP pentru actualizarea unei persoane
CREATE PROCEDURE [dbo].[sp_Personal_Update]
    @Id_Personal UNIQUEIDENTIFIER,
    @Cod_Angajat VARCHAR(20),
    @CNP VARCHAR(13),
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Nume_Anterior NVARCHAR(100) = NULL,
    @Data_Nasterii DATE,
    @Locul_Nasterii NVARCHAR(200) = NULL,
    @Nationalitate NVARCHAR(50) = 'Romana',
    @Cetatenie NVARCHAR(50) = 'Romana',
    @Telefon_Personal VARCHAR(20) = NULL,
    @Telefon_Serviciu VARCHAR(20) = NULL,
    @Email_Personal VARCHAR(100) = NULL,
    @Email_Serviciu VARCHAR(100) = NULL,
    @Adresa_Domiciliu NVARCHAR(MAX),
    @Judet_Domiciliu NVARCHAR(50),
    @Oras_Domiciliu NVARCHAR(100),
    @Cod_Postal_Domiciliu VARCHAR(10) = NULL,
    @Adresa_Resedinta NVARCHAR(MAX) = NULL,
    @Judet_Resedinta NVARCHAR(50) = NULL,
    @Oras_Resedinta NVARCHAR(100) = NULL,
    @Cod_Postal_Resedinta VARCHAR(10) = NULL,
    @Stare_Civila NVARCHAR(100) = NULL,
    @Functia NVARCHAR(100),
    @Departament NVARCHAR(100) = NULL,
    @Serie_CI VARCHAR(10) = NULL,
    @Numar_CI VARCHAR(20) = NULL,
    @Eliberat_CI_De NVARCHAR(100) = NULL,
    @Data_Eliberare_CI DATE = NULL,
    @Valabil_CI_Pana DATE = NULL,
    @Status_Angajat NVARCHAR(50) = 'Activ',
    @Observatii NVARCHAR(MAX) = NULL,
    @Modificat_De NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM Personal WHERE Id_Personal = @Id_Personal)
        BEGIN
            THROW 50003, 'Persoana nu a fost gasita.', 1;
        END
        
        -- Verificare unicitate CNP si Cod_Angajat (exclude ID-ul curent)
        IF EXISTS (SELECT 1 FROM Personal WHERE CNP = @CNP AND Id_Personal != @Id_Personal)
        BEGIN
            THROW 50001, 'CNP-ul exista deja in baza de date.', 1;
        END
        
        IF EXISTS (SELECT 1 FROM Personal WHERE Cod_Angajat = @Cod_Angajat AND Id_Personal != @Id_Personal)
        BEGIN
            THROW 50002, 'Codul de angajat exista deja in baza de date.', 1;
        END
        
        UPDATE Personal SET
            Cod_Angajat = @Cod_Angajat,
            CNP = @CNP,
            Nume = @Nume,
            Prenume = @Prenume,
            Nume_Anterior = @Nume_Anterior,
            Data_Nasterii = @Data_Nasterii,
            Locul_Nasterii = @Locul_Nasterii,
            Nationalitate = @Nationalitate,
            Cetatenie = @Cetatenie,
            Telefon_Personal = @Telefon_Personal,
            Telefon_Serviciu = @Telefon_Serviciu,
            Email_Personal = @Email_Personal,
            Email_Serviciu = @Email_Serviciu,
            Adresa_Domiciliu = @Adresa_Domiciliu,
            Judet_Domiciliu = @Judet_Domiciliu,
            Oras_Domiciliu = @Oras_Domiciliu,
            Cod_Postal_Domiciliu = @Cod_Postal_Domiciliu,
            Adresa_Resedinta = @Adresa_Resedinta,
            Judet_Resedinta = @Judet_Resedinta,
            Oras_Resedinta = @Oras_Resedinta,
            Cod_Postal_Resedinta = @Cod_Postal_Resedinta,
            Stare_Civila = @Stare_Civila,
            Functia = @Functia,
            Departament = @Departament,
            Serie_CI = @Serie_CI,
            Numar_CI = @Numar_CI,
            Eliberat_CI_De = @Eliberat_CI_De,
            Data_Eliberare_CI = @Data_Eliberare_CI,
            Valabil_CI_Pana = @Valabil_CI_Pana,
            Status_Angajat = @Status_Angajat,
            Observatii = @Observatii,
            Data_Ultimei_Modificari = GETDATE(), -- CORECTAT: folose?te ora local? �n loc de UTC
            Modificat_De = @Modificat_De
        WHERE Id_Personal = @Id_Personal;
        
        COMMIT TRANSACTION;
        
        -- Returnare persoana actualizata
        EXEC sp_Personal_GetById @Id_Personal;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
