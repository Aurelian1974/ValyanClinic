using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.Settings.Queries.GetProgramLucru;

public class GetProgramLucruQueryHandler : IRequestHandler<GetProgramLucruQuery, Result<List<ProgramLucruDto>>>
{
    private readonly IProgramLucruRepository _repository;
    private readonly ILogger<GetProgramLucruQueryHandler> _logger;

    public GetProgramLucruQueryHandler(
        IProgramLucruRepository repository,
        ILogger<GetProgramLucruQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<ProgramLucruDto>>> Handle(
        GetProgramLucruQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching program lucru clinica");

            var programLucru = await _repository.GetAllAsync(cancellationToken);

            var dtos = programLucru.Select(p => new ProgramLucruDto
            {
                Id = p.Id,
                ZiSaptamana = (int)p.ZiSaptamana,
                NumeZi = p.NumeZi,
                EsteDeschis = p.EsteDeschis,
                OraInceput = p.OraInceput?.ToString(@"hh\:mm"),
                OraSfarsit = p.OraSfarsit?.ToString(@"hh\:mm"),
                PauzaInceput = p.PauzaInceput?.ToString(@"hh\:mm"),
                PauzaSfarsit = p.PauzaSfarsit?.ToString(@"hh\:mm"),
                Observatii = p.Observatii,
                DataCrearii = p.DataCrearii,
                DataModificarii = p.DataModificarii,
                ModificatDe = p.ModificatDe
            }).OrderBy(p => p.ZiSaptamana == 0 ? 7 : p.ZiSaptamana) // Duminica la final
            .ToList();

            _logger.LogInformation("Found {Count} days in program lucru", dtos.Count);

            return Result<List<ProgramLucruDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching program lucru");
            return Result<List<ProgramLucruDto>>.Failure($"Error: {ex.Message}");
        }
    }
}
