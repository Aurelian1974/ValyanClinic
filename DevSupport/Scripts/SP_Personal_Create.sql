-- SP pentru crearea unei noi persoane
CREATE PROCEDURE [dbo].[sp_Personal_Create]
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
    @Creat_De NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
        
        -- Verificare unicitate CNP si Cod_Angajat
        IF EXISTS (SELECT 1 FROM Personal WHERE CNP = @CNP)
        BEGIN
            THROW 50001, 'CNP-ul exista deja in baza de date.', 1;
        END
        
        IF EXISTS (SELECT 1 FROM Personal WHERE Cod_Angajat = @Cod_Angajat)
        BEGIN
            THROW 50002, 'Codul de angajat exista deja in baza de date.', 1;
        END
        
        INSERT INTO Personal (
            Id_Personal, Cod_Angajat, CNP, Nume, Prenume, Nume_Anterior,
            Data_Nasterii, Locul_Nasterii, Nationalitate, Cetatenie,
            Telefon_Personal, Telefon_Serviciu, Email_Personal, Email_Serviciu,
            Adresa_Domiciliu, Judet_Domiciliu, Oras_Domiciliu, Cod_Postal_Domiciliu,
            Adresa_Resedinta, Judet_Resedinta, Oras_Resedinta, Cod_Postal_Resedinta,
            Stare_Civila, Functia, Departament,
            Serie_CI, Numar_CI, Eliberat_CI_De, Data_Eliberare_CI, Valabil_CI_Pana,
            Status_Angajat, Observatii, Creat_De
        ) VALUES (
            @NewId, @Cod_Angajat, @CNP, @Nume, @Prenume, @Nume_Anterior,
            @Data_Nasterii, @Locul_Nasterii, @Nationalitate, @Cetatenie,
            @Telefon_Personal, @Telefon_Serviciu, @Email_Personal, @Email_Serviciu,
            @Adresa_Domiciliu, @Judet_Domiciliu, @Oras_Domiciliu, @Cod_Postal_Domiciliu,
            @Adresa_Resedinta, @Judet_Resedinta, @Oras_Resedinta, @Cod_Postal_Resedinta,
            @Stare_Civila, @Functia, @Departament,
            @Serie_CI, @Numar_CI, @Eliberat_CI_De, @Data_Eliberare_CI, @Valabil_CI_Pana,
            @Status_Angajat, @Observatii, @Creat_De
        );
        
        COMMIT TRANSACTION;
        
        -- Returnare persoana creata
        EXEC sp_Personal_GetById @NewId;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;