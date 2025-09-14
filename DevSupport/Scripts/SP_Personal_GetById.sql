-- SP pentru obtinerea unei persoane dupa ID
CREATE PROCEDURE [dbo].[sp_Personal_GetById]
    @Id_Personal UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id_Personal,
        Cod_Angajat,
        CNP,
        Nume,
        Prenume,
        Nume_Anterior,
        Data_Nasterii,
        Locul_Nasterii,
        Nationalitate,
        Cetatenie,
        Telefon_Personal,
        Telefon_Serviciu,
        Email_Personal,
        Email_Serviciu,
        Adresa_Domiciliu,
        Judet_Domiciliu,
        Oras_Domiciliu,
        Cod_Postal_Domiciliu,
        Adresa_Resedinta,
        Judet_Resedinta,
        Oras_Resedinta,
        Cod_Postal_Resedinta,
        Stare_Civila,
        Functia,
        Departament,
        Serie_CI,
        Numar_CI,
        Eliberat_CI_De,
        Data_Eliberare_CI,
        Valabil_CI_Pana,
        Status_Angajat,
        Observatii,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Personal 
    WHERE Id_Personal = @Id_Personal;
END;