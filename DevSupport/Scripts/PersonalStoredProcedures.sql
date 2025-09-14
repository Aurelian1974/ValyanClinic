-- Stored Procedures pentru tabela Personal
-- CRUD operations optimizate pentru performance

-- =============================================
-- SP pentru obtinerea listei de personal cu filtrare si paginare
-- =============================================
CREATE PROCEDURE [dbo].[sp_Personal_GetAll]
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SearchText NVARCHAR(255) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Status NVARCHAR(50) = NULL,
    @SortColumn NVARCHAR(50) = 'Nume',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @CountSQL NVARCHAR(MAX);
    DECLARE @WhereClause NVARCHAR(MAX) = ' WHERE 1=1 ';
    
    -- Construire WHERE clause dinamic
    IF @SearchText IS NOT NULL AND @SearchText != ''
    BEGIN
        SET @WhereClause = @WhereClause + ' AND (Nume LIKE ''%' + @SearchText + '%'' OR Prenume LIKE ''%' + @SearchText + '%'' OR Email_Personal LIKE ''%' + @SearchText + '%'' OR Telefon_Personal LIKE ''%' + @SearchText + '%'') ';
    END
    
    IF @Departament IS NOT NULL AND @Departament != ''
    BEGIN
        SET @WhereClause = @WhereClause + ' AND Departament = ''' + @Departament + ''' ';
    END
    
    IF @Status IS NOT NULL AND @Status != ''
    BEGIN
        SET @WhereClause = @WhereClause + ' AND Status_Angajat = ''' + @Status + ''' ';
    END
    
    -- Query principal cu paginare
    SET @SQL = '
    SELECT 
        Id_Personal,
        Cod_Angajat,
        CNP,
        Nume,
        Prenume,
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
    FROM Personal ' + @WhereClause + '
    ORDER BY ' + @SortColumn + ' ' + @SortDirection + '
    OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + ' ROWS
    FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + ' ROWS ONLY';
    
    -- Query pentru total count
    SET @CountSQL = 'SELECT COUNT(*) as TotalCount FROM Personal ' + @WhereClause;
    
    -- Executare queries
    EXEC sp_executesql @SQL;
    EXEC sp_executesql @CountSQL;
END;

-- =============================================
-- SP pentru obtinerea statisticilor
-- =============================================
CREATE PROCEDURE [dbo].[sp_Personal_GetStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Total Personal' as StatisticName,
        COUNT(*) as Value,
        'fas fa-users' as IconClass,
        '#007bff' as ColorClass
    FROM Personal
    
    UNION ALL
    
    SELECT 
        'Personal Activ' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-check' as IconClass,
        '#28a745' as ColorClass
    FROM Personal 
    WHERE Status_Angajat = 'Activ'
    
    UNION ALL
    
    SELECT 
        'Personal Inactiv' as StatisticName,
        COUNT(*) as Value,
        'fas fa-user-times' as IconClass,
        '#dc3545' as ColorClass
    FROM Personal 
    WHERE Status_Angajat = 'Inactiv';
END;

-- =============================================
-- SP pentru obtinerea unei persoane dupa ID
-- =============================================
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

-- =============================================
-- SP pentru verificarea unicit??ii CNP ?i Cod_Angajat
-- =============================================
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

-- =============================================
-- SP pentru crearea unei noi persoane
-- =============================================
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

-- =============================================
-- SP pentru actualizarea unei persoane
-- =============================================
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
            Data_Ultimei_Modificari = GETUTCDATE(),
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

-- =============================================
-- SP pentru stergerea unei persoane (soft delete)
-- =============================================
CREATE PROCEDURE [dbo].[sp_Personal_Delete]
    @Id_Personal UNIQUEIDENTIFIER,
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
        
        -- Soft delete - setare status Inactiv
        UPDATE Personal SET
            Status_Angajat = 'Inactiv',
            Data_Ultimei_Modificari = GETUTCDATE(),
            Modificat_De = @Modificat_De,
            Observatii = ISNULL(Observatii + ' | ', '') + 'Dezactivat pe ' + CONVERT(NVARCHAR, GETUTCDATE(), 120) + ' de ' + @Modificat_De
        WHERE Id_Personal = @Id_Personal;
        
        COMMIT TRANSACTION;
        
        SELECT 1 as Success;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

-- =============================================
-- SP pentru obtinerea optiunilor pentru dropdown-uri
-- =============================================
CREATE PROCEDURE [dbo].[sp_Personal_GetDropdownOptions]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Departamente distincte
    SELECT DISTINCT 
        Departament as Value,
        Departament as Text
    FROM Personal 
    WHERE Departament IS NOT NULL 
    ORDER BY Departament;
    
    -- Functii distincte
    SELECT DISTINCT 
        Functia as Value,
        Functia as Text
    FROM Personal 
    WHERE Functia IS NOT NULL 
    ORDER BY Functia;
    
    -- Judete distincte
    SELECT DISTINCT 
        Judet_Domiciliu as Value,
        Judet_Domiciliu as Text
    FROM Personal 
    WHERE Judet_Domiciliu IS NOT NULL 
    ORDER BY Judet_Domiciliu;
END;