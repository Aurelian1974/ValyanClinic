DECLARE @json NVARCHAR(MAX) = N'[{"Column":"Nume","Operator":"Equals","Value":"__NO_MATCH__"}]';
SELECT [Column],[Operator],[Value]
FROM OPENJSON(@json) WITH (
    [Column] NVARCHAR(100) '$.Column',
    [Operator] NVARCHAR(20) '$.Operator',
    [Value] NVARCHAR(4000) '$.Value'
);
GO