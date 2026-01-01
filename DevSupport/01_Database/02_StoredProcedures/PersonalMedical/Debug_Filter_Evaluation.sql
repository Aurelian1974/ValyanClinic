DECLARE @cf NVARCHAR(MAX) = N'[{"Column":"Nume","Operator":"Contains","Value":"Iancu"}]';

SELECT pm.PersonalID, pm.Nume + ' ' + pm.Prenume AS FullName, f.[Column], f.[Operator], f.[Value],
    CASE WHEN (f.[Column] = 'Nume' AND ((f.[Operator] = 'Contains' AND UPPER(pm.Nume + ' ' + pm.Prenume) LIKE '%' + UPPER(f.[Value]) + '%')
        OR (f.[Operator] = 'Equals' AND UPPER(pm.Nume + ' ' + pm.Prenume) = UPPER(f.[Value]))
        OR (f.[Operator] = 'StartsWith' AND UPPER(pm.Nume + ' ' + pm.Prenume) LIKE UPPER(f.[Value]) + '%')
        OR (f.[Operator] = 'EndsWith' AND UPPER(pm.Nume + ' ' + pm.Prenume) LIKE '%' + UPPER(f.[Value]))
    )) THEN 1 ELSE 0 END AS NameMatch
FROM dbo.PersonalMedical pm
CROSS APPLY OPENJSON(@cf) WITH (
    [Column] NVARCHAR(100)  '$.Column',
    [Operator] NVARCHAR(20) '$.Operator',
    [Value] NVARCHAR(4000)  '$.Value'
) f
ORDER BY FullName;