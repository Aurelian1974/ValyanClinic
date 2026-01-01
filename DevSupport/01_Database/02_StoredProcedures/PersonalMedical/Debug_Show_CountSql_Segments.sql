DECLARE @ColumnFiltersJson NVARCHAR(MAX) = N'[{"Column":"Nume","Operator":"Contains","Value":"ian"}]';
DECLARE @countExecSql NVARCHAR(MAX) = N'SELECT @out = COUNT(1) FROM dbo.PersonalMedical PM WHERE 1=1';

IF @ColumnFiltersJson IS NOT NULL AND ISJSON(@ColumnFiltersJson) = 1
BEGIN
    SET @countExecSql += N' AND NOT EXISTS (
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
        )';
END

-- split into 500-char segments
DECLARE @pos INT = 1, @len INT = LEN(@countExecSql);
CREATE TABLE #Seg (segid INT IDENTITY(1,1), txt NVARCHAR(500));
WHILE @pos <= @len
BEGIN
    INSERT INTO #Seg (txt) VALUES (SUBSTRING(@countExecSql,@pos,500));
    SET @pos = @pos + 500;
END
SELECT segid, txt FROM #Seg ORDER BY segid;
GO