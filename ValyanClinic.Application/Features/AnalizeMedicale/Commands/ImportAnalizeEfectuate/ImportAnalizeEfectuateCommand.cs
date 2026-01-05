using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.Analize.Models;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.ImportAnalizeEfectuate;

/// <summary>
/// Command pentru importarea analizelor efectuate din PDF
/// Salvează în tabela ConsultatieAnalizeMedicale cu rezultate complete
/// </summary>
public record ImportAnalizeEfectuateCommand : IRequest<Result<int>>
{
    public Guid ConsultatieID { get; init; }
    public List<AnalizaImportDto> Analize { get; init; } = new();
    public string? Laborator { get; init; }
    public string? NumarBuletin { get; init; }
    public string? DataRecoltare { get; init; }
    public Guid CreatDe { get; init; }
}
