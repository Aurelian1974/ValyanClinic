using FluentValidation;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;

public class GetPersonalMedicalListQueryValidator : AbstractValidator<GetPersonalMedicalListQuery>
{
    private static readonly string[] AllowedSortColumns = new[]
    {
        "Nume",
        "Prenume",
        "NumeComplet",
        "Specializare",
        "Departament",
        "Pozitie",
        "NumarLicenta",
        "EsteActiv",
        "DataCreare"
    };

    public GetPersonalMedicalListQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber trebuie sa fie > 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(10, 1000).WithMessage("PageSize trebuie sa fie intre 10 si 1000");

        RuleFor(x => x.GlobalSearchText)
            .MaximumLength(200).WithMessage("Cautare: maximum 200 caractere")
            .Matches(@"^[a-zA-Z0-9\s\-._@]*$").WithMessage("Cautare contine caractere nepermise");

        RuleFor(x => x.SortColumn)
            .Must(col => string.IsNullOrEmpty(col) || AllowedSortColumns.Contains(col))
            .WithMessage($"SortColumn invalid. Valori permise: {string.Join(",", AllowedSortColumns)}");

        RuleFor(x => x.SortDirection)
            .Must(d => string.IsNullOrEmpty(d) || d.Equals("ASC", StringComparison.OrdinalIgnoreCase) || d.Equals("DESC", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection invalid - foloseste 'ASC' sau 'DESC'");

        // No validation needed for nullable boolean FilterEsteActiv - server-side will interpret correctly

        // Column filters validation (if provided)
        RuleForEach(x => x.ColumnFilters).ChildRules(cf =>
        {
            cf.RuleFor(f => f.Column)
              .NotEmpty().WithMessage("Column filter requires a Column name")
              .Must(col => AllowedSortColumns.Contains(col) || col.Equals("NumeComplet", StringComparison.OrdinalIgnoreCase))
              .WithMessage("Invalid column name in ColumnFilters");

            cf.RuleFor(f => f.Operator)
              .NotEmpty()
              .Must(op => new[] { "Contains", "Equals", "StartsWith", "EndsWith" }.Contains(op))
              .WithMessage("Operator invalid in ColumnFilters");

            cf.RuleFor(f => f.Value)
              .NotEmpty().WithMessage("Filter value required").MaximumLength(200);
        });
    }
} 
