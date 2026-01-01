EXEC dbo.sp_PersonalMedical_GetAll @PageNumber=1,@PageSize=10,@ColumnFiltersJson = N'[{"Column":"Nume","Operator":"Contains","Value":"Iancu"}]', @ReturnGeneratedSql = 1;
GO