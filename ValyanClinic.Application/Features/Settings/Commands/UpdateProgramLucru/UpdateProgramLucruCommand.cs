using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.Settings.Commands.UpdateProgramLucru;

/// <summary>
/// Command pentru actualizarea programului de lucru pentru o zi
/// </summary>
public record UpdateProgramLucruCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public int ZiSaptamana { get; init; }
    public bool EsteDeschis { get; init; }
    public string? OraInceput { get; init; }
    public string? OraSfarsit { get; init; }
    public string? PauzaInceput { get; init; }
    public string? PauzaSfarsit { get; init; }
    public string? Observatii { get; init; }
    public Guid ModificatDe { get; init; }
}
