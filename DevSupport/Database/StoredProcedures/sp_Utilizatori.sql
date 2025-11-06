-- ========================================
-- Stored Procedures: Utilizatori
-- Database: ValyanMed
-- Descriere: 12 Stored Procedures pentru gestionarea utilizatorilor
-- Created: 2025-01-24
-- ========================================

USE [ValyanMed]
GO

-- ========================================
-- 1. sp_Utilizatori_GetAll
-- Descriere: Returneaza toti utilizatorii cu detalii PersonalMedical
-- ========================================

IF OBJECT_ID('sp_Utilizatori_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE sp_Utilizatori_GetAll
GO

CREATE PROCEDURE sp_Utilizatori_GetAll
    @EsteActiv BIT = NULL,
    @Rol NVARCHAR(50) = NULL,
    @SearchText NVARCHAR(100) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SortColumn NVARCHAR(50) = 'Username',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH UtilizatoriCTE AS (
        SELECT 
  u.UtilizatorID,
     u.PersonalMedicalID,
      u.Username,
 u.Email,
            u.Rol,
            u.EsteActiv,
        u.DataCreare,
    u.DataUltimaAutentificare,
            u.NumarIncercariEsuate,
            u.DataBlocare,
        pm.Nume,
pm.Prenume,
     pm.Nume + ' ' + pm.Prenume AS NumeComplet,
        pm.Specializare,
     pm.Departament,
        pm.Telefon,
        u.CreatDe,
u.DataCrearii,
            u.ModificatDe,
   u.DataUltimeiModificari
  FROM Utilizatori u
        INNER JOIN PersonalMedical pm ON u.PersonalMedicalID = pm.PersonalID
        WHERE 
        (@EsteActiv IS NULL OR u.EsteActiv = @EsteActiv)
          AND (@Rol IS NULL OR u.Rol = @Rol)
      AND (
     @SearchText IS NULL 
        OR u.Username LIKE '%' + @SearchText + '%'
          OR u.Email LIKE '%' + @SearchText + '%'
    OR pm.Nume LIKE '%' + @SearchText + '%'
           OR pm.Prenume LIKE '%' + @SearchText + '%'
      OR u.Rol LIKE '%' + @SearchText + '%'
            )
    )
    SELECT *
  FROM UtilizatoriCTE
    ORDER BY 
        CASE WHEN @SortColumn = 'Username' AND @SortDirection = 'ASC' THEN Username END ASC,
        CASE WHEN @SortColumn = 'Username' AND @SortDirection = 'DESC' THEN Username END DESC,
        CASE WHEN @SortColumn = 'Email' AND @SortDirection = 'ASC' THEN Email END ASC,
  CASE WHEN @SortColumn = 'Email' AND @SortDirection = 'DESC' THEN Email END DESC,
        CASE WHEN @SortColumn = 'Rol' AND @SortDirection = 'ASC' THEN Rol END ASC,
        CASE WHEN @SortColumn = 'Rol' AND @SortDirection = 'DESC' THEN Rol END DESC,
        CASE WHEN @SortColumn = 'NumeComplet' AND @SortDirection = 'ASC' THEN NumeComplet END ASC,
        CASE WHEN @SortColumn = 'NumeComplet' AND @SortDirection = 'DESC' THEN NumeComplet END DESC,
     CASE WHEN @SortColumn = 'DataCreare' AND @SortDirection = 'ASC' THEN DataCreare END ASC,
        CASE WHEN @SortColumn = 'DataCreare' AND @SortDirection = 'DESC' THEN DataCreare END DESC
    OFFSET @Offset ROWS
  FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ========================================
-- 2. sp_Utilizatori_GetCount
-- Descriere: Returneaza numarul total de utilizatori (pentru paginare)
-- ========================================

IF OBJECT_ID('sp_Utilizatori_GetCount', 'P') IS NOT NULL
    DROP PROCEDURE sp_Utilizatori_GetCount
GO

CREATE PROCEDURE sp_Utilizatori_GetCount
    @EsteActiv BIT = NULL,
  @Rol NVARCHAR(50) = NULL,
    @SearchText NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS TotalCount
    FROM Utilizatori u
    INNER JOIN PersonalMedical pm ON u.PersonalMedicalID = pm.PersonalID
    WHERE 
        (@EsteActiv IS NULL OR u.EsteActiv = @EsteActiv)
        AND (@Rol IS NULL OR u.Rol = @Rol)
      AND (
     @SearchText IS NULL 
          OR u.Username LIKE '%' + @SearchText + '%'
    OR u.Email LIKE '%' + @SearchText + '%'
   OR pm.Nume LIKE '%' + @SearchText + '%'
         OR pm.Prenume LIKE '%' + @SearchText + '%'
       OR u.Rol LIKE '%' + @SearchText + '%'
        );
END
GO

-- ========================================
-- 3. sp_Utilizatori_GetById
-- Descriere: Returneaza un utilizator dupa ID
-- ========================================

IF OBJECT_ID('sp_Utilizatori_GetById', 'P') IS NOT NULL
    DROP PROCEDURE sp_Utilizatori_GetById
GO

CREATE PROCEDURE sp_Utilizatori_GetById
    @UtilizatorID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UtilizatorID,
        u.PersonalMedicalID,
        u.Username,
        u.Email,
        u.PasswordHash,
        u.Salt,
        u.Rol,
      u.EsteActiv,
        u.DataCreare,
u.DataUltimaAutentificare,
     u.NumarIncercariEsuate,
        u.DataBlocare,
   u.TokenResetareParola,
        u.DataExpirareToken,
        pm.Nume,
        pm.Prenume,
        pm.Nume + ' ' + pm.Prenume AS NumeComplet,
   pm.Specializare,
        pm.Departament,
        pm.Telefon,
     pm.Email AS EmailPersonalMedical,
    u.CreatDe,
        u.DataCrearii,
    u.ModificatDe,
        u.DataUltimeiModificari
    FROM Utilizatori u
    INNER JOIN PersonalMedical pm ON u.PersonalMedicalID = pm.PersonalID
    WHERE u.UtilizatorID = @UtilizatorID;
END
GO

-- ========================================
-- 4. sp_Utilizatori_GetByUsername
-- Descriere: Returneaza un utilizator dupa Username (pentru autentificare)
-- ========================================

IF OBJECT_ID('sp_Utilizatori_GetByUsername', 'P') IS NOT NULL
    DROP PROCEDURE sp_Utilizatori_GetByUsername
GO

CREATE PROCEDURE sp_Utilizatori_GetByUsername
    @Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UtilizatorID,
        u.PersonalMedicalID,
        u.Username,
        u.Email,
        u.PasswordHash,
        u.Salt,
        u.Rol,
  u.EsteActiv,
        u.DataCreare,
        u.DataUltimaAutentificare,
        u.NumarIncercariEsuate,
    u.DataBlocare,
        pm.Nume,
 pm.Prenume,
  pm.Nume + ' ' + pm.Prenume AS NumeComplet,
     pm.Specializare,
    pm.Departament
    FROM Utilizatori u
    INNER JOIN PersonalMedical pm ON u.PersonalMedicalID = pm.PersonalID
    WHERE u.Username = @Username;
END
GO

-- ========================================
-- 5. sp_Utilizatori_GetByEmail
-- Descriere: Returneaza un utilizator dupa Email
-- ========================================

IF OBJECT_ID('sp_Utilizatori_GetByEmail', 'P') IS NOT NULL
    DROP PROCEDURE sp_Utilizatori_GetByEmail
GO

CREATE PROCEDURE sp_Utilizatori_GetByEmail
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
  u.UtilizatorID,
        u.PersonalMedicalID,
        u.Username,
        u.Email,
 u.Rol,
  u.EsteActiv,
        pm.Nume + ' ' + pm.Prenume AS NumeComplet
    FROM Utilizatori u
    INNER JOIN PersonalMedical pm ON u.PersonalMedicalID = pm.PersonalID
    WHERE u.Email = @Email;
END
GO

-- ========================================
-- 6. sp_Utilizatori_Create
-- Descriere: Creeaza un utilizator nou
-- ========================================

IF OBJECT_ID('sp_Utilizatori_Create', 'P') IS NOT NULL
    DROP PROCEDURE sp_Utilizatori_Create
GO

CREATE PROCEDURE sp_Utilizatori_Create
    @PersonalMedicalID UNIQUEIDENTIFIER,
    @Username NVARCHAR(100),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(256),
    @Salt NVARCHAR(100),
    @Rol NVARCHAR(50),
    @EsteActiv BIT = 1,
  @CreatDe NVARCHAR(100) = 'System'
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Verificare PersonalMedical exista si este activ
        IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE PersonalID = @PersonalMedicalID AND EsteActiv = 1)
        BEGIN
      RAISERROR('PersonalMedical ID nu exista sau nu este activ', 16, 1);
         RETURN;
        END
        
        -- Verificare Username nu exista deja
    IF EXISTS (SELECT 1 FROM Utilizatori WHERE Username = @Username)
        BEGIN
       RAISERROR('Username-ul exista deja', 16, 1);
    RETURN;
  END

        -- Verificare Email nu exista deja
        IF EXISTS (SELECT 1 FROM Utilizatori WHERE Email = @Email)
        BEGIN
RAISERROR('Email-ul exista deja', 16, 1);
            RETURN;
   END
        
        -- Verificare PersonalMedicalID nu este deja asociat cu alt utilizator
        IF EXISTS (SELECT 1 FROM Utilizatori WHERE PersonalMedicalID = @PersonalMedicalID)
        BEGIN
            RAISERROR('Acest membru al personalului medical are deja un cont de utilizator', 16, 1);
            RETURN;
   END
        
        DECLARE @UtilizatorID UNIQUEIDENTIFIER = NEWID();
        
        INSERT INTO Utilizatori (
          UtilizatorID,
    PersonalMedicalID,
     Username,
    Email,
   PasswordHash,
      Salt,
            Rol,
     EsteActiv,
            CreatDe,
     DataCrearii,
            DataUltimeiModificari
        )
 VALUES (
   @UtilizatorID,
            @PersonalMedicalID,
    @Username,
            @Email,
@PasswordHash,
            @Salt,
    @Rol,
    @EsteActiv,
            @CreatDe,
   GETDATE(),
          GETDATE()
        );
        
        COMMIT TRANSACTION;
        
        -- Return the created user
        EXEC sp_Utilizatori_GetById @UtilizatorID;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
END CATCH
END
GO

-- ========================================
-- 7. sp_Utilizatori_Update
-- Descriere: Actualizeaza un utilizator existent
-- ========================================

IF OBJECT_ID('sp_Utilizatori_Update', 'P') IS NOT NULL
    DROP PROCEDURE sp_Utilizatori_Update
GO

CREATE PROCEDURE sp_Utilizatori_Update
    @UtilizatorID UNIQUEIDENTIFIER,
    @Username NVARCHAR(100),
    @Email NVARCHAR(100),
    @Rol NVARCHAR(50),
    @EsteActiv BIT,
    @ModificatDe NVARCHAR(100) = 'System'
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
    -- Verificare utilizator exista
        IF NOT EXISTS (SELECT 1 FROM Utilizatori WHERE UtilizatorID = @UtilizatorID)
   BEGIN
            RAISERROR('Utilizatorul nu exista', 16, 1);
   RETURN;
        END
        
        -- Verificare Username nu este folosit de alt utilizator
        IF EXISTS (SELECT 1 FROM Utilizatori WHERE Username = @Username AND UtilizatorID <> @UtilizatorID)
        BEGIN
            RAISERROR('Username-ul este folosit de alt utilizator', 16, 1);
 RETURN;
END
        
        -- Verificare Email nu este folosit de alt utilizator
   IF EXISTS (SELECT 1 FROM Utilizatori WHERE Email = @Email AND UtilizatorID <> @UtilizatorID)
        BEGIN
            RAISERROR('Email-ul este folosit de alt utilizator', 16, 1);
   RETURN;
     END
      
        UPDATE Utilizatori
        SET 
      Username = @Username,
            Email = @Email,
            Rol = @Rol,
    EsteActiv = @EsteActiv,
     ModificatDe = @ModificatDe,
        DataUltimeiModificari = GETDATE()
        WHERE UtilizatorID = @UtilizatorID;
        
        COMMIT TRANSACTION;
 
 -- Return the updated user
        EXEC sp_Utilizatori_GetById @UtilizatorID;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
   RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- ========================================
-- 8. sp_Utilizatori_ChangePassword
-- Descriere: Schimba parola unui utilizator
-- ========================================

IF OBJECT_ID('sp_Utilizatori_ChangePassword', 'P') IS NOT NULL
    DROP PROCEDURE sp_Utilizatori_ChangePassword
GO

CREATE PROCEDURE sp_Utilizatori_ChangePassword
    @UtilizatorID UNIQUEIDENTIFIER,
    @NewPasswordHash NVARCHAR(256),
    @NewSalt NVARCHAR(100),
@ModificatDe NVARCHAR(100) = 'System'
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
     UPDATE Utilizatori
        SET 
  PasswordHash = @NewPasswordHash,
            Salt = @NewSalt,
            NumarIncercariEsuate = 0,
     DataBlocare = NULL,
            TokenResetareParola = NULL,
          DataExpirareToken = NULL,
            ModificatDe = @ModificatDe,
        DataUltimeiModificari = GETDATE()
        WHERE UtilizatorID = @UtilizatorID;
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success, 'Parola schimbata cu succes' AS Message;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
     DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
   RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- ========================================
-- 9. sp_Utilizatori_UpdateUltimaAutentificare
-- Descriere: Actualizeaza data ultimei autentificari si reseteaza incercarile esuate
-- ========================================

IF OBJECT_ID('sp_Utilizatori_UpdateUltimaAutentificare', 'P') IS NOT NULL
    DROP PROCEDURE sp_Utilizatori_UpdateUltimaAutentificare
GO

CREATE PROCEDURE sp_Utilizatori_UpdateUltimaAutentificare
    @UtilizatorID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
  
    UPDATE Utilizatori
    SET 
        DataUltimaAutentificare = GETDATE(),
 NumarIncercariEsuate = 0,
        DataBlocare = NULL
    WHERE UtilizatorID = @UtilizatorID;
    
    SELECT 1 AS Success;
END
GO

-- ========================================
-- 10. sp_Utilizatori_IncrementIncercariEsuate
-- Descriere: Incrementeaza numarul de incercari esuate si blocheaza dupa 5 incercari
-- ========================================

IF OBJECT_ID('sp_Utilizatori_IncrementIncercariEsuate', 'P') IS NOT NULL
    DROP PROCEDURE sp_Utilizatori_IncrementIncercariEsuate
GO

CREATE PROCEDURE sp_Utilizatori_IncrementIncercariEsuate
  @UtilizatorID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NumarIncercari INT;
    
    UPDATE Utilizatori
    SET 
        NumarIncercariEsuate = NumarIncercariEsuate + 1,
        @NumarIncercari = NumarIncercariEsuate + 1
    WHERE UtilizatorID = @UtilizatorID;
    
    -- Blocheaza dupa 5 incercari esuate
    IF @NumarIncercari >= 5
    BEGIN
        UPDATE Utilizatori
        SET DataBlocare = GETDATE()
        WHERE UtilizatorID = @UtilizatorID;
        
        SELECT 0 AS Success, 'Cont blocat dupa 5 incercari esuate' AS Message;
  END
    ELSE
    BEGIN
     SELECT 1 AS Success, CAST((5 - @NumarIncercari) AS NVARCHAR) + ' incercari ramase' AS Message;
    END
END
GO

-- ========================================
-- 11. sp_Utilizatori_SetTokenResetareParola
-- Descriere: Seteaza token pentru resetare parola
-- ========================================

IF OBJECT_ID('sp_Utilizatori_SetTokenResetareParola', 'P') IS NOT NULL
    DROP PROCEDURE sp_Utilizatori_SetTokenResetareParola
GO

CREATE PROCEDURE sp_Utilizatori_SetTokenResetareParola
    @Email NVARCHAR(100),
    @Token NVARCHAR(256),
    @DataExpirare DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
 UPDATE Utilizatori
    SET 
        TokenResetareParola = @Token,
        DataExpirareToken = @DataExpirare
    WHERE Email = @Email;
    
    IF @@ROWCOUNT > 0
      SELECT 1 AS Success, 'Token setat cu succes' AS Message;
ELSE
 SELECT 0 AS Success, 'Email-ul nu exista' AS Message;
END
GO

-- ========================================
-- 12. sp_Utilizatori_GetStatistics
-- Descriere: Returneaza statistici despre utilizatori
-- ========================================

IF OBJECT_ID('sp_Utilizatori_GetStatistics', 'P') IS NOT NULL
    DROP PROCEDURE sp_Utilizatori_GetStatistics
GO

CREATE PROCEDURE sp_Utilizatori_GetStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT Categorie, Numar, Activi
    FROM (
        SELECT 
          'Total' AS Categorie,
            COUNT(*) AS Numar,
   SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END) AS Activi,
            1 AS SortOrder
        FROM Utilizatori
      
        UNION ALL   
        SELECT 
 'Administratori' AS Categorie,
      COUNT(*) AS Numar,
   SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END) AS Activi,
      2 AS SortOrder
        FROM Utilizatori
 WHERE Rol = 'Administrator'
        
        UNION ALL
        
        SELECT 
    'Doctori' AS Categorie,
  COUNT(*) AS Numar,
  SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END) AS Activi,
            3 AS SortOrder
        FROM Utilizatori
     WHERE Rol = 'Doctor'
     
        UNION ALL
     
        SELECT 
      'Asistenti' AS Categorie,
      COUNT(*) AS Numar,
            SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END) AS Activi,
       4 AS SortOrder
        FROM Utilizatori
        WHERE Rol = 'Asistent'
        
        UNION ALL
        
        SELECT 
     'Receptioneri' AS Categorie,
      COUNT(*) AS Numar,
      SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END) AS Activi,
            5 AS SortOrder
        FROM Utilizatori
        WHERE Rol = 'Receptioner'
        
        UNION ALL
        
        SELECT 
            'Blocati' AS Categorie,
            COUNT(*) AS Numar,
    0 AS Activi,
        6 AS SortOrder
        FROM Utilizatori
 WHERE DataBlocare IS NOT NULL
) AS Stats
 ORDER BY SortOrder;
END
GO

PRINT '========================================';
PRINT '12 Stored Procedures create cu succes!';
PRINT '========================================';
PRINT '';
PRINT 'Stored Procedures disponibile:';
PRINT '  1. sp_Utilizatori_GetAll';
PRINT '  2. sp_Utilizatori_GetCount';
PRINT '  3. sp_Utilizatori_GetById';
PRINT '  4. sp_Utilizatori_GetByUsername';
PRINT '  5. sp_Utilizatori_GetByEmail';
PRINT '  6. sp_Utilizatori_Create';
PRINT '  7. sp_Utilizatori_Update';
PRINT '  8. sp_Utilizatori_ChangePassword';
PRINT '  9. sp_Utilizatori_UpdateUltimaAutentificare';
PRINT ' 10. sp_Utilizatori_IncrementIncercariEsuate';
PRINT ' 11. sp_Utilizatori_SetTokenResetareParola';
PRINT ' 12. sp_Utilizatori_GetStatistics';
PRINT '';
