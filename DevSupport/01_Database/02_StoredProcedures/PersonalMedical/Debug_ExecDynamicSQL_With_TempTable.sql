DECLARE @ColumnFiltersJson NVARCHAR(MAX) = N'[{"Column":"Nume","Operator":"Contains","Value":"Iancu"}]';
IF OBJECT_ID('tempdb..#ColumnFilters') IS NOT NULL DROP TABLE #ColumnFilters;
CREATE TABLE #ColumnFilters (
    [Column] NVARCHAR(100),
    [Operator] NVARCHAR(20),
    [Value] NVARCHAR(4000)
);
INSERT INTO #ColumnFilters ([Column],[Operator],[Value])
SELECT [Column],[Operator],[Value]
FROM OPENJSON(@ColumnFiltersJson) WITH (
    [Column] NVARCHAR(100)  '$.Column',
    [Operator] NVARCHAR(20) '$.Operator',
    [Value] NVARCHAR(4000)  '$.Value'
);

DECLARE @sql NVARCHAR(MAX) = N'
    SELECT
        PersonalID, Nume, Prenume, Specializare, NumarLicenta
    FROM dbo.PersonalMedical PM
    WHERE 1=1
    AND NOT EXISTS (
        SELECT 1 FROM #ColumnFilters f
        WHERE f.[Column] IN (''Nume'',''Specializare'',''NumarLicenta'')
          AND NOT (
            (
                f.[Column] = ''Nume'' AND (
                    (f.[Operator] = ''Contains'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''Equals'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS = UPPER(f.[Value]) COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.Nume + '' '' + PM.Prenume) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + '') COLLATE SQL_Latin1_General_CP1_CS_AS)
                )
            )
            OR
            (
                f.[Column] = ''Specializare'' AND (
                    (f.[Operator] = ''Contains'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''Equals'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS = UPPER(f.[Value]) COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.Specializare) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + '') COLLATE SQL_Latin1_General_CP1_CS_AS)
                )
            )
            OR
            (
                f.[Column] = ''NumarLicenta'' AND (
                    (f.[Operator] = ''Contains'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''Equals'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS = UPPER(f.[Value]) COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (UPPER(f.[Value]) + ''%'') COLLATE SQL_Latin1_General_CP1_CS_AS)
                    OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.NumarLicenta) COLLATE SQL_Latin1_General_CP1_CS_AS LIKE (''%'' + UPPER(f.[Value]) + '') COLLATE SQL_Latin1_General_CP1_CS_AS)
                )
            )
          )
    )
    ORDER BY Nume ASC
    OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY';

DECLARE @params NVARCHAR(MAX) = N'@ColumnFiltersJson NVARCHAR(MAX)';
EXEC sp_executesql @sql;