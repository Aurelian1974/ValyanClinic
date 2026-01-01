-- Inspect generated dynamic SQL for sp_PersonalMedical_GetAll with a sample ColumnFiltersJson
DECLARE @ColumnFiltersJson NVARCHAR(MAX) = N'[{"Column":"Nume","Operator":"Contains","Value":"Iancu"}]';
DECLARE @PageNumber INT = 1;
DECLARE @PageSize INT = 10;
DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

DECLARE @sql NVARCHAR(MAX) = N'
    SELECT
        PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare
    FROM dbo.PersonalMedical PM
    WHERE 1=1'
;

IF @ColumnFiltersJson IS NOT NULL AND ISJSON(@ColumnFiltersJson) = 1
BEGIN
    SET @sql += N' AND NOT EXISTS (
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
    )';
END

SET @sql += N' ORDER BY ' + QUOTENAME('Nume') + ' ASC' + ' OFFSET ' + CAST(@Offset AS NVARCHAR(20)) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(20)) + ' ROWS ONLY';

SELECT @sql AS GeneratedSQL;
GO