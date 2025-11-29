-- ========================================
-- Verificare Instalare Pacienti
-- Database: ValyanMed
-- Descriere: Script pentru verificarea instalarii complete a tabelei Pacienti
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================'
PRINT 'VERIFICARE INSTALARE PACIENTI'
PRINT '========================================'
PRINT ''

-- ============================================================================
-- 1. VERIFICARE EXISTENTA TABELA
-- ============================================================================
PRINT '1. VERIFICARE EXISTENTA TABELA'
PRINT '----------------------------------------'

IF OBJECT_ID('dbo.Pacienti', 'U') IS NOT NULL
BEGIN
    PRINT '? Tabela Pacienti exista'
    
    -- Numar inregistrari
    DECLARE @Count INT
    SELECT @Count = COUNT(*) FROM dbo.Pacienti
    PRINT '  Numar inregistrari: ' + CAST(@Count AS VARCHAR(10))
END
ELSE
BEGIN
    PRINT '? EROARE: Tabela Pacienti NU exista!'
    PRINT '  Rulati Pacienti_Install.sql pentru instalare'
END
PRINT ''

-- ============================================================================
-- 2. VERIFICARE STRUCTURA TABELA
-- ============================================================================
PRINT '2. VERIFICARE STRUCTURA TABELA'
PRINT '----------------------------------------'

IF OBJECT_ID('dbo.Pacienti', 'U') IS NOT NULL
BEGIN
    DECLARE @ColumnCount INT
    SELECT @ColumnCount = COUNT(*) 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.Pacienti')
    
    PRINT '? Numar coloane: ' + CAST(@ColumnCount AS VARCHAR(10))
    
    -- Lista coloane
    PRINT ''
    PRINT '  Coloane principale:'
    SELECT 
        c.name AS Coloana,
        t.name AS TipDate,
        c.max_length AS Lungime,
        CASE WHEN c.is_nullable = 1 THEN 'Da' ELSE 'Nu' END AS Nullable
    FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.Pacienti')
    ORDER BY c.column_id
END
PRINT ''

-- ============================================================================
-- 3. VERIFICARE CONSTRAINTE
-- ============================================================================
PRINT '3. VERIFICARE CONSTRAINTE'
PRINT '----------------------------------------'

IF OBJECT_ID('dbo.Pacienti', 'U') IS NOT NULL
BEGIN
    -- Primary Key
    IF EXISTS (
        SELECT 1 FROM sys.key_constraints 
        WHERE parent_object_id = OBJECT_ID('dbo.Pacienti') 
        AND type = 'PK'
    )
        PRINT '? Primary Key: PK_Pacienti'
    ELSE
        PRINT '? Primary Key LIPSA!'
    
    -- Unique Constraints
    DECLARE @UniqueCount INT
    SELECT @UniqueCount = COUNT(*) 
    FROM sys.key_constraints 
    WHERE parent_object_id = OBJECT_ID('dbo.Pacienti') 
    AND type = 'UQ'
    
    PRINT '? Unique Constraints: ' + CAST(@UniqueCount AS VARCHAR(10))
    
    SELECT name AS Constraint_Name
    FROM sys.key_constraints 
    WHERE parent_object_id = OBJECT_ID('dbo.Pacienti') 
    AND type = 'UQ'
    
    -- Check Constraints
    DECLARE @CheckCount INT
    SELECT @CheckCount = COUNT(*) 
    FROM sys.check_constraints 
    WHERE parent_object_id = OBJECT_ID('dbo.Pacienti')
    
    PRINT '? Check Constraints: ' + CAST(@CheckCount AS VARCHAR(10))
    
    SELECT name AS Constraint_Name
    FROM sys.check_constraints 
    WHERE parent_object_id = OBJECT_ID('dbo.Pacienti')
END
PRINT ''

-- ============================================================================
-- 4. VERIFICARE INDEXURI
-- ============================================================================
PRINT '4. VERIFICARE INDEXURI'
PRINT '----------------------------------------'

IF OBJECT_ID('dbo.Pacienti', 'U') IS NOT NULL
BEGIN
    DECLARE @IndexCount INT
    SELECT @IndexCount = COUNT(*) 
    FROM sys.indexes 
    WHERE object_id = OBJECT_ID('dbo.Pacienti')
    AND name IS NOT NULL
    
    PRINT '? Numar indexuri: ' + CAST(@IndexCount AS VARCHAR(10))
    
    PRINT ''
    PRINT '  Lista indexuri:'
    SELECT 
        i.name AS Index_Name,
        i.type_desc AS Tip,
        CASE WHEN i.is_unique = 1 THEN 'Da' ELSE 'Nu' END AS Este_Unic
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID('dbo.Pacienti')
    AND i.name IS NOT NULL
    ORDER BY i.name
END
PRINT ''

-- ============================================================================
-- 5. VERIFICARE TRIGGERE
-- ============================================================================
PRINT '5. VERIFICARE TRIGGERE'
PRINT '----------------------------------------'

IF OBJECT_ID('dbo.Pacienti', 'U') IS NOT NULL
BEGIN
    DECLARE @TriggerCount INT
    SELECT @TriggerCount = COUNT(*) 
    FROM sys.triggers 
    WHERE parent_id = OBJECT_ID('dbo.Pacienti')
    
    IF @TriggerCount > 0
    BEGIN
        PRINT '? Numar triggere: ' + CAST(@TriggerCount AS VARCHAR(10))
        
        SELECT 
            name AS Trigger_Name,
            CASE WHEN is_disabled = 1 THEN 'Dezactivat' ELSE 'Activ' END AS Status
        FROM sys.triggers 
        WHERE parent_id = OBJECT_ID('dbo.Pacienti')
    END
    ELSE
    BEGIN
        PRINT '? Nici un trigger gasit'
    END
END
PRINT ''

-- ============================================================================
-- 6. VERIFICARE STORED PROCEDURES
-- ============================================================================
PRINT '6. VERIFICARE STORED PROCEDURES'
PRINT '----------------------------------------'

DECLARE @SPCount INT
SELECT @SPCount = COUNT(*) 
FROM sys.procedures 
WHERE name LIKE 'sp_Pacienti_%'

IF @SPCount > 0
BEGIN
    PRINT '? Numar stored procedures: ' + CAST(@SPCount AS VARCHAR(10))
    
    PRINT ''
    PRINT '  Lista stored procedures:'
    SELECT 
        name AS Procedura,
        create_date AS Data_Creare
    FROM sys.procedures 
    WHERE name LIKE 'sp_Pacienti_%'
    ORDER BY name
END
ELSE
BEGIN
    PRINT '? ATENTIE: Nici o stored procedure gasita!'
    PRINT '  Rulati sp_Pacienti.sql pentru a crea procedurile'
END
PRINT ''

-- ============================================================================
-- 7. VERIFICARE COMENTARII DOCUMENTATIE
-- ============================================================================
PRINT '7. VERIFICARE COMENTARII DOCUMENTATIE'
PRINT '----------------------------------------'

IF OBJECT_ID('dbo.Pacienti', 'U') IS NOT NULL
BEGIN
    DECLARE @PropertyCount INT
    SELECT @PropertyCount = COUNT(*) 
    FROM sys.extended_properties 
    WHERE major_id = OBJECT_ID('dbo.Pacienti')
    
    IF @PropertyCount > 0
    BEGIN
        PRINT '? Comentarii documentatie: ' + CAST(@PropertyCount AS VARCHAR(10))
    END
    ELSE
    BEGIN
        PRINT '? Nici un comentariu de documentatie gasit'
    END
END
PRINT ''

-- ============================================================================
-- 8. TEST FUNCTIONALITATE DE BAZA
-- ============================================================================
PRINT '8. TEST FUNCTIONALITATE DE BAZA'
PRINT '----------------------------------------'

IF OBJECT_ID('dbo.Pacienti', 'U') IS NOT NULL
BEGIN
    BEGIN TRY
        -- Test generare cod pacient
        DECLARE @NextCod NVARCHAR(20)
        
        IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_Pacienti_GenerateNextCodPacient')
        BEGIN
            EXEC sp_Pacienti_GenerateNextCodPacient
            PRINT '? Test generare cod pacient: SUCCESS'
        END
        ELSE
        BEGIN
            PRINT '? Procedura sp_Pacienti_GenerateNextCodPacient nu exista'
        END
        
        -- Test select
        SELECT TOP 1 * FROM dbo.Pacienti
        PRINT '? Test SELECT: SUCCESS'
        
    END TRY
    BEGIN CATCH
        PRINT '? EROARE la testare: ' + ERROR_MESSAGE()
    END CATCH
END
PRINT ''

-- ============================================================================
-- SUMAR VERIFICARE
-- ============================================================================
PRINT '========================================'
PRINT 'SUMAR VERIFICARE'
PRINT '========================================'

IF OBJECT_ID('dbo.Pacienti', 'U') IS NOT NULL
BEGIN
    DECLARE @TotalColoane INT, @TotalIndexuri INT, @TotalTriggere INT, @TotalSP INT
    
    SELECT @TotalColoane = COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Pacienti')
    SELECT @TotalIndexuri = COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Pacienti') AND name IS NOT NULL
    SELECT @TotalTriggere = COUNT(*) FROM sys.triggers WHERE parent_id = OBJECT_ID('dbo.Pacienti')
    SELECT @TotalSP = COUNT(*) FROM sys.procedures WHERE name LIKE 'sp_Pacienti_%'
    
    PRINT 'Status: INSTALARE COMPLETA'
    PRINT ''
    PRINT 'Componente instalate:'
    PRINT '  • Tabela: Pacienti'
    PRINT '  • Coloane: ' + CAST(@TotalColoane AS VARCHAR(10))
    PRINT '  • Indexuri: ' + CAST(@TotalIndexuri AS VARCHAR(10))
    PRINT '  • Triggere: ' + CAST(@TotalTriggere AS VARCHAR(10))
    PRINT '  • Stored Procedures: ' + CAST(@TotalSP AS VARCHAR(10))
    PRINT ''
    
    IF @TotalSP = 0
    BEGIN
        PRINT 'ATENTIE: Stored procedures nu sunt instalate!'
        PRINT '  ? Rulati: sp_Pacienti.sql'
        PRINT ''
    END
    ELSE
    BEGIN
        PRINT '? Toate componentele sunt instalate corect!'
        PRINT '? Tabela Pacienti este gata de utilizare!'
    END
END
ELSE
BEGIN
    PRINT 'Status: INSTALARE INCOMPLET'
    PRINT ''
    PRINT '? Tabela Pacienti NU exista!'
    PRINT '  ? Rulati: Pacienti_Install.sql'
END

PRINT ''
PRINT '========================================'
GO
