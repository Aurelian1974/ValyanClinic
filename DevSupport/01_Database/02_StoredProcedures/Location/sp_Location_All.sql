-- =============================================
-- Stored Procedures pentru Location (Judete si Localitati)
-- Pentru ValyanClinic - IJudeteService
-- Created: 2025-12-27
-- =============================================

-- =============================================
-- 1. sp_Location_GetJudete - Obtine toate judetele
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Location_GetJudete')
    DROP PROCEDURE [dbo].[sp_Location_GetJudete];
GO

CREATE PROCEDURE [dbo].[sp_Location_GetJudete]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdJudet,
        Nume
    FROM Judet
    ORDER BY Ordine ASC, Nume ASC;
END;
GO

PRINT '✅ sp_Location_GetJudete creat/actualizat';
GO

-- =============================================
-- 2. sp_Location_GetLocalitatiByJudetId - Obtine localitatile dintr-un judet
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Location_GetLocalitatiByJudetId')
    DROP PROCEDURE [dbo].[sp_Location_GetLocalitatiByJudetId];
GO

CREATE PROCEDURE [dbo].[sp_Location_GetLocalitatiByJudetId]
    @JudetId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdOras,
        Nume
    FROM Localitate
    WHERE IdJudet = @JudetId
    ORDER BY Nume ASC;
END;
GO

PRINT '✅ sp_Location_GetLocalitatiByJudetId creat/actualizat';
GO

-- =============================================
-- 3. sp_Location_GetJudetNameById - Obtine numele unui judet dupa ID
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Location_GetJudetNameById')
    DROP PROCEDURE [dbo].[sp_Location_GetJudetNameById];
GO

CREATE PROCEDURE [dbo].[sp_Location_GetJudetNameById]
    @JudetId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT Nume 
    FROM Judet 
    WHERE IdJudet = @JudetId;
END;
GO

PRINT '✅ sp_Location_GetJudetNameById creat/actualizat';
GO

-- =============================================
-- 4. sp_Location_GetLocalitateNameById - Obtine numele unei localitati dupa ID
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Location_GetLocalitateNameById')
    DROP PROCEDURE [dbo].[sp_Location_GetLocalitateNameById];
GO

CREATE PROCEDURE [dbo].[sp_Location_GetLocalitateNameById]
    @LocalitateId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT Nume 
    FROM Localitate 
    WHERE IdOras = @LocalitateId;
END;
GO

PRINT '✅ sp_Location_GetLocalitateNameById creat/actualizat';
GO

-- =============================================
-- Verificare finala
-- =============================================
PRINT '';
PRINT '========== VERIFICARE SP-uri Location ==========';

SELECT 
    name AS [Stored Procedure],
    create_date AS [Data Creare],
    modify_date AS [Ultima Modificare]
FROM sys.procedures 
WHERE name LIKE 'sp_Location_%'
ORDER BY name;

PRINT '';
PRINT '✅ Toate SP-urile Location au fost create/actualizate cu succes!';
GO
