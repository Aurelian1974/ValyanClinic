-- Tabel Principal Pentru Personal (SQL Server)
CREATE TABLE Personal (
    -- Identificatori Unici
    Id_Personal UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Cod_Angajat VARCHAR(20) UNIQUE NOT NULL,
    CNP VARCHAR(13) UNIQUE NOT NULL,
    
    -- Date Personale De Baza
    Nume NVARCHAR(100) NOT NULL,
    Prenume NVARCHAR(100) NOT NULL,
    Nume_Anterior NVARCHAR(100), -- Pentru Femei Casatorite
    Data_Nasterii DATE NOT NULL,
    Locul_Nasterii NVARCHAR(200),
    Nationalitate NVARCHAR(50) DEFAULT N'Romana',
    Cetatenie NVARCHAR(50) DEFAULT N'Romana',
    
    -- Contact
    Telefon_Personal VARCHAR(20),
    Telefon_Serviciu VARCHAR(20),
    Email_Personal VARCHAR(100),
    Email_Serviciu VARCHAR(100),
    
    -- Adresa Domiciliu
    Adresa_Domiciliu NVARCHAR(MAX) NOT NULL,
    Judet_Domiciliu NVARCHAR(50) NOT NULL,
    Oras_Domiciliu NVARCHAR(100) NOT NULL,
    Cod_Postal_Domiciliu VARCHAR(10),
    
    -- Adresa Resedinta (Daca Difera)
    Adresa_Resedinta NVARCHAR(MAX),
    Judet_Resedinta NVARCHAR(50),
    Oras_Resedinta NVARCHAR(100),
    Cod_Postal_Resedinta VARCHAR(10),
    
    -- Stare Civila Si Familie
    Stare_Civila NVARCHAR(100), -- ('Necasatorit/a', 'Casatorit/a', 'Divortat/a', 'Vaduv/a', 'Uniune Consensuala') - vom folosi enum-uri in C#
    
    -- Date Profesionale
    Functia NVARCHAR(100) NOT NULL,
    Departament NVARCHAR(100),
      
    -- Date Administrative
    Serie_CI VARCHAR(10),
    Numar_CI VARCHAR(20),
    Eliberat_CI_De NVARCHAR(100),
    Data_Eliberare_CI DATE,
    Valabil_CI_Pana DATE,
    
    -- Status Si Metadata (auditarea se face in aplicatie)
    Status_Angajat NVARCHAR(50) DEFAULT N'Activ', -- vom folosi enum-uri in C# ('Activ', 'Suspendat', 'Concediu_Fara_Plata', 'Concediu_Medical', 'Plecat')
    Observatii NVARCHAR(MAX),
    Data_Crearii DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Data_Ultimei_Modificari DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Creat_De NVARCHAR(50),
    Modificat_De NVARCHAR(50),
    
    -- Constrangeri
    CONSTRAINT CHK_Personal_Data_Nasterii CHECK (Data_Nasterii <= CAST(GETDATE() AS DATE)),
    CONSTRAINT CHK_Personal_CNP_Length CHECK (LEN(CNP) = 13),
    CONSTRAINT CHK_Personal_Email_Personal_Format CHECK (Email_Personal IS NULL OR Email_Personal LIKE '%_@_%._%'),
    CONSTRAINT CHK_Personal_Email_Serviciu_Format CHECK (Email_Serviciu IS NULL OR Email_Serviciu LIKE '%_@_%._%')
);

-- Indexuri Pentru Performanta
CREATE INDEX IX_Personal_Nume ON Personal(Nume, Prenume);
CREATE INDEX IX_Personal_CNP ON Personal(CNP);
CREATE INDEX IX_Personal_Cod_Angajat ON Personal(Cod_Angajat);
CREATE INDEX IX_Personal_Departament ON Personal(Departament);
CREATE INDEX IX_Personal_Functia ON Personal(Functia);
CREATE INDEX IX_Personal_Status ON Personal(Status_Angajat);
CREATE INDEX IX_Personal_Data_Crearii ON Personal(Data_Crearii);
