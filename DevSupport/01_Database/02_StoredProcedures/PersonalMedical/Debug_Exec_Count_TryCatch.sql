DECLARE @ColumnFiltersJson NVARCHAR(MAX) = N'[{"Column":"Nume","Operator":"Contains","Value":"ian"}]';
IF OBJECT_ID('tempdb..#ColumnFilters') IS NOT NULL DROP TABLE #ColumnFilters;
CREATE TABLE #ColumnFilters ([Column] NVARCHAR(100), [Operator] NVARCHAR(20), [Value] NVARCHAR(4000) COLLATE SQL_Latin1_General_CP1_CS_AS);
INSERT INTO #ColumnFilters ([Column],[Operator],[Value])
SELECT [Column],[Operator],[Value]
FROM OPENJSON(@ColumnFiltersJson) WITH ([Column] NVARCHAR(100) '$.Column', [Operator] NVARCHAR(20) '$.Operator', [Value] NVARCHAR(4000) '$.Value');

DECLARE @countExecSql NVARCHAR(MAX) = N'SELECT @out = COUNT(1) FROM dbo.PersonalMedical PM WHERE 1=1';

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

-- show the SQL
PRINT '---- COUNT SQL START ----';
PRINT @countExecSql;
PRINT '---- COUNT SQL END ----';

-- Try executing
BEGIN TRY
    DECLARE @out INT = 0;
    EXEC sp_executesql @countExecSql, N'@out INT OUTPUT', @out = @out OUTPUT;
    PRINT 'EXEC OK, out=' + CAST(@out AS NVARCHAR(20));
END TRY
BEGIN CATCH
    PRINT 'ERROR: ' + ERROR_MESSAGE();
END CATCH
GO