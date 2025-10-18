-- ========================================
-- SETUP COMPLET PACIENTI - SCRIPT COMBINAT
-- Database: ValyanMed
-- Creat: 2025-01-23
-- ========================================
-- INSTRUCTIUNI:
-- 1. Deschide SQL Server Management Studio
-- 2. Conecteaza-te la serverul: DESKTOP-9H54BCS\SQLSERVER
-- 3. Selecteaza baza de date: ValyanMed
-- 4. Copiaza si ruleaza tot acest script
-- ========================================

USE [ValyanMed]
GO

PRINT '==========================================';
PRINT '   SETUP MODUL PACIENTI - START';
PRINT '==========================================';
PRINT '';

-- ========================================
-- PARTEA 1: CREARE TABELA PACIENTI
-- ========================================

PRINT 'Pasul 1: Verificare si creare tabela Pacienti...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Pacienti')
BEGIN
    PRINT '   ATENTIE: Tabela Pacienti exista deja!';
    PRINT '   Daca doresti sa o recreezi, sterge mai intai datele existente.';
    -- UNCOMMENT pentru a sterge tabela existenta (ATENTIE: PIERDERE DATE!)
    -- DROP TABLE Pacienti;
    -- PRINT '   Tabela Pacienti stearsa.';
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pacienti')
BEGIN
    CREATE TABLE [dbo].[Pacienti] (
        -- Primary Key
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        
        -- Identificare
        [Cod_Pacient] NVARCHAR(20) NOT NULL,
        [CNP] NVARCHAR(13) NULL,
        
        -- Date Personale
        [Nume] NVARCHAR(100) NOT NULL,
        [Prenume] NVARCHAR(100) NOT NULL,
        [Data_Nasterii] DATE NOT NULL,
        [Sex] NVARCHAR(1) NOT NULL CHECK ([Sex] IN ('M', 'F', 'A')),
        
        -- Contact
        [Telefon] NVARCHAR(15) NULL,
        [Telefon_Secundar] NVARCHAR(15) NULL,
        [Email] NVARCHAR(100) NULL,
        
        -- Adresa
        [Judet] NVARCHAR(50) NULL,
        [Localitate] NVARCHAR(100) NULL,
        [Adresa] NVARCHAR(255) NULL,
        [Cod_Postal] NVARCHAR(10) NULL,
        
        -- Asigurare
        [Asigurat] BIT NOT NULL DEFAULT 0,
        [CNP_Asigurat] NVARCHAR(13) NULL,
        [Nr_Card_Sanatate] NVARCHAR(20) NULL,
        [Casa_Asigurari] NVARCHAR(100) NULL,
        
        -- Informatii Medicale
        [Alergii] NVARCHAR(MAX) NULL,
        [Boli_Cronice] NVARCHAR(MAX) NULL,
        [Medic_Familie] NVARCHAR(150) NULL,
        
        -- Contact Urgenta
        [Persoana_Contact] NVARCHAR(150) NULL,
        [Telefon_Urgenta] NVARCHAR(15) NULL,
        [Relatie_Contact] NVARCHAR(50) NULL,
        
        -- Tracking Vizite
        [Data_Inregistrare] DATE NOT NULL DEFAULT GETDATE(),
        [Ultima_Vizita] DATE NULL,
        [Nr_Total_Vizite] INT NOT NULL DEFAULT 0,
        
        -- Status
        [Activ] BIT NOT NULL DEFAULT 1,
        [Observatii] NVARCHAR(MAX) NULL,
        
        -- Audit
        [Data_Crearii] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [Data_Ultimei_Modificari] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [Creat_De] NVARCHAR(100) NOT NULL,
        [Modificat_De] NVARCHAR(100) NOT NULL,
        
        -- Constraints
        CONSTRAINT [PK_Pacienti] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Pacienti_Cod] UNIQUE NONCLUSTERED ([Cod_Pacient] ASC),
        CONSTRAINT [UQ_Pacienti_CNP] UNIQUE NONCLUSTERED ([CNP] ASC)
    );
    
    PRINT '   ? Tabela Pacienti creata cu succes!';
END
ELSE
BEGIN
    PRINT '   ? Tabela Pacienti exista deja, se sare peste creare.';
END

-- Creare indecsi
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Pacienti_Nume_Prenume')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Pacienti_Nume_Prenume]
    ON [dbo].[Pacienti] ([Nume] ASC, [Prenume] ASC);
    PRINT '   ? Index IX_Pacienti_Nume_Prenume creat.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Pacienti_DataNasterii')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Pacienti_DataNasterii]
    ON [dbo].[Pacienti] ([Data_Nasterii] ASC);
    PRINT '   ? Index IX_Pacienti_DataNasterii creat.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Pacienti_Judet')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Pacienti_Judet]
    ON [dbo].[Pacienti] ([Judet] ASC) WHERE [Activ] = 1;
    PRINT '   ? Index IX_Pacienti_Judet creat.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Pacienti_Activ')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Pacienti_Activ]
    ON [dbo].[Pacienti] ([Activ] ASC) INCLUDE ([Nume], [Prenume], [Cod_Pacient]);
    PRINT '   ? Index IX_Pacienti_Activ creat.';
END

PRINT '';
PRINT 'Pasul 1: COMPLET';
PRINT '';

-- ========================================
-- PARTEA 2: CREARE STORED PROCEDURES
-- ========================================

PRINT 'Pasul 2: Creare stored procedures pentru Pacienti...';

-- Pentru spatiu, include doar cateva proceduri importante
-- Restul pot fi rulate din scriptul sp_Pacienti.sql

-- sp_Pacienti_GetAll
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
    
    IF @SortDirection NOT IN ('ASC', 'DESC')
        SET @SortDirection = 'ASC';
    
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
        CASE WHEN @SortColumn = 'Prenume' AND @SortDirection = 'DESC' THEN Prenume END DESC
    OFFSET @Offset ROWS 
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT '   ? sp_Pacienti_GetAll creata.';

-- sp_Pacienti_GetCount
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

PRINT '   ? sp_Pacienti_GetCount creata.';

-- sp_Pacienti_GetById
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetById')
    DROP PROCEDURE sp_Pacienti_GetById
GO

CREATE PROCEDURE sp_Pacienti_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM Pacienti WHERE Id = @Id;
END
GO

PRINT '   ? sp_Pacienti_GetById creata.';

PRINT '';
PRINT 'Pasul 2: COMPLET (3 proceduri de baza create)';
PRINT '   NOTA: Pentru toate procedurile, ruleaza scriptul sp_Pacienti.sql';
PRINT '';

-- ========================================
-- PARTEA 3: DATE DE TEST (OPTIONAL)
-- ========================================

PRINT 'Pasul 3: Inserare date de test...';

IF NOT EXISTS (SELECT * FROM Pacienti WHERE Cod_Pacient = 'PACIENT00000001')
BEGIN
    INSERT INTO Pacienti (
        Cod_Pacient, CNP, Nume, Prenume, Data_Nasterii, Sex,
        Telefon, Email, Judet, Localitate, Adresa,
        Asigurat, Casa_Asigurari, Activ,
        Creat_De, Modificat_De
    ) VALUES
    ('PACIENT00000001', '1800515123456', 'Popescu', 'Ion', '1980-05-15', 'M',
     '0721234567', 'ion.popescu@email.com', 'Bucuresti', 'Bucuresti', 'Str. Exemplu nr. 123',
     1, 'CNAS', 1, 'System', 'System'),
    
    ('PACIENT00000002', '2850610234567', 'Ionescu', 'Maria', '1985-06-10', 'F',
     '0723456789', 'maria.ionescu@email.com', 'Cluj', 'Cluj-Napoca', 'Str. Libert??ii nr. 45',
     1, 'CNAS', 1, 'System', 'System'),
    
    ('PACIENT00000003', '1921125345678', 'Constantinescu', 'George', '1992-11-25', 'M',
     '0734567890', 'george.const@email.com', 'Timis', 'Timisoara', 'Bd. Revolutiei nr. 78',
     0, NULL, 1, 'System', 'System');
    
    PRINT '   ? 3 pacienti de test adaugati.';
END
ELSE
BEGIN
    PRINT '   ? Date de test exista deja, se sare peste inserare.';
END

PRINT '';
PRINT 'Pasul 3: COMPLET';
PRINT '';

-- ========================================
-- VERIFICARE FINALA
-- ========================================

PRINT '==========================================';
PRINT '   VERIFICARE SETUP';
PRINT '==========================================';

-- Verificare tabela
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Pacienti')
    PRINT '? Tabela Pacienti: EXISTA'
ELSE
    PRINT '? Tabela Pacienti: NU EXISTA'

-- Verificare stored procedures
DECLARE @ProcCount INT;
SELECT @ProcCount = COUNT(*)
FROM sys.procedures 
WHERE name LIKE 'sp_Pacienti_%';

PRINT '? Stored Procedures sp_Pacienti_*: ' + CAST(@ProcCount AS NVARCHAR(10)) + ' gasite';

-- Verificare date
DECLARE @RecordCount INT;
SELECT @RecordCount = COUNT(*) FROM Pacienti;
PRINT '? Inregistrari in Pacienti: ' + CAST(@RecordCount AS NVARCHAR(10));

PRINT '';
PRINT '==========================================';
PRINT '   SETUP MODUL PACIENTI - COMPLET!';
PRINT '==========================================';
PRINT '';
PRINT 'URMATORUL PAS:';
PRINT '1. Porneste aplicatia ValyanClinic';
PRINT '2. Navigheaz? la: /pacienti/vizualizare';
PRINT '3. Ar trebui s? vezi grid-ul cu pacien?ii';
PRINT '';
PRINT 'Pentru toate stored procedures, ruleaza:';
PRINT '   DevSupport\Database\StoredProcedures\sp_Pacienti.sql';
PRINT '';

GO
