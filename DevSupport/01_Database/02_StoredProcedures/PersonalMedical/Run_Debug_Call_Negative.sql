EXEC dbo.sp_PersonalMedical_GetAll @PageNumber=1, @PageSize=10, @ColumnFiltersJson = N'[{"Column":"Nume","Operator":"Equals","Value":"__NO_MATCH__"}]', @ReturnGeneratedSql = 1;
GO