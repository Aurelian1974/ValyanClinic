using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

namespace ValyanClinic.Application.Services.ScrisoareMedicala;

/// <summary>
/// Serviciu pentru generarea Scrisorii Medicale Anexa 43
/// Conform Ordin MS nr. 1411/2016
/// </summary>
public interface IScrisoareMedicalaService
{
    /// <summary>
    /// Generează datele pentru Scrisoarea Medicală pe baza ID-ului consultației
    /// </summary>
    /// <param name="consultatieId">ID-ul consultației</param>
    /// <param name="cancellationToken">Token de anulare</param>
    /// <returns>DTO cu datele pentru scrisoare</returns>
    Task<Result<ScrisoareMedicalaDto>> GenerateFromConsultatieAsync(
        Guid consultatieId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generează datele pentru Scrisoarea Medicală din DTO-ul consultației
    /// (pentru preview înainte de salvare)
    /// </summary>
    /// <param name="consultatie">DTO-ul consultației curente</param>
    /// <param name="cancellationToken">Token de anulare</param>
    /// <returns>DTO cu datele pentru scrisoare</returns>
    Task<Result<ScrisoareMedicalaDto>> GenerateFromDraftAsync(
        ConsulatieDetailDto consultatie,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generează date mock pentru preview/demo
    /// </summary>
    /// <returns>DTO cu date mock</returns>
    ScrisoareMedicalaDto GenerateMockData();
}
