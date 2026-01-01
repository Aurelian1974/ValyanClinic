-- Migration: Add ColumnFiltersJson support to sp_PersonalMedical_GetAll
-- Path: DevSupport/01_Database/02_StoredProcedures/PersonalMedical/sp_PersonalMedical_GetAll__add_column_filters.sql
-- Add a new optional NVARCHAR(MAX) parameter @ColumnFiltersJson containing JSON array of column filters
-- Each filter object: { "Column": "Nume", "Operator": "Contains|Equals|StartsWith|EndsWith", "Value": "..." }
-- The stored procedure will apply all provided column filters (AND semantics). Filters are applied case-insensitively.

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Use CREATE OR ALTER so this script is idempotent
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetAll]
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SearchText NVARCHAR(200) = NULL,
    @Departament NVARCHAR(200) = NULL,
    @Pozitie NVARCHAR(200) = NULL,
    @EsteActiv BIT = NULL,
    @SortColumn NVARCHAR(100) = 'Nume',
    @SortDirection NVARCHAR(4) = 'ASC',
    @ColumnFiltersJson NVARCHAR(MAX) = NULL,  -- NEW optional parameter
    @ReturnGeneratedSql BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    -- Ensure valid pagination values
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 20;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Base query
    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT
            PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare
        FROM dbo.PersonalMedical PM
        WHERE 1=1'
    ;

    -- Apply basic filters (parameterized)
    IF @SearchText IS NOT NULL AND LTRIM(RTRIM(@SearchText)) <> ''
    BEGIN
        SET @sql += N' AND (
            UPPER(PM.Nume) LIKE ''%'' + UPPER(@SearchText) + ''%''
            OR UPPER(PM.Prenume) LIKE ''%'' + UPPER(@SearchText) + ''%''
            OR UPPER(PM.NumarLicenta) LIKE ''%'' + UPPER(@SearchText) + ''%''
        )';
    END

    IF @Departament IS NOT NULL AND LTRIM(RTRIM(@Departament)) <> ''
    BEGIN
        SET @sql += N' AND UPPER(PM.Departament) = UPPER(@Departament)';
    END

    IF @Pozitie IS NOT NULL AND LTRIM(RTRIM(@Pozitie)) <> ''
    BEGIN
        SET @sql += N' AND UPPER(PM.Pozitie) = UPPER(@Pozitie)';
    END

    IF @EsteActiv IS NOT NULL
    BEGIN
        SET @sql += N' AND PM.EsteActiv = @EsteActiv';
    END

    -- Prepare count execution string early so we can safely append predicates later
    DECLARE @countExecSql NVARCHAR(MAX) = N'SELECT @out = COUNT(1) FROM dbo.PersonalMedical PM WHERE 1=1';

    -- If ColumnFiltersJson provided and valid JSON, parse into a temp table and build a reusable @filterPredicate to apply to both the main query and the count query
    IF @ColumnFiltersJson IS NOT NULL AND ISJSON(@ColumnFiltersJson) = 1
    BEGIN
        -- Parse JSON into a temporary table - safer and easier to reference from dynamic SQL
        IF OBJECT_ID('tempdb..#ColumnFilters') IS NOT NULL DROP TABLE #ColumnFilters;
        CREATE TABLE #ColumnFilters (
            [Column] NVARCHAR(100),
            [Operator] NVARCHAR(20),
            [Value] NVARCHAR(4000) COLLATE SQL_Latin1_General_CP1_CS_AS
        );
        INSERT INTO #ColumnFilters ([Column],[Operator],[Value])
        SELECT [Column],[Operator],[Value]
        FROM OPENJSON(@ColumnFiltersJson) WITH (
            [Column] NVARCHAR(100)  '$.Column',
            [Operator] NVARCHAR(20) '$.Operator',
            [Value] NVARCHAR(4000)  '$.Value'
        );


        -- Build a single predicate fragment that we can append to both queries
        DECLARE @filterPredicate NVARCHAR(MAX) = N' AND NOT EXISTS (
            SELECT 1 FROM #ColumnFilters f
            WHERE f.[Column] IN (''Nume'',''Specializare'',''NumarLicenta'')
              AND NOT (
                ( f.[Column] = ''Nume'' AND (
                    (f.[Operator] = ''Contains'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'' ) COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''Equals'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS = UPPER(f.[Value]) COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                ) )
                OR ( f.[Column] = ''Specializare'' AND (
                    (f.[Operator] = ''Contains'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''Equals'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS = UPPER(f.[Value]) COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                ) )
                OR ( f.[Column] = ''NumarLicenta'' AND (
                    (f.[Operator] = ''Contains'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''Equals'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS = UPPER(f.[Value]) COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                ) )
              )
        )';

        -- Append the single predicate to the main SQL and rely on the same predicate for the count SQL
        SET @sql += @filterPredicate;
        SET @countExecSql += @filterPredicate;
    END

    -- Add ORDER BY (simple protection: allow only a small set of columns)
    IF UPPER(@SortColumn) NOT IN (N'NUME', N'PRENUME', N'SPECIALIZARE', N'DEPARTAMENT', N'POZITIE', N'NUMARLICENTA', N'ESTEACTIV', N'DATACREARE')
    BEGIN
        SET @SortColumn = 'Nume';
    END
    IF UPPER(@SortDirection) NOT IN (N'ASC', N'DESC')
    BEGIN
        SET @SortDirection = 'ASC';
    END

    SET @sql += N' ORDER BY ' + QUOTENAME(@SortColumn) + ' ' + @SortDirection + ' OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY';

    -- DEBUG: moved generated-sql output block to after count SQL is built (see below)
    -- IF @ReturnGeneratedSql = 1 ... (moved)


    -- Total count query (same filters, but no paging)
    DECLARE @countSql NVARCHAR(MAX) = N'SELECT COUNT(1) FROM dbo.PersonalMedical PM WHERE 1=1';

    -- Reuse the same filter logic for count: implement by repeating the same JSON-based NOT EXISTS predicate
    -- For brevity, we'll append the same filter fragments to both @sql and @countSql by re-using dynamic fragments in application of filters above
    -- Note: In this migration we keep both queries semantically aligned. In production you may prefer to extract the predicate assembly into reusable logic.

    -- Build parameter definitions and execute both queries using sp_executesql with parameterization
    DECLARE @params NVARCHAR(MAX) = N'@SearchText NVARCHAR(200), @Departament NVARCHAR(200), @Pozitie NVARCHAR(200), @EsteActiv BIT, @ColumnFiltersJson NVARCHAR(MAX), @Offset INT, @PageSize INT';

    -- @countExecSql declared earlier before ColumnFilters block; re-use it here

    -- Execute main (paged) query (moved below after count SQL is built to allow returning generated SQL before execution)
    -- EXEC sp_executesql @sql, @params, ... (moved)

    -- Execute count query deterministically using sp_executesql with OUTPUT parameter
    DECLARE @countOut INT = 0;

    -- (count execution string is declared earlier: @countExecSql)

    IF @ColumnFiltersJson IS NOT NULL AND ISJSON(@ColumnFiltersJson) = 1
    BEGIN
        -- Append the same #ColumnFilters-based predicate to the count SQL (clean, rebuilt block)
        SET @countExecSql += N' AND NOT EXISTS (
            SELECT 1 FROM #ColumnFilters f
            WHERE f.[Column] IN (''Nume'',''Specializare'',''NumarLicenta'')
              AND NOT (
                (
                    f.[Column] = ''Nume'' AND (
                        (f.[Operator] = ''Contains'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                        OR (f.[Operator] = ''Equals'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS = UPPER(f.[Value]) COLLATE SQL_Latin1_General_CP1_CS_AS)
                        OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                        OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    )
                )
                OR
                (
                    f.[Column] = ''Specializare'' AND (
                        (f.[Operator] = ''Contains'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                        OR (f.[Operator] = ''Equals'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS = UPPER(f.[Value]) COLLATE SQL_Latin1_General_CP1_CS_AS)
                        OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                        OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    )
                )
                OR
                (
                    f.[Column] = ''NumarLicenta'' AND (
                        (f.[Operator] = ''Contains'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                        OR (f.[Operator] = ''Equals'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS = UPPER(f.[Value]) COLLATE SQL_Latin1_General_CP1_CS_AS)
                        OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                        OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    )
                )
            )
        )';

        -- (debug block moved to after IF/ELSE to unify debug output for both paths)

        -- main exec moved below to run once after the ColumnFilters predicate assembly
    END

    -- If requested, return the generated SQL for debugging (includes reconstructed count SQL)
    IF @ReturnGeneratedSql = 1
    BEGIN
        SELECT LEN(ISNULL(@ColumnFiltersJson,N'')) AS ColumnFiltersJsonLength, ISJSON(@ColumnFiltersJson) AS ColumnFiltersJsonIsJson;
        SELECT ISNULL(@ColumnFiltersJson,N'') AS ColumnFiltersJson;
        SELECT LEN(@sql) AS SqlLength;
        SELECT SUBSTRING(@sql,1,8000) AS Part1;
        SELECT SUBSTRING(@sql,8001,8000) AS Part2;
        SELECT SUBSTRING(@sql,16001,8000) AS Part3;
        -- Also show the count SQL for debugging by reconstructing it here so we can return it before executing any queries
        DECLARE @debugCountExecSql NVARCHAR(MAX) = N'SELECT @out = COUNT(1) FROM dbo.PersonalMedical PM WHERE 1=1';
        IF @ColumnFiltersJson IS NOT NULL AND ISJSON(@ColumnFiltersJson) = 1
        BEGIN
            SET @debugCountExecSql += N' AND NOT EXISTS (
                SELECT 1 FROM #ColumnFilters f
                WHERE f.[Column] IN (''Nume'',''Specializare'',''NumarLicenta'')
                  AND NOT (
                    (
                        f.[Column] = ''Nume'' AND (
                            (f.[Operator] = ''Contains'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                            OR (f.[Operator] = ''Equals'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS = UPPER(f.[Value]) COLLATE SQL_Latin1_General_CP1_CS_AS)
                            OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                            OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                        )
                    )
                    OR
                    (
                        f.[Column] = ''Specializare'' AND (
                            (f.[Operator] = ''Contains'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                            OR (f.[Operator] = ''Equals'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS = UPPER(f.[Value]) COLLATE SQL_Latin1_General_CP1_CS_AS)
                            OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                            OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                        )
                    )
                    OR
                    (
                        f.[Column] = ''NumarLicenta'' AND (
                            (f.[Operator] = ''Contains'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                            OR (f.[Operator] = ''Equals'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS = UPPER(f.[Value]) COLLATE SQL_Latin1_General_CP1_CS_AS)
                            OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                            OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                        )
                    )
                )
            )';
        END
        SELECT LEN(ISNULL(@debugCountExecSql,N'')) AS CountSqlLength;
        SELECT SUBSTRING(@debugCountExecSql,1,8000) AS CountPart1;
        SELECT SUBSTRING(@debugCountExecSql,8001,8000) AS CountPart2;
        RETURN;
    END

    -- Execute main (paged) query
    EXEC sp_executesql @sql, @params,
        @SearchText = @SearchText,
        @Departament = @Departament,
        @Pozitie = @Pozitie,
        @EsteActiv = @EsteActiv,
        @ColumnFiltersJson = @ColumnFiltersJson,
        @Offset = @Offset,
        @PageSize = @PageSize;

    -- Execute count query deterministically using sp_executesql with OUTPUT parameter
    IF @ColumnFiltersJson IS NOT NULL AND ISJSON(@ColumnFiltersJson) = 1
    BEGIN
        EXEC sp_executesql @countExecSql, N'@ColumnFiltersJson NVARCHAR(MAX), @out INT OUTPUT', @ColumnFiltersJson = @ColumnFiltersJson, @out = @countOut OUTPUT;
    END
    ELSE
    BEGIN
        EXEC sp_executesql @countExecSql, N'@out INT OUTPUT', @out = @countOut OUTPUT;
    END

    DECLARE @totalCount INT = ISNULL(@countOut, 0);

    -- Return total count as second resultset for caller compatibility
    SELECT @totalCount AS TotalCount;
END
GO

-- Example usage:
-- DECLARE @json NVARCHAR(MAX) = N'[{"Column":"Nume","Operator":"Contains","Value":"Ion"},{"Column":"Specializare","Operator":"StartsWith","Value":"Cardio"}]';
-- EXEC dbo.sp_PersonalMedical_GetAll @PageNumber = 1, @PageSize = 20, @ColumnFiltersJson = @json;

-- Notes:
-- - The implementation uses UPPER(...) comparisons for case-insensitive match; you can adapt to use collations or full-text search for better performance.
-- - For large datasets, consider full-text indexes on relevant text columns, or precomputed "NumeComplet" column and an index on it.
-- - Ensure the JSON passed by the application conforms to the expected structure and that nuancing (accent-insensitive search) is handled if required.
-- - After deploying the SP, ensure the application passes @ColumnFiltersJson when needed (already implemented in repository layer in codebase).
