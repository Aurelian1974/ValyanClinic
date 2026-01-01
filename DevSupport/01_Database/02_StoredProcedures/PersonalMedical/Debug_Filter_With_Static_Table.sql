-- Use static inline filter table to simulate OPENJSON result
SELECT pm.PersonalID, pm.Nume, pm.Prenume
FROM dbo.PersonalMedical pm
WHERE NOT EXISTS (
    SELECT 1 FROM (VALUES ('Nume','Contains','Iancu')) AS f([Column],[Operator],[Value])
    WHERE NOT (
        (
            f.[Column] = 'Nume' AND (
                (f.[Operator] = 'Contains' AND UPPER(pm.Nume + ' ' + pm.Prenume) LIKE '%' + UPPER(f.[Value]) + '%')
                OR (f.[Operator] = 'Equals' AND UPPER(pm.Nume + ' ' + pm.Prenume) = UPPER(f.[Value]))
                OR (f.[Operator] = 'StartsWith' AND UPPER(pm.Nume + ' ' + pm.Prenume) LIKE UPPER(f.[Value]) + '%')
                OR (f.[Operator] = 'EndsWith' AND UPPER(pm.Nume + ' ' + pm.Prenume) LIKE '%' + UPPER(f.[Value]))
            )
        )
        OR
        (
            f.[Column] = 'Specializare' AND (
                (f.[Operator] = 'Contains' AND UPPER(pm.Specializare) LIKE '%' + UPPER(f.[Value]) + '%')
                OR (f.[Operator] = 'Equals' AND UPPER(pm.Specializare) = UPPER(f.[Value]))
                OR (f.[Operator] = 'StartsWith' AND UPPER(pm.Specializare) LIKE UPPER(f.[Value]) + '%')
                OR (f.[Operator] = 'EndsWith' AND UPPER(pm.Specializare) LIKE '%' + UPPER(f.[Value]))
            )
        )
        OR
        (
            f.[Column] = 'NumarLicenta' AND (
                (f.[Operator] = 'Contains' AND UPPER(pm.NumarLicenta) LIKE '%' + UPPER(f.[Value]) + '%')
                OR (f.[Operator] = 'Equals' AND UPPER(pm.NumarLicenta) = UPPER(f.[Value]))
                OR (f.[Operator] = 'StartsWith' AND UPPER(pm.NumarLicenta) LIKE UPPER(f.[Value]) + '%')
                OR (f.[Operator] = 'EndsWith' AND UPPER(pm.NumarLicenta) LIKE '%' + UPPER(f.[Value]))
            )
        )
    )
)
ORDER BY pm.Nume, pm.Prenume;