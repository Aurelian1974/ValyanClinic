-- ========================================
-- Stored Procedure: AnalizeMedicale_Search
-- C?utare analize în nomenclator
-- ========================================
USE [ValyanMed]
GO

CREATE OR ALTER PROCEDURE [dbo].[AnalizeMedicale_Search]
    @SearchTerm NVARCHAR(200) = NULL,
    @CategorieID UNIQUEIDENTIFIER = NULL,
    @LaboratorID UNIQUEIDENTIFIER = NULL,
    @DoarActive BIT = 1,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Query principal
    ;WITH FilteredAnalize AS (
        SELECT 
            a.AnalizaID,
            a.NumeAnaliza,
            a.NumeScurt,
            a.Acronime,
            a.Pret,
            a.Moneda,
            c.NumeCategorie,
            c.Icon AS CategorieIcon,
            l.NumeLaborator,
            l.Acronim AS LaboratorAcronim
        FROM dbo.AnalizeMedicale a
        INNER JOIN dbo.AnalizeMedicaleCategorii c ON a.CategorieID = c.CategorieID
        INNER JOIN dbo.AnalizeMedicaleLaboratoare l ON a.LaboratorID = l.LaboratorID
        WHERE (@DoarActive = 0 OR a.EsteActiv = 1)
          AND (@CategorieID IS NULL OR a.CategorieID = @CategorieID)
          AND (@LaboratorID IS NULL OR a.LaboratorID = @LaboratorID)
          AND (
              @SearchTerm IS NULL 
              OR a.NumeAnaliza LIKE '%' + @SearchTerm + '%'
              OR a.Acronime LIKE '%' + @SearchTerm + '%'
              OR a.NumeScurt LIKE '%' + @SearchTerm + '%'
          )
    )
    SELECT *
    FROM FilteredAnalize
    ORDER BY NumeAnaliza
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Total count
    SELECT COUNT(*) AS TotalRecords
    FROM dbo.AnalizeMedicale a
    WHERE (@DoarActive = 0 OR a.EsteActiv = 1)
      AND (@CategorieID IS NULL OR a.CategorieID = @CategorieID)
      AND (@LaboratorID IS NULL OR a.LaboratorID = @LaboratorID)
      AND (
          @SearchTerm IS NULL 
          OR a.NumeAnaliza LIKE '%' + @SearchTerm + '%'
          OR a.Acronime LIKE '%' + @SearchTerm + '%'
          OR a.NumeScurt LIKE '%' + @SearchTerm + '%'
      );
END
GO

-- ========================================
-- Stored Procedure: AnalizeMedicale_GetById
-- ========================================
CREATE OR ALTER PROCEDURE [dbo].[AnalizeMedicale_GetById]
    @AnalizaID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.AnalizaID,
        a.LaboratorID,
        a.CategorieID,
        a.CodAnaliza,
        a.NumeAnaliza,
        a.NumeScurt,
        a.Acronime,
        a.Descriere,
        a.PreparareaTestului,
        a.Material,
        a.TermenProcesare,
        a.Pret,
        a.Moneda,
        a.PretActualizatLa,
        a.URLSursa,
        a.EsteActiv,
        a.DataScraping,
        a.DataCreare,
        a.DataUltimaActualizare,
        -- Categorie
        c.NumeCategorie,
        c.Icon AS CategorieIcon,
        -- Laborator
        l.NumeLaborator,
        l.Acronim AS LaboratorAcronim,
        l.Website AS LaboratorWebsite
    FROM dbo.AnalizeMedicale a
    INNER JOIN dbo.AnalizeMedicaleCategorii c ON a.CategorieID = c.CategorieID
    INNER JOIN dbo.AnalizeMedicaleLaboratoare l ON a.LaboratorID = l.LaboratorID
    WHERE a.AnalizaID = @AnalizaID;
END
GO

-- ========================================
-- Stored Procedure: AnalizeMedicaleCategorii_GetAll
-- ========================================
CREATE OR ALTER PROCEDURE [dbo].[AnalizeMedicaleCategorii_GetAll]
    @DoarActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.CategorieID,
        c.NumeCategorie,
        c.Descriere,
        c.Icon,
        c.OrdineAfisare,
        c.EsteActiv,
        COUNT(a.AnalizaID) AS NumarAnalize
    FROM dbo.AnalizeMedicaleCategorii c
    LEFT JOIN dbo.AnalizeMedicale a ON c.CategorieID = a.CategorieID AND a.EsteActiv = 1
    WHERE (@DoarActive = 0 OR c.EsteActiv = 1)
    GROUP BY c.CategorieID, c.NumeCategorie, c.Descriere, c.Icon, c.OrdineAfisare, c.EsteActiv
    ORDER BY c.OrdineAfisare, c.NumeCategorie;
END
GO

PRINT '? Stored Procedures create cu succes!';
GO
