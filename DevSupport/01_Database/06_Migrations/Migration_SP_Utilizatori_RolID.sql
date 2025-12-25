-- ========================================
-- Script: Actualizare Stored Procedures Utilizatori pentru RolID
-- Database: ValyanMed
-- Data: 2025-12-25
-- ========================================

USE [ValyanMed]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ========================================
-- sp_Utilizatori_GetAll - Actualizat pentru RolID
-- ========================================
ALTER PROCEDURE [dbo].[sp_Utilizatori_GetAll]
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SearchText NVARCHAR(100) = NULL,
    @Rol NVARCHAR(50) = NULL,  -- Acum este denumirea rolului pentru filtrare
    @EsteActiv BIT = NULL,
    @SortColumn NVARCHAR(50) = 'Username',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        u.UtilizatorID,
        u.PersonalMedicalID,
        u.Username,
        u.Email,
        u.PasswordHash,
        u.Salt,
        u.RolID,
        r.Denumire AS RolDenumire,
        u.EsteActiv,
        u.DataCreare,
        u.DataUltimaAutentificare,
        u.NumarIncercariEsuate,
        u.DataBlocare,
        u.TokenResetareParola,
        u.DataExpirareToken,
        u.CreatDe,
        u.DataCrearii,
        u.ModificatDe,
        u.DataUltimeiModificari
    FROM Utilizatori u
    LEFT JOIN Roluri r ON u.RolID = r.RolID
    WHERE 
        (@SearchText IS NULL OR 
         u.Username LIKE '%' + @SearchText + '%' OR 
         u.Email LIKE '%' + @SearchText + '%')
        AND (@Rol IS NULL OR r.Denumire = @Rol)
        AND (@EsteActiv IS NULL OR u.EsteActiv = @EsteActiv)
    ORDER BY
        CASE WHEN @SortColumn = 'Username' AND @SortDirection = 'ASC' THEN u.Username END ASC,
        CASE WHEN @SortColumn = 'Username' AND @SortDirection = 'DESC' THEN u.Username END DESC,
        CASE WHEN @SortColumn = 'Email' AND @SortDirection = 'ASC' THEN u.Email END ASC,
        CASE WHEN @SortColumn = 'Email' AND @SortDirection = 'DESC' THEN u.Email END DESC,
        CASE WHEN @SortColumn = 'RolDenumire' AND @SortDirection = 'ASC' THEN r.Denumire END ASC,
        CASE WHEN @SortColumn = 'RolDenumire' AND @SortDirection = 'DESC' THEN r.Denumire END DESC,
        CASE WHEN @SortColumn = 'DataCreare' AND @SortDirection = 'ASC' THEN u.DataCreare END ASC,
        CASE WHEN @SortColumn = 'DataCreare' AND @SortDirection = 'DESC' THEN u.DataCreare END DESC,
        u.Username ASC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT 'sp_Utilizatori_GetAll actualizat.'
GO

-- ========================================
-- sp_Utilizatori_GetCount - Actualizat pentru RolID
-- ========================================
ALTER PROCEDURE [dbo].[sp_Utilizatori_GetCount]
    @SearchText NVARCHAR(100) = NULL,
    @Rol NVARCHAR(50) = NULL,
    @EsteActiv BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*)
    FROM Utilizatori u
    LEFT JOIN Roluri r ON u.RolID = r.RolID
    WHERE 
        (@SearchText IS NULL OR 
         u.Username LIKE '%' + @SearchText + '%' OR 
         u.Email LIKE '%' + @SearchText + '%')
        AND (@Rol IS NULL OR r.Denumire = @Rol)
        AND (@EsteActiv IS NULL OR u.EsteActiv = @EsteActiv);
END
GO

PRINT 'sp_Utilizatori_GetCount actualizat.'
GO

-- ========================================
-- sp_Utilizatori_GetById - Actualizat pentru RolID
-- ========================================
ALTER PROCEDURE [dbo].[sp_Utilizatori_GetById]
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
        u.RolID,
        r.Denumire AS RolDenumire,
        u.EsteActiv,
        u.DataCreare,
        u.DataUltimaAutentificare,
        u.NumarIncercariEsuate,
        u.DataBlocare,
        u.TokenResetareParola,
        u.DataExpirareToken,
        u.CreatDe,
        u.DataCrearii,
        u.ModificatDe,
        u.DataUltimeiModificari,
        -- PersonalMedical data
        pm.Nume,
        pm.Prenume,
        s.Denumire AS Specializare,
        d.Denumire AS Departament,
        p.Denumire AS Pozitie,
        pm.Telefon,
        pm.Email AS EmailPersonalMedical
    FROM Utilizatori u
    LEFT JOIN Roluri r ON u.RolID = r.RolID
    LEFT JOIN PersonalMedical pm ON u.PersonalMedicalID = pm.PersonalID
    LEFT JOIN Specializari s ON pm.SpecializareID = s.SpecializareID
    LEFT JOIN Departamente d ON pm.DepartamentID = d.DepartamentID
    LEFT JOIN Pozitii p ON pm.PozitieID = p.PozitieID
    WHERE u.UtilizatorID = @UtilizatorID;
END
GO

PRINT 'sp_Utilizatori_GetById actualizat.'
GO

-- ========================================
-- sp_Utilizatori_GetByUsername - Actualizat pentru RolID
-- ========================================
ALTER PROCEDURE [dbo].[sp_Utilizatori_GetByUsername]
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
        u.RolID,
        r.Denumire AS RolDenumire,
        u.EsteActiv,
        u.DataCreare,
        u.DataUltimaAutentificare,
        u.NumarIncercariEsuate,
        u.DataBlocare,
        u.TokenResetareParola,
        u.DataExpirareToken,
        u.CreatDe,
        u.DataCrearii,
        u.ModificatDe,
        u.DataUltimeiModificari,
        u.LockoutEndDateUtc,
        u.AccessFailedCount,
        u.LockoutEnabled,
        u.LastPasswordChangedDate,
        u.PasswordExpirationDate,
        u.MustChangePasswordOnNextLogin,
        u.LastLoginDate,
        u.LastLoginIP
    FROM Utilizatori u
    LEFT JOIN Roluri r ON u.RolID = r.RolID
    WHERE u.Username = @Username;
END
GO

PRINT 'sp_Utilizatori_GetByUsername actualizat.'
GO

-- ========================================
-- sp_Utilizatori_GetByEmail - Actualizat pentru RolID
-- ========================================
ALTER PROCEDURE [dbo].[sp_Utilizatori_GetByEmail]
    @Email NVARCHAR(100)
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
        u.RolID,
        r.Denumire AS RolDenumire,
        u.EsteActiv,
        u.DataCreare,
        u.DataUltimaAutentificare,
        u.NumarIncercariEsuate,
        u.DataBlocare,
        u.TokenResetareParola,
        u.DataExpirareToken,
        u.CreatDe,
        u.DataCrearii,
        u.ModificatDe,
        u.DataUltimeiModificari,
        u.LockoutEndDateUtc,
        u.AccessFailedCount,
        u.LockoutEnabled,
        u.LastPasswordChangedDate,
        u.PasswordExpirationDate,
        u.MustChangePasswordOnNextLogin,
        u.LastLoginDate,
        u.LastLoginIP
    FROM Utilizatori u
    LEFT JOIN Roluri r ON u.RolID = r.RolID
    WHERE u.Email = @Email;
END
GO

PRINT 'sp_Utilizatori_GetByEmail actualizat.'
GO

-- ========================================
-- sp_Utilizatori_Create - Actualizat pentru RolID
-- ========================================
ALTER PROCEDURE [dbo].[sp_Utilizatori_Create]
    @PersonalMedicalID UNIQUEIDENTIFIER = NULL,
    @Username NVARCHAR(100),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(256),
    @Salt NVARCHAR(100) = NULL,
    @RolID UNIQUEIDENTIFIER,
    @EsteActiv BIT = 1,
    @CreatDe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UtilizatorID UNIQUEIDENTIFIER = NEWID();
    DECLARE @Now DATETIME2 = GETDATE();

    INSERT INTO Utilizatori (
        UtilizatorID,
        PersonalMedicalID,
        Username,
        Email,
        PasswordHash,
        Salt,
        RolID,
        EsteActiv,
        DataCreare,
        NumarIncercariEsuate,
        CreatDe,
        DataCrearii,
        DataUltimeiModificari,
        AccessFailedCount,
        LockoutEnabled,
        MustChangePasswordOnNextLogin
    )
    VALUES (
        @UtilizatorID,
        @PersonalMedicalID,
        @Username,
        @Email,
        @PasswordHash,
        @Salt,
        @RolID,
        @EsteActiv,
        @Now,
        0,
        @CreatDe,
        @Now,
        @Now,
        0,
        1,
        0
    );

    -- Return created record
    SELECT 
        u.UtilizatorID,
        u.PersonalMedicalID,
        u.Username,
        u.Email,
        u.RolID,
        r.Denumire AS RolDenumire,
        u.EsteActiv,
        u.DataCreare,
        u.CreatDe,
        u.DataCrearii
    FROM Utilizatori u
    LEFT JOIN Roluri r ON u.RolID = r.RolID
    WHERE u.UtilizatorID = @UtilizatorID;
END
GO

PRINT 'sp_Utilizatori_Create actualizat.'
GO

-- ========================================
-- sp_Utilizatori_Update - Actualizat pentru RolID
-- ========================================
ALTER PROCEDURE [dbo].[sp_Utilizatori_Update]
    @UtilizatorID UNIQUEIDENTIFIER,
    @Username NVARCHAR(100) = NULL,
    @Email NVARCHAR(100) = NULL,
    @RolID UNIQUEIDENTIFIER = NULL,
    @EsteActiv BIT = NULL,
    @ModificatDe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Utilizatori
    SET 
        Username = ISNULL(@Username, Username),
        Email = ISNULL(@Email, Email),
        RolID = ISNULL(@RolID, RolID),
        EsteActiv = ISNULL(@EsteActiv, EsteActiv),
        ModificatDe = @ModificatDe,
        DataUltimeiModificari = GETDATE()
    WHERE UtilizatorID = @UtilizatorID;

    -- Return updated record
    SELECT 
        u.UtilizatorID,
        u.PersonalMedicalID,
        u.Username,
        u.Email,
        u.RolID,
        r.Denumire AS RolDenumire,
        u.EsteActiv,
        u.ModificatDe,
        u.DataUltimeiModificari
    FROM Utilizatori u
    LEFT JOIN Roluri r ON u.RolID = r.RolID
    WHERE u.UtilizatorID = @UtilizatorID;
END
GO

PRINT 'sp_Utilizatori_Update actualizat.'
GO

-- ========================================
-- sp_Utilizatori_GetStatistics - Actualizat pentru RolID
-- ========================================
ALTER PROCEDURE [dbo].[sp_Utilizatori_GetStatistics]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        COUNT(*) AS TotalUtilizatori,
        SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END) AS UtilizatoriActivi,
        SUM(CASE WHEN EsteActiv = 0 THEN 1 ELSE 0 END) AS UtilizatoriInactivi,
        SUM(CASE WHEN DataBlocare IS NOT NULL THEN 1 ELSE 0 END) AS UtilizatoriBlocati
    FROM Utilizatori;

    -- Stats per rol
    SELECT 
        r.Denumire AS Rol,
        COUNT(u.UtilizatorID) AS NumarUtilizatori
    FROM Roluri r
    LEFT JOIN Utilizatori u ON r.RolID = u.RolID
    WHERE r.Este_Activ = 1
    GROUP BY r.RolID, r.Denumire
    ORDER BY r.Ordine_Afisare;
END
GO

PRINT 'sp_Utilizatori_GetStatistics actualizat.'
GO

PRINT ''
PRINT '=========================================='
PRINT 'STORED PROCEDURES ACTUALIZATE CU SUCCES!'
PRINT '=========================================='
GO
