-- ========================================
-- Stored Procedures pentru Pacienti
-- Database: ValyanMed
-- Creat: 2025-01-23
-- ========================================

USE [ValyanMed]
GO

-- ============================================================================
-- 1. sp_Pacienti_GetAll - Obtinere lista completa cu filtrare si sortare
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetAll')
    DROP PROCEDURE sp_Pacienti_GetAll
GO

CREATE PROCEDURE sp_Pacienti_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SearchText NVARCHAR(255) = NULL,
    @Judet NVARCHAR(50) = NULL,
    @Asigurat BIT = NULL,
    @Activ BIT = NULL,
    @SortColumn NVARCHAR(50) = 'Nume',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate sort direction
    IF @SortDirection NOT IN ('ASC', 'DESC')
        SET @SortDirection = 'ASC';
    
    -- Whitelist for sort columns
    IF @SortColumn NOT IN ('Nume', 'Prenume', 'CNP', 'Cod_Pacient', 'Data_Nasterii', 'Data_Inregistrare', 'Ultima_Vizita', 'Data_Crearii')
        SET @SortColumn = 'Nume';
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id,
        Cod_Pacient,
        CNP,
        Nume,
        Prenume,
        Data_Nasterii,
        Sex,
        Telefon,
        Telefon_Secundar,
        Email,
        Judet,
        Localitate,
        Adresa,
        Cod_Postal,
        Asigurat,
        CNP_Asigurat,
        Nr_Card_Sanatate,
        Casa_Asigurari,
        Alergii,
        Boli_Cronice,
        Medic_Familie,
        Persoana_Contact,
        Telefon_Urgenta,
        Relatie_Contact,
        Data_Inregistrare,
        Ultima_Vizita,
        Nr_Total_Vizite,
        Activ,
        Observatii,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Pacienti
    WHERE 1=1
        AND (@SearchText IS NULL OR 
             Nume LIKE '%' + @SearchText + '%' OR 
             Prenume LIKE '%' + @SearchText + '%' OR
             CNP LIKE '%' + @SearchText + '%' OR
             Cod_Pacient LIKE '%' + @SearchText + '%' OR
             Telefon LIKE '%' + @SearchText + '%' OR
             Email LIKE '%' + @SearchText + '%')
        AND (@Judet IS NULL OR Judet = @Judet)
        AND (@Asigurat IS NULL OR Asigurat = @Asigurat)
        AND (@Activ IS NULL OR Activ = @Activ)
    ORDER BY 
        CASE WHEN @SortColumn = 'Nume' AND @SortDirection = 'ASC' THEN Nume END ASC,
        CASE WHEN @SortColumn = 'Nume' AND @SortDirection = 'DESC' THEN Nume END DESC,
        CASE WHEN @SortColumn = 'Prenume' AND @SortDirection = 'ASC' THEN Prenume END ASC,
        CASE WHEN @SortColumn = 'Prenume' AND @SortDirection = 'DESC' THEN Prenume END DESC,
        CASE WHEN @SortColumn = 'CNP' AND @SortDirection = 'ASC' THEN CNP END ASC,
        CASE WHEN @SortColumn = 'CNP' AND @SortDirection = 'DESC' THEN CNP END DESC,
        CASE WHEN @SortColumn = 'Cod_Pacient' AND @SortDirection = 'ASC' THEN Cod_Pacient END ASC,
        CASE WHEN @SortColumn = 'Cod_Pacient' AND @SortDirection = 'DESC' THEN Cod_Pacient END DESC,
        CASE WHEN @SortColumn = 'Data_Nasterii' AND @SortDirection = 'ASC' THEN Data_Nasterii END ASC,
        CASE WHEN @SortColumn = 'Data_Nasterii' AND @SortDirection = 'DESC' THEN Data_Nasterii END DESC,
        CASE WHEN @SortColumn = 'Data_Inregistrare' AND @SortDirection = 'ASC' THEN Data_Inregistrare END ASC,
        CASE WHEN @SortColumn = 'Data_Inregistrare' AND @SortDirection = 'DESC' THEN Data_Inregistrare END DESC,
        CASE WHEN @SortColumn = 'Ultima_Vizita' AND @SortDirection = 'ASC' THEN Ultima_Vizita END ASC,
        CASE WHEN @SortColumn = 'Ultima_Vizita' AND @SortDirection = 'DESC' THEN Ultima_Vizita END DESC,
        CASE WHEN @SortColumn = 'Data_Crearii' AND @SortDirection = 'ASC' THEN Data_Crearii END ASC,
        CASE WHEN @SortColumn = 'Data_Crearii' AND @SortDirection = 'DESC' THEN Data_Crearii END DESC
    OFFSET @Offset ROWS 
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ============================================================================
-- 2. sp_Pacienti_GetCount - Obtinere numar total cu filtrare
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetCount')
    DROP PROCEDURE sp_Pacienti_GetCount
GO

CREATE PROCEDURE sp_Pacienti_GetCount
    @SearchText NVARCHAR(255) = NULL,
    @Judet NVARCHAR(50) = NULL,
    @Asigurat BIT = NULL,
    @Activ BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS TotalCount
    FROM Pacienti
    WHERE 1=1
        AND (@SearchText IS NULL OR 
             Nume LIKE '%' + @SearchText + '%' OR 
             Prenume LIKE '%' + @SearchText + '%' OR
             CNP LIKE '%' + @SearchText + '%' OR
             Cod_Pacient LIKE '%' + @SearchText + '%' OR
             Telefon LIKE '%' + @SearchText + '%' OR
             Email LIKE '%' + @SearchText + '%')
        AND (@Judet IS NULL OR Judet = @Judet)
        AND (@Asigurat IS NULL OR Asigurat = @Asigurat)
        AND (@Activ IS NULL OR Activ = @Activ);
END
GO

-- ============================================================================
-- 3. sp_Pacienti_GetById - Obtinere pacient dupa ID
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetById')
    DROP PROCEDURE sp_Pacienti_GetById
GO

CREATE PROCEDURE sp_Pacienti_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Cod_Pacient,
        CNP,
        Nume,
        Prenume,
        Data_Nasterii,
        Sex,
        Telefon,
        Telefon_Secundar,
        Email,
        Judet,
        Localitate,
        Adresa,
        Cod_Postal,
        Asigurat,
        CNP_Asigurat,
        Nr_Card_Sanatate,
        Casa_Asigurari,
        Alergii,
        Boli_Cronice,
        Medic_Familie,
        Persoana_Contact,
        Telefon_Urgenta,
        Relatie_Contact,
        Data_Inregistrare,
        Ultima_Vizita,
        Nr_Total_Vizite,
        Activ,
        Observatii,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Pacienti 
    WHERE Id = @Id;
END
GO

-- ============================================================================
-- 4. sp_Pacienti_GetByCodPacient - Obtinere pacient dupa Cod Pacient
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetByCodPacient')
    DROP PROCEDURE sp_Pacienti_GetByCodPacient
GO

CREATE PROCEDURE sp_Pacienti_GetByCodPacient
    @CodPacient NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Cod_Pacient,
        CNP,
        Nume,
        Prenume,
        Data_Nasterii,
        Sex,
        Telefon,
        Telefon_Secundar,
        Email,
        Judet,
        Localitate,
        Adresa,
        Cod_Postal,
        Asigurat,
        CNP_Asigurat,
        Nr_Card_Sanatate,
        Casa_Asigurari,
        Alergii,
        Boli_Cronice,
        Medic_Familie,
        Persoana_Contact,
        Telefon_Urgenta,
        Relatie_Contact,
        Data_Inregistrare,
        Ultima_Vizita,
        Nr_Total_Vizite,
        Activ,
        Observatii,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Pacienti 
    WHERE Cod_Pacient = @CodPacient;
END
GO

-- ============================================================================
-- 5. sp_Pacienti_GetByCNP - Obtinere pacient dupa CNP
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetByCNP')
    DROP PROCEDURE sp_Pacienti_GetByCNP
GO

CREATE PROCEDURE sp_Pacienti_GetByCNP
    @CNP NVARCHAR(13)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Cod_Pacient,
        CNP,
        Nume,
        Prenume,
        Data_Nasterii,
        Sex,
        Telefon,
        Telefon_Secundar,
        Email,
        Judet,
        Localitate,
        Adresa,
        Cod_Postal,
        Asigurat,
        CNP_Asigurat,
        Nr_Card_Sanatate,
        Casa_Asigurari,
        Alergii,
        Boli_Cronice,
        Medic_Familie,
        Persoana_Contact,
        Telefon_Urgenta,
        Relatie_Contact,
        Data_Inregistrare,
        Ultima_Vizita,
        Nr_Total_Vizite,
        Activ,
        Observatii,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Pacienti 
    WHERE CNP = @CNP;
END
GO

-- ============================================================================
-- 6. sp_Pacienti_GetJudete - Obtinere lista judete unice
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetJudete')
    DROP PROCEDURE sp_Pacienti_GetJudete
GO

CREATE PROCEDURE sp_Pacienti_GetJudete
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT
        Judet AS Value,
        Judet AS Text,
        COUNT(*) AS NumarPacienti
    FROM Pacienti
    WHERE Judet IS NOT NULL
      AND Activ = 1
    GROUP BY Judet
    ORDER BY Judet ASC;
END
GO

-- ============================================================================
-- 7. sp_Pacienti_GetDropdownOptions - Optiuni pentru dropdown-uri
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetDropdownOptions')
    DROP PROCEDURE sp_Pacienti_GetDropdownOptions
GO

CREATE PROCEDURE sp_Pacienti_GetDropdownOptions
    @Activ BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CAST(Id AS NVARCHAR(36)) AS Value,
        Nume + ' ' + Prenume + ' (' + Cod_Pacient + ')' AS Text,
        CNP,
        Data_Nasterii
    FROM Pacienti 
    WHERE Activ = @Activ
    ORDER BY Nume ASC, Prenume ASC;
END
GO

-- ============================================================================
-- 8. sp_Pacienti_GenerateNextCodPacient - Genereaza urmatorul cod pacient
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GenerateNextCodPacient')
    DROP PROCEDURE sp_Pacienti_GenerateNextCodPacient
GO

CREATE PROCEDURE sp_Pacienti_GenerateNextCodPacient
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NextNumber INT;
    DECLARE @NextCod NVARCHAR(20);
    
    -- Obtine numarul maxim din codurile existente
    SELECT @NextNumber = ISNULL(MAX(CAST(SUBSTRING(Cod_Pacient, 8, 8) AS INT)), 0) + 1
    FROM Pacienti
    WHERE Cod_Pacient LIKE 'PACIENT%';
    
    -- Genereaza codul cu format PACIENT00000001
    SET @NextCod = 'PACIENT' + RIGHT('00000000' + CAST(@NextNumber AS NVARCHAR(8)), 8);
    
    SELECT @NextCod AS NextCodPacient;
END
GO

-- ============================================================================
-- 9. sp_Pacienti_Create - Creare pacient nou
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_Create')
    DROP PROCEDURE sp_Pacienti_Create
GO

CREATE PROCEDURE sp_Pacienti_Create
    @Cod_Pacient NVARCHAR(20) = NULL,
    @CNP NVARCHAR(13) = NULL,
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Data_Nasterii DATE,
    @Sex NVARCHAR(1),
    @Telefon NVARCHAR(15) = NULL,
    @Telefon_Secundar NVARCHAR(15) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Judet NVARCHAR(50) = NULL,
    @Localitate NVARCHAR(100) = NULL,
    @Adresa NVARCHAR(255) = NULL,
    @Cod_Postal NVARCHAR(10) = NULL,
    @Asigurat BIT = 0,
    @CNP_Asigurat NVARCHAR(13) = NULL,
    @Nr_Card_Sanatate NVARCHAR(20) = NULL,
    @Casa_Asigurari NVARCHAR(100) = NULL,
    @Alergii NVARCHAR(MAX) = NULL,
    @Boli_Cronice NVARCHAR(MAX) = NULL,
    @Medic_Familie NVARCHAR(150) = NULL,
    @Persoana_Contact NVARCHAR(150) = NULL,
    @Telefon_Urgenta NVARCHAR(15) = NULL,
    @Relatie_Contact NVARCHAR(50) = NULL,
    @Activ BIT = 1,
    @Observatii NVARCHAR(MAX) = NULL,
    @CreatDe NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Genereaza Cod_Pacient daca nu este furnizat
        IF @Cod_Pacient IS NULL OR @Cod_Pacient = ''
        BEGIN
            DECLARE @NextNumber INT;
            SELECT @NextNumber = ISNULL(MAX(CAST(SUBSTRING(Cod_Pacient, 8, 8) AS INT)), 0) + 1
            FROM Pacienti
            WHERE Cod_Pacient LIKE 'PACIENT%';
            
            SET @Cod_Pacient = 'PACIENT' + RIGHT('00000000' + CAST(@NextNumber AS NVARCHAR(8)), 8);
        END
        
        -- Verificare duplicat Cod_Pacient
        IF EXISTS (SELECT 1 FROM Pacienti WHERE Cod_Pacient = @Cod_Pacient)
        BEGIN
            THROW 50001, 'Un pacient cu acest cod exista deja.', 1;
        END
        
        -- Verificare duplicat CNP (daca este furnizat)
        IF @CNP IS NOT NULL AND EXISTS (SELECT 1 FROM Pacienti WHERE CNP = @CNP)
        BEGIN
            THROW 50002, 'Un pacient cu acest CNP exista deja.', 1;
        END
        
        DECLARE @NewId UNIQUEIDENTIFIER;
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        DECLARE @OutputTable TABLE (Id UNIQUEIDENTIFIER);
        
        -- Insert cu OUTPUT pentru a captura ID-ul generat
        INSERT INTO Pacienti (
            Cod_Pacient,
            CNP,
            Nume,
            Prenume,
            Data_Nasterii,
            Sex,
            Telefon,
            Telefon_Secundar,
            Email,
            Judet,
            Localitate,
            Adresa,
            Cod_Postal,
            Asigurat,
            CNP_Asigurat,
            Nr_Card_Sanatate,
            Casa_Asigurari,
            Alergii,
            Boli_Cronice,
            Medic_Familie,
            Persoana_Contact,
            Telefon_Urgenta,
            Relatie_Contact,
            Data_Inregistrare,
            Activ,
            Observatii,
            Data_Crearii,
            Data_Ultimei_Modificari,
            Creat_De,
            Modificat_De
        )
        OUTPUT INSERTED.Id INTO @OutputTable(Id)
        VALUES (
            @Cod_Pacient,
            @CNP,
            @Nume,
            @Prenume,
            @Data_Nasterii,
            @Sex,
            @Telefon,
            @Telefon_Secundar,
            @Email,
            @Judet,
            @Localitate,
            @Adresa,
            @Cod_Postal,
            @Asigurat,
            @CNP_Asigurat,
            @Nr_Card_Sanatate,
            @Casa_Asigurari,
            @Alergii,
            @Boli_Cronice,
            @Medic_Familie,
            @Persoana_Contact,
            @Telefon_Urgenta,
            @Relatie_Contact,
            @CurrentDate,
            @Activ,
            @Observatii,
            @CurrentDate,
            @CurrentDate,
            @CreatDe,
            @CreatDe
        );
        
        -- Preluare ID din table variable
        SELECT @NewId = Id FROM @OutputTable;
        
        COMMIT TRANSACTION;
        
        -- Returnare pacient creat
        EXEC sp_Pacienti_GetById @NewId;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 10. sp_Pacienti_Update - Actualizare pacient existent
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_Update')
    DROP PROCEDURE sp_Pacienti_Update
GO

CREATE PROCEDURE sp_Pacienti_Update
    @Id UNIQUEIDENTIFIER,
    @CNP NVARCHAR(13) = NULL,
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Data_Nasterii DATE,
    @Sex NVARCHAR(1),
    @Telefon NVARCHAR(15) = NULL,
    @Telefon_Secundar NVARCHAR(15) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Judet NVARCHAR(50) = NULL,
    @Localitate NVARCHAR(100) = NULL,
    @Adresa NVARCHAR(255) = NULL,
    @Cod_Postal NVARCHAR(10) = NULL,
    @Asigurat BIT,
    @CNP_Asigurat NVARCHAR(13) = NULL,
    @Nr_Card_Sanatate NVARCHAR(20) = NULL,
    @Casa_Asigurari NVARCHAR(100) = NULL,
    @Alergii NVARCHAR(MAX) = NULL,
    @Boli_Cronice NVARCHAR(MAX) = NULL,
    @Medic_Familie NVARCHAR(150) = NULL,
    @Persoana_Contact NVARCHAR(150) = NULL,
    @Telefon_Urgenta NVARCHAR(15) = NULL,
    @Relatie_Contact NVARCHAR(50) = NULL,
    @Activ BIT,
    @Observatii NVARCHAR(MAX) = NULL,
    @ModificatDe NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM Pacienti WHERE Id = @Id)
        BEGIN
            THROW 50003, 'Pacientul specificat nu exista.', 1;
        END
        
        -- Verificare duplicat CNP (exclude current ID)
        IF @CNP IS NOT NULL AND EXISTS (SELECT 1 FROM Pacienti WHERE CNP = @CNP AND Id != @Id)
        BEGIN
            THROW 50002, 'Un pacient cu acest CNP exista deja.', 1;
        END
        
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        
        UPDATE Pacienti SET
            CNP = @CNP,
            Nume = @Nume,
            Prenume = @Prenume,
            Data_Nasterii = @Data_Nasterii,
            Sex = @Sex,
            Telefon = @Telefon,
            Telefon_Secundar = @Telefon_Secundar,
            Email = @Email,
            Judet = @Judet,
            Localitate = @Localitate,
            Adresa = @Adresa,
            Cod_Postal = @Cod_Postal,
            Asigurat = @Asigurat,
            CNP_Asigurat = @CNP_Asigurat,
            Nr_Card_Sanatate = @Nr_Card_Sanatate,
            Casa_Asigurari = @Casa_Asigurari,
            Alergii = @Alergii,
            Boli_Cronice = @Boli_Cronice,
            Medic_Familie = @Medic_Familie,
            Persoana_Contact = @Persoana_Contact,
            Telefon_Urgenta = @Telefon_Urgenta,
            Relatie_Contact = @Relatie_Contact,
            Activ = @Activ,
            Observatii = @Observatii,
            Data_Ultimei_Modificari = @CurrentDate,
            Modificat_De = @ModificatDe
        WHERE Id = @Id;
        
        COMMIT TRANSACTION;
        
        -- Returnare pacient actualizat
        EXEC sp_Pacienti_GetById @Id;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 11. sp_Pacienti_Delete - Soft delete pentru pacient
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_Delete')
    DROP PROCEDURE sp_Pacienti_Delete
GO

CREATE PROCEDURE sp_Pacienti_Delete
    @Id UNIQUEIDENTIFIER,
    @ModificatDe NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM Pacienti WHERE Id = @Id)
        BEGIN
            THROW 50003, 'Pacientul specificat nu exista.', 1;
        END
        
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        
        -- Soft delete - marcare ca inactiv
        UPDATE Pacienti SET
            Activ = 0,
            Data_Ultimei_Modificari = @CurrentDate,
            Modificat_De = @ModificatDe
        WHERE Id = @Id;
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success, 'Pacientul a fost dezactivat cu succes.' AS Message;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 12. sp_Pacienti_HardDelete - Stergere fizica (folosire cu precautie)
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_HardDelete')
    DROP PROCEDURE sp_Pacienti_HardDelete
GO

CREATE PROCEDURE sp_Pacienti_HardDelete
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM Pacienti WHERE Id = @Id)
        BEGIN
            THROW 50003, 'Pacientul specificat nu exista.', 1;
        END
        
        -- TODO: Verificare referinte in alte tabele (ex: Consultatii, Programari)
        
        DELETE FROM Pacienti WHERE Id = @Id;
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success, 'Pacientul a fost sters definitiv.' AS Message;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 13. sp_Pacienti_CheckUnique - Verificare unicitate CNP si Cod_Pacient
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_CheckUnique')
    DROP PROCEDURE sp_Pacienti_CheckUnique
GO

CREATE PROCEDURE sp_Pacienti_CheckUnique
    @CNP NVARCHAR(13) = NULL,
    @Cod_Pacient NVARCHAR(20) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CNP_Exists BIT = 0;
    DECLARE @CodPacient_Exists BIT = 0;
    
    -- Check CNP (doar daca este furnizat)
    IF @CNP IS NOT NULL
    BEGIN
        IF EXISTS (
            SELECT 1 
            FROM Pacienti 
            WHERE CNP = @CNP 
            AND (@ExcludeId IS NULL OR Id != @ExcludeId)
        )
            SET @CNP_Exists = 1;
    END
    
    -- Check Cod_Pacient (doar daca este furnizat)
    IF @Cod_Pacient IS NOT NULL
    BEGIN
        IF EXISTS (
            SELECT 1 
            FROM Pacienti 
            WHERE Cod_Pacient = @Cod_Pacient 
            AND (@ExcludeId IS NULL OR Id != @ExcludeId)
        )
            SET @CodPacient_Exists = 1;
    END
    
    SELECT @CNP_Exists AS CNP_Exists, @CodPacient_Exists AS CodPacient_Exists;
END
GO

-- ============================================================================
-- 14. sp_Pacienti_UpdateUltimaVizita - Actualizeaza ultima vizita si numar vizite
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_UpdateUltimaVizita')
    DROP PROCEDURE sp_Pacienti_UpdateUltimaVizita
GO

CREATE PROCEDURE sp_Pacienti_UpdateUltimaVizita
    @Id UNIQUEIDENTIFIER,
    @DataVizita DATE,
    @ModificatDe NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM Pacienti WHERE Id = @Id)
        BEGIN
            THROW 50003, 'Pacientul specificat nu exista.', 1;
        END
        
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        
        UPDATE Pacienti SET
            Ultima_Vizita = @DataVizita,
            Nr_Total_Vizite = Nr_Total_Vizite + 1,
            Data_Ultimei_Modificari = @CurrentDate,
            Modificat_De = @ModificatDe
        WHERE Id = @Id;
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success, 'Ultima vizita actualizata cu succes.' AS Message;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 15. sp_Pacienti_GetStatistics - Statistici pentru dashboard
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetStatistics')
    DROP PROCEDURE sp_Pacienti_GetStatistics
GO

CREATE PROCEDURE sp_Pacienti_GetStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Statistici generale
    SELECT 
        'Total Pacienti' AS Categorie,
        COUNT(*) AS Numar,
        SUM(CASE WHEN Activ = 1 THEN 1 ELSE 0 END) AS Activi
    FROM Pacienti
    
    UNION ALL
    
    SELECT 
        'Pacienti Asigurati',
        COUNT(*),
        SUM(CASE WHEN Activ = 1 THEN 1 ELSE 0 END)
    FROM Pacienti
    WHERE Asigurat = 1
    
    UNION ALL
    
    SELECT 
        'Pacienti Neasigurati',
        COUNT(*),
        SUM(CASE WHEN Activ = 1 THEN 1 ELSE 0 END)
    FROM Pacienti
    WHERE Asigurat = 0
    
    UNION ALL
    
    -- Statistici pe sex
    SELECT 
        'Pacienti ' + CASE Sex WHEN 'M' THEN 'Masculin' WHEN 'F' THEN 'Feminin' ELSE 'Nedefinit' END,
        COUNT(*),
        SUM(CASE WHEN Activ = 1 THEN 1 ELSE 0 END)
    FROM Pacienti
    WHERE Sex IN ('M', 'F')
    GROUP BY Sex;
END
GO

-- ============================================================================
-- 16. sp_Pacienti_GetBirthdays - Pacienti cu ziua de nastere in perioada specificata
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetBirthdays')
    DROP PROCEDURE sp_Pacienti_GetBirthdays
GO

CREATE PROCEDURE sp_Pacienti_GetBirthdays
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Daca nu sunt specificate date, foloseste luna curenta
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(DAY, 1, GETDATE());
    
    IF @EndDate IS NULL
        SET @EndDate = DATEADD(DAY, 30, @StartDate);
    
    SELECT 
        Id,
        Cod_Pacient,
        Nume,
        Prenume,
        Data_Nasterii,
        DATEDIFF(YEAR, Data_Nasterii, GETDATE()) AS Varsta,
        Telefon,
        Email
    FROM Pacienti
    WHERE Activ = 1
      AND (
          (MONTH(Data_Nasterii) = MONTH(@StartDate) AND DAY(Data_Nasterii) >= DAY(@StartDate))
          OR
          (MONTH(Data_Nasterii) = MONTH(@EndDate) AND DAY(Data_Nasterii) <= DAY(@EndDate))
          OR
          (MONTH(Data_Nasterii) > MONTH(@StartDate) AND MONTH(Data_Nasterii) < MONTH(@EndDate))
      )
    ORDER BY MONTH(Data_Nasterii), DAY(Data_Nasterii);
END
GO

-- ============================================================================
-- VERIFICARE CREARE PROCEDURI
-- ============================================================================
PRINT 'Verificare proceduri create:';
SELECT 
    name AS 'Procedura Creata',
    create_date AS 'Data Creare'
FROM sys.procedures 
WHERE name LIKE 'sp_Pacienti_%'
ORDER BY name;

PRINT '';
PRINT 'Script executat cu succes! Toate procedurile stocate pentru Pacienti au fost create.';
GO
