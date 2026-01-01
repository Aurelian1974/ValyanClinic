EXEC dbo.sp_PersonalMedical_GetAll @PageNumber=1, @PageSize=1, @ColumnFiltersJson = N'[{"Column":"Nume","Operator":"StartsWith","Value":"Ian"}]';
GO
