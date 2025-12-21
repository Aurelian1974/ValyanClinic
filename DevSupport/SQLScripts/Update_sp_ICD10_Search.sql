-- Update sp_ICD10_Search to return direct column names for Dapper mapping
ALTER PROCEDURE dbo.sp_ICD10_Search
    @SearchTerm NVARCHAR(100),
    @Category NVARCHAR(50) = NULL,
    @ChapterNumber INT = NULL,
    @OnlyCommon BIT = 0,
    @OnlyLeaf BIT = 1,
    @OnlyTranslated BIT = 0,
    @MaxResults INT = 50,
    @Language NVARCHAR(2) = 'ro'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SearchPattern NVARCHAR(102) = '%' + @SearchTerm + '%'

    SELECT TOP (@MaxResults)
        c.ICD10_ID,
        c.Code,
        c.FullCode,
        c.ShortDescriptionRo,
        c.LongDescriptionRo,
        c.ShortDescriptionEn,
        c.LongDescriptionEn,
        c.Category,
        c.Severity,
        c.IsCommon,
        c.IsLeafNode,
        c.IsBillable,
        c.IsTranslated,
        ch.ChapterNumber,
        CASE @Language
            WHEN 'en' THEN ch.DescriptionEn
            ELSE COALESCE(ch.DescriptionRo, ch.DescriptionEn)
        END AS ChapterDescription,
        CASE
            WHEN c.Code = @SearchTerm THEN 100
            WHEN c.Code LIKE @SearchTerm + '%' THEN 90
            WHEN c.ShortDescriptionRo LIKE @SearchTerm + '%' THEN 80
            WHEN c.ShortDescriptionEn LIKE @SearchTerm + '%' THEN 75
            WHEN c.Code LIKE @SearchPattern THEN 70
            WHEN c.ShortDescriptionRo LIKE @SearchPattern THEN 60
            WHEN c.ShortDescriptionEn LIKE @SearchPattern THEN 55
            WHEN c.SearchTermsRo LIKE @SearchPattern THEN 50
            WHEN c.SearchTermsEn LIKE @SearchPattern THEN 45
            ELSE 10
        END AS RelevanceScore
    FROM dbo.ICD10_Codes c
    INNER JOIN dbo.ICD10_Chapters ch ON c.ChapterId = ch.ChapterId
    WHERE c.IsActive = 1
      AND (
          c.Code LIKE @SearchPattern
          OR c.ShortDescriptionRo LIKE @SearchPattern
          OR c.ShortDescriptionEn LIKE @SearchPattern
          OR c.LongDescriptionRo LIKE @SearchPattern
          OR c.LongDescriptionEn LIKE @SearchPattern
          OR c.SearchTermsRo LIKE @SearchPattern
          OR c.SearchTermsEn LIKE @SearchPattern
      )
      AND (@Category IS NULL OR c.Category = @Category)
      AND (@ChapterNumber IS NULL OR ch.ChapterNumber = @ChapterNumber)
      AND (@OnlyCommon = 0 OR c.IsCommon = 1)
      AND (@OnlyLeaf = 0 OR c.IsLeafNode = 1)
      AND (@OnlyTranslated = 0 OR c.IsTranslated = 1)
    ORDER BY
        RelevanceScore DESC,
        c.IsCommon DESC,
        c.Code
END
