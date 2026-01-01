DECLARE @ColumnFiltersJson NVARCHAR(MAX) = N'[{"Column":"Nume","Operator":"Contains","Value":"Iancu"}]';
DECLARE @sql NVARCHAR(MAX) = N'
    SELECT
        PersonalID, Nume, Prenume, Specializare, NumarLicenta
    FROM dbo.PersonalMedical PM
    WHERE 1=1
    AND NOT EXISTS (
        SELECT 1 FROM OPENJSON(@ColumnFiltersJson) WITH (
            [Column] NVARCHAR(100)  "$.Column",
            [Operator] NVARCHAR(20) "$.Operator",
            [Value] NVARCHAR(4000)  "$.Value"
        ) f
        WHERE NOT (
            (
                f.[Column] = ''Nume'' AND (
                    (f.[Operator] = ''Contains'' AND UPPER(PM.Nume + '' '' + PM.Prenume) LIKE ''%'' + UPPER(f.[Value]) + ''%'')
                    OR (f.[Operator] = ''Equals'' AND UPPER(PM.Nume + '' '' + PM.Prenume) = UPPER(f.[Value]))
                    OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.Nume + '' '' + PM.Prenume) LIKE UPPER(f.[Value]) + ''%'')
                    OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.Nume + '' '' + PM.Prenume) LIKE ''%'' + UPPER(f.[Value]))
                )
            )
            OR
            (
                f.[Column] = ''Specializare'' AND (
                    (f.[Operator] = ''Contains'' AND UPPER(PM.Specializare) LIKE ''%'' + UPPER(f.[Value]) + ''%'')
                    OR (f.[Operator] = ''Equals'' AND UPPER(PM.Specializare) = UPPER(f.[Value]))
                    OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.Specializare) LIKE UPPER(f.[Value]) + ''%'')
                    OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.Specializare) LIKE ''%'' + UPPER(f.[Value]))
                )
            )
            OR
            (
                f.[Column] = ''NumarLicenta'' AND (
                    (f.[Operator] = ''Contains'' AND UPPER(PM.NumarLicenta) LIKE ''%'' + UPPER(f.[Value]) + ''%'')
                    OR (f.[Operator] = ''Equals'' AND UPPER(PM.NumarLicenta) = UPPER(f.[Value]))
                    OR (f.[Operator] = ''StartsWith'' AND UPPER(PM.NumarLicenta) LIKE UPPER(f.[Value]) + ''%'')
                    OR (f.[Operator] = ''EndsWith'' AND UPPER(PM.NumarLicenta) LIKE ''%'' + UPPER(f.[Value]))
                )
            )
        )
    )
    ORDER BY Nume ASC
    OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY';

DECLARE @params NVARCHAR(MAX) = N'@ColumnFiltersJson NVARCHAR(MAX)';
EXEC sp_executesql @sql, @params, @ColumnFiltersJson = @ColumnFiltersJson;