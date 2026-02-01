using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities.Settings;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.Settings.Commands.UpdateProgramLucru;

public class UpdateProgramLucruCommandHandler : IRequestHandler<UpdateProgramLucruCommand, Result<bool>>
{
    private readonly IProgramLucruRepository _repository;
    private readonly ILogger<UpdateProgramLucruCommandHandler> _logger;

    public UpdateProgramLucruCommandHandler(
        IProgramLucruRepository repository,
        ILogger<UpdateProgramLucruCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateProgramLucruCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating program lucru for day {ZiSaptamana}", request.ZiSaptamana);

            var programLucru = new ProgramLucru
            {
                Id = request.Id,
                ZiSaptamana = (DayOfWeek)request.ZiSaptamana,
                EsteDeschis = request.EsteDeschis,
                OraInceput = !string.IsNullOrEmpty(request.OraInceput) 
                    ? TimeSpan.Parse(request.OraInceput) 
                    : null,
                OraSfarsit = !string.IsNullOrEmpty(request.OraSfarsit) 
                    ? TimeSpan.Parse(request.OraSfarsit) 
                    : null,
                PauzaInceput = !string.IsNullOrEmpty(request.PauzaInceput) 
                    ? TimeSpan.Parse(request.PauzaInceput) 
                    : null,
                PauzaSfarsit = !string.IsNullOrEmpty(request.PauzaSfarsit) 
                    ? TimeSpan.Parse(request.PauzaSfarsit) 
                    : null,
                Observatii = request.Observatii
            };

            var result = await _repository.UpdateAsync(programLucru, request.ModificatDe.ToString(), cancellationToken);

            if (result)
            {
                _logger.LogInformation("Successfully updated program lucru for day {ZiSaptamana}", request.ZiSaptamana);
                return Result<bool>.Success(true);
            }

            _logger.LogWarning("Failed to update program lucru for day {ZiSaptamana}", request.ZiSaptamana);
            return Result<bool>.Failure("Nu s-a putut actualiza programul de lucru.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating program lucru for day {ZiSaptamana}", request.ZiSaptamana);
            return Result<bool>.Failure($"Eroare: {ex.Message}");
        }
    }
}
